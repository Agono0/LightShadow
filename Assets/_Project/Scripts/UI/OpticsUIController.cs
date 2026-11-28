using LightShadow.Contrib.Abdelraouf.Overlay;
using System;
using System.Collections;
using LightShadow.Core;
using LightShadow.Player;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // Optics-bench overlay for the wing puzzles (finish-the-game plan,
    // workstream 5), first used by the science lab. The player rotates each
    // bench element in 90-degree steps and tests the beam. When every element
    // matches the calibration the beam lands (onSolved); a wrong test fizzles
    // and counts as a wrong move (meter penalty + noise hook through
    // PuzzleBase.WrongAttempt).
    //
    // Self-registers a static Instance in Awake (InteractionRaycaster pattern),
    // holds the story input lock while open, frees the cursor like the quiz UI.
    // Desktop: keys 1-3 rotate an element, Enter tests, H hint, Esc steps away.
    // Mobile: tap the element and test buttons.
    public class OpticsUIController : MonoBehaviour
    {
        public static OpticsUIController Instance { get; private set; }

        [Header("Wiring")]
        public GameObject panel;
        public Text titleText;
        public Text diagramText;
        public Button[] rotorButtons = new Button[3];
        public Text[] rotorLabels = new Text[3];
        public Button testButton;
        public Text feedbackText;
        public Text hintText;
        public Button hintButton;
        public Button closeButton;

        public bool IsOpen => panel != null && panel.activeSelf;

        string titleEn = "";
        string titleAr = "";
        string[] elementsEn = new string[0];
        string[] elementsAr = new string[0];
        int[] solution = new int[0];
        readonly int[] rotations = new int[3];
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

        // Arrays are read only; Arabic pools fall back to English per entry
        // when empty. onSolved fires once after a landed beam; onWrongAttempt
        // fires per fizzled test.
        public void Show(string titleEn, string titleAr,
            string[] elementsEn, string[] elementsAr, int[] solution,
            Func<string> hintProvider, Action onSolved, Action onWrongAttempt)
        {
            if (elementsEn == null || solution == null) return;
            if (elementsEn.Length != 3 || solution.Length != 3) return;
            if (!ValidSolution(solution, elementsEn.Length))
            {
                Debug.LogWarning("[OpticsUI] Show refused: every solution entry must be a rotation step 0..3.");
                return;
            }

            StopAllCoroutines();

            this.titleEn = titleEn ?? "";
            this.titleAr = titleAr ?? "";
            this.elementsEn = elementsEn;
            this.elementsAr = elementsAr ?? new string[0];
            this.solution = solution;
            this.hintProvider = hintProvider;
            this.onSolved = onSolved;
            this.onWrongAttempt = onWrongAttempt;

            for (int i = 0; i < rotations.Length; i++) rotations[i] = 0;
            busy = false;

            SetLocalized(feedbackText, "", "");
            SetLocalized(hintText, "", "");

            for (int i = 0; i < rotorButtons.Length; i++)
            {
                if (rotorButtons[i] == null) continue;
                rotorButtons[i].onClick.RemoveAllListeners();
                int index = i;
                rotorButtons[i].onClick.AddListener(() => Rotate(index));
            }
            if (testButton != null)
            {
                testButton.onClick.RemoveAllListeners();
                testButton.onClick.AddListener(Test);
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
            RenderRotors();

            if (panel != null) panel.SetActive(true);

            lockToken = OverlayStack.Acquire("OpticsUI", true, true, () => Close(false));
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

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) Rotate(0);
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) Rotate(1);
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) Rotate(2);
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Test();
            if (Input.GetKeyDown(KeyCode.H)) ShowHint();
            if (Input.GetKeyDown(KeyCode.Escape)) Close(false);
        }

        void Rotate(int index)
        {
            if (busy || index < 0 || index >= rotations.Length) return;
            rotations[index] = (rotations[index] + 1) % 4;
            LightShadow.Audio.Sfx.Play("SFX_MirrorTurn");
            RenderRotors();
        }

        void Test()
        {
            if (busy) return;

            bool aligned = true;
            for (int i = 0; i < rotations.Length; i++)
            {
                if (rotations[i] != solution[i])
                {
                    aligned = false;
                    break;
                }
            }

            if (aligned)
            {
                busy = true;
                SetLocalized(feedbackText,
                    "The beam lands on the sensor with a warm click.",
                    "أصاب الشعاع المستشعر بنقرة دافئة.");
                StartCoroutine(CloseAfterFeedback(1.4f));
            }
            else
            {
                SetLocalized(feedbackText,
                    "The beam scatters and dies before the sensor.",
                    "تبعثر الشعاع وخبت قبل المستشعر.");
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

        // P1: each rotor's target must be a legal quarter-turn (0..3); an
        // out-of-range target can never be matched and soft-locks the overlay.
        public static bool ValidSolution(int[] solution, int elementCount)
        {
            if (solution == null || solution.Length != elementCount) return false;
            for (int i = 0; i < solution.Length; i++)
                if (solution[i] < 0 || solution[i] > 3) return false;
            return true;
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
            RenderRotors();
        }

        void RenderStatics()
        {
            SetLocalized(titleText, titleEn, titleAr);
        }

        void RenderRotors()
        {
            bool arabic = Localization.Current == GameLanguage.Arabic;
            for (int i = 0; i < rotorLabels.Length; i++)
            {
                string en = i < elementsEn.Length ? elementsEn[i] : "";
                string ar = i < elementsAr.Length ? elementsAr[i] : "";
                SetLocalized(rotorLabels[i],
                    en + "  -  " + (rotations[i] * 90) + "°",
                    ar + "  -  " + (rotations[i] * 90) + "°");
                if (rotorLabels[i] != null)
                {
                    rotorLabels[i].alignment = arabic ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
                }
            }

            if (diagramText != null)
            {
                string beam =
                    ">> [" + rotations[0] * 90 + "°] >> [" + rotations[1] * 90 + "°] >> [" + rotations[2] * 90 + "°] >> ?";
                diagramText.text = beam;
                diagramText.font = Localization.CurrentFont;
            }
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
