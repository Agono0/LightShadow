using LightShadow.Core;
using UnityEngine;
using UnityEngine.Events;

namespace LightShadow.Player
{
    // Doc 4.1: the Bright Idea Meter is Nour's knowledge gauge (0-100, starts at 60).
    // Correct answers add 25 and flare the light; wrong answers remove 10.
    // While the flashlight is on the meter drains slowly, so keeping it high means
    // answering well. The meter maps directly onto the spotlight range (4-12 m)
    // and intensity (1.5-3.5), exactly as Section 4.1 defines.
    public class BrightIdeaMeter : MonoBehaviour
    {
        [Header("Wiring")]
        public GameConfig config;
        public FlashlightBattery flashlight;
        public Light spotLight;
        public AudioClip correctClip;
        public AudioClip wrongClip;

        [Header("Events")]
        public UnityEvent<float> OnChanged = new UnityEvent<float>();

        float meter = 60f;
        float lastNotified = -1f;

        public float Meter => meter;
        public float Max => config != null ? config.meterMax : 100f;
        public float Normalized => Mathf.Clamp01(meter / Max);

        void Awake()
        {
            if (config == null && GameManager.Instance != null) config = GameManager.Instance.config;
            meter = config != null ? config.meterStart : 60f;
        }

        void Start()
        {
            Notify();
            ApplyToLight();
        }

        void Update()
        {
            // The quiet drain while the beam is on creates the resource tension.
            if (flashlight != null && flashlight.IsOn && config != null)
            {
                meter -= config.meterDrainPerSecond * Time.deltaTime;
                meter = Mathf.Clamp(meter, 0f, Max);
            }

            ApplyToLight();
            Notify();
        }

        // Called when the player answers a quiz correctly.
        public void AddCorrect()
        {
            float gain = config != null ? config.meterGainCorrect : 25f;
            SetMeter(meter + gain);

            // A correct answer spins the crank: sound plus a light flare.
            if (correctClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(correctClip);
            if (flashlight != null) flashlight.Flash(0.3f);
        }

        // Called on a wrong answer or a timeout.
        public void AddWrong()
        {
            float penalty = config != null ? config.meterPenaltyWrong : 10f;
            SetMeter(meter - penalty);
            if (wrongClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(wrongClip);
        }

        // Used by GameManager for the "caught" penalty (Doc 4.3).
        public void RemoveMeter(float amount)
        {
            SetMeter(meter - amount);
        }

        public void SetMeter(float value)
        {
            meter = Mathf.Clamp(value, 0f, Max);
            ApplyToLight();
            Notify();
        }

        // Strength = meter factor. The beam grows from a dim 4 m glow to a
        // bright 12 m cone as the meter climbs (Doc 4.1 / 4.2).
        public void ApplyToLight()
        {
            if (spotLight == null) return;
            float n = Normalized;
            float rangeMin = config != null ? config.lightRadiusMin : 4f;
            float rangeMax = config != null ? config.lightRadiusMax : 12f;
            float intensityMin = config != null ? config.lightIntensityMin : 1.5f;
            float intensityMax = config != null ? config.lightIntensityMax : 3.5f;

            spotLight.range = Mathf.Lerp(rangeMin, rangeMax, n);
            float boost = flashlight != null ? flashlight.FlashMultiplier : 1f;
            spotLight.intensity = Mathf.Lerp(intensityMin, intensityMax, n) * boost;
        }

        void Notify()
        {
            float n = Normalized;
            if (Mathf.Abs(n - lastNotified) < 0.001f) return;
            lastNotified = n;
            OnChanged.Invoke(n);
        }
    }
}
