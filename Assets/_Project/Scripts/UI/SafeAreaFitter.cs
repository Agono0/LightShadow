using UnityEngine;

namespace LightShadow.UI
{
    // Fits its RectTransform to Screen.safeArea so notched / rounded-corner
    // phones never clip HUD text or touch controls. When the safe area is the
    // full screen (desktop, older phones) the anchors stay at 0..1 and nothing
    // changes visually. SceneBuilderUI.CreateCanvas parents each canvas's
    // content under a "SafeArea" child that carries this component.
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        RectTransform rect;
        Rect lastSafeArea;
        Vector2Int lastScreen = new Vector2Int(-1, -1);

        void Awake()
        {
            rect = (RectTransform)transform;
            Apply();
        }

        void OnEnable()
        {
            if (rect == null) rect = (RectTransform)transform;
            Apply();
        }

        void OnRectTransformDimensionsChange()
        {
            Apply();
        }

        void Update()
        {
            // Rotation and gesture-nav visibility changes do not always reach
            // OnRectTransformDimensionsChange on a child rect, so poll the two
            // cheap inputs instead of relying on that callback alone.
            if (Screen.width != lastScreen.x || Screen.height != lastScreen.y || Screen.safeArea != lastSafeArea)
            {
                Apply();
            }
        }

        void Apply()
        {
            if (rect == null) return;
            int width = Screen.width;
            int height = Screen.height;
            if (width <= 0 || height <= 0) return;

            Rect safe = Screen.safeArea;
            lastSafeArea = safe;
            lastScreen = new Vector2Int(width, height);

            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;
            anchorMin.x /= width;
            anchorMin.y /= height;
            anchorMax.x /= width;
            anchorMax.y /= height;

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
