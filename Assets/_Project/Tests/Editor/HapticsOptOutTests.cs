using LightShadow.Platform;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    public class HapticsOptOutTests
    {
        int saved;

        [SetUp]
        public void SetUp() { saved = PlayerPrefs.GetInt(HapticsService.PrefKey, 1); }

        [TearDown]
        public void TearDown() { PlayerPrefs.SetInt(HapticsService.PrefKey, saved); }

        [Test]
        public void DefaultsOnAndRoundTrips()
        {
            PlayerPrefs.DeleteKey(HapticsService.PrefKey);
            Assert.IsTrue(HapticsService.Enabled);
            HapticsService.Enabled = false;
            Assert.IsFalse(HapticsService.Enabled);
            HapticsService.Enabled = true;
            Assert.IsTrue(HapticsService.Enabled);
        }
    }
}
