using System;

namespace LightShadow.Contrib.Abdelraouf.ProximityText
{
    // Pure gating math for world-space proximity text: distance gate with
    // hysteresis, view-angle gate, and a smoothed 0-1 alpha. No UnityEngine
    // references, so the EditMode tests drive it without a scene.
    public static class ProximityTextMath
    {
        // Cosine of the cone half-angle, clamped to 0-180 degrees. The caller
        // compares dot(viewerForward, directionToText) against this value.
        public static float MinCosForDegrees(float maxAngleDegrees)
        {
            float clamped = Math.Max(0f, Math.Min(180f, maxAngleDegrees));
            return (float)Math.Cos(clamped * Math.PI / 180.0);
        }

        // Visibility with hysteresis: hidden text appears inside showRadius,
        // visible text stays until hideRadius (hideRadius >= showRadius). The
        // angle gate applies in both directions so leaving the view cone hides
        // the text even while still close.
        public static bool Visible(bool wasVisible, float distance, float cosAngle, float showRadius, float hideRadius, float minCosAngle)
        {
            float show = Math.Max(0f, showRadius);
            float hide = Math.Max(show, hideRadius);
            bool inAngle = cosAngle >= minCosAngle;
            if (wasVisible) return distance <= hide && inAngle;
            return distance <= show && inAngle;
        }

        // Moves alpha toward 1 (visible) or 0 (hidden) at fadePerSecond,
        // clamped to 0-1. Uses unscaled time so fades keep running in pause.
        public static float StepAlpha(float current, bool visible, float unscaledDt, float fadePerSecond)
        {
            float target = visible ? 1f : 0f;
            float step = Math.Max(0.0001f, fadePerSecond) * Math.Max(0f, unscaledDt);
            if (current < target) return Math.Min(target, current + step);
            return Math.Max(target, current - step);
        }
    }
}
