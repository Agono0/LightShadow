using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LightShadow.Core
{
    // Applies the brightness option with URP post-exposure, with an ambient-light
    // fallback for quality levels where post-processing is disabled (the mobile
    // URP asset). One instance lives in the school scene.
    public class BrightnessController : MonoBehaviour
    {
        Volume volume;
        VolumeProfile profile;
        ColorAdjustments color;
        Color baseAmbient;

        void Awake()
        {
            baseAmbient = RenderSettings.ambientLight;

            Camera camera = Camera.main;
            if (camera != null)
            {
                UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();
                if (data != null) data.renderPostProcessing = true;
            }

            volume = gameObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 100f;

            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "RuntimeBrightnessProfile";
            color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(0f);
            volume.profile = profile;

            GameSettings.Changed += Apply;
            Apply();
        }

        void OnDestroy()
        {
            GameSettings.Changed -= Apply;
            if (volume != null) volume.profile = null;
            if (profile != null) Destroy(profile);
        }

        void Apply()
        {
            float exposure = Mathf.Lerp(-1.2f, 1.2f, GameSettings.Brightness);
            if (color != null) color.postExposure.Override(exposure);

            // Also lift ambient a little: this works even when post-processing is
            // off, and keeps the flashlight feeling like the main light source.
            RenderSettings.ambientLight = baseAmbient * Mathf.Lerp(0.75f, 1.45f, GameSettings.Brightness);
        }
    }
}
