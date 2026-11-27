using UnityEngine;
using UnityEditor;

namespace LightShadow.Tools
{
    public static partial class SceneBuilder
    {
        // Swaps a primitive stand-in for a landed CC0 GLB (P1 props from the
        // model stream). The GLB is fitted so its largest dimension equals
        // targetSize, centred on 'center' (or standing on 'center' when
        // standOnBase), and the stand-in renderers are switched off while their
        // colliders stay, so physics and interaction are unchanged. When the
        // editor cannot import the GLB the primitives stay as before. NOT SEEN
        // IN THE EDITOR: scale, pivot and orientation are unverified.
        static bool TryAddModelVisual(Transform parent, string assetPath, string visualName,
            Vector3 center, float targetSize, bool standOnBase, Transform hideRenderersUnder)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (model == null)
            {
                Debug.LogWarning("[LightShadow] " + assetPath + " did not import; keeping primitives.");
                return false;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(model, parent);
            instance.name = visualName;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                Object.DestroyImmediate(instance);
                return false;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            float largest = Mathf.Max(0.001f, Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z)));
            float scale = Mathf.Clamp(targetSize / largest, 0.001f, 1000f);
            instance.transform.localScale = Vector3.one * scale;

            // Offset from the instance origin to the bounds centre / base, scaled.
            Vector3 anchor = standOnBase
                ? new Vector3(bounds.center.x, bounds.min.y, bounds.center.z)
                : bounds.center;
            Vector3 offset = (anchor - instance.transform.position) * scale;
            instance.transform.localPosition = center - offset;

            // Visual only: no colliders from the model, layer 0 like the dynamic props.
            foreach (Collider c in instance.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            foreach (Transform t in instance.GetComponentsInChildren<Transform>()) t.gameObject.layer = 0;

            if (hideRenderersUnder != null)
            {
                foreach (Renderer r in hideRenderersUnder.GetComponentsInChildren<Renderer>())
                {
                    if (r.transform.IsChildOf(instance.transform)) continue;
                    r.enabled = false;
                }
            }
            return true;
        }
    }
}
