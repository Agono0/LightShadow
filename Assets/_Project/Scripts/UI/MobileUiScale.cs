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
            if (Application.isMobilePlatform) Apply(GetComponent<CanvasScaler>());
        }

        // Public so the rule is testable without a mobile platform.
        public static void Apply(CanvasScaler scaler)
        {
            if (scaler == null) return;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = MobileReference;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
        }
    }
}
