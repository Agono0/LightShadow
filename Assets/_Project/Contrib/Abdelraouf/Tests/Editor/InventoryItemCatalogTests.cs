using LightShadow.Contrib.Abdelraouf.InventoryScreen;
using LightShadow.Core;
using NUnit.Framework;

namespace LightShadow.Contrib.Abdelraouf.Tests.Editor
{
    // EditMode tests for the item catalog. Use handlers are registered by the
    // owner's side at startup; these tests register their own throwaway
    // handlers and clean them up again.
    public class InventoryItemCatalogTests
    {
        const string TestId = "catalog_test_item";

        [TearDown]
        public void TearDown()
        {
            InventoryItemCatalog.Register(TestId, null);
            InventoryItemCatalog.ClearUseHandlers();
            Inventory.Remove(TestId);
        }

        [Test]
        public void Defaults_CoverTheFiveKnownIds_WithDescriptionsAndIcons()
        {
            string[] known = { "master_key", "fuse", "classroom_key", "staff_key", "medkit" };
            foreach (string id in known)
            {
                Assert.IsNotEmpty(InventoryItemCatalog.Description(id), id);
            }
            Assert.AreEqual(InventoryItemCatalog.IconKeyKey, InventoryItemCatalog.IconKey("master_key"));
            Assert.AreEqual(InventoryItemCatalog.IconKeyMedkit, InventoryItemCatalog.IconKey("medkit"));
        }

        [Test]
        public void UnknownId_FallsBackToStripHeuristic()
        {
            Assert.AreEqual(InventoryItemCatalog.IconKeyKey, InventoryItemCatalog.IconKey("brass_key"));
            Assert.AreEqual(InventoryItemCatalog.IconKeyGeneric, InventoryItemCatalog.IconKey("rock"));
            Assert.AreEqual("", InventoryItemCatalog.Description("rock"));
        }

        [Test]
        public void TryUse_WithoutHandler_KeepsTheItem()
        {
            Inventory.Add(TestId);
            Assert.IsFalse(InventoryItemCatalog.TryUse(TestId));
            Assert.IsTrue(Inventory.Has(TestId));
        }

        [Test]
        public void TryUse_HandlerDeclines_KeepsTheItem()
        {
            Inventory.Add(TestId);
            InventoryItemCatalog.Register(TestId, () => false);
            Assert.IsFalse(InventoryItemCatalog.TryUse(TestId));
            Assert.IsTrue(Inventory.Has(TestId));
        }

        [Test]
        public void TryUse_HandlerConsumes_RemovesItemAndFires()
        {
            Inventory.Add(TestId);
            string usedId = null;
            InventoryItemCatalog.ItemUsed += id => usedId = id;
            InventoryItemCatalog.Register(TestId, () => true);

            Assert.IsTrue(InventoryItemCatalog.TryUse(TestId));
            Assert.IsFalse(Inventory.Has(TestId));
            Assert.AreEqual(TestId, usedId);
        }

        [Test]
        public void TryUse_ThrowingHandler_KeepsTheItem()
        {
            Inventory.Add(TestId);
            InventoryItemCatalog.Register(TestId, () => throw new System.InvalidOperationException("boom"));
            Assert.IsFalse(InventoryItemCatalog.TryUse(TestId));
            Assert.IsTrue(Inventory.Has(TestId));
            // The thrown handler logs a Unity warning on purpose.
        }
    }
}
