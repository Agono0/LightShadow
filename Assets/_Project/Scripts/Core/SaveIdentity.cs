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
            // Medkit props that were taken (audit K1). hasMedkitIdentity is
            // false in saves written before this field existed.
            public bool hasMedkitIdentity;
            public List<string> medkits = new List<string>();
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
            foreach (FuseBox box in UnityEngine.Object.FindObjectsByType<FuseBox>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (box != null && box.Restored) blob.fuses.Add(box.name);
            }
            foreach (KnowledgeFragment fragment in UnityEngine.Object.FindObjectsByType<KnowledgeFragment>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (fragment != null && fragment.Collected) blob.fragments.Add(fragment.SaveId);
            }
            blob.hasMedkitIdentity = true;
            foreach (MedkitPickup kit in UnityEngine.Object.FindObjectsByType<MedkitPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (kit != null && kit.Collected) blob.medkits.Add(kit.SaveId);
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
                if (blob.medkits == null) blob.medkits = new List<string>();
                return blob;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Which medkit props to hide on load (audit K1). New saves name the
        // kits taken. Old saves have no medkit identity: they only know the
        // inventory held a kit, so one kit (the first) is hidden, the other
        // stays available.
        public static bool[] SelectMedkitRestores(IList<string> names, Blob blob, bool oldSaveCarriedKit)
        {
            if (blob != null && blob.hasMedkitIdentity)
                return SelectRestores(names, blob.medkits, blob.medkits.Count);
            return SelectRestores(names, null, oldSaveCarriedKit ? 1 : 0);
        }

        // Which of the scene's objects to restore. names[i] is object i's
        // identity. Rules (audit F3/F4):
        //  - identity null (old save): the first `count` objects.
        //  - each saved identity restores ONE object, the first match; later
        //    objects with the same name are skipped (dup count is logged).
        //  - saved identities that match nothing (renamed or removed objects)
        //    fall back to the count: the remainder is filled with the first
        //    objects that were not matched, so the count and the world agree.
        public static bool[] SelectRestores(IList<string> names, List<string> saved, int count)
        {
            int n = names == null ? 0 : names.Count;
            bool[] pick = new bool[n];
            if (saved == null)
            {
                for (int i = 0; i < n && i < count; i++) pick[i] = true;
                return pick;
            }
            var used = new HashSet<string>();
            int matched = 0;
            for (int i = 0; i < n; i++)
            {
                string id = names[i];
                if (!Contains(saved, id)) continue;
                if (!used.Add(id)) { Debug.LogWarning("[LightShadow] Duplicate save identity '" + id + "', restored once."); continue; }
                pick[i] = true;
                matched++;
            }
            int missing = Mathf.Max(0, Mathf.Min(count, saved.Count) - matched);
            for (int i = 0; i < n && missing > 0; i++)
            {
                if (pick[i] || Contains(saved, names[i])) continue;
                pick[i] = true;
                missing--;
            }
            return pick;
        }

        public static bool Contains(List<string> ids, string id)
        {
            return ids != null && !string.IsNullOrEmpty(id) && ids.Contains(id);
        }
    }
}
