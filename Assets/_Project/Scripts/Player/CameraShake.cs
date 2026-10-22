using UnityEngine;

namespace LightShadow.Player
{
    // Small decaying shake on the first-person camera. Used when the Shadow
    // catches Nour so the hit has weight before the fade to black.
    // The pivot keeps handling pitch; this only offsets the camera itself.
    public class CameraShake : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform shakeRoot;

        [Header("Defaults")]
        public float frequency = 30f;

        Vector3 basePosition;
        float remaining;
        float duration;
        float magnitude;

        void Awake()
        {
            if (shakeRoot == null) shakeRoot = transform;
            basePosition = shakeRoot.localPosition;
        }

        public void Shake(float seconds, float amount)
        {
            if (shakeRoot == null) return;
            duration = Mathf.Max(0.01f, seconds);
            remaining = duration;
            magnitude = Mathf.Max(0f, amount);
        }

        void LateUpdate()
        {
            if (shakeRoot == null) return;

            if (remaining <= 0f)
            {
                shakeRoot.localPosition = Vector3.Lerp(shakeRoot.localPosition, basePosition,
                    Time.deltaTime * 14f);
                return;
            }

            remaining -= Time.deltaTime;
            float decay = Mathf.Clamp01(remaining / duration);
            float x = (Mathf.PerlinNoise(Time.time * frequency, 0.37f) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(0.71f, Time.time * frequency * 1.13f) - 0.5f) * 2f;
            Vector3 offset = new Vector3(x, y, 0f) * magnitude * decay * decay;
            shakeRoot.localPosition = basePosition + offset;
        }
    }
}
