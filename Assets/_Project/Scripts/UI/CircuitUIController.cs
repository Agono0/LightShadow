using System;
using System.Collections;
using LightShadow.Core;
using LightShadow.Player;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // Wiring-board overlay for the finale puzzle (finish-the-game plan,
    // workstream 5): wing feeds on the left, bus terminals on the right. The
    // player picks a feed, then a terminal; a right pair locks with a clunk,
    // a wrong one sparks and counts as a wrong move (meter penalty + noise
    // hook through PuzzleBase.WrongAttempt). Routing every feed solves.
    //
    // Self-registers a static Instance in Awake (InteractionRaycaster pattern),
    // holds the story input lock while open, frees the cursor like the quiz UI.
    // Desktop: 1-3 pick a feed, Q/W/E pick a terminal, H hint, Esc steps away.
    // Mobile: tap feed then terminal.
    public class CircuitUIController : MonoBehaviour
    {
        public static CircuitUIController Instance { get; private set; }

        [Header("Wiring")]
        public GameObject panel;
        public Text titleText;
        public Text progressText;
        public Button[] feedButtons = new Button[3];
        public Text[] feedLabels = new Text[3];
        public Button[] terminalButtons = new Button[3];
        public Text[] terminalLabels = new Text[3];
        public Text feedbackText;
        public Text hintText;
        public Button hintButton;
        public Button closeButton;

        public bool IsOpen => panel != null && panel.activeSelf;

        string titleEn = "";
        string titleAr = "";
        string[] feedsEn = new string[0];
        string[] feedsAr = new string[0];
        string[] terminalsEn = new string[0];
        string[] terminalsAr = new string[0];
        int[] mapping = new int[0];
        readonly bool[] lockedFeeds = new bool[3];
        readonly bool[] lockedTerminals = new bool[3];
        int selectedFeed = -1;
        int lockedCount;
        Func<string> hintProvider;
        Action onSolved;
        Action onWrongAttempt;
        bool busy;
        bool cursorWasLocked;

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
        // when empty. onSolved fires once after the last pair locks;
        // onWrongAttempt fires per wrong pairing.
        public void Show(string titleEn, string titleAr,
            string[] feedsEn, string[] feedsAr, string[] terminalsEn, string[] terminalsAr, int[] mapping,
            Func<string> hintProvider, Action onSolved, Action onWrongAttempt)
        {
            if (feedsEn == null || terminalsEn == null || mapping == null) return;
            if (feedsEn.Length != 3 || terminalsEn.Length != 3 || mapping.Length != 3) return;

            StopAllCoroutines();

            this.titleEn = titleEn ?? "";
            this.titleAr = titleAr ?? "";
            this.feedsEn = feedsEn;
            this.feedsAr = feedsAr ?? new string[0];
            this.terminalsEn = terminalsEn;
            this.terminalsAr = terminalsAr ?? new string[0];
            this.mapping = mapping;
            this.hintProvider = hintProvider;
            this.onSolved = onSolved;
            this.onWrongAttempt = onWrongAttempt;

            for (int i = 0; i < 3; i++)
            {
                lockedFeeds[i] = false;
                lockedTerminals[i] = false;
                if (feedButtons[i] != null) feedButtons[i].interactable = true;
                if (terminalButtons[i] != null) terminalButtons[i].interactable = true;
            }
            selectedFeed = -1;
            lockedCount = 0;
            busy = false;

            SetLocalized(feedbackText,
                "Pick a feed, then its terminal.",
                "اختر تغذية، ثم طرفها.");
            SetLocalized(hintText, "", "");

            for (int i = 0; i < feedButtons.Length; i++)
            {
                if (feedButtons[i] == null) continue;
                feedButtons[i].onClick.RemoveAllListeners();
                int index = i;
                feedButtons[i].onClick.AddListener(() => PickFeed(index));
            }
            for (int i = 0; i < terminalButtons.Length; i++)
            {
                if (terminalButtons[i] == null) continue;
                terminalButtons[i].onClick.RemoveAllListeners();
                int index = i;
                terminalButtons[i].onClick.AddListener(() => PickTerminal(index));
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
            RenderProgress();

            if (panel != null) panel.SetActive(true);

            PlayerController.ExternalInputLocked = true;
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

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) PickFeed(0);
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) PickFeed(1);
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) PickFeed(2);
            if (Input.GetKeyDown(KeyCode.Q)) PickTerminal(0);
            if (Input.GetKeyDown(KeyCode.W)) PickTerminal(1);
            if (Input.GetKeyDown(KeyCode.E)) PickTerminal(2);
            if (Input.GetKeyDown(KeyCode.H)) ShowHint();
            if (Input.GetKeyDown(KeyCode.Escape)) Close(false);
        }

        void PickFeed(int index)
        {
            if (busy || index < 0 || index >= 3 || lockedFeeds[index]) return;
            selectedFeed = index;
            string feed = LocalName(feedsEn, feedsAr, index);
            SetLocalized(feedbackText,
                feed + " held. Now its terminal.",
                feed + " بيدك. الآن طرفه.");
        }

        void PickTerminal(int index)
        {
            if (busy || index < 0 || index >= 3 || lockedTerminals[index]) return;
            if (selectedFeed < 0)
            {
                SetLocalized(feedbackText,
                    "Pick a feed first.",
                    "اختر تغذية أولًا.");
                return;
            }

            int feed = selectedFeed;
            selectedFeed = -1;

            if (mapping[feed] == index)
            {
                lockedFeeds[feed] = true;
                lockedTerminals[index] = true;
                if (feedButtons[feed] != null) feedButtons[feed].interactable = false;
                if (terminalButtons[index] != null) terminalButtons[index].interactable = false;
                lockedCount++;

                if (lockedCount >= 3)
                {
                    busy = true;
                    SetLocalized(feedbackText,
                        "Every bus hums. The board is live.",
                        "كل الخطوط تطن. اللوحة حية.");
                    StartCoroutine(CloseAfterFeedback(1.4f));
                }
                else
                {
                    SetLocalized(feedbackText,
                        LocalName(feedsEn, feedsAr, feed) + " -> " + LocalName(terminalsEn, terminalsAr, index) + ". A clunk, and the bus wakes.",
                        LocalName(feedsEn, feedsAr, feed) + " -> " + LocalName(terminalsEn, terminalsAr, index) + ". طقطقة، واستيقظ الخط.");
                }
                RenderProgress();
            }
            else
            {
                SetLocalized(feedbackText,
                    "A spark spits from the board. Not that pair.",
                    "قذفت اللوحة شرارة. ليس هذا الزوج.");
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
            PlayerController.ExternalInputLocked = false;
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
            for (int i = 0; i < feedLabels.Length; i++)
            {
                SetLocalized(feedLabels[i], (i + 1) + ". " + (i < feedsEn.Length ? feedsEn[i] : ""),
                    i < feedsAr.Length ? feedsAr[i] : "");
                if (feedLabels[i] != null)
                {
                    feedLabels[i].alignment = arabic ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
                }
            }
            string[] termKeys = { "Q", "W", "E" };
            for (int i = 0; i < terminalLabels.Length; i++)
            {
                SetLocalized(terminalLabels[i], termKeys[i] + ". " + (i < terminalsEn.Length ? terminalsEn[i] : ""),
                    i < terminalsAr.Length ? terminalsAr[i] : "");
                if (terminalLabels[i] != null)
                {
                    terminalLabels[i].alignment = arabic ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
                }
            }
        }

        void RenderProgress()
        {
            SetLocalized(progressText,
                "Connections " + lockedCount + " of 3",
                "وصلات " + lockedCount + " من 3");
        }

        string LocalName(string[] en, string[] ar, int index)
        {
            bool useArabic = Localization.Current == GameLanguage.Arabic
                && ar != null && index < ar.Length && !string.IsNullOrEmpty(ar[index]);
            string name = useArabic ? ar[index] : (index < en.Length ? en[index] : "");
            return useArabic ? Localization.Present(name) : name;
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
