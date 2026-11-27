using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // Medkits are the only way to heal (user brief 2026-11-27) and stay rare:
    // mostly puzzle rewards, never lying around in bulk. One tap to collect;
    // use with H (desktop) or the MEDKIT touch button.
    public class MedkitPickup : MonoBehaviour, IInteractable
    {
        [Header("Wiring")]
        public AudioClip pickupClip;

        public string Prompt => "Take the medkit";
        public bool CanInteract => true;

        void Start()
        {
            // ItemPickup's rule: a loaded game never offers the same kit twice.
            if (Inventory.Has("medkit")) gameObject.SetActive(false);
        }

        public void Interact(GameObject interactor)
        {
            Inventory.Add("medkit");

            if (pickupClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(pickupClip);
            else LightShadow.Audio.Sfx.Play("SFX_Pickup", ProceduralSfx.Pickup);

            GameManager gm = GameManager.Instance;
            if (gm != null) gm.ShowMessage("Medkit packed. Press H when she needs it.", 4f);

            gameObject.SetActive(false);
        }
    }
}
