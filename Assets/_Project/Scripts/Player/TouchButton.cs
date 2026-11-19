using LightShadow.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LightShadow.Player
{
    // Generic on-screen button for phones. Pick what it does from the Kind list;
    // no code changes needed to re-purpose a button in the mobile HUD.
    public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public enum Kind { SprintHold, CrouchToggle, Flashlight, Interact, Pause, Objectives, CrankHold, BreathHold }

        [Header("Wiring")]
        public Kind kind;
        public PlayerController player;
        public FlashlightBattery flashlight;
        public FlashlightCrank crank;
        public HidingSystem hiding;

        // Flashlight hold-vs-tap (P3 banish): a press held longer than
        // GameConfig.flashlightTapSeconds focuses the beam; a shorter tap
        // toggles it. UGUI captures the pointer that pressed, so only one
        // pointer ever drives this at a time.
        bool flashDown;
        float flashDownTime;
        bool flashFocusStarted;

        void Awake()
        {
            ResolveReferences();
        }

        void Start()
        {
            ResolveReferences();
        }

        void Update()
        {
            if (kind != Kind.Flashlight || !flashDown) return;
            if (flashFocusStarted) return;
            if (flashlight == null || !flashlight.IsOn) return;
            if (Time.unscaledTime - flashDownTime >= TapSeconds)
            {
                flashFocusStarted = true;
                flashlight.SetFocus(true);
            }
        }

        float TapSeconds
        {
            get
            {
                Core.GameConfig config = Core.GameManager.Instance != null ? Core.GameManager.Instance.config : null;
                return config != null ? config.flashlightTapSeconds : 0.25f;
            }
        }

        void ResolveReferences()
        {
            if (player == null && Core.GameManager.Instance != null && Core.GameManager.Instance.player != null)
            {
                player = Core.GameManager.Instance.player.GetComponent<PlayerController>();
            }
            if (flashlight == null && player != null)
            {
                flashlight = player.GetComponent<FlashlightBattery>();
            }
            if (crank == null && player != null)
            {
                crank = player.GetComponent<FlashlightCrank>();
            }
            if (hiding == null && player != null)
            {
                hiding = player.GetComponent<HidingSystem>();
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
                    // Hold-to-focus / tap-to-toggle (P3 banish). The tap fires
                    // on release so the hold can win first.
                    flashDown = true;
                    flashDownTime = Time.unscaledTime;
                    flashFocusStarted = false;
                    break;
                case Kind.Interact:
                    // While hidden the interact button means "leave the spot"
                    // (the ray cannot see the spot from inside the prop).
                    if (HidingSystem.IsHidden) { HidingSystem.ExitCurrent(); break; }
                    if (InteractionRaycaster.Instance != null) InteractionRaycaster.Instance.PressInteract();
                    break;
                case Kind.Pause:
                    if (PauseMenu.Instance != null) PauseMenu.Instance.Toggle();
                    break;
                case Kind.Objectives:
                    if (HUDController.Instance != null) HUDController.Instance.ToggleObjectives();
                    break;
                case Kind.CrankHold:
                    if (crank != null) crank.SetHeld(true);
                    break;
                case Kind.BreathHold:
                    if (hiding != null) hiding.SetBreathHold(true);
                    break;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (kind == Kind.SprintHold && player != null) player.SetSprint(false);
            if (kind == Kind.CrankHold && crank != null) crank.SetHeld(false);
            if (kind == Kind.BreathHold && hiding != null) hiding.SetBreathHold(false);

            if (kind == Kind.Flashlight && flashlight != null && flashDown)
            {
                if (flashFocusStarted) flashlight.SetFocus(false);
                else flashlight.Toggle();
                flashDown = false;
                flashFocusStarted = false;
            }
        }
    }
}
