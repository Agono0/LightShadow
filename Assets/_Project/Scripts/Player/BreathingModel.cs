using System;

namespace LightShadow.Player
{
    // Pure stamina-and-fear breathing model. A calm walk is silent. Breath
    // only becomes audible after real exertion (stamina spent) or real fear
    // (wraith close and aware). It ramps over seconds, not frames, recovers
    // slowly, and the cycle gets faster as intensity rises, so two steps never
    // produce panting.
    public struct BreathingInputs
    {
        // 1 full stamina .. 0 spent.
        public float Stamina01;
        public bool IsSprinting;
        public bool IsExhausted;
        // Metres to the wraith (large when gone) and whether it is aware of her.
        public float WraithDistance;
        public bool WraithAware;
        public int HitsRemaining;
    }

    public class BreathingModel
    {
        // Intensity 0..1. 0 = silent, ~0.3 = audible and steady, 1 = gasping.
        public float Intensity { get; private set; }

        // Stamina spent below this starts to matter. Walking never reaches it.
        public const float ExertionStart = 0.65f;
        // Fear only counts inside this range and only when the wraith is aware.
        public const float FearRange = 7f;

        public const float RiseSeconds = 4.0f;   // calm to gasping, at full drive
        public const float FallSeconds = 9.0f;   // gasping back to calm

        public static float ExertionDrive(float stamina01, bool sprinting, bool exhausted)
        {
            float spent = 1f - Clamp01(stamina01);
            float drive = Clamp01((spent - (1f - ExertionStart)) / ExertionStart);
            if (exhausted) drive = Math.Max(drive, 0.9f);
            // Sprinting alone is a small bump so a short dash is not silent forever.
            if (sprinting) drive = Math.Max(drive, 0.18f + 0.5f * drive);
            return Clamp01(drive);
        }

        public static float FearDrive(float distance, bool aware, int hitsRemaining)
        {
            if (!aware) return 0f;
            float d = Clamp01(1f - distance / FearRange);
            d *= d;
            if (hitsRemaining <= 1) d = Math.Min(1f, d * 1.25f);
            return d * 0.85f;
        }

        public float Target(BreathingInputs input)
        {
            float exertion = ExertionDrive(input.Stamina01, input.IsSprinting, input.IsExhausted);
            float fear = FearDrive(input.WraithDistance, input.WraithAware, input.HitsRemaining);
            return Clamp01(Math.Max(exertion, fear));
        }

        public void Tick(BreathingInputs input, float deltaTime)
        {
            if (deltaTime <= 0f) return;
            float target = Target(input);
            float rate = target > Intensity ? 1f / RiseSeconds : 1f / FallSeconds;
            float step = rate * deltaTime;
            Intensity = target > Intensity
                ? Math.Min(target, Intensity + step)
                : Math.Max(target, Intensity - step);
        }

        public void Reset()
        {
            Intensity = 0f;
        }

        // Linear volume for the breath source. Capped low; never loud.
        public float Volume()
        {
            if (Intensity < 0.05f) return 0f;
            return 0.04f + 0.26f * Intensity;
        }

        // Seconds for one full breath (inhale plus exhale plus rest).
        public float CycleSeconds()
        {
            return 3.4f - 2.0f * Intensity;
        }

        // Pitch stays almost natural; rate carries the urgency, not chipmunk pitch.
        public float Pitch()
        {
            return 0.95f + 0.12f * Intensity;
        }

        static float Clamp01(float v)
        {
            return v < 0f ? 0f : (v > 1f ? 1f : v);
        }
    }
}
