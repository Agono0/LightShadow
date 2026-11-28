using System;
using LightShadow.Audio;
using LightShadow.Enemy;
using NUnit.Framework;

namespace LightShadow.Contrib.Ahmed.Tests.Editor
{
    // Pins the enum-mirror contract behind AudioDirector.Sample
    // (AudioDirector.cs: `State = (DangerState)(int)enemy.CurrentState`):
    // DangerState is a hand-maintained parallel copy of EnemyBehaviour.State,
    // bridged by a raw int cast. Today the ordinals match; a reorder or
    // insert in either enum would silently re-weight every state's danger
    // with no compile error. These tests make that drift loud instead:
    // full explicit member sets and counts on BOTH enums, and per-name
    // ordinal equality in BOTH directions. A drift fails a specific assert
    // and forces a conscious decision - no auto-pass tautology.
    //
    // Scope: the mirror contract only. Runtime audio behaviour is out of
    // scope. UNRUN: never compiled or run here; static expected-pass is
    // not a pass.
    public class EnemyDangerStateContractTests
    {
        static readonly string[] ExpectedStateNames =
        {
            "Patrol", "Stalk", "Chase", "Repelled",
            "Investigate", "ForcedCooldown", "Search", "Ambush",
        };

        [Test]
        public void EnemyState_HasExpectedFullMemberSet()
        {
            CollectionAssert.AreEquivalent(ExpectedStateNames,
                Enum.GetNames(typeof(EnemyBehaviour.State)),
                "EnemyBehaviour.State membership changed; review the DangerState mirror");
        }

        [Test]
        public void DangerState_HasExpectedFullMemberSet()
        {
            CollectionAssert.AreEquivalent(ExpectedStateNames,
                Enum.GetNames(typeof(DangerState)),
                "DangerState membership changed; review the EnemyBehaviour.State mirror");
        }

        [Test]
        public void EveryStateName_ParsesToDangerState_WithSameOrdinal()
        {
            foreach (string name in Enum.GetNames(typeof(EnemyBehaviour.State)))
            {
                object parsed = Enum.Parse(typeof(DangerState), name); // throws on drift: loud
                int stateOrdinal = (int)Enum.Parse(typeof(EnemyBehaviour.State), name);
                Assert.AreEqual(stateOrdinal, (int)parsed,
                    "ordinal drift on '" + name + "': the int cast in AudioDirector.Sample would mis-weight this state");
            }
        }

        [Test]
        public void EveryDangerStateName_ParsesToState_WithSameOrdinal()
        {
            foreach (string name in Enum.GetNames(typeof(DangerState)))
            {
                object parsed = Enum.Parse(typeof(EnemyBehaviour.State), name); // throws on drift: loud
                int dangerOrdinal = (int)Enum.Parse(typeof(DangerState), name);
                Assert.AreEqual(dangerOrdinal, (int)parsed,
                    "ordinal drift on '" + name + "': the int cast in AudioDirector.Sample would mis-weight this state");
            }
        }
    }
}
