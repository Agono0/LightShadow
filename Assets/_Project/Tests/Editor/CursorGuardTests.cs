using LightShadow.Core;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class CursorGuardTests
    {
        [Test]
        public void RelocksWhenNothingIsOpen()
        {
            Assert.IsTrue(CursorGuard.ShouldRelock(false, true, false, false, false, false, true));
        }

        [Test]
        public void LeavesCursorAloneWhileAnythingIsOpen()
        {
            Assert.IsFalse(CursorGuard.ShouldRelock(false, true, true, false, false, false, true));
            Assert.IsFalse(CursorGuard.ShouldRelock(false, true, false, true, false, false, true));
            Assert.IsFalse(CursorGuard.ShouldRelock(false, true, false, false, true, false, true));
            Assert.IsFalse(CursorGuard.ShouldRelock(false, true, false, false, false, true, true));
        }

        [Test]
        public void NeverActsOnMobileOrOutsideGameplayOrWhenAlreadyLocked()
        {
            Assert.IsFalse(CursorGuard.ShouldRelock(true, true, false, false, false, false, true));
            Assert.IsFalse(CursorGuard.ShouldRelock(false, false, false, false, false, false, true));
            Assert.IsFalse(CursorGuard.ShouldRelock(false, true, false, false, false, false, false));
        }
    }
}
