using UnityEngine;

namespace LightShadow.Enemy
{
    // Answers one question: "can the Shadow see Nour right now?"
    // It casts a line from the Shadow's head to the player's camera; walls block
    // the line, the player's own body does not (Doc 4.3 line-of-sight rule).
    public class EnemySight : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform head;
        public Transform target;
        public LayerMask obstacleMask = ~0;
        public float range = 15f;

        public bool HasLineOfSight()
        {
            if (head == null || target == null) return false;

            Vector3 origin = head.position;
            Vector3 destination = target.position;
            if ((destination - origin).magnitude > range) return false;

            if (Physics.Linecast(origin, destination, out RaycastHit hit, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                // Hitting the player's own capsule means nothing else blocked the view.
                if (hit.transform.IsChildOf(target.root)) return true;
                return false;
            }
            return true;
        }
    }
}
