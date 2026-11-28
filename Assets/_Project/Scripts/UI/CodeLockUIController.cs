using LightShadow.Contrib.Abdelraouf.Overlay;
using System;
using System.Collections;
using LightShadow.Core;
using LightShadow.Player;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // Digit-lock overlay for the wing puzzles (finish-the-game plan, workstream
    // 5). PuzzleBase subclasses open it through the static Instance, which the
    // controller registers itself in Awake (the InteractionRaycaster pattern),
    // so no GameManager wiring is needed.
    //
    // The world keeps moving while the lock is open - the Shadow does not wait.
    // Desktop: 0-9 edit the selected wheel, Left/Right move, Enter confirms,
    // Esc closes. Mobile: the +/- buttons over and under each wheel. While the
    // panel is up it holds the story system's input lock
    // (PlayerController.ExternalInputLocked), released on close.
    public class CodeLockUIController : MonoBehaviour
    {
        public static CodeLockUIController Instance { get; private set; }

        [Header("Wiring")]
        public GameObject panel;
        public Text titleText;
        public Text sequenceText;
        public Text feedbackText;
        public Text hintText;
        public Text[] digitTexts = new Text[4];
        public Button[] plusButtons = new Button[4];
        public Button[] minusButtons = new Button[4];
        public Button confirmButton;
        public Button hintButton;
        public Button closeButton;

        public bool IsOpen => panel != null && panel.activeSelf;

        static readonly Color SelectedDigit = new Color(1f, 0.84f, 0.55f, 1f);
        static readonly Color IdleDigit = new Color(0.90f, 0.88f, 0.80f, 1f);

        readonly int[] digits = new int[4];
        int selected;
        string expectedCode = "";
        string titleEn = "";
        string titleAr = "";
        string sequenceEn = "";
        string sequenceAr = "";
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

        // titleAr / sequenceAr fall back to the English text when empty (the
        // chalk sums are language-neutral digits). hintProvider returns an
        // already-shaped hint string (PuzzleBase.NextHint). onSolved fires once,
        // after the success feedback has played; onWrongAttempt fires on every
        // wrong confirm and the lock stays open.
        public void Show(string titleEn, string titleAr, string sequenceEn, string sequenceAr,
            string code, Func<string> hintProvider, Action onSolved, Action onWrongAttempt)
        {
            if (string.IsNullOrEmpty(code) || code.Length != digits.Length) return;
            if (!ValidCode(code, digits.Length))
            {
                Debug.LogWarning("[CodeLockUI] Show refused: code must be exactly " + digits.Length + " digits 0-9.");
                return;
            }

            StopAllCoroutines();

            this.titleEn = titleEn ?? "";
            this.titleAr = titleAr ?? "";
            this.sequenceEn = sequenceEn ?? "";
            this.sequenceAr = sequenceAr ?? "";
            expectedCode = code;
            this.hintProvider = hintProvider;
            this.onSolved = onSolved;
            this.onWrongAttempt = onWrongAttempt;

            for (int i = 0; i < digits.Length; i++) digits[i] = 0;
            selected = 0;
            busy = false;

            SetText(feedbackText, "", "");
            SetText(hintText, "", "");

            for (int i = 0; i < plusButtons.Length; i++)
            {
                if (plusButtons[i] == null) continue;
                plusButtons[i].onClick.RemoveAllListeners();
                int index = i;
                plusButtons[i].onClick.AddListener(() => Nudge(index, +1));
            }
            for (int i = 0; i < minusButtons.Length; i++)
            {
                if (minusButtons[i] == null) continue;
                minusButtons[i].onClick.RemoveAllListeners();
                int index = i;
                minusButtons[i].onClick.AddListener(() => Nudge(index, -1));
            }
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(Confirm);
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
                closeButton.interactable = true;
            }

            RenderTexts();
            RenderDigits();

            if (panel != null) panel.SetActive(true);

            lockToken = OverlayStack.Acquire("CodeLockUI", true, true, () => Close(false));
            cursorWasLocked = Cursor.lockState == CursorLockMode.Locked;
            if (cursorWasLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        void Update()
        {
            // A respawn clears every overlay token (OverlayStack.ResetAll) but a busy
            // success panel refuses Close(false) and stayed on screen unlocked.
            // Not while busy: Close(false) would drop onSolved and the player would
            // have to re-enter the code (the success beat closes itself).
            if (IsOpen && !busy && (lockToken == null || !lockToken.IsActive))
            {
                Close(false);
                return;
            }
            if (!IsOpen || busy) return;

            for (int i = 0; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
                {
                    digits[selected] = i;
                    if (selected < digits.Length - 1) selected++;
                    RenderDigits();
                }
            }

            if (Input.GetKeyDown(KeyCode.RightArrow) && selected < digits.Length - 1)
            {
                selected++;
                RenderDigits();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) && selected > 0)
            {
                selected--;
                RenderDigits();
            }
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                digits[selected] = 0;
                if (selected > 0) selected--;
                RenderDigits();
            }
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Confirm();
            if (Input.GetKeyDown(KeyCode.H)) ShowHint();
            if (Input.GetKeyDown(KeyCode.Escape)) Close(false);
        }

        void Nudge(int index, int delta)
        {
            if (busy || index < 0 || index >= digits.Length) return;
            ClearButtonFocus();
            digits[index] = (digits[index] + delta + 10) % 10;
            selected = index;
            RenderDigits();
        }

        // A clicked +/- button stays selected, and Enter then also "submits" it
        // through the EventSystem on top of our own Enter = Confirm.
        static void ClearButtonFocus()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        void Confirm()
        {
            if (busy) return;
            ClearButtonFocus();

            string entered = "";
            for (int i = 0; i < digits.Length; i++) entered += digits[i].ToString();

            if (entered == expectedCode)
            {
                busy = true;
                if (closeButton != null) closeButton.interactable = false;
                SetText(feedbackText, "The lock clicks open.", "فُتح القفل.");
                StartCoroutine(CloseAfterFeedback(1.2f));
            }
            else
            {
                SetText(feedbackText, "Nothing turns. Check the chalk sums.", "لا يتحرك شيء. راجع مسائل السبورة.");
                Action wrong = onWrongAttempt;
                if (wrong != null) wrong();
            }
        }

        void ShowHint()
        {
            if (busy || hintProvider == null) return;
            string hint = hintProvider();
            if (string.IsNullOrEmpty(hint)) return;
            if (hintText != null)
            {
                hintText.text = hint;
                hintText.font = Localization.CurrentFont;
                hintText.alignment = Localization.Current == GameLanguage.Arabic
                    ? TextAnchor.MiddleRight
                    : TextAnchor.MiddleCenter;
            }
        }

        // P1: the expected code must be all digits; anything else can never
        // be entered on the +/- lock and soft-locks the overlay.
        public static bool ValidCode(string code, int digitCount)
        {
            if (string.IsNullOrEmpty(code) || code.Length != digitCount) return false;
            for (int i = 0; i < code.Length; i++)
                if (code[i] < '0' || code[i] > '9') return false;
            return true;
        }

        // P2: pure discard policy - a busy overlay (the success beat, the
        // sequence playback) may only close solved. Static so tests pin it.
        public static bool CanDiscard(bool busy) => !busy;

        IEnumerator CloseAfterFeedback(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Close(true);
        }

        void Close(bool solved)
        {
            // P2: an unsolved dismiss (close button, Esc, overlay dismiss
            // callback) must not discard the overlay while it is busy; the
            // success beat closes itself with Close(true).
            if (!solved && !CanDiscard(busy)) return;
            if (panel != null) panel.SetActive(false);
            ReleaseLock();

            Action solved2 = solved ? onSolved : null;
            onSolved = null;
            onWrongAttempt = null;
            hintProvider = null;
            if (solved2 != null) solved2();
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
            if (IsOpen) RenderTexts();
        }

        void RenderTexts()
        {
            bool arabic = Localization.Current == GameLanguage.Arabic;
            SetText(titleText, titleEn, titleAr);
            SetText(sequenceText, sequenceEn, sequenceAr);
            if (titleText != null) titleText.alignment = TextAnchor.MiddleCenter;
            if (sequenceText != null) sequenceText.alignment = TextAnchor.MiddleCenter;
            if (feedbackText != null) feedbackText.alignment = arabic ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
        }

        static void SetText(Text target, string english, string arabic)
        {
            if (target == null) return;
            bool useArabic = Localization.Current == GameLanguage.Arabic && !string.IsNullOrEmpty(arabic);
            target.text = useArabic ? Localization.Present(arabic) : english;
            target.font = Localization.CurrentFont;
        }

        void RenderDigits()
        {
            for (int i = 0; i < digitTexts.Length && i < digits.Length; i++)
            {
                if (digitTexts[i] == null) continue;
                digitTexts[i].text = digits[i].ToString();
                digitTexts[i].color = i == selected ? SelectedDigit : IdleDigit;
            }
        }
    }
}
