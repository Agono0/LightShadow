using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Audio
{
    // Layered procedural tension music, extending the AmbientDrone concept
    // (Core/AmbientDrone stays untouched - this owns its own AudioSources):
    //
    //   calm layer    - always on. A slow stack of detuned sines on a minor
    //                   ninth pad, the "school after hours" bed.
    //   tension layer - rises as the Bright Idea Meter falls (dark = danger)
    //                   and spikes for a while after the Shadow catches Nour.
    //   pulse layer   - heartbeat kick that fades in on recent catches, when
    //                   the gate powers (the last surge) and at dawn.
    //
    // The chase state is not on the GameEvents contract, so it is approximated
    // honestly: caught-recency (heavy), gate-powered (medium), dawn (medium).
    // Crossfades are volume-only moves per frame - the clips are pre-rendered
    // once at start, so the runtime path is allocation-free.
    //
    // Category is Music: VolumeCategories owns these sources' final volume.
    public class AdaptiveMusic : MonoBehaviour
    {
        public static AdaptiveMusic Instance { get; private set; }

        [Header("Mix (bases before category scaling)")]
        public float calmBase = 0.16f;
        public float tensionBase = 0.20f;
        public float pulseBase = 0.26f;

        [Header("Response curves")]
        [Tooltip("Meter at or below this starts raising tension (0-1).")]
        public float tensionMeterFloor = 0.55f;
        [Tooltip("Seconds of elevated tension after a catch.")]
        public float catchSpikeSeconds = 25f;
        [Tooltip("Seconds of pulse after a catch.")]
        public float catchPulseSeconds = 12f;

        [Header("Crossfade speed (volume units per second)")]
        public float fadeSpeed = 0.35f;

        AudioSource calmSource;
        AudioSource tensionSource;
        AudioSource pulseSource;

        float lastMeter = 1f;
        float lastCatchAt = -999f;
        float gatePoweredAt = -999f;
        float dawnStartedAt = -999f;

        float calmVolume;
        float tensionVolume;
        float pulseVolume;

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            new GameObject("AdaptiveMusic").AddComponent<AdaptiveMusic>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            calmSource = BuildSource("Music_CalmPad", RenderCalmPad());
            tensionSource = BuildSource("Music_Tension", RenderTensionLayer());
            pulseSource = BuildSource("Music_Pulse", RenderPulseLayer());

            // These three sources are NOT registered with VolumeCategories (it
            // would fight this Update over .volume). The Music/Master sliders are
            // applied instead by reading the category volumes directly each frame
            // (D15) - see MixVolume below.
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void OnEnable()
        {
            GameEvents.MeterChanged += OnMeterChanged;
            GameEvents.PlayerCaught += OnPlayerCaught;
            GameEvents.GatePowered += OnGatePowered;
            GameEvents.DawnStarted += OnDawnStarted;
        }

        void OnDisable()
        {
            GameEvents.MeterChanged -= OnMeterChanged;
            GameEvents.PlayerCaught -= OnPlayerCaught;
            GameEvents.GatePowered -= OnGatePowered;
            GameEvents.DawnStarted -= OnDawnStarted;
        }

        void OnMeterChanged(float normalized) => lastMeter = Mathf.Clamp01(normalized);
        void OnPlayerCaught() => lastCatchAt = Time.unscaledTime;
        void OnGatePowered() => gatePoweredAt = Time.unscaledTime;
        void OnDawnStarted() => dawnStartedAt = Time.unscaledTime;

        void Update()
        {
            float now = Time.unscaledTime;

            // Target volumes from the (honest) chase approximation.
            float meterPressure = Mathf.Clamp01((tensionMeterFloor - lastMeter) / tensionMeterFloor);
            float catchSpike = now - lastCatchAt < catchSpikeSeconds
                ? 1f - (now - lastCatchAt) / catchSpikeSeconds : 0f;
            float tensionTarget = Mathf.Clamp01(meterPressure * 0.7f + catchSpike * 0.9f);

            float surge = 0f;
            if (now - gatePoweredAt < 9999f && gatePoweredAt > 0f)
            {
                surge = Mathf.Clamp01(1f - (now - gatePoweredAt) / 120f) * 0.6f;
            }
            if (dawnStartedAt > 0f) surge = Mathf.Max(surge, 0.65f);

            float pulseTarget = 0f;
            float catchPulse = now - lastCatchAt < catchPulseSeconds
                ? 1f - (now - lastCatchAt) / catchPulseSeconds : 0f;
            pulseTarget = Mathf.Clamp01(catchPulse + surge);

            // Smooth volume-only crossfades; zero allocations.
            calmVolume = MoveToward(calmVolume, 1f - tensionTarget * 0.45f);
            tensionVolume = MoveToward(tensionVolume, tensionTarget);
            pulseVolume = MoveToward(pulseVolume, pulseTarget);

            // Final mix: base * envelope * Music slider * Master slider. Re-read
            // every frame so slider moves apply instantly without event plumbing.
            if (calmSource != null) calmSource.volume = MixVolume(calmBase, calmVolume);
            if (tensionSource != null) tensionSource.volume = MixVolume(tensionBase, tensionVolume);
            if (pulseSource != null) pulseSource.volume = MixVolume(pulseBase, pulseVolume);
        }

        float MoveToward(float current, float target)
        {
            return Mathf.MoveTowards(current, target, fadeSpeed * Time.unscaledDeltaTime);
        }

        static float MixVolume(float layerBase, float envelope)
        {
            float category = VolumeCategories.CategoryVolume01(VolumeCategories.Category.Music);
            return Mathf.Clamp01(layerBase * envelope * category * VolumeCategories.MasterVolume01);
        }

        AudioSource BuildSource(string name, AudioClip clip)
        {
            var owner = new GameObject(name);
            owner.transform.SetParent(transform, false);
            AudioSource source = owner.AddComponent<AudioSource>();
            source.playOnAwake = true;
            source.loop = true;
            source.clip = clip;
            source.volume = 0f;
            source.spatialBlend = 0f; // 2D bed
            source.Play();
            return source;
        }

        // ------------------------------------------------------------ synthesis

        const int SampleRate = 44100;

        AudioClip RenderCalmPad()
        {
            // 14 s loop, slow detuned sine stack (A2 minor add9) + faint air.
            const float seconds = 14f;
            int samples = (int)(seconds * SampleRate);
            float[] data = new float[samples];
            float[] freqs = { 110.0f, 130.81f, 164.81f, 220.0f, 246.94f, 329.63f };
            float[] amps = { 0.32f, 0.22f, 0.16f, 0.18f, 0.10f, 0.07f };

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float loopPhase = t / seconds;
                // Slow breathing so the loop point is inaudible.
                float breath = 0.75f + 0.25f * Mathf.Sin(loopPhase * Mathf.PI * 2f);
                float sample = 0f;
                for (int f = 0; f < freqs.Length; f++)
                {
                    sample += amps[f] * Mathf.Sin(2f * Mathf.PI * freqs[f] * t
                        + Mathf.Sin(2f * Mathf.PI * 0.07f * f * t) * 0.4f); // gentle detune drift
                }
                sample = sample / freqs.Length * 2.2f * breath;
                data[i] = Mathf.Clamp(sample, -1f, 1f) * 0.8f;
            }

            AudioClip clip = AudioClip.Create("MUS_CalmPad", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        AudioClip RenderTensionLayer()
        {
            // 8 s loop: a low 55 Hz drone against a minor-second neighbour
            // (58.3 Hz) whose amplitude pulses slowly - unease without melody.
            const float seconds = 8f;
            int samples = (int)(seconds * SampleRate);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float gate = 0.55f + 0.45f * Mathf.Sin(2f * Mathf.PI * t / seconds * 2f);
                float drone = Mathf.Sin(2f * Mathf.PI * 55f * t);
                float neighbour = Mathf.Sin(2f * Mathf.PI * 58.3f * t) * 0.55f;
                float chalk = Mathf.Sin(2f * Mathf.PI * 2200f * t) * 0.015f * gate; // faint static
                float sample = (drone + neighbour) * gate * 0.35f + chalk;
                data[i] = Mathf.Clamp(sample, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("MUS_Tension", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        AudioClip RenderPulseLayer()
        {
            // 4 s loop: two soft heartbeat kicks (60 Hz decaying sines), the
            // chase signal. Rendered with a hard loop-safe envelope.
            const float seconds = 4f;
            int samples = (int)(seconds * SampleRate);
            float[] data = new float[samples];

            float[] hitTimes = { 0.0f, 0.42f }; // lub ... dub, then rest
            for (int hit = 0; hit < hitTimes.Length; hit++)
            {
                int start = (int)(hitTimes[hit] * SampleRate);
                const float decay = 0.9f;
                const float freq = 60f;
                for (int i = start; i < samples; i++)
                {
                    float t = (float)(i - start) / SampleRate;
                    float envelope = Mathf.Exp(-decay * t * 8f);
                    if (envelope < 0.001f) break;
                    data[i] += Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.7f;
                }
            }

            AudioClip clip = AudioClip.Create("MUS_Pulse", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
