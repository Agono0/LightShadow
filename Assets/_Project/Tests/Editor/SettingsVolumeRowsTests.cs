using System.Reflection;
using LightShadow.Audio;
using LightShadow.Core;
using LightShadow.Story;
using LightShadow.UI;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    // Appended Voice (25) and Shadow (26) volume rows: the settings card
    // promises existing row indices never move, so these constants, the
    // private RowKeys table (order + length) and the row -> category mapping
    // are pinned here. No layout or mixer checks: the SceneBuilderStory
    // column formula needs an editor scene rebuild, and Voice/Shadow mixer
    // groups resolve only against the live M_Main asset.
    public class SettingsVolumeRowsTests
    {
        static readonly FieldInfo RowKeysField =
            typeof(SettingsPanelController).GetField("RowKeys", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo RowValueMethod =
            typeof(SettingsPanelController).GetMethod("RowValue", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        // The exact declared order of all 27 rows (page A then page B, with
        // Voice/Shadow appended last - indices 0..24 must never move).
        static readonly string[] ExpectedRowKeys =
        {
            "Resolution", "Fullscreen", "Quality", "VSync", "Shadows",
            "Anti-aliasing", "Render scale", "Brightness", "Mouse sensitivity",
            "Language", "Narrator voice", "FPS counter",
            "Text speed", "Subtitle size", "Colourblind palette", "Auto-advance",
            "Skip hold time",
            "Master volume", "Music volume", "Ambience volume", "SFX volume", "UI volume",
            "Touch control size",
            "Sound captions", "Hints",
            "Voice volume", "Shadow volume",
        };

        GameObject host;
        SettingsPanelController controller;
        GameLanguage originalLanguage;
        bool hadVoicePref, hadShadowPref;
        int originalVoicePref, originalShadowPref;
        float originalVoiceLevel, originalShadowLevel;

        [SetUp]
        public void SetUp()
        {
            originalLanguage = Localization.Current;

            hadVoicePref = PlayerPrefs.HasKey("ls_vol_voice");
            hadShadowPref = PlayerPrefs.HasKey("ls_vol_shadow");
            originalVoicePref = PlayerPrefs.GetInt("ls_vol_voice", 6);
            originalShadowPref = PlayerPrefs.GetInt("ls_vol_shadow", 6);
            originalVoiceLevel = VolumeCategories.GetLevel(VolumeCategories.Category.Voice);
            originalShadowLevel = VolumeCategories.GetLevel(VolumeCategories.Category.Shadow);

            // Inactive host: Awake/Refresh never run, so RowValue is exercised
            // in isolation from the full card wiring.
            host = new GameObject("SettingsVolumeRowsTest");
            host.SetActive(false);
            controller = host.AddComponent<SettingsPanelController>();
        }

        [TearDown]
        public void TearDown()
        {
            VolumeCategories.SetLevel(VolumeCategories.Category.Voice, Mathf.RoundToInt(originalVoiceLevel));
            VolumeCategories.SetLevel(VolumeCategories.Category.Shadow, Mathf.RoundToInt(originalShadowLevel));
            RestorePref("ls_vol_voice", hadVoicePref, originalVoicePref);
            RestorePref("ls_vol_shadow", hadShadowPref, originalShadowPref);
            PlayerPrefs.Save();
            Localization.SetLanguage(originalLanguage);
            if (host != null) Object.DestroyImmediate(host);
        }

        static void RestorePref(string key, bool existed, int value)
        {
            if (existed) PlayerPrefs.SetInt(key, value);
            else PlayerPrefs.DeleteKey(key);
        }

        string RowValue(int row)
        {
            return (string)RowValueMethod.Invoke(controller, new object[] { row });
        }

        [Test]
        public void VolumeRows_AreAppendedAfterHintsRow()
        {
            Assert.AreEqual(24, SettingsPanelController.HintsRow);
            Assert.AreEqual(25, SettingsPanelController.VolumeVoiceRow);
            Assert.AreEqual(26, SettingsPanelController.VolumeShadowRow);
        }

        [Test]
        public void TotalRows_Is27()
        {
            Assert.AreEqual(27, SettingsPanelController.TotalRows);
        }

        [Test]
        public void RowKeys_LengthEqualsTotalRows()
        {
            Assert.NotNull(RowKeysField, "RowKeys field not found - reflection seam broken");
            string[] keys = (string[])RowKeysField.GetValue(null);
            Assert.AreEqual(SettingsPanelController.TotalRows, keys.Length);
        }

        [Test]
        public void RowKeys_Order_MatchesDeclaredRows()
        {
            string[] keys = (string[])RowKeysField.GetValue(null);
            Assert.AreEqual(ExpectedRowKeys.Length, keys.Length, "RowKeys length changed - update this expectation deliberately");
            for (int i = 0; i < ExpectedRowKeys.Length; i++)
            {
                Assert.AreEqual(ExpectedRowKeys[i], keys[i], "RowKeys[" + i + "]");
            }
        }

        [Test]
        public void VoiceAndShadowRows_ReadTheirOwnCategory()
        {
            VolumeCategories.SetLevel(VolumeCategories.Category.Voice, 3);
            VolumeCategories.SetLevel(VolumeCategories.Category.Shadow, 8);
            Assert.AreEqual("3", RowValue(SettingsPanelController.VolumeVoiceRow));
            Assert.AreEqual("8", RowValue(SettingsPanelController.VolumeShadowRow));

            // A Shadow write must not move the Voice row (appended switch arms
            // are not off by one), and vice versa.
            VolumeCategories.SetLevel(VolumeCategories.Category.Shadow, 5);
            Assert.AreEqual("3", RowValue(SettingsPanelController.VolumeVoiceRow));
            VolumeCategories.SetLevel(VolumeCategories.Category.Voice, 7);
            Assert.AreEqual("5", RowValue(SettingsPanelController.VolumeShadowRow));
        }

        [Test]
        public void VoiceAndShadowLabels_HaveArabicTranslations()
        {
            // StoryStrings carries AR entries for both keys; a missing entry
            // would leave the English label on screen in Arabic mode.
            Localization.SetLanguage(GameLanguage.Arabic);
            Assert.AreNotEqual("Voice volume", StoryStrings.Tr("Voice volume"));
            Assert.AreNotEqual("Shadow volume", StoryStrings.Tr("Shadow volume"));
        }
    }
}
