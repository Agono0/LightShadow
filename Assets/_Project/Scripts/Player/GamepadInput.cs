using LightShadow.Core;
using LightShadow.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LightShadow.Player
{
    // Gamepad layer (premium pass, decision D7). Polls the Input System's
    // Gamepad every frame and feeds the SAME methods the touch layer already
    // uses - PlayerController.Move/Look/SetSprint/ToggleCrouch - so there is
    // still exactly one movement code path. Classic keyboard/mouse input is
    // untouched and keeps working at the same time (Active Input Handling = Both).
    //
    // Mapping: left stick = move, right stick = look, A/cross = interact,
    // X/square = flashlight, LB/RB = sprint (either), stick click = crouch toggle,
    // Start = pause (the same PauseMenu.Toggle() the touch pause button uses).
    //
    // Allocation: zero per frame. Sticks are read with ReadValue() into reused
    // fields; no LINQ, no closures, no per-frame allocations.
    public class GamepadInput : MonoBehaviour
    {
        [Tooltip("Look sensitivity multiplier for the right stick, in degrees per second at full deflection.")]
        public float lookSpeed = 160f;

        [Tooltip("Dead zone applied to both sticks (raw sticks are never perfectly centred).")]
        public float stickDeadZone = 0.15f;

        PlayerController player;
        FlashlightBattery flashlight;

        // Reused every frame; no struct churn beyond what the API already returns.
        Vector2 leftStick;
        Vector2 rightStick;
        bool sprintWasHeld;

        // Attaches (or reuses) the polling layer on the player rig. Called from
        // PlayerController.Awake, so a scene never needs explicit wiring.
        public static GamepadInput EnsureOn(GameObject playerRoot)
        {
            if (playerRoot == null) return null;
            GamepadInput existing = playerRoot.GetComponent<GamepadInput>();
            return existing != null ? existing : playerRoot.AddComponent<GamepadInput>();
        }

        void Awake()
        {
            player = GetComponent<PlayerController>();
            if (flashlight == null) flashlight = GetComponent<FlashlightBattery>();
        }

        void Start()
        {
            // GameManager may finish wiring after this Awake; resolve late too.
            if (player == null) player = GetComponent<PlayerController>();
            if (flashlight == null) flashlight = GetComponent<FlashlightBattery>();
        }

        void Update()
        {
            Gamepad pad = Gamepad.current;
            if (pad == null) return;

            // ---- Pause: always available, even during dialogue (matches Esc).
            if (pad.startButton.wasPressedThisFrame && PauseMenu.Instance != null)
            {
                PauseMenu.Instance.Toggle();
            }

            // ---- Locked (dialogue): everything below is frozen.
            if (PlayerController.ExternalInputLocked)
            {
                if (sprintWasHeld && player != null) player.SetSprint(false);
                sprintWasHeld = false;
                return;
            }

            // ---- Look (right stick): the same Look() path touch drags use.
            if (player != null)
            {
                rightStick = pad.rightStick.ReadValue();
                rightStick = ApplyDeadZone(rightStick);
                if (rightStick.sqrMagnitude > 0.0001f)
                {
                    player.Look(rightStick * (lookSpeed * Time.deltaTime));
                }

                // ---- Move (left stick): the touch joystick path.
                leftStick = pad.leftStick.ReadValue();
                leftStick = ApplyDeadZone(leftStick);
                player.Move(leftStick);

                // ---- Sprint: either shoulder, hold to run.
                bool sprint = pad.leftShoulder.isPressed || pad.rightShoulder.isPressed;
                if (sprint != sprintWasHeld)
                {
                    sprintWasHeld = sprint;
                    player.SetSprint(sprint);
                }

                // ---- Crouch toggle on stick click (same as the touch button).
                if (pad.leftStickButton.wasPressedThisFrame) player.ToggleCrouch();
            }

            // ---- Interact (A/cross): same call the touch Interact button makes.
            if (pad.buttonSouth.wasPressedThisFrame && InteractionRaycaster.Instance != null)
            {
                InteractionRaycaster.Instance.PressInteract();
            }

            // ---- Flashlight (X/square): same Toggle() as the HUD button.
            if (pad.buttonWest.wasPressedThisFrame)
            {
                if (flashlight == null && player != null)
                {
                    flashlight = player.GetComponent<FlashlightBattery>();
                }
                if (flashlight != null) flashlight.Toggle();
            }
        }

        // Radial dead zone: rescales the usable range to 0-1 after the cut.
        Vector2 ApplyDeadZone(Vector2 stick)
        {
            float magnitude = stick.magnitude;
            if (magnitude <= stickDeadZone) return Vector2.zero;
            float rescaled = (magnitude - stickDeadZone) / (1f - stickDeadZone);
            return stick * (rescaled / magnitude);
        }
    }
}
