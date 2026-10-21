using System.Collections.Generic;
using System.Text;
using LightShadow.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // Doc 4.4/5: the "study recap" shown at dawn. Every question Nour met during
    // the night, her answer, the correct answer, and why - a study sheet built
    // from the night itself. Ends on the game's closing line.
    public class RecapScreen : MonoBehaviour
    {
        [Header("Wiring")]
        public GameObject panel;
        public Text titleText;
        public Text statsText;
        public Text recapText;
        public Button quitButton;
        public Button menuButton;

        void Awake()
        {
            if (panel != null) panel.SetActive(false);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
            if (menuButton != null) menuButton.onClick.AddListener(ReturnToMenu);
        }

        public void Show(List<GameManager.AnswerRecord> answers)
        {
            Time.timeScale = 1f;

            bool arabic = Localization.Current == GameLanguage.Arabic;
            Font font = Localization.CurrentFont;

            if (titleText != null)
            {
                titleText.text = Localization.TrPresent("Study Recap");
                titleText.font = font;
            }

            if (statsText != null)
            {
                statsText.text = BuildStats(answers);
                statsText.font = font;
            }

            if (recapText != null)
            {
                recapText.text = BuildBody(answers);
                recapText.font = font;
                recapText.alignment = arabic ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
            }

            if (panel != null) panel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (EventSystem.current != null && quitButton != null)
            {
                EventSystem.current.SetSelectedGameObject(quitButton.gameObject);
            }
        }

        static string BuildStats(List<GameManager.AnswerRecord> answers)
        {
            if (answers == null || answers.Count == 0)
                return Localization.TrPresent("No questions were answered tonight.");

            int correct = 0;
            for (int i = 0; i < answers.Count; i++)
            {
                if (answers[i].correct) correct++;
            }

            int percent = Mathf.RoundToInt(100f * correct / answers.Count);
            return Localization.Format("Answered: {0}    Correct: {1} ({2}%)", answers.Count, correct, percent);
        }

        static string BuildBody(List<GameManager.AnswerRecord> answers)
        {
            StringBuilder builder = new StringBuilder();

            if (answers == null || answers.Count == 0)
            {
                builder.Append(Localization.TrPresent("Nothing was asked tonight."));
            }
            else
            {
                for (int i = 0; i < answers.Count; i++)
                {
                    GameManager.AnswerRecord record = answers[i];
                    QuestionData question = record.question;

                    builder.Append(i + 1).Append(". ");
                    if (question != null) builder.Append(Localization.QuestionText(question));
                    builder.AppendLine();

                    if (question != null)
                    {
                        bool hasAnswer = record.chosenIndex >= 0
                            && question.options != null
                            && record.chosenIndex < question.options.Length;

                        builder.Append(hasAnswer
                            ? Localization.Format("Your answer: {0}", Localization.QuestionOption(question, record.chosenIndex))
                            : Localization.TrPresent("Your answer: none (out of time)"));
                        builder.AppendLine();

                        builder.Append(Localization.Format("Correct answer: {0}",
                            Localization.QuestionOption(question, question.correctIndex))).AppendLine();
                        if (!string.IsNullOrEmpty(Localization.QuestionExplanation(question)))
                        {
                            builder.Append(Localization.QuestionExplanation(question)).AppendLine();
                        }
                    }

                    builder.AppendLine();
                }
            }

            builder.AppendLine();
            builder.Append(Localization.TrPresent(
                "\"Knowledge is the only light that follows a person into the dark.\""));
            return builder.ToString();
        }

        void QuitGame()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}
