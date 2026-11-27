using LightShadow.Core;
using LightShadow.Player;
using LightShadow.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LightShadow.Contrib.Abdelraouf.InventoryScreen
{
    // Input polling for the inventory screen. Approved mapping (2026-11-27):
    // I key and gamepad Y (buttonNorth) toggle the screen. Inside it:
    // arrows / D-pad / left stick select, Enter / gamepad A use,
    // I / gamepad B close. Esc is NOT consumed here: the owner's side adds the
    // Esc/Start close-first guard in PauseMenu, which calls Close() first.
    //
    // This component never touches GamepadInput; it polls the same devices
    // itself. While the screen holds the external input lock, GamepadInput
    // returns early (its locked branch), so A/X/dpad are not double-handled.
    public class InventoryScreenInput : MonoBehaviour
    {
        [Tooltip("Seconds before a held direction repeats, then the repeat rate.")]
        public float repeatDelay = 0.35f;
        public float repeatRate = 0.12f;
        public float stickThreshold = 0.5f;

        float repeatClock;
        int heldColumn;
        int heldRow;

        void Awake()
        {
            // The input poller lives wherever the screen lives.
            if (GetComponent<InventoryScreenUI>() == null)
            {
                gameObject.AddComponent<InventoryScreenUI>();
            }
        }

        void Update()
        {
            Gamepad pad = Gamepad.current;

            bool togglePressed = Input.GetKeyDown(KeyCode.I)
                || (pad != null && pad.buttonNorth.wasPressedThisFrame);

            if (togglePressed)
            {
                if (InventoryScreenUI.IsOpen) InventoryScreenUI.Instance.Close();
                else if (CanOpenNow()) InventoryScreenUI.EnsureExists().Open();
                return;
            }

            if (!InventoryScreenUI.IsOpen) return;
            InventoryScreenUI ui = InventoryScreenUI.Instance;
            if (ui == null) return;

            // ---- keyboard ----
            if (Input.GetKeyDown(KeyCode.LeftArrow)) ui.MoveSelection(-1, 0);
            if (Input.GetKeyDown(KeyCode.RightArrow)) ui.MoveSelection(1, 0);
            if (Input.GetKeyDown(KeyCode.UpArrow)) ui.MoveSelection(0, -1);
            if (Input.GetKeyDown(KeyCode.DownArrow)) ui.MoveSelection(0, 1);
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) ui.UseSelected();

            if (pad == null) return;

            // ---- gamepad: D-pad edges ----
            int stepColumn = 0;
            int stepRow = 0;
            if (pad.dpad.left.wasPressedThisFrame) stepColumn = -1;
            else if (pad.dpad.right.wasPressedThisFrame) stepColumn = 1;
            if (pad.dpad.up.wasPressedThisFrame) stepRow = -1;
            else if (pad.dpad.down.wasPressedThisFrame) stepRow = 1;

            // ---- gamepad: left stick with repeat ----
            Vector2 stick = pad.leftStick.ReadValue();
            int stickColumn = stick.x > stickThreshold ? 1 : (stick.x < -stickThreshold ? -1 : 0);
            int stickRow = stick.y > stickThreshold ? -1 : (stick.y < -stickThreshold ? 1 : 0);

            if (stickColumn == 0 && stickRow == 0)
            {
                heldColumn = 0;
                heldRow = 0;
                repeatClock = 0f;
            }
            else if (stickColumn != heldColumn || stickRow != heldRow)
            {
                heldColumn = stickColumn;
                heldRow = stickRow;
                repeatClock = repeatDelay;
                stepColumn = stickColumn;
                stepRow = stickRow;
            }
            else
            {
                repeatClock -= Time.unscaledDeltaTime;
                if (repeatClock <= 0f)
                {
                    repeatClock = repeatRate;
                    stepColumn = stickColumn;
                    stepRow = stickRow;
                }
            }

            if (stepColumn != 0 || stepRow != 0)
            {
                // One axis per step, columns first, matching grid navigation.
                if (stepColumn != 0) ui.MoveSelection(stepColumn, 0);
                else ui.MoveSelection(0, stepRow);
            }

            if (pad.buttonSouth.wasPressedThisFrame) ui.UseSelected();
            if (pad.buttonEast.wasPressedThisFrame) ui.Close();
        }

        // Refuse to open while anything else owns input or an overlay is up.
        // QuizUIController has no static accessor and never takes the input
        // lock (by design the world keeps moving), so its open state is read
        // through GameManager.Instance.quizUI.
        static bool CanOpenNow()
        {
            if (PlayerController.ExternalInputLocked) return false;
            if (PauseMenu.IsOpen || InspectViewController.IsOpen) return false;
            GameManager gm = GameManager.Instance;
            if (gm == null) return false;
            if (gm.quizUI != null && gm.quizUI.IsOpen) return false;
            return true;
        }
    }
}
