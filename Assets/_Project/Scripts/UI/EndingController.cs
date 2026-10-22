using System.Collections;
using LightShadow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // 03_Ending: dawn outside the gate, the closing line, then the study recap.
    // The recap is the real ending: every question of the run, the chosen answer,
    // the right answer and the explanation - a printable study sheet.
    public class EndingController : MonoBehaviour
    {
        [Header("Wiring")]
        public Text dawnTitle;
        public Text dawnBody;
        public RecapScreen recap;
        public CanvasGroup dawnGroup;

        [Header("Dawn set")]
        public Transform sun;
        public Renderer sunRenderer;
        public Renderer skyRenderer;
        public Light dawnLight;
        public Camera endingCamera;

        [Header("Timing")]
        public float dawnSeconds = 8f;

        [Header("Night / dawn colours")]
        public Color skyNight = new Color(0.085f, 0.085f, 0.135f);
        public Color skyDawn = new Color(0.93f, 0.62f, 0.33f);
        public Color sunNight = new Color(0.80f, 0.26f, 0.12f) * 0.9f;
        public Color sunDawn = new Color(1f, 0.80f, 0.48f) * 3.4f;
        public Color cameraNight = new Color(0.05f, 0.05f, 0.08f);
        public Color cameraDawn = new Color(0.74f, 0.50f, 0.29f);
        public Color ambientNight = new Color(0.075f, 0.065f, 0.070f);
        public Color ambientDawn = new Color(0.38f, 0.28f, 0.20f);
        public Color fogNight = new Color(0.10f, 0.08f, 0.09f);
        public Color fogDawn = new Color(0.42f, 0.28f, 0.20f);

        public float sunStartHeight = 1.5f;
        public float sunEndHeight = 5.8f;
        public float lightEndIntensity = 1.5f;

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        MaterialPropertyBlock block;

        IEnumerator Start()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (dawnTitle != null)
            {
                dawnTitle.text = Localization.TrPresent("DAWN");
                dawnTitle.font = Localization.CurrentFont;
            }

            if (dawnBody != null)
            {
                // The string is the exact translation key, so Arabic is picked up too.
                dawnBody.text = Localization.TrPresent("The gate closes behind her. The sky above the school turns from black to grey to a thin, clean gold.\n\n"
                    + "She opens the memory box on the pavement and reads her mother's lesson plans one last time.");
                dawnBody.font = Localization.CurrentFont;
                dawnBody.alignment = Localization.Current == GameLanguage.Arabic
                    ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
            }

            if (dawnGroup != null) dawnGroup.alpha = 0f;
            ApplyDawn(0f);

            // The narrator closes the night once, before the recap appears.
            StoryTeller.Speak(Localization.Tr(
                "The gate closes behind Nour. She opens the memory box on the pavement and reads the lesson plans one last time."), true);

            // Everything is driven from one eased value so the sun, the sky, the
            // light, the fog and the text cannot drift out of step.
            float t = 0f;
            float duration = Mathf.Max(0.5f, dawnSeconds);
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / duration;
                float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
                ApplyDawn(eased);

                if (dawnGroup != null)
                {
                    // The words arrive while the sun is still almost down.
                    dawnGroup.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 2.4f));
                }

                yield return null;
            }

            ApplyDawn(1f);
            if (dawnGroup != null) dawnGroup.alpha = 1f;

            if (recap != null) recap.Show(RunResults.Answers);
        }

        void ApplyDawn(float k)
        {
            if (sun != null)
            {
                Vector3 position = sun.localPosition;
                position.y = Mathf.Lerp(sunStartHeight, sunEndHeight, k);
                sun.localPosition = position;
            }

            SetBaseColor(skyRenderer, Color.Lerp(skyNight, skyDawn, k));
            SetBaseColor(sunRenderer, Color.Lerp(sunNight, sunDawn, k));

            if (dawnLight != null) dawnLight.intensity = lightEndIntensity * k;
            if (endingCamera != null) endingCamera.backgroundColor = Color.Lerp(cameraNight, cameraDawn, k);

            RenderSettings.ambientLight = Color.Lerp(ambientNight, ambientDawn, k);
            RenderSettings.fogColor = Color.Lerp(fogNight, fogDawn, k);
        }

        void SetBaseColor(Renderer renderer, Color color)
        {
            if (renderer == null) return;

            if (block == null) block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor(BaseColorId, color);
            renderer.SetPropertyBlock(block);
        }
    }
}
