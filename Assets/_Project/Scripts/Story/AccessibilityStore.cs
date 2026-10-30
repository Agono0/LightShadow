using System;
using UnityEngine;

namespace LightShadow.Story
{
    // PlayerPrefs-backed accessibility settings for the story pass (brief #6):
    // text speed, subtitle size, colourblind-safe palette, auto-advance and the
    // skip-hold duration. Styled after Core/GameSettings (static, Changed event,
    // keys under "ls_"), but story-owned so Core stays untouched.
    //
    // The quality-tier row reads/writes the "ls_qualityTier" PlayerPrefs key that
    // the Engineering PerformanceManager already owns (brief-specified pattern to
    // avoid a compile dependency). Values: -1 = auto (manager decides), 0..3 = Low
    // / Medium / High / Ultra. When PerformanceManager is present it applies the
    // tier on its own schedule; the row only moves the pref.
    public static class AccessibilityStore
    {
        public const string TextSpeedKey = "ls_textSpeed";       // 0..3
        public const string SubtitleSizeKey = "ls_subtitleSize"; // 0..2
        public const string ColorblindKey = "ls_colorblind";     // 0..2
        public const string AutoAdvanceKey = "ls_autoAdvance";   // 0/1
        public const string SkipHoldKey = "ls_skipHold";         // index into SkipHoldOptions
        public const string QualityTierKey = "ls_qualityTier";   // shared with PerformanceManager

        public static event Action Changed;

        // Chars per second for the typewriter per text-speed index. 3 = instant.
        static readonly float[] CharsPerSecond = { 22f, 38f, 58f };

        // Skip-hold durations in seconds (row cycles these).
        public static readonly float[] SkipHoldOptions = { 0.4f, 0.8f, 1.5f };

        public static int TextSpeed { get; private set; } = 1;
        public static int SubtitleSize { get; private set; } = 1;
        public static int ColorblindMode { get; private set; } = 0;
        public static bool AutoAdvance { get; private set; } = false;
        public static int SkipHoldIndex { get; private set; } = 1;
        public static int QualityTier { get; private set; } = -1;

        public static float SkipHoldSeconds => SkipHoldOptions[Mathf.Clamp(SkipHoldIndex, 0, SkipHoldOptions.Length - 1)];

        // Typewriter speed in chars/sec for the current setting (instant at max).
        public static float TypewriterCharsPerSecond
        {
            get
            {
                if (TextSpeed >= 3) return float.MaxValue; // instant
                return CharsPerSecond[Mathf.Clamp(TextSpeed, 0, CharsPerSecond.Length - 1)];
            }
        }

        // Dialogue text size multiplier for the subtitle-size setting.
        public static float SubtitleScale
        {
            get
            {
                switch (Mathf.Clamp(SubtitleSize, 0, 2))
                {
                    case 0: return 0.85f;
                    case 2: return 1.3f;
                    default: return 1f;
                }
            }
        }

        public static void Load()
        {
            TextSpeed = PlayerPrefs.GetInt(TextSpeedKey, 1);
            SubtitleSize = PlayerPrefs.GetInt(SubtitleSizeKey, 1);
            ColorblindMode = PlayerPrefs.GetInt(ColorblindKey, 0);
            AutoAdvance = PlayerPrefs.GetInt(AutoAdvanceKey, 0) == 1;
            SkipHoldIndex = PlayerPrefs.GetInt(SkipHoldKey, 1);
            QualityTier = PlayerPrefs.GetInt(QualityTierKey, -1);
        }

        public static void SetTextSpeed(int value)
        {
            TextSpeed = Mathf.Clamp(Wrap(value, 0, 3), 0, 3);
            PlayerPrefs.SetInt(TextSpeedKey, TextSpeed);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public static void SetSubtitleSize(int value)
        {
            SubtitleSize = Mathf.Clamp(Wrap(value, 0, 2), 0, 2);
            PlayerPrefs.SetInt(SubtitleSizeKey, SubtitleSize);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public static void SetColorblindMode(int value)
        {
            ColorblindMode = Mathf.Clamp(Wrap(value, 0, 2), 0, 2);
            PlayerPrefs.SetInt(ColorblindKey, ColorblindMode);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public static void ToggleAutoAdvance()
        {
            AutoAdvance = !AutoAdvance;
            PlayerPrefs.SetInt(AutoAdvanceKey, AutoAdvance ? 1 : 0);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public static void SetSkipHold(int index)
        {
            SkipHoldIndex = Mathf.Clamp(Wrap(index, 0, SkipHoldOptions.Length - 1), 0, SkipHoldOptions.Length - 1);
            PlayerPrefs.SetInt(SkipHoldKey, SkipHoldIndex);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public static void SetQualityTier(int tier)
        {
            // Cycles -1 (auto) -> 0..3 -> back to auto. The PerformanceManager
            // re-reads the pref next launch; nothing here depends on its code.
            QualityTier = tier < -1 || tier > 3 ? -1 : Wrap(tier, -1, 3);
            PlayerPrefs.SetInt(QualityTierKey, QualityTier);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        static int Wrap(int value, int min, int max)
        {
            if (value < min) return max;
            if (value > max) return min;
            return value;
        }

        // ------------------------------------------------------------- palette --
        // Colourblind-safe UI colours for the elements this pass owns (dialogue
        // box states, choice highlights, timed bar, save-slot states, loading
        // bar). Mode 0 keeps the game's warm standard; mode 1 swaps red/green
        // pairs for blue/orange (deuteranopia-friendly); mode 2 is a high
        // contrast set for bright-room readability. World-label colours are NOT
        // touched (they live in Core materials); documented in the report.
        public enum PaletteSlot { Good, Bad, Active, Neutral, Warning }

        static readonly Color[][] Palettes =
        {
            // 0: standard warm (matches the game's amber-on-dark chrome)
            new[]
            {
                new Color(0.62f, 0.86f, 0.55f), // Good   (soft green)
                new Color(0.86f, 0.42f, 0.38f), // Bad    (soft red)
                new Color(1f, 0.86f, 0.62f),    // Active (warm amber)
                new Color(0.55f, 0.55f, 0.58f), // Neutral (dim grey)
                new Color(0.95f, 0.75f, 0.35f), // Warning (amber)
            },
            // 1: blue / orange (deuteranopia- and protanopia-safe)
            new[]
            {
                new Color(0.42f, 0.72f, 0.96f), // Good   (sky blue)
                new Color(0.98f, 0.60f, 0.24f), // Bad    (orange)
                new Color(0.65f, 0.85f, 1f),    // Active (pale blue)
                new Color(0.55f, 0.55f, 0.58f), // Neutral (dim grey)
                new Color(0.99f, 0.78f, 0.35f), // Warning (gold)
            },
            // 2: high contrast (dark room legibility)
            new[]
            {
                new Color(0.55f, 1f, 0.55f),    // Good   (bright green)
                new Color(1f, 0.45f, 0.45f),    // Bad    (bright red)
                new Color(1f, 1f, 0.85f),       // Active (near white)
                new Color(0.72f, 0.72f, 0.76f), // Neutral (light grey)
                new Color(1f, 0.9f, 0.4f),      // Warning (bright gold)
            },
        };

        public static Color PaletteColor(PaletteSlot slot)
        {
            int mode = Mathf.Clamp(ColorblindMode, 0, Palettes.Length - 1);
            return Palettes[mode][(int)slot];
        }
    }
}
