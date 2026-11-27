using LightShadow.Audio;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class DangerModelTests
    {
        static DangerInputs Calm()
        {
            return new DangerInputs
            {
                Distance = 999f, State = DangerState.Patrol, InSight = false,
                PlayerLight = 0f, HitsRemaining = 2, Hidden = false,
            };
        }

        [Test]
        public void FarWraithIsCalm()
        {
            Assert.AreEqual(0f, DangerModel.Raw(Calm()), 0.0001f);
        }

        [Test]
        public void CalmMixIsQuiet()
        {
            DangerMix mix = DangerModel.Mix(0f);
            Assert.AreEqual(0f, mix.Ambience, 0.0001f, "no ambience bed in calm");
            Assert.AreEqual(0f, mix.WraithVoice, 0.0001f);
            Assert.AreEqual(0f, mix.Breathing, 0.0001f);
            Assert.AreEqual(0f, mix.Heartbeat, 0.0001f);
            Assert.Less(mix.Sfx, 0.5f);
            Assert.Less(mix.ScareEvents, 0.2f);
        }

        [Test]
        public void DangerRisesAsWraithApproaches()
        {
            DangerInputs input = Calm();
            input.State = DangerState.Chase;
            float previous = -1f;
            foreach (float distance in new[] { 30f, 20f, 12f, 6f, 3f })
            {
                input.Distance = distance;
                float danger = DangerModel.Raw(input);
                Assert.GreaterOrEqual(danger, previous, "distance " + distance);
                previous = danger;
            }
            Assert.Greater(previous, 0.9f);
        }

        [Test]
        public void ChaseIsWorseThanPatrolAtTheSameDistance()
        {
            DangerInputs input = Calm();
            input.Distance = 8f;
            input.State = DangerState.Patrol;
            float patrol = DangerModel.Raw(input);
            input.State = DangerState.Chase;
            Assert.Greater(DangerModel.Raw(input), patrol);
        }

        [Test]
        public void BeingSeenAddsDanger()
        {
            DangerInputs input = Calm();
            input.Distance = 10f;
            input.State = DangerState.Stalk;
            float unseen = DangerModel.Raw(input);
            input.InSight = true;
            Assert.Greater(DangerModel.Raw(input), unseen);
        }

        [Test]
        public void HiddenAndUnseenMutesDanger()
        {
            DangerInputs input = Calm();
            input.Distance = 6f;
            input.State = DangerState.Search;
            float open = DangerModel.Raw(input);
            input.Hidden = true;
            Assert.Less(DangerModel.Raw(input), open);
        }

        [Test]
        public void WoundedIsMoreDangerous()
        {
            DangerInputs input = Calm();
            input.Distance = 9f;
            input.State = DangerState.Stalk;
            float healthy = DangerModel.Raw(input);
            input.HitsRemaining = 1;
            Assert.Greater(DangerModel.Raw(input), healthy);
        }

        [Test]
        public void RawStaysInRange()
        {
            DangerInputs input = Calm();
            input.State = DangerState.Chase;
            input.InSight = true;
            input.HitsRemaining = 1;
            foreach (float distance in new[] { -5f, 0f, 1f, 3f, 24f, 1000f })
            {
                input.Distance = distance;
                float danger = DangerModel.Raw(input);
                Assert.GreaterOrEqual(danger, 0f);
                Assert.LessOrEqual(danger, 1f);
            }
        }

        [Test]
        public void SmoothRisesFastAndFallsSlowly()
        {
            float up = DangerModel.Smooth(0f, 1f, 0.5f);
            float down = DangerModel.Smooth(1f, 0f, 0.5f);
            Assert.Greater(up, 1f - down, "rise step is bigger than fall step");
            Assert.AreEqual(1f, DangerModel.Smooth(0.99f, 1f, 5f), 0.0001f, "never overshoots");
            Assert.AreEqual(0.4f, DangerModel.Smooth(0.4f, 0.9f, 0f), 0.0001f, "zero dt holds");
        }

        [Test]
        public void MixNeverExceedsTheCaps()
        {
            DangerMix mix = DangerModel.Mix(5f);
            Assert.LessOrEqual(mix.Ambience, 0.56f);
            Assert.LessOrEqual(mix.Breathing, 0.56f);
            Assert.LessOrEqual(mix.Sfx, 0.81f);
            Assert.LessOrEqual(mix.WraithVoice, 0.91f);
            Assert.LessOrEqual(mix.Footsteps, 0.91f);
        }

        [Test]
        public void ScareEventsAreRarerWhenCalm()
        {
            float calm = DangerModel.ScareDelaySeconds(0f, 0f);
            float tense = DangerModel.ScareDelaySeconds(1f, 1f);
            Assert.GreaterOrEqual(calm, 70f);
            Assert.Less(tense, calm);
            Assert.GreaterOrEqual(DangerModel.ScareDelaySeconds(1f, 0f), 20f);
        }

        [Test]
        public void LoudSoundsTravelFurther()
        {
            Assert.Greater(DangerModel.AudibleRadius(1f), DangerModel.AudibleRadius(0.2f));
            Assert.LessOrEqual(DangerModel.AudibleRadius(5f), 22.01f);
        }

        [Test]
        public void StateOrderMatchesEnemyBehaviour()
        {
            // AudioDirector casts EnemyBehaviour.State to DangerState by number.
            string[] enemy = System.Enum.GetNames(typeof(LightShadow.Enemy.EnemyBehaviour.State));
            string[] danger = System.Enum.GetNames(typeof(DangerState));
            Assert.AreEqual(enemy.Length, danger.Length);
            for (int i = 0; i < enemy.Length; i++) Assert.AreEqual(enemy[i], danger[i]);
        }
    }
}
