using LightShadow.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace LightShadow.Player
{
    // Doc 4.2: the hand-crank flashlight has a physical battery (fuel), while the
    // Bright Idea Meter amplifies it. F toggles the light; battery pickups call AddBattery().
    // This script also publishes PlayerLightLevel, the single number the Shadow reads
    // to decide how visible Nour is (0 = pitch dark, 1 = bright beam).
    //
    // P3 banish (plan 4.6): the beam has a FOCUSED mode - hold right mouse
    // (desktop), gamepad Y, or the LIGHT touch button - which drains the
    // battery focusBatteryDrainMultiplier times faster while held. EnemyBehaviour
    // reads IsFocused for the hold-2s banish.
    public class FlashlightBattery : MonoBehaviour
    {
        [Header("Wiring")]
        public GameConfig config;
        public BrightIdeaMeter meter;
        public Light spotLight;
        public AudioClip toggleClip;
        public FlashlightCrank crank;   // shares the desktop F key with Toggle

        [Header("Events")]
        public UnityEvent<float> OnBatteryChanged = new UnityEvent<float>();
        public UnityEvent OnBatteryEmpty = new UnityEvent();

        // How lit up Nour currently is. Read by EnemyBehaviour every frame.
        public static float PlayerLightLevel;

        float battery = 1f;
        bool switchedOn;
        float flashMultiplier = 1f;
        float lastBatteryNotified = 1f;

        // Focus sources: the touch button holds SetFocus; desktop mouse and
        // gamepad are polled. The combined state needs the beam switched on.
        bool touchFocus;
        bool desktopFocusHeld;
        bool padFocusHeld;

        // Desktop F is double-bound (P3 crank): tap = toggle, hold + circle =
        // crank. The toggle waits for the release and is skipped when the crank
        // consumed the press by actually turning, so starting a crank no longer
        // switches the beam mid-gesture.
        bool fTogglePending;
        bool fPressCranked;

        public bool IsOn => switchedOn && battery > 0f;
        public bool IsFocused { get; private set; }
        public float Battery01 => battery;
        public float FlashMultiplier => flashMultiplier;

        // Touch layer API: the LIGHT button held = focused beam.
        public void SetFocus(bool value)
        {
            touchFocus = value;
        }

        void Awake()
        {
            if (config == null && GameManager.Instance != null) config = GameManager.Instance.config;
            if (toggleClip == null) toggleClip = ProceduralSfx.Click;
            if (crank == null) crank = GetComponent<FlashlightCrank>();
            battery = 1f;
        }

        void Start()
        {
            ApplyState();
            OnBatteryChanged.Invoke(battery);
        }

        void OnDisable()
        {
            // Never leave a stale light level behind for the Shadow to read.
            PlayerLightLevel = 0f;
            if (spotLight != null) spotLight.enabled = false;
        }

        void OnDestroy()
        {
            PlayerLightLevel = 0f;
        }

        void Update()
        {
            // Desktop toggle key. On phones the HUD button calls Toggle(); on
            // gamepad the GamepadInput layer calls it. Locked during dialogue.
            // The press is decided on release: toggled unless the crank turned
            // during the hold (FlashlightCrank holds F for its gesture).
            if (!PlayerController.ExternalInputLocked)
            {
                if (Input.GetKeyDown(KeyCode.F) && !LightShadow.Gameplay.ThrowableProp.OwnsFKey)
                {
                    fTogglePending = true;
                    fPressCranked = false;
                }
                if (fTogglePending && crank != null && crank.IsCranking) fPressCranked = true;
                if (Input.GetKeyUp(KeyCode.F))
                {
                    if (fTogglePending && !fPressCranked) Toggle();
                    fTogglePending = false;
                }
            }
            else
            {
                fTogglePending = false;
            }

            // Focus input: right mouse (desktop), gamepad Y, or the held LIGHT
            // touch button. Focus only counts while the beam is actually on.
            desktopFocusHeld = !PlayerController.ExternalInputLocked
                && !Application.isMobilePlatform && Input.GetMouseButton(1);
            Gamepad pad = Gamepad.current;
            padFocusHeld = !PlayerController.ExternalInputLocked
                && pad != null && pad.buttonNorth.isPressed;
            IsFocused = IsOn && (touchFocus || desktopFocusHeld || padFocusHeld);

            if (IsOn)
            {
                // Config must never gate dying (user report: the light stayed
                // on at 0): fall back to sane defaults when it is unwired.
                float focusMult = config != null ? config.focusBatteryDrainMultiplier : 4f;
                float batterySeconds = config != null ? config.batterySeconds : 240f;
                // Plan 4.6: the focused beam drains 4x (focusBatteryDrainMultiplier).
                float drainMultiplier = IsFocused ? Mathf.Max(1f, focusMult) : 1f;
                battery -= Time.deltaTime * drainMultiplier / Mathf.Max(1f, batterySeconds);
                if (battery <= 0f)
                {
                    battery = 0f;
                    switchedOn = false;
                    IsFocused = false;
                    OnBatteryEmpty.Invoke();
                }
                // Repaint the HUD bar only when the fraction actually moved a
                // visible step: skips a UnityEvent invoke on most frames.
                if (Mathf.Abs(battery - lastBatteryNotified) > 0.005f || battery <= 0f)
                {
                    lastBatteryNotified = battery;
                    OnBatteryChanged.Invoke(battery);
                }
            }

            // Publish visible light level: only a working, switched-on beam counts.
            PlayerLightLevel = IsOn && meter != null ? meter.Normalized : 0f;
            ApplyState();
        }

        public void Toggle()
        {
            if (battery <= 0f) return;
            switchedOn = !switchedOn;
            if (toggleClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(toggleClip);
            ApplyState();
        }

        // Battery pickups call this with 0.5 for a 50% charge.
        public void AddBattery(float fraction)
        {
            battery = Mathf.Clamp01(battery + fraction);
            lastBatteryNotified = battery;
            OnBatteryChanged.Invoke(battery);
            ApplyState();
        }

        // Save restore: jump the battery and switch state to the saved values
        // without playing the toggle sound (premium-pass SaveSystem).
        public void RestoreBattery(float fraction, bool on)
        {
            battery = Mathf.Clamp01(fraction);
            switchedOn = on && battery > 0f;
            lastBatteryNotified = battery;
            OnBatteryChanged.Invoke(battery);
            ApplyState();
        }

        // Doc 4.1: a correct answer briefly flares the light. The meter drives this.
        public void Flash(float seconds)
        {
            StopAllCoroutines();
            StartCoroutine(FlashRoutine(seconds));
        }

        System.Collections.IEnumerator FlashRoutine(float seconds)
        {
            flashMultiplier = 1.8f;
            if (meter != null) meter.ApplyToLight();
            yield return new WaitForSeconds(seconds);
            flashMultiplier = 1f;
            if (meter != null) meter.ApplyToLight();
        }

        void ApplyState()
        {
            if (spotLight != null) spotLight.enabled = IsOn;
        }
    }
}
