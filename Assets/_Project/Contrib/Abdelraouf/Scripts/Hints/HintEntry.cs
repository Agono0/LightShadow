using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.Hints
{
    // One puzzle's three hint levels as stored in Data/Hints/hints.json.
    // puzzleId is the PuzzleBase.puzzleId value, which is the puzzle's
    // GameObject name when the scene builder leaves it empty (for example
    // "Puzzle_ChalkboardCipher"). English is the source text; Arabic sits
    // next to it and falls back to English when empty.
    [Serializable]
    public class HintEntry
    {
        public string puzzleId = "";
        public string nudgeEn = "";
        public string clueEn = "";
        public string answerEn = "";
        public string nudgeAr = "";
        public string clueAr = "";
        public string answerAr = "";

        // level: 1 nudge, 2 clue, 3 answer.
        public string Get(int level, bool arabic)
        {
            string en;
            string ar;
            switch (level)
            {
                case 1: en = nudgeEn; ar = nudgeAr; break;
                case 2: en = clueEn; ar = clueAr; break;
                default: en = answerEn; ar = answerAr; break;
            }
            return arabic && !string.IsNullOrEmpty(ar) ? ar : en;
        }
    }

    [Serializable]
    class HintFile
    {
        public bool draft = true;
        public List<HintEntry> hints = new List<HintEntry>();
    }

    // Parses and looks up hint entries. Pure data, no scene access.
    public static class HintCatalog
    {
        static List<HintEntry> cache;
        static string cachedSource;

        public static IReadOnlyList<HintEntry> Load(string json)
        {
            if (cache != null && cachedSource == json) return cache;
            var result = new List<HintEntry>();
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    HintFile file = JsonUtility.FromJson<HintFile>(json);
                    if (file != null && file.hints != null)
                    {
                        for (int i = 0; i < file.hints.Count; i++)
                        {
                            HintEntry entry = file.hints[i];
                            if (entry != null && !string.IsNullOrEmpty(entry.puzzleId)) result.Add(entry);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("[LightShadow] Hint data unreadable, hints will be empty: " + exception.Message);
                }
            }
            cache = result;
            cachedSource = json;
            return cache;
        }

        public static HintEntry Find(string json, string puzzleId)
        {
            IReadOnlyList<HintEntry> all = Load(json);
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].puzzleId == puzzleId) return all[i];
            }
            return null;
        }
    }
}
