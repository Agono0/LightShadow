using LightShadow.Contrib.Abdelraouf.Overlay;
using System;
using System.Collections;
using LightShadow.Core;
using LightShadow.Player;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // Card-sorting overlay for the wing puzzles (finish-the-game plan,
    // workstream 5), first used by the library card catalogue. The player reads
    // one index card at a time and picks the drawer it belongs to. A right
    // drawer files the card; a wrong one rattles shut and counts as a wrong
    // move (meter penalty + noise hook through PuzzleBase.WrongAttempt).
    //
    // Self-registers a static Instance in Awake (InteractionRaycaster pattern),
    // holds the story input lock while open, frees the cursor like the quiz UI.
    // Desktop: keys 1-4 pick a drawer, H hint, Esc steps away. Mobile: tap the
    // drawer buttons.
    public class CardSortUIController : MonoBehaviour
    {
        public static CardSortUIController Instance { get; private set; }

        [Header("Wiring")]
        public GameObject panel;
        public Text titleText;
        public Text counterText;
        public Text clueText;
        public Text feedbackText;
        public Text hintText;
        public Button[] drawerButtons = new Button[4];
        public Text[] drawerLabels = new Text[4];
        public Button hintButton;
        public Button closeButton;

        public bool IsOpen => panel != null && panel.activeSelf;

        string titleEn = "";
        string titleAr = "";
        string[] cluesEn = new string[0];
        string[] cluesAr = new string[0];
        string[] drawersEn = new string[0];
        string[] drawersAr = new string[0];
        int[] correctDrawers = new int[0];
        int cardIndex;
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

        // Arrays are copied by reference and read only; Arabic pools fall back
        // to English per entry when empty. onSolved fires once after the last
        // card files; onWrongAttempt fires per wrong drawer.
        public void Show(string titleEn, string titleAr,
            string[] cluesEn, string[] cluesAr, string[] drawersEn, string[] drawersAr, int[] correctDrawers,
            Func<string> hintProvider, Action onSolved, Action onWrongAttempt)
        {
            if (cluesEn == null || correctDrawers == null) return;
            if (cluesEn.Length == 0 || cluesEn.Length != correctDrawers.Length) return;
            if (!ValidDrawers(correctDrawers, drawerButtons.Length))
            {
                Debug.LogWarning("[CardSortUI] Show refused: every correctDrawers entry must index drawerButtons.");
                return;
            }

            StopAllCoroutines();

            this.titleEn = titleEn ?? "";
            this.titleAr = titleAr ?? "";
            this.cluesEn = cluesEn;
            this.cluesAr = cluesAr ?? new string[0];
            this.drawersEn = drawersEn ?? new string[0];
            this.drawersAr = drawersAr ?? new string[0];
            this.correctDrawers = correctDrawers;
            this.hintProvider = hintProvider;
            this.onSolved = onSolved;
            this.onWrongAttempt = onWrongAttempt;

            cardIndex = 0;
            busy = false;

            SetLocalized(feedbackText, "", "");
            SetLocalized(hintText, "", "");

            for (int i = 0; i < drawerButtons.Length; i++)
            {
                if (drawerButtons[i] == null) continue;
                drawerButtons[i].onClick.RemoveAllListeners();
                int index = i;
                drawerButtons[i].onClick.AddListener(() => Choose(index));
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

            RenderStatics();
            RenderCard();

            if (panel != null) panel.SetActive(true);

            lockToken = OverlayStack.Acquire("CardSortUI", true, true, () => Close(false));
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

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) Choose(0);
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) Choose(1);
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) Choose(2);
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) Choose(3);
            if (Input.GetKeyDown(KeyCode.H)) ShowHint();
            if (Input.GetKeyDown(KeyCode.Escape)) Close(false);
        }

        void Choose(int drawer)
        {
            if (busy || cardIndex >= cluesEn.Length) return;

            if (drawer == correctDrawers[cardIndex])
            {
                cardIndex++;
                if (cardIndex >= cluesEn.Length)
                {
                    busy = true;
                    if (closeButton != null) closeButton.interactable = false;
                    SetLocalized(feedbackText,
                        "Something slim drops into the return tray.",
                        "سقط شيء رفيع في صينية الإرجاع.");
                    StartCoroutine(CloseAfterFeedback(1.4f));
                }
                else
                {
                    SetLocalized(feedbackText, "Filed.", "أُرشفت.");
                    RenderCard();
                }
            }
            else
            {
                SetLocalized(feedbackText,
                    "The drawer rattles shut. Not that one.",
                    "أُغلق الدرج بصوت عالٍ. ليس هذا.");
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

        // P1: every clue's drawer must be a real button, or the last card
        // can never file and the overlay soft-locks. Pure, so tests pin it.
        public static bool ValidDrawers(int[] correctDrawers, int drawerCount)
        {
            if (correctDrawers == null || correctDrawers.Length == 0) return false;
            for (int i = 0; i < correctDrawers.Length; i++)
                if (correctDrawers[i] < 0 || correctDrawers[i] >= drawerCount) return false;
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
            RenderCard();
        }

        void RenderStatics()
        {
            SetLocalized(titleText, titleEn, titleAr);
            bool arabic = Localization.Current == GameLanguage.Arabic;
            for (int i = 0; i < drawerLabels.Length; i++)
            {
                string en = i < drawersEn.Length ? drawersEn[i] : "";
                string ar = i < drawersAr.Length ? drawersAr[i] : "";
                SetLocalized(drawerLabels[i], (i + 1) + ". " + en, string.IsNullOrEmpty(ar) ? "" : (i + 1) + ". " + ar);
                if (drawerLabels[i] != null)
                {
                    drawerLabels[i].alignment = arabic ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
                }
            }
        }

        void RenderCard()
        {
            if (cardIndex >= cluesEn.Length) return;

            string clueEn = cluesEn[cardIndex];
            string clueAr = cardIndex < cluesAr.Length ? cluesAr[cardIndex] : "";
            SetLocalized(clueText, clueEn, clueAr);
            if (clueText != null)
            {
                clueText.alignment = Localization.Current == GameLanguage.Arabic && !string.IsNullOrEmpty(clueAr)
                    ? TextAnchor.MiddleRight
                    : TextAnchor.MiddleCenter;
            }

            SetLocalized(counterText,
                "Card " + (cardIndex + 1) + " of " + cluesEn.Length,
                "بطاقة " + (cardIndex + 1) + " من " + cluesEn.Length);
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
