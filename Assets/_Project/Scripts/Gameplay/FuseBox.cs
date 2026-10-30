using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // Doc 4.4: restoring a fuse box floods its wing with warm light, creates a
    // checkpoint and prints one line of the mother's voice. This script does
    // the lights and the zone; the GameManager counts fuses and shows the line.
    public class FuseBox : MonoBehaviour, IInteractable
    {
        [Header("Wiring")]
        public GameObject lightsRoot;
        public LitZone[] wingZones;
        public AudioClip restoreClip;

        [Header("Feedback")]
        public Renderer indicator;
        public Material restoredMaterial;

        [Header("Memory Flash")]
        [TextArea(1, 3)]
        public string memoryFlash = "She always said a library card is a key that never rusts.";

        public bool Restored { get; private set; }

        public string Prompt => Restored ? "The fuse hums quietly." : "Restore the fuse box";
        public bool CanInteract => !Restored;

        public void Interact(GameObject interactor)
        {
            if (Restored) return;
            Restored = true;

            if (lightsRoot != null) lightsRoot.SetActive(true);

            if (wingZones != null)
            {
                foreach (LitZone zone in wingZones)
                {
                    if (zone != null) zone.gameObject.SetActive(true);
                }
            }

            if (restoreClip == null) restoreClip = ProceduralSfx.Power;
            if (restoreClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(restoreClip);

            if (indicator != null && restoredMaterial != null) indicator.sharedMaterial = restoredMaterial;

            if (GameManager.Instance != null) GameManager.Instance.OnFuseRestored(memoryFlash);
        }

        // Save restore (premium pass): put this box in its restored visual state
        // WITHOUT counting it again, showing messages, or creating a checkpoint.
        public void RestoreFromSave()
        {
            if (Restored) return;

            if (lightsRoot != null) lightsRoot.SetActive(true);
            if (wingZones != null)
            {
                for (int i = 0; i < wingZones.Length; i++)
                {
                    if (wingZones[i] != null) wingZones[i].gameObject.SetActive(true);
                }
            }
            if (indicator != null && restoredMaterial != null) indicator.sharedMaterial = restoredMaterial;

            // Reflection of the private setter path: the count stays owned by
            // GameManager.ApplyLoadedState, so only the visuals happen here.
            Restored = true;
        }
    }
}
