using LightShadow.UI;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class EndingLineBudgetTests
    {
        [Test]
        public void NoClip_UsesBudget()
        {
            Assert.AreEqual(3.4f, EndingCinematicController.LineSeconds(3.4f, 0f, 0.6f, 14f), 0.0001f);
        }

        [Test]
        public void ShortClip_KeepsBudget()
        {
            Assert.AreEqual(5.5f, EndingCinematicController.LineSeconds(5.5f, 2f, 0.6f, 14f), 0.0001f);
        }

        [Test]
        public void LongClip_ExtendsToClipPlusTail()
        {
            Assert.AreEqual(9.2f, EndingCinematicController.LineSeconds(3.4f, 8.6f, 0.6f, 14f), 0.0001f);
        }

        [Test]
        public void HugeClip_IsCapped()
        {
            Assert.AreEqual(14f, EndingCinematicController.LineSeconds(3.4f, 60f, 0.6f, 14f), 0.0001f);
        }

        [Test]
        public void CapNeverCutsBelowBudget()
        {
            Assert.AreEqual(5f, EndingCinematicController.LineSeconds(5f, 20f, 0.6f, 3f), 0.0001f);
        }

        [Test]
        public void BadLength_FallsBackToBudget()
        {
            Assert.AreEqual(3f, EndingCinematicController.LineSeconds(3f, float.NaN, 0.6f, 14f), 0.0001f);
            Assert.AreEqual(3f, EndingCinematicController.LineSeconds(3f, float.PositiveInfinity, 0.6f, 14f), 0.0001f);
        }
    }
}
