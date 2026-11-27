using LightShadow.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace LightShadow.Player
{
    // P3 crank minigame (plan 4.6): the emergency hand-crank. Hold the crank
    // input and TURN IN CIRCLES - a full revolution of the gesture drives the
    // dynamo. Every ratchet tick adds charge (capped at crankMaxCharge), plays
    // the ratchet and emits crankNoiseRange: cranking is loud, never calming.
    //
    // Charge accrues per second while the gesture is ACTIVE (any motion above
    // the dead zone within the reset window); the angle itself is not measured
    // (audit 9: the old accumulator was decorative and was removed).
    // Input sources, all feeding the same gesture-activity clock:
    //   - touch: hold the CRANK button (TouchButton.Kind.CrankHold) and swipe
    //     circles with a second finger anywhere off the UI;
    //   - desktop: hold F and move the mouse in circles (cursor must be locked,
    //     i.e. actively looking around - same condition PlayerController uses);
    //   - gamepad: hold RT and rotate the left stick.
    //
    // The gesture activity clock resets after crankGestureResetSeconds without
    // motion, so pausing stops the charge.
    public class FlashlightCrank : MonoBehaviour
    {
        [Header("Wiring")]
        public GameConfig config;
        public FlashlightBattery battery;
        public NoiseEmitter noise;

        // Touch button hold state (TouchButton.Kind.CrankHold calls SetHeld).
        bool touchHeld;
        bool padHeld;

        // Gesture state: one accumulator for mouse deltas, touch deltas and the
        // left stick alike (all are "angle between successive vectors").
        bool hasMotion;
        float idleTime;
        float ratchetClock;

        public bool IsHeld => touchHeld || KeyboardHeld || padHeld;
        public bool IsCranking => IsHeld && idleTime < GestureResetSeconds;

        bool KeyboardHeld => !Application.isMobilePlatform && Input.GetKey(KeyCode.F)
            && !LightShadow.Gameplay.ThrowableProp.OwnsFKey;

        // Config shortcuts with safe fallbacks (same pattern as EnemyBehaviour).
        float ChargePerSecond => config != null ? config.crankChargePerSecond : 0.02f;
        float MaxChargeFraction => (config != null ? config.crankMaxCharge : 15f) / 100f;
        float RatchetSeconds => config != null ? config.crankRatchetSeconds : 0.45f;
        float GestureResetSeconds => config != null ? config.crankGestureResetSeconds : 0.35f;
        float GestureDeadZone => config != null ? config.crankGestureDeadZone : 4f;

        void Awake()
        {
            if (config == null && GameManager.Instance != null) config = GameManager.Instance.config;
            ResolveReferences();
        }

        void Start()
        {
            // GameManager may finish wiring after this Awake; resolve late too.
            if (config == null && GameManager.Instance != null) config = GameManager.Instance.config;
            ResolveReferences();
        }

        void ResolveReferences()
        {
            if (battery == null) battery = GetComponent<FlashlightBattery>();
            if (noise == null) noise = GetComponent<NoiseEmitter>();
        }

        // Touch layer API: the CRANK button holds the crank down.
        public void SetHeld(bool value)
        {
            touchHeld = value;
        }

        void Update()
        {
            ResolveReferences();

            // The input lock (dialogue, hiding, quiz) freezes the crank exactly
            // like every other button; paused time (timescale 0) stops it too.
            bool activeHeld = !PlayerController.ExternalInputLocked && Time.timeScale > 0f && IsHeld;

            padHeld = Gamepad.current != null && Gamepad.current.rightTrigger.isPressed;

            if (!activeHeld)
            {
                // Zero the ratchet clock too: re-engaging the crank must not
                // fire an instant ratchet - the first tick needs RatchetSeconds
                // of actual turning, same as a fresh hold.
                ResetGesture();
                ratchetClock = 0f;
                return;
            }

            // ---- collect motion while held ----
            // Idle time grows while held without motion; FeedMotion resets it,
            // so stopping mid-revolution for a moment drops the banked angle.
            idleTime += Time.deltaTime;
            if (!Application.isMobilePlatform && Cursor.lockState == CursorLockMode.Locked)
            {
                FeedMotion(new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")));
            }

            Gamepad pad = Gamepad.current;
            if (pad != null) FeedMotion(pad.leftStick.ReadValue());

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                EventSystem system = EventSystem.current;
                if (system != null && system.IsPointerOverGameObject(touch.fingerId)) continue;
                FeedMotion(touch.deltaPosition);
            }

            // ---- turning: charge + ratchet ----
            bool turning = idleTime < GestureResetSeconds;
            if (turning)
            {
                ratchetClock += Time.deltaTime;
                if (ratchetClock >= RatchetSeconds)
                {
                    ratchetClock = 0f;
                    RatchetTick();
                }

                if (battery != null && battery.Battery01 < MaxChargeFraction)
                {
                    float gain = ChargePerSecond * Time.deltaTime;
                    // Cranking never charges past the emergency cap.
                    gain = Mathf.Min(gain, MaxChargeFraction - battery.Battery01);
                    if (gain > 0f) battery.AddBattery(gain);
                }
            }
            else
            {
                ResetGesture();
            }
        }

        // angle between the previous motion vector and this one, accumulated
        // while the crank is held; idle time resets the gesture if the player
        // stops moving.
        void FeedMotion(Vector2 motion)
        {
            if (motion.sqrMagnitude < GestureDeadZone * GestureDeadZone) return;

            hasMotion = true;
            idleTime = 0f;
        }

        void ResetGesture()
        {
            hasMotion = false;
            idleTime = GestureResetSeconds;
        }

        // One ratchet: click + a puff of noise. Charge happens continuously
        // while turning; the tick is the audible (and hearable) rhythm.
        void RatchetTick()
        {
            if (AudioManager.Instance != null)
            {
                LightShadow.Audio.Sfx.Play("SFX_Crank", ProceduralSfx.Crank);
            }
            if (noise != null) noise.Emit(config != null ? config.crankNoiseRange : 12f);
        }
    }
}
