using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace LightShadow.Audio
{
    // Category mixer bridge (D15 follow-up, AGENT_REPORT_AUDIO_MIXER §4.4):
    // the settings-card UX is unchanged (sliders Master/Music/Ambience/SFX/UI,
    // 0-10, PlayerPrefs-backed) but the levels now land on the REAL AudioMixer
    // asset (M_Main) when one is wired, and keep the original per-source
    // multiply when it is not (headless validation, direct scene play before
    // the bootstrap runs).
    //
    // Split of ownership (§4.1 — the SetFloat/snapshot orthogonality rule):
    //   - The seven user params (MasterVol..UiVol) are script-owned: SetFloat
    //     only ever targets them, so snapshot transitions never fight sliders.
    //   - The duck children + Master low-pass are snapshot-owned and are NEVER
    //     SetFloat'ed (AudioSnapshotController.AssertNotSnapshotOwned asserts it).
    //
    // Sources register two ways (unchanged):
    //   1. explicitly  - VolumeCategories.Register(source, Category.Music)
    //   2. by scan     - every scene load scans live AudioSources; component
    //      overrides first (Shadow FSM parent -> Shadow, AmbientDrone ->
    //      Ambience, "*whisper*" -> Shadow), then the naming convention
    //      "MUS_"/"Music*" -> Music, "AMB_"/"Ambien*" -> Ambience, "SFX_" -> Sfx,
    //      "UI*" -> Ui, unmatched -> Sfx.
    public static class VolumeCategories
    {
        // Append-only enum: PlayerPrefs indices of the original five categories
        // must never move (saved player settings survive the mixer migration).
        public enum Category { Master = 0, Music = 1, Ambience = 2, Sfx = 3, Ui = 4, Voice = 5, Shadow = 6 }

        public const int LevelCount = 11; // 0..10 slider steps

        static readonly string[] PrefKeys =
        {
            "ls_vol_master", "ls_vol_music", "ls_vol_ambience", "ls_vol_sfx", "ls_vol_ui",
            "ls_vol_voice", "ls_vol_shadow",
        };

        const float DefaultLevel = 6f;

        // Mixer handle (§4.4.3): wired by AudioSnapshotController.SetMixer; an
        // unresolved group leaves GroupFor null and the legacy path keeps working.
        static AudioMixer mixer;
        static readonly AudioMixerGroup[] groups = new AudioMixerGroup[7];

        // Category value -> exact mixer group name (FindMatchingGroups matches by
        // sub-path, so the exact-name pick stops "Music" resolving to "MusicDuck").
        static readonly string[] GroupNames =
        {
            "Master", "Music", "Ambience", "SFX", "UI", "Voice", "Shadow",
        };

        public static bool MixerActive => mixer != null;

        // One registered source: the base volume the owner set, scaled live.
        struct Registered
        {
            public AudioSource source;
            public Category category;
            public float baseVolume;
            public bool overridesBase; // true = baseVolume is explicit, not captured
        }

        static readonly List<Registered> registered = new List<Registered>(16);
        static readonly float[] levels =
        {
            DefaultLevel, DefaultLevel, DefaultLevel, DefaultLevel, DefaultLevel, DefaultLevel, DefaultLevel,
        };
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

        // ----------------------------------------------------------- mixer ----

        // Idempotent: the first non-null mixer wins, groups resolve once, and the
        // saved slider levels are applied to the mixer immediately. Sources that
        // registered before the mixer landed are re-routed onto the resolved
        // groups. Safe to call from both scene-built AudioSnapshotControllers (§4.2).
        public static void SetMixer(AudioMixer value)
        {
            if (value == null || mixer == value) return;
            mixer = value;

            for (int i = 0; i < groups.Length; i++)
            {
                groups[i] = FindGroupExact(GroupNames[i]);
            }

            ApplyMixerVolumes();
            // Defensive: a source registered before mixer activation got the
            // legacy volume math and no bus (GroupFor was null). Re-apply the
            // full registry now that the groups exist so every source lands
            // on its mixer group.
            ApplyAll();
        }

        public static AudioMixerGroup GroupFor(Category category)
        {
            int index = (int)category;
            return index >= 0 && index < groups.Length ? groups[index] : null;
        }

        static AudioMixerGroup FindGroupExact(string name)
        {
            if (mixer == null) return null;
            AudioMixerGroup[] matches = mixer.FindMatchingGroups(name);
            for (int i = 0; i < matches.Length; i++)
            {
                if (matches[i] != null && matches[i].name == name) return matches[i];
            }
            return null;
        }

        // Slider level (0..10) -> dB for the exposed user params (§4.1):
        // 10 -> 0 dB, 5 -> about -6 dB, 0 -> -80 dB (silent, like the old 0).
        static float LevelToDb(float level)
        {
            return level <= 0f ? -80f : 20f * Mathf.Log10(level / 10f);
        }

        // Only the seven user params; duck/lowpass names are hard-asserted off
        // limits (AudioSnapshotController.AssertNotSnapshotOwned).
        static void ApplyMixerVolumes()
        {
            if (!MixerActive) return;
            for (int i = 0; i < groups.Length; i++)
            {
                string parameter = GroupToParameter[i];
                if (parameter == null) continue;
                AudioSnapshotController.AssertNotSnapshotOwned(parameter);
                mixer.SetFloat(parameter, LevelToDb(levels[i]));
            }
        }

        // Indexed by Category; Master carries MasterVol (its group volume is the
        // master slider). Duck/lowpass params are deliberately absent here.
        static readonly string[] GroupToParameter =
        {
            "MasterVol", "MusicVol", "AmbienceVol", "SfxVol", "UiVol", "VoiceVol", "ShadowVol",
        };

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
            if (MixerActive) ApplyMixerVolumes();
            else ApplyAll();
            CategoryChanged?.Invoke();
        }

        // Category volume 0..1 as heard under Master (legacy path + AdaptiveMusic).
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

        static bool IsRegistered(AudioSource source)
        {
            for (int i = 0; i < registered.Count; i++)
            {
                if (registered[i].source == source) return true;
            }
            return false;
        }

        public static void Unregister(AudioSource source)
        {
            for (int i = registered.Count - 1; i >= 0; i--)
            {
                if (registered[i].source == source) registered.RemoveAt(i);
            }
        }

        // Scan over the active scene. Safe to call repeatedly.
        public static void ScanActiveScene()
        {
            AudioSource[] sources = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            for (int i = 0; i < sources.Length; i++)
            {
                AudioSource source = sources[i];
                if (source == null) continue;
                // Already registered (explicitly or by an earlier scan): do not
                // re-register. Re-registering captured the already scaled volume
                // as the new base and stomped pads for one frame (audit 4.3).
                if (IsRegistered(source)) continue;
                Category category = CategoryFor(source);
                if (category == Category.Master) continue; // not a category source
                Register(source, category, source.volume);
            }
        }

        // Component overrides first (§4.4.6), then the naming convention. The
        // component checks fix the known mis-categorizations: the Shadow's
        // breathing/one-shot/growl/stinger sources and the pooled whispers land
        // on the Shadow bus, AmbientDrone's player-rooted source on Ambience.
        static Category CategoryFor(AudioSource source)
        {
            if (source == null) return Category.Sfx;

            LightShadow.Core.AudioManager manager = LightShadow.Core.AudioManager.Instance;
            if (manager != null && manager.voiceSource == source) return Category.Voice;

            if (source.GetComponentInParent<LightShadow.Enemy.EnemyBehaviour>() != null) return Category.Shadow;
            LightShadow.Core.AmbientDrone drone = source.GetComponent<LightShadow.Core.AmbientDrone>();
            if (drone != null && drone.Source == source) return Category.Ambience; // only the drone's own source, not its siblings

            string name = source.gameObject.name;
            if (!string.IsNullOrEmpty(name) && name.ToLowerInvariant().Contains("whisper")) return Category.Shadow;

            return CategoryFromName(name);
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

        // Mixer active: the category x master product lives on the mixer group
        // gains (same product as the legacy math, §4.4.5), so the per-source
        // volume keeps only the authored base. Legacy: the original multiply.
        static void ApplyTo(AudioSource source, Category category, float baseVolume)
        {
            if (source == null) return;
            AudioMixerGroup group = GroupFor(category);
            if (group != null) source.outputAudioMixerGroup = group;
            source.volume = MixerActive
                ? Mathf.Clamp01(baseVolume)
                : Mathf.Clamp01(baseVolume * CategoryVolume01(category) * MasterVolume01);
        }
    }
}
