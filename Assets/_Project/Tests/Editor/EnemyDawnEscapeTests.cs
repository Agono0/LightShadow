using LightShadow.Enemy;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class EnemyDawnEscapeTests
    {
        [Test]
        public void Grace_StopsInvestigateAndSearchCatches_ButNotStalkOrChase()
        {
            Assert.IsFalse(EnemyBehaviour.CatchAllowedDuringGrace(EnemyBehaviour.State.Investigate, false, true));
            Assert.IsFalse(EnemyBehaviour.CatchAllowedDuringGrace(EnemyBehaviour.State.Search, false, true));
            Assert.IsTrue(EnemyBehaviour.CatchAllowedDuringGrace(EnemyBehaviour.State.Stalk, false, true));
            Assert.IsTrue(EnemyBehaviour.CatchAllowedDuringGrace(EnemyBehaviour.State.Chase, false, true));
        }

        [Test]
        public void NoGrace_OrDawn_CatchesAsBefore()
        {
            Assert.IsTrue(EnemyBehaviour.CatchAllowedDuringGrace(EnemyBehaviour.State.Investigate, false, false));
            Assert.IsTrue(EnemyBehaviour.CatchAllowedDuringGrace(EnemyBehaviour.State.Investigate, true, true));
        }

        [Test]
        public void DawnEscapeWindow_IsRealButShorterThanCatchRearm()
        {
            Assert.Greater(EnemyBehaviour.DawnEscapeSeconds, 3f);
            Assert.LessOrEqual(EnemyBehaviour.DawnEscapeSeconds, CatchRecovery.RearmSeconds);
            Assert.IsFalse(EnemyBehaviour.CanForceCooldown(true));
        }
    }
}
