using System.Collections.Generic;
using LightShadow.Audio;
using UnityEditor;
using UnityEngine;

namespace LightShadow.Tools
{
    // Keeps the AudioLibrary asset's clip references in sync with the audio
    // folder by naming convention, so bro's clip drops need no serialized
    // .asset edits: SFX_<Family>_<n> files fill the matching array field,
    // SFX_<Single> fills a single field. Runs at the top of every batch
    // build (SceneBuilder.BuildAll) and from the menu for local checks.
    public static class AudioLibrarySync
    {
        const string LibraryPath = "Assets/_Project/Audio/AudioLibrary.asset";
        const string SfxFolder = "Assets/_Project/Audio/SFX";

        // Registry id -> AudioLibrary field. Arrays are numbered families;
        // singles map 1:1. Pure mapping, unit-tested in EditMode.
        public static string FieldForId(string id)
        {
            switch (id)
            {
                case "SFX_FootstepConcrete": return "footstepConcrete";
                case "SFX_FootstepWood": return "footstepWood";
                case "SFX_FootstepCarpet": return "footstepCarpet";
                case "SFX_FootstepGrass": return "footstepGrass";
                case "SFX_FootstepGravel": return "footstepGravel";
                case "SFX_FootstepLeaves": return "footstepLeaves";
                case "SFX_FootstepMud": return "footstepMud";
                case "SFX_Breath_Tired": return "breathTired";
                default: return null;
            }
        }

        static readonly string[] FamilyIds =
        {
            "SFX_FootstepConcrete", "SFX_FootstepWood", "SFX_FootstepCarpet",
            "SFX_FootstepGrass", "SFX_FootstepGravel", "SFX_FootstepLeaves",
            "SFX_FootstepMud",
        };

        static readonly string[] SingleIds = { "SFX_Breath_Tired" };

        [MenuItem("LightShadow/Sync Audio Library")]
        public static void SyncMenu()
        {
            Debug.Log("[LightShadow] " + Sync());
        }

        // Fills the mapped fields from the SFX folder. Returns a one-line
        // summary; saves the asset only when a field actually changed.
        public static string Sync()
        {
            AudioLibrary library = AssetDatabase.LoadAssetAtPath<AudioLibrary>(LibraryPath);
            if (library == null) return "AudioLibrary sync skipped: no asset at " + LibraryPath;

            SerializedObject so = new SerializedObject(library);
            int changed = 0;

            for (int i = 0; i < FamilyIds.Length; i++)
                changed += SyncArray(so, FamilyIds[i]);
            for (int i = 0; i < SingleIds.Length; i++)
                changed += SyncSingle(so, SingleIds[i]);

            if (changed > 0)
            {
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(library);
                AssetDatabase.SaveAssetIfDirty(library);
            }
            return "AudioLibrary sync: " + changed + " field(s) updated.";
        }

        static int SyncArray(SerializedObject so, string family)
        {
            SerializedProperty field = so.FindProperty(FieldForId(family));
            if (field == null || !field.isArray) return 0;

            List<AudioClip> clips = new List<AudioClip>();
            for (int n = 0; ; n++)
            {
                AudioClip clip = LoadClip(family + "_" + n);
                if (clip == null) break;
                clips.Add(clip);
            }
            if (clips.Count == 0) return 0; // never wipe a field on an empty folder

            if (field.arraySize == clips.Count)
            {
                bool same = true;
                for (int i = 0; i < clips.Count; i++)
                    if (field.GetArrayElementAtIndex(i).objectReferenceValue != clips[i]) { same = false; break; }
                if (same) return 0;
            }

            field.arraySize = clips.Count;
            for (int i = 0; i < clips.Count; i++)
                field.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
            return 1;
        }

        static int SyncSingle(SerializedObject so, string id)
        {
            SerializedProperty field = so.FindProperty(FieldForId(id));
            if (field == null) return 0;
            AudioClip clip = LoadClip(id);
            if (clip == null || field.objectReferenceValue == clip) return 0;
            field.objectReferenceValue = clip;
            return 1;
        }

        static AudioClip LoadClip(string name)
        {
            string[] guids = AssetDatabase.FindAssets(name + " t:AudioClip", new[] { SfxFolder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == name)
                    return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            return null;
        }
    }
}
