using System.Reflection;
using LightShadow.Core;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    // EditMode tests for the static inventory (finish-the-game audit, step 3:
    // tests for pure logic). Drives the real save-blob handlers by reflection;
    // they stay private to the class in production.
    public class InventoryTests
    {
        static readonly MethodInfo CaptureMethod =
            typeof(Inventory).GetMethod("OnSaveCaptured", BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo ApplyMethod =
            typeof(Inventory).GetMethod("OnSaveApplied", BindingFlags.NonPublic | BindingFlags.Static);

        [SetUp]
        public void SetUp()
        {
            Assert.NotNull(CaptureMethod, "Inventory.OnSaveCaptured missing");
            Assert.NotNull(ApplyMethod, "Inventory.OnSaveApplied missing");
            ApplyMethod.Invoke(null, new object[] { new SaveSystem.SaveData() });
        }

        [TearDown]
        public void TearDown()
        {
            ApplyMethod.Invoke(null, new object[] { new SaveSystem.SaveData() });
        }

        [Test]
        public void Add_Has_Remove_RoundTrip()
        {
            Assert.IsFalse(Inventory.Has("master_key"));
            Inventory.Add("master_key");
            Assert.IsTrue(Inventory.Has("master_key"));
            Inventory.Remove("master_key");
            Assert.IsFalse(Inventory.Has("master_key"));
        }

        [Test]
        public void Add_IgnoresNullAndEmpty()
        {
            Inventory.Add(null);
            Inventory.Add("");
            Assert.IsFalse(Inventory.Has(null));
            Assert.IsFalse(Inventory.Has(""));
        }

        [Test]
        public void Add_Duplicate_FiresItemAddedOnce()
        {
            int count = 0;
            System.Action<string> listener = _ => count++;
            Inventory.ItemAdded += listener;
            try
            {
                Inventory.Add("fuse");
                Inventory.Add("fuse");
            }
            finally
            {
                Inventory.ItemAdded -= listener;
            }
            Assert.AreEqual(1, count);
        }

        [Test]
        public void SaveBlob_RoundTrips_CarriedItems()
        {
            Inventory.Add("master_key");
            Inventory.Add("fuse");

            var data = new SaveSystem.SaveData();
            CaptureMethod.Invoke(null, new object[] { data });
            Assert.IsFalse(string.IsNullOrEmpty(data.inventoryJson));

            ApplyMethod.Invoke(null, new object[] { new SaveSystem.SaveData() });
            Assert.IsFalse(Inventory.Has("master_key"));

            ApplyMethod.Invoke(null, new object[] { data });
            Assert.IsTrue(Inventory.Has("master_key"));
            Assert.IsTrue(Inventory.Has("fuse"));
        }

        [Test]
        public void SaveBlob_Corrupt_ResetsWithoutThrowing()
        {
            Inventory.Add("master_key");
            var data = new SaveSystem.SaveData { inventoryJson = "{not json" };
            Assert.DoesNotThrow(() => ApplyMethod.Invoke(null, new object[] { data }));
            Assert.IsFalse(Inventory.Has("master_key"));
        }

        [Test]
        public void DisplayName_KnownItem_LocalizedAndFallback()
        {
            Localization.SetLanguage(GameLanguage.English);
            Assert.AreEqual("Fuse", Inventory.DisplayName("fuse"));
            Localization.SetLanguage(GameLanguage.Arabic);
            Assert.AreEqual("مصهر", Inventory.DisplayName("fuse"));
            Assert.AreEqual("unknown_thing", Inventory.DisplayName("unknown_thing"));
            Localization.SetLanguage(GameLanguage.English);
        }
    }
}
