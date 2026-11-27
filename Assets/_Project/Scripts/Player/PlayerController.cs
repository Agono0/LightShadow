using LightShadow.Contrib.Abdelraouf.Overlay;
using LightShadow.Core;
using LightShadow.UI;
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
        // Input lock contract (premium pass): set true by the story/dialogue
        // system while a conversation owns the screen. Every input source
        // (keyboard, mouse, touch, gamepad) checks this before reading, so one
        // flag freezes Nour without touching gravity, the camera pivot or the
        // crouch/eye-height smoothing. Pause stays available by design.
        // Now a view of OverlayStack: locked while any overlay holds a token.
        // The setter is kept for unmigrated writers; it maps to one shared
        // legacy token and "false" frees only that token.
        public static bool ExternalInputLocked
        {
            get { return OverlayStack.AnyLocked; }
            set { OverlayStack.SetLegacyLock(value); }
        }

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
        public bool IsSprinting => (sprintHeld || sprintFromTouch) && !IsCrouching && HasMoveInput && !exhausted && stamina01 > 0f;

        // Stamina (user brief 2026-11-27): about five seconds of sprint, then
        // she is winded until roughly half recovered. Her breathing
        // (PlayerBreathing) is the tell - no bar by design.
        const float SprintSeconds = 5f;
        const float StaminaRegenPerSecond = 0.16f;  // fraction of a full bar
        const float ExhaustedRecoverAt = 0.45f;
        float stamina01 = 1f;
        bool exhausted;
        public float Stamina01 => stamina01;
        public bool IsExhausted => exhausted;
        public Camera PlayerCamera => playerCamera != null ? playerCamera : cameraPivot != null ? cameraPivot.GetComponentInChildren<Camera>() : null;

        float WalkSpeed => config != null ? config.walkSpeed : 3f;
        float SprintSpeed => config != null ? config.sprintSpeed : 5f;
        Vector3 planarVelocity;
        float CrouchSpeed => config != null ? config.crouchSpeed : 1.5f;
        float Sensitivity => (config != null ? config.mouseSensitivity : 2f) * GameSettings.Sensitivity;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (config == null && GameManager.Instance != null) config = GameManager.Instance.config;
            // Gamepad polling rides along on the same rig (decision D7: classic
            // Input stays the movement core; the Input System is a polling layer).
            GamepadInput.EnsureOn(gameObject);
        }

        void Start()
        {
            LockCursorForPlay();
        }

        void OnApplicationFocus(bool hasFocus)
        {
            // Alt-tab (or any focus loss) silently releases the cursor lock;
            // on focus regain it never comes back by itself, which read as
            // "the mouse randomly gets off the game". Re-assert while playing.
            if (hasFocus && !Application.isMobilePlatform)
            {
                LockCursorForPlay();
            }
        }

        void LockCursorForPlay()
        {
            // Overlays that legitimately free the cursor (pause, quiz, dialogue,
            // ending) re-lock on their own close paths; do not fight them.
            if (PauseMenu.IsOpen || OverlayStack.WantsFreeCursor || GameManager.Instance != null && GameManager.Instance.quizUI != null && GameManager.Instance.quizUI.IsOpen)
            {
                return;
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>Full pose/state reset for checkpoint respawn: the catch
        /// sequence (camera shake, cinematic shot, crouch) can leave the rig in
        /// a state that read as "reversed controls / broken camera". Zeroes
        /// everything the respawn does not restore by itself.</summary>
        public void RespawnReset()
        {
            pitch = 0f;
            crouchToggle = false;
            crouchHeldByKey = false;
            sprintHeld = false;
            sprintFromTouch = false;
            keyboardMove = Vector2.zero;
            touchMove = Vector2.zero;
            verticalVelocity = 0f;
            // Respawn clears every overlay: visible ones close themselves first.
            OverlayStack.ResetAll(true);

            // A hide interrupted by the catch must not survive the respawn:
            // stale IsHidden leaves the Shadow blind and E hops Nour back into
            // the old spot (no teleport here - the wake already moved her).
            HidingSystem.ResetState();

            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.identity;
            }
            if (playerCamera != null)
            {
                playerCamera.transform.localPosition = Vector3.zero;
                playerCamera.transform.localRotation = Quaternion.identity;
            }
            // CharacterController height follows the crouch smoothing on the
            // next Update once crouchToggle is cleared.

            if (!Application.isMobilePlatform)
            {
                LockCursorForPlay();
            }
        }

        void Update()
        {
            // The input lock (dialogue) freezes move/look but never gravity or
            // the crouch smoothing, so Nour keeps landing and keeps her pose.
            if (ExternalInputLocked)
            {
                keyboardMove = Vector2.zero;
                touchMove = Vector2.zero;
                sprintHeld = false;
                sprintFromTouch = false;
            }
            else
            {
                ReadKeyboardAndMouse();
            }
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
            // The lock is re-checked here so no input source (touch joystick
            // included) can slip a move vector past the dialogue freeze.
            Vector2 input = ExternalInputLocked
                ? Vector2.zero
                : (keyboardMove.sqrMagnitude > 0.01f ? keyboardMove : touchMove);

            float speed = IsCrouching ? CrouchSpeed : IsSprinting ? SprintSpeed : WalkSpeed;
            Vector3 direction = transform.right * input.x + transform.forward * input.y;

            // Stamina drain/regen.
            if (IsSprinting)
            {
                stamina01 -= Time.deltaTime / SprintSeconds;
                if (stamina01 <= 0f)
                {
                    stamina01 = 0f;
                    exhausted = true;
                }
            }
            else
            {
                stamina01 = Mathf.Clamp01(stamina01 + StaminaRegenPerSecond * Time.deltaTime);
                if (exhausted && stamina01 >= ExhaustedRecoverAt) exhausted = false;
            }

            if (controller.isGrounded)
            {
                verticalVelocity = -2f;
            }
            else
            {
                verticalVelocity += Physics.gravity.y * Time.deltaTime;
            }

            // Smoothed acceleration (same brief: improve the whole
            // controller): planar velocity eases toward the target instead of
            // snapping, so starts and stops stop feeling binary.
            Vector3 planarTarget = direction * speed;
            planarVelocity = Vector3.MoveTowards(planarVelocity, planarTarget,
                (planarTarget.sqrMagnitude > planarVelocity.sqrMagnitude ? 24f : 34f) * Time.deltaTime);
            Vector3 velocity = planarVelocity + Vector3.up * verticalVelocity;
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
