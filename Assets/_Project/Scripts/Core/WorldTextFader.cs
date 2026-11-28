using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightShadow.Core
{
    // Fixes the "quiz text is transparent and visible from anywhere" bug.
    // The scene builder makes world labels with TextMesh, whose default
    // material ignores depth, so the text drew through walls and across the
    // whole school. After each scene load this swaps every such label onto the
    // depth-tested LightShadow/WorldText material (Resources/ProximityText) and
    // fades each one by distance and viewing angle: readable when the player is
    // near and looking at it, gone otherwise.
    //
    // Labels that already belong to a Contrib ProximityText (it brings its own
    // depth-tested material and fade) are left alone.
    public class WorldTextFader : MonoBehaviour
    {
        public const string MaterialPath = "ProximityText/DepthTestedTextMaterial";
        public const float ShowDistance = 4.5f;
        public const float HideDistance = 6f;
        public const float ViewHalfAngle = 70f;
        const float TickSeconds = 0.05f;

        static readonly int FadeId = Shader.PropertyToID("_Fade");

        class Entry
        {
            public TextMesh text;
            public MeshRenderer renderer;
            public Material material;
            public float fade;
        }

        static WorldTextFader instance;
        readonly List<Entry> entries = new List<Entry>();
        Material template;
        Camera viewer;
        float clock;

        // ---- pure rules (EditMode tests) ----------------------------------

        // Target visibility 0..1 from distance (m) and angle (deg) between the
        // camera's forward and the direction to the text.
        public static float TargetFade(float distance, float angleDegrees)
        {
            if (distance >= HideDistance || angleDegrees >= 90f) return 0f;
            float near = distance <= ShowDistance
                ? 1f
                : 1f - Mathf.InverseLerp(ShowDistance, HideDistance, distance);
            float view = angleDegrees <= ViewHalfAngle
                ? 1f
                : 1f - Mathf.InverseLerp(ViewHalfAngle, 90f, angleDegrees);
            return Mathf.Clamp01(near * view);
        }

        public static float Approach(float current, float target, float deltaTime, float perSecond = 4f)
        {
            return Mathf.MoveTowards(current, target, perSecond * Mathf.Max(0f, deltaTime));
        }

        // ---- wiring ---------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Hook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Collect();
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Collect();
        }

        static void Collect()
        {
            TextMesh[] texts = FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (texts.Length == 0) return;

            if (instance == null)
            {
                instance = new GameObject("WorldTextFader").AddComponent<WorldTextFader>();
            }
            for (int i = 0; i < texts.Length; i++) instance.Adopt(texts[i]);
        }

        void OnEnable()
        {
            Font.textureRebuilt += OnFontRebuilt;
        }

        void OnDisable()
        {
            Font.textureRebuilt -= OnFontRebuilt;
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].material != null) Destroy(entries[i].material);
            }
        }

        // Dynamic fonts rebuild their atlas as new glyphs appear; keep the
        // label material pointed at the live atlas.
        void OnFontRebuilt(Font font)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];
                if (entry.text != null && entry.text.font == font && entry.material != null)
                {
                    entry.material.mainTexture = font.material.mainTexture;
                }
            }
        }

        void Adopt(TextMesh text)
        {
            if (text == null) return;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].text == text) return;
            }
            if (text.GetComponentInParent<LightShadow.Contrib.Abdelraouf.ProximityText.ProximityText>() != null) return;

            MeshRenderer renderer = text.GetComponent<MeshRenderer>();
            Font font = text.font;
            if (renderer == null || font == null) return;

            if (template == null) template = Resources.Load<Material>(MaterialPath);
            if (template == null)
            {
                Debug.LogWarning("[LightShadow] World text material missing at Resources/" + MaterialPath
                    + ": labels keep the default font material (not depth-tested).");
                return;
            }

            Material material = new Material(template);
            material.mainTexture = font.material.mainTexture;
            material.color = text.color;
            material.SetFloat(FadeId, 0f);
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            entries.Add(new Entry { text = text, renderer = renderer, material = material, fade = 0f });
        }

        void Update()
        {
            clock -= Time.unscaledDeltaTime;
            if (clock > 0f) return;
            float dt = TickSeconds + Mathf.Abs(clock);
            clock = TickSeconds;

            if (viewer == null) viewer = Camera.main;
            if (viewer == null) return;

            Vector3 eye = viewer.transform.position;
            Vector3 forward = viewer.transform.forward;

            for (int i = entries.Count - 1; i >= 0; i--)
            {
                Entry entry = entries[i];
                if (entry.text == null || entry.material == null)
                {
                    if (entry.material != null) Destroy(entry.material);
                    entries.RemoveAt(i);
                    continue;
                }

                Vector3 toText = entry.text.transform.position - eye;
                float distance = toText.magnitude;
                float angle = distance < 0.01f ? 0f : Vector3.Angle(forward, toText);
                float target = TargetFade(distance, angle);
                entry.fade = Approach(entry.fade, target, dt);
                entry.material.SetFloat(FadeId, entry.fade);
                bool show = entry.fade > 0.01f;
                if (entry.renderer.enabled != show) entry.renderer.enabled = show;
            }
        }
    }
}
