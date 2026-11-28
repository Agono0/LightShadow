using System;
using System.Linq;
using System.Reflection;
using LightShadow.Core;
using LightShadow.UI;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.Tests.Editor
{
    // AI-prepared, not compiled or run in Unity.
    //
    // HUDController.Awake subscribes OnObjectiveInputChanged to three static
    // events (Inventory.ItemAdded, Inventory.ItemRemoved, GameEvents.PuzzleSolved)
    // so the objective chain follows the master key and puzzle solves
    // (HUDController.cs Awake / OnDestroy). A missed unsubscribe would leave a
    // destroyed HUD on a static event for the rest of the session, and a
    // missed subscribe would silently stop the objectives refreshing.
    //
    // ObjectiveLinesTests covers the pure chain text; HUDLanguageRefreshTests
    // covers LanguageChanged only. Nothing covers these three subscriptions.
    //
    // Isolation: the handlers are only counted, never invoked, so no event
    // fires. The mutations are the HUD's own Awake (+=, and Instance = this)
    // and OnDestroy (-=). SetUp records every handler list and
    // HUDController.Instance; TearDown destroys the host, restores Instance,
    // then asserts the lists match the snapshot.
    public class HUDObjectiveSubscriptionTests
    {
        const BindingFlags AnyStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        static readonly MethodInfo AwakeMethod =
            typeof(HUDController).GetMethod("Awake", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        GameObject host;
        HUDController hud;
        int itemAddedBefore, itemRemovedBefore, puzzleSolvedBefore;
        HUDController instanceBefore;

        static readonly PropertyInfo InstanceProperty =
            typeof(HUDController).GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        static Delegate EventField(Type owner, string name)
        {
            FieldInfo field = owner.GetField(name, AnyStatic);
            Assert.NotNull(field, owner.Name + "." + name + " backing field missing");
            return (Delegate)field.GetValue(null);
        }

        static int Count(Delegate d) { return d == null ? 0 : d.GetInvocationList().Length; }

        static int CountFor(Delegate d, object target)
        {
            return d == null ? 0 : d.GetInvocationList().Count(h => ReferenceEquals(h.Target, target));
        }

        [SetUp]
        public void SetUp()
        {
            Assert.NotNull(AwakeMethod, "HUDController.Awake missing");
            Assert.NotNull(InstanceProperty, "HUDController.Instance missing");
            instanceBefore = (HUDController)InstanceProperty.GetValue(null);
            itemAddedBefore = Count(EventField(typeof(Inventory), "ItemAdded"));
            itemRemovedBefore = Count(EventField(typeof(Inventory), "ItemRemoved"));
            puzzleSolvedBefore = Count(EventField(typeof(GameEvents), "PuzzleSolved"));

            host = new GameObject("HudObjectiveSubscriptionTest", typeof(HUDController));
            hud = host.GetComponent<HUDController>();
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null) UnityEngine.Object.DestroyImmediate(host); // OnDestroy unsubscribes
            host = null;
            InstanceProperty.GetSetMethod(true).Invoke(null, new object[] { instanceBefore }); // Awake sets Instance
            Assert.AreEqual(itemAddedBefore, Count(EventField(typeof(Inventory), "ItemAdded")), "ItemAdded handler leaked");
            Assert.AreEqual(itemRemovedBefore, Count(EventField(typeof(Inventory), "ItemRemoved")), "ItemRemoved handler leaked");
            Assert.AreEqual(puzzleSolvedBefore, Count(EventField(typeof(GameEvents), "PuzzleSolved")), "PuzzleSolved handler leaked");
        }

        [Test]
        public void Awake_SubscribesObjectiveRefreshToKeyAndPuzzleEvents()
        {
            AwakeMethod.Invoke(hud, null);

            Assert.AreEqual(1, CountFor(EventField(typeof(Inventory), "ItemAdded"), hud));
            Assert.AreEqual(1, CountFor(EventField(typeof(Inventory), "ItemRemoved"), hud));
            Assert.AreEqual(1, CountFor(EventField(typeof(GameEvents), "PuzzleSolved"), hud));
        }

        [Test]
        public void OnDestroy_RemovesEveryObjectiveSubscription()
        {
            AwakeMethod.Invoke(hud, null);
            UnityEngine.Object.DestroyImmediate(host);
            host = null;

            Assert.AreEqual(itemAddedBefore, Count(EventField(typeof(Inventory), "ItemAdded")));
            Assert.AreEqual(itemRemovedBefore, Count(EventField(typeof(Inventory), "ItemRemoved")));
            Assert.AreEqual(puzzleSolvedBefore, Count(EventField(typeof(GameEvents), "PuzzleSolved")));
        }
    }
}
