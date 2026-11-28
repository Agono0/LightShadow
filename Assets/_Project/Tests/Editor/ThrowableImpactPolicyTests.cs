using LightShadow.Gameplay;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class ThrowableImpactPolicyTests
    {
        const float DropImpact = 4.97f; // free fall from the carry hold point

        [Test]
        public void GentleDrop_DoesNotShatterBottle()
        {
            Assert.AreNotEqual(ThrowableProp.ImpactKind.Shatter,
                ThrowableProp.Classify(DropImpact, true, true));
        }

        [Test]
        public void Throw_ShattersBottleAtSpeed()
        {
            Assert.AreEqual(ThrowableProp.ImpactKind.Shatter, ThrowableProp.Classify(DropImpact, true, false));
        }

        [Test]
        public void NonBreakable_HardAboveFour_SoftBelow()
        {
            Assert.AreEqual(ThrowableProp.ImpactKind.Hard, ThrowableProp.Classify(5f, false, false));
            Assert.AreEqual(ThrowableProp.ImpactKind.Soft, ThrowableProp.Classify(3f, false, false));
        }

        [Test]
        public void BelowNoiseGate_IsNone()
        {
            Assert.AreEqual(ThrowableProp.ImpactKind.None, ThrowableProp.Classify(1f, true, false));
            Assert.AreEqual(ThrowableProp.ImpactKind.None, ThrowableProp.Classify(1f, true, true));
        }

        [Test]
        public void GraceWindow_ExpiresAndNeedsArming()
        {
            Assert.IsTrue(ThrowableProp.IsGentleActive(true, 1f, 3f));
            Assert.IsFalse(ThrowableProp.IsGentleActive(true, 3.5f, 3f));
            Assert.IsFalse(ThrowableProp.IsGentleActive(false, 1f, 3f));
        }
    }
}
