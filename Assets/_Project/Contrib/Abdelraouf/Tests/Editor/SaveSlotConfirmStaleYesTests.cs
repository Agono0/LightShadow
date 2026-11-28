using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LightShadow.Core;
using LightShadow.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Contrib.Abdelraouf.Tests.Editor
{
    // Regression coverage for the ConfirmYes stale-row fix in
    // Scripts/UI/SaveSlotUI.cs (PR129, commit 25342c1): a stale ConfirmYes
    // must act on nothing, and once pending is cleared NO dialog may stay
    // open - ConfirmYes now closes every row, not just the clicked one.
    //
    // Distinct from Tests/Editor/SaveSlotConfirmTests.cs. That suite's own
    // header states its UI-only stale cases prove the no-op branch is
    // reached but do NOT prove filesystem safety, and its disk cases cover
    // only the Notice, Delete and Cancel arms - never the stale arm. And
    // because ShowConfirm closes the other rows on open, that suite never
    // has two dialogs open at once, so the new close-all loop is never
    // exercised with a dialog ShowConfirm did not open. These cases fill
    // exactly those two gaps:
    //  * StaleYes_BothDeleteIntents_LeavesBothSlotFilesIntact (disk): the
    //    stale arm's first filesystem-safety proof - both saves and both
    //    files byte-identical after a stale Yes.
    //  * PendingYes_ClosesOrphanedDialogs_AndStillDeletes (disk): a real
    //    Yes still deletes its slot while also closing an orphaned dialog
    //    (a confirmRoot force-activated outside ShowConfirm).
    //  * StaleYes_ClosesOrphanedDialogs_OnEveryRow (UI-only): a stale Yes
    //    closes all three rows, orphan included.
    //  * NoneYes_ClosesOrphanedDialog (UI-only): with nothing pending, a
    //    Yes still closes an orphaned dialog and acts on nothing.
    //
    // NOT covered here (editor limits, stated honestly): the NewGame arm of
    // ConfirmYes ends in a scene transition, same reason the existing suite
    // gives; only Delete intents are used on disk.
    //
    // EXECUTION STATUS: written by inspection against the sources at PR129
    // tip 7400f4d27dc8321f68f669c3542c66bf5695a749; NOT compiled or executed
    // by the author (no Unity editor available where these were written).
    // Static expected-pass is not a pass. No Assert.Inconclusive anywhere.
    //
    // FIXTURE (mirrors Tests/Editor/SaveSlotConfirmTests.cs):
    //  * Disk cases run entirely inside a UNIQUE temp folder via
    //    SaveSystem.RootDirectoryOverride, set BEFORE any save call; the
    //    prior override value (not just null) and the prior CurrentSlot are
    //    restored in TearDown, and only our own prefix-checked temp folder
    //    is deleted. No real slot is ever touched, and the global
    //    ResetRootDirectoryOverride is never called.
    //  * The host GameObject is created INACTIVE before AddComponent, so
    //    Awake stays unrun and no listeners are wired.
    //  * Localization is forced to English and restored, as in the existing
    //    suite.
    //  * Every GameObject this fixture creates is DestroyImmediate'd in
    //    TearDown; reflection members are hard-asserted non-null in SetUp,
    //    so a renamed member fails loudly instead of passing vacuously.
    public class SaveSlotConfirmStaleYesTests
    {
        static readonly Type IntentType =
            typeof(SaveSlotUI).GetNestedType("Intent", BindingFlags.NonPublic);
        static readonly MethodInfo ShowConfirmMethod =
            typeof(SaveSlotUI).GetMethod("ShowConfirm", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo ConfirmYesMethod =
            typeof(SaveSlotUI).GetMethod("ConfirmYes", BindingFlags.NonPublic | BindingFlags.Instance);
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
            Assert.NotNull(IntentType, "SaveSlotUI.Intent nested type missing or renamed");
            Assert.NotNull(ShowConfirmMethod, "SaveSlotUI.ShowConfirm missing or renamed");
            Assert.NotNull(ConfirmYesMethod, "SaveSlotUI.ConfirmYes missing or renamed");
            Assert.NotNull(PendingIntentField, "SaveSlotUI.pendingIntent missing or renamed");
            Assert.NotNull(PendingSlotField, "SaveSlotUI.pendingSlot missing or renamed");

            priorLanguage = Localization.Current;
            Localization.SetLanguage(GameLanguage.English);

            // Own unique temp root, set BEFORE any save call. The real
            // persistentDataPath is never used by these tests.
            priorCurrentSlot = SaveSystem.CurrentSlot;
            priorRootOverride = SaveSystem.RootDirectoryOverride;
            tempDir = Path.Combine(Path.GetTempPath(), "ls_saveslot_staleyes_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            SaveSystem.RootDirectoryOverride = tempDir;

            // Inactive host: Awake stays unrun, so no listeners are wired.
            host = new GameObject("SaveSlotConfirmStaleYesTestsHost");
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
            if (tempDir != null && tempDir.Contains("ls_saveslot_staleyes_") && Directory.Exists(tempDir))
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

        void AssertAllRowsClosed()
        {
            for (int i = 0; i < rows.Length; i++)
            {
                Assert.IsFalse(rows[i].confirmRoot.activeSelf, "row " + i + " dialog must be closed");
            }
        }

        static SaveSystem.SaveData MakeSave()
        {
            return new SaveSystem.SaveData
            {
                sceneName = "School",
                savedAtUtc = "2026-01-01T00:00:00Z",
            };
        }

        // Our temp root contains only files these tests wrote, so a full
        // snapshot proves nothing was created, deleted or modified.
        Dictionary<string, byte[]> SnapshotFiles()
        {
            var snapshot = new Dictionary<string, byte[]>();
            foreach (string file in Directory.GetFiles(tempDir, "*.json", SearchOption.AllDirectories))
            {
                snapshot[file] = File.ReadAllBytes(file);
            }
            return snapshot;
        }

        void AssertFilesUnchanged(Dictionary<string, byte[]> before)
        {
            string[] after = Directory.GetFiles(tempDir, "*.json", SearchOption.AllDirectories);
            Assert.AreEqual(before.Count, after.Length, "the file set inside the temp root must not change");
            foreach (string file in after)
            {
                Assert.IsTrue(before.ContainsKey(file), "no new file may appear inside the temp root: " + file);
                CollectionAssert.AreEqual(before[file], File.ReadAllBytes(file),
                    "a stale Yes must leave save files byte-identical: " + file);
            }
        }

        // ---------------------------------------------------------------- --
        // Disk cases: run entirely inside the unique temp root set in SetUp.

        [Test]
        public void StaleYes_BothDeleteIntents_LeavesBothSlotFilesIntact()
        {
            Assert.IsTrue(SaveSystem.SaveToSlot(0, MakeSave()), "fixture save should write");
            Assert.IsTrue(SaveSystem.SaveToSlot(1, MakeSave()), "fixture save should write");
            Dictionary<string, byte[]> before = SnapshotFiles();
            Assert.AreEqual(2, before.Count, "fixture should have written exactly two slot files");

            ShowConfirm(0, "Delete this slot?", "Delete");
            ShowConfirm(1, "Delete this slot?", "Delete");

            ConfirmYes(0); // stale: the pending dialog is slot 1's

            AssertNothingPending();
            Assert.IsTrue(SaveSystem.HasSave(0), "a stale Yes must not delete the clicked slot's save");
            Assert.IsTrue(SaveSystem.HasSave(1), "a stale Yes must not delete the pending slot's save");
            AssertFilesUnchanged(before);
            AssertAllRowsClosed();
            Assert.IsTrue(ui.IsOpen, "panel must stay open: nothing may start");
        }

        [Test]
        public void PendingYes_ClosesOrphanedDialogs_AndStillDeletes()
        {
            Assert.IsTrue(SaveSystem.SaveToSlot(2, MakeSave()), "fixture save should write");
            Assert.IsTrue(SaveSystem.HasSave(2));

            ShowConfirm(2, "Delete this slot?", "Delete");
            rows[0].confirmRoot.SetActive(true); // orphaned dialog ShowConfirm never opened

            ConfirmYes(2); // the real Yes on the pending slot

            Assert.IsFalse(SaveSystem.HasSave(2), "confirming Delete on the pending slot must still delete it");
            Assert.IsFalse(rows[0].confirmRoot.activeSelf, "a real Yes must also close orphaned dialogs");
            Assert.IsFalse(rows[2].confirmRoot.activeSelf, "the answered dialog must be closed");
            AssertNothingPending();
            Assert.IsTrue(ui.IsOpen, "panel must stay open");
        }

        // ---------------------------------------------------------------- --
        // UI-only guard cases: no disk. They assert pending/confirmRoot/panel
        // state only; the disk arm above is what proves filesystem safety.

        [Test]
        public void StaleYes_ClosesOrphanedDialogs_OnEveryRow()
        {
            ShowConfirm(0, "Delete this slot?", "Delete");
            rows[2].confirmRoot.SetActive(true); // orphaned dialog ShowConfirm never opened

            ConfirmYes(1); // stale: the pending dialog is slot 0's

            AssertNothingPending();
            AssertAllRowsClosed();
            Assert.IsTrue(ui.IsOpen, "panel must stay open: nothing may start");
        }

        [Test]
        public void NoneYes_ClosesOrphanedDialog()
        {
            rows[1].confirmRoot.SetActive(true); // orphaned dialog, nothing pending at all

            ConfirmYes(0);

            AssertNothingPending();
            Assert.IsFalse(rows[1].confirmRoot.activeSelf,
                "with nothing pending a Yes must still close an orphaned dialog");
            Assert.IsTrue(ui.IsOpen, "panel must stay open: nothing may start");
        }
    }
}
