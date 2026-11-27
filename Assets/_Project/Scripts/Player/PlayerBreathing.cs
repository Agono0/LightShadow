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
        float intensity; // 0 calm .. 1 gasping

        void Start()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.clip = MakeBreathLoop();
            source.loop = true;
            source.spatialBlend = 0f; // her own breath: inside the head
            source.volume = 0f;
            source.playOnAwake = false;
            source.Play();
        }

        void Update()
        {
            float exertion = 0f;
            PlayerController player = GetComponent<PlayerController>();
            if (player != null)
            {
                exertion = player.IsSprinting ? 0.8f : 1f - player.Stamina01;
                if (player.IsExhausted) exertion = 1f;
            }

            float fear = 0f;
            Enemy.EnemyBehaviour enemy = GameManager.Instance != null ? GameManager.Instance.enemy : null;
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                fear = Mathf.Clamp01((14f - distance) / 14f);
            }

            float target = Mathf.Clamp01(Mathf.Max(exertion, fear * 0.9f));
            intensity = Mathf.MoveTowards(intensity, target, Time.deltaTime * 0.5f);
            if (source != null)
            {
                source.volume = intensity * 0.35f;
                source.pitch = Mathf.Lerp(0.85f, 1.5f, intensity);
            }
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
