using LightShadow.Story;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    // Unity pass (2026-11-28): fixes found by playing in the editor.
    public class UnityPassTests
    {
        [Test]
        public void TypewriterFollowsTheVoiceClip()
        {
            // 60 characters over a 7.5 s line: typed across 80% of the clip.
            string line = new string('a', 60);
            Assert.AreEqual(10f, DialogueUIController.VoiceSyncedCharsPerSecond(line, 40f, 7.5f), 0.01f);
        }

        [Test]
        public void TypewriterKeepsThePlayerSpeedWithoutAVoice()
        {
            Assert.AreEqual(40f, DialogueUIController.VoiceSyncedCharsPerSecond("hello", 40f, 0f));
            Assert.AreEqual(40f, DialogueUIController.VoiceSyncedCharsPerSecond("", 40f, 7f));
        }

        [Test]
        public void TypewriterNeverCrawlsOrRaces()
        {
            // A long clip for a short line stays readable (>= 8 cps), a tiny clip
            // for a long line is capped at three times the player's speed.
            Assert.AreEqual(8f, DialogueUIController.VoiceSyncedCharsPerSecond("hi", 40f, 20f));
            Assert.AreEqual(120f, DialogueUIController.VoiceSyncedCharsPerSecond(new string('a', 1000), 40f, 1f));
        }
    }
}
