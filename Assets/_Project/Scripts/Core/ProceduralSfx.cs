using UnityEngine;

namespace LightShadow.Core
{
    // The project ships without recorded audio, so the small interaction sounds
    // are synthesised once at runtime. This keeps the game audible with zero
    // external assets and no package downloads.
    public static class ProceduralSfx
    {
        const int SampleRate = 44100;

        static AudioClip correct;
        static AudioClip wrong;
        static AudioClip click;
        static AudioClip pickup;
        static AudioClip power;

        public static AudioClip Correct
        {
            get
            {
                if (correct == null) correct = MakeSequence("SFX_Correct", new[] { 660f, 990f }, 0.22f, 0.35f);
                return correct;
            }
        }

        public static AudioClip Wrong
        {
            get
            {
                if (wrong == null) wrong = MakeSequence("SFX_Wrong", new[] { 220f, 165f }, 0.28f, 0.32f);
                return wrong;
            }
        }

        public static AudioClip Click
        {
            get
            {
                if (click == null) click = MakeSequence("SFX_Click", new[] { 900f }, 0.05f, 0.22f);
                return click;
            }
        }

        public static AudioClip Pickup
        {
            get
            {
                if (pickup == null) pickup = MakeSequence("SFX_Pickup", new[] { 660f, 880f, 1100f }, 0.24f, 0.28f);
                return pickup;
            }
        }

        public static AudioClip Power
        {
            get
            {
                if (power == null) power = MakeSequence("SFX_Power", new[] { 110f, 165f, 220f, 330f }, 0.7f, 0.38f);
                return power;
            }
        }

        static AudioClip MakeSequence(string name, float[] frequencies, float duration, float volume)
        {
            int totalSamples = Mathf.Max(1, Mathf.CeilToInt(SampleRate * duration));
            float[] data = new float[totalSamples];
            int segmentSamples = Mathf.Max(1, totalSamples / frequencies.Length);

            for (int i = 0; i < totalSamples; i++)
            {
                int segment = Mathf.Min(i / segmentSamples, frequencies.Length - 1);
                float local = (i - segment * segmentSamples) / (float)segmentSamples;
                float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(local));
                float time = i / (float)SampleRate;
                data[i] = Mathf.Sin(2f * Mathf.PI * frequencies[segment] * time) * envelope * volume;
            }

            AudioClip clip = AudioClip.Create(name, totalSamples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
