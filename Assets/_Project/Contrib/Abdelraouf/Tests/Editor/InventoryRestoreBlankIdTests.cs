using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightShadow.Core;
using NUnit.Framework;

namespace LightShadow.Contrib.Abdelraouf.Tests.Editor
{
    // Restore-payload coverage for Inventory.OnSaveApplied (Scripts/Core/Inventory.cs).
    //
    // Inventory.Add() rejects null and empty ids, but the restore loop adds
    // every itemIds entry straight into the carried set. A valid save blob
    // such as {"itemIds":["","master_key"]} therefore puts "" into
    // Inventory.Carried: Has("") is false, so the entry can never be used or
    // removed, and the HUD strip (InventoryStripUI.Rebuild, which iterates
    // Carried) draws a blank chip for it.
    //
    // DEPENDENCY: BlankIds_AreNotRestored is EXPECTED RED on base bc7e6e14
    // until the owner's local guard folds in: skip null/empty ids in the
    // OnSaveApplied restore loop (same rule as Add). The other two cases
    // pass without the guard and protect against over-filtering. Nothing here
    // was compiled or run in Unity.
    //
    // Isolation: the private OnSaveApplied handler is invoked directly by
    // reflection, so the global SaveSystem.SaveApplied broadcast (and every
    // other subscriber) is never touched. The handler only edits the static
    // carried set. SetUp snapshots that set and TearDown puts it back by
    // editing the set in place, without Inventory.Add/Remove, so ItemAdded
    // and ItemRemoved subscribers are never fired and no event
    // subscription is added or removed. No disk, prefs, scene or overlay state.
    public class InventoryRestoreBlankIdTests
    {
        static readonly MethodInfo ApplyMethod =
            typeof(Inventory).GetMethod("OnSaveApplied", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        static readonly FieldInfo ItemsField =
            typeof(Inventory).GetField("items", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        List<string> snapshot;

        [SetUp]
        public void SetUp()
        {
            Assert.NotNull(ApplyMethod, "Inventory.OnSaveApplied missing");
            Assert.NotNull(ItemsField, "Inventory.items missing");
            snapshot = new List<string>(CarriedSet());
        }

        [TearDown]
        public void TearDown()
        {
            if (snapshot == null) return;
            HashSet<string> set = CarriedSet();
            set.Clear();
            for (int i = 0; i < snapshot.Count; i++) set.Add(snapshot[i]);
        }

        static HashSet<string> CarriedSet()
        {
            return (HashSet<string>)ItemsField.GetValue(null);
        }

        static void Apply(string inventoryJson)
        {
            var data = new SaveSystem.SaveData { inventoryJson = inventoryJson };
            ApplyMethod.Invoke(null, new object[] { data });
        }

        [Test]
        public void BlankIds_AreNotRestored()
        {
            Apply("{\"itemIds\":[\"\",\"master_key\",\"\",\"master_key\"]}");

            List<string> carried = Inventory.Carried.ToList();
            Assert.IsTrue(Inventory.Has("master_key"));
            Assert.IsFalse(carried.Any(string.IsNullOrEmpty), "restore must not carry an empty id");
            Assert.AreEqual(1, carried.Count);
        }

        [Test]
        public void EmptyList_RestoresNothing()
        {
            Apply("{\"itemIds\":[]}");

            Assert.AreEqual(0, Inventory.Carried.Count());
        }

        [Test]
        public void UnknownId_IsKeptAndShownByItsId()
        {
            Apply("{\"itemIds\":[\"not_a_known_item\"]}");

            Assert.IsTrue(Inventory.Has("not_a_known_item"));
            Assert.AreEqual("not_a_known_item", Inventory.DisplayName("not_a_known_item"));
            Assert.AreEqual(1, Inventory.Carried.Count());
        }
    }
}
