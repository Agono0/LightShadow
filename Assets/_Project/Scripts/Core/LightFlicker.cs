using UnityEngine;

namespace LightShadow.Core
{
    // Subtle power flicker for the emergency lamps. Perlin noise instead of a
    // random jitter, so the light breathes like a tired fluorescent tube rather
    // than strobing. Amplitude is low on purpose: atmosphere, not a disco.
    public class LightFlicker : MonoBehaviour
    {
        [Header("Flicker")]
        public float amplitude = 0.14f;
        public float speed = 9f;

        [Header("Dropouts (faulty lamps)")]
        // User brief 2026-11-27: lamps that flicker or switch. Candles keep
        // this off; a faulty corridor lamp blinks out for a beat now and then.
        public bool blinks;
        public float blinkChancePerSecond = 0.06f;
        public float blinkMinSeconds = 0.05f;
        public float blinkMaxSeconds = 0.22f;

        float blinkTimer;

        Light target;
        float baseIntensity;
        float seed;

        void Awake()
        {
            target = GetComponent<Light>();
            if (target != null) baseIntensity = target.intensity;
            seed = Random.value * 50f;
        }

        void Update()
        {
            if (target == null) return;

            if (blinkTimer > 0f)
            {
                blinkTimer -= Time.deltaTime;
                target.intensity = 0f;
                return;
            }
            if (blinks && Random.value < blinkChancePerSecond * Time.deltaTime)
            {
                blinkTimer = Random.Range(blinkMinSeconds, Mathf.Max(blinkMinSeconds, blinkMaxSeconds));
                target.intensity = 0f;
                return;
            }

            float noise = Mathf.PerlinNoise(seed, Time.time * speed) - 0.5f;
            target.intensity = baseIntensity * (1f + noise * amplitude);
        }
    }
}
