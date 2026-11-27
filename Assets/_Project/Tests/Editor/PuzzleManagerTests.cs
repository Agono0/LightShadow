using System.Collections.Generic;
using System.Reflection;
using LightShadow.Core;
using LightShadow.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    // GAP COVERED: PuzzleManager (Scripts/Gameplay/Puzzles/PuzzleManager.cs) has no
    // tests. It owns the solved-id set, the puzzleJson save blob and the restore
    // on Register. Same reflection approach as InventoryTests: the capture/apply
    // handlers are private, so they are driven by name.
    //
    // UNVERIFIED LIMITS: EditMode does not run Awake/OnEnable for plain
    // MonoBehaviours, so Awake is invoked by reflection to set Instance and the
    // SaveSystem event subscription is not exercised (handlers are called
    // directly). Not run in an editor by the author; CI test-editmode is the first run.
    public class PuzzleManagerTests
    {
        class TestPuzzle : PuzzleBase
        {
            public int restoredCount;
            protected override void OnInteracted(GameObject interactor) { }
            protected override void OnRestoredSolved() { restoredCount++; }
        }

        static readonly MethodInfo AwakeMethod =
            typeof(PuzzleManager).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo CaptureMethod =
            typeof(PuzzleManager).GetMethod("OnSaveCaptured", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo ApplyMethod =
            typeof(PuzzleManager).GetMethod("OnSaveApplied", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly PropertyInfo InstanceProperty =
            typeof(PuzzleManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);

        readonly List<GameObject> created = new List<GameObject>();
        PuzzleManager manager;

        [SetUp]
        public void SetUp()
        {
            Assert.NotNull(AwakeMethod, "PuzzleManager.Awake missing");
            Assert.NotNull(CaptureMethod, "PuzzleManager.OnSaveCaptured missing");
            Assert.NotNull(ApplyMethod, "PuzzleManager.OnSaveApplied missing");
            var go = new GameObject("PuzzleManagerTests_manager");
            created.Add(go);
            manager = go.AddComponent<PuzzleManager>();
            AwakeMethod.Invoke(manager, null);
        }

        [TearDown]
        public void TearDown()
        {
            InstanceProperty.GetSetMethod(true).Invoke(null, new object[] { null });
            foreach (GameObject go in created) if (go != null) Object.DestroyImmediate(go);
            created.Clear();
        }

        TestPuzzle MakePuzzle(string id)
        {
            var go = new GameObject("PuzzleManagerTests_" + id);
            created.Add(go);
            var puzzle = go.AddComponent<TestPuzzle>();
            puzzle.puzzleId = id;
            return puzzle;
        }

        SaveSystem.SaveData Capture()
        {
            var data = new SaveSystem.SaveData();
            CaptureMethod.Invoke(manager, new object[] { data });
            return data;
        }

        [Test]
        public void Awake_SetsInstance()
        {
            Assert.AreSame(manager, PuzzleManager.Instance);
        }

        [Test]
        public void MarkSolved_IsSolved_IgnoresNullAndEmpty()
        {
            Assert.IsFalse(manager.IsSolved("a"));
            manager.MarkSolved("a");
            manager.MarkSolved(null);
            manager.MarkSolved("");
            Assert.IsTrue(manager.IsSolved("a"));
            Assert.IsFalse(manager.IsSolved(null));
            Assert.IsFalse(manager.IsSolved(""));
        }

        [Test]
        public void Capture_Apply_RoundTripsSolvedIds()
        {
            manager.MarkSolved("classroom_cipher");
            manager.MarkSolved("library_catalogue");
            SaveSystem.SaveData data = Capture();
            StringAssert.Contains("classroom_cipher", data.puzzleJson);

            manager.MarkSolved("not_in_save");
            ApplyMethod.Invoke(manager, new object[] { data });
            Assert.IsTrue(manager.IsSolved("classroom_cipher"));
            Assert.IsTrue(manager.IsSolved("library_catalogue"));
            Assert.IsFalse(manager.IsSolved("not_in_save"), "apply replaces the set, it does not merge");
        }

        [Test]
        public void Apply_NullOrEmptyBlob_ClearsSolved()
        {
            manager.MarkSolved("a");
            ApplyMethod.Invoke(manager, new object[] { new SaveSystem.SaveData() });
            Assert.IsFalse(manager.IsSolved("a"));

            manager.MarkSolved("a");
            ApplyMethod.Invoke(manager, new object[] { null });
            Assert.IsFalse(manager.IsSolved("a"));
        }

        [Test]
        public void Apply_CorruptBlob_DoesNotThrow_AndStartsFresh()
        {
            manager.MarkSolved("a");
            var data = new SaveSystem.SaveData { puzzleJson = "{not json" };
            Assert.DoesNotThrow(() => ApplyMethod.Invoke(manager, new object[] { data }));
            Assert.IsFalse(manager.IsSolved("a"));
        }

        [Test]
        public void Capture_NullData_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => CaptureMethod.Invoke(manager, new object[] { null }));
        }

        [Test]
        public void Register_SolvedIdInBlob_RestoresPuzzle()
        {
            manager.MarkSolved("restore_me");
            TestPuzzle puzzle = MakePuzzle("restore_me");
            manager.Register(puzzle);
            Assert.AreEqual(PuzzleBase.PuzzleState.Solved, puzzle.State);
            Assert.AreEqual(1, puzzle.restoredCount);
        }

        [Test]
        public void Register_UnsolvedId_LeavesPuzzleAvailable()
        {
            TestPuzzle puzzle = MakePuzzle("fresh");
            manager.Register(puzzle);
            Assert.AreEqual(PuzzleBase.PuzzleState.Available, puzzle.State);
        }

        [Test]
        public void Register_NullOrEmptyId_IsIgnored()
        {
            TestPuzzle noId = MakePuzzle("");
            Assert.DoesNotThrow(() => manager.Register(null));
            Assert.DoesNotThrow(() => manager.Register(noId));
            Assert.AreEqual(PuzzleBase.PuzzleState.Available, noId.State);
        }
    }
}
