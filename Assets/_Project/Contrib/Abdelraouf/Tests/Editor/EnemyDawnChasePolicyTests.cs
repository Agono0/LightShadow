using NUnit.Framework;
using UnityEngine;
using LightShadow.Core;
using LightShadow.Enemy;

namespace LightShadow.Contrib.Abdelraouf.Tests.Editor
{
    // Regression coverage for the E2 dawn-chase policy in
    // Scripts/Enemy/EnemyBehaviour.cs (PR129, commit 23c21a3, tip 7400f4d):
    // once the gate is powered (SetAggressive(true)) the Shadow stalks across
    // lit ground - the entrance included - and engages from the config's
    // dawnStalkRadius; before dawn the lit rooms stay safe and the normal
    // stalk radius applies.
    //
    // Distinct from Tests/Editor/EnemyHiderChaseTests.cs, which covers
    // ChaseSightLost, ShouldExitChase, ShouldExtractHider, ResolveSightRange,
    // BreathingFor, ClampsToLitZones (both values), LitZoneProtects for
    // (false,true)/(false,false)/(true,true), and CanForceCooldown (both
    // values, added with N1). None of those touch the seams here:
    //  * EngageRadius: dawnStalkRadius when aggressive with a config, the
    //    stalkRadius fallback when aggressive without one, and never
    //    dawnStalkRadius before dawn.
    //  * SetAggressive / IsAggressive: the gate-powered toggle itself.
    //  * LitZoneProtects(true, false): the one combination of the 2x2
    //    matrix the existing suite does not assert (dawn chase, player
    //    outside a lit zone: still no protection).
    //
    // NOT covered here (editor limits, stated honestly): the pre-dawn engage
    // gate at Update() (`playerIsDark || aggressive`) is inline FSM logic
    // that needs a NavMesh and a player; the destination clamp in
    // SetDestination needs a NavMeshAgent on a real NavMesh. Neither is
    // faked.
    //
    // EXECUTION STATUS: written by inspection against the sources at PR129
    // tip 7400f4d27dc8321f68f669c3542c66bf5695a749; NOT compiled or executed
    // by the author (no Unity editor available where these were written).
    // Static expected-pass is not a pass. No Assert.Inconclusive anywhere.
    //
    // FIXTURE SAFETY (verified against the source at the tip above):
    //  * The host GameObject is created INACTIVE before AddComponent, the
    //    same convention as Tests/Editor/SaveSlotConfirmTests.cs, so the
    //    editor never dispatches Awake, OnEnable, OnDisable or Start. That
    //    matters: OnEnable subscribes GameEvents.NoiseEmitted and Start
    //    resolves GameManager references; on an inactive host none of that
    //    runs and no static event is touched.
    //  * [RequireComponent(typeof(NavMeshAgent))] adds an inert NavMeshAgent
    //    at AddComponent time. Nothing here ever calls SetDestination, so no
    //    NavMesh query happens.
    //  * SetAggressive is a pure field setter (verified: it only assigns
    //    `aggressive = value`), and EngageRadius/StalkRadius are pure
    //    getters over the config fields.
    //  * GameConfig has no Awake/OnEnable/OnValidate (verified), so
    //    ScriptableObject.CreateInstance is a plain allocation.
    //  * EnemyBehaviour has no singletons or statics; TearDown
    //    DestroyImmediates only the objects this fixture created.
    public class EnemyDawnChasePolicyTests
    {
        static readonly System.Reflection.PropertyInfo EngageRadiusProperty =
            typeof(EnemyBehaviour).GetProperty("EngageRadius",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        GameObject host;
        EnemyBehaviour enemy;
        GameConfig config;

        [SetUp]
        public void SetUp()
        {
            Assert.NotNull(EngageRadiusProperty, "EnemyBehaviour.EngageRadius missing or renamed");

            host = new GameObject("EnemyDawnChasePolicyTestsHost");
            host.SetActive(false); // Awake/OnEnable/Start stay undispatched
            enemy = host.AddComponent<EnemyBehaviour>();
            Assert.IsFalse(host.activeSelf, "fixture host must stay inactive so no lifecycle method runs");
            Assert.IsFalse(enemy.IsAggressive, "a fresh EnemyBehaviour must not start aggressive");

            config = ScriptableObject.CreateInstance<GameConfig>();
            config.stalkRadius = 7f;
            config.dawnStalkRadius = 20f;
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null) Object.DestroyImmediate(host);
            if (config != null) Object.DestroyImmediate(config);
            host = null;
            enemy = null;
            config = null;
        }

        float EngageRadius()
        {
            return (float)EngageRadiusProperty.GetValue(enemy, null);
        }

        [Test]
        public void EngageRadius_AggressiveWithConfig_UsesDawnStalkRadius()
        {
            enemy.config = config;
            enemy.SetAggressive(true);

            Assert.AreEqual(20f, EngageRadius(),
                "the dawn chase must engage from the config's dawnStalkRadius");
        }

        [Test]
        public void EngageRadius_AggressiveWithoutConfig_FallsBackToStalkRadius()
        {
            enemy.config = null;
            enemy.SetAggressive(true);

            Assert.AreEqual(12f, EngageRadius(),
                "without a config the dawn chase must fall back to the hard-coded stalk radius");
        }

        [Test]
        public void EngageRadius_BeforeDawn_NeverUsesDawnStalkRadius()
        {
            enemy.config = config;

            Assert.AreEqual(7f, EngageRadius(),
                "before dawn the normal stalkRadius applies even when a dawnStalkRadius exists");
        }

        [Test]
        public void SetAggressive_Toggles_IsAggressive()
        {
            Assert.IsFalse(enemy.IsAggressive, "default must be calm");

            enemy.SetAggressive(true);
            Assert.IsTrue(enemy.IsAggressive, "powering the gate turns the Shadow aggressive");

            enemy.SetAggressive(false);
            Assert.IsFalse(enemy.IsAggressive, "SetAggressive(false) must turn it back off");
        }

        [Test]
        public void LitZoneProtects_DawnChase_OutsideLitZone_StaysUnprotected()
        {
            Assert.IsFalse(EnemyBehaviour.LitZoneProtects(true, false),
                "outside a lit zone there is nothing to protect with, dawn chase or not");
        }
    }
}
