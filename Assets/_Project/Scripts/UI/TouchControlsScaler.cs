using LightShadow.Core;
using LightShadow.Player;
using UnityEngine;

namespace LightShadow.UI
{
    // Applies the "Touch control size" setting to the builder-generated mobile
    // controls (user brief 2026-11-27). Every direct child of the mobile root
    // has its sizeDelta and corner offset scaled, and the joystick radius and
    // handle follow so throw distance and visuals stay proportional. Baselines
    // are captured once in Awake, so re-applying on GameSettings.Changed never
    // compounds. Awake is deferred until the mobile root first activates.
    public class TouchControlsScaler : MonoBehaviour
    {
        struct Baseline
        {
            public RectTransform rect;
            public Vector2 size;
            public Vector2 position;
        }

        Baseline[] items;
        TouchJoystick joystick;
        float joystickRadius;
        RectTransform joystickHandle;
        Vector2 handleSize;

        void Awake()
        {
            var list = new System.Collections.Generic.List<Baseline>();
            for (int i = 0; i < transform.childCount; i++)
            {
                RectTransform child = transform.GetChild(i) as RectTransform;
                if (child == null) continue;
                list.Add(new Baseline
                {
                    rect = child,
                    size = child.sizeDelta,
                    position = child.anchoredPosition,
                });

                TouchJoystick candidate = child.GetComponent<TouchJoystick>();
                if (candidate != null)
                {
                    joystick = candidate;
                    joystickRadius = candidate.radius;
                    joystickHandle = candidate.handle;
                    if (joystickHandle != null) handleSize = joystickHandle.sizeDelta;
                }
            }
            items = list.ToArray();
            Apply();
        }

        void OnEnable()
        {
            GameSettings.Changed += Apply;
        }

        void OnDisable()
        {
            GameSettings.Changed -= Apply;
        }

        void Apply()
        {
            float scale = GameSettings.ControlScale;
            if (items != null)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].rect == null) continue;
                    items[i].rect.sizeDelta = items[i].size * scale;
                    items[i].rect.anchoredPosition = items[i].position * scale;
                }
            }
            if (joystick != null) joystick.radius = joystickRadius * scale;
            if (joystickHandle != null) joystickHandle.sizeDelta = handleSize * scale;
        }
    }
}
