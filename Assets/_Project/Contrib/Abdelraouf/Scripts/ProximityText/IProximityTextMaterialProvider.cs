using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.ProximityText
{
    // Handoff point for the text material. The owner's side adds a
    // build-included ZTest LEqual text shader and material in shared
    // Resources (contract 2026-11-27); this interface is how ProximityText
    // receives that material without this module touching shared shader or
    // material assets.
    //
    // Implementations may return null: the text then renders with the font's
    // default material, which is NOT depth-tested (it draws through walls).
    public interface IProximityTextMaterialProvider
    {
        Material GetTextMaterial();
    }
}
