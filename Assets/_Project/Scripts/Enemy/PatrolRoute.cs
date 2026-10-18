using System.Collections.Generic;
using UnityEngine;

namespace LightShadow.Enemy
{
    // An ordered list of patrol waypoints for the Shadow.
    // SceneBuilder fills it with small marker cubes spread through the corridors.
    public class PatrolRoute : MonoBehaviour
    {
        public List<Transform> waypoints = new List<Transform>();

        public int Count => waypoints.Count;

        public Transform Get(int index)
        {
            if (Count == 0) return null;
            return waypoints[((index % Count) + Count) % Count];
        }

        public int ClosestIndex(Vector3 position)
        {
            int best = 0;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < Count; i++)
            {
                if (waypoints[i] == null) continue;
                float d = (waypoints[i].position - position).sqrMagnitude;
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = i;
                }
            }
            return best;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.4f, 0.4f, 1f, 0.7f);
            for (int i = 0; i < Count; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);
                if (i + 1 < Count && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
        }
    }
}
