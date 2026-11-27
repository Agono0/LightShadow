using UnityEngine;
using UnityEngine.EventSystems;

namespace LightShadow.UI
{
    /// <summary>
    /// Hand-rolled hover/press animation for menu buttons (the plan's menu
    /// pass: "animated buttons"). Hover lifts the button slightly with a slow
    /// ease; press dips it; leaving settles it back. Runs on unscaled time so
    /// it behaves identically in menus, and scales the RectTransform only - no
    /// tween library, matching MenuTitleFX.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class MenuButtonFX : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public float hoverScale = 1.06f;
        public float pressScale = 0.97f;
        public float easeSpeed = 10f;

        RectTransform rect;
        float target = 1f;
        bool hovered;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        void OnEnable()
        {
            target = 1f;
            hovered = false;
            if (rect != null) rect.localScale = Vector3.one;
        }

        void Update()
        {
            float current = rect.localScale.x;
            float next = Mathf.Lerp(current, target, Time.unscaledDeltaTime * easeSpeed);
            if (Mathf.Abs(next - target) < 0.001f) next = target;
            rect.localScale = new Vector3(next, next, 1f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            target = hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            target = 1f;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            target = pressScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            target = hovered ? hoverScale : 1f;
        }
    }
}
