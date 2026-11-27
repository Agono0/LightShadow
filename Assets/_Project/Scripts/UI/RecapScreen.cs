using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LightShadow.Core;
using LightShadow.Story;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // Doc 4.4/5: the "study recap" shown at dawn. Every question Nour met during
    // the night, her answer, the correct answer, and why - a study sheet built
    // from the night itself. Ends on the game's closing line.
    //
    // Premium pass (additive): the recap gains the ending name (D13, set by
    // EndingController via SetEnding) and a REPLAY button back to the main
    // menu through the LoadingScreen. The original recap behaviour is intact.
    //
    // P3: SAVE PNG exports the recap as an image under
    // Application.persistentDataPath/lightshadow/recap_<timestamp>.png
    // (ScreenCapture pattern - works on Windows and Android alike) so the
    // study sheet can be shared or printed.
    public class RecapScreen : MonoBehaviour
    {
        [Header("Wiring")]
        public GameObject panel;
        public Text titleText;
        public Text statsText;
        public Text recapText;
        public Button quitButton;
        public Button menuButton;

        [Header("Story pass (built by SceneBuilderStory)")]
        public Text endingText;
        public Button replayButton;

        [Header("P3 export")]
        public Button savePngButton;
        public Text exportFeedbackText;

        string endingId = "";

        void Awake()
        {
            if (panel != null) panel.SetActive(false);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
            if (menuButton != null) menuButton.onClick.AddListener(ReturnToMenu);
            if (replayButton != null) replayButton.onClick.AddListener(ReplayTheNight);
            if (savePngButton != null) savePngButton.onClick.AddListener(SavePng);
            EnsureFeedbackLabel();
        }

        // EndingController calls this before Show(). Empty string hides the row.
        public void SetEnding(string id)
        {
            endingId = id ?? "";
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

            if (endingText != null)
            {
                bool hasEnding = !string.IsNullOrEmpty(endingId);
                endingText.gameObject.SetActive(hasEnding);
                if (hasEnding)
                {
                    endingText.text = Localization.TrPresent("ENDING") + " - "
                        + StoryStrings.Tr(EndingDisplayName());
                    endingText.font = font;
                    endingText.alignment = arabic ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
                }
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

        string EndingDisplayName()
        {
            switch (endingId)
            {
                case "FullRecovery": return "Full Recovery";
                case "HollowEscape": return "Hollow Escape";
                case "Dawnless": return "Dawnless";
                default: return "Partial Memory";
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

        // Story pass: replay intent - back to the main menu through the loading
        // screen; from there CONTINUE / NEW GAME pick up the run.
        void ReplayTheNight()
        {
            Time.timeScale = 1f;
            LoadingScreen.Transition(SceneNames.MainMenu);
        }

        // ---------------------------------------------------- P3 PNG export --

        // One recap image per press; the timestamp keeps every export.
        public void SavePng()
        {
            StartCoroutine(CaptureRoutine());
        }

        // audit v3 5.6: the export used to succeed (or fail) silently. The
        // result is now shown in a feedback label under the panel and the
        // button re-arms when the capture finishes.
        public string lastExportMessage = "";

        public static string FormatSavedMessage(string fileName)
        {
            return Localization.Format("Recap saved: {0}", fileName);
        }

        public static string FormatFailedMessage()
        {
            return Localization.TrPresent("Recap export failed");
        }

        public void SetExportFeedback(string message)
        {
            lastExportMessage = message;
            if (exportFeedbackText != null)
            {
                exportFeedbackText.text = message;
                exportFeedbackText.font = Localization.CurrentFont;
            }
        }

        // Runtime-built feedback label so SceneBuilderStory (outside this
        // lane's scope) needs no edit; a pre-wired exportFeedbackText wins.
        void EnsureFeedbackLabel()
        {
            if (exportFeedbackText != null) return;

            Transform parent = panel != null ? panel.transform : transform;
            var labelGo = new GameObject("ExportFeedback", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(parent, false);
            var rect = (RectTransform)labelGo.transform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 12f);
            rect.sizeDelta = new Vector2(640f, 28f);

            exportFeedbackText = labelGo.GetComponent<Text>();
            exportFeedbackText.font = Localization.CurrentFont;
            exportFeedbackText.fontSize = 18;
            exportFeedbackText.alignment = TextAnchor.MiddleCenter;
            exportFeedbackText.color = new Color(0.85f, 0.9f, 0.92f, 0.95f);
            exportFeedbackText.horizontalOverflow = HorizontalWrapMode.Overflow;
            exportFeedbackText.text = "";
        }

        IEnumerator CaptureRoutine()
        {
            // ScreenCapture only works at the end of a rendered frame.
            yield return new WaitForEndOfFrame();

            if (savePngButton != null) savePngButton.interactable = false;
            try
            {
                Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
                if (texture == null)
                {
                    SetExportFeedback(FormatFailedMessage());
                    yield break;
                }

                try
                {
                    string directory = Path.Combine(Application.persistentDataPath, "lightshadow");
                    if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                    string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string path = Path.Combine(directory, "recap_" + timestamp + ".png");
                    File.WriteAllBytes(path, texture.EncodeToPNG());
                    SetExportFeedback(FormatSavedMessage(Path.GetFileName(path)));
                }
                catch (Exception)
                {
                    SetExportFeedback(FormatFailedMessage());
                }
                finally
                {
                    Destroy(texture);
                }
            }
            finally
            {
                if (savePngButton != null) savePngButton.interactable = true;
            }
        }
    }
}
