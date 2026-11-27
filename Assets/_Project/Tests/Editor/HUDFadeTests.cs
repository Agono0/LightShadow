using System.Reflection;
using LightShadow.Core;
using LightShadow.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Tests.Editor
{
    // FadeToBlack must reset the fade base colour to black: an interrupted
    // hurt flash leaves the image red and only that coroutine's normal end
    // restores black, so without the reset a black fade starting mid-flash
    // fades to red. The reset is synchronous, so it is edit-mode testable;
    // the fade lerp itself needs play mode and is not covered here.
    public class HUDFadeTests
    {
        static readonly MethodInfo AwakeMethod =
            typeof(HUDController).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);

        GameObject host;
        HUDController hud;
        Image fade;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("HudFadeTest", typeof(HUDController));
            hud = host.GetComponent<HUDController>();
            fade = new GameObject("Fade", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            hud.fadeImage = fade;
            AwakeMethod.Invoke(hud, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null) Object.DestroyImmediate(host);
        }

        [Test]
        public void FadeToBlack_ResetsRedBaseToBlack()
        {
            fade.color = new Color(0.55f, 0.05f, 0.03f, 0.38f); // interrupted hurt flash
            hud.ResetFadeBaseToBlack(); // the synchronous half of FadeToBlack; the coroutine is not started in EditMode
            Assert.AreEqual(0f, fade.color.r);
            Assert.AreEqual(0f, fade.color.g);
            Assert.AreEqual(0f, fade.color.b);
        }

        [Test]
        public void FadeToBlack_PreservesCurrentAlpha()
        {
            fade.color = new Color(0.55f, 0.05f, 0.03f, 0.38f);
            hud.ResetFadeBaseToBlack(); // the synchronous half of FadeToBlack; the coroutine is not started in EditMode
            Assert.AreEqual(0.38f, fade.color.a, 0.0001f);
        }
    }
}
