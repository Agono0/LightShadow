using System.Collections.Generic;
using NUnit.Framework;

namespace LightShadow.Contrib.Usef.LevelLogic.Tests
{
    // Tests the validator on small synthetic graphs. GAP COVERED: there is no check at
    // all that a key/puzzle chain can be finished. LIMIT: this proves the validator's
    // logic, nothing about the real scene.
    public class LevelGraphValidatorTests
    {
        static LevelStep S(string id, string[] all, string[] any, string[] grants)
        {
            return new LevelStep { Id = id, RequiresAll = all, RequiresAny = any, Grants = grants };
        }

        static readonly string[] None = new string[0];

        static LevelGraph G(string goal, params LevelStep[] steps)
        {
            var g = new LevelGraph { GoalToken = goal };
            g.Steps.AddRange(steps);
            return g;
        }

        [Test]
        public void Chain_IsReachable_AndWitnessOrderRespectsDependencies()
        {
            LevelGraph g = G("goal",
                S("c", new[] { "b" }, None, new[] { "goal" }),
                S("b", new[] { "a" }, None, new[] { "b" }),
                S("a", None, None, new[] { "a" }));
            LevelGraphReport r = LevelGraphValidator.Validate(g);
            Assert.IsTrue(r.Ok, string.Join("; ", r.Errors));
            Assert.AreEqual(new[] { "a", "b", "c" }, r.WitnessOrder.ToArray());
        }

        [Test]
        public void Cycle_IsReportedAsBlockedAndGoalUnreachable()
        {
            LevelGraph g = G("goal",
                S("a", new[] { "tb" }, None, new[] { "ta" }),
                S("b", new[] { "ta" }, None, new[] { "tb", "goal" }));
            LevelGraphReport r = LevelGraphValidator.Validate(g);
            Assert.IsFalse(r.GoalReachable);
            Assert.IsFalse(r.Ok);
            Assert.IsTrue(r.Errors.Exists(e => e.Contains("can never run")));
        }

        [Test]
        public void DanglingRequirement_IsAnError()
        {
            LevelGraph g = G("goal", S("a", new[] { "nobody_grants_this" }, None, new[] { "goal" }));
            LevelGraphReport r = LevelGraphValidator.Validate(g);
            Assert.IsTrue(r.Errors.Exists(e => e.Contains("never granted")));
            Assert.IsFalse(r.Ok);
        }

        [Test]
        public void InitialToken_SatisfiesRequirement()
        {
            LevelGraph g = G("goal", S("a", new[] { "start" }, None, new[] { "goal" }));
            g.InitialTokens.Add("start");
            Assert.IsTrue(LevelGraphValidator.Validate(g).Ok);
        }

        [Test]
        public void RequiresAny_NeedsOnlyOneOfTheTokens()
        {
            LevelGraph g = G("goal",
                S("x", None, None, new[] { "x" }),
                S("end", None, new[] { "x", "y" }, new[] { "goal" }));
            LevelGraphReport r = LevelGraphValidator.Validate(g);
            Assert.IsTrue(r.GoalReachable);
            Assert.IsTrue(r.Errors.Exists(e => e.Contains("y")), "y is never granted, still an error");
        }

        [Test]
        public void DuplicateAndEmptyIds_AreErrors()
        {
            LevelGraph g = G("goal",
                S("a", None, None, new[] { "goal" }),
                S("a", None, None, new[] { "other" }),
                S("", None, None, new[] { "more" }));
            LevelGraphReport r = LevelGraphValidator.Validate(g);
            Assert.IsTrue(r.Errors.Exists(e => e.Contains("duplicate step id")));
            Assert.IsTrue(r.Errors.Exists(e => e.Contains("empty id")));
        }

        [Test]
        public void MissingGoal_IsAnError()
        {
            Assert.IsTrue(LevelGraphValidator.Validate(G("", S("a", None, None, new[] { "t" }))).Errors.Exists(e => e.Contains("no goal")));
            Assert.IsTrue(LevelGraphValidator.Validate(G("goal", S("a", None, None, new[] { "t" }))).Errors.Exists(e => e.Contains("nothing grants the goal")));
        }

        [Test]
        public void NullGraph_DoesNotThrow()
        {
            LevelGraphReport r = null;
            Assert.DoesNotThrow(() => r = LevelGraphValidator.Validate(null));
            Assert.IsFalse(r.Ok);
        }

        [Test]
        public void CriticalSteps_AreTheSinglePointsOfFailure()
        {
            // two ways to get "k", only one way to get the goal.
            LevelGraph g = G("goal",
                S("k1", None, None, new[] { "k" }),
                S("k2", None, None, new[] { "k" }),
                S("end", new[] { "k" }, None, new[] { "goal" }));
            List<string> critical = LevelGraphValidator.CriticalSteps(g);
            CollectionAssert.AreEquivalent(new[] { "end" }, critical);
        }

        [Test]
        public void StepThatGrantsNothing_IsAWarning_NotAnError()
        {
            LevelGraph g = G("goal",
                S("a", None, None, new[] { "goal" }),
                S("empty", None, None, None));
            LevelGraphReport r = LevelGraphValidator.Validate(g);
            Assert.IsTrue(r.Warnings.Exists(w => w.Contains("grants nothing")));
            Assert.IsTrue(r.Ok);
        }
    }
}
