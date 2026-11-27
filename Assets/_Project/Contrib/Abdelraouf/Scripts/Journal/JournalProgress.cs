using System;
using System.Collections.Generic;
using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.Journal
{
    // Which notes the player has found. Static and scene-independent, like
    // Inventory. Persists through SaveSystem contract H1 under the key
    // "journal": SetContrib on SaveCaptured, GetContrib on SaveApplied.
    public static class JournalProgress
    {
        public const string SaveKey = "journal";

        static readonly List<string> found = new List<string>();

        [Serializable]
        class JournalSave
        {
            public List<string> ids = new List<string>();
        }

        // Fired after a new note is added or the found set is reloaded.
        public static event Action Changed;

        public static IReadOnlyList<string> Found => found;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Hook()
        {
            found.Clear();
            SaveSystem.SaveCaptured -= OnSaveCaptured;
            SaveSystem.SaveCaptured += OnSaveCaptured;
            SaveSystem.SaveApplied -= OnSaveApplied;
            SaveSystem.SaveApplied += OnSaveApplied;
        }

        public static bool Has(string id)
        {
            return !string.IsNullOrEmpty(id) && found.Contains(id);
        }

        // Returns true when the note was new.
        public static bool Add(string id)
        {
            if (string.IsNullOrEmpty(id) || found.Contains(id)) return false;
            found.Add(id);
            RaiseChanged();
            return true;
        }

        static void OnSaveCaptured(SaveSystem.SaveData data)
        {
            try
            {
                var save = new JournalSave();
                save.ids.AddRange(found);
                SaveSystem.SetContrib(data, SaveKey, JsonUtility.ToJson(save));
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Journal could not write its save blob: " + exception.Message);
            }
        }

        static void OnSaveApplied(SaveSystem.SaveData data)
        {
            try
            {
                found.Clear();
                string json = SaveSystem.GetContrib(data, SaveKey);
                if (!string.IsNullOrEmpty(json))
                {
                    JournalSave save = JsonUtility.FromJson<JournalSave>(json);
                    if (save != null && save.ids != null)
                    {
                        for (int i = 0; i < save.ids.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(save.ids[i]) && !found.Contains(save.ids[i])) found.Add(save.ids[i]);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Journal save blob unreadable, starting with no notes: " + exception.Message);
            }
            RaiseChanged();
        }

        static void RaiseChanged()
        {
            Action handler = Changed;
            if (handler == null) return;
            try { handler(); }
            catch (Exception exception) { Debug.LogWarning("[LightShadow] Journal listener failed: " + exception.Message); }
        }
    }
}
