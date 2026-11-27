using System;

namespace LightShadow.Audio
{
    // Pure model (no UnityEngine types) that turns "how dangerous is this
    // moment" into one number, 0 calm .. 1 mortal danger, and from that into
    // the loudness of each sound family. The game should be quiet and calm and
    // get loud only as the wraith closes in, so every audible thing asks this
    // model instead of owning its own volume.
    //
    // Inputs are plain numbers so the model is covered by EditMode tests with
    // no scene. AudioDirector samples the world and feeds it.
    public enum DangerState
    {
        Patrol = 0,
        Stalk = 1,
        Chase = 2,
        Repelled = 3,
        Investigate = 4,
        ForcedCooldown = 5,
        Search = 6,
        Ambush = 7,
    }

    public struct DangerInputs
    {
        // Straight-line metres to the wraith. Use a large number when it is gone.
        public float Distance;
        public DangerState State;
        // True when the wraith has line of sight to Nour (or she to it).
        public bool InSight;
        // 0 dark .. 1 fully lit by her torch or a lit zone. A lit player is safer.
        public float PlayerLight;
        // Strikes left: 2 untouched, 1 hurt, 0 dead.
        public int HitsRemaining;
        // True while hidden in a locker or similar.
        public bool Hidden;
    }

    public struct DangerMix
    {
        // Multipliers applied on top of each bus. 0 = silent, 1 = full.
        public float Ambience;
        public float ScareEvents;
        public float Footsteps;
        public float Breathing;
        public float Heartbeat;
        public float WraithVoice;
        public float Sfx;
    }

    public static class DangerModel
    {
        // Beyond this the wraith adds nothing to the danger number.
        public const float FarDistance = 24f;
        // Inside this it is as dangerous as it gets.
        public const float NearDistance = 3f;

        // Instantaneous danger before smoothing.
        public static float Raw(DangerInputs input)
        {
            float distance = Math.Max(0f, input.Distance);
            if (distance >= FarDistance) return 0f;

            float proximity = 1f - Clamp01((distance - NearDistance) / (FarDistance - NearDistance));
            // Ease the curve: far away adds little, the last few metres add a lot.
            proximity = proximity * proximity;

            float stateWeight = StateWeight(input.State);
            float danger = proximity * stateWeight;

            // Being seen matters more than being near.
            if (input.InSight) danger = Math.Min(1f, danger + 0.25f * proximity + 0.1f);

            // A lit player is safer, but never safe when it is on top of her.
            danger *= 1f - 0.35f * Clamp01(input.PlayerLight) * (1f - Clamp01(1f - distance / 4f));

            // Wounded: everything feels worse.
            if (input.HitsRemaining <= 1) danger = Math.Min(1f, danger * 1.2f + 0.05f * proximity);

            // Hidden and unseen: the world holds its breath, it does not shout.
            if (input.Hidden && !input.InSight) danger *= 0.6f;

            return Clamp01(danger);
        }

        public static float StateWeight(DangerState state)
        {
            switch (state)
            {
                case DangerState.Chase: return 1f;
                case DangerState.Ambush: return 0.85f;
                case DangerState.Stalk: return 0.7f;
                case DangerState.Search: return 0.55f;
                case DangerState.Investigate: return 0.45f;
                case DangerState.Patrol: return 0.3f;
                case DangerState.Repelled: return 0.2f;
                case DangerState.ForcedCooldown: return 0.05f;
                default: return 0.3f;
            }
        }

        // Smooths toward the target: fast up (danger arrives suddenly), slow
        // down (the dread lingers). Rates are per second.
        public static float Smooth(float current, float target, float deltaTime, float riseRate = 1.6f, float fallRate = 0.25f)
        {
            if (deltaTime <= 0f) return current;
            float rate = target > current ? riseRate : fallRate;
            float step = rate * deltaTime;
            if (target > current) return Math.Min(target, current + step);
            return Math.Max(target, current - step);
        }

        // Maps danger 0..1 to a gain per sound family. Calm is nearly silent;
        // each family has its own floor and ceiling so nothing screams.
        public static DangerMix Mix(float danger)
        {
            float d = Clamp01(danger);
            return new DangerMix
            {
                Ambience = Lerp(0.0f, 0.55f, Curve(d, 1.5f)),
                ScareEvents = Lerp(0.15f, 0.8f, Curve(d, 1.2f)),
                Footsteps = Lerp(0.55f, 0.9f, d),
                Breathing = Lerp(0.0f, 0.55f, Curve(d, 1.4f)),
                Heartbeat = Curve(Math.Max(0f, d - 0.4f) / 0.6f, 1.2f) * 0.6f,
                WraithVoice = Lerp(0.0f, 0.9f, Curve(d, 1.1f)),
                Sfx = Lerp(0.45f, 0.8f, d),
            };
        }

        // How long to wait before the next ambient scare event. Quiet rooms
        // get long silences; danger shortens them but never below a floor.
        public static float ScareDelaySeconds(float danger, float random01)
        {
            float d = Clamp01(danger);
            float min = Lerp(70f, 20f, d);
            float max = Lerp(150f, 45f, d);
            return Lerp(min, max, Clamp01(random01));
        }

        // Linear volume to a metre-scaled rolloff end for a positional sound:
        // a loud sound travels further, a soft one stays local.
        public static float AudibleRadius(float loudness01)
        {
            return Lerp(4f, 22f, Clamp01(loudness01));
        }

        static float Curve(float t, float power)
        {
            return (float)Math.Pow(Clamp01(t), power);
        }

        static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp01(t);
        }

        static float Clamp01(float v)
        {
            return v < 0f ? 0f : (v > 1f ? 1f : v);
        }
    }
}
