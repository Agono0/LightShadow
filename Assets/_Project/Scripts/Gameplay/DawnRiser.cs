using System.Collections;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // Wakes the dawn-chase lighting gently when the gate is powered. Without it
    // seven lamps and a glow plane snap on in a single frame, which reads as a
    // bug rather than a sunrise. Runs once, then leaves the lights alone.
    public class DawnRiser : MonoBehaviour
    {
        [Header("Wiring")]
        public Renderer glow;
        public float duration = 2.4f;

        Light[] lights;
        float[] baseIntensity;
        Color baseGlow = Color.black;
        bool hasBaseColor;

        void Awake()
        {
            Capture();
        }

        void OnEnable()
        {
            if (lights == null) Capture();
            StartCoroutine(Rise());
        }

        void Capture()
        {
            lights = GetComponentsInChildren<Light>(true);
            baseIntensity = new float[lights.Length];
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null) baseIntensity[i] = lights[i].intensity;
            }

            if (glow == null)
            {
                Transform plane = transform.Find("DawnPlane");
                if (plane != null) glow = plane.GetComponent<Renderer>();
            }

            if (glow == null) return;
            Material material = glow.sharedMaterial;
            if (material == null) return;
            hasBaseColor = material.HasProperty("_BaseColor");
            baseGlow = hasBaseColor ? material.GetColor("_BaseColor") : material.color;
        }

        IEnumerator Rise()
        {
            Material glowMaterial = null;
            if (glow != null)
            {
                glowMaterial = glow.material;
                if (glowMaterial != null)
                {
                    if (hasBaseColor) glowMaterial.SetColor("_BaseColor", Color.black);
                    else glowMaterial.color = Color.black;
                }
            }

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.1f, duration);
                float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

                for (int i = 0; i < lights.Length; i++)
                {
                    if (lights[i] != null) lights[i].intensity = baseIntensity[i] * eased;
                }

                if (glowMaterial != null)
                {
                    Color color = baseGlow * eased;
                    if (hasBaseColor) glowMaterial.SetColor("_BaseColor", color);
                    else glowMaterial.color = color;
                }

                yield return null;
            }

            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null) lights[i].intensity = baseIntensity[i];
            }

            if (glowMaterial != null)
            {
                if (hasBaseColor) glowMaterial.SetColor("_BaseColor", baseGlow);
                else glowMaterial.color = baseGlow;
            }
        }
    }
}
