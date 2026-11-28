using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.ProximityText
{
    // World-space text that fades in when the player is close and looking at
    // it, and fades out otherwise. Built in code on a child object (TextMesh +
    // MeshRenderer), so it can sit on any GameObject with zero scene wiring
    // beyond the component itself.
    //
    // Depth testing (contract 2026-11-27): the owner's side provides a
    // build-included ZTest LEqual text material through an
    // IProximityTextMaterialProvider (see ResourcesProximityTextMaterial).
    // When no provider is assigned this component logs a warning once and uses
    // the font's default material, which is NOT depth-tested.
    //
    // Arabic: the localized font comes from Localization.CurrentFont and the
    // text switches on Localization.Current. Unity's TextMesh does no
    // right-to-left layout, so long Arabic lines can need manual line breaks;
    // keep the AR strings short. Editor/device check required.
    public class ProximityText : MonoBehaviour
    {
        [Header("Content")]
        public string textEn = "";
        [Multiline] public string textAr = "";

        [Header("Gating")]
        public float showRadius = 4f;
        public float hideRadius = 5f;
        [Tooltip("Cone half-angle in degrees around the camera forward.")]
        public float maxAngleDegrees = 60f;
        public float fadePerSecond = 3f;
        public bool billboard = true;

        [Header("Text")]
        public float characterSize = 0.2f;
        public int fontSize = 64;
        public Color color = new Color(0.95f, 0.9f, 0.8f, 1f);

        [Header("Material (owner's contract)")]
        [Tooltip("Component implementing IProximityTextMaterialProvider. Empty = warned font-material fallback, not depth-tested.")]
        public MonoBehaviour materialProviderSource;

        TextMesh label;
        MeshRenderer labelRenderer;
        Material fallbackMaterial;
        IProximityTextMaterialProvider provider;
        static bool warnedFallback;

        bool visible;
        float alpha;

        void Awake()
        {
            BuildLabel();
            Localization.LanguageChanged += OnLanguageChanged;
        }

        void OnDestroy()
        {
            Localization.LanguageChanged -= OnLanguageChanged;
        }

        void OnEnable()
        {
            ProximityTextManager.Register(this);
        }

        void OnDisable()
        {
            ProximityTextManager.Unregister(this);
        }

        public void SetText(string english, string arabic)
        {
            textEn = english ?? "";
            textAr = arabic ?? "";
            ApplyText();
        }

        void OnLanguageChanged()
        {
            ApplyText();
            ApplyFontAndMaterial();
        }

        // Driven by ProximityTextManager (one Update and one camera fetch for
        // every proximity text in the scene).
        internal void Tick(Camera viewer, float unscaledDt)
        {
            if (label == null) return;

            float distance = float.MaxValue;
            float cosAngle = 1f;
            if (viewer != null)
            {
                Vector3 toText = transform.position - viewer.transform.position;
                distance = toText.magnitude;
                if (distance > 0.0001f)
                {
                    cosAngle = Vector3.Dot(viewer.transform.forward, toText / distance);
                }
            }

            visible = ProximityTextMath.Visible(visible, distance, cosAngle,
                showRadius, hideRadius, ProximityTextMath.MinCosForDegrees(maxAngleDegrees));
            alpha = ProximityTextMath.StepAlpha(alpha, visible, unscaledDt, fadePerSecond);

            Color c = color;
            c.a *= alpha;
            label.color = c;

            if (billboard && viewer != null && alpha > 0f)
            {
                // Face the camera; TextMesh text reads correctly when rotated
                // to look the same way the camera looks.
                label.transform.rotation = Quaternion.LookRotation(label.transform.position - viewer.transform.position);
            }
        }

        void ApplyText()
        {
            if (label == null) return;
            bool arabic = Localization.Current == GameLanguage.Arabic;
            label.text = arabic && !string.IsNullOrEmpty(textAr) ? textAr : textEn;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
        }

        void ApplyFontAndMaterial()
        {
            if (label == null || labelRenderer == null) return;

            Font font = Localization.CurrentFont;
            if (font != null)
            {
                label.font = font;
                fallbackMaterial = font.material;
            }

            if (provider == null && materialProviderSource is IProximityTextMaterialProvider cast)
            {
                provider = cast;
            }

            Material material = provider != null ? provider.GetTextMaterial() : null;
            if (material != null)
            {
                labelRenderer.sharedMaterial = material;
                return;
            }

            if (!warnedFallback)
            {
                warnedFallback = true;
                Debug.LogWarning("[LightShadow] ProximityText '" + name + "' has no material provider; "
                    + "using the font material, which is NOT depth-tested (text draws through walls). "
                    + "Assign an IProximityTextMaterialProvider that returns the owner's ZTest LEqual material.");
            }
            if (fallbackMaterial != null) labelRenderer.sharedMaterial = fallbackMaterial;
        }

        void BuildLabel()
        {
            var labelObject = new GameObject("ProximityLabel");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = Vector3.zero;
            labelObject.transform.localRotation = Quaternion.identity;

            label = labelObject.AddComponent<TextMesh>();
            label.characterSize = characterSize;
            label.fontSize = fontSize;
            label.richText = false;
            labelRenderer = labelObject.GetComponent<MeshRenderer>();

            ApplyText();
            ApplyFontAndMaterial();

            // Hidden until the first Tick proves the player is close enough.
            Color c = color;
            c.a = 0f;
            label.color = c;
        }
    }
}
