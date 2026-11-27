using System.Collections.Generic;
using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Audio
{
    // Listener-side occlusion (premium-pass brief §8.3, AGENT_REPORT_AUDIO_MIXER
    // §5). Sits on the player camera, next to the AudioListener. Every
    // occlusionIntervalSeconds it linecasts from the listener to every active 3D
    // AudioSource in the scene; a wall between the two muffles that source — a
    // lazy per-source AudioLowPassFilter (occlusionCutoffHz) plus a volume dip
    // (occlusionVolumeDb converted dB -> linear) — and a clear line restores it.
    // Interval-driven only: the FindObjectsByType refresh and the raycasts run on
    // the tick, never per frame (the array allocation is 4 Hz, not 60 Hz).
    //
    // Interaction with the mixer (orthogonality reasoning, D15 follow-up §4.1):
    //   - AudioLowPassFilter is per-source DSP inserted after the source feeds
    //     its mixer group; the snapshot duck children and MasterLowpass are
    //     group-level gains animated by AudioSnapshotController. The two chain
    //     (source filter -> category group -> duck -> Master) and never write the
    //     same parameter, so the SetFloat/snapshot orthogonality rule is
    //     untouched — a muffling world and a ducked mix compose instead of fight.
    //   - The volume dip writes source.volume, which VolumeCategories.ApplyTo
    //     (slider moves) and the per-frame envelope writers (ShadowAudio's growl,
    //     AdaptiveMusic's layers) also write. ApplyVolumeFactor arbitrates: the
    //     probe re-scales only when the current volume is exactly the value it
    //     wrote last tick, so it can never divide a stale factor out of another
    //     writer's fresh value (which would boost the source past its envelope).
    public class AudioOcclusionProbe : MonoBehaviour
    {
        // One tracked source and its occlusion state. writtenVolume +
        // appliedFactor implement the multi-writer volume arbitration.
        struct Occluded
        {
            public AudioSource source;
            public AudioLowPassFilter filter;
            public float appliedFactor; // 1 = no dip currently baked into source.volume
            public float writtenVolume; // the exact float the probe last wrote
        }

        readonly List<Occluded> tracked = new List<Occluded>(24);
        float timer;

        [Header("Wiring (scene builder)")]
        public GameConfig config;

        GameConfig Config => config != null ? config
            : (config = GameManager.Instance != null ? GameManager.Instance.config : null);

        void Update()
        {
            GameConfig tuning = Config;
            if (tuning == null) return;

            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = Mathf.Max(0.05f, tuning.occlusionIntervalSeconds);

            Probe(tuning);
        }

        void Probe(GameConfig tuning)
        {
            RefreshTracked();

            Vector3 listenerPosition = transform.position;
            float dipFactor = DbToLinear(tuning.occlusionVolumeDb);

            for (int i = 0; i < tracked.Count; i++)
            {
                Occluded entry = tracked[i];

                if (entry.source == null || !Eligible(entry.source))
                {
                    // A stopped or deactivated source must not sit muffled
                    // silently; restore it so the next play starts clean.
                    RestoreClear(ref entry);
                    tracked[i] = entry;
                    continue;
                }

                if (IsOccluded(listenerPosition, entry.source.transform))
                {
                    EnsureFilter(ref entry, tuning.occlusionCutoffHz);
                    ApplyVolumeFactor(ref entry, dipFactor);
                }
                else
                {
                    RestoreClear(ref entry);
                }

                tracked[i] = entry;
            }
        }

        // 2D sources are omniscient by design (music/ambience/UI — the mixer's
        // business, not the world's): only spatial sources can be occluded.
        static bool Eligible(AudioSource source)
        {
            return source != null
                && source.isActiveAndEnabled
                && source.spatialBlend > 0f
                && source.isPlaying;
        }

        // Prune destroyed sources (their filter dies with them), then pick up new
        // ones; surviving entries keep their arbitration state across rescans.
        void RefreshTracked()
        {
            for (int i = tracked.Count - 1; i >= 0; i--)
            {
                if (tracked[i].source == null) tracked.RemoveAt(i);
            }

            AudioSource[] sources = FindObjectsByType<AudioSource>();
            for (int i = 0; i < sources.Length; i++)
            {
                AudioSource source = sources[i];
                if (!Eligible(source) || IndexOf(source) >= 0) continue;
                tracked.Add(new Occluded { source = source, appliedFactor = 1f });
            }
        }

        int IndexOf(AudioSource source)
        {
            for (int i = 0; i < tracked.Count; i++)
            {
                if (tracked[i].source == source) return i;
            }
            return -1;
        }

        // A wall between listener and source muffles the source. Hits belonging
        // to the source's own hierarchy (the Shadow's capsule in front of its
        // breathing source) or the listener's own body (the player root is the
        // camera's PARENT, hence transform.root) never count as occluders — the
        // same rule EnemySight applies to its LOS test. Reverb-zone and lit-zone
        // triggers are ignored via QueryTriggerInteraction.Ignore.
        bool IsOccluded(Vector3 listenerPosition, Transform sourceTransform)
        {
            Vector3 sourcePosition = sourceTransform.position;
            if ((sourcePosition - listenerPosition).sqrMagnitude < 0.0001f) return false;

            if (Physics.Linecast(listenerPosition, sourcePosition, out RaycastHit hit,
                ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.IsChildOf(sourceTransform)) return false;
                if (hit.transform.IsChildOf(transform.root)) return false;
                return true;
            }
            return false;
        }

        // Lazily add the filter the first time a source is occluded; afterwards
        // toggling enabled + cutoff restores the clear state without the
        // add/destroy churn of repeatedly hugging a wall.
        static void EnsureFilter(ref Occluded entry, float cutoffHz)
        {
            if (entry.filter == null)
            {
                AudioLowPassFilter existing = entry.source.GetComponent<AudioLowPassFilter>();
                entry.filter = existing != null
                    ? existing
                    : entry.source.gameObject.AddComponent<AudioLowPassFilter>();
            }

            entry.filter.enabled = true;
            entry.filter.cutoffFrequency = Mathf.Clamp(cutoffHz, 10f, 22000f);
        }

        static void RestoreClear(ref Occluded entry)
        {
            if (entry.filter != null) entry.filter.enabled = false;
            ApplyVolumeFactor(ref entry, 1f);
        }

        // Multiplicative dip with ownership arbitration (class header): re-scale
        // only when the current volume is exactly what this probe wrote last
        // tick; otherwise another writer owns the value and the factor applies
        // on top of its fresh envelope instead.
        static void ApplyVolumeFactor(ref Occluded entry, float factor)
        {
            AudioSource source = entry.source;
            if (source == null) return;

            float current = source.volume;
            bool oursIntact = entry.appliedFactor != 1f && current == entry.writtenVolume;
            float baseVolume = oursIntact ? current / entry.appliedFactor : current;
            float written = Mathf.Clamp01(baseVolume * factor);

            if (written != current) source.volume = written;
            entry.appliedFactor = factor;
            entry.writtenVolume = written;
        }

        static float DbToLinear(float db)
        {
            return Mathf.Pow(10f, db / 20f);
        }

        void OnDisable()
        {
            // Restore everything the probe touched so a disabled probe leaves the
            // mix exactly as it found it (volumes un-dipped, filters off).
            for (int i = 0; i < tracked.Count; i++)
            {
                Occluded entry = tracked[i];
                RestoreClear(ref entry);
                tracked[i] = entry;
            }
        }
    }
}
