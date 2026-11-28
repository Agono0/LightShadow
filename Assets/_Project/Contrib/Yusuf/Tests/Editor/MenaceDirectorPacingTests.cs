using System.Reflection;
using LightShadow.Core;
using LightShadow.Enemy;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Contrib.Yusuf.Tests.Editor
{
    // Pins the MenaceDirector pacing rules (Scripts/Enemy/MenaceDirector.cs) that no
    // existing suite touches: the post-chase grace window, the gauge reset after a
    // forced cool-down, and the per-state rise table in ComputeRise.
    //
    // Scope: policy at the event seam (OnEnemyStateChanged) and the pure rise table.
    // The FSM, NavMesh and line-of-sight are not exercised.
    // UNRUN: written by source inspection at PR130 head 2d3632f; not compiled or run in a
    // Unity editor by the author.
    //
    // FIXTURE ISOLATION:
    //  * Every GameObject is created inactive, so the editor dispatches no
    //    Awake/OnEnable/Start. The only lifecycle method called is MenaceDirector.Awake,
    //    by reflection, to set the Instance singleton that ChaseBlocked reads.
    //  * TearDown calls OnDestroy by reflection (clears Instance) and DestroyImmediates
    //    everything created here. OnEnable is never called, so GameEvents stays untouched.
    //  * Events are fed straight into the private OnEnemyStateChanged, not through the
    //    static GameEvents bus.
    public class MenaceDirectorPacingTests
    {
        const BindingFlags NonPublic = BindingFlags.NonPublic | BindingFlags.Instance;

        GameObject directorHost;
        GameObject enemyHost;
        GameObject playerHost;
        MenaceDirector director;
        EnemyBehaviour enemy;
        GameConfig config;

        [SetUp]
        public void SetUp()
        {
            Assert.IsNull(MenaceDirector.Instance, "another MenaceDirector is alive; the singleton must be free for this fixture");

            config = ScriptableObject.CreateInstance<GameConfig>();
            directorHost = new GameObject("MenaceDirectorPacingTestsDirector");
            directorHost.SetActive(false);
            director = directorHost.AddComponent<MenaceDirector>();
            director.config = config;
            Call(director, "Awake");
            Assert.AreSame(director, MenaceDirector.Instance);

            enemyHost = new GameObject("MenaceDirectorPacingTestsEnemy");
            enemyHost.SetActive(false);
            enemy = enemyHost.AddComponent<EnemyBehaviour>();
            playerHost = new GameObject("MenaceDirectorPacingTestsPlayer");
            playerHost.SetActive(false);
            director.enemy = enemy;
            director.player = playerHost.transform;
        }

        [TearDown]
        public void TearDown()
        {
            if (director != null) Call(director, "OnDestroy");
            if (directorHost != null) Object.DestroyImmediate(directorHost);
            if (enemyHost != null) Object.DestroyImmediate(enemyHost);
            if (playerHost != null) Object.DestroyImmediate(playerHost);
            if (config != null) Object.DestroyImmediate(config);
            Assert.IsNull(MenaceDirector.Instance, "fixture must leave the singleton cleared");
        }

        static void Call(object target, string method, params object[] args)
        {
            MethodInfo info = target.GetType().GetMethod(method, NonPublic);
            Assert.NotNull(info, method + " missing or renamed");
            info.Invoke(target, args);
        }

        static void SetField(object target, string field, object value)
        {
            FieldInfo info = target.GetType().GetField(field, NonPublic);
            Assert.NotNull(info, field + " missing or renamed");
            info.SetValue(target, value);
        }

        static float GetFloat(object target, string field)
        {
            FieldInfo info = target.GetType().GetField(field, NonPublic);
            Assert.NotNull(info, field + " missing or renamed");
            return (float)info.GetValue(target);
        }

        void Raise(EnemyBehaviour.State state)
        {
            Call(director, "OnEnemyStateChanged", (int)state);
        }

        float Rise()
        {
            MethodInfo info = typeof(MenaceDirector).GetMethod("ComputeRise", NonPublic);
            Assert.NotNull(info, "ComputeRise missing or renamed");
            return (float)info.Invoke(director, null);
        }

        // ---- grace window ----

        [Test]
        public void ChaseBlocked_IsFalse_UntilAChaseEnds()
        {
            Assert.IsFalse(MenaceDirector.ChaseBlocked);
            Raise(EnemyBehaviour.State.Stalk);
            Raise(EnemyBehaviour.State.Patrol);
            Assert.IsFalse(MenaceDirector.ChaseBlocked, "ending a non-chase state must not arm the grace");
        }

        [Test]
        public void LeavingChase_ArmsGraceWithConfiguredSeconds()
        {
            config.chaseGraceSeconds = 12f;
            Raise(EnemyBehaviour.State.Chase);
            Assert.IsFalse(MenaceDirector.ChaseBlocked, "entering a chase does not block the next one");
            Raise(EnemyBehaviour.State.Search);
            Assert.IsTrue(MenaceDirector.ChaseBlocked);
            Assert.AreEqual(12f, GetFloat(director, "chaseGraceTimer"));
        }

        [Test]
        public void LeavingChaseIntoForcedCooldown_AlsoArmsGrace()
        {
            Raise(EnemyBehaviour.State.Chase);
            Raise(EnemyBehaviour.State.ForcedCooldown);
            Assert.IsTrue(MenaceDirector.ChaseBlocked, "any exit from Chase arms the grace, including the forced cool-down");
        }

        // ---- gauge reset ----

        [Test]
        public void LeavingForcedCooldown_ResetsGaugeToConfiguredValue()
        {
            config.menaceResetValue = 25f;
            Assert.AreEqual(0f, director.Gauge);
            Raise(EnemyBehaviour.State.ForcedCooldown);
            Assert.AreEqual(0f, director.Gauge, "entering the cool-down does not touch the gauge");
            Raise(EnemyBehaviour.State.Patrol);
            Assert.AreEqual(25f, director.Gauge);
        }

        [Test]
        public void OtherTransitions_DoNotResetGauge()
        {
            config.menaceResetValue = 25f;
            Raise(EnemyBehaviour.State.Stalk);
            Raise(EnemyBehaviour.State.Investigate);
            Raise(EnemyBehaviour.State.Patrol);
            Assert.AreEqual(0f, director.Gauge);
        }

        // ---- rise table ----

        [Test]
        public void Rise_FarAndUnseen_DecaysAtConfiguredRate()
        {
            config.menaceDecayPerSecond = 2.5f;
            playerHost.transform.position = new Vector3(config.menaceNearDistance + 5f, 0f, 0f);
            Assert.AreEqual(-2.5f, Rise());
        }

        [Test]
        public void Rise_NearButUnseen_UsesNearRate()
        {
            config.menaceNearRisePerSecond = 3f;
            playerHost.transform.position = new Vector3(config.menaceNearDistance - 1f, 0f, 0f);
            Assert.AreEqual(3f, Rise(), "enemy.sight is null in this fixture, so the player counts as unseen");
        }

        [Test]
        public void Rise_NearBoundary_IsInclusive()
        {
            config.menaceNearRisePerSecond = 3f;
            playerHost.transform.position = new Vector3(config.menaceNearDistance, 0f, 0f);
            Assert.AreEqual(3f, Rise());
        }

        [Test]
        public void Rise_ChaseState_WinsOverDistance()
        {
            config.menaceChaseRisePerSecond = 7f;
            SetField(enemy, "state", EnemyBehaviour.State.Chase);
            playerHost.transform.position = new Vector3(100f, 0f, 0f);
            Assert.AreEqual(7f, Rise(), "a chase keeps the gauge climbing however far apart they are");
        }

        [Test]
        public void Rise_DawnMultiplier_AppliesToRiseAndDecayAlike()
        {
            config.dawnMenaceMultiplier = 2f;
            config.menaceNearRisePerSecond = 3f;
            config.menaceDecayPerSecond = 1f;
            enemy.config = config;
            enemy.SetAggressive(true);

            playerHost.transform.position = new Vector3(1f, 0f, 0f);
            Assert.AreEqual(6f, Rise());
            playerHost.transform.position = new Vector3(100f, 0f, 0f);
            Assert.AreEqual(-2f, Rise(), "the dawn multiplier also doubles the decay, so the gauge falls faster at a distance too");
        }
    }
}
