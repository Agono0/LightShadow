using System.Reflection;
using LightShadow.Contrib.Abdelraouf.Overlay;
using LightShadow.Gameplay;
using LightShadow.Player;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Contrib.Ziad.Tests.Editor
{
    // D1: HidingSystem.ResetState must clear the hidden statics and release the
    // "Hiding" overlay lock WITHOUT the exit choreography, so a catch
    // interrupted mid-hide cannot leave IsHidden stuck on (EnemySight goes
    // permanently blind) or the input lock held forever.
    //
    // Fixture policy: STRICT empty state. SetUp fails fast when the overlay
    // stack is not empty or a HidingSystem/HideSpot already exists in the open
    // scene - these tests never fight another fixture's state. The only lock
    // token touched is the one this fixture acquires itself; TearDown releases
    // exactly that token when a failure path left it held, restores every
    // static it snapshotted, and asserts the stack is net-zero.
    // No OverlayStack.ResetAll, no scene sweeps, no coroutines.
    //
    // UNRUN: never compiled or run here; static expected-pass is not a pass.
    public class HidingSystemResetTests
    {
        static readonly PropertyInfo IsHiddenProp = typeof(HidingSystem).GetProperty("IsHidden");
        static readonly PropertyInfo CurrentSpotProp = typeof(HidingSystem).GetProperty("CurrentSpot");
        static readonly PropertyInfo WatchedProp = typeof(HidingSystem).GetProperty("EnteredUnderWatch");
        static readonly FieldInfo HideLockField =
            typeof(HidingSystem).GetField("hideLock", BindingFlags.NonPublic | BindingFlags.Static);

        bool wasHidden;
        HideSpot wasSpot;
        bool wasWatched;
        InputLockToken wasLock;

        static void SetPrivate(PropertyInfo prop, object value)
        {
            prop.GetSetMethod(true).Invoke(null, new[] { value });
        }

        [SetUp]
        public void SnapshotAndGuard()
        {
            Assert.AreEqual(0, OverlayStack.Count, "fixture requires an empty overlay stack");
            Assert.IsNull(Object.FindObjectOfType<HidingSystem>(), "fixture requires no HidingSystem in the open scene");
            Assert.IsNull(Object.FindObjectOfType<HideSpot>(), "fixture requires no HideSpot in the open scene");
            Assert.IsNotNull(HideLockField, "HidingSystem.hideLock must exist for this fixture");
            wasHidden = HidingSystem.IsHidden;
            wasSpot = HidingSystem.CurrentSpot;
            wasWatched = HidingSystem.EnteredUnderWatch;
            wasLock = (InputLockToken)HideLockField.GetValue(null);
        }

        [TearDown]
        public void RestoreStatics()
        {
            // A failing ResetState could leave OUR token held; release exactly
            // that token, never a blanket reset.
            InputLockToken held = (InputLockToken)HideLockField.GetValue(null);
            if (held != null && held != wasLock && held.IsActive) OverlayStack.Release(ref held);
            SetPrivate(IsHiddenProp, wasHidden);
            SetPrivate(CurrentSpotProp, wasSpot);
            SetPrivate(WatchedProp, wasWatched);
            HideLockField.SetValue(null, wasLock);
            Assert.AreEqual(0, OverlayStack.Count, "fixture must leave the overlay stack net-zero");
        }

        [Test]
        public void ResetState_ClearsHiddenStatics()
        {
            SetPrivate(IsHiddenProp, true);
            SetPrivate(WatchedProp, true);

            HidingSystem.ResetState();

            Assert.IsFalse(HidingSystem.IsHidden, "IsHidden must clear");
            Assert.IsNull(HidingSystem.CurrentSpot, "CurrentSpot must clear");
            Assert.IsFalse(HidingSystem.EnteredUnderWatch, "EnteredUnderWatch must clear");
        }

        [Test]
        public void ResetState_ClearsCurrentSpotReference()
        {
            var go = new GameObject("OwnSpot");
            try
            {
                HideSpot spot = go.AddComponent<HideSpot>();
                SetPrivate(IsHiddenProp, true);
                SetPrivate(CurrentSpotProp, spot);

                HidingSystem.ResetState();

                Assert.IsNull(HidingSystem.CurrentSpot, "the spot reference must not survive a reset");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ResetState_ReleasesOwnHidingLock()
        {
            InputLockToken token = OverlayStack.Acquire("Hiding");
            Assert.IsTrue(OverlayStack.Holds("Hiding"), "precondition: this fixture's token is held");
            HideLockField.SetValue(null, token);

            HidingSystem.ResetState();

            Assert.IsFalse(OverlayStack.Holds("Hiding"), "the Hiding lock must be released");
            Assert.AreEqual(0, OverlayStack.Count, "the stack must return to empty");
        }

        [Test]
        public void ResetState_WithoutLock_DoesNotThrowAndKeepsStackEmpty()
        {
            Assert.DoesNotThrow(() => HidingSystem.ResetState());
            Assert.AreEqual(0, OverlayStack.Count);
        }
    }
}
