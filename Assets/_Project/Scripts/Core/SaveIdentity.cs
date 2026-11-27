using System;
using System.Collections.Generic;
using LightShadow.Gameplay;
using UnityEngine;

namespace LightShadow.Core
{
    // Saves WHICH fuse boxes and knowledge fragments were done, by stable name,
    // in one contrib blob. Older saves lack it and GameManager falls back to
    // counts. Names come from the scene builder (Fuse_Library, fragment names).
    public static class SaveIdentity
    {
        public const string Key = "pickup_identity";

        [Serializable]
        public class Blob
        {
            public List<string> fuses = new List<string>();
            public List<string> fragments = new List<string>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Hook()
        {
            SaveSystem.SaveCaptured -= OnCaptured;
            SaveSystem.SaveCaptured += OnCaptured;
        }

        static void OnCaptured(SaveSystem.SaveData data)
        {
            if (data == null) return;
            var blob = new Blob();
            foreach (FuseBox box in UnityEngine.Object.FindObjectsByType<FuseBox>(FindObjectsSortMode.None))
            {
                if (box != null && box.Restored) blob.fuses.Add(box.name);
            }
            foreach (KnowledgeFragment fragment in UnityEngine.Object.FindObjectsByType<KnowledgeFragment>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (fragment != null && fragment.Collected) blob.fragments.Add(fragment.SaveId);
            }
            SaveSystem.SetContrib(data, Key, Write(blob));
        }

        public static string Write(Blob blob) => JsonUtility.ToJson(blob ?? new Blob());

        // Null when the save has no identity blob (or it is unreadable).
        public static Blob Read(SaveSystem.SaveData data)
        {
            string json = SaveSystem.GetContrib(data, Key);
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                Blob blob = JsonUtility.FromJson<Blob>(json);
                if (blob == null) return null;
                if (blob.fuses == null) blob.fuses = new List<string>();
                if (blob.fragments == null) blob.fragments = new List<string>();
                return blob;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool Contains(List<string> ids, string id)
        {
            return ids != null && !string.IsNullOrEmpty(id) && ids.Contains(id);
        }
    }
}
