using LightShadow.Story;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    // The accessibility setters wrap (cycle) instead of clamping at the ends;
    // these tests pin that behaviour so a refactor cannot silently turn the
    // settings rows into dead ends.
    public class AccessibilityStoreTests
    {
        [Test]
        public void SetSkipHold_WrapsPastBothEnds()
        {
            AccessibilityStore.SetSkipHold(1);
            Assert.AreEqual(0.8f, AccessibilityStore.SkipHoldSeconds, 0.0001f);

            AccessibilityStore.SetSkipHold(3); // past max 2 -> wraps to 0
            Assert.AreEqual(0, AccessibilityStore.SkipHoldIndex);
            Assert.AreEqual(0.4f, AccessibilityStore.SkipHoldSeconds, 0.0001f);

            AccessibilityStore.SetSkipHold(-1); // below min -> wraps to max
            Assert.AreEqual(2, AccessibilityStore.SkipHoldIndex);
            Assert.AreEqual(1.5f, AccessibilityStore.SkipHoldSeconds, 0.0001f);

            AccessibilityStore.SetSkipHold(1);
        }

        [Test]
        public void SetTextSpeed_WrapsAndInstantAtMax()
        {
            AccessibilityStore.SetTextSpeed(4); // past max 3 -> wraps to 0
            Assert.AreEqual(0, AccessibilityStore.TextSpeed);

            AccessibilityStore.SetTextSpeed(3);
            Assert.AreEqual(float.MaxValue, AccessibilityStore.TypewriterCharsPerSecond);

            AccessibilityStore.SetTextSpeed(0);
            Assert.Greater(AccessibilityStore.TypewriterCharsPerSecond, 0f);
            Assert.Less(AccessibilityStore.TypewriterCharsPerSecond, float.MaxValue);

            AccessibilityStore.SetTextSpeed(1);
        }

        [Test]
        public void SetQualityTier_OutOfRangeFallsBackToAuto()
        {
            AccessibilityStore.SetQualityTier(2);
            Assert.AreEqual(2, AccessibilityStore.QualityTier);

            AccessibilityStore.SetQualityTier(9);
            Assert.AreEqual(-1, AccessibilityStore.QualityTier);

            AccessibilityStore.SetQualityTier(-1);
        }
    }
}
