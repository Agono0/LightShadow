using LightShadow.Core;
using LightShadow.Player;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // Doc Act III: the main gate only opens with power from all three fuse boxes.
    // Interacting with a powered gate turns the crank, then walking into the
    // escape zone finishes the night.
    public class ExitGate : MonoBehaviour, IInteractable
    {
        [Header("Wiring")]
        public Transform gateLeaf;
        public BoxCollider escapeZone;
        public GameObject dawnChaseRoot;
        public AudioClip powerClip;

        public bool Powered { get; private set; }

        public string Prompt => Powered ? "The gate is open." : "Examine the gate";
        public bool CanInteract => true;

        public void Interact(GameObject interactor)
        {
            if (Powered) return;

            GameManager gm = GameManager.Instance;
            if (gm == null) return;

            if (!gm.CanPowerGate)
            {
                gm.ShowMessage("The gate crank is dead. Three fuse boxes must be restored first.", 4f);
                return;
            }

            Power();
        }

        void Power()
        {
            Powered = true;

            if (powerClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(powerClip);
            if (dawnChaseRoot != null) dawnChaseRoot.SetActive(true);
            if (escapeZone != null) escapeZone.enabled = true;
            if (gateLeaf != null) StartCoroutine(TurnCrank());

            if (GameManager.Instance != null) GameManager.Instance.OnGatePowered();
        }

        System.Collections.IEnumerator TurnCrank()
        {
            Quaternion start = gateLeaf.localRotation;
            Quaternion end = start * Quaternion.Euler(0f, 0f, 90f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 1.5f;
                gateLeaf.localRotation = Quaternion.Slerp(start, end, t);
                yield return null;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!Powered) return;
            if (other.GetComponentInParent<PlayerController>() == null) return;

            if (GameManager.Instance != null) GameManager.Instance.Escape();
        }
    }
}
