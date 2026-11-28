using LightShadow.Contrib.Abdelraouf.Overlay;
using System;
using System.Collections;
using LightShadow.Core;
using LightShadow.Player;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // Color-sequence overlay for the wing puzzles (finish-the-game plan,
    // workstream 5), first used by the stage light rig: the rig plays a
    // pattern of colored flashes, the player repeats it on the switch buttons.
    // A wrong switch buzzes and counts as a wrong move (meter penalty + noise
    // hook through PuzzleBase.WrongAttempt); the full pattern solves.
    //
    // Self-registers a static Instance in Awake (InteractionRaycaster pattern),
    // holds the story input lock while open, frees the cursor like the quiz UI.
    // Desktop: keys 1-4 press a switch, R replays the pattern, H hint, Esc
    // steps away. Mobile: tap the switches.
    public class SequenceUIController : MonoBehaviour
    {
        public static SequenceUIController Instance { get; private set; }

        [Header("Wiring")]
        public GameObject panel;
        public Text titleText;
        public Text progressText;
        public Image[] swatches = new Image[4];
        public Button[] switchButtons = new Button[4];
        public Text[] switchLabels = new Text[4];
        public Button replayButton;
        public Text feedbackText;
        public Text hintText;
        public Button hintButton;
        public Button closeButton;

        [Header("Flash")]
        public float flashSeconds = 0.45f;
        public float gapSeconds = 0.2f;

        public bool IsOpen => panel != null && panel.activeSelf;

        string titleEn = "";
        string titleAr = "";
        string[] colorsEn = new string[0];
        string[] colorsAr = new string[0];
        int[] sequence = new int[0];
        Color[] baseColors = new Color[0];
        int inputIndex;
        Func<string> hintProvider;
        Action onSolved;
        Action onWrongAttempt;
        bool busy;
        bool solving; // the winning beat only: leaving is allowed while the sequence plays
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

        // swatch base colors are read from the swatch Images the builder
        // painted, so the overlay always matches its own buttons. onSolved
        // fires once after the full pattern; onWrongAttempt per wrong switch.
        public void Show(string titleEn, string titleAr,
            string[] colorsEn, string[] colorsAr, int[] sequence,
            Func<string> hintProvider, Action onSolved, Action onWrongAttempt)
        {
            if (colorsEn == null || sequence == null) return;
            if (colorsEn.Length == 0 || sequence.Length == 0) return;
            if (!ValidSequence(sequence, swatches.Length))
            {
                Debug.LogWarning("[SequenceUI] Show refused: every sequence entry must index a swatch switch.");
                return;
            }

            StopAllCoroutines();

            this.titleEn = titleEn ?? "";
            this.titleAr = titleAr ?? "";
            this.colorsEn = colorsEn;
            this.colorsAr = colorsAr ?? new string[0];
            this.sequence = sequence;
            this.hintProvider = hintProvider;
            this.onSolved = onSolved;
            this.onWrongAttempt = onWrongAttempt;

            baseColors = new Color[swatches.Length];
            for (int i = 0; i < swatches.Length; i++)
            {
                baseColors[i] = swatches[i] != null ? swatches[i].color : Color.gray;
            }

            inputIndex = 0;
            solving = false;
            busy = true; // until the first playback finishes

            SetLocalized(feedbackText,
                "Watch the rig.",
                "راقب أضواء المسرح.");
            SetLocalized(hintText, "", "");

            for (int i = 0; i < switchButtons.Length; i++)
            {
                if (switchButtons[i] == null) continue;
                switchButtons[i].onClick.RemoveAllListeners();
                int index = i;
                switchButtons[i].onClick.AddListener(() => Press(index));
            }
            if (replayButton != null)
            {
                replayButton.onClick.RemoveAllListeners();
                replayButton.onClick.AddListener(Replay);
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
            RenderProgress();

            if (panel != null) panel.SetActive(true);

            lockToken = OverlayStack.Acquire("SequenceUI", true, true, () => Close(false));
            cursorWasLocked = Cursor.lockState == CursorLockMode.Locked;
            if (cursorWasLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            StartCoroutine(PlaySequence());
        }

        void Update()
        {
            if (!IsOpen || busy) return;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) Press(0);
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) Press(1);
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) Press(2);
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) Press(3);
            if (Input.GetKeyDown(KeyCode.R)) Replay();
            if (Input.GetKeyDown(KeyCode.H)) ShowHint();
            if (Input.GetKeyDown(KeyCode.Escape)) Close(false);
        }

        IEnumerator PlaySequence()
        {
            busy = true;
            yield return new WaitForSeconds(0.5f);
            for (int i = 0; i < sequence.Length; i++)
            {
                int sw = Mathf.Clamp(sequence[i], 0, swatches.Length - 1);
                yield return StartCoroutine(Flash(sw));
                yield return new WaitForSeconds(gapSeconds);
            }
            busy = false;
            inputIndex = 0;
            SetLocalized(feedbackText,
                "Your turn - repeat the pattern.",
                "دورك - كرّر النمط.");
            RenderProgress();
        }

        IEnumerator Flash(int index)
        {
            if (index < 0 || index >= swatches.Length || swatches[index] == null) yield break;
            Image sw = swatches[index];
            Color baseColor = baseColors != null && index < baseColors.Length ? baseColors[index] : sw.color;
            sw.color = Color.Lerp(baseColor, Color.white, 0.75f);
            yield return new WaitForSeconds(flashSeconds);
            if (sw != null) sw.color = baseColor;
        }

        void Replay()
        {
            if (busy) return;
            SetLocalized(feedbackText,
                "Watch the rig.",
                "راقب أضواء المسرح.");
            StartCoroutine(PlaySequence());
        }

        void Press(int index)
        {
            if (busy || inputIndex >= sequence.Length) return;

            StartCoroutine(Flash(index));

            if (index == sequence[inputIndex])
            {
                inputIndex++;
                RenderProgress();
                if (inputIndex >= sequence.Length)
                {
                    busy = true;
                    solving = true;
                    if (closeButton != null) closeButton.interactable = false;
                    SetLocalized(feedbackText,
                        "The whole rig blooms - somewhere a hatch rattles open.",
                        "أضاءت كل الأضواء - وفي مكان ما اهتزت فتحة مفتوحة.");
                    StartCoroutine(CloseAfterFeedback(1.4f));
                }
            }
            else
            {
                inputIndex = 0;
                SetLocalized(feedbackText,
                    "A wrong switch buzzes. The pattern resets.",
                    "أزّ المفتاح الخاطئ. عاد النمط من الأول.");
                RenderProgress();
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

        // P1: every programmed step must name a real switch. PlaySequence
        // clamps entries for display, but Press compares the raw entry, so an
        // out-of-range step can never be tapped and the overlay soft-locks.
        public static bool ValidSequence(int[] sequence, int switchCount)
        {
            if (sequence == null || sequence.Length == 0) return false;
            for (int i = 0; i < sequence.Length; i++)
                if (sequence[i] < 0 || sequence[i] >= switchCount) return false;
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
            if (!solved && !CanDiscard(solving)) return;
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
            RenderProgress();
        }

        void RenderStatics()
        {
            SetLocalized(titleText, titleEn, titleAr);
            bool arabic = Localization.Current == GameLanguage.Arabic;
            for (int i = 0; i < switchLabels.Length; i++)
            {
                string en = i < colorsEn.Length ? colorsEn[i] : "";
                string ar = i < colorsAr.Length ? colorsAr[i] : "";
                SetLocalized(switchLabels[i], (i + 1) + ". " + en, string.IsNullOrEmpty(ar) ? "" : (i + 1) + ". " + ar);
                if (switchLabels[i] != null)
                {
                    switchLabels[i].alignment = arabic ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
                }
            }
        }

        void RenderProgress()
        {
            SetLocalized(progressText,
                "Switch " + Mathf.Min(inputIndex + 1, sequence.Length) + " of " + sequence.Length,
                "مفتاح " + Mathf.Min(inputIndex + 1, sequence.Length) + " من " + sequence.Length);
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
