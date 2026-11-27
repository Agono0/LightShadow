using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LightShadow.Core;
using LightShadow.Player;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // The heads-up display: meter "light" bar, battery bar, interaction prompt,
    // chalk messages, the Tab objectives list, and the fade used by being caught
    // and by the dawn escape (Doc 4.3).
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance { get; private set; }

        [Header("Wiring")]
        public Image meterFill;
        public Image batteryFill;
        public Image healthFill;
        public Text promptText;
        public Text messageText;
        public GameObject objectivesPanel;
        public Text objectivesText;
        public GameObject crosshair;
        public Image fadeImage;

        Coroutine fadeRoutine;
        float messageTimer;
        bool objectivesShown;

        // Raw English sources of the on-screen prompt and message, cached so a
        // language switch can re-render them in place (HUD review, 5.4b family).
        string lastPromptSource = "";
        string lastMessageSource = "";

        // Story beats queue behind whatever is on screen, so a memory flash and
        // its follow-up line both play without overwriting each other.
        struct QueuedMessage
        {
            public string text;
            public float seconds;
        }

        readonly Queue<QueuedMessage> messageQueue = new Queue<QueuedMessage>();

        void Awake()
        {
            Instance = this;
            if (promptText != null) promptText.text = "";
            if (messageText != null) messageText.gameObject.SetActive(false);
            if (objectivesPanel != null) objectivesPanel.SetActive(false);
            SetFadeAlpha(0f);

            Localization.LanguageChanged += ApplyLanguageUI;
            ApplyLanguageUI();
        }

        void Start()
        {
            if (Application.isMobilePlatform && crosshair != null) crosshair.SetActive(false);

            GameManager gm = GameManager.Instance;
            if (gm != null)
            {
                BrightIdeaMeter meter = gm.GetMeter();
                if (meter != null)
                {
                    meter.OnChanged.AddListener(OnMeterChanged);
                    // Paint immediately: the start-of-scene invoke can land
                    // before the HUD subscribes, leaving a stale bar.
                    OnMeterChanged(meter.Normalized);
                }

                FlashlightBattery flashlight = gm.player != null
                    ? gm.player.GetComponentInChildren<FlashlightBattery>()
                    : null;
                if (flashlight != null)
                {
                    flashlight.OnBatteryChanged.AddListener(OnBatteryChanged);
                    flashlight.OnBatteryEmpty.AddListener(OnBatteryEmpty);
                    OnBatteryChanged(flashlight.Battery01);
                }

                PlayerHealth health = gm.player != null ? gm.player.GetComponentInChildren<PlayerHealth>() : null;
                if (health != null)
                {
                    health.Changed += OnHealthChanged;
                    health.Hit += OnHealthHit;
                    // Paint immediately: the bar must never sit stale (battery
                    // bar lesson, PR #49).
                    OnHealthChanged(health.Normalized);
                }
            }

            RefreshObjectives();
        }

        void OnDestroy()
        {
            Localization.LanguageChanged -= ApplyLanguageUI;

            if (Instance == this) Instance = null;

            GameManager gm = GameManager.Instance;
            if (gm != null)
            {
                BrightIdeaMeter meter = gm.GetMeter();
                if (meter != null) meter.OnChanged.RemoveListener(OnMeterChanged);

                FlashlightBattery flashlight = gm.player != null
                    ? gm.player.GetComponentInChildren<FlashlightBattery>()
                    : null;
                if (flashlight != null)
                {
                    flashlight.OnBatteryChanged.RemoveListener(OnBatteryChanged);
                    flashlight.OnBatteryEmpty.RemoveListener(OnBatteryEmpty);
                }

                PlayerHealth health = gm.player != null ? gm.player.GetComponentInChildren<PlayerHealth>() : null;
                if (health != null)
                {
                    health.Changed -= OnHealthChanged;
                    health.Hit -= OnHealthHit;
                }
            }
        }

        void Update()
        {
            // Tab peeks at the objective list; the touch button calls ToggleObjectives().
            if (Input.GetKeyDown(KeyCode.Tab)) ToggleObjectives();

            if (messageTimer > 0f)
            {
                messageTimer -= Time.deltaTime;
                if (messageTimer <= 0f)
                {
                    if (messageQueue.Count > 0)
                    {
                        QueuedMessage next = messageQueue.Dequeue();
                        ShowMessage(next.text, next.seconds);
                    }
                    else if (messageText != null)
                    {
                        messageText.gameObject.SetActive(false);
                    }
                }
            }
        }

        // ---- Bars --------------------------------------------------------------

        void OnMeterChanged(float normalized)
        {
            if (meterFill != null) meterFill.fillAmount = normalized;
        }

        void OnBatteryChanged(float value)
        {
            if (batteryFill != null) batteryFill.fillAmount = value;
        }

        void OnHealthChanged(float normalized)
        {
            if (healthFill != null) healthFill.fillAmount = normalized;
        }

        // Survived strike: a red pulse over the screen (the lethal strike goes
        // through GameManager's slower black fade instead).
        void OnHealthHit()
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(HurtFlashRoutine());
        }

        IEnumerator HurtFlashRoutine()
        {
            if (fadeImage != null) fadeImage.color = new Color(0.55f, 0.05f, 0.03f, fadeImage.color.a);
            SetFadeAlpha(0.38f);
            float elapsed = 0f;
            while (elapsed < 0.7f)
            {
                elapsed += Time.unscaledDeltaTime;
                SetFadeAlpha(Mathf.Lerp(0.38f, 0f, Mathf.Clamp01(elapsed / 0.7f)));
                yield return null;
            }
            SetFadeAlpha(0f);
            // Restore black so the next FadeToBlack does not fade in red.
            if (fadeImage != null) fadeImage.color = new Color(0f, 0f, 0f, fadeImage.color.a);
            fadeRoutine = null;
        }

        void OnBatteryEmpty()
        {
            ShowMessage("The crank light died. Find a battery.", 4f);
        }

        // ---- Prompt and messages -----------------------------------------------

        void ApplyLanguageUI()
        {
            bool arabic = Localization.Current == GameLanguage.Arabic;
            Font font = Localization.CurrentFont;

            if (promptText != null)
            {
                promptText.font = font;
                promptText.alignment = arabic ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
                // Re-render the cached source: a mid-game language switch must
                // not leave the previous language on screen (HUD review).
                promptText.text = string.IsNullOrEmpty(lastPromptSource)
                    ? "" : Localization.TrPresent(lastPromptSource);
            }

            if (messageText != null)
            {
                messageText.font = font;
                messageText.alignment = arabic ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
                if (!string.IsNullOrEmpty(lastMessageSource))
                {
                    messageText.text = Localization.TrPresent(lastMessageSource);
                }
            }

            if (objectivesText != null)
            {
                objectivesText.font = font;
                objectivesText.alignment = arabic ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
            }

            // The objectives lines are built from templates too; rebuild them
            // for the new language (null-safe without a GameManager).
            RefreshObjectives();
        }

        public void SetPrompt(string text)
        {
            if (promptText == null) return;
            // Prompts arrive as the English source sentence; TrPresent maps and
            // shapes them for the active language.
            lastPromptSource = text ?? "";
            string shown = string.IsNullOrEmpty(lastPromptSource) ? "" : Localization.TrPresent(lastPromptSource);
            if (promptText.text == shown) return;
            promptText.text = shown;
        }

        public void ShowMessage(string text, float seconds = 4f)
        {
            if (messageText == null || string.IsNullOrEmpty(text)) return;
            // An urgent message takes over: clear any stale queued beats.
            messageQueue.Clear();
            lastMessageSource = text;
            messageText.text = Localization.TrPresent(text);
            ApplyLanguageUI();
            messageText.gameObject.SetActive(true);
            messageTimer = seconds;
        }

        // Story beats: shown after the current message finishes instead of
        // replacing it.
        public void QueueMessage(string text, float seconds = 4f)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (messageTimer <= 0f) ShowMessage(text, seconds);
            else messageQueue.Enqueue(new QueuedMessage { text = text, seconds = seconds });
        }

        // ---- Objectives list (Tab / touch button) --------------------------------

        public void ToggleObjectives()
        {
            objectivesShown = !objectivesShown;
            if (objectivesPanel != null) objectivesPanel.SetActive(objectivesShown);
            if (objectivesShown) RefreshObjectives();
        }

        public void RefreshObjectives()
        {
            if (objectivesText == null) return;

            GameManager gm = GameManager.Instance;
            if (gm == null) return;

            bool fusesDone = gm.FusesRestored >= GameManager.FuseTarget;
            bool fragmentsDone = gm.FragmentsCollected >= GameManager.FragmentTarget;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(Localization.Format(fusesDone
                ? "Restore fuse boxes: {0}/{1}  (done)"
                : "Restore fuse boxes: {0}/{1}", gm.FusesRestored, GameManager.FuseTarget));

            builder.AppendLine(Localization.Format(fragmentsDone
                ? "Collect Knowledge Fragments: {0}/{1}  (done)"
                : "Collect Knowledge Fragments: {0}/{1}", gm.FragmentsCollected, GameManager.FragmentTarget));

            builder.AppendLine(Localization.Format(gm.CanPowerGate
                ? "Power the main gate  (the crank is live)"
                : "Power the main gate  (three fuse boxes first)"));

            builder.Append(gm.GatePowered
                ? Localization.TrPresent("Escape at dawn  (go now)")
                : Localization.TrPresent("Escape at dawn"));

            objectivesText.text = builder.ToString();
        }

        // ---- Fade ----------------------------------------------------------------

        // Fades to black over 'seconds', then calls onBlack (checkpoint hop, recap).
        public void FadeToBlack(float seconds, Action onBlack)
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeRoutine(1f, seconds, onBlack));
        }

        public void FadeIn(float seconds)
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeRoutine(0f, seconds, null));
        }

        IEnumerator FadeRoutine(float target, float seconds, Action onDone)
        {
            if (fadeImage != null)
            {
                float start = fadeImage.color.a;
                float elapsed = 0f;
                while (elapsed < seconds)
                {
                    // Unscaled so the fade still plays if the world is frozen.
                    elapsed += Time.unscaledDeltaTime;
                    SetFadeAlpha(Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / Mathf.Max(0.01f, seconds))));
                    yield return null;
                }
            }

            SetFadeAlpha(target);
            if (onDone != null) onDone();
        }

        void SetFadeAlpha(float alpha)
        {
            if (fadeImage == null) return;
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
            fadeImage.gameObject.SetActive(alpha > 0.001f);
        }
    }
}
