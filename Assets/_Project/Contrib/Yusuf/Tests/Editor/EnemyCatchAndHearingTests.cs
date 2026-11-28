using System.Reflection;
using LightShadow.Core;
using LightShadow.Enemy;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Contrib.Yusuf.Tests.Editor
{
    // Covers EnemyBehaviour seams the existing suites do not: how a catch plan is
    // applied (ApplyCatchPlan), the noise-hearing gate (HearNoiseAt, OnNoiseEmitted,
    // FreshNoiseHeard) and the Investigate timer reset.
    //
    // Existing coverage NOT repeated: CatchRecoveryTests (the pure plan values),
    // EnemyHiderChaseTests (static seams), EnemyDawnChasePolicyTests (EngageRadius).
    //
    // CHARACTERIZATION NOTE: DawnSurvivedStrike_* pins what the code does today. In the
    // dawn chase a survived strike's retreat request is dropped by CanForceCooldown, so
    // the wraith keeps its state. If the production hunk proposed in the audit report is
    // applied, that test is expected to fail and should be flipped to assert the
    // withdrawal. It is not written to hide a defect.
    //
    // UNRUN: written by source inspection at PR130 head 2d3632f; not compiled or run in a
    // Unity editor by the author. No Assert.Inconclusive or Ignore.
    //
    // FIXTURE ISOLATION: the EnemyBehaviour host is created inactive before AddComponent,
    // so Awake/OnEnable/Start never run (no GameEvents subscription, no GameManager
    // lookup, no NavMesh query). Only paths that never touch the NavMeshAgent are driven:
    // ApplyCatchPlan with the dawn gate closed, and the hearing methods. State and timers
    // are set by reflection on this fixture's own instance. Nothing static is touched.
    public class EnemyCatchAndHearingTests
    {
        const BindingFlags NonPublic = BindingFlags.NonPublic | BindingFlags.Instance;

        GameObject host;
        GameObject playerHost;
        EnemyBehaviour enemy;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("EnemyCatchAndHearingTestsEnemy");
            host.SetActive(false);
            enemy = host.AddComponent<EnemyBehaviour>();
            playerHost = new GameObject("EnemyCatchAndHearingTestsPlayer");
            playerHost.SetActive(false);
            Assert.IsFalse(host.activeSelf, "fixture host must stay inactive so no lifecycle method runs");
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null) Object.DestroyImmediate(host);
            if (playerHost != null) Object.DestroyImmediate(playerHost);
        }

        static FieldInfo Field(string name)
        {
            FieldInfo info = typeof(EnemyBehaviour).GetField(name, NonPublic);
            Assert.NotNull(info, name + " missing or renamed");
            return info;
        }

        bool Heard()
        {
            MethodInfo info = typeof(EnemyBehaviour).GetMethod("FreshNoiseHeard", NonPublic);
            Assert.NotNull(info, "FreshNoiseHeard missing or renamed");
            return (bool)info.Invoke(enemy, null);
        }

        void ApplyPlan(CatchRecovery.Plan plan)
        {
            MethodInfo info = typeof(EnemyBehaviour).GetMethod("ApplyCatchPlan", NonPublic);
            Assert.NotNull(info, "ApplyCatchPlan missing or renamed");
            info.Invoke(enemy, new object[] { plan });
        }

        // ---- catch plan vs the dawn gate ----

        [Test]
        public void DawnSurvivedStrike_RetreatRequest_IsRefusedByTheForceCooldownGate()
        {
            CatchRecovery.Plan plan = CatchRecovery.AfterStrike(true);
            Assert.IsTrue(plan.Retreat, "a survived strike asks the wraith to withdraw");
            Assert.IsFalse(EnemyBehaviour.CanForceCooldown(true), "but the dawn chase refuses any forced cool-down");
            Assert.IsFalse(plan.ResetToPatrol, "and the plan has no fallback, so nothing else moves the wraith");
        }

        [Test]
        public void DawnSurvivedStrike_KeepsTheChaseStateAndOnlyRearms()
        {
            enemy.SetAggressive(true);
            Field("catching").SetValue(enemy, true);
            Field("state").SetValue(enemy, EnemyBehaviour.State.Chase);

            ApplyPlan(CatchRecovery.AfterStrike(true));

            Assert.IsFalse((bool)Field("catching").GetValue(enemy), "the freeze flag is cleared");
            Assert.AreEqual(EnemyBehaviour.State.Chase, enemy.CurrentState,
                "characterization: no withdrawal happens in the dawn chase");
            Assert.AreEqual(CatchRecovery.RearmSeconds, (float)Field("catchCooldownTimer").GetValue(enemy),
                "only the rearm timer protects Nour from the next strike");
        }

        [Test]
        public void SurvivedStrike_NeverShortensAnExistingCatchCooldown()
        {
            enemy.SetAggressive(true);
            Field("catching").SetValue(enemy, true);
            Field("catchCooldownTimer").SetValue(enemy, 20f);

            ApplyPlan(CatchRecovery.AfterStrike(true));

            Assert.AreEqual(20f, (float)Field("catchCooldownTimer").GetValue(enemy));
        }

        // ---- hearing gate ----

        [Test]
        public void NoNoiseYet_IsNotHeard()
        {
            Assert.IsFalse(Heard());
        }

        [Test]
        public void HearNoiseAt_InsideRadius_IsHeard_AndRadiusIsInclusive()
        {
            enemy.transform.position = Vector3.zero;
            enemy.HearNoiseAt(new Vector3(3f, 0f, 0f), 4f);
            Assert.IsTrue(Heard());
            enemy.HearNoiseAt(new Vector3(4f, 0f, 0f), 4f);
            Assert.IsTrue(Heard(), "standing exactly on the radius still hears it");
        }

        [Test]
        public void HearNoiseAt_OutsideRadius_IsNotHeard()
        {
            enemy.transform.position = Vector3.zero;
            enemy.HearNoiseAt(new Vector3(4.5f, 0f, 0f), 4f);
            Assert.IsFalse(Heard());
        }

        [Test]
        public void StaleNoise_IsNotHeard_EvenInsideRadius()
        {
            enemy.transform.position = Vector3.zero;
            enemy.HearNoiseAt(new Vector3(1f, 0f, 0f), 4f);
            Assert.IsTrue(Heard());
            // Age the noise past investigateNoiseMaxAge (default 3 s, no config here).
            Field("lastNoiseTime").SetValue(enemy, Time.time - 3.5f);
            Assert.IsFalse(Heard());
        }

        [Test]
        public void PlayerNoise_IsLocatedAtThePlayer_NotAtTheEnemy()
        {
            enemy.transform.position = Vector3.zero;
            enemy.player = playerHost.transform;
            playerHost.transform.position = new Vector3(10f, 0f, 0f);

            MethodInfo emit = typeof(EnemyBehaviour).GetMethod("OnNoiseEmitted", NonPublic);
            Assert.NotNull(emit, "OnNoiseEmitted missing or renamed");

            emit.Invoke(enemy, new object[] { 4f });
            Assert.IsFalse(Heard(), "a footstep 10 m away does not carry 4 m");
            emit.Invoke(enemy, new object[] { 12f });
            Assert.IsTrue(Heard(), "a sprint-loud noise at 10 m does");
        }

        [Test]
        public void NewNoise_WhileInvestigating_RestartsTheLookAroundTimer()
        {
            Field("state").SetValue(enemy, EnemyBehaviour.State.Investigate);
            Field("investigateTimer").SetValue(enemy, 5f);

            enemy.HearNoiseAt(Vector3.one, 4f);

            Assert.AreEqual(0f, (float)Field("investigateTimer").GetValue(enemy),
                "every fresh noise restarts the 8 s sweep, so steady noise keeps it investigating");
        }

        [Test]
        public void NewNoise_InOtherStates_LeavesTheInvestigateTimerAlone()
        {
            Field("state").SetValue(enemy, EnemyBehaviour.State.Patrol);
            Field("investigateTimer").SetValue(enemy, 5f);

            enemy.HearNoiseAt(Vector3.one, 4f);

            Assert.AreEqual(5f, (float)Field("investigateTimer").GetValue(enemy));
        }
    }
}
