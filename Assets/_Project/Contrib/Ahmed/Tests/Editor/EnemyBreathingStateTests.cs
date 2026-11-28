using System;
using System.Collections.Generic;
using LightShadow.Enemy;
using NUnit.Framework;

namespace LightShadow.Contrib.Ahmed.Tests.Editor
{
    // Pins the central breathing policy seam: EnemyBehaviour.BreathingFor must
    // return true ONLY for Stalk, Investigate and Search, and false for every
    // other state (policy confirmed by the peer on 2026-11-28). Before the
    // seam, EnterState silenced the loop only when leaving Stalk, so the stalk
    // breath leaked out of Investigate/Search into Chase and Patrol.
    //
    // The whole enum is enumerated and compared against an EXPLICIT expected
    // set: a new State member fails the count guard and lands in neither set,
    // forcing a conscious policy decision instead of an auto-false tautology.
    //
    // Scope: the pure policy function only. These tests do NOT verify the
    // EnterState wiring (one line) or any runtime audio, and Ambush's
    // separate lunge-burst SetBreathing(true) is out of scope by policy.
    //
    // UNRUN: never compiled or run here; static expected-pass is not a pass.
    public class EnemyBreathingStateTests
    {
        static readonly EnemyBehaviour.State[] ExpectedBreathingStates =
        {
            EnemyBehaviour.State.Stalk,
            EnemyBehaviour.State.Investigate,
            EnemyBehaviour.State.Search,
        };

        [Test]
        public void BreathingFor_TrueExactlyFor_StalkInvestigateSearch()
        {
            var actual = new List<EnemyBehaviour.State>();
            foreach (EnemyBehaviour.State state in Enum.GetValues(typeof(EnemyBehaviour.State)))
            {
                if (EnemyBehaviour.BreathingFor(state)) actual.Add(state);
            }

            CollectionAssert.AreEquivalent(ExpectedBreathingStates, actual,
                "the breathing set must be exactly Stalk, Investigate and Search");
        }

        [Test]
        public void BreathingFor_StateEnumStillHasEightMembers()
        {
            // The eight states at the time of writing: Patrol, Stalk, Chase,
            // Repelled, Investigate, ForcedCooldown, Search, Ambush. A new
            // member must force a policy decision, not slide past the tests.
            Assert.AreEqual(8, Enum.GetValues(typeof(EnemyBehaviour.State)).Length,
                "EnemyBehaviour.State changed size; decide BreathingFor for the new member(s)");
        }

        [TestCase(EnemyBehaviour.State.Patrol, false)]
        [TestCase(EnemyBehaviour.State.Stalk, true)]
        [TestCase(EnemyBehaviour.State.Chase, false)]
        [TestCase(EnemyBehaviour.State.Repelled, false)]
        [TestCase(EnemyBehaviour.State.Investigate, true)]
        [TestCase(EnemyBehaviour.State.ForcedCooldown, false)]
        [TestCase(EnemyBehaviour.State.Search, true)]
        [TestCase(EnemyBehaviour.State.Ambush, false)]
        public void BreathingFor_PerStateMatchesPolicy(EnemyBehaviour.State state, bool expected)
        {
            Assert.AreEqual(expected, EnemyBehaviour.BreathingFor(state), state.ToString());
        }
    }
}
