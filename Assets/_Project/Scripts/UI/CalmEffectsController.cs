using LightShadow.Core;
using LightShadow.Player;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LightShadow.UI
{
    // P3 calm FX (plan 4.7): low Calm adds vignette, a heartbeat thump and a
    // hand tremor on the beam. One component on the player camera.
    //
    //   - Vignette: a local global Volume with a runtime profile instance and a
    //     HIGHER priority than the scene's Post_Night/Post_Dawn volume (which
    //     is built with priority 1), so this controller's Vignette parameter
    //     wins while present. The baseline is read live from the scene volume
    //     each profile change, so dawn stays dawn - the controller only adds
    //     the calm-driven amount on top.
    //   - Heartbeat: a synthesised lub-dub (ProceduralSfx.Heartbeat, SFX_Crank
    //     style library-first id "SFX_Heartbeat") paced 55-150 bpm by calm.
    //   - Tremor: CameraShake.SetTremor, a persistent floor that one-shot
    //     shakes (the catch) still override.
    //
    // Attached by the scene builder; without it the game plays exactly as
    // before (no FX, no volume).
    public class CalmEffectsController : MonoBehaviour
    {
        [Header("Wiring (resolved when empty)")]
        public GameConfig config;
        public CameraShake cameraShake;

        Volume volume;                 // this controller's own override volume
        VolumeProfile profile;
        Vignette vignette;

        Volume sceneVolume;            // the scene's post volume (priority 1)
        VolumeProfile cachedSceneProfile;
        Vignette sceneVignette;

        float fx;                      // smoothed 0-1 FX intensity
        float heartbeatClock;

        // Config shortcuts with safe fallbacks (same pattern as EnemyBehaviour).
        float Floor => config != null ? config.calmFxFloor : 0.45f;
        float VignetteBaseline => config != null ? config.calmFxVignetteBaseline : 0.34f;
        float VignetteMax => config != null ? config.calmFxVignetteMax : 0.45f;
        float HeartbeatMinBpm => config != null ? config.calmFxHeartbeatMinBpm : 55f;
        float HeartbeatMaxBpm => config != null ? config.calmFxHeartbeatMaxBpm : 150f;
        float HeartbeatGain => config != null ? config.calmFxHeartbeatGain : 0.55f;
        float TremorMax => config != null ? config.calmFxTremorMax : 0.004f;
        float SmoothSpeed => config != null ? config.calmFxSmoothSpeed : 2f;

        void Awake()
        {
            if (config == null && GameManager.Instance != null) config = GameManager.Instance.config;
            if (cameraShake == null) cameraShake = GetComponent<CameraShake>();

            // Runtime profile instance: the shared post assets are never touched.
            volume = gameObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 2f;    // the scene post volume sits at priority 1

            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            vignette = ScriptableObject.CreateInstance<Vignette>();
            vignette.active = true;
            vignette.intensity.overrideState = true;
            vignette.intensity.value = VignetteBaseline;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.55f;
            vignette.rounded.overrideState = true;
            vignette.rounded.value = false;
            profile.components.Add(vignette);
            volume.sharedProfile = profile;
        }

        void OnEnable()
        {
            GameEvents.CalmChanged += OnCalmChanged;
        }

        void OnDisable()
        {
            GameEvents.CalmChanged -= OnCalmChanged;
            if (cameraShake != null) cameraShake.SetTremor(0f);
        }

        void OnDestroy()
        {
            // Awake built a runtime profile instance for the local Volume:
            // detach and destroy it so the ScriptableObject does not outlive
            // the scene. The scene's shared post assets are never touched
            // (they belong to their own assets).
            if (volume != null) volume.sharedProfile = null;
            if (profile != null) Destroy(profile);
            profile = null;
            vignette = null;
        }

        void Start()
        {
            // First paint from wherever the calm stat already is.
            if (Story.StoryDirector.Instance != null) OnCalmChanged(Story.StoryDirector.Instance.NormalizedCalm);
        }

        void OnCalmChanged(float normalized)
        {
            // Store the target; the Update smooths toward it (no abrupt pops).
            normalizedTarget = normalized;
        }

        float normalizedTarget = 1f;

        void Update()
        {
            // FX ramp: full at calm 0, fading out at the floor.
            float target = Mathf.Clamp01((Floor - normalizedTarget) / Mathf.Max(0.001f, Floor));
            fx = Mathf.Lerp(fx, target, Time.deltaTime * SmoothSpeed);
            if (target <= 0f && fx < 0.005f) fx = 0f;   // snap the tail so the tremor floor fully releases

            // ---- vignette ----
            ResolveSceneVolume();
            float baseline = sceneVignette != null && sceneVignette.intensity.overrideState
                ? sceneVignette.intensity.value
                : VignetteBaseline;
            vignette.intensity.value = baseline + fx * VignetteMax;

            // ---- heartbeat ----
            if (fx > 0.01f)
            {
                heartbeatClock -= Time.deltaTime;
                if (heartbeatClock <= 0f)
                {
                    float bpm = Mathf.Lerp(HeartbeatMinBpm, HeartbeatMaxBpm, fx);
                    heartbeatClock = 60f / bpm;
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySfx(ProceduralSfx.Heartbeat, HeartbeatGain * fx);
                    }
                }
            }
            else
            {
                heartbeatClock = 0f;
            }

            // ---- beam tremor ----
            if (cameraShake != null) cameraShake.SetTremor(fx * TremorMax);
        }

        // The scene volume carries Post_Night / Post_Dawn (builder-made,
        // priority 1, sharedProfile). Cache it and re-scan for the Vignette
        // component whenever the profile swaps (dawn). Skip our own volume.
        void ResolveSceneVolume()
        {
            if (volume == null) return;
            if (sceneVolume == null)
            {
                Volume[] volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);
                for (int i = 0; i < volumes.Length; i++)
                {
                    Volume candidate = volumes[i];
                    if (candidate == volume || candidate.sharedProfile == null) continue;
                    if (candidate.priority > volume.priority) continue;
                    sceneVolume = candidate;
                    break;
                }
                cachedSceneProfile = sceneVolume != null ? sceneVolume.sharedProfile : null;
            }

            if (sceneVolume != null && sceneVolume.sharedProfile != cachedSceneProfile)
            {
                cachedSceneProfile = sceneVolume.sharedProfile;
                sceneVignette = null;
            }

            if (cachedSceneProfile != null && sceneVignette == null)
            {
                for (int i = 0; i < cachedSceneProfile.components.Count; i++)
                {
                    if (cachedSceneProfile.components[i] is Vignette found)
                    {
                        sceneVignette = found;
                        break;
                    }
                }
            }
        }
    }
}
