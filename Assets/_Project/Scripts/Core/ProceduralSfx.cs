using UnityEngine;

namespace LightShadow.Core
{
    // The interaction sounds are LIBRARY-FIRST since the audio pass: when the
    // imported CC0 clip exists (registered by AudioLibraryBootstrap) it plays;
    // otherwise the synthesised fallback below still keeps the game audible
    // with zero external assets. Nothing here throws when the library is
    // missing.
    public static class ProceduralSfx
    {
        const int SampleRate = 44100;

        // Library-first: the imported CC0 clip wins; the synthesised fallback
        // keeps every call site audible when the library has not shipped.
        static AudioClip Resolve(string id, System.Func<AudioClip> synthesise)
        {
            AudioClip imported = LightShadow.Audio.SfxRegistry.Clip(id);
            return imported != null ? imported : synthesise();
        }

        static AudioClip correct;
        static AudioClip wrong;
        static AudioClip click;
        static AudioClip pickup;
        static AudioClip power;
        static AudioClip crank;
        static AudioClip heartbeat;

        public static AudioClip Correct
        {
            get
            {
                if (correct == null) correct = Resolve("SFX_QuizCorrect", () => MakeSequence("SFX_Correct", new[] { 660f, 990f }, 0.22f, 0.35f));
                return correct;
            }
        }

        public static AudioClip Wrong
        {
            get
            {
                if (wrong == null) wrong = Resolve("SFX_QuizWrong", () => MakeSequence("SFX_Wrong", new[] { 220f, 165f }, 0.28f, 0.32f));
                return wrong;
            }
        }

        public static AudioClip Click
        {
            get
            {
                if (click == null) click = Resolve("SFX_Click", () => MakeSequence("SFX_Click", new[] { 900f }, 0.05f, 0.22f));
                return click;
            }
        }

        public static AudioClip Pickup
        {
            get
            {
                if (pickup == null) pickup = Resolve("SFX_Pickup", () => MakeSequence("SFX_Pickup", new[] { 660f, 880f, 1100f }, 0.24f, 0.28f));
                return pickup;
            }
        }

        public static AudioClip Power
        {
            get
            {
                if (power == null) power = Resolve("SFX_Power", () => MakeSequence("SFX_Power", new[] { 110f, 165f, 220f, 330f }, 0.7f, 0.38f));
                return power;
            }
        }

        // P3 crank ratchet (plan 4.6): a tight mechanical clack per tick. The
        // library-first id is "SFX_Crank"; this synthesised fallback keeps the
        // crank audible with zero external assets.
        public static AudioClip Crank
        {
            get
            {
                if (crank == null) crank = Resolve("SFX_Crank", () => MakeSequence("SFX_Crank", new[] { 1500f, 900f }, 0.09f, 0.30f));
                return crank;
            }
        }

        // P3 calm FX (plan 4.7): a low lub-dub thump the heartbeat pacer plays
        // as calm drops. Library-first id "SFX_Heartbeat".
        public static AudioClip Heartbeat
        {
            get
            {
                if (heartbeat == null) heartbeat = Resolve("SFX_Heartbeat", () => MakeSequence("SFX_Heartbeat", new[] { 62f, 44f }, 0.4f, 0.9f));
                return heartbeat;
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
