using System.Reflection;
using LightShadow.Contrib.Abdelraouf.InventoryScreen;
using LightShadow.Core;
using LightShadow.Player;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.Tests.Editor
{
    // EditMode regression tests for the REAL Core/InventoryUseHandlers.cs
    // medkit path: the owner's Register() wires "medkit" into
    // InventoryItemCatalog, and UseMedkit() drives PlayerHealth.TryUseMedkit.
    //
    // EXECUTION STATUS: written by inspection against the sources at PR129 tip
    // f16605faab607d310e3b688bfed6b49245017308; NOT compiled or executed by the
    // author (no Unity editor available where these were written). Every
    // reflection lookup is hard-asserted non-null before use: a renamed or
    // removed member fails the test loudly. No Assert.Inconclusive anywhere.
    //
    // SINGLETON / STATIC FIXTURE CLEANUP:
    // - PlayerHealth.Instance is set in Awake and cleared in OnDestroy; every
    //   created GameObject is DestroyImmediate'd in TearDown, so the static
    //   Instance cannot leak between tests.
    // - GameManager.Instance is never created here: it stays null for the
    //   whole fixture, which is itself asserted (the null-guard is a tested
    //   seam, not an accident).
    // - Inventory is a static store; SetUp/TearDown reset it through the same
    //   save-blob Apply method InventoryTests uses, with an empty SaveData.
    // - Catalog use handlers are cleared with the provided test hook
    //   InventoryItemCatalog.ClearUseHandlers() before and after each test.
    public class InventoryUseHandlersTests
    {
        static readonly MethodInfo RegisterMethod =
            typeof(InventoryUseHandlers).GetMethod("Register", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo UseMedkitMethod =
            typeof(InventoryUseHandlers).GetMethod("UseMedkit", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo HealthAwakeMethod =
            typeof(PlayerHealth).GetMethod("Awake", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo InventoryApplyMethod =
            typeof(Inventory).GetMethod("OnSaveApplied", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        GameObject healthHost;
        PlayerHealth health;

        [SetUp]
        public void SetUp()
        {
            Assert.NotNull(RegisterMethod, "InventoryUseHandlers.Register missing or renamed");
            Assert.NotNull(UseMedkitMethod, "InventoryUseHandlers.UseMedkit missing or renamed");
            Assert.NotNull(HealthAwakeMethod, "PlayerHealth.Awake missing or renamed");
            Assert.NotNull(InventoryApplyMethod, "Inventory.OnSaveApplied missing or renamed");
            InventoryApplyMethod.Invoke(null, new object[] { new SaveSystem.SaveData() });
            InventoryItemCatalog.ClearUseHandlers();
        }

        [TearDown]
        public void TearDown()
        {
            InventoryItemCatalog.ClearUseHandlers();
            Inventory.Remove("medkit");
            if (healthHost != null) Object.DestroyImmediate(healthHost); // OnDestroy clears PlayerHealth.Instance
            healthHost = null;
            health = null;
            InventoryApplyMethod.Invoke(null, new object[] { new SaveSystem.SaveData() });
        }

        // Creates the health singleton the way EditMode tests in this repo do:
        // Awake is invoked explicitly because the editor loop does not dispatch
        // it synchronously for AddComponent. Awake is idempotent here.
        PlayerHealth CreateHealth()
        {
            healthHost = new GameObject("HealthTest", typeof(PlayerHealth));
            health = healthHost.GetComponent<PlayerHealth>();
            HealthAwakeMethod.Invoke(health, null);
            Assert.AreSame(health, PlayerHealth.Instance, "PlayerHealth.Awake did not set Instance");
            return health;
        }

        [Test]
        public void Register_WiresTheRealMedkitHandler()
        {
            Assert.IsFalse(InventoryItemCatalog.HasUseHandler("medkit"), "pre-clean state expected");
            RegisterMethod.Invoke(null, null);
            Assert.IsTrue(InventoryItemCatalog.HasUseHandler("medkit"),
                "InventoryUseHandlers.Register did not wire the medkit handler");
        }

        [Test]
        public void UseMedkit_NoPlayer_ReturnsFalse_KeepsItem()
        {
            Assert.IsNull(PlayerHealth.Instance, "fixture leak: a PlayerHealth singleton exists before the test");
            RegisterMethod.Invoke(null, null);
            Inventory.Add("medkit");

            bool used = (bool)UseMedkitMethod.Invoke(null, null);

            Assert.IsFalse(used, "UseMedkit must decline when PlayerHealth.Instance is null");
            Assert.IsTrue(Inventory.Has("medkit"), "declined use must keep the item");
        }

        [Test]
        public void Medkit_FullHealth_NotConsumed_ItemKept()
        {
            CreateHealth();
            RegisterMethod.Invoke(null, null);
            Inventory.Add("medkit");

            bool used = InventoryItemCatalog.TryUse("medkit"); // through the real registered handler

            Assert.IsFalse(used, "a medkit at full health must not be consumed");
            Assert.IsTrue(Inventory.Has("medkit"));
            Assert.AreEqual(PlayerHealth.MaxHits, health.HitsRemaining);
        }

        [Test]
        public void Medkit_OneHit_HealsToFull_SingleRemoval_Events()
        {
            CreateHealth();
            RegisterMethod.Invoke(null, null);
            Assert.IsNull(GameManager.Instance,
                "this fixture never creates a GameManager; the ShowMessage null-guard is part of what is tested");

            Assert.IsTrue(health.ApplyHit(), "first strike should leave Nour standing");
            Assert.AreEqual(1, health.HitsRemaining);

            float changedTo = -1f;
            int changedCount = 0;
            health.Changed += normalized => { changedTo = normalized; changedCount++; };
            string usedId = null;
            InventoryItemCatalog.ItemUsed += id => usedId = id;
            int removedCount = 0;
            Inventory.ItemRemoved += id => { if (id == "medkit") removedCount++; };

            Inventory.Add("medkit");
            bool used = InventoryItemCatalog.TryUse("medkit");

            Assert.IsTrue(used, "one hit taken + kit in bag must consume");
            Assert.AreEqual(PlayerHealth.MaxHits, health.HitsRemaining, "one kit returns to full");
            Assert.IsFalse(Inventory.Has("medkit"), "consumed kit must leave the bag");
            Assert.GreaterOrEqual(changedCount, 1, "Changed must fire on the heal");
            Assert.AreEqual(1f, changedTo, "Changed must report full health");
            Assert.AreEqual("medkit", usedId, "catalog ItemUsed must fire for the consumed id");
            // The documented double-remove contract (InventoryUseHandlers.cs
            // comment): TryUseMedkit removes the id, the catalog removes it
            // again on success, and Inventory.Remove ignores a missing id, so
            // exactly one ItemRemoved may fire.
            Assert.AreEqual(1, removedCount, "double-remove contract broken: ItemRemoved fired more than once");
        }

        [Test]
        public void Medkit_OneHit_NoKit_ReturnsFalse_HealthUnchanged()
        {
            CreateHealth();
            RegisterMethod.Invoke(null, null);
            Assert.IsTrue(health.ApplyHit());
            Assert.AreEqual(1, health.HitsRemaining);

            bool used = InventoryItemCatalog.TryUse("medkit");

            Assert.IsFalse(used, "no kit in the bag must decline");
            Assert.AreEqual(1, health.HitsRemaining, "a declined use must not heal");
        }

        [Test]
        public void UseMedkit_DirectCall_MatchesCatalogPath()
        {
            // The Update() hotkey and the mobile MEDKIT TouchButton call
            // PlayerHealth.TryUseMedkit directly; the catalog path goes through
            // InventoryUseHandlers.UseMedkit. Both must agree on the outcome.
            CreateHealth();
            RegisterMethod.Invoke(null, null);
            Assert.IsTrue(health.ApplyHit());
            Inventory.Add("medkit");

            bool direct = health.TryUseMedkit();
            Assert.IsTrue(direct);
            Assert.AreEqual(PlayerHealth.MaxHits, health.HitsRemaining);
            Assert.IsFalse(Inventory.Has("medkit"));

            // Second use with no kit and full health: both paths decline.
            Assert.IsFalse((bool)UseMedkitMethod.Invoke(null, null));
            Assert.IsFalse(InventoryItemCatalog.TryUse("medkit"));
        }
    }
}
