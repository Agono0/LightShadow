using System.Collections.Generic;
using LightShadow.Core;
using LightShadow.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    // GAP COVERED: PuzzleBase (Scripts/Gameplay/Puzzles/PuzzleBase.cs) has no tests.
    // Its state machine, hint pool and event hooks are shared by all seven wing
    // puzzles. A tiny subclass lives in this file so no production code changes.
    //
    // UNVERIFIED LIMITS: Unity does not run Awake/Start for plain MonoBehaviours
    // in EditMode, so Awake is invoked by reflection and Start is never run
    // (the PuzzleManager registration path is covered in PuzzleManagerTests).
    // The Bright Idea Meter penalty branch needs a live GameManager and is not
    // covered. Not run in an editor by the author; CI test-editmode is the first run.
    public class PuzzleBaseTests
    {
        class TestPuzzle : PuzzleBase
        {
            public int interactedCount;
            public int solvedCount;
            public int wrongCount;
            public int restoredCount;

            protected override void OnInteracted(GameObject interactor) { interactedCount++; }
            protected override void OnSolved() { solvedCount++; }
            protected override void OnWrongAttempt() { wrongCount++; }
            protected override void OnRestoredSolved() { restoredCount++; }

            public void DoSolve() { Solve(); }
            public void DoWrong() { WrongAttempt(); }
            public void RunAwake() { Awake(); }
        }

        readonly List<GameObject> created = new List<GameObject>();

        TestPuzzle Make(string id = "test_puzzle")
        {
            var go = new GameObject("PuzzleBaseTests_" + id);
            created.Add(go);
            var puzzle = go.AddComponent<TestPuzzle>();
            puzzle.puzzleId = id;
            return puzzle;
        }

        [SetUp]
        public void SetUp()
        {
            Localization.SetLanguage(GameLanguage.English);
        }

        [TearDown]
        public void TearDown()
        {
            Localization.SetLanguage(GameLanguage.English);
            foreach (GameObject go in created) if (go != null) Object.DestroyImmediate(go);
            created.Clear();
        }

        [Test]
        public void NewPuzzle_IsAvailable_AndInteractable()
        {
            TestPuzzle puzzle = Make();
            Assert.AreEqual(PuzzleBase.PuzzleState.Available, puzzle.State);
            Assert.IsTrue(puzzle.CanInteract);
            puzzle.Interact(null);
            Assert.AreEqual(1, puzzle.interactedCount);
        }

        [Test]
        public void Awake_EmptyId_FallsBackToObjectName()
        {
            TestPuzzle puzzle = Make("");
            puzzle.RunAwake();
            Assert.AreEqual(puzzle.name, puzzle.puzzleId);
        }

        [Test]
        public void Awake_ExistingId_IsKept()
        {
            TestPuzzle puzzle = Make("keep_me");
            puzzle.RunAwake();
            Assert.AreEqual("keep_me", puzzle.puzzleId);
        }

        [Test]
        public void Lock_BlocksInteraction_UnlockRestoresIt()
        {
            TestPuzzle puzzle = Make();
            puzzle.Lock();
            Assert.AreEqual(PuzzleBase.PuzzleState.Locked, puzzle.State);
            Assert.IsFalse(puzzle.CanInteract);
            Assert.AreEqual("", puzzle.Prompt);
            puzzle.Interact(null);
            Assert.AreEqual(0, puzzle.interactedCount, "locked puzzle must not run OnInteracted");
            puzzle.Unlock();
            Assert.AreEqual(PuzzleBase.PuzzleState.Available, puzzle.State);
            Assert.IsTrue(puzzle.CanInteract);
        }

        [Test]
        public void Solve_SetsSolved_RaisesEventOnce_AndStopsPrompting()
        {
            TestPuzzle puzzle = Make("solve_me");
            var ids = new List<string>();
            void OnSolvedEvent(string id) { ids.Add(id); }
            GameEvents.PuzzleSolved += OnSolvedEvent;
            try
            {
                puzzle.DoSolve();
                puzzle.DoSolve(); // second call is a no-op
            }
            finally { GameEvents.PuzzleSolved -= OnSolvedEvent; }

            Assert.AreEqual(PuzzleBase.PuzzleState.Solved, puzzle.State);
            Assert.AreEqual(new[] { "solve_me" }, ids.ToArray());
            Assert.AreEqual(1, puzzle.solvedCount);
            Assert.IsFalse(puzzle.CanInteract);
            Assert.AreEqual("", puzzle.Prompt);
        }

        [Test]
        public void Solved_CannotBeLockedOrUnlocked()
        {
            TestPuzzle puzzle = Make();
            puzzle.DoSolve();
            puzzle.Lock();
            Assert.AreEqual(PuzzleBase.PuzzleState.Solved, puzzle.State);
            puzzle.Unlock();
            Assert.AreEqual(PuzzleBase.PuzzleState.Solved, puzzle.State);
        }

        [Test]
        public void WrongAttempt_RaisesFailedEvent_KeepsPuzzleAvailable()
        {
            TestPuzzle puzzle = Make("wrong_me");
            var ids = new List<string>();
            void OnFailedEvent(string id) { ids.Add(id); }
            GameEvents.PuzzleFailed += OnFailedEvent;
            try
            {
                puzzle.DoWrong();
                puzzle.DoWrong();
            }
            finally { GameEvents.PuzzleFailed -= OnFailedEvent; }

            Assert.AreEqual(2, ids.Count);
            Assert.AreEqual("wrong_me", ids[0]);
            Assert.AreEqual(2, puzzle.wrongCount);
            Assert.AreEqual(PuzzleBase.PuzzleState.Available, puzzle.State);
        }

        [Test]
        public void RestoreSolved_SetsSolved_NoEventNoRewardHook()
        {
            TestPuzzle puzzle = Make("restore_me");
            int solvedEvents = 0;
            void OnSolvedEvent(string id) { solvedEvents++; }
            GameEvents.PuzzleSolved += OnSolvedEvent;
            try
            {
                puzzle.RestoreSolved();
                puzzle.RestoreSolved(); // idempotent
            }
            finally { GameEvents.PuzzleSolved -= OnSolvedEvent; }

            Assert.AreEqual(PuzzleBase.PuzzleState.Solved, puzzle.State);
            Assert.AreEqual(1, puzzle.restoredCount);
            Assert.AreEqual(0, puzzle.solvedCount, "restore must not re-run OnSolved rewards");
            Assert.AreEqual(0, solvedEvents, "restore must not raise PuzzleSolved");
        }

        [Test]
        public void NextHint_ProgressesThenClampsOnLast_AndRaisesEvent()
        {
            TestPuzzle puzzle = Make("hint_me");
            puzzle.hintsEn = new[] { "one", "two", "three" };
            int hintEvents = 0;
            void OnHintEvent(string id) { hintEvents++; }
            GameEvents.PuzzleHintUsed += OnHintEvent;
            try
            {
                Assert.AreEqual("one", puzzle.NextHint());
                Assert.AreEqual("two", puzzle.NextHint());
                Assert.AreEqual("three", puzzle.NextHint());
                Assert.AreEqual("three", puzzle.NextHint(), "must clamp on the last hint");
            }
            finally { GameEvents.PuzzleHintUsed -= OnHintEvent; }
            Assert.AreEqual(4, hintEvents);
        }

        [Test]
        public void NextHint_NoHints_ReturnsEmpty_AndRaisesNoEvent()
        {
            TestPuzzle puzzle = Make();
            puzzle.hintsEn = new string[0];
            int hintEvents = 0;
            void OnHintEvent(string id) { hintEvents++; }
            GameEvents.PuzzleHintUsed += OnHintEvent;
            try { Assert.AreEqual("", puzzle.NextHint()); }
            finally { GameEvents.PuzzleHintUsed -= OnHintEvent; }
            Assert.AreEqual(0, hintEvents);
        }

        [Test]
        public void NextHint_Arabic_UsesArabicPool_FallsBackToEnglishWhenEmpty()
        {
            TestPuzzle puzzle = Make();
            puzzle.hintsEn = new[] { "english hint" };
            puzzle.hintsAr = new[] { "تلميح عربي" };
            Localization.SetLanguage(GameLanguage.Arabic);
            Assert.AreEqual(Localization.Present("تلميح عربي"), puzzle.NextHint());

            TestPuzzle noArabic = Make("no_ar");
            noArabic.hintsEn = new[] { "english hint" };
            noArabic.hintsAr = new string[0];
            Assert.AreEqual(Localization.Present("english hint"), noArabic.NextHint());
        }

        [Test]
        public void DisplayName_Arabic_FallsBackToEnglishWhenArabicEmpty()
        {
            TestPuzzle puzzle = Make();
            puzzle.displayNameEn = "Cipher";
            puzzle.displayNameAr = "";
            Localization.SetLanguage(GameLanguage.Arabic);
            Assert.AreEqual("Cipher", puzzle.DisplayName);
            puzzle.displayNameAr = "شفرة";
            Assert.AreEqual("شفرة", puzzle.DisplayName);
            Localization.SetLanguage(GameLanguage.English);
            Assert.AreEqual("Cipher", puzzle.DisplayName);
        }
    }
}
