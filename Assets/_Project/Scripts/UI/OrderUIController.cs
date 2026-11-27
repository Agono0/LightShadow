using LightShadow.Contrib.Abdelraouf.Overlay;
using System;
using System.Collections;
using LightShadow.Core;
using LightShadow.Player;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // Step-ordering overlay for the wing puzzles (finish-the-game plan,
    // workstream 5), first used by the infirmary's first-aid board: steps hang
    // in a scrambled order and the player taps them in the correct sequence.
    // A right step locks in with its number; a wrong one resets the board and
    // counts as a wrong move (meter penalty + noise hook through
    // PuzzleBase.WrongAttempt).
    //
    // Self-registers a static Instance in Awake (InteractionRaycaster pattern),
    // holds the story input lock while open, frees the cursor like the quiz UI.
    // Desktop: keys 1-5 tap a step, H hint, Esc steps away. Mobile: taps.
    public class OrderUIController : MonoBehaviour
    {
        public static OrderUIController Instance { get; private set; }

        [Header("Wiring")]
        public GameObject panel;
        public Text titleText;
        public Text progressText;
        public Button[] stepButtons = new Button[5];
        public Text[] stepLabels = new Text[5];
        public Text feedbackText;
        public Text hintText;
        public Button hintButton;
        public Button closeButton;

        public bool IsOpen => panel != null && panel.activeSelf;

        string titleEn = "";
        string titleAr = "";
        string[] stepsEn = new string[0];
        string[] stepsAr = new string[0];
        int[] displayOrder = new int[0];
        int nextStep;
        Func<string> hintProvider;
        Action onSolved;
        Action onWrongAttempt;
        bool busy;
        bool cursorWasLocked;
        InputLockToken lockToken;

        void Awake()
        {
            Instance = this;
            if (panel != null) panel.SetActive(false);
            Localization.LanguageChanged += OnLanguageChanged;
        }

        void OnDestroy()
        {
            Localization.LanguageChanged -= OnLanguageChanged;
            if (Instance == this) Instance = null;
            ReleaseLock();
        }

        // displayOrder maps each button to an index in the steps arrays
        // (correct order). onSolved fires once after the last step locks;
        // onWrongAttempt fires per out-of-order tap and resets the board.
        public void Show(string titleEn, string titleAr,
            string[] stepsEn, string[] stepsAr, int[] displayOrder,
            Func<string> hintProvider, Action onSolved, Action onWrongAttempt)
        {
            if (stepsEn == null || displayOrder == null) return;
            if (stepsEn.Length == 0 || stepsEn.Length != displayOrder.Length) return;

            StopAllCoroutines();

            this.titleEn = titleEn ?? "";
            this.titleAr = titleAr ?? "";
            this.stepsEn = stepsEn;
            this.stepsAr = stepsAr ?? new string[0];
            this.displayOrder = displayOrder;
            this.hintProvider = hintProvider;
            this.onSolved = onSolved;
            this.onWrongAttempt = onWrongAttempt;

            nextStep = 0;
            busy = false;

            SetLocalized(feedbackText,
                "Tap the steps in the right order.",
                "المس الخطوات بالترتيب الصحيح.");
            SetLocalized(hintText, "", "");

            for (int i = 0; i < stepButtons.Length; i++)
            {
                if (stepButtons[i] == null) continue;
                stepButtons[i].onClick.RemoveAllListeners();
                int button = i;
                stepButtons[i].onClick.AddListener(() => Tap(button));
            }
            if (hintButton != null)
            {
                hintButton.onClick.RemoveAllListeners();
                hintButton.onClick.AddListener(ShowHint);
            }
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(() => Close(false));
            }

            RenderStatics();
            RenderBoard();
            RenderProgress();

            if (panel != null) panel.SetActive(true);

            lockToken = OverlayStack.Acquire("OrderUI", true, true, () => Close(false));
            cursorWasLocked = Cursor.lockState == CursorLockMode.Locked;
            if (cursorWasLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        void Update()
        {
            if (!IsOpen || busy) return;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) Tap(0);
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) Tap(1);
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) Tap(2);
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) Tap(3);
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) Tap(4);
            if (Input.GetKeyDown(KeyCode.H)) ShowHint();
            if (Input.GetKeyDown(KeyCode.Escape)) Close(false);
        }

        void Tap(int button)
        {
            if (busy || button < 0 || button >= displayOrder.Length) return;
            if (stepButtons[button] != null && !stepButtons[button].interactable) return;

            int step = displayOrder[button];
            if (step == nextStep)
            {
                nextStep++;
                if (stepButtons[button] != null) stepButtons[button].interactable = false;
                RenderBoard();
                RenderProgress();

                if (nextStep >= stepsEn.Length)
                {
                    busy = true;
                    SetLocalized(feedbackText,
                        "Every step in place. The cabinet's clip lets go.",
                        "كل خطوة في مكانها. انفتح قفل الخزانة.");
                    StartCoroutine(CloseAfterFeedback(1.4f));
                }
                else
                {
                    SetLocalized(feedbackText,
                        "Step " + nextStep + " done.",
                        "اكتملت الخطوة " + nextStep + ".");
                }
            }
            else
            {
                nextStep = 0;
                for (int i = 0; i < stepButtons.Length; i++)
                {
                    if (stepButtons[i] != null) stepButtons[i].interactable = true;
                }
                RenderBoard();
                RenderProgress();
                SetLocalized(feedbackText,
                    "The board's clip snaps shut. Start the sequence over.",
                    "انغلق قفل اللوحة. أعد التسلسل من البداية.");
                Action wrong = onWrongAttempt;
                if (wrong != null) wrong();
            }
        }

        void ShowHint()
        {
            if (busy || hintProvider == null) return;
            string hint = hintProvider();
            if (string.IsNullOrEmpty(hint) || hintText == null) return;
            hintText.text = hint;
            hintText.font = Localization.CurrentFont;
            hintText.alignment = Localization.Current == GameLanguage.Arabic
                ? TextAnchor.MiddleRight
                : TextAnchor.MiddleCenter;
        }

        IEnumerator CloseAfterFeedback(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Close(true);
        }

        void Close(bool solved)
        {
            if (panel != null) panel.SetActive(false);
            ReleaseLock();

            Action done = solved ? onSolved : null;
            onSolved = null;
            onWrongAttempt = null;
            hintProvider = null;
            if (done != null) done();
        }

        void ReleaseLock()
        {
            OverlayStack.Release(ref lockToken);
            if (cursorWasLocked && !Application.isMobilePlatform)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            cursorWasLocked = false;
        }

        void OnLanguageChanged()
        {
            if (!IsOpen) return;
            RenderStatics();
            RenderBoard();
            RenderProgress();
        }

        void RenderStatics()
        {
            SetLocalized(titleText, titleEn, titleAr);
        }

        void RenderBoard()
        {
            bool arabic = Localization.Current == GameLanguage.Arabic;
            for (int i = 0; i < stepLabels.Length; i++)
            {
                if (i >= displayOrder.Length) break;
                int step = displayOrder[i];
                string en = step < stepsEn.Length ? stepsEn[step] : "";
                string ar = step < stepsAr.Length ? stepsAr[step] : "";

                // Locked steps wear their sequence number; open ones do not.
                bool locked = stepButtons[i] != null && !stepButtons[i].interactable;
                int number = step + 1;
                SetLocalized(stepLabels[i],
                    (i + 1) + ". " + (locked ? "[" + number + "] " : "") + en,
                    (locked ? "[" + number + "] " : "") + ar);
                if (stepLabels[i] != null)
                {
                    stepLabels[i].alignment = arabic ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
                }
            }
        }

        void RenderProgress()
        {
            SetLocalized(progressText,
                "Step " + Mathf.Min(nextStep + 1, stepsEn.Length) + " of " + stepsEn.Length,
                "خطوة " + Mathf.Min(nextStep + 1, stepsEn.Length) + " من " + stepsEn.Length);
        }

        static void SetLocalized(Text target, string english, string arabic)
        {
            if (target == null) return;
            bool useArabic = Localization.Current == GameLanguage.Arabic && !string.IsNullOrEmpty(arabic);
            target.text = useArabic ? Localization.Present(arabic) : english;
            target.font = Localization.CurrentFont;
        }
    }
}
