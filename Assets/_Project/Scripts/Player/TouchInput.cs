using UnityEngine;
using UnityEngine.EventSystems;

namespace LightShadow.Player
{
    // Doc 4.5 / Prompt 6: on phones the right half of the screen is the look area.
    // TouchInput watches raw touches, ignores fingers that started over HUD
    // buttons/jostick, and forwards drag deltas to PlayerController.Look().
    public class TouchInput : MonoBehaviour
    {
        [Header("Wiring")]
        public PlayerController player;
        public GameObject mobileHudRoot;

        [Tooltip("Finger pixels -> look units. Tuned so phone and mouse feel similar.")]
        public float lookScale = 0.15f;

        int lookFingerId = -1;

        void Awake()
        {
            if (player == null && Core.GameManager.Instance != null && Core.GameManager.Instance.player != null)
            {
                player = Core.GameManager.Instance.player.GetComponent<PlayerController>();
            }

            // Touch controls only appear on devices that actually have a touchscreen.
            if (mobileHudRoot != null) mobileHudRoot.SetActive(Application.isMobilePlatform);
        }

        void Update()
        {
            if (!Application.isMobilePlatform) return;

            foreach (Touch touch in Input.touches)
            {
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        // A finger that begins on top of UI belongs to a button, not to look.
                        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId)) break;
                        // Only the right half of the screen rotates the camera.
                        if (lookFingerId == -1 && touch.position.x > Screen.width * 0.5f)
                        {
                            lookFingerId = touch.fingerId;
                        }
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (touch.fingerId == lookFingerId && player != null)
                        {
                            Vector2 delta = touch.deltaPosition * lookScale;
                            player.Look(delta);
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (touch.fingerId == lookFingerId) lookFingerId = -1;
                        break;
                }
            }
        }
    }
}
