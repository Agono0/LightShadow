using LightShadow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // Live FPS counter, top right of the HUD (player request after the first
    // premium build: "the game does not feel smooth"). Updates twice a second
    // with the averaged frame rate and the equivalent frame time in
    // milliseconds, colour-coded so trouble is readable at a glance:
    //   >= 90 FPS  warm amber (the D8 target band)
    //   >= 55 FPS  soft paper white (60 Hz territory)
    //   <  55 FPS  muted red (below the stable minimum)
    // Visibility follows GameSettings.ShowFps (settings card, "FPS counter"
    // row) and the counter never allocates per frame: the label is rebuilt
    // only on the twice-per-second tick.
    public class FpsCounter : MonoBehaviour
    {
        const float UpdateInterval = 0.5f;
        const int TargetFpsAmber = 90;   // at or above: the 120 Hz target band
        const int TargetFpsWhite = 55;   // at or above: stable 60 territory

        public Text label;

        int frames;
        float accumulated;
        bool subscribed;

        void OnEnable()
        {
            if (!subscribed)
            {
                GameSettings.Changed += RefreshVisibility;
                subscribed = true;
            }
            frames = 0;
            accumulated = 0f;
            RefreshVisibility();
        }

        void OnDisable()
        {
            if (subscribed)
            {
                GameSettings.Changed -= RefreshVisibility;
                subscribed = false;
            }
        }

        void Update()
        {
            frames++;
            accumulated += Time.unscaledDeltaTime;
            if (accumulated < UpdateInterval) return;

            float averageFps = frames / accumulated;
            float frameMilliseconds = accumulated / frames * 1000f;
            frames = 0;
            accumulated = 0f;

            if (label == null) return;
            label.text = Mathf.RoundToInt(averageFps) + " FPS  ·  " + frameMilliseconds.ToString("0.0") + " ms";
            label.color = ColorFor((int)averageFps);
        }

        void RefreshVisibility()
        {
            // Only the LABEL is toggled. Disabling this component instead would
            // fire OnDisable, unsubscribe from GameSettings.Changed, and leave
            // the counter permanently dark after the first off-toggle.
            if (label != null) label.gameObject.SetActive(GameSettings.ShowFps);
        }

        static Color ColorFor(int fps)
        {
            if (fps >= TargetFpsAmber) return new Color(1f, 0.87f, 0.62f);   // warm amber
            if (fps >= TargetFpsWhite) return new Color(0.94f, 0.93f, 0.88f); // paper white
            return new Color(0.92f, 0.45f, 0.38f);                            // muted red
        }
    }
}
