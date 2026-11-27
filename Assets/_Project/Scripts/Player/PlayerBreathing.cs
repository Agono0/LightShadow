using LightShadow.Audio;
using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Player
{
    // Labored breathing (user brief 2026-11-27): she pants when the sprint has
    // winded her and when the wraith is close. The clip is a synthesized
    // shaped-noise loop until bro's recorded breathing ships; volume and rate
    // follow exertion and fear, and calm is silent.
    public class PlayerBreathing : MonoBehaviour
    {
        AudioSource source;
        PlayerController player;
        readonly BreathingModel model = new BreathingModel();
        float cycleClock;

        void Start()
        {
            player = GetComponent<PlayerController>();
            source = gameObject.AddComponent<AudioSource>();
            // Bro's recorded tired breath wins when the library has it
            // (SFX_Breath_Tired); the synthesized loop stays the fallback.
            AudioClip recorded = SfxRegistry.Clip("SFX_Breath_Tired");
            source.clip = recorded != null ? recorded : MakeBreathLoop();
            source.loop = true;
            source.spatialBlend = 0f; // her own breath: inside the head
            source.volume = 0f;
            source.playOnAwake = false;
            source.Play();
        }

        void OnDisable()
        {
            model.Reset();
            if (source != null) source.volume = 0f;
        }

        void Update()
        {
            BreathingInputs input = new BreathingInputs
            {
                Stamina01 = player != null ? player.Stamina01 : 1f,
                IsSprinting = player != null && player.IsSprinting,
                IsExhausted = player != null && player.IsExhausted,
                WraithDistance = 999f,
                WraithAware = false,
                HitsRemaining = PlayerHealth.Instance != null ? PlayerHealth.Instance.HitsRemaining : PlayerHealth.MaxHits,
            };

            Enemy.EnemyBehaviour enemy = GameManager.Instance != null ? GameManager.Instance.enemy : null;
            if (enemy != null)
            {
                input.WraithDistance = Vector3.Distance(transform.position, enemy.transform.position);
                // Only a wraith that has noticed her frightens her; a distant
                // patrol does not make her pant.
                Enemy.EnemyBehaviour.State state = enemy.CurrentState;
                input.WraithAware = state == Enemy.EnemyBehaviour.State.Chase
                    || state == Enemy.EnemyBehaviour.State.Stalk
                    || state == Enemy.EnemyBehaviour.State.Search;
            }

            model.Tick(input, Time.deltaTime);
            if (source == null) return;

            // One slow swell per breath: audible inhale and exhale, a gap
            // between them, instead of a constant hiss.
            cycleClock += Time.deltaTime / Mathf.Max(0.5f, model.CycleSeconds());
            if (cycleClock > 1f) cycleClock -= 1f;
            float swell = 0.5f - 0.5f * Mathf.Cos(cycleClock * Mathf.PI * 2f);

            source.volume = model.Volume() * swell * AudioDirector.Mix.Breathing / 0.55f;
            source.pitch = model.Pitch();
        }

        // One slow breath: a shaped noise inhale and exhale through a one-pole
        // lowpass, with a rest between. Quiet on purpose.
        static AudioClip MakeBreathLoop()
        {
            const int rate = 22050;
            const float seconds = 2.1f;
            int count = Mathf.CeilToInt(rate * seconds);
            float[] samples = new float[count];
            var random = new System.Random(77);
            float lowpass = 0f;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / count;
                // Envelope: inhale 0-0.3, exhale 0.35-0.75, rest after.
                float envelope = 0f;
                if (t < 0.30f) envelope = Mathf.Sin(t / 0.30f * Mathf.PI);
                else if (t < 0.75f) envelope = Mathf.Sin((t - 0.35f) / 0.40f * Mathf.PI) * 0.8f;
                float noise = (float)(random.NextDouble() * 2.0 - 1.0);
                lowpass += (noise - lowpass) * 0.18f;
                samples[i] = lowpass * envelope * 0.6f;
            }
            AudioClip clip = AudioClip.Create("BreathLoop", count, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
