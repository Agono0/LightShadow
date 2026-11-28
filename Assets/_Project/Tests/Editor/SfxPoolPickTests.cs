using LightShadow.Core;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class SfxPoolPickTests
    {
        [Test]
        public void PicksFirstFreeSource()
        {
            Assert.AreEqual(1, AudioManager.PickPoolIndex(new[] { true, false, false }, 0));
        }

        [Test]
        public void AllBusy_StealsRoundRobin()
        {
            bool[] busy = { true, true, true };
            Assert.AreEqual(2, AudioManager.PickPoolIndex(busy, 2));
            Assert.AreEqual(0, AudioManager.PickPoolIndex(busy, 3));
        }

        [Test]
        public void EmptyOrNull_ReturnsZero()
        {
            Assert.AreEqual(0, AudioManager.PickPoolIndex(null, 5));
            Assert.AreEqual(0, AudioManager.PickPoolIndex(new bool[0], 5));
        }
    }
}
