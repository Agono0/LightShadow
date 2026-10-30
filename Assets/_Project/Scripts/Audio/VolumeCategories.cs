using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightShadow.Audio
{
    // Code-level category mixer (decision D15, documented honestly in the brief
    // and in Docs/AGENT_REPORT_NARRATIVE.md): a real AudioMixer asset cannot be
    // created reliably headlessly, and the game's audio is synthesised at
    // runtime with no external files - so the mixer lives in code with the same
    // settings-card UX (five sliders Master/Music/Ambience/SFX/UI, 0-10,
    // PlayerPrefs-backed).
    //
    // Sources register two ways:
    //   1. explicitly  - VolumeCategories.Register(source, Category.Music)
    //   2. by name     - every scene load scans live AudioSources and maps them
    //      by naming convention: "MUS_"/"Music*" -> Music, "AMB_"/"Ambien*" ->
    //      Ambience, "SFX_"/"Sfx*"/"SFX*" -> Sfx, "UI*" -> Ui, unmatched -> Sfx.
    //
    // Volume math per source: source.volume = baseVolume * category * master,
    // re-applied when a slider moves or a scene loads. Zero per-frame cost.
    public static class VolumeCategories
    {
        public enum Category { Master = 0, Music = 1, Ambience = 2, Sfx = 3, Ui = 4 }

        public const int LevelCount = 11; // 0..10 slider steps

        static readonly string[] PrefKeys =
        {
            "ls_vol_master", "ls_vol_music", "ls_vol_ambience", "ls_vol_sfx", "ls_vol_ui",
        };

        const float DefaultLevel = 8f;

        // One registered source: the base volume the owner set, scaled live.
        struct Registered
        {
            public AudioSource source;
            public Category category;
            public float baseVolume;
            public bool overridesBase; // true = baseVolume is explicit, not captured
        }

        static readonly List<Registered> registered = new List<Registered>(16);
        static readonly float[] levels = { DefaultLevel, DefaultLevel, DefaultLevel, DefaultLevel, DefaultLevel };
        static bool bootstrapped;

        public static event Action CategoryChanged;

        // --------------------------------------------------------- bootstrap --

        // Wires the scene-load auto-registration once per process. Called from
        // [RuntimeInitializeOnLoadMethod], so no scene object is needed.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (bootstrapped) return;
            bootstrapped = true;

            for (int i = 0; i < levels.Length; i++)
            {
                levels[i] = PlayerPrefs.GetInt(PrefKeys[i], (int)DefaultLevel);
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            ScanActiveScene();
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ScanActiveScene();

        // ---------------------------------------------------------- settings --

        public static float GetLevel(Category category)
        {
            return levels[(int)category];
        }

        public static void SetLevel(Category category, int level)
        {
            int index = (int)category;
            if (index < 0 || index >= levels.Length) return;
            level = Mathf.Clamp(level, 0, LevelCount - 1);
            if (Mathf.Approximately(levels[index], level)) return;
            levels[index] = level;
            PlayerPrefs.SetInt(PrefKeys[index], level);
            PlayerPrefs.Save();
            ApplyAll();
            CategoryChanged?.Invoke();
        }

        // Category volume 0..1 as heard under Master.
        public static float CategoryVolume01(Category category)
        {
            return Mathf.Clamp01(levels[(int)category] / (LevelCount - 1f));
        }

        public static float MasterVolume01 => Mathf.Clamp01(levels[0] / (LevelCount - 1f));

        // ------------------------------------------------------ registration --

        // Explicit registration with a known category and base volume.
        public static void Register(AudioSource source, Category category, float baseVolume = 1f)
        {
            if (source == null) return;
            Unregister(source);
            registered.Add(new Registered
            {
                source = source,
                category = category,
                baseVolume = baseVolume,
                overridesBase = true,
            });
            ApplyTo(source, category, baseVolume);
        }

        public static void Unregister(AudioSource source)
        {
            for (int i = registered.Count - 1; i >= 0; i--)
            {
                if (registered[i].source == source) registered.RemoveAt(i);
            }
        }

        // Name-convention scan over the active scene. Safe to call repeatedly.
        public static void ScanActiveScene()
        {
            AudioSource[] sources = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            for (int i = 0; i < sources.Length; i++)
            {
                AudioSource source = sources[i];
                if (source == null) continue;
                Category category = CategoryFromName(source.gameObject.name);
                if (category == Category.Master) continue; // not a category source
                Register(source, category, source.volume);
            }
        }

        static Category CategoryFromName(string name)
        {
            if (string.IsNullOrEmpty(name)) return Category.Sfx;
            string lower = name.ToLowerInvariant();
            if (lower.StartsWith("mus_") || lower.StartsWith("music") || lower.Contains("music")) return Category.Music;
            if (lower.StartsWith("amb_") || lower.StartsWith("ambien") || lower.Contains("ambience")) return Category.Ambience;
            if (lower.StartsWith("ui") || lower.Contains("_ui")) return Category.Ui;
            if (lower.StartsWith("sfx_") || lower.StartsWith("sfx") || lower.Contains("sfx")) return Category.Sfx;
            return Category.Sfx;
        }

        // ----------------------------------------------------------- volume ----

        static void ApplyAll()
        {
            for (int i = registered.Count - 1; i >= 0; i--)
            {
                Registered entry = registered[i];
                if (entry.source == null)
                {
                    registered.RemoveAt(i); // destroyed sources leave quietly
                    continue;
                }
                ApplyTo(entry.source, entry.category, entry.baseVolume);
            }
        }

        static void ApplyTo(AudioSource source, Category category, float baseVolume)
        {
            if (source == null) return;
            float scaled = baseVolume * CategoryVolume01(category) * MasterVolume01;
            source.volume = Mathf.Clamp01(scaled);
        }
    }
}
