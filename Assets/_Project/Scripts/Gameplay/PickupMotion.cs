using UnityEngine;

namespace LightShadow.Gameplay
{
    // Slow bob and spin for collectables. The collider on the prefab root stays
    // still (so the interaction ray keeps a stable target) while the "Visual"
    // child does the moving.
    public class PickupMotion : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform visual;

        [Header("Motion")]
        public float bobHeight = 0.06f;
        public float bobSpeed = 1.6f;
        public float spinSpeed = 45f;

        Vector3 basePosition;
        Quaternion baseRotation;
        float phase;

        void Awake()
        {
            if (visual == null) visual = transform.Find("Visual");
            if (visual == null) return;
            basePosition = visual.localPosition;
            baseRotation = visual.localRotation;
            phase = Random.value * Mathf.PI * 2f;
        }

        void Update()
        {
            if (visual == null) return;

            float t = Time.time * bobSpeed + phase;
            visual.localPosition = basePosition + Vector3.up * (Mathf.Sin(t) * bobHeight);
            visual.localRotation = baseRotation * Quaternion.Euler(0f, Time.time * spinSpeed, 0f);
        }
    }
}
