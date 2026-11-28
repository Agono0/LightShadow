using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using LightShadow.Contrib.Abdelraouf.Journal;
using LightShadow.Core;
using LightShadow.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LightShadow.Tests.Editor
{
    // Save-restore-by-identity at the GameManager + capture level (the landed
    // F1-F5 contract). SaveIdentityTests covers the SelectRestores unit seam
    // and the blob round-trip; this suite pins the WIRING: ApplyLoadedState
    // picking the right scene objects, increment clamps, and inactive-object
    // capture. Distinct from those units: every case here builds real scene
    // objects and goes through GameManager or the reflected capture handler.
    //
    // Fixture isolation: the GameManager lives on an inactive host so
    // Awake/OnEnable never run (its public player/enemy/hud stay null and
    // every consumer is null-guarded - verified in source). FuseBox and
    // KnowledgeFragment have no Awake/OnEnable/Start. FindObjectsByType scans
    // are global, so SetUp FAILS FAST if any preexisting box/fragment exists
    // (including inactive) and all fixture objects use unique SRBIT_ names
    // destroyed in TearDown. Journal and Inventory statics are SNAPSHOT in
    // SetUp and RESTORED in TearDown - never blindly cleared.
    public class SaveRestoreByIdentityTests
    {
        static readonly MethodInfo OnCapturedMethod =
            typeof(SaveIdentity).GetMethod("OnCaptured", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        GameObject host;
        GameManager gm;
        readonly List<GameObject> created = new List<GameObject>();
        string[] journalSnapshot;
        string[] inventorySnapshot;

        [SetUp]
        public void SetUp()
        {
            journalSnapshot = new List<string>(JournalProgress.Found).ToArray();
            inventorySnapshot = new List<string>(Inventory.Carried).ToArray();

            Assert.That(OnCapturedMethod, Is.Not.Null, "SaveIdentity.OnCaptured reflection seam broken");
            Assert.That(Object.FindObjectsByType<FuseBox>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length,
                Is.EqualTo(0), "preexisting FuseBox in the scene would pollute the global scans");
            Assert.That(Object.FindObjectsByType<KnowledgeFragment>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length,
                Is.EqualTo(0), "preexisting KnowledgeFragment in the scene would pollute the global scans");

            host = new GameObject("SRBIT_Host");
            host.SetActive(false); // Awake/OnEnable never run: no Instance dance, no subscriptions
            gm = host.AddComponent<GameManager>();
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < created.Count; i++)
            {
                if (created[i] != null) Object.DestroyImmediate(created[i]);
            }
            created.Clear();
            if (host != null) Object.DestroyImmediate(host);

            // Restore the snapshotted statics (no blind clear-and-leave).
            JournalProgress.ResetForNewRun();
            for (int i = 0; i < journalSnapshot.Length; i++) JournalProgress.Add(journalSnapshot[i]);
            Inventory.ResetForNewRun();
            for (int i = 0; i < inventorySnapshot.Length; i++) Inventory.Add(inventorySnapshot[i]);
        }

        FuseBox MakeBox(string name, bool active = true)
        {
            var go = new GameObject(name);
            FuseBox box = go.AddComponent<FuseBox>();
            created.Add(go);
            if (!active) go.SetActive(false);
            return box;
        }

        KnowledgeFragment MakeFragment(string saveId)
        {
            var go = new GameObject("SRBIT_FragHost_" + saveId);
            KnowledgeFragment fragment = go.AddComponent<KnowledgeFragment>();
            fragment.fragmentName = saveId;
            created.Add(go);
            return fragment;
        }

        static SaveSystem.SaveData DataWith(SaveIdentity.Blob blob, int fuses, int fragments)
        {
            var data = new SaveSystem.SaveData();
            data.fusesRestored = fuses;
            data.fragmentsCollected = fragments;
            if (blob != null) SaveSystem.SetContrib(data, SaveIdentity.Key, SaveIdentity.Write(blob));
            return data;
        }

        static SaveIdentity.Blob BlobWith(string[] fuses, string[] fragments)
        {
            var blob = new SaveIdentity.Blob();
            if (fuses != null) blob.fuses.AddRange(fuses);
            if (fragments != null) blob.fragments.AddRange(fragments);
            return blob;
        }

        SaveSystem.SaveData Capture()
        {
            var data = new SaveSystem.SaveData();
            OnCapturedMethod.Invoke(null, new object[] { data });
            return data;
        }

        // ---- A. ApplyLoadedState wiring ------------------------------------

        [Test]
        public void ApplyLoadedState_RestoresNamedBox_NotFirstN()
        {
            FuseBox a = MakeBox("SRBIT_Fuse_A");
            FuseBox b = MakeBox("SRBIT_Fuse_B");
            gm.ApplyLoadedState(DataWith(BlobWith(new[] { "SRBIT_Fuse_B" }, null), 1, 0));
            Assert.IsFalse(a.Restored, "un-named box must stay unrestored");
            Assert.IsTrue(b.Restored, "the identity-named box must restore regardless of scan order");
        }

        [Test]
        public void ApplyLoadedState_OldSave_RestoresFirstN()
        {
            FuseBox a = MakeBox("SRBIT_Fuse_A");
            FuseBox b = MakeBox("SRBIT_Fuse_B");
            gm.ApplyLoadedState(DataWith(null, 1, 0));
            // Scan order is unspecified: the contract is exactly one restored.
            Assert.AreEqual(1, (a.Restored ? 1 : 0) + (b.Restored ? 1 : 0), "old save restores the first N of the scan");
        }

        [Test]
        public void ApplyLoadedState_DuplicateNames_RestoresOnceWithWarning()
        {
            FuseBox first = MakeBox("SRBIT_Fuse_Dup");
            FuseBox second = MakeBox("SRBIT_Fuse_Dup");
            LogAssert.Expect(LogType.Warning, new Regex("Duplicate save identity"));
            gm.ApplyLoadedState(DataWith(BlobWith(new[] { "SRBIT_Fuse_Dup" }, null), 1, 0));
            Assert.AreEqual(1, (first.Restored ? 1 : 0) + (second.Restored ? 1 : 0),
                "each identity restores once (first match), never every same-named object");
        }

        [Test]
        public void ApplyLoadedState_UnmatchedIdentity_FallsBackToCountRemainder()
        {
            FuseBox a = MakeBox("SRBIT_Fuse_A");
            FuseBox b = MakeBox("SRBIT_Fuse_B");
            FuseBox c = MakeBox("SRBIT_Fuse_C");
            gm.ApplyLoadedState(DataWith(BlobWith(new[] { "SRBIT_Gone", "SRBIT_Fuse_B" }, null), 2, 0));
            Assert.IsTrue(b.Restored, "matched identity restores");
            Assert.IsTrue(a.Restored, "the unmatched identity falls back to count: one more of the remainder");
            Assert.IsFalse(c.Restored, "the saved count is never exceeded");
        }

        [Test]
        public void ApplyLoadedState_InactiveFuseBox_IsScannedAndRestored()
        {
            FuseBox inactive = MakeBox("SRBIT_Fuse_Inactive", active: false);
            FuseBox active = MakeBox("SRBIT_Fuse_Active");
            gm.ApplyLoadedState(DataWith(BlobWith(new[] { "SRBIT_Fuse_Inactive" }, null), 1, 0));
            Assert.IsTrue(inactive.Restored, "the restore scan includes inactive fuse boxes");
            Assert.IsFalse(active.Restored);
        }

        [Test]
        public void ApplyLoadedState_FragmentRestoredByIdentity()
        {
            KnowledgeFragment a = MakeFragment("SRBIT_frag_a");
            KnowledgeFragment b = MakeFragment("SRBIT_frag_b");
            gm.ApplyLoadedState(DataWith(BlobWith(null, new[] { "SRBIT_frag_b" }), 0, 1));
            Assert.IsFalse(a.Collected);
            Assert.IsTrue(b.Collected, "the identity-named fragment restores");
            Assert.IsFalse(b.gameObject.activeSelf, "a restored fragment hides itself");
        }

        // ---- B. Increment clamps --------------------------------------------

        [Test]
        public void FuseIncrement_ClampsAtTarget()
        {
            for (int i = 0; i < GameManager.FuseTarget + 2; i++) gm.OnFuseRestored(null);
            Assert.AreEqual(GameManager.FuseTarget, gm.FusesRestored);
        }

        [Test]
        public void FragmentIncrement_ClampsAtTarget()
        {
            for (int i = 0; i < GameManager.FragmentTarget + 2; i++) gm.OnFragmentCollected();
            Assert.AreEqual(GameManager.FragmentTarget, gm.FragmentsCollected);
        }

        // ---- C. Capture (reflected OnCaptured) ------------------------------

        [Test]
        public void Capture_IncludesInactiveRestoredFuseBox()
        {
            FuseBox box = MakeBox("SRBIT_Fuse_Inactive");
            box.RestoreFromSave();
            box.gameObject.SetActive(false);
            SaveIdentity.Blob blob = SaveIdentity.Read(Capture());
            Assert.IsNotNull(blob);
            Assert.IsTrue(SaveIdentity.Contains(blob.fuses, "SRBIT_Fuse_Inactive"),
                "fuse identity capture must include inactive objects, like the fragment scan");
        }

        [Test]
        public void Capture_InactiveCollectedFragment_Recorded()
        {
            KnowledgeFragment fragment = MakeFragment("SRBIT_frag_collected");
            fragment.RestoreFromSave(); // marks collected and deactivates itself
            SaveIdentity.Blob blob = SaveIdentity.Read(Capture());
            Assert.IsNotNull(blob);
            Assert.IsTrue(SaveIdentity.Contains(blob.fragments, "SRBIT_frag_collected"));
        }

        // ---- D. Journal reset (distinct from the existing single-note case) --

        [Test]
        public void JournalReset_AllowsReaddingSameNote()
        {
            Assert.IsTrue(JournalProgress.Add("SRBIT_note_readd"));
            JournalProgress.ResetForNewRun();
            Assert.IsFalse(JournalProgress.Has("SRBIT_note_readd"));
            Assert.IsTrue(JournalProgress.Add("SRBIT_note_readd"),
                "reset leaves no tombstone: the same note id can be earned again");
            Assert.IsTrue(JournalProgress.Has("SRBIT_note_readd"));
        }
    }
}
