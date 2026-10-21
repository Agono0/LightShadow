using LightShadow.Core;
using UnityEngine;
using UnityEngine.Events;

namespace LightShadow.Player
{
    // Doc 4.2: the hand-crank flashlight has a physical battery (fuel), while the
    // Bright Idea Meter amplifies it. F toggles the light; battery pickups call AddBattery().
    // This script also publishes PlayerLightLevel, the single number the Shadow reads
    // to decide how visible Nour is (0 = pitch dark, 1 = bright beam).
    public class FlashlightBattery : MonoBehaviour
    {
        [Header("Wiring")]
        public GameConfig config;
        public BrightIdeaMeter meter;
        public Light spotLight;
        public AudioClip toggleClip;

        [Header("Events")]
        public UnityEvent<float> OnBatteryChanged = new UnityEvent<float>();
        public UnityEvent OnBatteryEmpty = new UnityEvent();

        // How lit up Nour currently is. Read by EnemyBehaviour every frame.
        public static float PlayerLightLevel;

        float battery = 1f;
        bool switchedOn;
        float flashMultiplier = 1f;

        public bool IsOn => switchedOn && battery > 0f;
        public float Battery01 => battery;
        public float FlashMultiplier => flashMultiplier;

        void Awake()
        {
            if (config == null && GameManager.Instance != null) config = GameManager.Instance.config;
            if (toggleClip == null) toggleClip = ProceduralSfx.Click;
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
            // Desktop toggle key. On phones the HUD button calls Toggle().
            if (Input.GetKeyDown(KeyCode.F)) Toggle();

            if (IsOn && config != null)
            {
                battery -= Time.deltaTime / Mathf.Max(1f, config.batterySeconds);
                if (battery <= 0f)
                {
                    battery = 0f;
                    switchedOn = false;
                    OnBatteryEmpty.Invoke();
                }
                OnBatteryChanged.Invoke(battery);
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
