using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightShadow.Core
{
    // One persistent place for every option the player can change, and the ONE
    // writer of graphics state (refactored after the premium pass: previously
    // GameSettings and PerformanceManager both wrote the live URP asset, so the
    // settings card could show requested values that the tier system had
    // overwritten — the "fighting" bug).
    //
    // Ownership now:
    //   PerformanceManager  → tier AUTO-DETECTION, frame-rate policy (120 FPS
    //                         targeting / Android refresh clamp), dynamic
    //                         resolution, boot summary log. It no longer writes
    //                         URP asset values.
    //   GameSettings        → every user-facing graphics value. The quality
    //                         TIER (Auto/Low/Medium/High/Ultra) sets the base
    //                         render scale / MSAA / shadow distance; the Shadows,
    //                         Anti-aliasing and Render-scale rows are explicit
    //                         overrides (-1 = follow the tier). After every
    //                         change the card reads back the LIVE pipeline
    //                         asset / Screen state, so displayed values are
    //                         always what is actually applied.
    //
    // Values are stored in PlayerPrefs so they survive a restart, and Changed
    // is raised after each edit so live UI (brightness, sensitivity, FPS
    // counter) can react immediately.
    public static class GameSettings
    {
        public const string LanguageKey = "LS_Language";
        public const string ShowFpsKey = "LS_ShowFps";
        public const string DebugModeKey = "LS_DebugMode";

        public static event Action Changed;

        static bool loaded;
        static readonly List<Resolution> resolutions = new List<Resolution>();

        public static int ResolutionIndex { get; private set; }
        public static bool Fullscreen { get; private set; }
        public static bool VSync { get; private set; }
        public static float Brightness { get; private set; } = 0.5f;
        public static float Sensitivity { get; private set; } = 1f;
        public static bool NarratorEnabled { get; private set; } = true;
        public static bool ShowFps { get; private set; } = true;

        // P3 dev console gate: the console (and its DEBUG button) only builds
        // when this is on. Off by default; dev-only, set via SetDebugMode or
        // the raw pref key.
        public static bool DebugMode { get; private set; }

        // Quality tier: -1 = auto-detect, 0..3 = Low/Medium/High/Ultra. Stored
        // under the same "ls_qualityTier" key the tier row and
        // PerformanceManager already used, so player choices survive the
        // refactor. AppliedTier is what the detector resolved (0..3).
        public const int TierAuto = -1;
        public static int TierIndex { get; private set; } = TierAuto;
        public static int AppliedTier { get; private set; }

        // Fine graphics overrides. -1 means "follow the tier"; otherwise an
        // index into the option tables below.
        public static int ShadowsIndex { get; private set; } = -1;     // 0 off · 1 medium · 2 high · -1 tier
        public static int MsaaIndex { get; private set; } = -1;        // index into MsaaSamples · -1 tier
        public static int RenderScaleIndex { get; private set; } = -1; // index into RenderScales · -1 tier

        // Touch control size (mobile): 0 small · 1 normal · 2 large · 3 huge.
        public static int ControlSizeIndex { get; private set; } = 1;
        static readonly float[] ControlScales = { 0.85f, 1f, 1.2f, 1.4f };
        public static float ControlScale
        {
            get
            {
                EnsureLoaded();
                return ControlScales[Mathf.Clamp(ControlSizeIndex, 0, ControlScales.Length - 1)];
            }
        }

        static readonly float[] RenderScales = { 0.5f, 0.65f, 0.75f, 0.9f, 1f, 1.25f };
        static readonly int[] MsaaSamples = { 0, 2, 4 };
        static readonly float[] ShadowDistances = { 0f, 20f, 40f, 60f };

        // Tier base values (match PerformanceManager's published tier table).
        //        shadows  msaa  renderScale
        // Low        0      0       0.65
        // Medium    20      0       0.75
        // High      40      2       0.90
        // Ultra     60      2       1.00
        static readonly float[] TierShadows = { 0f, 20f, 40f, 60f };
        static readonly int[] TierMsaa = { 0, 0, 2, 2 };
        static readonly float[] TierRenderScale = { 0.65f, 0.75f, 0.9f, 1f };
        static readonly string[] TierNames = { "Low", "Medium", "High", "Ultra" };

        public static IReadOnlyList<Resolution> Resolutions
        {
            get
            {
                EnsureLoaded();
                return resolutions;
            }
        }

        public static Resolution CurrentResolution
        {
            get
            {
                EnsureLoaded();
                if (resolutions.Count == 0) return Screen.currentResolution;
                return resolutions[Mathf.Clamp(ResolutionIndex, 0, resolutions.Count - 1)];
            }
        }

        static void EnsureLoaded()
        {
            if (loaded) return;
            loaded = true;

            BuildResolutionList();
            ResolutionIndex = Mathf.Clamp(PlayerPrefs.GetInt("LS_Resolution", DefaultResolutionIndex()), 0,
                Mathf.Max(0, resolutions.Count - 1));
            Fullscreen = PlayerPrefs.GetInt("LS_Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
            VSync = PlayerPrefs.GetInt("LS_VSync", 0) == 1; // 120 FPS targeting wants vSync off by default
            Brightness = Mathf.Clamp01(PlayerPrefs.GetFloat("LS_Brightness", 0.5f));
            Sensitivity = Mathf.Clamp(PlayerPrefs.GetFloat("LS_Sensitivity", 1f), 0.4f, 2.5f);
            NarratorEnabled = PlayerPrefs.GetInt("LS_Narrator", 1) == 1;
            ShowFps = PlayerPrefs.GetInt(ShowFpsKey, 1) == 1;
            DebugMode = PlayerPrefs.GetInt(DebugModeKey, 0) == 1;
            TierIndex = PlayerPrefs.GetInt("ls_qualityTier", TierAuto);
            // Missing fine-control prefs (fresh install) follow the tier; the
            // legacy literal defaults (2/2/2) are not assumed any more.
            ShadowsIndex = PlayerPrefs.GetInt("LS_Shadows", -1);
            ControlSizeIndex = PlayerPrefs.GetInt("LS_ControlSize", 1);
            MsaaIndex = PlayerPrefs.GetInt("LS_MSAA", -1);
            RenderScaleIndex = PlayerPrefs.GetInt("LS_RenderScale", -1);
        }

        static void BuildResolutionList()
        {
            resolutions.Clear();
            // -nographics batch sessions report no screen: guard the null so
            // editor tooling that touches GameSettings still works headlessly.
            Resolution[] available = Screen.resolutions ?? Array.Empty<Resolution>();
            var seen = new HashSet<long>();

            for (int i = 0; i < available.Length; i++)
            {
                long key = ((long)available[i].width << 32) | (uint)available[i].height;
                if (!seen.Add(key)) continue;
                resolutions.Add(available[i]);
            }

            if (resolutions.Count == 0)
            {
                resolutions.Add(new Resolution { width = Screen.width, height = Screen.height });
            }

            resolutions.Sort((a, b) =>
            {
                int byWidth = a.width.CompareTo(b.width);
                return byWidth != 0 ? byWidth : a.height.CompareTo(b.height);
            });
        }

        static int DefaultResolutionIndex()
        {
            for (int i = 0; i < resolutions.Count; i++)
            {
                if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height) return i;
            }
            return Mathf.Max(0, resolutions.Count - 1);
        }

        static void Save()
        {
            PlayerPrefs.SetInt("LS_Resolution", ResolutionIndex);
            PlayerPrefs.SetInt("LS_Fullscreen", Fullscreen ? 1 : 0);
            PlayerPrefs.SetInt("LS_VSync", VSync ? 1 : 0);
            PlayerPrefs.SetFloat("LS_Brightness", Brightness);
            PlayerPrefs.SetFloat("LS_Sensitivity", Sensitivity);
            PlayerPrefs.SetInt("LS_Narrator", NarratorEnabled ? 1 : 0);
            PlayerPrefs.SetInt(ShowFpsKey, ShowFps ? 1 : 0);
            PlayerPrefs.SetInt(DebugModeKey, DebugMode ? 1 : 0);
            PlayerPrefs.SetInt("ls_qualityTier", TierIndex);
            PlayerPrefs.SetInt("LS_Shadows", ShadowsIndex);
            PlayerPrefs.SetInt("LS_ControlSize", ControlSizeIndex);
            PlayerPrefs.SetInt("LS_MSAA", MsaaIndex);
            PlayerPrefs.SetInt("LS_RenderScale", RenderScaleIndex);
            PlayerPrefs.Save();
        }

        // ------------------------------------------------------------- apply

        public static void LoadAndApply()
        {
            EnsureLoaded();
            ApplyAll();
            Changed?.Invoke();
        }

        /// <summary>One deterministic application path: tier → quality level →
        /// fine graphics → vSync/frame policy → resolution. Everything the
        /// settings card can change funnels through here.</summary>
        static void ApplyAll()
        {
            ResolveTier();
            ApplyQualityLevel();
            ApplyGraphics();
            ApplyFramePolicy();
            ApplyResolution();
        }

        static void ResolveTier()
        {
            if (TierIndex < 0 || TierIndex > 3)
            {
                AppliedTier = (int)Systems.PerformanceManager.AutoDetectTier();
            }
            else
            {
                AppliedTier = TierIndex;
            }
        }

        // The Mobile/PC quality levels differ by renderer (SSAO) — the tier
        // picks the level: Low/Medium run the Mobile renderer, High/Ultra PC.
        // Only switch when the level actually differs: SetQualityLevel with
        // applyAll:true forces a render-pipeline recreation even for the same
        // level, and a recreation after the first frame re-runs URP's blitter
        // init, which throws "Blitter is already initialized" once and then
        // every frame thereafter (pipeline never caches) - the low-FPS bug.
        static void ApplyQualityLevel()
        {
            if (QualitySettings.names.Length == 0) return;
            int level = AppliedTier <= 1
                ? IndexOfQualityLevel("Mobile")
                : IndexOfQualityLevel("PC");
            if (level >= 0 && level != QualitySettings.GetQualityLevel())
            {
                QualitySettings.SetQualityLevel(level, true);
            }
        }

        static int IndexOfQualityLevel(string name)
        {
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                if (string.Equals(QualitySettings.names[i], name, StringComparison.OrdinalIgnoreCase)) return i;
            }
            return -1;
        }

        // Pushes the effective fine graphics values to the active URP asset.
        // An override (-1 aside) wins; otherwise the tier base applies. These
        // are the values the settings card reads back, so the menu can never
        // drift from reality.
        static void ApplyGraphics()
        {
            var graphics = Graphics;
            if (graphics == null) return;

            float shadows = ShadowsIndex >= 0
                ? ShadowDistances[Mathf.Clamp(ShadowsIndex, 0, ShadowDistances.Length - 1)]
                : TierShadows[AppliedTier];
            int msaa = MsaaIndex >= 0
                ? MsaaSamples[Mathf.Clamp(MsaaIndex, 0, MsaaSamples.Length - 1)]
                : TierMsaa[AppliedTier];
            float scale = RenderScaleIndex >= 0
                ? RenderScales[Mathf.Clamp(RenderScaleIndex, 0, RenderScales.Length - 1)]
                : TierRenderScale[AppliedTier];

#if UNITY_EDITOR
            RememberAssetForEditor(graphics);
#endif
            graphics.shadowDistance = shadows;
            graphics.msaaSampleCount = msaa;
            graphics.renderScale = scale;
        }

#if UNITY_EDITOR
        // Audit 6.5: in the editor the URP asset is a project file, so writing it
        // leaves the asset dirty on disk. Remember the authored values and put
        // them back when Play mode ends.
        static UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset rememberedAsset;
        static float rememberedShadows;
        static int rememberedMsaa;
        static float rememberedScale;

        static void RememberAssetForEditor(UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset asset)
        {
            if (rememberedAsset != null) return;
            rememberedAsset = asset;
            rememberedShadows = asset.shadowDistance;
            rememberedMsaa = asset.msaaSampleCount;
            rememberedScale = asset.renderScale;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        static void OnPlayModeChanged(UnityEditor.PlayModeStateChange change)
        {
            if (change != UnityEditor.PlayModeStateChange.EnteredEditMode) return;
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            if (rememberedAsset != null)
            {
                rememberedAsset.shadowDistance = rememberedShadows;
                rememberedAsset.msaaSampleCount = rememberedMsaa;
                rememberedAsset.renderScale = rememberedScale;
            }
            rememberedAsset = null;
        }
#endif

        // Frame-rate policy (decision D8): vSync on → the driver caps us and
        // the engine target is cleared; vSync off → Windows targets 120 FPS and
        // Android requests the display's refresh rate (clamped 60-120).
        static void ApplyFramePolicy()
        {
            QualitySettings.vSyncCount = VSync ? 1 : 0;
            if (VSync)
            {
                Application.targetFrameRate = -1;
            }
            else
            {
                Systems.PerformanceManager.ApplyFrameRatePolicy();
            }
            Systems.PerformanceManager.ApplyDynamicResolution();
        }

        static void ApplyResolution()
        {
            // Mobile renders at the panel's native resolution only. Forcing a
            // picked resolution makes Android scale the framebuffer to fill a
            // display of a different aspect ratio, which is the stretched look
            // reported on phones; the resolution/fullscreen rows are a
            // desktop-only feature.
            if (Application.isMobilePlatform) return;
            if (resolutions.Count == 0) return;
            Resolution resolution = resolutions[Mathf.Clamp(ResolutionIndex, 0, resolutions.Count - 1)];
            if (resolution.width <= 0 || resolution.height <= 0) return;
            Screen.SetResolution(resolution.width, resolution.height,
                Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }

        // ------------------------------------------------------------- setters

        public static void SetResolutionIndex(int index)
        {
            EnsureLoaded();
            if (resolutions.Count == 0) return;
            ResolutionIndex = (index % resolutions.Count + resolutions.Count) % resolutions.Count;
            ApplyResolution();
            Save();
            Changed?.Invoke();
        }

        public static void ToggleFullscreen()
        {
            EnsureLoaded();
            Fullscreen = !Fullscreen;
            ApplyResolution();
            Save();
            Changed?.Invoke();
        }

        public static void SetTier(int tier)
        {
            EnsureLoaded();
            // Cycle Auto → Low → Medium → High → Ultra → Auto.
            TierIndex = tier < -1 ? 3 : (tier > 3 ? -1 : tier);
            ApplyAll();
            Save();
            Changed?.Invoke();
        }

        public static void ToggleVSync()
        {
            EnsureLoaded();
            VSync = !VSync;
            ApplyFramePolicy();
            Save();
            Changed?.Invoke();
        }

        public static void SetBrightness(float value)
        {
            EnsureLoaded();
            Brightness = Mathf.Clamp01(value);
            Save();
            Changed?.Invoke();
        }

        public static void SetSensitivity(float multiplier)
        {
            EnsureLoaded();
            Sensitivity = Mathf.Clamp(multiplier, 0.4f, 2.5f);
            Save();
            Changed?.Invoke();
        }

        public static void SetLanguage(GameLanguage language)
        {
            EnsureLoaded();
            Localization.SetLanguage(language);
            Save();
            Changed?.Invoke();
        }

        public static void ToggleLanguage()
        {
            EnsureLoaded();
            SetLanguage(Localization.Current == GameLanguage.Arabic ? GameLanguage.English : GameLanguage.Arabic);
        }

        public static void ToggleNarrator()
        {
            EnsureLoaded();
            NarratorEnabled = !NarratorEnabled;
            Save();
            Changed?.Invoke();
        }

        public static void ToggleFps()
        {
            EnsureLoaded();
            ShowFps = !ShowFps;
            Save();
            Changed?.Invoke();
        }

        // P3 dev console gate setter (off by default; nothing in the shipped
        // UI touches this).
        public static void SetDebugMode(bool enabled)
        {
            EnsureLoaded();
            DebugMode = enabled;
            Save();
            Changed?.Invoke();
        }

        public static void SetShadows(int index)
        {
            EnsureLoaded();
            // -1 = follow the tier; anything out of range snaps back to it too.
            ShadowsIndex = index < 0 || index >= ShadowDistances.Length ? -1 : index;
            ApplyGraphics();
            Save();
            Changed?.Invoke();
        }

        public static void SetMsaa(int index)
        {
            EnsureLoaded();
            MsaaIndex = index < 0 || index >= MsaaSamples.Length ? -1 : index;
            ApplyGraphics();
            Save();
            Changed?.Invoke();
        }

        public static void SetRenderScale(int index)
        {
            EnsureLoaded();
            RenderScaleIndex = index < 0 || index >= RenderScales.Length ? -1 : index;
            ApplyGraphics();
            Save();
            Changed?.Invoke();
        }

        public static void SetControlSize(int index)
        {
            EnsureLoaded();
            ControlSizeIndex = (index % ControlScales.Length + ControlScales.Length) % ControlScales.Length;
            Save();
            Changed?.Invoke();
        }

        public static string ControlSizeLabel()
        {
            return Mathf.RoundToInt(ControlScale * 100f) + "%";
        }

        public static void ResetGraphics()
        {
            EnsureLoaded();
            ResolutionIndex = DefaultResolutionIndex();
            Fullscreen = false;
            VSync = false;
            Brightness = 0.5f;
            Sensitivity = 1f;
            TierIndex = TierAuto;
            ShadowsIndex = -1;
            MsaaIndex = -1;
            RenderScaleIndex = -1;
            ApplyAll();
            Save();
            Changed?.Invoke();
        }

        // -------------------------------------------------------------- labels
        // Every label reads the LIVE state (Screen / QualitySettings / the
        // active URP asset), so the card always shows what is really applied.

        static UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset Graphics
        {
            get
            {
                return UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline
                    as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            }
        }

        public static string ResolutionLabel()
        {
            Resolution resolution = Screen.currentResolution;
            if (resolution.width <= 0) resolution = CurrentResolution;
            int hz = Mathf.RoundToInt((float)resolution.refreshRateRatio.value);
            return hz > 0
                ? resolution.width + " × " + resolution.height + " @ " + hz
                : resolution.width + " × " + resolution.height;
        }

        public static string FullscreenLabel()
        {
            return Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen ||
                   Screen.fullScreenMode == FullScreenMode.FullScreenWindow
                ? "Fullscreen"
                : "Windowed";
        }

        public static string TierLabel()
        {
            string name = TierNames[Mathf.Clamp(AppliedTier, 0, TierNames.Length - 1)];
            return TierIndex < 0 ? name + " (auto)" : name;
        }

        public static string ShadowsLabel()
        {
            var graphics = Graphics;
            float distance = graphics != null ? graphics.shadowDistance : TierShadows[AppliedTier];
            if (distance <= 0.1f) return "Off";
            return Mathf.RoundToInt(distance) + " m";
        }

        public static string MsaaLabel()
        {
            var graphics = Graphics;
            int count = graphics != null ? graphics.msaaSampleCount : TierMsaa[AppliedTier];
            return count <= 1 ? "Off" : count + "x";
        }

        public static string RenderScaleLabel()
        {
            var graphics = Graphics;
            float scale = graphics != null ? graphics.renderScale : TierRenderScale[AppliedTier];
            if (Mathf.Approximately(scale, 1f)) return "Native";
            return Mathf.RoundToInt(scale * 100f) + "%";
        }

        public static string VSyncLabel()
        {
            return QualitySettings.vSyncCount > 0 ? "On" : "Off";
        }

        public static string SensitivityLabel()
        {
            return Mathf.RoundToInt(Sensitivity * 100f) + "%";
        }

        public static string BrightnessLabel()
        {
            return Mathf.RoundToInt(Brightness * 100f) + "%";
        }
    }
}
