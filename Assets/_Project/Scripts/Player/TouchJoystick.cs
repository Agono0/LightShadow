using UnityEngine;
using UnityEngine.EventSystems;

namespace LightShadow.Player
{
    // Left-hand movement stick for phones. Put this on the joystick's background
    // Image; the handle Image follows the finger while it stays inside the radius,
    // and every drag frame forwards a clamped (-1..1) vector to PlayerController.Move().
    public class TouchJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Wiring")]
        public RectTransform background;
        public RectTransform handle;
        public PlayerController player;
        public float radius = 80f;

        void Awake()
        {
            if (background == null) background = GetComponent<RectTransform>();
            ResolvePlayer();
        }

        void Start()
        {
            ResolvePlayer();
        }

        void ResolvePlayer()
        {
            if (player == null && Core.GameManager.Instance != null && Core.GameManager.Instance.player != null)
            {
                player = Core.GameManager.Instance.player.GetComponent<PlayerController>();
            }
        }

        void OnEnable()
        {
            ResetStick();
        }

        void OnDisable()
        {
            ResetStick();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null) return;

            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    background, eventData.position, eventData.pressEventCamera, out local))
            {
                return;
            }

            Vector2 direction = Vector2.ClampMagnitude(local / radius, 1f);
            if (handle != null) handle.localPosition = direction * radius;
            if (player != null) player.Move(direction);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetStick();
        }

        void ResetStick()
        {
            if (handle != null) handle.localPosition = Vector2.zero;
            if (player != null) player.Move(Vector2.zero);
        }
    }
}
