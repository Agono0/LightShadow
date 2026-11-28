using System.Collections;
using LightShadow.Core;
using LightShadow.Story;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // Fullscreen dark loading overlay used for every scene transition the UI
    // owns (brief #5): dark panel, a small Al-Noor silhouette motif drawn from
    // plain uGUI rectangles (no texture assets), an async progress bar, and a
    // "demolition at dawn" caption.
    //
    // Static entry points:
    //   LoadingScreen.Transition(sceneName) - fade in, async load, fade out.
    //   LoadingScreen.Show()                - just display the overlay (Core can
    //                                         call this if it ever wants the
    //                                         screen; Core keeps its own flow).
    //
    // Everything runs on unscaled time and re-creates itself on demand, so no
    // scene wiring is required.
    public class LoadingScreen : MonoBehaviour
    {
        public static LoadingScreen Instance { get; private set; }

        const float FadeSeconds = 0.3f;
        const float MinimumShowSeconds = 0.8f;
        const float StallTimeoutSeconds = 20f;
        const int SortOrder = 32000;

        Canvas canvas;
        CanvasGroup group;
        Image progressFill;
        Image beam;
        Text tipText;
        float beamClock;
        Text progressText;
        Text captionText;

        public static void Transition(string sceneName)
        {
            LoadingScreen screen = EnsureCreated();
            if (screen == null)
            {
                // No way to build UI (should not happen): load synchronously.
                SceneManager.LoadScene(sceneName);
                return;
            }
            screen.RunTransition(sceneName);
        }

        public static void Show()
        {
            LoadingScreen screen = EnsureCreated();
            if (screen != null) screen.StartCoroutine(screen.FadeTo(1f, FadeSeconds));
        }

        public static void Hide()
        {
            if (Instance == null) return;
            Instance.StartCoroutine(Instance.FadeTo(0f, FadeSeconds));
        }

        static LoadingScreen EnsureCreated()
        {
            if (Instance != null) return Instance;

            var owner = new GameObject("LoadingScreen");
            DontDestroyOnLoad(owner);
            return owner.AddComponent<LoadingScreen>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Localization.LanguageChanged += RefreshLanguage;
            BuildUI();
            group.alpha = 0f;
            canvas.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            Localization.LanguageChanged -= RefreshLanguage;
            if (Instance == this) Instance = null;
        }

        // audit v3 5.4: this canvas persists across scene loads
        // (DontDestroyOnLoad in EnsureCreated), so a language switch while it
        // exists must re-translate and re-font the visible strings in place or
        // they stay in the previous language until the object is destroyed.
        public void RefreshLanguage()
        {
            Font font = Localization.CurrentFont;
            if (captionText != null)
            {
                captionText.text = StoryStrings.Tr(CaptionKey);
                captionText.font = font;
            }
            if (tipText != null)
            {
                tipText.text = StoryStrings.Tr(TipKey);
                tipText.font = font;
            }
            if (progressText != null)
            {
                // Only the static label is language-dependent; the live
                // percent readout rewrites itself every frame while loading.
                if (progressLabelShowing) progressText.text = StoryStrings.Tr("Loading...");
                progressText.font = font;
            }
        }

        void Update()
        {
            if (group == null || group.alpha <= 0.01f) return;
            // The beam drifts across the school front and eases back - a
            // searchlight looking for whoever is still inside.
            beamClock += Time.unscaledDeltaTime;
            float sweep = Mathf.PingPong(beamClock * 0.22f, 1f) * 2f - 1f;
            if (beam != null)
            {
                beam.rectTransform.anchoredPosition = new Vector2(sweep * 760f, 80f);
                beam.color = new Color(0.80f, 0.87f, 1f, Mathf.Lerp(0.10f, 0.22f, (sweep + 1f) / 2f));
            }
        }

        void BuildUI()
        {
            var canvasGo = new GameObject("UI_Loading", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);

            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortOrder;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            group = canvasGo.AddComponent<CanvasGroup>();

            // Redesigned loading look: gradient night, the school silhouette,
            // and a flashlight beam that sweeps across it while you wait.
            var background = MakeRect(canvasGo.transform, "Background", Vector2.zero, Vector2.one, Color.white);
            // Night art ships as a Resources copy of T_UI_Loading_Night (see
            // ASSET_MANIFEST row T_UI_Loading_Night_Res); the gradient stays as fallback.
            Sprite nightArt = Resources.Load<Sprite>("UI/T_UI_Loading_Night");
            background.sprite = nightArt != null
                ? nightArt
                : LoadingTheme.Sprite(LoadingTheme.Gradient, "LoadingGradient", Vector4.zero);
            background.type = Image.Type.Simple;

            BuildSilhouette(canvasGo.transform);

            // The sweeping flashlight: a soft glow panning across the skyline.
            beam = MakeRect(canvasGo.transform, "BeamSweep", Vector2.zero, Vector2.one, new Color(0.80f, 0.87f, 1f, 0.0f));
            beam.sprite = LoadingTheme.Sprite(LoadingTheme.Glow, "LoadingGlow", Vector4.zero);
            beam.type = Image.Type.Simple;
            beam.rectTransform.sizeDelta = new Vector2(560f, 560f);
            beam.rectTransform.anchoredPosition = new Vector2(-700f, 80f);

            captionText = MakeText(canvasGo.transform, "Caption",
                StoryStrings.Tr(CaptionKey), 30,
                new Color(0.78f, 0.83f, 0.92f, 0.85f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(1100f, 60f));

            // Progress bar (bottom centre, rounded track with an amber fill).
            var track = MakeRect(canvasGo.transform, "ProgressTrack",
                new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.28f),
                new Color(1f, 1f, 1f, 0.9f));
            track.sprite = LoadingTheme.Sprite(LoadingTheme.Panel, "LoadingPanel", new Vector4(14f, 14f, 14f, 14f));
            track.type = Image.Type.Sliced;
            track.rectTransform.sizeDelta = new Vector2(920f, 22f);

            tipText = MakeText(canvasGo.transform, "Tip",
                StoryStrings.Tr(TipKey), 19,
                new Color(0.62f, 0.60f, 0.55f, 0.85f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.34f), new Vector2(0.5f, 0.34f), new Vector2(1100f, 46f));

            progressFill = MakeRect(track.transform, "Fill", Vector2.zero, Vector2.one,
                new Color(0.65f, 0.75f, 0.95f, 0.95f));
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            progressFill.fillAmount = 0f;
            // Inset the fill a few pixels inside the track.
            progressFill.rectTransform.offsetMin = new Vector2(3f, 3f);
            progressFill.rectTransform.offsetMax = new Vector2(-3f, -3f);

            progressText = MakeText(canvasGo.transform, "ProgressLabel",
                "", 20, new Color(0.68f, 0.65f, 0.60f, 0.9f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.24f), new Vector2(0.5f, 0.24f), new Vector2(600f, 40f));
        }

        // The school at dusk, drawn from rectangles: three wing blocks with
        // pitched beams, a gate pair, a fence line, and a scatter of warm
        // windows. Cheap, resolution-independent, no asset dependency.
        void BuildSilhouette(Transform parent)
        {
            Color silhouette = new Color(0.02f, 0.02f, 0.035f, 1f);
            Color windowOn = new Color(1f, 0.80f, 0.45f, 0.85f);
            float baseY = 0.52f; // horizon line in anchor space

            // Ground line.
            var ground = MakeRect(parent, "Ground", new Vector2(0f, baseY - 0.02f), new Vector2(1f, baseY), silhouette);
            ground.color = silhouette;

            // Three wing blocks (library / science / hall) on the skyline.
            float[][] blocks =
            {
                new[] { 0.26f, 0.13f, 0.16f },  // centre-x, width, height
                new[] { 0.44f, 0.10f, 0.12f },
                new[] { 0.63f, 0.16f, 0.18f },
            };
            for (int i = 0; i < blocks.Length; i++)
            {
                float cx = blocks[i][0];
                float w = blocks[i][1];
                float h = blocks[i][2];
                var block = MakeRect(parent, "Wing" + i,
                    new Vector2(cx - w / 2f, baseY), new Vector2(cx + w / 2f, baseY + h), silhouette);

                // Pitched roof: a rotated rectangle across the block top.
                var roof = MakeRect(parent, "Roof" + i,
                    new Vector2(cx - w / 2f, baseY + h - 0.015f), new Vector2(cx + w / 2f, baseY + h + 0.025f), silhouette);
                roof.color = silhouette;

                // Lit windows: two warm dots per block (the fuse boxes are on).
                for (int wIndex = 0; wIndex < 2; wIndex++)
                {
                    var window = MakeRect(parent, "Window" + i + "_" + wIndex,
                        new Vector2(cx - 0.012f + wIndex * 0.024f, baseY + h * 0.45f),
                        new Vector2(cx + 0.004f + wIndex * 0.024f, baseY + h * 0.45f + 0.018f),
                        windowOn);
                    window.color = windowOn;
                }
            }

            // The main gate: two posts slightly left of centre.
            var gateLeft = MakeRect(parent, "GatePostL", new Vector2(0.335f, baseY), new Vector2(0.341f, baseY + 0.09f), silhouette);
            var gateRight = MakeRect(parent, "GatePostR", new Vector2(0.355f, baseY), new Vector2(0.361f, baseY + 0.09f), silhouette);
            gateLeft.color = silhouette;
            gateRight.color = silhouette;

            // Fence posts marching out to the edges.
            for (int i = 0; i < 14; i++)
            {
                float x = 0.06f + i * 0.068f;
                var post = MakeRect(parent, "Fence" + i,
                    new Vector2(x, baseY), new Vector2(x + 0.004f, baseY + 0.035f), silhouette);
                post.color = silhouette;
            }
        }

        static Image MakeRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static Text MakeText(Transform parent, string name, string content, int fontSize, Color color,
            TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
            Text text = go.AddComponent<Text>();
            text.font = Localization.CurrentFont;
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }

        void RunTransition(string sceneName)
        {
            if (inTransition)
            {
                // Latest-wins (audit v3 5.4): queue the request instead of
                // dropping it; a newer request replaces any older queued one.
                pendingScene = sceneName;
                return;
            }

            StopAllCoroutines(); // cancel any Show/Hide fade before loading
            inTransition = true;
            StartCoroutine(TransitionRoutineGuarded(sceneName));
        }

        static bool inTransition;

        // LATEST-WINS concurrency (audit v3 5.4): a Transition() requested
        // while another transition is in flight is QUEUED here, not dropped.
        // A newer request replaces any older queued one, so the LAST scene
        // requested always wins; the queued scene loads as soon as the
        // current transition finishes.
        string pendingScene;

        // Source-string keys, kept so RefreshLanguage can re-translate the
        // persistent canvas in place (audit v3 5.4).
        const string CaptionKey = "DEMOLITION AT DAWN";
        const string TipKey = "Knowledge is the only light that follows a person into the dark.";

        // True only while the static "Loading..." label is on screen; the
        // live percent readout is language-independent.
        bool progressLabelShowing;

        IEnumerator TransitionRoutineGuarded(string sceneName)
        {
            // try/finally (legal with yield): an exception mid-transition
            // must not leave the guard closed forever.
            try
            {
                string next = sceneName;
                while (next != null)
                {
                    pendingScene = null;
                    yield return TransitionRoutine(next);
                    next = pendingScene; // latest-wins handoff (audit v3 5.4)
                }
            }
            finally
            {
                pendingScene = null;
                inTransition = false;
            }
        }

        IEnumerator TransitionRoutine(string sceneName)
        {
            canvas.gameObject.SetActive(true);
            yield return FadeTo(1f, FadeSeconds);
            progressFill.fillAmount = 0f;
            progressLabelShowing = true;
            progressText.text = StoryStrings.Tr("Loading...");

            float shownAt = Time.unscaledTime;
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation != null)
            {
                // NO activation gate: with allowSceneActivation=false Unity
                // throttles the background load, and with competing background
                // loads (our streaming audio) it stalled indefinitely at ~7%
                // (player report: 'stuck on loading at 7%'). Activation stays
                // allowed; the bar shows real progress to 100%.
                float lastProgress = 0f;
                float lastMoveAt = Time.unscaledTime;
                while (!operation.isDone)
                {
                    progressFill.fillAmount = Mathf.Clamp01(operation.progress);
                    progressLabelShowing = false;
                    progressText.text = Mathf.RoundToInt(progressFill.fillAmount * 100f) + "%";
                    // Belt and braces: if the loader ever stalls outright,
                    // force activation rather than hanging the night.
                    if (operation.allowSceneActivation &&
                        operation.progress - lastProgress > 0.001f)
                    {
                        lastProgress = operation.progress;
                        lastMoveAt = Time.unscaledTime;
                    }
                    else if (Time.unscaledTime - lastMoveAt > StallTimeoutSeconds)
                    {
                        operation.allowSceneActivation = true;
                    }
                    yield return null;
                }

                // Hold the silhouette a moment so the transition breathes.
                while (Time.unscaledTime - shownAt < MinimumShowSeconds) yield return null;
            }

            yield return FadeTo(0f, FadeSeconds);
            canvas.gameObject.SetActive(false);
        }

        IEnumerator FadeTo(float target, float seconds, System.Action onComplete = null)
        {
            canvas.gameObject.SetActive(true);
            float start = group.alpha;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / Mathf.Max(0.01f, seconds);
                group.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(t));
                yield return null;
            }
            group.alpha = target;
            onComplete?.Invoke();
        }
    }
}
