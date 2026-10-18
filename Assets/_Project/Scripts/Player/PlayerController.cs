using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Player
{
    // First-person movement for Nour: walk, sprint and crouch. No jumping, no combat.
    // Desktop input is read with the classic Input class; the touch layer
    // (TouchInput / TouchJoystick / TouchButton) drives the same Move/Look methods,
    // so there is only one movement code path to maintain.
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Wiring")]
        public GameConfig config;
        public Transform cameraPivot;
        public Camera playerCamera;

        CharacterController controller;

        // Movement state
        Vector2 keyboardMove;
        Vector2 touchMove;
        bool sprintHeld;
        bool sprintFromTouch;
        bool crouchHeldByKey;
        bool crouchToggle;
        float pitch;
        float verticalVelocity;

        // CharacterController height and matching eye height.
        const float StandHeight = 1.8f;
        const float CrouchHeight = 1.2f;
        const float StandEyeHeight = 1.6f;
        const float CrouchEyeHeight = 1.0f;

        public bool IsCrouching => crouchHeldByKey || crouchToggle;
        public bool HasMoveInput => keyboardMove.sqrMagnitude > 0.01f || touchMove.sqrMagnitude > 0.01f;
        public bool IsSprinting => (sprintHeld || sprintFromTouch) && !IsCrouching && HasMoveInput;
        public Camera PlayerCamera => playerCamera != null ? playerCamera : cameraPivot != null ? cameraPivot.GetComponentInChildren<Camera>() : null;

        float WalkSpeed => config != null ? config.walkSpeed : 3f;
        float SprintSpeed => config != null ? config.sprintSpeed : 5f;
        float CrouchSpeed => config != null ? config.crouchSpeed : 1.5f;
        float Sensitivity => config != null ? config.mouseSensitivity : 2f;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (config == null && GameManager.Instance != null) config = GameManager.Instance.config;
        }

        void Start()
        {
            // Desktop keeps the mouse locked to the window while playing.
            if (!Application.isMobilePlatform)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        void Update()
        {
            ReadKeyboardAndMouse();
            UpdateCrouch();
            UpdateMovement();
        }

        // ------------------------- Desktop input -------------------------
        void ReadKeyboardAndMouse()
        {
            float x = 0f;
            float z = 0f;
            if (Input.GetKey(KeyCode.A)) x -= 1f;
            if (Input.GetKey(KeyCode.D)) x += 1f;
            if (Input.GetKey(KeyCode.S)) z -= 1f;
            if (Input.GetKey(KeyCode.W)) z += 1f;
            Vector2 held = new Vector2(x, z);
            keyboardMove = held.sqrMagnitude > 1f ? held.normalized : held;

            sprintHeld = Input.GetKey(KeyCode.LeftShift);
            crouchHeldByKey = Input.GetKey(KeyCode.LeftControl);

            // The mouse turns the character and tilts the camera. Touch devices
            // skip this because TouchInput calls Look() with drag deltas instead.
            if (!Application.isMobilePlatform && (Cursor.lockState == CursorLockMode.Locked || Cursor.visible == false))
            {
                Look(new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")));
            }
        }

        // ------------------------- Shared look -------------------------
        // delta is in mouse units. TouchInput passes scaled finger deltas.
        public void Look(Vector2 delta)
        {
            transform.Rotate(Vector3.up, delta.x * Sensitivity, Space.Self);

            // Pitch is clamped so the camera cannot flip over.
            pitch = Mathf.Clamp(pitch - delta.y * Sensitivity, -80f, 80f);
            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        // ------------------------- Crouch -------------------------
        // Holding Ctrl crouches on desktop; the touch button toggles it.
        // The controller height and the camera eye height shrink smoothly.
        void UpdateCrouch()
        {
            float targetHeight = IsCrouching ? CrouchHeight : StandHeight;
            float targetEye = IsCrouching ? CrouchEyeHeight : StandEyeHeight;

            controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * 8f);
            controller.center = new Vector3(0f, controller.height * 0.5f, 0f);

            if (cameraPivot != null)
            {
                Vector3 position = cameraPivot.localPosition;
                position.y = Mathf.Lerp(position.y, targetEye, Time.deltaTime * 8f);
                cameraPivot.localPosition = position;
            }
        }

        // ------------------------- Movement -------------------------
        // WASD moves relative to where the camera faces. Gravity keeps Nour grounded.
        void UpdateMovement()
        {
            Vector2 input = keyboardMove.sqrMagnitude > 0.01f ? keyboardMove : touchMove;

            float speed = IsCrouching ? CrouchSpeed : IsSprinting ? SprintSpeed : WalkSpeed;
            Vector3 direction = transform.right * input.x + transform.forward * input.y;

            if (controller.isGrounded)
            {
                verticalVelocity = -2f;
            }
            else
            {
                verticalVelocity += Physics.gravity.y * Time.deltaTime;
            }

            Vector3 velocity = direction * speed + Vector3.up * verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }

        // ---------------------- Touch layer API ----------------------
        // Called by TouchJoystick every frame while held.
        public void Move(Vector2 input)
        {
            touchMove = Vector2.ClampMagnitude(input, 1f);
        }

        // Called by TouchButton (SprintHold) while pressed.
        public void SetSprint(bool held)
        {
            sprintFromTouch = held;
        }

        // Called by TouchButton (CrouchToggle).
        public void ToggleCrouch()
        {
            crouchToggle = !crouchToggle;
        }

        public void SetCrouch(bool toggled)
        {
            crouchToggle = toggled;
        }
    }
}
