using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Core
{
    // Attach to a static uGUI Text. The English string is authored in the scene;
    // the Arabic string can be supplied directly (for long paragraphs) or looked
    // up in Localization's English -> Arabic table.
    [RequireComponent(typeof(Text))]
    public class LocalizedText : MonoBehaviour
    {
        public string english;
        [TextArea(2, 8)] public string arabic;
        public bool rightAlignInArabic;
        public TextAnchor arabicAlignment = TextAnchor.MiddleRight;

        Text text;
        TextAnchor englishAlignment;
        bool ready;

        void Awake()
        {
            text = GetComponent<Text>();
            if (string.IsNullOrEmpty(english)) english = text.text;
            englishAlignment = text.alignment;
            ready = true;
            Localization.LanguageChanged += Refresh;
            Refresh();
        }

        void OnDestroy()
        {
            Localization.LanguageChanged -= Refresh;
        }

        public void SetKey(string englishText, string arabicText = null, bool rightAlign = false)
        {
            english = englishText;
            arabic = arabicText;
            rightAlignInArabic = rightAlign;
            if (ready) Refresh();
        }

        public void Refresh()
        {
            if (text == null) text = GetComponent<Text>();
            if (text == null) return;

            if (Localization.Current == GameLanguage.Arabic)
            {
                string source = string.IsNullOrEmpty(arabic) ? Localization.Tr(english) : arabic;
                text.text = Localization.Present(source);
                if (rightAlignInArabic) text.alignment = arabicAlignment;
            }
            else
            {
                text.text = english;
                text.alignment = englishAlignment;
            }
        }

        public static LocalizedText Attach(Text text, string arabicText = null, bool rightAlign = false)
        {
            if (text == null) return null;
            LocalizedText localized = text.GetComponent<LocalizedText>();
            if (localized == null) localized = text.gameObject.AddComponent<LocalizedText>();
            localized.english = text.text;
            localized.arabic = arabicText;
            localized.rightAlignInArabic = rightAlign;
            return localized;
        }
    }
}
