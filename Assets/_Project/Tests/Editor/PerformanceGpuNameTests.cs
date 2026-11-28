using LightShadow.Systems;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class PerformanceGpuNameTests
    {
        [TestCase("AMD Radeon(TM) Graphics")]
        [TestCase("AMD Radeon(TM) Vega 8 Graphics")]
        [TestCase("AMD Radeon 780M Graphics")]
        public void AmdApuNames_AreIntegrated(string name)
        {
            Assert.IsTrue(PerformanceManager.LooksLikeAmdIntegrated(name));
        }

        [TestCase("AMD Radeon RX 6600")]
        [TestCase("AMD Radeon Pro W6800")]
        [TestCase("NVIDIA GeForce RTX 3060")]
        [TestCase("")]
        [TestCase(null)]
        public void DiscreteOrUnknown_AreNotIntegrated(string name)
        {
            Assert.IsFalse(PerformanceManager.LooksLikeAmdIntegrated(name));
        }
    }
}
