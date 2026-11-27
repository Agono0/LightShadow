using System.Collections;
using LightShadow.Core;
using LightShadow.Story;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // 03_Ending: dawn outside the gate, the closing line, then the study recap.
    // The recap is the real ending: every question of the run, the chosen answer,
    // the right answer and the explanation - a printable study sheet.
    //
    // Premium pass (additive, D13): the StoryDirector picks one of four endings
    // (FullRecovery / PartialMemory / HollowEscape / Dawnless) from run state.
    // When it has decided, this screen shows that ending's title, body and dawn
    // tint before the recap; the original default-dawn behaviour stays as the
    // fallback when no director is present (scene played directly).
    public class EndingController : MonoBehaviour
    {
        [System.Serializable]
        public class EndingTint
        {
            public string endingId;
            [Tooltip("Applied while the dawn eases in, when this ending is chosen.")]
            public Color sky = new Color(0.93f, 0.62f, 0.33f);
            public Color sun = new Color(1f, 0.80f, 0.48f) * 3.4f;
            public Color camera = new Color(0.74f, 0.50f, 0.29f);
            public Color ambient = new Color(0.38f, 0.28f, 0.20f);
            public Color fog = new Color(0.42f, 0.28f, 0.20f);
        }

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

        [Header("Ending tints (D13; matched by id, first hit wins)")]
        public EndingTint[] endingTints =
        {
            new EndingTint { endingId = "FullRecovery",
                sky = new Color(0.97f, 0.66f, 0.34f), sun = new Color(1f, 0.82f, 0.5f) * 3.8f,
                camera = new Color(0.78f, 0.53f, 0.31f), ambient = new Color(0.42f, 0.31f, 0.22f),
                fog = new Color(0.46f, 0.31f, 0.22f) },
            new EndingTint { endingId = "PartialMemory",
                sky = new Color(0.80f, 0.66f, 0.47f), sun = new Color(1f, 0.78f, 0.5f) * 2.6f,
                camera = new Color(0.62f, 0.51f, 0.38f), ambient = new Color(0.34f, 0.28f, 0.22f),
                fog = new Color(0.38f, 0.32f, 0.26f) },
            new EndingTint { endingId = "HollowEscape",
                sky = new Color(0.58f, 0.62f, 0.70f), sun = new Color(0.9f, 0.72f, 0.55f) * 1.6f,
                camera = new Color(0.45f, 0.48f, 0.55f), ambient = new Color(0.26f, 0.27f, 0.31f),
                fog = new Color(0.30f, 0.32f, 0.37f) },
            new EndingTint { endingId = "Dawnless",
                sky = new Color(0.52f, 0.52f, 0.54f), sun = new Color(0.85f, 0.68f, 0.5f) * 0.9f,
                camera = new Color(0.40f, 0.40f, 0.42f), ambient = new Color(0.24f, 0.24f, 0.25f),
                fog = new Color(0.28f, 0.28f, 0.30f) },
        };

        [System.Serializable]
        public class EndingMusic
        {
            public string endingId;
            [Tooltip("One-shot cue played when the ending screen starts (null = silent until the Pixabay primaries land).")]
            public AudioClip clip;
        }

        [Header("Ending music (Phase 1; matched by id, first hit wins)")]
        public AudioSource musicSource;
        [Range(0f, 1f)] public float endingMusicVolume = 0.8f;
        public EndingMusic[] endingMusic;

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

            // D13: the director (if present) picks the ending from run state.
            StoryNode endingNode = null;
            string endingId = null;
            StoryDirector director = StoryDirector.Instance;
            if (director != null && director.config != null)
            {
                endingId = director.EnsureEndingDecided();
                endingNode = director.GetEndingNode(endingId);
                ApplyEndingTint(endingId);
                PlayEndingMusic(endingId);
            }

            if (dawnTitle != null)
            {
                // Ending node conventions (see StoryDirector.GetEndingNode):
                // localizationKeys[0] = title, text = body, localizationKeys[1] = narrator.
                dawnTitle.text = endingNode != null && endingNode.localizationKeys != null
                        && endingNode.localizationKeys.Length > 0
                    ? StoryStrings.Tr(endingNode.localizationKeys[0])
                    : Localization.TrPresent("DAWN");
                dawnTitle.font = Localization.CurrentFont;
            }

            if (dawnBody != null)
            {
                // The string is the exact translation key, so Arabic is picked up too.
                dawnBody.text = endingNode != null
                    ? StoryStrings.Tr(endingNode.text)
                    : Localization.TrPresent("The gate closes behind her. The sky above the school turns from black to grey to a thin, clean gold.\n\n"
                        + "She opens the memory box on the pavement and reads her mother's lesson plans one last time.");
                dawnBody.font = Localization.CurrentFont;
                dawnBody.alignment = Localization.Current == GameLanguage.Arabic
                    ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
            }

            if (dawnGroup != null) dawnGroup.alpha = 0f;
            ApplyDawn(0f);

            // The narrator closes the night once, before the recap appears.
            StoryTeller.Speak(endingNode != null && endingNode.localizationKeys != null
                    && endingNode.localizationKeys.Length > 1
                ? StoryStrings.Get(endingNode.localizationKeys[1])
                : Localization.Tr(
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

            if (recap != null)
            {
                StoryDirector withDirector = StoryDirector.Instance;
                if (withDirector != null && withDirector.config != null)
                {
                    recap.SetEnding(withDirector.CurrentEndingId);
                }
                recap.Show(RunResults.Answers);
            }
        }

        // Phase 1: one-shot cue per ending (builder wires the clips from the
        // AudioLibrary; silent fallback when the id has no clip yet or the
        // scene is played without the school library).
        // Registry fallback when no serialized entry matches (D13 endings):
        // HopeAlt is the alternate for PartialMemory/HollowEscape only; it never
        // replaces HopeGold (FullRecovery) or DawnlessGrey (Dawnless).
        static string RegistryCueFor(string id)
        {
            switch (id)
            {
                case "FullRecovery": return "MUS_Ending_HopeGold";
                case "Dawnless": return "MUS_Ending_DawnlessGrey";
                case "PartialMemory":
                case "HollowEscape": return "MUS_Ending_HopeAlt";
                default: return null;
            }
        }

        void PlayEndingMusic(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            // Dawn ambience layer (AMB_DawnBirds): null-safe, loops under the
            // ending music.
            if (Core.AudioManager.Instance != null)
                Core.AudioManager.Instance.PlayAmbience(Audio.SfxRegistry.Clip("AMB_DawnBirds"));

            AudioClip registered = Audio.SfxRegistry.Clip(RegistryCueFor(id));
            if (registered != null)
            {
                if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.spatialBlend = 0f;
                musicSource.loop = false;
                musicSource.playOnAwake = false;
                musicSource.volume = Mathf.Clamp01(endingMusicVolume);
                musicSource.clip = registered;
                musicSource.Play();
                return;
            }

            if (endingMusic == null) return;
            for (int i = 0; i < endingMusic.Length; i++)
            {
                if (endingMusic[i] == null || endingMusic[i].endingId != id || endingMusic[i].clip == null) continue;

                if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.spatialBlend = 0f;
                musicSource.loop = false;
                musicSource.playOnAwake = false;
                musicSource.volume = Mathf.Clamp01(endingMusicVolume);
                musicSource.clip = endingMusic[i].clip;
                musicSource.Play();
                return;
            }
        }

        // Swaps the dawn target colours for the chosen ending before the ease.
        void ApplyEndingTint(string endingId)
        {
            if (endingTints == null) return;
            for (int i = 0; i < endingTints.Length; i++)
            {
                if (endingTints[i] == null || endingTints[i].endingId != endingId) continue;
                skyDawn = endingTints[i].sky;
                sunDawn = endingTints[i].sun;
                cameraDawn = endingTints[i].camera;
                ambientDawn = endingTints[i].ambient;
                fogDawn = endingTints[i].fog;
                return;
            }
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
