using System.Reflection;
using LightShadow.Contrib.Abdelraouf.InventoryScreen;
using LightShadow.Core;
using LightShadow.Player;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.Tests.Editor
{
    // EditMode regression tests for the mobile BAG button path:
    // TouchButton (kind = Kind.Inventory) -> InventoryScreenUI.Toggle() ->
    // EnsureExists() / Open() / Close(). Dispatch source read at PR129 tip
    // f16605faab607d310e3b688bfed6b49245017308 (TouchButton.cs, case
    // Kind.Inventory calls InventoryScreenUI.Toggle(); eventData is unused in
    // that case, so passing null is safe).
    //
    // EXECUTION STATUS: written by inspection; NOT compiled or executed by the
    // author (no Unity editor available where these were written). Reflection
    // lookups are hard-asserted non-null before use. No Assert.Inconclusive.
    //
    // KNOWN UNCOVERED BRANCHES (listed, not hidden): Open() gates that need a
    // real GameManager fixture - the quiz-open gate (gm.quizUI.IsOpen), the
    // cursor unlock/restore, OverlayStack.Acquire interplay with a live pause
    // menu, and RefreshFromInventory grid rebuilds - are NOT covered here.
    // They need play mode or a heavy GameManager fixture and are flagged as
    // follow-up work instead of being faked. What IS covered: the button
    // dispatch itself, screen self-creation, the no-GameManager gate, toggle
    // idempotence, Close on a closed screen, and the IsOpen property seam.
    //
    // SINGLETON / STATIC FIXTURE CLEANUP: InventoryScreenUI.Instance is set in
    // Awake and cleared in OnDestroy; TearDown DestroyImmediate's every
    // InventoryScreenUI in the scene plus the button host, so no singleton or
    // event subscription leaks between tests. Awake is invoked explicitly via
    // reflection where needed because the editor loop does not dispatch it
    // synchronously in EditMode (same pattern as HUDFadeTests).
    public class InventoryBagButtonTests
    {
        static readonly MethodInfo ScreenAwakeMethod =
            typeof(InventoryScreenUI).GetMethod("Awake", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo RootField =
            typeof(InventoryScreenUI).GetField("root", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        GameObject buttonHost;

        [SetUp]
        public void SetUp()
        {
            Assert.NotNull(ScreenAwakeMethod, "InventoryScreenUI.Awake missing or renamed");
            Assert.NotNull(RootField, "InventoryScreenUI.root missing or renamed");
            buttonHost = new GameObject("BagButton", typeof(TouchButton));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (InventoryScreenUI ui in Object.FindObjectsByType<InventoryScreenUI>(FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(ui.gameObject); // OnDestroy clears Instance and unsubscribes
            }
            if (buttonHost != null) Object.DestroyImmediate(buttonHost);
            buttonHost = null;
        }

        TouchButton BagButton()
        {
            TouchButton button = buttonHost.GetComponent<TouchButton>();
            button.kind = TouchButton.Kind.Inventory; // public field, no reflection needed
            return button;
        }

        InventoryScreenUI FindScreen()
        {
            return Object.FindFirstObjectByType<InventoryScreenUI>();
        }

        InventoryScreenUI TapAndWakeScreen()
        {
            Assert.IsNull(FindScreen(), "pre-clean state expected");
            BagButton().OnPointerDown(null); // dispatch: case Kind.Inventory -> Toggle()
            InventoryScreenUI ui = FindScreen();
            Assert.NotNull(ui, "bag tap did not reach InventoryScreenUI.EnsureExists");
            ScreenAwakeMethod.Invoke(ui, null); // BuildUi + subscriptions + root inactive
            Assert.AreSame(ui, InventoryScreenUI.Instance, "Awake did not set the singleton");
            return ui;
        }

        [Test]
        public void BagButton_FirstTap_CreatesTheScreen()
        {
            InventoryScreenUI ui = TapAndWakeScreen();
            Assert.NotNull(ui);
        }

        [Test]
        public void BagButton_WithoutGameManager_ScreenStaysClosed()
        {
            TapAndWakeScreen();
            Assert.IsNull(GameManager.Instance, "fixture must run without a GameManager");
            Assert.IsFalse(InventoryScreenUI.IsOpen,
                "Open() must gate on GameManager.Instance == null; the screen stays closed");
        }

        [Test]
        public void Toggle_SecondTap_NeverThrows_ScreenStillClosed()
        {
            TapAndWakeScreen();
            // Second toggle takes the Close() path; Close on a closed screen is
            // a no-op and must not throw.
            Assert.DoesNotThrow(() => InventoryScreenUI.Toggle());
            Assert.IsFalse(InventoryScreenUI.IsOpen);
        }

        [Test]
        public void Close_OnClosedScreen_IsANoOp()
        {
            InventoryScreenUI ui = TapAndWakeScreen();
            Assert.DoesNotThrow(() => ui.Close());
            Assert.IsFalse(InventoryScreenUI.IsOpen);
        }

        [Test]
        public void EnsureExists_AfterAwake_ReturnsTheSameInstance()
        {
            InventoryScreenUI ui = TapAndWakeScreen();
            Assert.AreSame(ui, InventoryScreenUI.EnsureExists(), "EnsureExists must not spawn a duplicate");
        }

        [Test]
        public void IsOpen_ReflectsRootActive()
        {
            InventoryScreenUI ui = TapAndWakeScreen();
            var root = (GameObject)RootField.GetValue(ui);
            Assert.NotNull(root, "BuildUi did not create the root");
            Assert.IsFalse(InventoryScreenUI.IsOpen, "root starts inactive after Awake");

            root.SetActive(true);
            Assert.IsTrue(InventoryScreenUI.IsOpen, "IsOpen must track root.activeSelf");
            root.SetActive(false);
            Assert.IsFalse(InventoryScreenUI.IsOpen);
        }
    }
}
