using System.Reflection;
using LightShadow.Core;
using LightShadow.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Tests.Editor
{
    // HUD review (5.4b family): the HUD subscribes to LanguageChanged but used
    // to re-font only - the prompt, the chalk message and the objectives list
    // stayed in the previous language until re-triggered. ApplyLanguageUI now
    // re-renders the cached English sources in place.
    public class HUDLanguageRefreshTests
    {
        static readonly MethodInfo AwakeMethod =
            typeof(HUDController).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);

        GameObject host;
        HUDController hud;
        Text prompt;
        Text message;
        Text objectives;
        GameObject objectivesPanel;
        GameLanguage originalLanguage;

        [SetUp]
        public void SetUp()
        {
            originalLanguage = Localization.Current;
            Localization.SetLanguage(GameLanguage.English);

            host = new GameObject("HudLanguageTest", typeof(HUDController));
            hud = host.GetComponent<HUDController>();

            prompt = MakeText("Prompt");
            message = MakeText("Message");
            objectivesPanel = new GameObject("ObjectivesPanel");
            objectives = MakeText("Objectives", objectivesPanel.transform);
            objectivesPanel.SetActive(false);

            hud.promptText = prompt;
            hud.messageText = message;
            hud.objectivesPanel = objectivesPanel;
            hud.objectivesText = objectives;

            AwakeMethod.Invoke(hud, null); // subscribes to LanguageChanged
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null) Object.DestroyImmediate(host); // OnDestroy unsubscribes
            Localization.SetLanguage(originalLanguage);
        }

        static Text MakeText(string name, Transform parent = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            if (parent != null) go.transform.SetParent(parent, false);
            return go.GetComponent<Text>();
        }

        [Test]
        public void LanguageSwitch_RerendersVisiblePrompt()
        {
            hud.SetPrompt("Interact");
            Localization.SetLanguage(GameLanguage.Arabic);
            Assert.AreEqual(Localization.TrPresent("Interact"), prompt.text);
        }

        [Test]
        public void LanguageSwitch_RerendersShownMessage()
        {
            hud.ShowMessage("The crank light died. Find a battery.", 4f);
            Localization.SetLanguage(GameLanguage.Arabic);
            Assert.AreEqual(Localization.TrPresent("The crank light died. Find a battery."), message.text);
        }

        [Test]
        public void LanguageSwitch_ClearedPromptStaysEmpty()
        {
            hud.SetPrompt("Interact");
            hud.SetPrompt("");
            Localization.SetLanguage(GameLanguage.Arabic);
            Assert.AreEqual("", prompt.text);
        }

        [Test]
        public void LanguageSwitch_KeepsCurrentFontOnPromptAndMessage()
        {
            hud.SetPrompt("Interact");
            hud.ShowMessage("Hello", 4f);
            Localization.SetLanguage(GameLanguage.Arabic);
            Assert.AreSame(Localization.CurrentFont, prompt.font);
            Assert.AreSame(Localization.CurrentFont, message.font);
        }
    }
}
