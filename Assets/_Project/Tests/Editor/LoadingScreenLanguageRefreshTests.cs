using System;
using System.Reflection;
using LightShadow.Core;
using LightShadow.Story;
using LightShadow.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Tests.Editor
{
    // audit v3 5.4 (language refresh): the loading canvas persists across
    // scene loads, so Awake must subscribe to Localization.LanguageChanged and
    // refresh caption, tip and the loading label in place (text AND font),
    // instead of leaving the previous language on screen.
    public class LoadingScreenLanguageRefreshTests
    {
        const string CaptionKey = "DEMOLITION AT DAWN";
        const string TipKey = "Knowledge is the only light that follows a person into the dark.";

        static readonly MethodInfo AwakeMethod =
            typeof(LoadingScreen).GetMethod("Awake", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo OnDestroyMethod =
            typeof(LoadingScreen).GetMethod("OnDestroy", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo RefreshMethod =
            typeof(LoadingScreen).GetMethod("RefreshLanguage", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo CaptionField =
            typeof(LoadingScreen).GetField("captionText", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo TipField =
            typeof(LoadingScreen).GetField("tipText", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo LanguageChangedField =
            typeof(Localization).GetField("LanguageChanged",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        GameObject host;
        LoadingScreen screen;
        GameLanguage originalLanguage;

        [SetUp]
        public void SetUp()
        {
            originalLanguage = Localization.Current;
            host = new GameObject("LoadingScreenLanguageTest", typeof(LoadingScreen));
            screen = host.GetComponent<LoadingScreen>();
            AwakeMethod.Invoke(screen, null); // builds the UI and subscribes
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null) UnityEngine.Object.DestroyImmediate(host); // runs OnDestroy: unsubscribes
            Localization.SetLanguage(originalLanguage);
        }

        Text Caption => (Text)CaptionField.GetValue(screen);
        Text Tip => (Text)TipField.GetValue(screen);

        static Delegate Handlers() => LanguageChangedField.GetValue(null) as Delegate;

        [Test]
        public void Awake_SubscribesRefresh_OnDestroy_Unsubscribes()
        {
            Assert.NotNull(Handlers());
            Assert.IsTrue(Array.Exists(Handlers().GetInvocationList(),
                handler => ReferenceEquals(handler.Target, screen)
                    && handler.Method.Name == "RefreshLanguage"),
                "Awake must subscribe RefreshLanguage to Localization.LanguageChanged");

            OnDestroyMethod.Invoke(screen, null);

            // Other live LoadingScreens from other tests may still be
            // subscribed; only handlers bound to THIS screen count.
            Delegate remaining = Handlers();
            int left = remaining == null ? 0
                : Array.FindAll(remaining.GetInvocationList(), handler => ReferenceEquals(handler.Target, screen)).Length;
            Assert.AreEqual(0, left, "OnDestroy must unsubscribe RefreshLanguage");
        }

        [Test]
        public void LanguageChanged_RetranslatesCaptionAndTip()
        {
            Localization.SetLanguage(GameLanguage.Arabic);
            Assert.AreEqual(StoryStrings.Tr(CaptionKey), Caption.text);
            Assert.AreEqual(StoryStrings.Tr(TipKey), Tip.text);
        }

        [Test]
        public void LanguageChanged_RefreshesFontsToCurrentLanguage()
        {
            Localization.SetLanguage(GameLanguage.Arabic);
            Assert.AreSame(Localization.CurrentFont, Caption.font);
            Assert.AreSame(Localization.CurrentFont, Tip.font);

            Localization.SetLanguage(GameLanguage.English);
            Assert.AreSame(Localization.CurrentFont, Caption.font);
            Assert.AreSame(Localization.CurrentFont, Tip.font);
        }

        [Test]
        public void RefreshLanguage_WithoutBuiltUI_DoesNotThrow()
        {
            var bareGo = new GameObject("Bare", typeof(LoadingScreen));
            try
            {
                var bare = bareGo.GetComponent<LoadingScreen>();
                Assert.DoesNotThrow(() => RefreshMethod.Invoke(bare, null));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bareGo);
            }
        }
    }
}
