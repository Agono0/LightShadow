using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    /// <summary>
    /// Phones are small and held close, but the canvases are authored against
    /// a 1920x1080 reference, so 20 to 30 px text lands at a few millimetres
    /// (playtest: text too small on Android). On a mobile platform the
    /// reference drops to 1600x900 and matches height, a 1.2x lift for every
    /// piece of UI at once. Panels are authored to fit a 900 px tall canvas.
    /// Desktop is untouched.
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class MobileUiScale : MonoBehaviour
    {
        public static readonly Vector2 MobileReference = new Vector2(1600f, 900f);

        void Awake()
        {
            if (Application.isMobilePlatform) Apply(GetComponent<CanvasScaler>(), Screen.height > 0 ? (float)Screen.width / Screen.height : 2f);
        }

        // Public so the rule is testable without a mobile platform.
        // Wide phones match height (canvas stays 900 tall); squarer screens
        // such as 4:3 tablets match width (canvas stays 1600 wide), so the
        // 1400 px quiz card always fits.
        public static float MatchFor(float aspect)
        {
            return aspect >= 16f / 9f - 0.01f ? 1f : 0f;
        }

        public static void Apply(CanvasScaler scaler, float aspect)
        {
            if (scaler == null) return;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = MobileReference;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = MatchFor(aspect);
        }
    }
}
