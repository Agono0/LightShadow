using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightShadow.Core
{
    // One persistent place for every option the player can change. Values are
    // stored in PlayerPrefs so they survive a restart, and Changed is raised after
    // each edit so live UI (brightness, mouse sensitivity) can react immediately.
    public static class GameSettings
    {
        public const string LanguageKey = "LS_Language";

        public static event Action Changed;

        static bool loaded;
        static readonly List<Resolution> resolutions = new List<Resolution>();

        public static int ResolutionIndex { get; private set; }
        public static bool Fullscreen { get; private set; }
        public static int QualityLevel { get; private set; }
        public static bool VSync { get; private set; }
        public static float Brightness { get; private set; } = 0.5f;
        public static float Sensitivity { get; private set; } = 1f;
        public static bool NarratorEnabled { get; private set; } = true;

        // Fine graphics controls on top of the quality level, applied to the
        // active URP asset at runtime: shadows, anti-aliasing, render scale.
        static readonly float[] RenderScales = { 0.5f, 0.75f, 1f, 1.25f, 1.5f };
        static readonly int[] MsaaSamples = { 0, 2, 4, 8 };
        static readonly float[] ShadowDistances = { 0f, 40f, 80f };

        public static int ShadowsIndex { get; private set; } = 2;      // 0 off · 1 medium · 2 high
        public static int MsaaIndex { get; private set; } = 2;         // index into MsaaSamples (4x)
        public static int RenderScaleIndex { get; private set; } = 2;  // index into RenderScales (native)

        public static float RenderScale => RenderScales[Mathf.Clamp(RenderScaleIndex, 0, RenderScales.Length - 1)];

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
            QualityLevel = Mathf.Clamp(PlayerPrefs.GetInt("LS_Quality", QualitySettings.GetQualityLevel()), 0,
                Mathf.Max(0, QualitySettings.names.Length - 1));
            VSync = PlayerPrefs.GetInt("LS_VSync", QualitySettings.vSyncCount > 0 ? 1 : 0) == 1;
            Brightness = Mathf.Clamp01(PlayerPrefs.GetFloat("LS_Brightness", 0.5f));
            Sensitivity = Mathf.Clamp(PlayerPrefs.GetFloat("LS_Sensitivity", 1f), 0.4f, 2.5f);
            NarratorEnabled = PlayerPrefs.GetInt("LS_Narrator", 1) == 1;
            ShadowsIndex = Mathf.Clamp(PlayerPrefs.GetInt("LS_Shadows", 2), 0, ShadowDistances.Length - 1);
            MsaaIndex = Mathf.Clamp(PlayerPrefs.GetInt("LS_MSAA", 2), 0, MsaaSamples.Length - 1);
            RenderScaleIndex = Mathf.Clamp(PlayerPrefs.GetInt("LS_RenderScale", 2), 0, RenderScales.Length - 1);
        }

        static void BuildResolutionList()
        {
            resolutions.Clear();
            Resolution[] available = Screen.resolutions;
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
            PlayerPrefs.SetInt("LS_Quality", QualityLevel);
            PlayerPrefs.SetInt("LS_VSync", VSync ? 1 : 0);
            PlayerPrefs.SetFloat("LS_Brightness", Brightness);
            PlayerPrefs.SetFloat("LS_Sensitivity", Sensitivity);
            PlayerPrefs.SetInt("LS_Narrator", NarratorEnabled ? 1 : 0);
            PlayerPrefs.SetInt("LS_Shadows", ShadowsIndex);
            PlayerPrefs.SetInt("LS_MSAA", MsaaIndex);
            PlayerPrefs.SetInt("LS_RenderScale", RenderScaleIndex);
            PlayerPrefs.Save();
        }

        public static void LoadAndApply()
        {
            EnsureLoaded();
            ApplyQuality();
            ApplyResolution();
            ApplyVSync();
            ApplyGraphics();
            Changed?.Invoke();
        }

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

        public static void SetQualityLevel(int level)
        {
            EnsureLoaded();
            if (QualitySettings.names.Length == 0) return;
            QualityLevel = (level % QualitySettings.names.Length + QualitySettings.names.Length) % QualitySettings.names.Length;
            ApplyQuality();
            ApplyVSync();
            Save();
            Changed?.Invoke();
        }

        public static void ToggleVSync()
        {
            EnsureLoaded();
            VSync = !VSync;
            ApplyVSync();
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

        public static void SetShadows(int index)
        {
            EnsureLoaded();
            ShadowsIndex = (index % ShadowDistances.Length + ShadowDistances.Length) % ShadowDistances.Length;
            ApplyGraphics();
            Save();
            Changed?.Invoke();
        }

        public static void SetMsaa(int index)
        {
            EnsureLoaded();
            MsaaIndex = (index % MsaaSamples.Length + MsaaSamples.Length) % MsaaSamples.Length;
            ApplyGraphics();
            Save();
            Changed?.Invoke();
        }

        public static void SetRenderScale(int index)
        {
            EnsureLoaded();
            RenderScaleIndex = (index % RenderScales.Length + RenderScales.Length) % RenderScales.Length;
            ApplyGraphics();
            Save();
            Changed?.Invoke();
        }

        public static void ResetGraphics()
        {
            EnsureLoaded();
            ResolutionIndex = DefaultResolutionIndex();
            Fullscreen = false;
            QualityLevel = Mathf.Max(0, QualitySettings.names.Length - 1);
            VSync = true;
            Brightness = 0.5f;
            Sensitivity = 1f;
            ShadowsIndex = ShadowDistances.Length - 1;
            MsaaIndex = 2;
            RenderScaleIndex = 2;
            ApplyQuality();
            ApplyResolution();
            ApplyVSync();
            ApplyGraphics();
            Save();
            Changed?.Invoke();
        }

        static void ApplyResolution()
        {
            if (resolutions.Count == 0) return;
            Resolution resolution = resolutions[Mathf.Clamp(ResolutionIndex, 0, resolutions.Count - 1)];
            if (resolution.width <= 0 || resolution.height <= 0) return;
            Screen.SetResolution(resolution.width, resolution.height,
                Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }

        static void ApplyQuality()
        {
            if (QualitySettings.names.Length == 0) return;
            QualitySettings.SetQualityLevel(Mathf.Clamp(QualityLevel, 0, QualitySettings.names.Length - 1), true);
            // The active URP asset changes with the quality level, so the fine
            // graphics controls are re-asserted on whatever asset is live now.
            ApplyGraphics();
        }

        // Pushes the fine graphics controls to the active URP render pipeline
        // asset. Values are session-wide (they act on the shared asset) and are
        // re-applied whenever a quality switch or a settings change happens.
        static void ApplyGraphics()
        {
            if (Graphics == null) return;
            Graphics.shadowDistance = ShadowDistances[Mathf.Clamp(ShadowsIndex, 0, ShadowDistances.Length - 1)];
            Graphics.msaaSampleCount = MsaaSamples[Mathf.Clamp(MsaaIndex, 0, MsaaSamples.Length - 1)];
            Graphics.renderScale = RenderScales[Mathf.Clamp(RenderScaleIndex, 0, RenderScales.Length - 1)];
        }

        static UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset Graphics
        {
            get
            {
                return UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline
                    as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
            }
        }

        static void ApplyVSync()
        {
            QualitySettings.vSyncCount = VSync ? 1 : 0;
        }

        public static string ResolutionLabel()
        {
            Resolution resolution = CurrentResolution;
            return resolution.width + " × " + resolution.height;
        }

        public static string SensitivityLabel()
        {
            return Mathf.RoundToInt(Sensitivity * 100f) + "%";
        }

        public static string BrightnessLabel()
        {
            return Mathf.RoundToInt(Brightness * 100f) + "%";
        }

        public static string ShadowsLabel()
        {
            switch (ShadowsIndex)
            {
                case 0: return "Off";
                case 1: return "Medium";
                default: return "High";
            }
        }

        public static string MsaaLabel()
        {
            int count = MsaaSamples[Mathf.Clamp(MsaaIndex, 0, MsaaSamples.Length - 1)];
            return count <= 0 ? "Off" : count + "x";
        }

        public static string RenderScaleLabel()
        {
            float scale = RenderScales[Mathf.Clamp(RenderScaleIndex, 0, RenderScales.Length - 1)];
            if (Mathf.Approximately(scale, 1f)) return "Native";
            return Mathf.RoundToInt(scale * 100f) + "%";
        }
    }
}
