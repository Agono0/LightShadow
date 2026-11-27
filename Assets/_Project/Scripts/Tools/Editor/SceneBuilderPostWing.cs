using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LightShadow.Tools
{
    // Per-wing LIT grading (MASTER_PLAN section 10: every wing has two lighting
    // states, DARK and LIT, with its own grade). A local warm ColorAdjustments
    // volume lives inside each fuse-controlled lights group: a dark wing keeps
    // the global cold night grade; restoring the wing's fuse activates the
    // lamps AND warms exactly that wing. The volumes ride the group's active
    // state, so no runtime code or save handling is involved.
    public static partial class SceneBuilder
    {
        static readonly string PostWingLitPath = PostFolder + "/Post_WingLit.asset";

        // One shared warm grade for every lit wing. Rebuilt every run so
        // repeated builds never stack duplicate components (BuildPostProfile
        // rule). Only exposure/saturation/filter override: URP blends volume
        // properties individually, so the global bloom, vignette, grain,
        // contrast and tonemapping carry through untouched inside the wing.
        static void BuildWingLitProfile()
        {
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(PostWingLitPath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, PostWingLitPath);
            }

            for (int i = profile.components.Count - 1; i >= 0; i--)
            {
                VolumeComponent existing = profile.components[i];
                profile.components.RemoveAt(i);
                if (existing != null) Object.DestroyImmediate(existing, true);
            }

            ColorAdjustments color = profile.Add<ColorAdjustments>(true);
            color.postExposure.Override(0.38f);
            color.saturation.Override(8f);
            color.colorFilter.Override(new Color(1f, 0.93f, 0.80f));

            EditorUtility.SetDirty(profile);
        }

        // A local volume on the Default layer (inside every camera volume
        // mask), priority above the global night grade (1), with a soft 3 m
        // blend so the warm edge breathes instead of snapping at the doorway.
        static void AddWingLitGrade(GameObject lightsGroup, string name, Vector3 center, Vector3 size)
        {
            if (lightsGroup == null) return;
            if (lightsGroup.transform.Find(name) != null) return;

            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(PostWingLitPath);
            if (profile == null) return;

            var go = new GameObject(name);
            go.transform.SetParent(lightsGroup.transform, false);
            go.transform.position = center;

            BoxCollider box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;

            Volume volume = go.AddComponent<Volume>();
            volume.isGlobal = false;
            volume.priority = 2f;
            volume.blendDistance = 3f;
            volume.sharedProfile = profile;
        }
    }
}
