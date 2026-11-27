using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.ProximityText
{
    // Provider that loads the depth-tested text material from Resources by
    // path. The owner's side ships the material (build-included ZTest LEqual
    // text shader, contract 2026-11-27) at DefaultResourcePath; the path is
    // the part of the contract to coordinate - it is a string here and in
    // Docs/contrib/abdelraouf/PROXIMITY_TEXT.md, and this module never writes
    // into shared Resources itself.
    public class ResourcesProximityTextMaterial : MonoBehaviour, IProximityTextMaterialProvider
    {
        public const string DefaultResourcePath = "ProximityText/DepthTestedTextMaterial";

        [Tooltip("Resources-relative path of the owner's depth-tested text material.")]
        public string resourcePath = DefaultResourcePath;

        Material cached;
        bool attempted;
        static bool warnedMissing;

        public Material GetTextMaterial()
        {
            if (!attempted)
            {
                attempted = true;
                if (!string.IsNullOrEmpty(resourcePath)) cached = Resources.Load<Material>(resourcePath);
                if (cached == null && !warnedMissing)
                {
                    warnedMissing = true;
                    Debug.LogWarning("[LightShadow] ProximityText material missing at Resources/"
                        + resourcePath + ". Falling back to the font material, which is NOT "
                        + "depth-tested: the text will draw through walls until the owner's "
                        + "ZTest LEqual material lands.");
                }
            }
            return cached;
        }
    }
}
