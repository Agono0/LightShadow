using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LightShadow.Core;
using LightShadow.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Tests.Editor
{
    // audit v3 (menu/Loading review): a stale save-slot confirmation must never
    // act. Before the fix, ConfirmYes ran the FIRST dialog's pending intent
    // against the SECOND dialog's slot, so: repro A (open Delete on slot A,
    // then New Game on slot B, press Yes on A) deleted A's save AND started a
    // new game; repro B (New Game on B, then Delete on A, Yes on B) deleted
    // A's save. The fix makes ShowConfirm close the other rows and record
    // pendingSlot/pendingIntent, and ConfirmYes acts only when the clicked
    // slot equals pendingSlot; None or a stale click does nothing, and
    // ShowNotice is a close-only corrupt-load notice that leaves the file.
    //
    // Two kinds of cases live here:
    //  * UI-only guard cases (no disk): they assert pending state, confirmRoot
    //    state and panel state. They prove the stale/None/Notice arms reach
    //    the no-op branch of ConfirmYes; they do NOT prove filesystem safety
    //    by themselves.
    //  * Disk cases: they run entirely inside a unique temp folder via
    //    SaveSystem.RootDirectoryOverride, set BEFORE any save call, and the
    //    prior override value is restored afterwards. No real slot is ever
    //    touched.
    //
    // The corrupt-slot end-to-end path through StartRun, and the Load and
    // NewGame arms of ConfirmYes, are deliberately not tested: StartRun ends
    // in a scene transition.
    //
    // UNRUN: written against source at base 1c3a251; never compiled or run
    // here. Static expected-pass is not a pass.
    public class SaveSlotConfirmTests
    {
        static readonly Type IntentType =
            typeof(SaveSlotUI).GetNestedType("Intent", BindingFlags.NonPublic);
        static readonly MethodInfo ShowConfirmMethod =
            typeof(SaveSlotUI).GetMethod("ShowConfirm", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo ConfirmYesMethod =
            typeof(SaveSlotUI).GetMethod("ConfirmYes", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo CancelConfirmMethod =
            typeof(SaveSlotUI).GetMethod("CancelConfirm", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo ShowNoticeMethod =
            typeof(SaveSlotUI).GetMethod("ShowNotice", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo PendingIntentField =
            typeof(SaveSlotUI).GetField("pendingIntent", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo PendingSlotField =
            typeof(SaveSlotUI).GetField("pendingSlot", BindingFlags.NonPublic | BindingFlags.Instance);

        GameObject host;
        SaveSlotUI ui;
        GameObject panel;
        SaveSlotUI.SlotRow[] rows;
        readonly List<GameObject> created = new List<GameObject>();

        GameLanguage priorLanguage;
        string priorRootOverride;
        int priorCurrentSlot;
        string tempDir;

        [SetUp]
        public void SetUp()
        {
            priorLanguage = Localization.Current;
            Localization.SetLanguage(GameLanguage.English);

            // Own unique temp root, set BEFORE any save call. The real
            // persistentDataPath is never used by these tests.
            priorCurrentSlot = SaveSystem.CurrentSlot;
            priorRootOverride = SaveSystem.RootDirectoryOverride;
            tempDir = Path.Combine(Path.GetTempPath(), "ls_saveslot_confirm_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            SaveSystem.RootDirectoryOverride = tempDir;

            // Inactive host: Awake stays unrun, so no listeners are wired.
            host = new GameObject("SaveSlotConfirmTestsHost");
            host.SetActive(false);
            created.Add(host);
            ui = host.AddComponent<SaveSlotUI>();

            panel = new GameObject("Panel");
            created.Add(panel);
            ui.panel = panel;

            rows = new SaveSlotUI.SlotRow[SaveSystem.SlotCount];
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i] = FakeRow("Row" + i);
            }
            ui.rows = rows;
        }

        [TearDown]
        public void TearDown()
        {
            Localization.SetLanguage(priorLanguage);

            for (int i = created.Count - 1; i >= 0; i--)
            {
                if (created[i] != null) UnityEngine.Object.DestroyImmediate(created[i]);
            }
            created.Clear();

            // Restore the prior override value (not just reset to null), then
            // delete only our own known temp directory.
            SaveSystem.RootDirectoryOverride = priorRootOverride;
            SaveSystem.CurrentSlot = priorCurrentSlot;
            if (tempDir != null && tempDir.Contains("ls_saveslot_confirm_") && Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            tempDir = null;
        }

        SaveSlotUI.SlotRow FakeRow(string name)
        {
            GameObject root = Child(name + "_Root");
            GameObject confirmRoot = Child(name + "_Confirm");
            confirmRoot.SetActive(false);
            return new SaveSlotUI.SlotRow
            {
                root = root,
                slotText = MakeText(name + "_SlotText"),
                infoText = MakeText(name + "_InfoText"),
                loadButton = MakeButton(name + "_Load"),
                newButton = MakeButton(name + "_New"),
                deleteButton = MakeButton(name + "_Delete"),
                confirmRoot = confirmRoot,
                confirmText = MakeText(name + "_ConfirmText"),
                confirmYes = MakeButton(name + "_Yes"),
                confirmNo = MakeButton(name + "_No"),
            };
        }

        GameObject Child(string name)
        {
            var go = new GameObject(name);
            created.Add(go);
            return go;
        }

        Text MakeText(string name)
        {
            return Child(name).AddComponent<Text>();
        }

        Button MakeButton(string name)
        {
            return Child(name).AddComponent<Button>();
        }

        static object IntentValue(string name)
        {
            return Enum.Parse(IntentType, name);
        }

        void ShowConfirm(int slot, string message, string intent)
        {
            ShowConfirmMethod.Invoke(ui, new object[] { slot, message, IntentValue(intent) });
        }

        void ConfirmYes(int slot)
        {
            ConfirmYesMethod.Invoke(ui, new object[] { slot });
        }

        void CancelConfirm(int slot)
        {
            CancelConfirmMethod.Invoke(ui, new object[] { slot });
        }

        void ShowNotice(int slot, string message)
        {
            ShowNoticeMethod.Invoke(ui, new object[] { slot, message });
        }

        string PendingIntentName()
        {
            object value = PendingIntentField.GetValue(ui);
            return value == null ? null : Enum.GetName(IntentType, value);
        }

        int PendingSlot()
        {
            return (int)PendingSlotField.GetValue(ui);
        }

        void AssertNothingPending()
        {
            Assert.AreEqual("None", PendingIntentName(), "pendingIntent should be reset to None");
            Assert.AreEqual(-1, PendingSlot(), "pendingSlot should be reset to -1");
        }

        static SaveSystem.SaveData MakeSave()
        {
            return new SaveSystem.SaveData
            {
                sceneName = "School",
                savedAtUtc = "2026-01-01T00:00:00Z",
            };
        }

        // Our temp root contains only files these tests wrote; the single
        // slot file is found without depending on SaveSystem's private consts.
        string SingleSlotFile()
        {
            string[] files = Directory.GetFiles(tempDir, "*.json", SearchOption.AllDirectories);
            Assert.AreEqual(1, files.Length, "expected exactly one slot file inside the temp root");
            return files[0];
        }

        // ---------------------------------------------------------------- --
        // UI-only guard cases: no disk, no scene transition. They assert
        // pending/confirmRoot/panel state only; they do NOT prove filesystem
        // safety (the disk cases below cover that inside the temp root).

        [Test]
        public void ShowConfirm_ClosesOtherRowsDialogs_AndRecordsLatestPending()
        {
            ShowConfirm(0, "Delete this slot?", "Delete");
            Assert.IsTrue(rows[0].confirmRoot.activeSelf, "first dialog should be open");

            ShowConfirm(1, "Overwrite this slot?", "NewGame");

            Assert.IsFalse(rows[0].confirmRoot.activeSelf, "opening a second dialog must close the first");
            Assert.IsTrue(rows[1].confirmRoot.activeSelf, "second dialog should be open");
            Assert.AreEqual(1, PendingSlot(), "pendingSlot must track the latest dialog");
            Assert.AreEqual("NewGame", PendingIntentName(), "pendingIntent must track the latest dialog");
            Assert.AreEqual("Overwrite this slot?", rows[1].confirmText.text);
        }

        [Test]
        public void StaleYes_ReproA_DoesNothing()
        {
            // Delete on slot A, then New Game on slot B, then Yes on A.
            ShowConfirm(0, "Delete this slot?", "Delete");
            ShowConfirm(1, "Overwrite this slot?", "NewGame");

            ConfirmYes(0);

            AssertNothingPending();
            Assert.IsFalse(rows[0].confirmRoot.activeSelf, "stale dialog should be closed");
            Assert.IsFalse(rows[1].confirmRoot.activeSelf, "other dialog should stay closed");
            Assert.IsTrue(ui.IsOpen, "panel must stay open: nothing may start");
        }

        [Test]
        public void StaleYes_ReproB_DoesNothing()
        {
            // New Game on slot B, then Delete on slot A, then Yes on B.
            ShowConfirm(1, "Overwrite this slot?", "NewGame");
            ShowConfirm(0, "Delete this slot?", "Delete");

            ConfirmYes(1);

            AssertNothingPending();
            Assert.IsFalse(rows[0].confirmRoot.activeSelf, "other dialog should stay closed");
            Assert.IsFalse(rows[1].confirmRoot.activeSelf, "stale dialog should be closed");
            Assert.IsTrue(ui.IsOpen, "panel must stay open: nothing may start");
        }

        [Test]
        public void NoneYes_DoesNothing()
        {
            ConfirmYes(0);

            AssertNothingPending();
            Assert.IsFalse(rows[0].confirmRoot.activeSelf);
            Assert.IsTrue(ui.IsOpen, "panel must stay open: nothing may start");
        }

        [Test]
        public void Notice_ShowsMessage_AndYesJustCloses()
        {
            const string message = "That save could not be read. It was left untouched.";

            ShowNotice(2, message);

            Assert.IsTrue(rows[2].confirmRoot.activeSelf, "notice should open the row's dialog");
            Assert.AreEqual(message, rows[2].confirmText.text, "notice text must reach the row verbatim");
            Assert.AreEqual("Notice", PendingIntentName());
            Assert.AreEqual(2, PendingSlot());

            ConfirmYes(2);

            AssertNothingPending();
            Assert.IsFalse(rows[2].confirmRoot.activeSelf, "Yes on a notice should just close it");
            Assert.IsTrue(ui.IsOpen, "panel must stay open: nothing may start");
        }

        [Test]
        public void CancelConfirm_ClearsPending_AndLaterYesDoesNothing()
        {
            ShowConfirm(0, "Delete this slot?", "Delete");

            CancelConfirm(0);

            AssertNothingPending();
            Assert.IsFalse(rows[0].confirmRoot.activeSelf, "cancel should close the dialog");

            ConfirmYes(0);

            AssertNothingPending();
            Assert.IsTrue(ui.IsOpen, "panel must stay open: nothing may start");
        }

        // ---------------------------------------------------------------- --
        // Disk cases: run entirely inside the unique temp root set in SetUp.

        [Test]
        public void Notice_PreservesSlotFile()
        {
            Assert.IsTrue(SaveSystem.SaveToSlot(0, MakeSave()), "fixture save should write");
            string file = SingleSlotFile();
            byte[] before = File.ReadAllBytes(file);

            ShowNotice(0, "That save could not be read. It was left untouched.");
            ConfirmYes(0);

            Assert.IsTrue(SaveSystem.HasSave(0), "notice Yes must not delete the save");
            CollectionAssert.AreEqual(before, File.ReadAllBytes(SingleSlotFile()),
                "notice Yes must leave the save file byte-identical");
        }

        [Test]
        public void DeleteConfirm_Yes_DeletesSlotFile()
        {
            Assert.IsTrue(SaveSystem.SaveToSlot(1, MakeSave()), "fixture save should write");
            Assert.IsTrue(SaveSystem.HasSave(1));

            ShowConfirm(1, "Delete this slot?", "Delete");
            ConfirmYes(1);

            Assert.IsFalse(SaveSystem.HasSave(1), "confirming Delete on the pending slot must delete it");
        }

        [Test]
        public void CancelConfirm_ThenYes_LeavesSlotFileIntact()
        {
            Assert.IsTrue(SaveSystem.SaveToSlot(2, MakeSave()), "fixture save should write");
            string file = SingleSlotFile();
            byte[] before = File.ReadAllBytes(file);

            ShowConfirm(2, "Delete this slot?", "Delete");
            CancelConfirm(2);
            ConfirmYes(2);

            Assert.IsTrue(SaveSystem.HasSave(2), "a cancelled delete must not delete the save");
            CollectionAssert.AreEqual(before, File.ReadAllBytes(SingleSlotFile()),
                "a cancelled delete must leave the save file byte-identical");
        }
    }
}
