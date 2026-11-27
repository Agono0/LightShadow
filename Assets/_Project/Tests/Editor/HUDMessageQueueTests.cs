using System.Collections;
using System.Reflection;
using LightShadow.Core;
using LightShadow.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Tests.Editor
{
    // HUD chalk-message queue: QueueMessage shows immediately when idle and
    // enqueues while a message is up; Update hands off on timer expiry;
    // ShowMessage clears queued beats. These tests assert the REAL current
    // behavior, including that a handoff routes through ShowMessage and
    // therefore clears any deeper queue.
    public class HUDMessageQueueTests
    {
        static readonly MethodInfo AwakeMethod =
            typeof(HUDController).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo UpdateMethod =
            typeof(HUDController).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo MessageTimerField =
            typeof(HUDController).GetField("messageTimer", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo MessageQueueField =
            typeof(HUDController).GetField("messageQueue", BindingFlags.NonPublic | BindingFlags.Instance);

        GameObject host;
        HUDController hud;
        Text message;
        GameLanguage originalLanguage;

        [SetUp]
        public void SetUp()
        {
            originalLanguage = Localization.Current;
            Localization.SetLanguage(GameLanguage.English);

            host = new GameObject("HudQueueTest", typeof(HUDController));
            hud = host.GetComponent<HUDController>();
            message = new GameObject("Message", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            hud.messageText = message;
            AwakeMethod.Invoke(hud, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null) Object.DestroyImmediate(host); // OnDestroy unsubscribes
            Localization.SetLanguage(originalLanguage);
        }

        int QueueCount => ((ICollection)MessageQueueField.GetValue(hud)).Count;

        void ExpireTimer()
        {
            MessageTimerField.SetValue(hud, 0.00001f);
            UpdateMethod.Invoke(hud, null);
        }

        [Test]
        public void QueueMessage_WhenIdle_ShowsImmediatelyWithoutEnqueuing()
        {
            hud.QueueMessage("Beat one", 4f);
            Assert.AreEqual("Beat one", message.text);
            Assert.AreEqual(0, QueueCount);
        }

        [Test]
        public void QueueMessage_WhenShowing_EnqueuesWithoutReplacing()
        {
            hud.ShowMessage("Current", 4f);
            hud.QueueMessage("Next", 4f);
            Assert.AreEqual("Current", message.text);
            Assert.AreEqual(1, QueueCount);
        }

        [Test]
        public void TimerExpiry_HandsOffToNextQueuedBeat()
        {
            hud.ShowMessage("Current", 4f);
            hud.QueueMessage("Next", 4f);
            ExpireTimer();
            Assert.AreEqual("Next", message.text);
        }

        [Test]
        public void ShowMessage_ClearsQueuedBeats()
        {
            hud.ShowMessage("Current", 4f);
            hud.QueueMessage("Beat B", 4f);
            hud.QueueMessage("Beat C", 4f);
            hud.ShowMessage("Urgent", 4f);
            Assert.AreEqual("Urgent", message.text);
            Assert.AreEqual(0, QueueCount);
        }

        [Test]
        public void Handoff_ShowsNextBeat_AndClearsDeeperQueue_CurrentBehavior()
        {
            // The handoff routes through ShowMessage, whose clear drops any
            // beats queued behind the one being shown. Pins the CURRENT
            // behavior; if the queue is ever made lossless this test should
            // change with it deliberately.
            hud.ShowMessage("Current", 4f);
            hud.QueueMessage("Beat B", 4f);
            hud.QueueMessage("Beat C", 4f);
            ExpireTimer();
            Assert.AreEqual("Beat B", message.text);
            Assert.AreEqual(0, QueueCount);
        }
    }
}
