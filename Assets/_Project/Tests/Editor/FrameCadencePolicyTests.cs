using LightShadow.Systems;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class FrameCadencePolicyTests
    {
        [Test]
        public void VSyncOn_ManagerDoesNotOwnCadence()
        {
            Assert.IsFalse(PerformanceManager.OwnsFrameCadence(true));
        }

        [Test]
        public void VSyncOff_ManagerOwnsCadence()
        {
            Assert.IsTrue(PerformanceManager.OwnsFrameCadence(false));
        }
    }
}
