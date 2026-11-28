using LightShadow.Core;
using System.Collections.Generic;
using LightShadow.Contrib.Abdelraouf.Journal;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class SaveIdentityTests
    {
        [Test]
        public void OldSave_WithoutBlob_ReadsNull()
        {
            Assert.IsNull(SaveIdentity.Read(new SaveSystem.SaveData()));
        }

        [Test]
        public void RoundTrip_KeepsWhichPickupsWereDone()
        {
            var blob = new SaveIdentity.Blob();
            blob.fuses.Add("Fuse_Physics");
            blob.fragments.Add("the pressed-flower bookmark");
            var data = new SaveSystem.SaveData();
            SaveSystem.SetContrib(data, SaveIdentity.Key, SaveIdentity.Write(blob));

            SaveIdentity.Blob read = SaveIdentity.Read(data);
            Assert.IsNotNull(read);
            Assert.IsTrue(SaveIdentity.Contains(read.fuses, "Fuse_Physics"));
            Assert.IsFalse(SaveIdentity.Contains(read.fuses, "Fuse_Library"));
            Assert.IsTrue(SaveIdentity.Contains(read.fragments, "the pressed-flower bookmark"));
        }

        [Test]
        public void BrokenBlob_ReadsNull()
        {
            var data = new SaveSystem.SaveData();
            SaveSystem.SetContrib(data, SaveIdentity.Key, "{not json");
            Assert.IsNull(SaveIdentity.Read(data));
        }

        [Test]
        public void SelectRestores_OldSave_TakesFirstN()
        {
            bool[] pick = SaveIdentity.SelectRestores(new[] { "a", "b", "c" }, null, 2);
            CollectionAssert.AreEqual(new[] { true, true, false }, pick);
        }

        [Test]
        public void SelectRestores_ByName_SkipsDuplicateNames()
        {
            bool[] pick = SaveIdentity.SelectRestores(new[] { "a", "a", "b" }, new List<string> { "a" }, 1);
            CollectionAssert.AreEqual(new[] { true, false, false }, pick);
        }

        [Test]
        public void SelectRestores_UnmatchedIdentity_FallsBackToCount()
        {
            // Saved "gone" matches nothing; count 2 means one more is needed beyond "b".
            bool[] pick = SaveIdentity.SelectRestores(new[] { "a", "b", "c" }, new List<string> { "b", "gone" }, 2);
            CollectionAssert.AreEqual(new[] { true, true, false }, pick);
        }

        [Test]
        public void SelectRestores_NeverExceedsSavedCount()
        {
            bool[] pick = SaveIdentity.SelectRestores(new[] { "a", "b", "c" }, new List<string> { "a" }, 3);
            int n = 0;
            for (int i = 0; i < pick.Length; i++) if (pick[i]) n++;
            Assert.LessOrEqual(n, 1);
        }

        [Test]
        public void NewRun_ClearsInventoryAndJournal()
        {
            Inventory.Add("master_key");
            JournalProgress.Add("test_note_new_run");
            Inventory.ResetForNewRun();
            JournalProgress.ResetForNewRun();
            Assert.IsFalse(Inventory.Has("master_key"));
            Assert.IsFalse(JournalProgress.Has("test_note_new_run"));
        }
    }
}
