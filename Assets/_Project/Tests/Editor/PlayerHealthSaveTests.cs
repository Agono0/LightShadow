using LightShadow.Core;
using LightShadow.Player;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class PlayerHealthSaveTests
    {
        [Test]
        public void MissingOrBrokenBlob_RestoresFullHealth()
        {
            Assert.AreEqual(PlayerHealth.MaxHits, PlayerHealth.DecodeHits(""));
            Assert.AreEqual(PlayerHealth.MaxHits, PlayerHealth.DecodeHits("abc"));
        }

        [Test]
        public void OneHitLeft_SurvivesARoundTrip()
        {
            Assert.AreEqual(1, PlayerHealth.DecodeHits(PlayerHealth.EncodeHits(1)));
        }

        [Test]
        public void ADeadState_IsNeverSaved_AndOutOfRangeIsClamped()
        {
            Assert.AreEqual("1", PlayerHealth.EncodeHits(0));
            Assert.AreEqual(PlayerHealth.MaxHits, PlayerHealth.DecodeHits("99"));
            Assert.AreEqual(1, PlayerHealth.DecodeHits("-4"));
        }

        [Test]
        public void ContribKey_RoundTripsThroughSaveData()
        {
            var data = new SaveSystem.SaveData();
            SaveSystem.SetContrib(data, PlayerHealth.SaveKey, PlayerHealth.EncodeHits(1));
            Assert.AreEqual(1, PlayerHealth.DecodeHits(SaveSystem.GetContrib(data, PlayerHealth.SaveKey)));
        }
    }
}
