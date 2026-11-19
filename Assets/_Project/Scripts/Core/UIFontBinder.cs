using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Core
{
    // Makes every uGUI Text use the correct font for the selected language.
    // Static labels also get LocalizedText, which owns the translation itself;
    // this binder is the safety net for labels created or changed by gameplay.
    //
    // Fonts (Phase 1): the scene builder wires the bundled OFL fonts below —
    // body = Inter (EN) / Noto Naskh Arabic (AR), titles = Cairo Bold (EN) /
    // Noto Naskh Arabic Bold (AR), notes = Special Elite (Latin-only props).
    // Every field may be null; the binder then falls back to
    // Localization.CurrentFont, which falls back to the OS fonts. Cairo and
    // Reem Kufi are deliberately NOT used for Arabic strings: they lack the
    // legacy Presentation Forms-B glyphs the ArabicSupport shaper emits
    // (Docs/SOURCING_FONTS_MUSIC.md Part 1), so Arabic titles use Naskh Bold.
    public class UIFontBinder : MonoBehaviour
    {
        [Header("Bundled fonts (wired by the scene builder)")]
        [Tooltip("Latin body/UI font (Inter Regular).")]
        public Font latinFont;
        [Tooltip("Arabic body font (Noto Naskh Arabic Regular).")]
        public Font arabicFont;
        [Tooltip("Latin title font (Cairo Bold).")]
        public Font titleFont;
        [Tooltip("Arabic title font (Noto Naskh Arabic Bold).")]
        public Font titleArabicFont;
        [Tooltip("Typewriter notes/props font (Special Elite; Latin only).")]
        public Font noteFont;

        // Substrings (case-insensitive) that mark a Text's role. Name
        // conventions are the repo's wiring pattern (cf. VolumeCategories scan).
        const string TitleMarker = "title";
        const string HeadingMarker = "heading";
        const string NoteMarker = "note";
        const string TypewriterMarker = "typewriter";

        void OnEnable()
        {
            // Register the bundled pair once so every Localization consumer
            // (world text, quiz, story strings) picks the fonts up too.
            Localization.SetFonts(latinFont, arabicFont);
            Localization.LanguageChanged += Apply;
            Apply();
        }

        void OnDisable()
        {
            Localization.LanguageChanged -= Apply;
        }

        void Apply()
        {
            Text[] texts = GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] == null) continue;
                Font role = ResolveRoleFont(texts[i]);
                texts[i].font = role != null ? role : Localization.CurrentFont;
            }
        }

        // Role fonts win over the body font; a null role font (or an Arabic
        // string on a Latin-only prop font) falls back to the body font.
        Font ResolveRoleFont(Text text)
        {
            if (text == null) return null;
            string name = text.gameObject.name;
            if (string.IsNullOrEmpty(name)) return null;
            string lower = name.ToLowerInvariant();

            if (lower.Contains(NoteMarker) || lower.Contains(TypewriterMarker))
            {
                // Special Elite has no Arabic glyphs: shaped Arabic falls back
                // to the body font so the text still renders.
                if (Localization.Current == GameLanguage.Arabic) return arabicFont;
                return noteFont;
            }

            if (lower.Contains(TitleMarker) || lower.Contains(HeadingMarker))
            {
                return Localization.Current == GameLanguage.Arabic
                    ? (titleArabicFont != null ? titleArabicFont : arabicFont)
                    : (titleFont != null ? titleFont : latinFont);
            }

            return null;
        }
    }
}
