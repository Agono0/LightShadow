using LightShadow.UI;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class MobileUiScaleTests
    {
        [Test]
        public void WidePhones_MatchHeight()
        {
            Assert.AreEqual(1f, MobileUiScale.MatchFor(2.2f));
            Assert.AreEqual(1f, MobileUiScale.MatchFor(16f / 9f));
        }

        [Test]
        public void SquarerScreens_MatchWidth_SoTheQuizCardFits()
        {
            Assert.AreEqual(0f, MobileUiScale.MatchFor(4f / 3f));
            Assert.AreEqual(0f, MobileUiScale.MatchFor(1.6f));
        }
    }
}
