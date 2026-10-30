using UnityEngine;

namespace LightShadow.Systems
{
    // The pooled flashlight flare: a tiny unshadowed point light that flares up
    // and fades in half a second, then parks itself back in the ObjectPool.
    // Created lazily by PooledFx on first use; no scene wiring needed.
    public class PooledFlashEffect : MonoBehaviour
    {
        Light lightSource;
        float remaining;
        float duration = 0.5f;
        float baseIntensity = 2.2f;

        const float Range = 3.5f;

        void Awake()
        {
            lightSource = GetComponent<Light>();
            if (lightSource == null)
            {
                lightSource = gameObject.AddComponent<Light>();
                lightSource.type = LightType.Point;
                lightSource.shadows = LightShadows.None; // a flare never casts
                lightSource.range = Range;
            }
        }

        // Fires the flare. seconds <= 0 keeps the tuning default.
        public void Play(Color color, float seconds)
        {
            if (lightSource == null) Awake();
            duration = seconds > 0f ? seconds : 0.5f;
            remaining = duration;
            baseIntensity = 2.2f;
            if (lightSource != null)
            {
                lightSource.color = color;
                lightSource.intensity = baseIntensity;
            }
            enabled = true;
        }

        void Update()
        {
            remaining -= Time.deltaTime;
            if (remaining <= 0f)
            {
                if (lightSource != null) lightSource.intensity = 0f;
                ObjectPool.Release(this);
                enabled = false;
                return;
            }
            if (lightSource != null)
            {
                lightSource.intensity = baseIntensity * (remaining / duration);
            }
        }
    }
}
