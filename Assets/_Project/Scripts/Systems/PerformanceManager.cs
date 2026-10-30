using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LightShadow.Systems
{
    // Boot-time and runtime quality manager (premium pass, decisions D8/D16).
    // Four tiers (Low/Medium/High/Ultra) map onto the two existing quality
    // levels: Low/Medium run the Mobile URP asset (no SSAO, cheaper renderer),
    // High/Ultra run the PC URP asset (SSAO renderer). The active asset is then
    // retuned at runtime (render scale, shadow distance, MSAA, soft shadows) -
    // the same mechanism GameSettings.ApplyGraphics already uses, so the two
    // systems compose instead of fighting.
    //
    // SSAO note (documented honestly): SSAO is a renderer feature, and the
    // Mobile/PC renderer split already encodes the on/off decision - Low/Medium
    // never see SSAO, High/Ultra do. Toggling the feature itself at runtime is
    // not reliably scriptable headlessly, so it is handled by asset choice.
    //
    // Adaptive Performance (D16) was deliberately skipped; this tier system plus
    // refresh-rate targeting is the agreed mitigation (see Docs/PERFORMANCE.md).
    [DefaultExecutionOrder(-900)]
    public class PerformanceManager : MonoBehaviour
    {
        public enum Tier { Low = 0, Medium = 1, High = 2, Ultra = 3 }

        // Raised after a tier is fully applied (asset swap + retune + framerate).
        public static event Action<Tier> TierApplied;

        public static PerformanceManager Instance { get; private set; }
        public static Tier ActiveTier { get; private set; } = Tier.High;

        // PlayerPrefs key for the manual override. -1 = auto-detect (the default),
        // 0-3 pin Low/Medium/High/Ultra. The settings card writes through
        // SetTierOverride() so the change applies immediately.
        public const string TierPrefKey = "ls_qualityTier";

        // ---- Per-tier tuning (premium-pass targets, Docs/PERFORMANCE.md) ------
        // renderScale:            Low .65 | Medium .75 | High .90 | Ultra 1.0
        // shadowDistance (m):     Low 0   | Medium 20  | High 40  | Ultra 60
        // shadow fade per URP 17.4 defaults (the asset field is Vector3-based and
        // has no public setter; left at asset defaults - see report note).
        // MSAA:                   off/off/2x/2x   Soft shadows: off/off/on/on
        // Dynamic resolution:     enabled on High/Ultra; the per-tier min/max is
        //                         pushed to URP 17.4's core DynamicResolutionHandler
        //                         (the RP asset no longer carries these fields).
        const float LowRenderScale = 0.65f;
        const float MediumRenderScale = 0.75f;
        const float HighRenderScale = 0.9f;
        const float UltraRenderScale = 1.0f;

        const float LowShadowDistance = 0f;
        const float MediumShadowDistance = 20f;
        const float HighShadowDistance = 40f;
        const float UltraShadowDistance = 60f;

        const int LowMsaa = 1;        // 1 = MSAA off in URP
        const int MediumMsaa = 1;
        const int HighMsaa = 2;
        const int UltraMsaa = 2;

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
        const long LowTierGpuMemory = 2L * 1024 * 1024 * 1024;   // <= 2 GB GPU -> Low
        const long MediumTierGpuMemory = 4L * 1024 * 1024 * 1024; // <= 4 GB GPU -> Medium
        // Windows "dedicated-VRAM hint large" for Ultra: >= 8 GB GPU memory.
        const long UltraTierGpuMemory = 8L * 1024 * 1024 * 1024;

        // Dynamic resolution: allowed downward scaling per tier (percent of the
        // native target; the handler can only scale DOWN from maxPercentage).
        const int HighDynResMin = 60;
        const int UltraDynResMin = 70;
        const int DynResMax = 100;

        // Flashlight realtime shadows: the manager never touches Light
        // components at all, so the flashlight's realtime shadow flag set by the
        // scene builder is always preserved on every tier (only the URP asset's
        // shadow DISTANCE changes, which is a global culling distance).

        public static void EnsureCreated()
        {
            if (Instance != null) return;
            new GameObject("PerformanceManager").AddComponent<PerformanceManager>();
        }

        // Safety net for play sessions that skip the bootstrap scene (pressing
        // Play inside 02_School in the editor). Idempotent with EnsureCreated.
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
            ApplyTier(ResolveTier());
            ApplyFrameRatePolicy();
            LogBootSummary();

            // Tier applied before scene load flags no cameras yet; re-flag the
            // dynamic-resolution cameras whenever a scene arrives.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoadedReapply;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoadedReapply;
        }

        void OnSceneLoadedReapply(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            EnableDynamicResolutionOnCameras(ActiveTier >= Tier.High);
        }

        void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoadedReapply;
            if (Instance == this) Instance = null;
        }

        // ------------------------------------------------------------ tiers --

        // Manual override wins; otherwise auto-detect per platform.
        Tier ResolveTier()
        {
            int stored = PlayerPrefs.GetInt(TierPrefKey, -1);
            if (stored >= (int)Tier.Low && stored <= (int)Tier.Ultra) return (Tier)stored;
            return AutoDetectTier();
        }

        Tier AutoDetectTier()
        {
            long gpuMemory = SystemInfo.graphicsMemorySize; // reported in MB
            gpuMemory *= 1024L * 1024L;

            if (Application.isMobilePlatform)
            {
                // Android (and iOS) judge by GPU memory; the device model string
                // only guards the "unknown" case so old/odd devices get Medium.
                if (gpuMemory <= 0)
                {
                    Debug.LogWarning("[LightShadow] GPU memory unknown on " + SystemInfo.deviceModel + ", defaulting to Medium.");
                    return Tier.Medium;
                }
                if (gpuMemory <= LowTierGpuMemory) return Tier.Low;
                if (gpuMemory <= MediumTierGpuMemory) return Tier.Medium;
                return Tier.High;
            }

            // Desktop: High by default, Ultra when the dedicated-VRAM hint is large.
            return gpuMemory >= UltraTierGpuMemory ? Tier.Ultra : Tier.High;
        }

        // Applies a tier right now: swap the pipeline asset (quality level),
        // retune the live URP asset, flip per-camera dynamic resolution.
        public void ApplyTier(Tier tier)
        {
            ActiveTier = tier;

            // Low/Medium -> the Mobile quality level (Mobile_RPAsset, no SSAO
            // renderer). High/Ultra -> the PC level. Levels are matched by name
            // so reordering QualitySettings does not break the mapping.
            int level = QualityLevelForTier(tier);
            if (level >= 0 && level != QualitySettings.GetQualityLevel())
            {
                QualitySettings.SetQualityLevel(level, true);
            }

            UniversalRenderPipelineAsset asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (asset != null)
            {
                asset.renderScale = RenderScaleFor(tier);
                asset.shadowDistance = ShadowDistanceFor(tier);
                asset.msaaSampleCount = MsaaFor(tier);
                TrySetSoftShadows(asset, tier >= Tier.High);
            }
            else
            {
                Debug.LogWarning("[LightShadow] PerformanceManager: active pipeline is not URP; tier tuning skipped.");
            }

            EnableDynamicResolutionOnCameras(tier >= Tier.High);
            ApplyDynamicResolutionRange(tier);
            TierApplied?.Invoke(tier);
        }

        // Per-tier min/max scaling for the dynamic resolution handler (URP 17.4
        // moved these off the RP asset into the core handler). Guarded so a
        // future API change degrades to camera-flag-only behaviour.
        static void ApplyDynamicResolutionRange(Tier tier)
        {
            if (tier < Tier.High) return;
            try
            {
                var settings = UnityEngine.Rendering.GlobalDynamicResolutionSettings.NewDefault();
                settings.enabled = true;
                settings.minPercentage = tier == Tier.Ultra ? UltraDynResMin : HighDynResMin;
                settings.maxPercentage = DynResMax;
                UnityEngine.Rendering.DynamicResolutionHandler.instance.Update(settings);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Dynamic resolution range not applied: " + exception.Message);
            }
        }

        // Settings-card entry point: pin a tier (-1 = auto) and re-apply.
        public static void SetTierOverride(int tier)
        {
            PlayerPrefs.SetInt(TierPrefKey, tier);
            PlayerPrefs.Save();
            if (Instance == null) return;
            Instance.ApplyTier(tier >= (int)Tier.Low && tier <= (int)Tier.Ultra ? (Tier)tier : Instance.ResolveTier());
            Instance.ApplyFrameRatePolicy();
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

        // ---------------------------------------------------- tier numbers --

        public static float RenderScaleFor(Tier tier)
        {
            switch (tier)
            {
                case Tier.Low: return LowRenderScale;
                case Tier.Medium: return MediumRenderScale;
                case Tier.Ultra: return UltraRenderScale;
                default: return HighRenderScale;
            }
        }

        public static float ShadowDistanceFor(Tier tier)
        {
            switch (tier)
            {
                case Tier.Low: return LowShadowDistance;
                case Tier.Medium: return MediumShadowDistance;
                case Tier.Ultra: return UltraShadowDistance;
                default: return HighShadowDistance;
            }
        }

        public static int MsaaFor(Tier tier)
        {
            return tier >= Tier.High ? HighMsaa : LowMsaa;
        }

        int QualityLevelForTier(Tier tier)
        {
            string wanted = tier <= Tier.Medium ? "Mobile" : "PC";
            string[] names = QualitySettings.names;
            for (int i = 0; i < names.Length; i++)
            {
                if (string.Equals(names[i], wanted, StringComparison.OrdinalIgnoreCase)) return i;
            }
            return -1; // keep whatever level is already active
        }

        // softShadowsSupported has an internal setter in URP 17.4, so the
        // runtime flip goes through reflection with a graceful skip if the
        // property shape ever changes. The editor tool sets it properly on the
        // per-tier asset variants via SerializedObject.
        static void TrySetSoftShadows(UniversalRenderPipelineAsset asset, bool value)
        {
            try
            {
                var property = typeof(UniversalRenderPipelineAsset).GetProperty("softShadowsSupported");
                if (property != null && property.CanWrite) property.SetValue(asset, value);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Soft shadow tier flip skipped: " + exception.Message);
            }
        }

        // Dynamic resolution: camera.allowDynamicResolution is the stable public
        // flag (available since 2018.3 on UnityEngine.Camera). The min/max range
        // lives in ApplyDynamicResolutionRange, next to the handler.
        void EnableDynamicResolutionOnCameras(bool enabled)
        {
            Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] != null) cameras[i].allowDynamicResolution = enabled;
            }
        }

        // ------------------------------------------------------- framerate --

        void ApplyFrameRatePolicy()
        {
            if (Application.isMobilePlatform)
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
                // Windows: uncapped 120 FPS path (D8). The player's own VSync
                // toggle in the settings card still wins after boot.
                Application.targetFrameRate = WindowsTargetFps;
                QualitySettings.vSyncCount = 0;
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
