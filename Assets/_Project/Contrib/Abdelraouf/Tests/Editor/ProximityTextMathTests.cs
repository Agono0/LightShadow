using LightShadow.Contrib.Abdelraouf.ProximityText;
using NUnit.Framework;

namespace LightShadow.Contrib.Abdelraouf.Tests.Editor
{
    // EditMode tests for the proximity gating math (pure logic, no scene).
    public class ProximityTextMathTests
    {
        const float MinCos60 = 0.5f; // cos(60 deg)

        [Test]
        public void MinCosForDegrees_ClampsToHalfCircle()
        {
            Assert.AreEqual(1f, ProximityTextMath.MinCosForDegrees(-10f), 0.0001f);
            Assert.AreEqual(-1f, ProximityTextMath.MinCosForDegrees(270f), 0.0001f);
            Assert.AreEqual(MinCos60, ProximityTextMath.MinCosForDegrees(60f), 0.0001f);
        }

        [Test]
        public void Visible_ShowsInsideShowRadius_WhenInAngle()
        {
            Assert.IsTrue(ProximityTextMath.Visible(false, 3f, 1f, 4f, 5f, MinCos60));
        }

        [Test]
        public void Visible_StaysHiddenBetweenRadii_Hysteresis()
        {
            Assert.IsFalse(ProximityTextMath.Visible(false, 4.5f, 1f, 4f, 5f, MinCos60),
                "hidden text must not appear between show and hide radii");
        }

        [Test]
        public void Visible_StaysVisibleBetweenRadii_Hysteresis()
        {
            Assert.IsTrue(ProximityTextMath.Visible(true, 4.5f, 1f, 4f, 5f, MinCos60));
        }

        [Test]
        public void Visible_HidesBeyondHideRadius()
        {
            Assert.IsFalse(ProximityTextMath.Visible(true, 5.5f, 1f, 4f, 5f, MinCos60));
        }

        [Test]
        public void Visible_AngleGateAppliesBothWays()
        {
            Assert.IsFalse(ProximityTextMath.Visible(false, 1f, 0f, 4f, 5f, MinCos60),
                "close but behind the camera: must not appear");
            Assert.IsFalse(ProximityTextMath.Visible(true, 1f, 0f, 4f, 5f, MinCos60),
                "leaving the view cone hides even while close");
        }

        [Test]
        public void Visible_HideRadiusBelowShowRadius_IsClamped()
        {
            Assert.IsFalse(ProximityTextMath.Visible(true, 4.5f, 1f, 4f, 2f, MinCos60));
        }

        [Test]
        public void StepAlpha_MonotonicTowardTarget_AndClamped()
        {
            float up = ProximityTextMath.StepAlpha(0f, true, 0.1f, 3f);
            Assert.Greater(up, 0f);
            float over = ProximityTextMath.StepAlpha(0.9f, true, 1f, 3f);
            Assert.AreEqual(1f, over, "alpha never overshoots 1");
            float down = ProximityTextMath.StepAlpha(1f, false, 0.1f, 3f);
            Assert.Less(down, 1f);
            float under = ProximityTextMath.StepAlpha(0.1f, false, 1f, 3f);
            Assert.AreEqual(0f, under, "alpha never undershoots 0");
        }

        [Test]
        public void StepAlpha_ZeroDeltaTime_DoesNotMove()
        {
            Assert.AreEqual(0.4f, ProximityTextMath.StepAlpha(0.4f, true, 0f, 3f), 0.0001f);
        }
    }
}
