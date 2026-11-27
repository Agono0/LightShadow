using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Contrib.Usef.LevelLogic.Tests
{
    // Runs the validator on the hand-written model of 02_School. LIMIT: "completable" here
    // means completable in the MODEL, which was read from code by hand. It does not prove
    // the scene is reachable, that a key is not behind a wall, or that anything works at runtime.
    public class SchoolLevelModelTests
    {
        static readonly string[] ExpectedPuzzles =
        {
            "Puzzle_ChalkboardCipher", "Puzzle_CatalogueSort", "Puzzle_OpticsBench",
            "Puzzle_StageLights", "Puzzle_FirstAidOrder", "Puzzle_MapLocker", "Puzzle_GateCircuit",
        };

        [Test]
        public void Model_IsValid_AndGoalIsReachable()
        {
            LevelGraphReport r = LevelGraphValidator.Validate(SchoolLevelModel.Build());
            Assert.IsEmpty(r.Errors, string.Join("; ", r.Errors));
            Assert.IsTrue(r.GoalReachable);
        }

        [Test]
        public void Model_HasAllSevenWingPuzzles_AndUniqueIds()
        {
            LevelGraph g = SchoolLevelModel.Build();
            var ids = new HashSet<string>();
            foreach (LevelStep s in g.Steps) Assert.IsTrue(ids.Add(s.Id), "duplicate " + s.Id);
            foreach (string p in ExpectedPuzzles) Assert.IsTrue(ids.Contains(p), "missing " + p);
        }

        [Test]
        public void Model_EveryStepStatesItsSource_AndInferredStepsExplainWhy()
        {
            foreach (LevelStep s in SchoolLevelModel.Build().Steps)
            {
                Assert.IsNotEmpty(s.Source, s.Id + " has no source");
                if (s.Confidence == Confidence.Inferred) Assert.IsNotEmpty(s.Note, s.Id + " is inferred but has no note");
            }
        }

        [Test]
        public void Witness_StaffKeyChain_RunsInOrder()
        {
            List<string> order = LevelGraphValidator.Validate(SchoolLevelModel.Build()).WitnessOrder;
            int Idx(string id) { return order.IndexOf(id); }
            Assert.Less(Idx("Key_Staff"), Idx("Door_Staff"));
            Assert.Less(Idx("Door_Staff"), Idx("Puzzle_MapLocker"));
            Assert.Less(Idx("Puzzle_MapLocker"), Idx("Puzzle_GateCircuit"));
            Assert.Less(Idx("Puzzle_GateCircuit"), Idx("ExitGate"));
            Assert.Less(Idx("Fuse_MainHall"), Idx("Puzzle_GateCircuit"));
        }

        [Test]
        public void CriticalSteps_MatchTheCurrentDesign()
        {
            List<string> critical = LevelGraphValidator.CriticalSteps(SchoolLevelModel.Build());
            string[] mustBe =
            {
                "Key_Staff", "Door_Staff", "Puzzle_MapLocker", "Fuse_Library", "Fuse_Physics",
                "Fuse_MainHall", "Puzzle_GateCircuit", "ExitGate",
            };
            foreach (string id in mustBe) CollectionAssert.Contains(critical, id);
            // Optional today: nothing on the route to the gate needs these.
            string[] optional = { "Key_Classroom", "Door_Classroom", "Puzzle_ChalkboardCipher",
                "Puzzle_CatalogueSort", "Puzzle_OpticsBench", "Puzzle_StageLights", "Puzzle_FirstAidOrder" };
            foreach (string id in optional) CollectionAssert.DoesNotContain(critical, id);
        }

        // Soft drift guard. If the builders rename a puzzle or key, the model is stale. This
        // only warns (a log warning does not fail the run) so it never blocks the owners' PRs.
        [Test]
        public void ModelNames_StillAppearInBuilderSource_LogsDrift()
        {
            string dir = Path.Combine(Application.dataPath, "_Project/Scripts/Tools/Editor");
            string puzzles = Path.Combine(dir, "SceneBuilderPuzzles.cs");
            string doors = Path.Combine(dir, "SceneBuilderDoors.cs");
            if (!File.Exists(puzzles) || !File.Exists(doors))
            {
                Assert.Inconclusive("builder sources not found next to the project");
                return;
            }
            string text = File.ReadAllText(puzzles) + File.ReadAllText(doors);
            foreach (LevelStep s in SchoolLevelModel.Build().Steps)
            {
                if (s.Kind == StepKind.Puzzle && s.Id != "Puzzle_GateCircuit" || s.Kind == StepKind.Pickup || s.Kind == StepKind.Door)
                {
                    if (!text.Contains("\"" + s.Id + "\""))
                        Debug.LogWarning("LevelLogic model drift: '" + s.Id + "' not found in the scene builders; update SchoolLevelModel.");
                }
            }
            foreach (string item in new[] { "classroom_key", "staff_key", "master_key", "fuse" })
            {
                if (!text.Contains("\"" + item + "\""))
                    Debug.LogWarning("LevelLogic model drift: item id '" + item + "' not found in the scene builders.");
            }
        }
    }
}
