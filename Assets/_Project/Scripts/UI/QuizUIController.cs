using System;
using System.Collections;
using LightShadow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // The question overlay used by quiz boards, knowledge fragments and memory
    // flashes (Doc 4.1). The world keeps moving - the Shadow does not wait while
    // Nour reads. Answer with keys 1-4 or by clicking; 20 s timer, 3 s feedback.
    public class QuizUIController : MonoBehaviour
    {
        [Header("Wiring")]
        public GameObject panel;
        public Text headerText;
        public Text questionText;
        public Text timerText;
        public Button[] optionButtons = new Button[4];
        public Text[] optionTexts = new Text[4];
        public GameObject feedbackPanel;
        public Text feedbackText;

        public bool IsOpen => panel != null && panel.activeSelf;

        QuestionData current;
        Action<bool, int> onAnswered;
        float timeLeft;
        bool answered;
        bool cursorWasLocked;

        void Awake()
        {
            if (panel != null) panel.SetActive(false);
            if (feedbackPanel != null) feedbackPanel.SetActive(false);

            // Auto-wire the label inside each button if not set in the editor.
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] != null && (optionTexts.Length <= i || optionTexts[i] == null))
                {
                    if (optionTexts.Length <= i) Array.Resize(ref optionTexts, optionButtons.Length);
                    optionTexts[i] = optionButtons[i].GetComponentInChildren<Text>();
                }
            }
        }

        void Update()
        {
            if (!IsOpen || answered) return;

            timeLeft -= Time.deltaTime;

            if (timerText != null)
            {
                timerText.text = Mathf.CeilToInt(Mathf.Max(0f, timeLeft)) + "s";
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) Choose(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) Choose(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) Choose(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) Choose(3);

            if (timeLeft <= 0f) Choose(-1);
        }

        // Presented by QuizBoard, KnowledgeFragment, etc. Callback: (correct, chosenIndex)
        // with chosenIndex -1 when the timer runs out.
        public void ShowQuestion(QuestionData question, Action<bool, int> callback)
        {
            if (question == null || !question.IsValid)
            {
                if (callback != null) callback(false, -1);
                return;
            }

            StopAllCoroutines();

            current = question;
            onAnswered = callback;
            answered = false;

            GameConfig config = GameManager.Instance != null ? GameManager.Instance.config : null;
            timeLeft = config != null ? config.questionTimer : 20f;

            if (headerText != null)
            {
                headerText.text = question.category + " - " + question.difficulty;
            }

            if (questionText != null) questionText.text = question.question;

            for (int i = 0; i < optionButtons.Length && i < 4; i++)
            {
                if (optionButtons[i] == null) continue;

                if (optionTexts.Length > i && optionTexts[i] != null)
                {
                    optionTexts[i].text = (i + 1) + ". " + question.options[i];
                }

                optionButtons[i].onClick.RemoveAllListeners();
                int index = i; // capture for the closure
                optionButtons[i].onClick.AddListener(() => Choose(index));
            }

            if (feedbackPanel != null) feedbackPanel.SetActive(false);
            if (panel != null) panel.SetActive(true);

            // Free the cursor on desktop while the question is up.
            cursorWasLocked = Cursor.lockState == CursorLockMode.Locked;
            if (cursorWasLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        void Choose(int index)
        {
            if (answered || current == null) return;
            answered = true;

            bool correct = index == current.correctIndex;

            if (feedbackPanel != null) feedbackPanel.SetActive(true);
            if (feedbackText != null)
            {
                string head = index < 0 ? "Out of time." : (correct ? "Correct." : "Not quite.");
                feedbackText.text = head + "\nThe answer is: "
                    + current.options[current.correctIndex] + "\n" + current.explanation;
            }

            GameConfig config = GameManager.Instance != null ? GameManager.Instance.config : null;
            StartCoroutine(CloseAfterFeedback(config != null ? config.feedbackSeconds : 3f, correct, index));
        }

        IEnumerator CloseAfterFeedback(float seconds, bool correct, int chosen)
        {
            yield return new WaitForSeconds(seconds);

            if (panel != null) panel.SetActive(false);
            if (feedbackPanel != null) feedbackPanel.SetActive(false);

            if (cursorWasLocked && !Application.isMobilePlatform)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            Action<bool, int> callback = onAnswered;
            onAnswered = null;
            current = null;
            if (callback != null) callback(correct, chosen);
        }

        void OnDestroy()
        {
            if (answered) return;
            if (cursorWasLocked && !Application.isMobilePlatform)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
