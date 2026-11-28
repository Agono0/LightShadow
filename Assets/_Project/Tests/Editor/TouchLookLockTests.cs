using LightShadow.Player;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class TouchLookLockTests
    {
        [Test]
        public void Locked_DropsLookFinger()
        {
            Assert.AreEqual(-1, TouchInput.LookFingerAfterLock(true, 3));
        }

        [Test]
        public void Unlocked_KeepsLookFinger()
        {
            Assert.AreEqual(3, TouchInput.LookFingerAfterLock(false, 3));
            Assert.AreEqual(-1, TouchInput.LookFingerAfterLock(false, -1));
        }

        [Test]
        public void CanLook_FollowsLock()
        {
            Assert.IsFalse(TouchInput.CanLook(true));
            Assert.IsTrue(TouchInput.CanLook(false));
        }
    }
}
