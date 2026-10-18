using LightShadow.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LightShadow.Player
{
    // Generic on-screen button for phones. Pick what it does from the Kind list;
    // no code changes needed to re-purpose a button in the mobile HUD.
    public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public enum Kind { SprintHold, CrouchToggle, Flashlight, Interact, Pause, Objectives }

        [Header("Wiring")]
        public Kind kind;
        public PlayerController player;
        public FlashlightBattery flashlight;

        void Awake()
        {
            if (player == null && Core.GameManager.Instance != null && Core.GameManager.Instance.player != null)
            {
                player = Core.GameManager.Instance.player.GetComponent<PlayerController>();
            }
            if (flashlight == null && player != null)
            {
                flashlight = player.GetComponent<FlashlightBattery>();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            switch (kind)
            {
                case Kind.SprintHold:
                    if (player != null) player.SetSprint(true);
                    break;
                case Kind.CrouchToggle:
                    if (player != null) player.ToggleCrouch();
                    break;
                case Kind.Flashlight:
                    if (flashlight != null) flashlight.Toggle();
                    break;
                case Kind.Interact:
                    if (InteractionRaycaster.Instance != null) InteractionRaycaster.Instance.PressInteract();
                    break;
                case Kind.Pause:
                    if (PauseMenu.Instance != null) PauseMenu.Instance.Toggle();
                    break;
                case Kind.Objectives:
                    if (HUDController.Instance != null) HUDController.Instance.ToggleObjectives();
                    break;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (kind == Kind.SprintHold && player != null) player.SetSprint(false);
        }
    }
}
