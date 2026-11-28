using LightShadow.Contrib.Abdelraouf.Journal;
using LightShadow.Core;
using LightShadow.UI;
using NUnit.Framework;

namespace LightShadow.Contrib.Abdelraouf.Tests.Editor
{
    // EditMode tests for the small pure rules added with the overlay work:
    // pause Esc/cursor/autosave decisions, inventory removal events, chip and
    // hint title text, journal note visibility. NOT run when written; the
    // frame-order behaviour of Esc (which Update runs first) is play mode only
    // and unverified.
    public class OverlayRulesTests
    {
        // ---- PauseMenu rules ----

        [Test]
        public void PauseYieldsEscWhileADismissableOverlayIsUp()
        {
            Assert.IsTrue(PauseMenu.ShouldYieldEscape(true, false));
        }

        [Test]
        public void PauseYieldsEscWhenAnOverlayClosedThisFrame()
        {
            Assert.IsTrue(PauseMenu.ShouldYieldEscape(false, true));
        }

        [Test]
        public void PauseOpensWhenNothingOwnsEsc()
        {
            Assert.IsFalse(PauseMenu.ShouldYieldEscape(false, false));
        }

        [Test]
        public void PauseDoesNotRelockCursorWhileAnOverlayNeedsIt()
        {
            Assert.IsFalse(PauseMenu.ShouldRelockCursor(true, false));
            Assert.IsTrue(PauseMenu.ShouldRelockCursor(false, false));
            Assert.IsFalse(PauseMenu.ShouldRelockCursor(false, true), "mobile never locks the cursor");
        }

        [Test]
        public void AutosaveOnlyForAValidSlotThatAlreadyHoldsASave()
        {
            Assert.IsTrue(PauseMenu.ShouldAutosaveToSlot(0, true));
            Assert.IsFalse(PauseMenu.ShouldAutosaveToSlot(0, false), "no save yet: do not create one");
            Assert.IsFalse(PauseMenu.ShouldAutosaveToSlot(-1, true));
            Assert.IsFalse(PauseMenu.ShouldAutosaveToSlot(SaveSystem.SlotCount, true));
        }

        // ---- Inventory.ItemRemoved ----

        [Test]
        public void RemoveFiresItemRemovedOnceForAHeldItem()
        {
            const string id = "overlay_test_item";
            Inventory.Remove(id);
            Inventory.Add(id);
            int fired = 0;
            string seen = null;
            System.Action<string> handler = x => { fired++; seen = x; };
            Inventory.ItemRemoved += handler;
            try
            {
                Inventory.Remove(id);
                Inventory.Remove(id);
            }
            finally { Inventory.ItemRemoved -= handler; }
            Assert.AreEqual(1, fired);
            Assert.AreEqual(id, seen);
            Assert.IsFalse(Inventory.Has(id));
        }

        [Test]
        public void RemoveOfAnItemNotHeldFiresNothing()
        {
            int fired = 0;
            System.Action<string> handler = x => fired++;
            Inventory.ItemRemoved += handler;
            try
            {
                Inventory.Remove("never_added_item");
                Inventory.Remove("");
                Inventory.Remove(null);
            }
            finally { Inventory.ItemRemoved -= handler; }
            Assert.AreEqual(0, fired);
        }

        // ---- InventoryStripUI.ChipText ----

        [Test]
        public void EnglishChipKeepsTheLeadingIconIndent()
        {
            Assert.AreEqual("      Medkit", InventoryStripUI.ChipText("Medkit", true, false, null));
            Assert.AreEqual("Fuse", InventoryStripUI.ChipText("Fuse", false, false, null));
        }

        [Test]
        public void ArabicChipShapesTheNameOnceAndTrailsTheIndent()
        {
            int calls = 0;
            string text = InventoryStripUI.ChipText("name", true, true, s => { calls++; return "<" + s + ">"; });
            Assert.AreEqual(1, calls);
            Assert.AreEqual("<name>      ", text);
            Assert.AreEqual("<name>", InventoryStripUI.ChipText("name", false, true, s => "<" + s + ">"));
        }

        // ---- Hint title ----

        [Test]
        public void EnglishHintTitleIsUnchanged()
        {
            Assert.AreEqual("Hint 1/3 - Nudge",
                LightShadow.Contrib.Abdelraouf.Hints.HintService.BuildTitle(1, 3, false, "Nudge", "x", null));
        }

        [Test]
        public void ArabicHintTitleKeepsTheCounterOutsideTheShapedName()
        {
            string title = LightShadow.Contrib.Abdelraouf.Hints.HintService.BuildTitle(
                2, 3, true, "Clue", "name", s => "[" + s + "]");
            Assert.AreEqual("2/3 [name]", title);
        }

        // ---- Journal note ----

        [Test]
        public void CollectedNoteIsHiddenAndAnUncollectedOneShows()
        {
            Assert.IsFalse(JournalNote.ShouldBeActive(true));
            Assert.IsTrue(JournalNote.ShouldBeActive(false));
        }

        [Test]
        public void JournalToastsAreLogicalStringsWithTheKeyHintInBothLanguages()
        {
            Assert.IsTrue(JournalNote.EnglishToast.Contains("J"));
            Assert.IsTrue(JournalNote.ArabicToast.Contains("J"), "audit 3.7");
            // Logical text: no Arabic presentation forms (U+FB50 to U+FEFF).
            foreach (char c in JournalNote.ArabicToast + JournalNote.ArabicPrompt)
                Assert.IsFalse(c >= '\uFB50' && c <= '\uFEFF', "shaped glyph found: " + (int)c);
        }
    }
}
