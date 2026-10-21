using UnityEngine;

namespace LightShadow.Core
{
    // A quiet, seamless school-at-night drone: two low sine layers and a slow
    // breathing amplitude. Generated once at runtime, so the build stays free of
    // audio assets while the dark rooms are not silent.
    [RequireComponent(typeof(AudioSource))]
    public class AmbientDrone : MonoBehaviour
    {
        public float targetVolume = 0.14f;
        public float fadeSeconds = 3f;

        AudioSource source;
        float elapsed;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0f;
            source.clip = BuildClip();
        }

        void Start()
        {
            source.Play();
        }

        void Update()
        {
            if (source == null || fadeSeconds <= 0f) return;
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, Mathf.Clamp01(elapsed / fadeSeconds));
        }

        static AudioClip BuildClip()
        {
            const int sampleRate = 44100;
            const float duration = 8f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;

                // Frequencies are exact multiples of the loop length (55 Hz x 8 s
                // = 440 cycles), so the seam is inaudible.
                float tone = Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.55f
                    + Mathf.Sin(2f * Mathf.PI * 82.5f * t) * 0.28f
                    + Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.12f;

                // One slow breath per loop.
                float breath = 0.72f + 0.28f * Mathf.Sin(2f * Mathf.PI * 0.125f * t - Mathf.PI * 0.5f);
                data[i] = tone * breath * 0.10f;
            }

            AudioClip clip = AudioClip.Create("AMB_AlNoor", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
