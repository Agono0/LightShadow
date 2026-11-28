using NUnit.Framework;
using LightShadow.Enemy;

namespace LightShadow.Tests
{
    // Pure policy seams only; the FSM itself needs a NavMesh and was not run.
    public class EnemyHiderChaseTests
    {
        [Test] public void Hidden_counts_as_sight_lost_even_when_sighted() { Assert.IsTrue(EnemyBehaviour.ChaseSightLost(true, true)); }
        [Test] public void Visible_and_sighted_keeps_chase() { Assert.IsFalse(EnemyBehaviour.ChaseSightLost(true, false)); }
        [Test] public void Unsighted_is_sight_lost() { Assert.IsTrue(EnemyBehaviour.ChaseSightLost(false, false)); }

        [Test]
        public void Chase_exits_after_grace_or_escape()
        {
            Assert.IsFalse(EnemyBehaviour.ShouldExitChase(false, 4.9f, 5f));
            Assert.IsTrue(EnemyBehaviour.ShouldExitChase(false, 5.1f, 5f));
            Assert.IsTrue(EnemyBehaviour.ShouldExitChase(true, 0f, 5f));
        }

        [Test]
        public void Extraction_only_for_witnessed_hider_at_the_occupied_stop()
        {
            Assert.IsTrue(EnemyBehaviour.ShouldExtractHider(0, true, true, true));
            Assert.IsFalse(EnemyBehaviour.ShouldExtractHider(0, true, true, false), "unwitnessed hider stays safe");
            Assert.IsFalse(EnemyBehaviour.ShouldExtractHider(1, true, true, true), "only the first stop is her spot");
            Assert.IsFalse(EnemyBehaviour.ShouldExtractHider(0, false, true, true));
            Assert.IsFalse(EnemyBehaviour.ShouldExtractHider(0, true, false, true));
        }

        [Test]
        public void SightRange_configWins_but_falls_back()
        {
            Assert.AreEqual(18f, EnemyBehaviour.ResolveSightRange(true, 18f, 15f));
            Assert.AreEqual(15f, EnemyBehaviour.ResolveSightRange(false, 18f, 15f));
            Assert.AreEqual(15f, EnemyBehaviour.ResolveSightRange(true, 0f, 15f));
        }

        [Test]
        public void Breathing_policy_unchanged()
        {
            Assert.IsTrue(EnemyBehaviour.BreathingFor(EnemyBehaviour.State.Search));
            Assert.IsTrue(EnemyBehaviour.BreathingFor(EnemyBehaviour.State.Stalk));
            Assert.IsTrue(EnemyBehaviour.BreathingFor(EnemyBehaviour.State.Investigate));
            Assert.IsFalse(EnemyBehaviour.BreathingFor(EnemyBehaviour.State.Chase));
        }

        [Test]
        public void LitZones_protectBeforeDawn_butNotDuringTheDawnChase()
        {
            Assert.IsTrue(EnemyBehaviour.ClampsToLitZones(false));
            Assert.IsFalse(EnemyBehaviour.ClampsToLitZones(true));
            Assert.IsTrue(EnemyBehaviour.LitZoneProtects(false, true));
            Assert.IsFalse(EnemyBehaviour.LitZoneProtects(false, false));
            Assert.IsFalse(EnemyBehaviour.LitZoneProtects(true, true), "dawn chase catches inside lit rooms, entrance included");
        }

        [Test]
        public void ForceCooldown_allowedBeforeDawn_blockedDuringDawnChase()
        {
            Assert.IsTrue(EnemyBehaviour.CanForceCooldown(false));
            Assert.IsFalse(EnemyBehaviour.CanForceCooldown(true));
        }
    }
}
