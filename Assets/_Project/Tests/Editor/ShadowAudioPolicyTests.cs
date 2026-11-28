using LightShadow.Enemy;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class ShadowAudioPolicyTests
    {
        [Test]
        public void NearBreath_SilentWhileAmbushing()
        {
            Assert.IsFalse(ShadowAudio.NearBreathAllowed(EnemyBehaviour.State.Ambush, 3f));
        }

        [Test]
        public void NearBreath_SilentWhenRepelled()
        {
            Assert.IsFalse(ShadowAudio.NearBreathAllowed(EnemyBehaviour.State.Repelled, 3f));
        }

        [Test]
        public void NearBreath_PlaysCloseInOtherStates()
        {
            Assert.IsTrue(ShadowAudio.NearBreathAllowed(EnemyBehaviour.State.Stalk, 3f));
            Assert.IsTrue(ShadowAudio.NearBreathAllowed(EnemyBehaviour.State.Chase, 4.9f));
        }

        [Test]
        public void NearBreath_SilentAtOrBeyondRange()
        {
            Assert.IsFalse(ShadowAudio.NearBreathAllowed(EnemyBehaviour.State.Stalk, 5f));
        }
    }
}
