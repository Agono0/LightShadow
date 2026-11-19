using System;
using LightShadow.Core;
using LightShadow.Enemy;
using LightShadow.Gameplay;
using UnityEngine;
using UnityEngine.Audio;

namespace LightShadow.Audio
{
    /// <summary>
    /// Runtime bridge for the real AudioMixer (D15 follow-up, AGENT_REPORT_AUDIO_MIXER
    /// §4). Two jobs, one component:
    ///
    ///   1. Wiring — hands the M_Main mixer to VolumeCategories once (idempotent),
    ///      so every routed source hears the sliders through mixer group gains.
    ///   2. Snapshot driving — holds a persistent snapshot (Explore/Stalk/Chase/
    ///      Dawn, chosen by the Shadow FSM via GameEvents.EnemyStateChanged) plus a
    ///      transient overlay (MemoryFlash, on FragmentCollected/FuseRestored) and
    ///      polls LitZone.PlayerInLitZone for the Hide muffle. The highest-priority
    ///      active layer wins: MemoryFlash overlay > Hide > Dawn > FSM state.
    ///
    /// HARD RULE (spec §4.1, mixer semantics): a parameter that ever receives
    /// SetFloat is never again controlled by snapshots. The user sliders own the
    /// seven *Vol params (VolumeCategories); the duck children + MasterLowpass are
    /// snapshot-owned and must NEVER be SetFloat'ed. AssertNotSnapshotOwned enforces
    /// that in the editor.
    /// </summary>
    public class AudioSnapshotController : MonoBehaviour
    {
        // Exact asset names — the mixer builder (AudioMixerTool) asserts against
        // these, so a rename must land on both sides in the same commit.
        public const string SnapshotExplore = "Snapshot_Explore";
        public const string SnapshotStalk = "Snapshot_Stalk";
        public const string SnapshotChase = "Snapshot_Chase";
        public const string SnapshotHide = "Snapshot_Hide";
        public const string SnapshotMemoryFlash = "Snapshot_MemoryFlash";
        public const string SnapshotDawn = "Snapshot_Dawn";

        public static readonly string[] SnapshotNames =
        {
            SnapshotExplore, SnapshotStalk, SnapshotChase, SnapshotHide, SnapshotMemoryFlash, SnapshotDawn,
        };

        // Parameters the snapshots animate; SetFloat on any of these would freeze
        // the snapshot choreography on that value forever.
        public static readonly string[] SnapshotOwnedParams =
        {
            "MusicDuckVol", "AmbienceDuckVol", "SfxDuckVol", "VoiceDuckVol", "ShadowDuckVol", "UiDuckVol",
            "MasterLowpass",
        };

        [Header("Wiring (scene builder)")]
        public AudioMixer mixer;
        public GameConfig config;

        static AudioSnapshotController instance;

        // Persistent layer ---------------------------------------------------
        EnemyBehaviour.State state = EnemyBehaviour.State.Patrol;
        EnemyBehaviour.State previousState = EnemyBehaviour.State.Patrol;
        bool dawnActive;

        // Hide muffle (polled; LitZone publishes a static bool, no event exists).
        bool playerInLitZone;

        // MemoryFlash overlay (expires back to the persistent layer).
        float overlayExpiresAt = -1f;

        string appliedSnapshot;

        // ------------------------------------------------------- lifecycle ----

