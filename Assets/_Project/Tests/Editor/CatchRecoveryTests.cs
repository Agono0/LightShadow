using LightShadow.Enemy;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class CatchRecoveryTests
    {
        [Test]
        public void SurvivedStrikeClearsCatchingAndRetreats()
        {
            CatchRecovery.Plan plan = CatchRecovery.AfterStrike(true);
            Assert.IsTrue(plan.ClearCatching, "the first-hit freeze: this flag was never cleared");
            Assert.IsTrue(plan.Retreat);
            Assert.IsFalse(plan.ResetToPatrol);
            Assert.Greater(plan.RetreatSeconds, 0f);
            Assert.Greater(plan.CatchCooldownSeconds, 0f);
        }

        [Test]
        public void SecondStrikeIsNeverFree()
        {
            // After a survived strike the wraith leaves for longer than the
            // time it needs to come back and re-arm, so Nour gets room to run.
            CatchRecovery.Plan plan = CatchRecovery.AfterStrike(true);
            Assert.GreaterOrEqual(plan.RetreatSeconds, 5f);
            Assert.GreaterOrEqual(plan.CatchCooldownSeconds, 3f);
        }

        [Test]
        public void LethalStrikeWaitsForTheCheckpointWake()
        {
            CatchRecovery.Plan plan = CatchRecovery.AfterStrike(false);
            Assert.IsFalse(plan.ClearCatching);
            Assert.IsFalse(plan.Retreat);
            Assert.IsTrue(plan.ResetToPatrol);
        }

        [Test]
        public void WatchdogFiresOnlyAfterItsLimit()
        {
            Assert.IsFalse(CatchRecovery.WatchdogExpired(0f));
            Assert.IsFalse(CatchRecovery.WatchdogExpired(CatchRecovery.WatchdogSeconds - 0.01f));
            Assert.IsTrue(CatchRecovery.WatchdogExpired(CatchRecovery.WatchdogSeconds));
        }

        [Test]
        public void WatchdogOutlastsTheLethalFade()
        {
            // GameManager fades to black for 2 s before the checkpoint wake.
            Assert.Greater(CatchRecovery.WatchdogSeconds, 2.5f);
        }
    }
}
