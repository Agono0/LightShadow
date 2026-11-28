using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LightShadow.Systems
{
    // Hardware detection + frame policy for the premium pass (decisions D8/D16).
    //
    // Refactored after the first premium build: this manager previously ALSO
    // wrote the live URP asset (render scale, MSAA, shadow distance), which
    // fought GameSettings' own writes — the settings card could show values the
    // tier system had silently replaced. Ownership is now split cleanly:
    //
    //   PerformanceManager  → tier auto-detection, frame-rate policy (120 FPS
    //                         targeting / Android refresh clamp), dynamic
    //                         resolution, boot summary log. Read-only towards
    //                         the pipeline asset.
    //   GameSettings (Core) → the single writer of every user-facing graphics
    //                         value, including the tier's URP base values.
    //
    // Adaptive Performance (D16) was deliberately skipped; this tier system plus
    // refresh-rate targeting is the agreed mitigation (see Docs/PERFORMANCE.md).
    [DefaultExecutionOrder(-900)]
    public class PerformanceManager : MonoBehaviour
    {
        public enum Tier { Low = 0, Medium = 1, High = 2, Ultra = 3 }

        // Raised after the frame policy / dynamic resolution were (re)applied.
        public static event Action<Tier> TierApplied;

        public static PerformanceManager Instance { get; private set; }
        public static Tier ActiveTier { get; private set; } = Tier.High;

        // PlayerPrefs key for the manual override. -1 = auto-detect (the default),
        // 0-3 pin Low/Medium/High/Ultra. Owned by GameSettings (Core) — this
        // constant documents the shared key.
        public const string TierPrefKey = "ls_qualityTier";

        // Frame rate policy (D8): Windows targets 120 FPS with vsync off (the
        // uncapped path); Android requests the display refresh clamped 60-120
        // and falls back to 60 when the display does not report one. Sustained
        // 120 FPS needs a 8.3 ms frame - the tiers are what make that reachable
        // per hardware; it is documented, not guaranteed.
        const int WindowsTargetFps = 120;
        const int AndroidMinFps = 60;
        const int AndroidMaxFps = 120;
        const int AndroidFallbackFps = 60;

        // Android auto-detect thresholds on GPU memory (bytes, SystemInfo).
        const long LowTierGpuMemory = 2L * 1024 * 1024 * 1024;    // <= 2 GB GPU -> Low
        const long MediumTierGpuMemory = 4L * 1024 * 1024 * 1024; // <= 4 GB GPU -> Medium
        // Windows "dedicated-VRAM hint large" for Ultra: >= 8 GB GPU memory.
        const long UltraTierGpuMemory = 8L * 1024 * 1024 * 1024;

        // Dynamic resolution: allowed downward scaling per tier (percent of the
        // native target; the handler can only scale DOWN from maxPercentage).
        const int HighDynResMin = 60;
        const int UltraDynResMin = 70;
        const int DynResMax = 100;

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            new GameObject("PerformanceManager").AddComponent<PerformanceManager>();
        }

        // Safety net for play sessions that skip the bootstrap scene (pressing
        // Play inside 02_School in the editor). GameSettings.LoadAndApply is
        // idempotent, so whichever component runs first wins the ordering.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BootstrapFromAttribute()
        {
            EnsureCreated();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // GameSettings owns the full application path; this manager only
            // mirrors the resolved tier for its own decisions and re-asserts
            // the frame policy / dynamic resolution.
            Core.GameSettings.LoadAndApply();
            ApplyFrameRatePolicy();
            LogBootSummary();

            // Tier applied before scene load flags no cameras yet; re-flag the
            // dynamic-resolution cameras whenever a scene arrives.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoadedReapply;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoadedReapply;
        }

        void OnSceneLoadedReapply(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            ApplyDynamicResolution();
        }

        void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoadedReapply;
            if (Instance == this) Instance = null;
        }

        // ------------------------------------------------------------ tiers --

        // Manual override wins inside GameSettings; this is the auto-detect
        // half. Desktop: High by default, Ultra on large dedicated VRAM.
        // Mobile: judged by GPU memory with a Medium fallback for unknowns.
        public static Tier AutoDetectTier()
        {
            long gpuMemory = SystemInfo.graphicsMemorySize; // reported in MB
            gpuMemory *= 1024L * 1024L;

            if (Application.isMobilePlatform)
            {
                if (gpuMemory <= 0)
                {
                    Debug.LogWarning("[LightShadow] GPU memory unknown on " + SystemInfo.deviceModel + ", defaulting to Medium.");
                    return Tier.Medium;
                }
                if (gpuMemory <= LowTierGpuMemory) return Tier.Low;
                if (gpuMemory <= MediumTierGpuMemory) return Tier.Medium;
                return Tier.High;
            }

            // Integrated GPUs report shared system memory as VRAM (an Intel
            // iGPU with 32 GB RAM reports ~9 GB "graphics memory"), which the
            // old threshold mis-read as Ultra-class hardware. Vendor check:
            // Intel iGPUs cap at High; Ultra requires a discrete card.
            string gpuName = SystemInfo.graphicsDeviceName ?? string.Empty;
            bool looksIntegrated = gpuName.IndexOf("Intel", StringComparison.OrdinalIgnoreCase) >= 0
                && gpuName.IndexOf("Arc", StringComparison.OrdinalIgnoreCase) < 0;
            if (looksIntegrated || LooksLikeAmdIntegrated(gpuName)) return Tier.High;

            return gpuMemory >= UltraTierGpuMemory ? Tier.Ultra : Tier.High;
        }

        // AMD APUs report shared system memory as VRAM like Intel iGPUs do
        // (audit 6.4). Their names read "AMD Radeon(TM) Graphics", "Radeon
        // Vega 8 Graphics", "Radeon 780M"; discrete cards say RX, Pro or Fury.
        public static bool LooksLikeAmdIntegrated(string gpuName)
        {
            if (string.IsNullOrEmpty(gpuName)) return false;
            if (gpuName.IndexOf("Radeon", StringComparison.OrdinalIgnoreCase) < 0) return false;
            string[] discrete = { " RX", "Radeon Pro", "Radeon VII", "Fury", "Nano" };
            for (int i = 0; i < discrete.Length; i++)
            {
                if (gpuName.IndexOf(discrete[i], StringComparison.OrdinalIgnoreCase) >= 0) return false;
            }
            return true;
        }

        public static string TierLabel(Tier tier)
        {
            switch (tier)
            {
                case Tier.Low: return "Low";
                case Tier.Medium: return "Medium";
                case Tier.Ultra: return "Ultra";
                default: return "High";
            }
        }

        // Settings-card compatibility entry point: GameSettings.SetTier is the
        // real path now; this forwards so any older caller keeps working.
        public static void SetTierOverride(int tier)
        {
            Core.GameSettings.SetTier(tier);
        }

        // ------------------------------------------------------- framerate --

        // Pure policy (audit V1): this manager sets the frame cadence only
        // while the player's vSync is off; with vSync on the driver owns it.
        public static bool OwnsFrameCadence(bool vSyncOn) { return !vSyncOn; }

        // Called by GameSettings.ApplyFramePolicy when vSync is off (D8).
        public static void ApplyFrameRatePolicy()
        {
            if (!OwnsFrameCadence(Core.GameSettings.VSync))
            {
                // vSync is on (audit V1): GameSettings already cleared the
                // engine target and set vSyncCount 1. Leave both alone; only
                // the tier bookkeeping below still runs.
            }
            else if (Application.isMobilePlatform)
            {
                // Request the display refresh, clamped 60-120; fall back to 60
                // when the panel does not report a usable rate.
                double hz = Screen.currentResolution.refreshRateRatio.numerator > 0
                    ? (double)Screen.currentResolution.refreshRateRatio.numerator
                      / (double)Screen.currentResolution.refreshRateRatio.denominator
                    : 0.0;
                int target = hz <= 0.5
                    ? AndroidFallbackFps
                    : Mathf.Clamp((int)Math.Round(hz), AndroidMinFps, AndroidMaxFps);
                Application.targetFrameRate = target;
                QualitySettings.vSyncCount = 0; // frame pacing owns the cadence
            }
            else
            {
                // Windows: uncapped 120 FPS path (D8).
                Application.targetFrameRate = WindowsTargetFps;
                QualitySettings.vSyncCount = 0;
            }

            Tier tier = ResolveCurrentTier();
            ActiveTier = tier;
            EnableDynamicResolutionOnCameras(tier >= Tier.High);
            ApplyDynamicResolutionRange(tier);
            TierApplied?.Invoke(tier);
        }

        // Re-asserts per-camera dynamic resolution for the live tier. Public
        // because GameSettings calls it after any graphics change.
        public static void ApplyDynamicResolution()
        {
            Tier tier = ResolveCurrentTier();
            EnableDynamicResolutionOnCameras(tier >= Tier.High);
            ApplyDynamicResolutionRange(tier);
        }

        static Tier ResolveCurrentTier()
        {
            int stored = PlayerPrefs.GetInt(TierPrefKey, -1);
            if (stored >= (int)Tier.Low && stored <= (int)Tier.Ultra) return (Tier)stored;
            return Core.GameSettings.AppliedTier >= 0 ? (Tier)Core.GameSettings.AppliedTier : AutoDetectTier();
        }

        // Per-tier min/max scaling for the dynamic resolution handler (URP 17.4
        // moved these off the RP asset into the core handler). Guarded so a
        // future API change degrades to camera-flag-only behaviour.
        static void ApplyDynamicResolutionRange(Tier tier)
        {
            if (tier < Tier.High) return;
            try
            {
                var settings = GlobalDynamicResolutionSettings.NewDefault();
                settings.enabled = true;
                settings.minPercentage = tier == Tier.Ultra ? UltraDynResMin : HighDynResMin;
                settings.maxPercentage = DynResMax;
                DynamicResolutionHandler.instance.Update(settings);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Dynamic resolution range not applied: " + exception.Message);
            }
        }

        // Dynamic resolution: camera.allowDynamicResolution is the stable public
        // flag (available since 2018.3 on UnityEngine.Camera). The min/max range
        // lives in ApplyDynamicResolutionRange, next to the handler.
        static void EnableDynamicResolutionOnCameras(bool enabled)
        {
            Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] != null) cameras[i].allowDynamicResolution = enabled;
            }
        }

        void LogBootSummary()
        {
            UniversalRenderPipelineAsset asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            float scale = asset != null ? asset.renderScale : -1f;
            float shadows = asset != null ? asset.shadowDistance : -1f;
            Debug.Log("[LightShadow] Performance: tier=" + TierLabel(ActiveTier)
                + ", targetFps=" + Application.targetFrameRate
                + ", renderScale=" + scale.ToString("0.00")
                + ", shadowDistance=" + shadows.ToString("0")
                + ", device=" + SystemInfo.deviceModel
                + ", gpuMemoryMB=" + SystemInfo.graphicsMemorySize);
        }
    }
}
