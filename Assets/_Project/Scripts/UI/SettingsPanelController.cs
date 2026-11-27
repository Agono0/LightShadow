using LightShadow.Core;
using LightShadow.Audio;
using LightShadow.Story;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // Shared settings card used by both the main menu and the pause menu.
    //
    // Graphics refactor (post-premium pass): GameSettings is the single writer
    // of graphics state, and every graphics row below reads back the LIVE
    // applied value (Screen / QualitySettings / the active URP asset) instead
    // of a saved intention — the card can no longer drift from reality. The
    // old Mobile/PC "Quality" row and the page-B "Quality tier" row are merged
    // into one Quality row (Auto → Low → Medium → High → Ultra), and the
    // Shadows / Anti-aliasing / Render-scale rows are explicit overrides that
    // default to following the tier (Reset returns them to the tier).
    //
    // Rows 12-21 (page B, behind MORE OPTIONS) keep the story pass's
    // accessibility rows and the five volume sliders (D15 code mixer). Scenes
    // that have not been rebuilt keep the original row wiring and behave as
    // before, minus the graphics rows that now show live values.
    public class SettingsPanelController : MonoBehaviour
    {
        // ---- Page A: graphics + interface --------------------------------
        public const int ResolutionRow = 0;
        public const int FullscreenRow = 1;
        public const int QualityRow = 2;      // quality tier: Auto/Low/Medium/High/Ultra
        public const int VSyncRow = 3;
        public const int ShadowsRow = 4;
        public const int MsaaRow = 5;
        public const int RenderScaleRow = 6;
        public const int BrightnessRow = 7;
        public const int SensitivityRow = 8;
        public const int LanguageRow = 9;
        public const int NarratorRow = 10;
        public const int FpsCounterRow = 11;

        // ---- Page B: accessibility + audio (story pass) -------------------
        public const int TextSpeedRow = 12;
        public const int SubtitleSizeRow = 13;
        public const int ColorblindRow = 14;
        public const int AutoAdvanceRow = 15;
        public const int SkipHoldRow = 16;
        public const int VolumeMasterRow = 17;
        public const int VolumeMusicRow = 18;
        public const int VolumeAmbienceRow = 19;
        public const int VolumeSfxRow = 20;
        public const int VolumeUiRow = 21;

        public const int PageARows = 12;
        public const int TotalRows = 22;

        [Header("Wiring")]
        public GameObject panel;
        public Text title;
        public Text[] rowLabels = new Text[TotalRows];
        public Text[] rowValues = new Text[TotalRows];
        public Button[] prevButtons = new Button[TotalRows];
        public Button[] nextButtons = new Button[TotalRows];
        public Button resetButton;
        public Button closeButton;
        public Text resetLabel;
        public Text closeLabel;

        [Header("Story pass (built by SceneBuilderStory)")]
        public Button moreButton;
        public Text moreLabel;

        public bool IsOpen => panel != null && panel.activeSelf;

        int currentPage;
        bool storyRowsWired;

        static readonly string[] RowKeys =
        {
            "Resolution", "Fullscreen", "Quality", "VSync", "Shadows",
            "Anti-aliasing", "Render scale", "Brightness", "Mouse sensitivity",
            "Language", "Narrator voice", "FPS counter",
            // Page B (story pass; hidden unless SceneBuilderStory built them).
            "Text speed", "Subtitle size", "Colourblind palette", "Auto-advance",
            "Skip hold time",
            "Master volume", "Music volume", "Ambience volume", "SFX volume", "UI volume",
        };

        // Rows that only make sense where a picked resolution exists (audit
        // A5). Page A reflows around them on mobile; page B is untouched.
        static bool HiddenOnMobile(int row)
        {
            return Application.isMobilePlatform && (row == ResolutionRow || row == FullscreenRow);
        }

        // The builder lays page A rows out at y = -120 - i * 64 (prev/next sit
        // 3 lower). Re-run the same formula over the rows that stay visible.
        void ReflowPageAForMobile()
        {
            int visible = 0;
            for (int i = 0; i < PageARows; i++)
            {
                if (HiddenOnMobile(i)) continue;
                float y = -120f - visible * 64f;
                if (rowLabels != null && i < rowLabels.Length && rowLabels[i] != null)
                {
                    rowLabels[i].rectTransform.anchoredPosition = new Vector2(rowLabels[i].rectTransform.anchoredPosition.x, y);
                }
                if (rowValues != null && i < rowValues.Length && rowValues[i] != null)
                {
                    rowValues[i].rectTransform.anchoredPosition = new Vector2(rowValues[i].rectTransform.anchoredPosition.x, y);
                }
                if (prevButtons != null && i < prevButtons.Length && prevButtons[i] != null)
                {
                    prevButtons[i].GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(prevButtons[i].GetComponent<RectTransform>().anchoredPosition.x, y + 3f);
                }
                if (nextButtons != null && i < nextButtons.Length && nextButtons[i] != null)
                {
                    nextButtons[i].GetComponent<RectTransform>().anchoredPosition =
                        new Vector2(nextButtons[i].GetComponent<RectTransform>().anchoredPosition.x, y + 3f);
                }
                visible++;
            }
        }

        void Awake()
        {
            if (panel != null) panel.SetActive(false);

            for (int i = 0; i < prevButtons.Length; i++)
            {
                int index = i;
                if (prevButtons[i] != null) prevButtons[i].onClick.AddListener(() => Previous(index));
                if (nextButtons[i] != null) nextButtons[i].onClick.AddListener(() => Next(index));
            }

            if (resetButton != null) resetButton.onClick.AddListener(GameSettings.ResetGraphics);
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (moreButton != null) moreButton.onClick.AddListener(TogglePage);

            // A5: the Resolution and Fullscreen rows are no-ops on mobile (the
            // game renders native-only there since the A1 stretch fix). Hide
            // them and close the gap so page A has no dead rows on phones.
            if (Application.isMobilePlatform) ReflowPageAForMobile();
            ApplyPageVisibility();

            Localization.LanguageChanged += Refresh;
            GameSettings.Changed += Refresh;
            Refresh();
        }

        void OnDestroy()
        {
            Localization.LanguageChanged -= Refresh;
            GameSettings.Changed -= Refresh;
        }

        // SceneBuilderStory calls this right after extending the wiring arrays
        // inside an already-open editor scene (Awake has already run there).
        // At runtime, Awake's own loop covers the extended arrays, so this is
        // only an editor-time convenience and is guarded against duplication.
        public void BindStoryRows()
        {
            if (storyRowsWired) return;
            storyRowsWired = true;

            for (int i = PageARows; i < prevButtons.Length; i++)
            {
                int index = i;
                if (prevButtons[i] != null) prevButtons[i].onClick.AddListener(() => Previous(index));
                if (nextButtons[i] != null) nextButtons[i].onClick.AddListener(() => Next(index));
            }
            if (moreButton != null) moreButton.onClick.AddListener(TogglePage);
        }

        public void TogglePage()
        {
            SetPage(currentPage == 0 ? 1 : 0);
        }

        public void SetPage(int page)
        {
            currentPage = Mathf.Clamp(page, 0, 1);
            ApplyPageVisibility();
            Refresh();
        }

        void ApplyPageVisibility()
        {
            int bound = Mathf.Min(rowLabels != null ? rowLabels.Length : 0,
                rowValues != null ? rowValues.Length : 0);
            for (int i = 0; i < bound; i++)
            {
                bool onThisPage = (i < PageARows) == (currentPage == 0) && !HiddenOnMobile(i);
                if (rowLabels[i] != null) rowLabels[i].gameObject.SetActive(onThisPage);
                if (rowValues[i] != null) rowValues[i].gameObject.SetActive(onThisPage);
                if (prevButtons != null && i < prevButtons.Length && prevButtons[i] != null)
                {
                    prevButtons[i].gameObject.SetActive(onThisPage);
                }
                if (nextButtons != null && i < nextButtons.Length && nextButtons[i] != null)
                {
                    nextButtons[i].gameObject.SetActive(onThisPage);
                }
            }

            if (moreLabel != null)
            {
                moreLabel.text = Localization.TrPresent(currentPage == 0 ? "MORE OPTIONS" : "BASIC OPTIONS");
                moreLabel.font = Localization.CurrentFont;
            }
        }

        public void Show()
        {
            if (panel != null) panel.SetActive(true);
            ApplyPageVisibility();
            Refresh();
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        public void Toggle()
        {
            if (IsOpen) Hide();
            else Show();
        }

        void Previous(int row)
        {
            switch (row)
            {
                case ResolutionRow: GameSettings.SetResolutionIndex(GameSettings.ResolutionIndex - 1); break;
                case FullscreenRow: GameSettings.ToggleFullscreen(); break;
                case QualityRow: GameSettings.SetTier(PreviousTier()); break;
                case VSyncRow: GameSettings.ToggleVSync(); break;
                case ShadowsRow: GameSettings.SetShadows(PreviousIndex(GameSettings.ShadowsIndex, 4)); break;
                case MsaaRow: GameSettings.SetMsaa(PreviousIndex(GameSettings.MsaaIndex, 3)); break;
                case RenderScaleRow: GameSettings.SetRenderScale(PreviousIndex(GameSettings.RenderScaleIndex, 6)); break;
                case BrightnessRow: GameSettings.SetBrightness(GameSettings.Brightness - 0.1f); break;
                case SensitivityRow: GameSettings.SetSensitivity(GameSettings.Sensitivity - 0.1f); break;
                case LanguageRow: GameSettings.ToggleLanguage(); break;
                case NarratorRow: GameSettings.ToggleNarrator(); break;
                case FpsCounterRow: GameSettings.ToggleFps(); break;

                // ---- story pass (page B) ----
                case TextSpeedRow: AccessibilityStore.SetTextSpeed(AccessibilityStore.TextSpeed - 1); break;
                case SubtitleSizeRow: AccessibilityStore.SetSubtitleSize(AccessibilityStore.SubtitleSize - 1); break;
                case ColorblindRow: AccessibilityStore.SetColorblindMode(AccessibilityStore.ColorblindMode - 1); break;
                case AutoAdvanceRow: AccessibilityStore.ToggleAutoAdvance(); break;
                case SkipHoldRow: AccessibilityStore.SetSkipHold(AccessibilityStore.SkipHoldIndex - 1); break;
                case VolumeMasterRow: VolumeCategories.SetLevel(VolumeCategories.Category.Master, Mathf.RoundToInt(VolumeCategories.GetLevel(VolumeCategories.Category.Master)) - 1); break;
                case VolumeMusicRow: VolumeCategories.SetLevel(VolumeCategories.Category.Music, Mathf.RoundToInt(VolumeCategories.GetLevel(VolumeCategories.Category.Music)) - 1); break;
                case VolumeAmbienceRow: VolumeCategories.SetLevel(VolumeCategories.Category.Ambience, Mathf.RoundToInt(VolumeCategories.GetLevel(VolumeCategories.Category.Ambience)) - 1); break;
                case VolumeSfxRow: VolumeCategories.SetLevel(VolumeCategories.Category.Sfx, Mathf.RoundToInt(VolumeCategories.GetLevel(VolumeCategories.Category.Sfx)) - 1); break;
                case VolumeUiRow: VolumeCategories.SetLevel(VolumeCategories.Category.Ui, Mathf.RoundToInt(VolumeCategories.GetLevel(VolumeCategories.Category.Ui)) - 1); break;
            }
        }

        void Next(int row)
        {
            switch (row)
            {
                case ResolutionRow: GameSettings.SetResolutionIndex(GameSettings.ResolutionIndex + 1); break;
                case FullscreenRow: GameSettings.ToggleFullscreen(); break;
                case QualityRow: GameSettings.SetTier(NextTier()); break;
                case VSyncRow: GameSettings.ToggleVSync(); break;
                case ShadowsRow: GameSettings.SetShadows(NextIndex(GameSettings.ShadowsIndex, 4)); break;
                case MsaaRow: GameSettings.SetMsaa(NextIndex(GameSettings.MsaaIndex, 3)); break;
                case RenderScaleRow: GameSettings.SetRenderScale(NextIndex(GameSettings.RenderScaleIndex, 6)); break;
                case BrightnessRow: GameSettings.SetBrightness(GameSettings.Brightness + 0.1f); break;
                case SensitivityRow: GameSettings.SetSensitivity(GameSettings.Sensitivity + 0.1f); break;
                case LanguageRow: GameSettings.ToggleLanguage(); break;
                case NarratorRow: GameSettings.ToggleNarrator(); break;
                case FpsCounterRow: GameSettings.ToggleFps(); break;

                // ---- story pass (page B) ----
                case TextSpeedRow: AccessibilityStore.SetTextSpeed(AccessibilityStore.TextSpeed + 1); break;
                case SubtitleSizeRow: AccessibilityStore.SetSubtitleSize(AccessibilityStore.SubtitleSize + 1); break;
                case ColorblindRow: AccessibilityStore.SetColorblindMode(AccessibilityStore.ColorblindMode + 1); break;
                case AutoAdvanceRow: AccessibilityStore.ToggleAutoAdvance(); break;
                case SkipHoldRow: AccessibilityStore.SetSkipHold(AccessibilityStore.SkipHoldIndex + 1); break;
                case VolumeMasterRow: VolumeCategories.SetLevel(VolumeCategories.Category.Master, Mathf.RoundToInt(VolumeCategories.GetLevel(VolumeCategories.Category.Master)) + 1); break;
                case VolumeMusicRow: VolumeCategories.SetLevel(VolumeCategories.Category.Music, Mathf.RoundToInt(VolumeCategories.GetLevel(VolumeCategories.Category.Music)) + 1); break;
                case VolumeAmbienceRow: VolumeCategories.SetLevel(VolumeCategories.Category.Ambience, Mathf.RoundToInt(VolumeCategories.GetLevel(VolumeCategories.Category.Ambience)) + 1); break;
                case VolumeSfxRow: VolumeCategories.SetLevel(VolumeCategories.Category.Sfx, Mathf.RoundToInt(VolumeCategories.GetLevel(VolumeCategories.Category.Sfx)) + 1); break;
                case VolumeUiRow: VolumeCategories.SetLevel(VolumeCategories.Category.Ui, Mathf.RoundToInt(VolumeCategories.GetLevel(VolumeCategories.Category.Ui)) + 1); break;
            }
        }

        // Tier cycle: Auto(-1) → Low(0) → Medium(1) → High(2) → Ultra(3) → Auto.
        static int PreviousTier()
        {
            int tier = PlayerPrefs.GetInt("ls_qualityTier", -1);
            return tier - 1;
        }

        static int NextTier()
        {
            int tier = PlayerPrefs.GetInt("ls_qualityTier", -1);
            return tier + 1;
        }

        // Fine overrides cycle -1 (follow tier) → 0 → … → max-1 → -1.
        static int PreviousIndex(int current, int count)
        {
            return current <= -1 ? count - 1 : current - 1;
        }

        static int NextIndex(int current, int count)
        {
            return current >= count - 1 ? -1 : current + 1;
        }

        public void Refresh()
        {
            bool arabic = Localization.Current == GameLanguage.Arabic;
            Font font = Localization.CurrentFont;

            // Colours are re-asserted on every refresh so the card always stays
            // readable, whatever state a previous language/font swap left behind.
            Color labelColor = new Color(0.93f, 0.91f, 0.82f); // parchment
            Color valueColor = new Color(1f, 0.94f, 0.75f);    // bright warm amber
            Color titleColor = new Color(0.99f, 0.95f, 0.86f);
            Color buttonColor = new Color(1f, 0.9f, 0.7f);

            if (title != null)
            {
                title.text = Localization.TrPresent("SETTINGS");
                title.font = font;
                title.color = titleColor;
            }

            if (resetLabel != null)
            {
                resetLabel.text = Localization.TrPresent("Reset defaults");
                resetLabel.font = font;
                resetLabel.color = buttonColor;
            }

            if (closeLabel != null)
            {
                closeLabel.text = Localization.TrPresent("Close");
                closeLabel.font = font;
                closeLabel.color = buttonColor;
            }

            for (int i = 0; i < RowKeys.Length; i++)
            {
                if (rowLabels != null && i < rowLabels.Length && rowLabels[i] != null)
                {
                    rowLabels[i].text = Localization.TrPresent(RowKeys[i]);
                    rowLabels[i].font = font;
                    rowLabels[i].color = labelColor;
                    rowLabels[i].alignment = arabic ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
                }

                if (rowValues != null && i < rowValues.Length && rowValues[i] != null)
                {
                    rowValues[i].text = RowValue(i);
                    rowValues[i].font = font;
                    rowValues[i].color = valueColor;
                }
            }
        }

        string RowValue(int row)
        {
            switch (row)
            {
                // Live values: every graphics label reads what is applied now.
                case ResolutionRow: return Localization.Present(GameSettings.ResolutionLabel());
                case FullscreenRow: return Localization.Present(GameSettings.FullscreenLabel());
                case QualityRow: return Localization.Present(GameSettings.TierLabel());
                case VSyncRow: return Localization.Present(GameSettings.VSyncLabel());
                case ShadowsRow: return Localization.Present(ShadowsValueLabel());
                case MsaaRow: return Localization.Present(GameSettings.MsaaLabel());
                case RenderScaleRow: return Localization.Present(GameSettings.RenderScaleLabel());
                case BrightnessRow: return Localization.Present(GameSettings.BrightnessLabel());
                case SensitivityRow: return Localization.Present(GameSettings.SensitivityLabel());
                case LanguageRow: return Localization.LanguageName(Localization.Current);
                case NarratorRow:
                    return GameSettings.NarratorEnabled ? Localization.TrPresent("On") : Localization.TrPresent("Off");
                case FpsCounterRow:
                    return GameSettings.ShowFps ? Localization.TrPresent("On") : Localization.TrPresent("Off");

                // ---- story pass (page B) ----
                case TextSpeedRow: return AccessibilitySpeedLabel();
                case SubtitleSizeRow: return AccessibilitySizeLabel();
                case ColorblindRow: return AccessibilityColorblindLabel();
                case AutoAdvanceRow:
                    return AccessibilityStore.AutoAdvance ? Localization.TrPresent("On") : Localization.TrPresent("Off");
                case SkipHoldRow: return Localization.TimerLabel(AccessibilityStore.SkipHoldSeconds);
                case VolumeMasterRow: return VolumeSliderLabel(VolumeCategories.Category.Master);
                case VolumeMusicRow: return VolumeSliderLabel(VolumeCategories.Category.Music);
                case VolumeAmbienceRow: return VolumeSliderLabel(VolumeCategories.Category.Ambience);
                case VolumeSfxRow: return VolumeSliderLabel(VolumeCategories.Category.Sfx);
                case VolumeUiRow: return VolumeSliderLabel(VolumeCategories.Category.Ui);
                default: return "";
            }
        }

        // The shadows row shows the live distance plus what it follows
        // ("40 m · tier" when the player has not overridden it).
        static string ShadowsValueLabel()
        {
            string live = GameSettings.ShadowsLabel();
            return GameSettings.ShadowsIndex >= 0 ? live : live + " · tier";
        }

        static string AccessibilitySpeedLabel()
        {
            switch (Mathf.Clamp(AccessibilityStore.TextSpeed, 0, 3))
            {
                case 0: return StoryStrings.Tr("Slow");
                case 2: return StoryStrings.Tr("Fast");
                case 3: return StoryStrings.Tr("Instant");
                default: return StoryStrings.Tr("Normal");
            }
        }

        static string AccessibilitySizeLabel()
        {
            switch (Mathf.Clamp(AccessibilityStore.SubtitleSize, 0, 2))
            {
                case 0: return StoryStrings.Tr("Small");
                case 2: return StoryStrings.Tr("Large");
                default: return StoryStrings.Tr("Normal");
            }
        }

        static string AccessibilityColorblindLabel()
        {
            switch (Mathf.Clamp(AccessibilityStore.ColorblindMode, 0, 2))
            {
                case 1: return StoryStrings.Tr("Blue-Orange");
                case 2: return StoryStrings.Tr("High contrast");
                default: return StoryStrings.Tr("Standard");
            }
        }

        static string VolumeSliderLabel(VolumeCategories.Category category)
        {
            return Mathf.RoundToInt(VolumeCategories.GetLevel(category)).ToString();
        }
    }
}
