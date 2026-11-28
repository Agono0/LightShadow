namespace LightShadow.Enemy
{
    // Pure rules for what the wraith does after it strikes Nour. Keeping them
    // out of EnemyBehaviour makes the first-hit freeze (catching stayed true
    // forever after a non-lethal strike) testable without a scene.
    public static class CatchRecovery
    {
        // Seconds the wraith withdraws after a strike Nour survives.
        public const float RetreatSeconds = 9f;
        // Seconds before it may strike again once it is back on patrol.
        public const float RearmSeconds = 6f;
        // If the catch sequence has not handed back after this long, force it.
        public const float WatchdogSeconds = 4f;

        public struct Plan
        {
            public bool ClearCatching;
            public bool Retreat;
            public bool ResetToPatrol;
            public float RetreatSeconds;
            public float CatchCooldownSeconds;
        }

        // survived = Nour still has health after the strike.
        public static Plan AfterStrike(bool survived)
        {
            if (survived)
            {
                return new Plan
                {
                    ClearCatching = true,
                    Retreat = true,
                    ResetToPatrol = false,
                    RetreatSeconds = RetreatSeconds,
                    CatchCooldownSeconds = RearmSeconds,
                };
            }

            // Lethal: the wraith stays frozen through the fade and the
            // checkpoint wake resets it (GameManager.WakeAtCheckpoint). The
            // watchdog is the safety net if that wake never comes.
            return new Plan
            {
                ClearCatching = false,
                Retreat = false,
                ResetToPatrol = true,
                RetreatSeconds = 0f,
                CatchCooldownSeconds = 1.5f,
            };
        }

        public static bool WatchdogExpired(float secondsCatching)
        {
            return secondsCatching >= WatchdogSeconds;
        }
    }
}
