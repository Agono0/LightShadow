using System;
using System.Reflection;
using LightShadow.Audio;
using LightShadow.Core;
using LightShadow.Story;
using LightShadow.UI;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    // SettingsPanelController's private row-label builders: the accessibility
    // rows map store indices to localized labels, the shadows row marks the
    // tier-following state, and the volume rows are plain rounded integers.
    // Store values and language are restored in TearDown.
    public class SettingsRowLabelTests
    {
        static readonly MethodInfo SpeedLabelMethod = typeof(SettingsPanelController)
            .GetMethod("AccessibilitySpeedLabel", BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo SizeLabelMethod = typeof(SettingsPanelController)
            .GetMethod("AccessibilitySizeLabel", BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo ColorblindLabelMethod = typeof(SettingsPanelController)
            .GetMethod("AccessibilityColorblindLabel", BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo ShadowsLabelMethod = typeof(SettingsPanelController)
            .GetMethod("ShadowsValueLabel", BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo VolumeLabelMethod = typeof(SettingsPanelController)
            .GetMethod("VolumeSliderLabel", BindingFlags.NonPublic | BindingFlags.Static);

        int originalSpeed;
        int originalSize;
        int originalColorblind;
        GameLanguage originalLanguage;

        [SetUp]
        public void SetUp()
        {
            originalSpeed = AccessibilityStore.TextSpeed;
            originalSize = AccessibilityStore.SubtitleSize;
            originalColorblind = AccessibilityStore.ColorblindMode;
            originalLanguage = Localization.Current;
            Localization.SetLanguage(GameLanguage.English);
        }

        [TearDown]
        public void TearDown()
        {
            AccessibilityStore.SetTextSpeed(originalSpeed);
            AccessibilityStore.SetSubtitleSize(originalSize);
            AccessibilityStore.SetColorblindMode(originalColorblind);
            Localization.SetLanguage(originalLanguage);
        }

        static string Label(MethodInfo method, params object[] args)
        {
            return (string)method.Invoke(null, args);
        }

        [Test]
        public void SpeedLabel_MapsAllFourIndices()
        {
            string[] expected = { "Slow", "Normal", "Fast", "Instant" };
            for (int i = 0; i < expected.Length; i++)
            {
                AccessibilityStore.SetTextSpeed(i);
                Assert.AreEqual(expected[i], Label(SpeedLabelMethod), "speed index " + i);
            }
        }

        [Test]
        public void SizeLabel_MapsAllThreeIndices()
        {
            string[] expected = { "Small", "Normal", "Large" };
            for (int i = 0; i < expected.Length; i++)
            {
                AccessibilityStore.SetSubtitleSize(i);
                Assert.AreEqual(expected[i], Label(SizeLabelMethod), "size index " + i);
            }
        }

        [Test]
        public void ColorblindLabel_MapsAllThreeModes()
        {
            string[] expected = { "Standard", "Blue-Orange", "High contrast" };
            for (int i = 0; i < expected.Length; i++)
            {
                AccessibilityStore.SetColorblindMode(i);
                Assert.AreEqual(expected[i], Label(ColorblindLabelMethod), "colorblind mode " + i);
            }
        }

        [Test]
        public void SpeedLabel_InArabic_DoesNotStayEnglish()
        {
            // The labels route through StoryStrings.Tr, which carries Arabic
            // table entries for every one of these keys; a missing entry would
            // leave the English word on screen.
            Localization.SetLanguage(GameLanguage.Arabic);
            AccessibilityStore.SetTextSpeed(3);
            string label = Label(SpeedLabelMethod);
            Assert.IsFalse(string.IsNullOrEmpty(label));
            Assert.AreNotEqual("Instant", label);
        }

        [Test]
        public void ShadowsLabel_MarksTierFollowingState()
        {
            string live = GameSettings.ShadowsLabel();
            string label = Label(ShadowsLabelMethod);
            if (GameSettings.ShadowsIndex < 0)
            {
                StringAssert.EndsWith(" · tier", label);
                StringAssert.StartsWith(live, label);
            }
            else
            {
                Assert.AreEqual(live, label);
            }
        }

        [Test]
        public void VolumeLabel_IsPlainIntegerForEveryCategory()
        {
            foreach (VolumeCategories.Category category in Enum.GetValues(typeof(VolumeCategories.Category)))
            {
                string label = Label(VolumeLabelMethod, category);
                StringAssert.IsMatch(@"^\d+$", label, category.ToString());
            }
        }
    }
}
