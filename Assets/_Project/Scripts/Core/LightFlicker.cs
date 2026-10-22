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

            float noise = Mathf.PerlinNoise(seed, Time.time * speed) - 0.5f;
            target.intensity = baseIntensity * (1f + noise * amplitude);
        }
    }
}
