using System;
using System.Collections;
using LightShadow.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LightShadow.Story
{
    // Bottom dialogue box for the story graph (brief #3): typewriter body with
    // punctuation-aware pauses, speaker tag, 2-4 choice buttons with hover /
    // press feedback and controller navigation, a shrinking bar for timed
    // choices, hold-to-skip and an auto-advance toggle.
    //
    // The controller is deliberately dumb: the StoryDirector decides which node
    // shows, which choices are offered and what every choice routes to. Layout
    // is built by Tools/Editor/SceneBuilderStory.cs in the SceneBuilderUI style
    // (dark card, warm text, uGUI). It coexists with the quiz overlay - the
    // director never opens a node while QuizUIController is open.
    public class DialogueUIController : MonoBehaviour
    {
        [Header("Wiring (built by SceneBuilderStory)")]
        public GameObject root;
        public GameObject hitPanel;          // click-anywhere-to-advance area
        public Text speakerText;
        public Text bodyText;
        public Image timerFill;              // shrinking bar for timed choices
        public Button[] choiceButtons = new Button[4];
        public Text[] choiceTexts = new Text[4];
        public Button skipButton;
        public Text skipLabel;
        public Button autoButton;
        public Text autoLabel;

        [Header("Feel (overridable by AccessibilityStore)")]
        public float autoAdvanceDelayPerChar = 0.018f;
        public float autoAdvanceMinDelay = 1.1f;
        public float choiceHoverScale = 1.045f;

        Action<int, bool> choiceCallback;
        Action advanceCallback;
        int[] offered = Array.Empty<int>();
        int timedOfferedIndex = -1;

        Coroutine typewriter;
        bool revealing;
        string fullText = "";
        bool choicesShown;
        float timerRemaining;
        float timerTotal;
        bool timerRunning;
        float skipHeld;
        bool skipPointerDown;
        float autoWait;
        bool autoWaiting;

        void Awake()
        {
            if (root != null) root.SetActive(false);

            // Runtime wiring for everything the builder creates. (Editor-time
            // AddListener does not survive a scene save, so the controller owns
            // its own plumbing.)
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                int slot = i;
                if (choiceButtons[i] != null) choiceButtons[i].onClick.AddListener(() => ChooseOffered(slot));
            }

            if (skipButton != null)
            {
                EventTrigger trigger = skipButton.gameObject.AddComponent<EventTrigger>();
                var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                down.callback.AddListener(_ => NotifySkipDown());
                var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                up.callback.AddListener(_ => NotifySkipUp());
                var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
                exit.callback.AddListener(_ => NotifySkipUp());
                trigger.triggers.Add(down);
                trigger.triggers.Add(up);
                trigger.triggers.Add(exit);
            }

            if (autoButton != null) autoButton.onClick.AddListener(ToggleAuto);

            if (hitPanel != null)
            {
                // The committed scene can carry the old flat box; the glass kit wins at runtime.
                UnityEngine.UI.Image hitImage = hitPanel.GetComponent<UnityEngine.UI.Image>();
                if (hitImage != null) LightShadow.UI.GlassSkin.ApplyPanel(hitImage, 0.97f);

                EventTrigger panelTrigger = hitPanel.gameObject.AddComponent<EventTrigger>();
                var click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
                click.callback.AddListener(_ => NotifyPanelPressed());
                panelTrigger.triggers.Add(click);
            }

            Localization.LanguageChanged += HandleLanguageChanged;
        }

        // Choice button entry point (slot = index into the offered list).
        public void ChooseOffered(int slot)
        {
            if (!choicesShown || slot < 0 || slot >= offered.Length) return;
            choiceCallback?.Invoke(slot, false);
        }

        void OnDestroy()
        {
            Localization.LanguageChanged -= HandleLanguageChanged;
        }

        void HandleLanguageChanged()
        {
            // Re-render the current node in the new language (restart typewriter).
            if (root == null || !root.activeSelf || currentNode == null) return;
            RenderNode();
        }

        // Current node copy for the language-change re-render.
        StoryNode currentNode;

        // ------------------------------------------------------------- show --

        public void ShowNode(StoryNode node, int[] offeredIndices, int timedIndex,
            Action<int, bool> onChoice, Action onAdvance)
        {
            if (root == null || node == null) return;

            currentNode = node;
            offered = offeredIndices ?? Array.Empty<int>();
            timedOfferedIndex = timedIndex;
            choiceCallback = onChoice;
            advanceCallback = onAdvance;

            RenderNode();

            root.SetActive(true);
            choicesShown = false;
            timerRunning = false;
        }

        void RenderNode()
        {
            if (currentNode == null) return;

            bool arabic = Localization.Current == GameLanguage.Arabic;
            Font font = Localization.CurrentFont;

            if (speakerText != null)
            {
                speakerText.text = StoryStrings.Tr(StoryStrings.SpeakerName(currentNode.speaker));
                speakerText.font = font;
                speakerText.color = SpeakerColor(currentNode.speaker);
                speakerText.alignment = arabic ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
            }

            if (bodyText != null)
            {
                bodyText.font = font;
                bodyText.alignment = arabic ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
                bodyText.fontSize = Mathf.RoundToInt(24f * AccessibilityStore.SubtitleScale);
                fullText = StoryStrings.Tr(currentNode.text);
            }

            HideChoices();

            if (typewriter != null) StopCoroutine(typewriter);
            revealing = true;
            typewriter = StartCoroutine(Typewriter(fullText));

            if (skipLabel != null)
            {
                skipLabel.text = StoryStrings.Tr("HOLD TO SKIP");
                skipLabel.font = font;
            }
            if (autoLabel != null)
            {
                autoLabel.text = StoryStrings.Tr("AUTO-ADVANCE") + "  " + Localization.PresentBool(AccessibilityStore.AutoAdvance);
                autoLabel.font = font;
            }
        }

        static Color SpeakerColor(StorySpeaker speaker)
        {
            switch (speaker)
            {
                case StorySpeaker.Mother: return new Color(1f, 0.86f, 0.62f);   // warm amber
                case StorySpeaker.Shadow: return new Color(0.72f, 0.66f, 0.80f); // pale violet-grey
                case StorySpeaker.System: return new Color(0.68f, 0.65f, 0.60f); // dim
                default: return new Color(0.93f, 0.91f, 0.82f);                  // paper
            }
        }

        // Characters per second that spread the line over most of the voice
        // clip (punctuation pauses take the rest). Pure, tested.
        public static float VoiceSyncedCharsPerSecond(string text, float fallbackCps, float voiceSeconds)
        {
            if (string.IsNullOrEmpty(text) || voiceSeconds < 0.5f) return fallbackCps;
            float synced = text.Length / (voiceSeconds * 0.8f);
            return Mathf.Clamp(synced, 8f, Mathf.Max(8f, fallbackCps * 3f));
        }

        IEnumerator Typewriter(string text)
        {
            if (bodyText == null) yield break;

            float cps = AccessibilityStore.TypewriterCharsPerSecond;
            if (float.IsInfinity(cps))
            {
                bodyText.text = Localization.Present(text);
                revealing = false;
                OnRevealDone();
                yield break;
            }

            // Voice sync (owner brief 2026-11-28): when a voice clip plays for this
            // line, type at the pace that finishes with the voice, so the words
            // appear as Nour says them. Never slower than the clip, never faster
            // than the player's chosen speed allows a read.
            cps = VoiceSyncedCharsPerSecond(text, cps, LightShadow.Core.VoiceoverPlayer.LastPlayedSeconds);

            var builder = new System.Text.StringBuilder(text.Length);
            int i = 0;
            while (i < text.Length)
            {
                char c = text[i];
                builder.Append(c);
                // Arabic is shaped per finished word, not per character (audit
                // 2.5): reshaping a growing string is O(n^2) and the letters
                // visibly change form as the word grows.
                if (Localization.Current != GameLanguage.Arabic || char.IsWhiteSpace(c) || i == text.Length - 1)
                {
                    bodyText.text = Localization.Present(builder.ToString());
                }
                i++;

                // Punctuation-aware pauses so delivery breathes (matches the
                // StoryTeller SAPI pauses).
                float pause = 0f;
                switch (c)
                {
                    case '.':
                    case '!':
                    case '?':
                    case '…': pause = 0.28f; break;
                    case ',':
                    case '،': pause = 0.14f; break;
                    case ':':
                    case ';': pause = 0.20f; break;
                    case '\n': pause = 0.22f; break;
                }

                if (pause > 0f)
                {
                    float t = 0f;
                    while (t < pause && !SkipRequested()) { t += Time.unscaledDeltaTime; yield return null; }
                }
                else
                {
                    // Chars per second, but never wait on the same frame twice
                    // for very high speeds.
                    float step = 1f / cps;
                    if (step > Time.unscaledDeltaTime)
                    {
                        float t = 0f;
                        while (t < step && !SkipRequested()) { t += Time.unscaledDeltaTime; yield return null; }
                    }
                }
            }

            bodyText.text = Localization.Present(text);
            revealing = false;
            OnRevealDone();
        }

        bool SkipRequested()
        {
            return (skipPointerDown || Input.GetKey(KeyCode.Space))
                && skipHeld > 0f;
        }

        void OnRevealDone()
        {
            ShowChoices();

            if (AccessibilityStore.AutoAdvance && offered.Length == 0 && advanceCallback != null)
            {
                autoWait = Mathf.Max(autoAdvanceMinDelay, fullText.Length * autoAdvanceDelayPerChar);
                autoWaiting = true;
            }
        }

        void ShowChoices()
        {
            choicesShown = true;

            bool arabic = Localization.Current == GameLanguage.Arabic;
            Font font = Localization.CurrentFont;

            for (int slot = 0; slot < choiceButtons.Length; slot++)
            {
                Button button = choiceButtons[slot];
                if (button == null) continue;

                if (slot < offered.Length)
                {
                    StoryNode.StoryChoice choice = currentNode.choices[offered[slot]];
                    if (choiceTexts != null && slot < choiceTexts.Length && choiceTexts[slot] != null)
                    {
                        int number = slot + 1;
                        string label = Localization.OptionLabel(number - 1, StoryStrings.Get(choice.text));
                        choiceTexts[slot].text = label;
                        choiceTexts[slot].font = font;
                        choiceTexts[slot].alignment = arabic ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
                    }

                    button.gameObject.SetActive(true);
                    AttachScaleFeedback(button);

                    // Highlight the timed row with the palette warning colour.
                    if (slot == timedOfferedIndex && timerFill != null)
                    {
                        timerFill.gameObject.SetActive(true);
                    }
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }

            // Timed choice clock starts once the text has been read.
            if (timedOfferedIndex >= 0 && timedOfferedIndex < offered.Length)
            {
                StoryNode.StoryChoice timed = currentNode.choices[offered[timedOfferedIndex]];
                timerTotal = Mathf.Max(1f, timed.timeSeconds);
                timerRemaining = timerTotal;
                timerRunning = true;
            }
            else if (timerFill != null)
            {
                timerFill.gameObject.SetActive(false);
            }

            if (EventSystem.current != null && choiceButtons.Length > 0
                && choiceButtons[0] != null && choiceButtons[0].gameObject.activeSelf)
            {
                EventSystem.current.SetSelectedGameObject(choiceButtons[0].gameObject);
            }
        }

        void HideChoices()
        {
            choicesShown = false;
            autoWaiting = false;
            timerRunning = false;
            if (timerFill != null) timerFill.gameObject.SetActive(false);
            if (choiceButtons != null)
            {
                for (int i = 0; i < choiceButtons.Length; i++)
                {
                    if (choiceButtons[i] != null) choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }

        public void Hide()
        {
            if (typewriter != null) { StopCoroutine(typewriter); typewriter = null; }
            HideChoices();
            currentNode = null;
            if (root != null) root.SetActive(false);
        }

        // ----------------------------------------------------------- update --

        void Update()
        {
            if (root == null || !root.activeSelf) return;

            // ---- skip (hold) -------------------------------------------------
            bool skipHeldNow = (skipPointerDown || Input.GetKey(KeyCode.Space));
            if (skipHeldNow)
            {
                skipHeld += Time.unscaledDeltaTime;
                if (skipHeld >= AccessibilityStore.SkipHoldSeconds)
                {
                    if (revealing)
                    {
                        // Jump straight to the full text.
                        if (typewriter != null) StopCoroutine(typewriter);
                        typewriter = null;
                        revealing = false;
                        if (bodyText != null) bodyText.text = Localization.Present(fullText);
                        OnRevealDone();
                    }
                    else if (offered.Length == 0 && advanceCallback != null)
                    {
                        advanceCallback();
                    }
                    skipHeld = float.MinValue / 2f; // don't retrigger while held
                }
            }
            else if (skipHeld < 0f)
            {
                skipHeld = 0f; // released after a completed skip; re-arm
            }
            else
            {
                skipHeld = 0f;
            }

            // ---- timed choice bar --------------------------------------------
            if (timerRunning)
            {
                timerRemaining -= Time.unscaledDeltaTime;
                if (timerFill != null)
                {
                    timerFill.fillAmount = Mathf.Clamp01(timerRemaining / Mathf.Max(0.01f, timerTotal));
                    timerFill.color = AccessibilityStore.PaletteColor(AccessibilityStore.PaletteSlot.Warning);
                }

                if (timerRemaining <= 0f)
                {
                    timerRunning = false;
                    int timedOffered = timedOfferedIndex;
                    timedOfferedIndex = -1;
                    if (choiceCallback != null && timedOffered >= 0) choiceCallback(timedOffered, true);
                }
            }

            // ---- auto-advance (non-choice nodes only) -------------------------
            if (autoWaiting)
            {
                autoWait -= Time.unscaledDeltaTime;
                if (autoWait <= 0f)
                {
                    autoWaiting = false;
                    advanceCallback?.Invoke();
                }
            }

            // ---- keyboard: reveal / advance / pick by number -------------------
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)
                || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (revealing)
                {
                    if (typewriter != null) StopCoroutine(typewriter);
                    typewriter = null;
                    revealing = false;
                    if (bodyText != null) bodyText.text = Localization.Present(fullText);
                    OnRevealDone();
                }
                else if (offered.Length == 0 && advanceCallback != null)
                {
                    advanceCallback();
                }
            }

            for (int i = 0; i < 4; i++)
            {
                KeyCode key = KeyCode.Alpha1 + i;
                if (!Input.GetKeyDown(key)) continue;
                if (!choicesShown || i >= offered.Length) continue;
                choiceCallback?.Invoke(i, false);
                break;
            }
        }

        // --------------------------------------------------- pointer plumbing --

        // Click on the box (not on a choice) reveals or advances.
        public void NotifyPanelPressed()
        {
            if (revealing)
            {
                if (typewriter != null) StopCoroutine(typewriter);
                typewriter = null;
                revealing = false;
                if (bodyText != null) bodyText.text = Localization.Present(fullText);
                OnRevealDone();
            }
            else if (offered.Length == 0 && advanceCallback != null)
            {
                advanceCallback();
            }
        }

        // Hold-to-skip button plumbing (PointerDown/Up via EventTrigger entries).
        public void NotifySkipDown() => skipPointerDown = true;
        public void NotifySkipUp() => skipPointerDown = false;

        public void ToggleAuto()
        {
            AccessibilityStore.ToggleAutoAdvance();
            if (autoLabel != null)
            {
                autoLabel.text = StoryStrings.Tr("AUTO-ADVANCE") + "  " + Localization.PresentBool(AccessibilityStore.AutoAdvance);
            }
        }

        // Hover/press scale feedback on choice rows (in addition to the standard
        // ColorBlock tint the builder already sets).
        void AttachScaleFeedback(Button button)
        {
            if (button.GetComponent<ChoiceScaleFeedback>() != null) return;
            var feedback = button.gameObject.AddComponent<ChoiceScaleFeedback>();
            feedback.scale = choiceHoverScale;
        }
    }

    // Small selectable-feedback helper: grows the row while hovered or selected,
    // shrinks on press. Kept internal to this file - it exists only for the
    // dialogue choice rows.
    public class ChoiceScaleFeedback : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        public float scale = 1.045f;
        public float speed = 10f;

        Vector3 baseScale = Vector3.one;
        float target = 1f;
        bool pressed;

        void OnEnable() => baseScale = Vector3.one;

        public void OnPointerEnter(PointerEventData eventData) => target = scale;
        public void OnPointerExit(PointerEventData eventData) => target = EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject ? scale : 1f;
        public void OnSelect(BaseEventData eventData) => target = scale;
        public void OnDeselect(BaseEventData eventData) => target = pressed ? scale : 1f;
        public void OnPointerDown(PointerEventData eventData) { pressed = true; target = 0.97f; }
        public void OnPointerUp(PointerEventData eventData) { pressed = false; target = 1f; }

        void Update()
        {
            Vector3 current = transform.localScale;
            Vector3 goal = baseScale * target;
            if ((current - goal).sqrMagnitude > 0.000001f)
            {
                transform.localScale = Vector3.Lerp(current, goal, Mathf.Clamp01(speed * Time.unscaledDeltaTime));
            }
        }
    }
}
