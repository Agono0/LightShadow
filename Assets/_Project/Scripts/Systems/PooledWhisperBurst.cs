using UnityEngine;

namespace LightShadow.Systems
{
    // A pooled whisper burst: a spatial one-shot whisper that plays and then
    // returns to the ObjectPool. The clip is synthesised once at first use
    // (the project ships no audio files - ambient audio is all procedural).
    public class PooledWhisperBurst : MonoBehaviour
    {
        AudioSource source;
        static AudioClip whisperClip;

        const int SampleRate = 44100;
        const float ClipSeconds = 1.4f;
        const float Volume = 0.28f;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.spatialBlend = 1f;
                source.minDistance = 1.5f;
                source.maxDistance = 18f;
                source.rolloffMode = AudioRolloffMode.Linear;
            }
            // Mixer routing (D15 follow-up §4.5): whispers are the Shadow's voice
            // and must ride the Shadow bus (memory-flash duck, stalk gain).
            source.outputAudioMixerGroup =
                LightShadow.Audio.VolumeCategories.GroupFor(LightShadow.Audio.VolumeCategories.Category.Shadow);
            if (whisperClip == null) whisperClip = BuildWhisper("SFX_Whisper_Burst");
        }

        public void Play()
        {
            if (source == null) Awake();
            if (source == null || whisperClip == null)
            {
                ObjectPool.Release(this);
                return;
            }
            source.clip = whisperClip;
            source.volume = Volume;
            source.pitch = Random.Range(0.9f, 1.1f);
            source.Play();
            enabled = true;
        }

        void Update()
        {
            if (source != null && source.isPlaying) return;
            ObjectPool.Release(this);
            enabled = false;
        }

        // Band-passed noise with a slow amplitude wobble: reads as a whisper
        // without any recorded asset. Runs once, on first burst only.
        static AudioClip BuildWhisper(string name)
        {
            int samples = Mathf.RoundToInt(SampleRate * ClipSeconds);
            float[] data = new float[samples];
            var random = new System.Random(20261);
            float filtered = 0f;
            float filtered2 = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SampleRate;
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                filtered = Mathf.Lerp(filtered, noise, 0.35f);   // breathy band
                filtered2 = Mathf.Lerp(filtered2, filtered, 0.08f); // slow body

                float wobble = 0.55f + 0.45f * Mathf.Sin(2f * Mathf.PI * 7f * t);
                float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / ClipSeconds));
                float fade = Mathf.Min(1f, Mathf.Min(t / 0.05f, (ClipSeconds - t) / 0.05f));
                data[i] = filtered2 * wobble * envelope * fade * Volume;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
