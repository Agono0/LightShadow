using UnityEngine;

namespace LightShadow.Platform
{
    // P3 haptics: light/heavy wrappers around the only engine-level Android
    // vibration without a native plugin - Handheld.Vibrate() (~0.5 s pulse).
    // Both intensities use the same primitive today; the split exists so a
    // native plugin can upgrade Heavy() later without touching call sites.
    // No-ops in the editor and on desktop.
    public static class HapticsService
    {
        // Light: correct answers, fragment pickups.
        public static void Light()
        {
#if UNITY_EDITOR
            return;
#else
            if (!Application.isMobilePlatform) return;
            Handheld.Vibrate();
#endif
        }

        // Heavy: being caught.
        public static void Heavy()
        {
#if UNITY_EDITOR
            return;
#else
            if (!Application.isMobilePlatform) return;
            Handheld.Vibrate();
#endif
        }
    }
}