        void Awake()
        {
            if (instance != null && instance != this)
            {
                // Shared objects (00_Bootstrap, school Systems): only the first
                // controller drives; destroy the duplicate component, never the
                // host GameObject.
                Destroy(this);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            if (config == null && GameManager.Instance != null) config = GameManager.Instance.config;

            // Idempotent; also applies the saved slider levels to the mixer.
            VolumeCategories.SetMixer(mixer);
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        void OnEnable()
        {
            GameEvents.EnemyStateChanged += OnEnemyStateChanged;
            GameEvents.FragmentCollected += OnMemoryFlash;
            GameEvents.FuseRestored += OnMemoryFlash;
            GameEvents.DawnStarted += OnDawnStarted;
            GameEvents.EscapeStarted += OnEscapeStarted;
        }

        void OnDisable()
        {
            GameEvents.EnemyStateChanged -= OnEnemyStateChanged;
            GameEvents.FragmentCollected -= OnMemoryFlash;
            GameEvents.FuseRestored -= OnMemoryFlash;
            GameEvents.DawnStarted -= OnDawnStarted;
            GameEvents.EscapeStarted -= OnEscapeStarted;
        }

        void Start()
        {
            // First state raise: Explore until the Shadow does something.
            Apply(ResolvePersistent(), TransitionExplore);
        }

        // ---------------------------------------------------------- events ----

        void OnEnemyStateChanged(int stateValue)
        {
            previousState = state;
            state = (EnemyBehaviour.State)stateValue;
            if (overlayExpiresAt > 0f) return; // MemoryFlash owns the mix until it expires
            Apply(ResolvePersistent(), SecondsFor(state, previousState));
        }

        void OnMemoryFlash(int a, int b) => BeginMemoryFlash();

        void OnDawnStarted()
        {
            dawnActive = true;
            if (overlayExpiresAt > 0f) return; // the overlay expiry re-resolves with Dawn live
            playerInLitZone = LitZone.PlayerInLitZone;
            Apply(ResolvePersistent(), TransitionDawn);
        }

        // Dawn holds through the escape (the ending keeps the open mix) — the
        // EscapeStarted subscription exists to make that hold explicit, not to
        // change anything (spec §4.3 table).
        void OnEscapeStarted() { }

        // ------------------------------------------------------------ update ----

        void Update()
        {
            // MemoryFlash overlay owns the mix until it expires; nothing below
            // may stomp it mid-flash (state changes / zone entries are recorded
            // and resolved on expiry).
            if (overlayExpiresAt > 0f)
            {
                if (Time.unscaledTime >= overlayExpiresAt)
                {
                    overlayExpiresAt = -1f;
                    Apply(ResolvePersistent(), TransitionMemoryFlashRelease);
                }
                return;
            }

            // Hide muffle: one bool poll per frame (LitZone has no event; a poll
            // is free). True = Snapshot_Hide, false = back to the FSM snapshot.
            bool inLitZone = LitZone.PlayerInLitZone;
            if (inLitZone != playerInLitZone)
            {
                playerInLitZone = inLitZone;
                Apply(ResolvePersistent(),
                    inLitZone ? TransitionHide : TransitionHideRelease);
            }
        }

        // -------------------------------------------------------- resolution ----

        // Priority (documented in the class header): MemoryFlash overlay > Hide >
        // Dawn > FSM state. Fragment pickups duck the world around the narrator
        // even mid-chase; lit rooms muffle even after the gate powers.
        string ResolvePersistent()
        {
            if (playerInLitZone) return SnapshotHide;
            if (dawnActive) return SnapshotDawn;
            return StateSnapshot(state);
        }

        static string StateSnapshot(EnemyBehaviour.State value)
        {
            switch (value)
            {
                case EnemyBehaviour.State.Chase: return SnapshotChase;
                case EnemyBehaviour.State.Stalk:
                case EnemyBehaviour.State.Repelled: return SnapshotStalk;
                default: return SnapshotExplore;
            }
        }

        // Leaving the chase releases (0.6 s) regardless of where it lands — the
        // panic needs a tail, not a cliff (spec §4.3).
        static float SecondsFor(EnemyBehaviour.State next, EnemyBehaviour.State before)
        {
            if (before == EnemyBehaviour.State.Chase && next != EnemyBehaviour.State.Chase)
            {
                return TransitionRelease;
            }
            return StateSeconds(next);
        }

        static float StateSeconds(EnemyBehaviour.State value)
        {
            switch (value)
            {
                case EnemyBehaviour.State.Chase: return TransitionChase;
                case EnemyBehaviour.State.Stalk:
                case EnemyBehaviour.State.Repelled: return TransitionStalk;
                default: return TransitionExplore;
            }
        }

        void BeginMemoryFlash()
        {
            overlayExpiresAt = Time.unscaledTime + TransitionMemoryFlashSeconds;
            Apply(SnapshotMemoryFlash, TransitionMemoryFlash);
        }

        // ------------------------------------------------------------ apply ----

        void Apply(string snapshotName, float seconds)
        {
            if (mixer == null || string.IsNullOrEmpty(snapshotName)) return;
            if (appliedSnapshot == snapshotName) return; // never restart a transition

            AudioMixerSnapshot snapshot = mixer.FindSnapshot(snapshotName);
            if (snapshot == null) return; // mixer asset missing/broken: legacy path keeps playing

            appliedSnapshot = snapshotName;
            snapshot.TransitionTo(Mathf.Max(0f, seconds));
        }

        // Config shortcuts with the spec's defaults as safe fallbacks
        // (same pattern as EnemyBehaviour).
        GameConfig Config => config != null ? config : (config = GameManager.Instance != null ? GameManager.Instance.config : null);
        static float TransitionExplore => GetConfig(c => c.snapshotTransitionExplore, 0.75f);
        static float TransitionStalk => GetConfig(c => c.snapshotTransitionStalk, 0.5f);
        static float TransitionChase => GetConfig(c => c.snapshotTransitionChase, 0.15f);
        static float TransitionRelease => GetConfig(c => c.snapshotTransitionRelease, 0.6f);
        static float TransitionHide => GetConfig(c => c.snapshotTransitionHide, 0.8f);
        static float TransitionHideRelease => GetConfig(c => c.snapshotTransitionHideRelease, 0.5f);
        static float TransitionMemoryFlash => GetConfig(c => c.snapshotTransitionMemoryFlash, 0.35f);
        static float TransitionMemoryFlashRelease => GetConfig(c => c.snapshotTransitionMemoryFlashRelease, 1.2f);
        static float TransitionDawn => GetConfig(c => c.snapshotTransitionDawn, 2.5f);
        static float TransitionMemoryFlashSeconds => GetConfig(c => c.memoryFlashSeconds, 6f);

        static float GetConfig(Func<GameConfig, float> selector, float fallback)
        {
            AudioSnapshotController owner = instance;
            GameConfig config = owner != null ? owner.Config : null;
            return config != null ? selector(config) : fallback;
        }

        // ------------------------------------------------------ hard rule ------

        /// <summary>
        /// Editor assert for the SetFloat/snapshot orthogonality rule (spec §4.1,
        /// §6.2): VolumeCategories must only SetFloat the seven user params. Call
        /// before every mixer.SetFloat with a user parameter name; fires when a
        /// snapshot-owned name is passed (asserts are active in the editor and in
        /// development builds only, so the release path pays nothing).
        /// </summary>
        public static void AssertNotSnapshotOwned(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName)) return;
            for (int i = 0; i < SnapshotOwnedParams.Length; i++)
            {
                if (SnapshotOwnedParams[i] == parameterName)
                {
                    UnityEngine.Assertions.Assert.IsFalse(
                        true,
                        "[LightShadow] SetFloat must never target snapshot-owned parameter '" + parameterName
                        + "' — snapshots would stop animating it (AGENT_REPORT_AUDIO_MIXER §4.1).");
                    return;
                }
            }
        }
    }
}
