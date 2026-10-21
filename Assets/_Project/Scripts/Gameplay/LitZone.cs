using System.Collections.Generic;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // A room/area the Shadow cannot enter (Doc 4.3: "cannot enter lit rooms").
    // LitZone does three jobs:
    //   1. Tracks whether the player currently stands in any lit zone
    //      (published as the static PlayerInLitZone).
    //   2. Serves as a safe checkpoint when it has a checkpoint flag.
    //   3. Clamps enemy movement targets so steering never sends the enemy inside.
    // The Library wing zone starts enabled; the Science and Main Hall wing zones
    // are enabled by their fuse boxes.
    public class LitZone : MonoBehaviour
    {
        [Header("Checkpoint")]
        public bool isCheckpoint;
        [TextArea(1, 2)]
        public string chalkLine = "The light holds. This feels safe.";

        static readonly List<LitZone> All = new List<LitZone>();
        static int playerInsideCount;

        public static bool PlayerInLitZone => playerInsideCount > 0;

        BoxCollider box;
        bool playerInside;

        public Bounds Bounds => box != null ? box.bounds : new Bounds(transform.position, Vector3.one);

        void Awake()
        {
            box = GetComponent<BoxCollider>();
        }

        void OnEnable()
        {
            if (!All.Contains(this)) All.Add(this);
        }

        void OnDisable()
        {
            All.Remove(this);
            // If this zone vanishes while the player stands inside, fix the count.
            if (playerInside)
            {
                playerInside = false;
                playerInsideCount = Mathf.Max(0, playerInsideCount - 1);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (playerInside) return;
            if (other.GetComponentInParent<Player.PlayerController>() == null) return;

            playerInside = true;
            playerInsideCount++;
            RegisterCheckpoint();
        }

        void OnTriggerExit(Collider other)
        {
            if (!playerInside) return;
            if (other.GetComponentInParent<Player.PlayerController>() == null) return;

            playerInside = false;
            playerInsideCount = Mathf.Max(0, playerInsideCount - 1);
        }

        void RegisterCheckpoint()
        {
            if (!isCheckpoint) return;
            if (Core.CheckpointManager.Instance != null)
            {
                Core.CheckpointManager.Instance.SetCheckpoint(transform, chalkLine);
            }
        }

        // ---------- Static helpers used by the enemy ----------

        // Push a point out of every lit zone it sits inside. The enemy calls this
        // on both patrol destinations and chase targets, which is what keeps the
        // Shadow out of lit rooms without complex navigation tricks.
        public static Vector3 ClampPoint(Vector3 point, float margin = 0.6f)
        {
            for (int i = 0; i < All.Count; i++)
            {
                Bounds bounds = All[i].Bounds;
                bounds.Expand(margin * 2f);
                if (!bounds.Contains(point)) continue;

                // Find the nearest face of the box and step out through it.
                float dxLeft = point.x - bounds.min.x;
                float dxRight = bounds.max.x - point.x;
                float dyBack = point.z - bounds.min.z;
                float dzFront = bounds.max.z - point.z;

                float min = dxLeft;
                Vector3 result = new Vector3(bounds.min.x - margin, point.y, point.z);
                if (dxRight < min) { min = dxRight; result = new Vector3(bounds.max.x + margin, point.y, point.z); }
                if (dyBack < min) { min = dyBack; result = new Vector3(point.x, point.y, bounds.min.z - margin); }
                if (dzFront < min) { min = dzFront; result = new Vector3(point.x, point.y, bounds.max.z + margin); }

                point = result;
            }
            return point;
        }
    }
}
