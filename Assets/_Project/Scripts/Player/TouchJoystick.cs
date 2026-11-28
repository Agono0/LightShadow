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
        [Tooltip("Fraction of the radius ignored around the centre, so a resting thumb does not creep.")]
        [Range(0f, 0.4f)] public float deadZone = 0.12f;
        [Tooltip("Handle follow speed (1/s). The movement vector itself is never delayed.")]
        public float handleFollow = 30f;

        // The stick centre: where the thumb first landed (clamped inside the
        // pad), so there is no jump to full speed when the touch starts at the
        // pad edge. The old stick measured from the pad centre.
        Vector2 origin;
        Vector2 handleTarget;
        Vector2 current;
        bool pressed;

        // Pure shaping (tested): dead zone, then a linear remap so the output
        // still reaches 1 at the rim, then a gentle ease so small deflections
        // give fine control. offset is in pad units, radius > 0.
        public static Vector2 Shape(Vector2 offset, float radius, float deadZone)
        {
            if (radius <= 0f) return Vector2.zero;
            Vector2 raw = Vector2.ClampMagnitude(offset / radius, 1f);
            float magnitude = raw.magnitude;
            float dz = Mathf.Clamp(deadZone, 0f, 0.9f);
            if (magnitude <= dz) return Vector2.zero;
            float remapped = (magnitude - dz) / (1f - dz);
            float eased = remapped * (0.65f + 0.35f * remapped); // 0.65x near 0, 1x at 1
            return raw / magnitude * Mathf.Clamp01(eased);
        }

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
            Vector2 local;
            if (background != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    background, eventData.position, eventData.pressEventCamera, out local))
            {
                origin = Vector2.ClampMagnitude(local, radius * 0.6f);
            }
            else origin = Vector2.zero;
            if (PlayerController.ExternalInputLocked) { ResetStick(); return; }
            pressed = true;
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null) return;
            // A lock (dialogue, catch) ends this touch: the thumb must lift and
            // press again, so a held thumb never resumes movement on wake (R3).
            if (PlayerController.ExternalInputLocked) { ResetStick(); return; }
            if (!pressed) return;

            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    background, eventData.position, eventData.pressEventCamera, out local))
            {
                return;
            }

            Vector2 offset = local - origin;
            Vector2 direction = Shape(offset, radius, deadZone);
            current = direction;
            handleTarget = Vector2.ClampMagnitude(origin + Vector2.ClampMagnitude(offset, radius), radius);
            if (player != null) player.Move(direction);
        }

        void Update()
        {
            // The rig can be rebuilt (respawn, scene reload): re-resolve a lost player
            // so the stick never keeps steering a destroyed controller.
            if (player == null) ResolvePlayer();
            // Visual only: the handle eases to the thumb, the move vector does not wait for it.
            // A held thumb keeps steering even when no drag event arrives (still thumb,
            // or the dialogue lock cleared the vector and then released).
            if (pressed && PlayerController.ExternalInputLocked) ResetStick();
            if (pressed && player != null) player.Move(current);
            if (handle == null) return;
            handle.localPosition = Vector2.Lerp(handle.localPosition, handleTarget,
                1f - Mathf.Exp(-handleFollow * Time.unscaledDeltaTime));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ResetStick();
        }

        void ResetStick()
        {
            pressed = false;
            current = Vector2.zero;
            origin = Vector2.zero;
            handleTarget = Vector2.zero;
            if (handle != null) handle.localPosition = Vector2.zero;
            if (player != null) player.Move(Vector2.zero);
        }
    }
}
