using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Core
{
    // Makes every uGUI Text use the correct font for the selected language.
    // Static labels also get LocalizedText, which owns the translation itself;
    // this binder is the safety net for labels created or changed by gameplay.
    public class UIFontBinder : MonoBehaviour
    {
        void OnEnable()
        {
            Localization.LanguageChanged += Apply;
            Apply();
        }

        void OnDisable()
        {
            Localization.LanguageChanged -= Apply;
        }

        void Apply()
        {
            Font font = Localization.CurrentFont;
            if (font == null) return;

            Text[] texts = GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null) texts[i].font = font;
            }
        }
    }
}
