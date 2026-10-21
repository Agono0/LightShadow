using UnityEngine;

namespace LightShadow.Core
{
    // World-space TextMesh counterpart of LocalizedText. It also swaps the font
    // material because a TextMesh needs its renderer updated when the font atlas
    // changes (the built-in Arial atlas has no Arabic glyphs).
    [RequireComponent(typeof(TextMesh))]
    public class LocalizedWorldText : MonoBehaviour
    {
        public string english;
        [TextArea(2, 6)] public string arabic;

        TextMesh text;
        MeshRenderer renderer;

        void Awake()
        {
            text = GetComponent<TextMesh>();
            renderer = GetComponent<MeshRenderer>();
            if (string.IsNullOrEmpty(english)) english = text.text;
            Localization.LanguageChanged += Refresh;
            Refresh();
        }

        void OnDestroy()
        {
            Localization.LanguageChanged -= Refresh;
        }

        public void Refresh()
        {
            if (text == null) text = GetComponent<TextMesh>();
            if (text == null) return;

            if (Localization.Current == GameLanguage.Arabic)
            {
                string source = string.IsNullOrEmpty(arabic) ? Localization.Tr(english) : arabic;
                text.text = Localization.Present(source);
                text.font = Localization.ArabicFont;
            }
            else
            {
                text.text = english;
                text.font = Localization.EnglishFont;
            }

            if (renderer == null) renderer = GetComponent<MeshRenderer>();
            if (renderer != null && text.font != null)
            {
                renderer.sharedMaterial = text.font.material;
            }
        }

        public static LocalizedWorldText Attach(TextMesh text, string arabicText = null)
        {
            if (text == null) return null;
            LocalizedWorldText localized = text.GetComponent<LocalizedWorldText>();
            if (localized == null) localized = text.gameObject.AddComponent<LocalizedWorldText>();
            localized.english = text.text;
            localized.arabic = arabicText;
            return localized;
        }
    }
}
