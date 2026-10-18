using System.Collections.Generic;
using System.Text;
using LightShadow.Core;
using UnityEngine;
using UnityEngine.EventSystems;
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

        void Awake()
        {
            if (panel != null) panel.SetActive(false);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        }

        public void Show(List<GameManager.AnswerRecord> answers)
        {
            if (titleText != null) titleText.text = "Study Recap";
            if (statsText != null) statsText.text = BuildStats(answers);
            if (recapText != null) recapText.text = BuildBody(answers);
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
            if (answers == null || answers.Count == 0) return "No questions were answered tonight.";

            int correct = 0;
            for (int i = 0; i < answers.Count; i++)
            {
                if (answers[i].correct) correct++;
            }

            int percent = Mathf.RoundToInt(100f * correct / answers.Count);
            return "Answered: " + answers.Count + "    Correct: " + correct + " (" + percent + "%)";
        }

        static string BuildBody(List<GameManager.AnswerRecord> answers)
        {
            StringBuilder builder = new StringBuilder();

            if (answers == null || answers.Count == 0)
            {
                builder.Append("Nothing was asked tonight.");
            }
            else
            {
                for (int i = 0; i < answers.Count; i++)
                {
                    GameManager.AnswerRecord record = answers[i];
                    QuestionData question = record.question;

                    builder.Append(i + 1).Append(". ");
                    if (question != null) builder.Append(question.question);
                    builder.AppendLine();

                    if (question != null)
                    {
                        bool hasAnswer = record.chosenIndex >= 0
                            && question.options != null
                            && record.chosenIndex < question.options.Length;

                        builder.Append(hasAnswer
                            ? "Your answer: " + question.options[record.chosenIndex]
                            : "Your answer: none (out of time)");
                        builder.AppendLine();

                        builder.Append("Correct answer: ").Append(question.options[question.correctIndex]).AppendLine();
                        if (!string.IsNullOrEmpty(question.explanation)) builder.Append(question.explanation).AppendLine();
                    }

                    builder.AppendLine();
                }
            }

            builder.AppendLine();
            builder.Append("\"Knowledge is the only light that follows a person into the dark.\"");
            return builder.ToString();
        }

        void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
