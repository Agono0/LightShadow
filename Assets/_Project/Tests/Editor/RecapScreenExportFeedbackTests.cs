using System.Reflection;
using LightShadow.UI;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    // audit v3 5.6: SAVE PNG used to complete (or fail) silently. The export
    // now reports through an on-panel feedback label - the saved filename on
    // success, a failure message otherwise.
    public class RecapScreenExportFeedbackTests
    {
        static readonly MethodInfo AwakeMethod =
            typeof(RecapScreen).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo SetFeedbackMethod =
            typeof(RecapScreen).GetMethod("SetExportFeedback", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo SavedMessageMethod =
            typeof(RecapScreen).GetMethod("FormatSavedMessage", BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo FailedMessageMethod =
            typeof(RecapScreen).GetMethod("FormatFailedMessage", BindingFlags.NonPublic | BindingFlags.Static);

        GameObject host;
        RecapScreen screen;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("RecapScreenTest", typeof(RecapScreen));
            screen = host.GetComponent<RecapScreen>();
            AwakeMethod.Invoke(screen, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null) Object.DestroyImmediate(host);
        }

        [Test]
        public void Awake_BuildsEmptyFeedbackLabel()
        {
            Assert.NotNull(screen.exportFeedbackText,
                "a runtime-built label must exist without a SceneBuilderStory edit");
            Assert.AreEqual("", screen.exportFeedbackText.text);
        }

        [Test]
        public void SavedMessage_ContainsFileName()
        {
            string message = (string)SavedMessageMethod.Invoke(null,
                new object[] { "recap_20261127_233000.png" });
            StringAssert.Contains("recap_20261127_233000.png", message);
        }

        [Test]
        public void FailedMessage_IsNonEmpty()
        {
            string message = (string)FailedMessageMethod.Invoke(null, null);
            Assert.IsFalse(string.IsNullOrEmpty(message));
        }

        [Test]
        public void SetExportFeedback_ShowsMessageOnLabel()
        {
            string message = (string)SavedMessageMethod.Invoke(null, new object[] { "recap_x.png" });
            SetFeedbackMethod.Invoke(screen, new object[] { message });
            Assert.AreEqual(message, screen.exportFeedbackText.text);
        }
    }
}
