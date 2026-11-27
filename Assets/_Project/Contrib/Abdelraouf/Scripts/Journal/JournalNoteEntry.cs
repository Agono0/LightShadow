using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.Journal
{
    // One journal note as stored in Data/Journal/journal_notes.json.
    // English is the source text; Arabic is stored next to it (logical order,
    // shaped at display time by Localization.Present).
    [Serializable]
    public class JournalNoteEntry
    {
        public string id = "";
        public string titleEn = "";
        public string bodyEn = "";
        public string titleAr = "";
        public string bodyAr = "";
    }

    [Serializable]
    class JournalNoteFile
    {
        public bool draft = true;
        public List<JournalNoteEntry> notes = new List<JournalNoteEntry>();
    }

    // Parses and looks up notes. Pure data, no scene access.
    public static class JournalCatalog
    {
        static List<JournalNoteEntry> cache;
        static string cachedSource;

        public static IReadOnlyList<JournalNoteEntry> Load(string json)
        {
            if (cache != null && cachedSource == json) return cache;
            var result = new List<JournalNoteEntry>();
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    JournalNoteFile file = JsonUtility.FromJson<JournalNoteFile>(json);
                    if (file != null && file.notes != null)
                    {
                        for (int i = 0; i < file.notes.Count; i++)
                        {
                            JournalNoteEntry entry = file.notes[i];
                            if (entry != null && !string.IsNullOrEmpty(entry.id)) result.Add(entry);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("[LightShadow] Journal notes unreadable, journal will be empty: " + exception.Message);
                }
            }
            cache = result;
            cachedSource = json;
            return cache;
        }

        public static JournalNoteEntry Find(string json, string id)
        {
            IReadOnlyList<JournalNoteEntry> all = Load(json);
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].id == id) return all[i];
            }
            return null;
        }
    }
}
