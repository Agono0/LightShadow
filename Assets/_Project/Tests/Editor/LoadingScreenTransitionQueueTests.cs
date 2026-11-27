using System.Reflection;
using LightShadow.UI;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    // audit v3 5.4 (latest-wins): a Transition requested while another is in
    // flight must be QUEUED, not dropped; a newer request replaces the older
    // queued one so the LAST requested scene wins. The queue path runs before
    // any coroutine or scene load, so it is exercisable in edit mode.
    public class LoadingScreenTransitionQueueTests
    {
        static readonly FieldInfo PendingSceneField =
            typeof(LoadingScreen).GetField("pendingScene", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo InTransitionField =
            typeof(LoadingScreen).GetField("inTransition", BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo RunTransitionMethod =
            typeof(LoadingScreen).GetMethod("RunTransition", BindingFlags.NonPublic | BindingFlags.Instance);

        GameObject host;
        LoadingScreen screen;

        [SetUp]
        public void SetUp()
        {
            InTransitionField.SetValue(null, false);
            host = new GameObject("LoadingScreenQueueTest", typeof(LoadingScreen));
            screen = host.GetComponent<LoadingScreen>();
        }

        [TearDown]
        public void TearDown()
        {
            InTransitionField.SetValue(null, false);
            if (host != null) Object.DestroyImmediate(host);
        }

        string PendingScene => (string)PendingSceneField.GetValue(screen);

        [Test]
        public void TransitionDuringFlight_IsQueuedNotDropped()
        {
            InTransitionField.SetValue(null, true); // simulate a running transition
            RunTransitionMethod.Invoke(screen, new object[] { "MainMenu" });
            Assert.AreEqual("MainMenu", PendingScene);
        }

        [Test]
        public void NewerTransitionDuringFlight_ReplacesQueuedScene()
        {
            InTransitionField.SetValue(null, true);
            RunTransitionMethod.Invoke(screen, new object[] { "School" });
            RunTransitionMethod.Invoke(screen, new object[] { "MainMenu" });
            Assert.AreEqual("MainMenu", PendingScene,
                "latest request must win over an older queued one");
        }

        [Test]
        public void QueuedTransition_KeepsGuardClosed()
        {
            InTransitionField.SetValue(null, true);
            RunTransitionMethod.Invoke(screen, new object[] { "MainMenu" });
            Assert.IsTrue((bool)InTransitionField.GetValue(null),
                "queuing must not release the guard; the running transition owns it");
        }
    }
}
