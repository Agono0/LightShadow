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
        [Tooltip("Finale wiring puzzle on the gate console. When set and unsolved, a powered-up gate routes through it instead of powering directly.")]
        public GateCircuitPuzzle circuitPuzzle;

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

            // Finale pass: with the fuses restored, the console's wiring board
            // stands between the player and the crank until it is solved.
            if (circuitPuzzle != null && !circuitPuzzle.Solved)
            {
                circuitPuzzle.Interact(interactor);
                return;
            }

            Power();
        }

        // Called by the gate circuit puzzle when every feed is routed. Same
        // power-up path as a direct crank turn.
        public void PowerFromPuzzle()
        {
            if (Powered) return;
            Power();
        }

        void Power()
        {
            Powered = true;

            if (powerClip == null) powerClip = ProceduralSfx.Power;
            LightShadow.Audio.Sfx.Play("SFX_GateCrank", powerClip);
            if (dawnChaseRoot != null) dawnChaseRoot.SetActive(true);
            if (escapeZone != null) escapeZone.enabled = true;
            if (gateLeaf != null) StartCoroutine(TurnCrank());

            if (GameManager.Instance != null) GameManager.Instance.OnGatePowered();
        }

        // Save restore (premium pass): the gate was already powered when the
        // run was saved. Restore the state instantly - no sound, no count, the
        // gate leaf lands open, and the dawn chase lights wake through
        // dawnChaseRoot (which raises DawnRiser and the DawnStarted event).
        public void PowerFromSave()
        {
            if (Powered) return;
            Powered = true;

            if (dawnChaseRoot != null) dawnChaseRoot.SetActive(true);
            if (escapeZone != null) escapeZone.enabled = true;
            if (gateLeaf != null)
            {
                Quaternion start = gateLeaf.localRotation;
                gateLeaf.localRotation = start * Quaternion.Euler(0f, 90f, 0f);
            }
        }

        System.Collections.IEnumerator TurnCrank()
        {
            Quaternion start = gateLeaf.localRotation;
            Quaternion end = start * Quaternion.Euler(0f, 90f, 0f);
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

        // F5 restore support: is the player capsule inside the escape zone
        // right now? Bounded, non-allocating overlap query:
        //   - abs(lossyScale) per axis, so a mirrored gate (negative scale)
        //     still measures the box correctly;
        //   - returns false when no player collider is among the hits;
        //   - every overlapping collider is inspected (no fixed buffer);
        //   - the query spans every layer and includes trigger colliders, so
        //     the escapeZone layer setup cannot exclude the player capsule.
        public bool PlayerInsideEscapeZone()
        {
            if (escapeZone == null) return false;

            Vector3 scale = escapeZone.transform.lossyScale;
            Vector3 half = new Vector3(
                Mathf.Abs(scale.x) * escapeZone.size.x * 0.5f,
                Mathf.Abs(scale.y) * escapeZone.size.y * 0.5f,
                Mathf.Abs(scale.z) * escapeZone.size.z * 0.5f);
            // Allocating query on purpose: this runs once per restore, and a
            // fixed buffer could miss the player behind set dressing.
            Collider[] hits = Physics.OverlapBox(
                escapeZone.transform.TransformPoint(escapeZone.center), half,
                escapeZone.transform.rotation, ~0, QueryTriggerInteraction.Collide);
            return ContainsPlayer(hits, hits.Length);
        }

        // Pure scan seam for tests: true when any of the first count entries
        // belongs to a PlayerController. Tolerates a null array, null entries
        // and a count past the buffer length.
        public static bool ContainsPlayer(Collider[] hits, int count)
        {
            if (hits == null) return false;
            if (count > hits.Length) count = hits.Length;
            for (int i = 0; i < count; i++)
            {
                if (hits[i] != null && hits[i].GetComponentInParent<PlayerController>() != null)
                    return true;
            }
            return false;
        }

    }
}
