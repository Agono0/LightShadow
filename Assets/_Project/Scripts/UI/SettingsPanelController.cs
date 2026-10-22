using LightShadow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // Shared settings card used by both the main menu and the pause menu.
    // Graphics options are applied live and persisted by GameSettings.
    public class SettingsPanelController : MonoBehaviour
    {
        public const int ResolutionRow = 0;
        public const int FullscreenRow = 1;
        public const int QualityRow = 2;
        public const int VSyncRow = 3;
        public const int ShadowsRow = 4;
        public const int MsaaRow = 5;
        public const int RenderScaleRow = 6;
        public const int BrightnessRow = 7;
        public const int SensitivityRow = 8;
        public const int LanguageRow = 9;
        public const int NarratorRow = 10;

        [Header("Wiring")]
        public GameObject panel;
        public Text title;
        public Text[] rowLabels = new Text[11];
        public Text[] rowValues = new Text[11];
        public Button[] prevButtons = new Button[11];
        public Button[] nextButtons = new Button[11];
        public Button resetButton;
        public Button closeButton;
        public Text resetLabel;
        public Text closeLabel;

        public bool IsOpen => panel != null && panel.activeSelf;

        static readonly string[] RowKeys =
        {
            "Resolution", "Fullscreen", "Quality", "VSync", "Shadows",
            "Anti-aliasing", "Render scale", "Brightness", "Mouse sensitivity",
            "Language", "Narrator voice"
        };

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

            Localization.LanguageChanged += Refresh;
            GameSettings.Changed += Refresh;
            Refresh();
        }

        void OnDestroy()
        {
            Localization.LanguageChanged -= Refresh;
            GameSettings.Changed -= Refresh;
        }

        public void Show()
        {
            if (panel != null) panel.SetActive(true);
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
                case QualityRow: GameSettings.SetQualityLevel(GameSettings.QualityLevel - 1); break;
                case VSyncRow: GameSettings.ToggleVSync(); break;
                case ShadowsRow: GameSettings.SetShadows(GameSettings.ShadowsIndex - 1); break;
                case MsaaRow: GameSettings.SetMsaa(GameSettings.MsaaIndex - 1); break;
                case RenderScaleRow: GameSettings.SetRenderScale(GameSettings.RenderScaleIndex - 1); break;
                case BrightnessRow: GameSettings.SetBrightness(GameSettings.Brightness - 0.1f); break;
                case SensitivityRow: GameSettings.SetSensitivity(GameSettings.Sensitivity - 0.1f); break;
                case LanguageRow: GameSettings.ToggleLanguage(); break;
                case NarratorRow: GameSettings.ToggleNarrator(); break;
            }
        }

        void Next(int row)
        {
            switch (row)
            {
                case ResolutionRow: GameSettings.SetResolutionIndex(GameSettings.ResolutionIndex + 1); break;
                case FullscreenRow: GameSettings.ToggleFullscreen(); break;
                case QualityRow: GameSettings.SetQualityLevel(GameSettings.QualityLevel + 1); break;
                case VSyncRow: GameSettings.ToggleVSync(); break;
                case ShadowsRow: GameSettings.SetShadows(GameSettings.ShadowsIndex + 1); break;
                case MsaaRow: GameSettings.SetMsaa(GameSettings.MsaaIndex + 1); break;
                case RenderScaleRow: GameSettings.SetRenderScale(GameSettings.RenderScaleIndex + 1); break;
                case BrightnessRow: GameSettings.SetBrightness(GameSettings.Brightness + 0.1f); break;
                case SensitivityRow: GameSettings.SetSensitivity(GameSettings.Sensitivity + 0.1f); break;
                case LanguageRow: GameSettings.ToggleLanguage(); break;
                case NarratorRow: GameSettings.ToggleNarrator(); break;
            }
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
                case ResolutionRow: return Localization.Present(GameSettings.ResolutionLabel());
                case FullscreenRow:
                    return GameSettings.Fullscreen ? Localization.TrPresent("Fullscreen") : Localization.TrPresent("Windowed");
                case QualityRow: return Localization.Present(Localization.QualityName(GameSettings.QualityLevel));
                case VSyncRow:
                    return GameSettings.VSync ? Localization.TrPresent("On") : Localization.TrPresent("Off");
                case ShadowsRow: return Localization.Present(GameSettings.ShadowsLabel());
                case MsaaRow: return Localization.Present(GameSettings.MsaaLabel());
                case RenderScaleRow: return Localization.Present(GameSettings.RenderScaleLabel());
                case BrightnessRow: return Localization.Present(GameSettings.BrightnessLabel());
                case SensitivityRow: return Localization.Present(GameSettings.SensitivityLabel());
                case LanguageRow: return Localization.LanguageName(Localization.Current);
                case NarratorRow:
                    return GameSettings.NarratorEnabled ? Localization.TrPresent("On") : Localization.TrPresent("Off");
                default: return "";
            }
        }
    }
}
