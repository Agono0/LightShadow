using UnityEngine;

namespace LightShadow.Platform
{
    // P3 haptics: light/heavy wrappers around the only engine-level Android
    // vibration without a native plugin - Handheld.Vibrate() (~0.5 s pulse).
    // Both intensities use the same primitive today; the split exists so a
    // native plugin can upgrade Heavy() later without touching call sites.
    // IMPORTANT: Handheld exists only in the mobile build profiles — the call
    // is compiled out everywhere else via defines (a runtime isMobilePlatform
    // check alone cannot save compilation on Windows Standalone).
    public static class HapticsService
    {
        // Player opt-out (audit 7.2). Default on; stored so it survives restarts.
        public const string PrefKey = "ls_haptics_enabled";

        public static bool Enabled
        {
            get { return PlayerPrefs.GetInt(PrefKey, 1) != 0; }
            set { PlayerPrefs.SetInt(PrefKey, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        // Light: correct answers, fragment pickups.
        public static void Light()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            if (Enabled) Handheld.Vibrate();
#endif
        }

        // Heavy: being caught.
        public static void Heavy()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            if (Enabled) Handheld.Vibrate();
#endif
        }
    }
}
