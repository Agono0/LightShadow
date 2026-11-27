using System.Collections.Generic;
using LightShadow.Contrib.Abdelraouf.Hints;
using LightShadow.Contrib.Abdelraouf.HintLog;
using NUnit.Framework;

namespace LightShadow.Contrib.Abdelraouf.Tests.Editor
{
    // EditMode tests for the Hint Recall view model: the spoiler rule
    // (only unlocked levels are ever returned), ordering and selection.
    public class HintLogModelTests
    {
        static HintEntry Entry(string id)
        {
            return new HintEntry
            {
                puzzleId = id,
                nudgeEn = id + " nudge", clueEn = id + " clue", answerEn = id + " answer",
                nudgeAr = id + " nudge-ar", clueAr = id + " clue-ar", answerAr = id + " answer-ar",
            };
        }

        static List<HintEntry> Catalog()
        {
            return new List<HintEntry> { Entry("Puzzle_B"), Entry("Puzzle_A"), Entry("Puzzle_C") };
        }

        static int Levels(string id)
        {
            switch (id)
            {
                case "Puzzle_A": return 2;
                case "Puzzle_B": return 3;
                default: return 0;
            }
        }

        [Test]
        public void Rebuild_ListsOnlyPuzzlesWithUnlockedHints_SortedByIdOrdinal()
        {
            var model = new HintLogModel();
            model.Rebuild(Catalog(), Levels);
            Assert.AreEqual(2, model.Count);
            Assert.AreEqual("Puzzle_A", model.Rows[0].PuzzleId);
            Assert.AreEqual("Puzzle_B", model.Rows[1].PuzzleId);
        }

        [Test]
        public void UnlockedTexts_NeverIncludesLevelsAboveUnlocked()
        {
            var model = new HintLogModel();
            model.Rebuild(Catalog(), Levels);
            HintLogModel.Row a = model.Rows[0]; // unlocked 2
            List<KeyValuePair<int, string>> texts = HintLogModel.UnlockedTexts(a, false);
            Assert.AreEqual(2, texts.Count);
            Assert.AreEqual("Puzzle_A nudge", texts[0].Value);
            Assert.AreEqual("Puzzle_A clue", texts[1].Value);
            Assert.IsNull(HintLogModel.TextFor(a, 3, false));
            Assert.IsNull(HintLogModel.TextFor(a, 3, true));
            Assert.AreEqual(1, HintLogModel.LockedCount(a));
        }

        [Test]
        public void Clamp_StopsCorruptValuesFromUnlockingMore()
        {
            Assert.AreEqual(0, HintLogModel.Clamp(-5));
            Assert.AreEqual(HintService.MaxLevel, HintLogModel.Clamp(99));

            var model = new HintLogModel();
            model.Rebuild(Catalog(), id => 99);
            Assert.AreEqual(3, model.Count);
            foreach (HintLogModel.Row row in model.Rows)
            {
                Assert.AreEqual(HintService.MaxLevel, row.Unlocked);
                Assert.IsNull(HintLogModel.TextFor(row, HintService.MaxLevel + 1, false));
            }
        }

        [Test]
        public void TextFor_UsesArabicWhenPresent_AndFallsBackToEnglish()
        {
            var entry = Entry("Puzzle_A");
            entry.clueAr = "";
            var model = new HintLogModel();
            model.Rebuild(new List<HintEntry> { entry }, id => 2);
            Assert.AreEqual("Puzzle_A nudge-ar", HintLogModel.TextFor(model.Rows[0], 1, true));
            Assert.AreEqual("Puzzle_A clue", HintLogModel.TextFor(model.Rows[0], 2, true));
        }

        [Test]
        public void Rebuild_KeepsSelectionById_AndDropsToFirstWhenGone()
        {
            var model = new HintLogModel();
            model.Rebuild(Catalog(), Levels);
            model.Select(1);
            Assert.AreEqual("Puzzle_B", model.Selected.PuzzleId);

            model.Rebuild(Catalog(), id => id == "Puzzle_A" ? 1 : (id == "Puzzle_B" ? 1 : 0));
            Assert.AreEqual("Puzzle_B", model.Selected.PuzzleId);

            model.Rebuild(Catalog(), id => id == "Puzzle_A" ? 1 : 0);
            Assert.AreEqual("Puzzle_A", model.Selected.PuzzleId);
        }

        [Test]
        public void Move_WrapsAndEmptyIsSafe()
        {
            var model = new HintLogModel();
            Assert.IsFalse(model.Move(1));
            Assert.IsNull(model.Selected);

            model.Rebuild(Catalog(), Levels);
            Assert.AreEqual(0, model.SelectedIndex);
            Assert.IsTrue(model.Move(-1));
            Assert.AreEqual(1, model.SelectedIndex);
            Assert.IsTrue(model.Move(1));
            Assert.AreEqual(0, model.SelectedIndex);
        }

        [Test]
        public void Rebuild_IgnoresNullEntriesEmptyIdsAndDuplicates()
        {
            var catalog = new List<HintEntry> { null, new HintEntry { puzzleId = "" }, Entry("Puzzle_A"), Entry("Puzzle_A") };
            var model = new HintLogModel();
            model.Rebuild(catalog, id => 1);
            Assert.AreEqual(1, model.Count);
            model.Rebuild(null, null);
            Assert.AreEqual(0, model.Count);
        }

        [Test]
        public void DisplayName_StripsPrefixAndSplitsWords()
        {
            Assert.AreEqual("Chalkboard Cipher", HintLogModel.DisplayName("Puzzle_ChalkboardCipher"));
            Assert.AreEqual("Optics Bench", HintLogModel.DisplayName("Puzzle_OpticsBench"));
            Assert.AreEqual("some thing", HintLogModel.DisplayName("some_thing"));
            Assert.AreEqual("", HintLogModel.DisplayName(null));
        }
    }
}
