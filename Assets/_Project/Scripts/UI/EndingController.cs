using System.Collections;
using LightShadow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // 03_Ending: dawn outside the gate, the closing line, then the study recap.
    // The recap is the real ending: every question of the run, the chosen answer,
    // the right answer and the explanation - a printable study sheet.
    public class EndingController : MonoBehaviour
    {
        [Header("Wiring")]
        public Text dawnTitle;
        public Text dawnBody;
        public RecapScreen recap;
        public CanvasGroup dawnGroup;
        public float dawnSeconds = 2.5f;

        IEnumerator Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (dawnTitle != null)
            {
                dawnTitle.text = Localization.TrPresent("DAWN");
                dawnTitle.font = Localization.CurrentFont;
            }

            if (dawnBody != null)
            {
                // The string is the exact translation key, so Arabic is picked up too.
                dawnBody.text = Localization.TrPresent("The gate closes behind her. The sky above the school turns from black to grey to a thin, clean gold.\n\n"
                    + "She opens the memory box on the pavement and reads her mother's lesson plans one last time.");
                dawnBody.font = Localization.CurrentFont;
                dawnBody.alignment = Localization.Current == GameLanguage.Arabic
                    ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
            }

            if (dawnGroup != null) dawnGroup.alpha = 1f;

            // The narrator closes the night once, before the recap appears.
            StoryTeller.Speak(Localization.Tr(
                "The gate closes behind Nour. She opens the memory box on the pavement and reads the lesson plans one last time."), true);

            yield return new WaitForSecondsRealtime(dawnSeconds);

            if (recap != null) recap.Show(RunResults.Answers);
        }
    }
}
