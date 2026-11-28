using UnityEngine;

namespace LightShadow.UI
{
    /// <summary>
    /// Keeps a fixed-size card inside its parent (the safe-area overlay).
    /// The settings card is 1080 tall and the mobile canvas is 900, so on a
    /// phone the title and Reset/Close ran off screen. The whole card scales
    /// down only as far as needed (never up), children included.
    /// </summary>
    public class CardFit : MonoBehaviour
    {
        public const float Margin = 24f;
        Vector2 lastParent = new Vector2(-1f, -1f);

        // Pure rule so tests can check the real card against real canvases.
        public static float FitScale(Vector2 card, Vector2 available, float margin)
        {
            if (card.x <= 0f || card.y <= 0f) return 1f;
            float sx = (available.x - 2f * margin) / card.x;
            float sy = (available.y - 2f * margin) / card.y;
            float s = Mathf.Min(1f, Mathf.Min(sx, sy));
            return s < 0.1f ? 0.1f : s;
        }

        void LateUpdate()
        {
            RectTransform self = transform as RectTransform;
            RectTransform parent = transform.parent as RectTransform;
            if (self == null || parent == null) return;
            Vector2 size = parent.rect.size;
            if (size == lastParent) return;
            lastParent = size;
            float s = FitScale(self.sizeDelta, size, Margin);
            self.localScale = new Vector3(s, s, 1f);
        }
    }
}
