using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Enemy
{
    // The Shadow's voice: a low, breathing growl that follows the player and
    // sharpens into a chase drone when it sees her. All clips are synthesised at
    // runtime, so the enemy is audible without any imported audio files.
    public class ShadowAudio : MonoBehaviour
    {
        [Header("Wiring")]
        public EnemyBehaviour behaviour;
        public Transform player;
        public float maxHearDistance = 30f;

        AudioSource growlSource;
        AudioSource stingerSource;
        AudioClip patrolClip;
        AudioClip chaseClip;
        AudioClip stingerClip;
        EnemyBehaviour.State lastState;
        float smoothedVolume;

        void Awake()
        {
            if (behaviour == null) behaviour = GetComponent<EnemyBehaviour>();

            growlSource = gameObject.AddComponent<AudioSource>();
            growlSource.playOnAwake = false;
            growlSource.loop = true;
            growlSource.spatialBlend = 1f;
            growlSource.minDistance = 2f;
            growlSource.maxDistance = maxHearDistance;
            growlSource.rolloffMode = AudioRolloffMode.Linear;
            growlSource.volume = 0f;
            growlSource.dopplerLevel = 0f;

            stingerSource = gameObject.AddComponent<AudioSource>();
            stingerSource.playOnAwake = false;
            stingerSource.loop = false;
            stingerSource.spatialBlend = 1f;
            stingerSource.minDistance = 2f;
            stingerSource.maxDistance = maxHearDistance + 5f;
            stingerSource.rolloffMode = AudioRolloffMode.Linear;

            patrolClip = BuildGrowl("SFX_Shadow_Patrol", 48f, 72f, 0.12f, 0.55f);
            chaseClip = BuildGrowl("SFX_Shadow_Chase", 84f, 126f, 0.22f, 0.75f);
            stingerClip = BuildStinger("SFX_Shadow_Stinger");

            growlSource.clip = patrolClip;
            growlSource.Play();
            lastState = EnemyBehaviour.State.Patrol;
        }

        void Start()
        {
            if (player == null && GameManager.Instance != null) player = GameManager.Instance.player;
        }

        void Update()
        {
            if (behaviour == null) return;

            EnemyBehaviour.State state = behaviour.CurrentState;
            if (state != lastState)
            {
                if (state == EnemyBehaviour.State.Chase || state == EnemyBehaviour.State.Repelled)
                {
                    if (stingerClip != null) stingerSource.PlayOneShot(stingerClip);
                }

                AudioClip wanted = state == EnemyBehaviour.State.Chase ? chaseClip : patrolClip;
                if (wanted != null && growlSource.clip != wanted)
                {
                    growlSource.clip = wanted;
                    growlSource.Play();
                }

                lastState = state;
            }

            float distance = player != null
                ? Vector3.Distance(transform.position, player.position)
                : maxHearDistance;
            float proximity = Mathf.Clamp01(1f - distance / maxHearDistance);
            float target = Mathf.Lerp(0.10f, 0.65f, proximity);
            if (state == EnemyBehaviour.State.Chase) target *= 1.25f;
            if (state == EnemyBehaviour.State.Repelled) target *= 0.6f;

            smoothedVolume = Mathf.Lerp(smoothedVolume, target, Time.deltaTime * 2.5f);
            growlSource.volume = smoothedVolume;
            growlSource.pitch = state == EnemyBehaviour.State.Chase ? 1.08f : 0.92f;
        }

        static AudioClip BuildGrowl(string name, float baseFrequency, float harmonic, float noiseAmount, float volume)
        {
            const int sampleRate = 44100;
            const float duration = 6f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];
            var random = new System.Random(9137 + (int)baseFrequency);

            float filteredNoise = 0f;
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float tone = Mathf.Sin(2f * Mathf.PI * baseFrequency * t) * 0.62f
                    + Mathf.Sin(2f * Mathf.PI * (baseFrequency * 1.5f) * t) * 0.24f
                    + Mathf.Sin(2f * Mathf.PI * harmonic * t) * 0.14f;

                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                filteredNoise = Mathf.Lerp(filteredNoise, noise, 0.025f);

                float breath = 0.72f + 0.28f * Mathf.Sin(2f * Mathf.PI * (1f / duration) * t);
                float edgeFade = Mathf.Min(1f, Mathf.Min(t / 0.2f, (duration - t) / 0.2f));
                data[i] = (tone + filteredNoise * noiseAmount) * breath * edgeFade * volume * 0.35f;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip BuildStinger(string name)
        {
            const int sampleRate = 44100;
            const float duration = 1.1f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];
            var random = new System.Random(4242);
            float filteredNoise = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / duration));
                float sweep = Mathf.Lerp(90f, 34f, t / duration);
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                filteredNoise = Mathf.Lerp(filteredNoise, noise, 0.06f);
                data[i] = (Mathf.Sin(2f * Mathf.PI * sweep * t) * 0.5f + filteredNoise * 0.5f)
                    * envelope * 0.32f;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
