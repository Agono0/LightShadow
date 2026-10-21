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
            PlayerPrefs.Save();
        }

        public static void LoadAndApply()
        {
            EnsureLoaded();
            ApplyQuality();
            ApplyResolution();
            ApplyVSync();
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

        public static void ResetGraphics()
        {
            EnsureLoaded();
            ResolutionIndex = DefaultResolutionIndex();
            Fullscreen = false;
            QualityLevel = Mathf.Max(0, QualitySettings.names.Length - 1);
            VSync = true;
            Brightness = 0.5f;
            Sensitivity = 1f;
            ApplyQuality();
            ApplyResolution();
            ApplyVSync();
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
    }
}
