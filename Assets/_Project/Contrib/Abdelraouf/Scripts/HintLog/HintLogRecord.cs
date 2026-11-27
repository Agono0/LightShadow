using System;
using System.Collections.Generic;
using LightShadow.Contrib.Abdelraouf.Hints;
using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.HintLog
{
    // Remembers the highest hint level actually shown per puzzle, for the
    // Hint Recall screen only. It does not change HintService.
    //
    // Why it exists: HintService keeps its levels in a private static map and
    // removes a puzzle's entry when GameEvents.PuzzleSolved fires, so after a
    // solve HintService.LevelFor(id) is 0 again. This record keeps the shown
    // level so the player can still read hints they already saw. The log
    // reads max(HintService.LevelFor, this record), both clamped, so it can
    // never show a level that was not shown.
    //
    // Save: one contributor blob under SaveKey ("hintlog") through the same
    // SaveCaptured/SaveApplied + SetContrib/GetContrib path HintService uses.
    //
    // Run scope: HintService resets its levels in Awake for a fresh scene. This
    // record follows the HintService instance: when the instance changes
    // (a new scene or run) the record is cleared, unless a save restore has
    // just filled it. UNVERIFIED in the editor: the order of SaveApplied and
    // scene-load callbacks was read from source, not observed.
    public static class HintLogRecord
    {
        public const string SaveKey = "hintlog";

        static readonly Dictionary<string, int> seen = new Dictionary<string, int>();
        static object owner;
        static bool restoredPending;

        [Serializable]
        class Blob
        {
            public List<string> ids = new List<string>();
            public List<int> levels = new List<int>();
        }

        public static int Count => seen.Count;

        public static int Level(string puzzleId)
        {
            int level;
            return !string.IsNullOrEmpty(puzzleId) && seen.TryGetValue(puzzleId, out level) ? level : 0;
        }

        public static void Record(string puzzleId, int level)
        {
            if (string.IsNullOrEmpty(puzzleId)) return;
            level = HintLogModel.Clamp(level);
            if (level < 1) return;
            int existing;
            if (!seen.TryGetValue(puzzleId, out existing) || level > existing) seen[puzzleId] = level;
        }

        public static void Clear()
        {
            seen.Clear();
            restoredPending = false;
            owner = null;
        }

        // Called with the live HintService before reading or recording. A new
        // instance means a fresh scene: start empty unless a restore just ran.
        public static void Adopt(object currentOwner)
        {
            if (ReferenceEquals(currentOwner, owner)) return;
            if (!restoredPending) seen.Clear();
            restoredPending = false;
            owner = currentOwner;
        }

        public static string ToJson()
        {
            var blob = new Blob();
            foreach (KeyValuePair<string, int> pair in seen)
            {
                blob.ids.Add(pair.Key);
                blob.levels.Add(pair.Value);
            }
            return JsonUtility.ToJson(blob);
        }

        // Replaces the record. Bad or empty input leaves it empty.
        public static void FromJson(string json)
        {
            seen.Clear();
            if (string.IsNullOrEmpty(json)) return;
            Blob blob = JsonUtility.FromJson<Blob>(json);
            if (blob == null || blob.ids == null || blob.levels == null) return;
            int count = Math.Min(blob.ids.Count, blob.levels.Count);
            for (int i = 0; i < count; i++) Record(blob.ids[i], blob.levels[i]);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Hook()
        {
            Clear();
            HintService.HintShown -= OnHintShown;
            HintService.HintShown += OnHintShown;
            SaveSystem.SaveCaptured -= OnSaveCaptured;
            SaveSystem.SaveCaptured += OnSaveCaptured;
            SaveSystem.SaveApplied -= OnSaveApplied;
            SaveSystem.SaveApplied += OnSaveApplied;
        }

        static void OnHintShown(string puzzleId, int level)
        {
            Adopt(HintService.Instance);
            Record(puzzleId, level);
        }

        static void OnSaveCaptured(SaveSystem.SaveData data)
        {
            try
            {
                Adopt(HintService.Instance);
                SaveSystem.SetContrib(data, SaveKey, ToJson());
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Hint log could not write its save blob: " + exception.Message);
            }
        }

        static void OnSaveApplied(SaveSystem.SaveData data)
        {
            try
            {
                FromJson(SaveSystem.GetContrib(data, SaveKey));
            }
            catch (Exception exception)
            {
                seen.Clear();
                Debug.LogWarning("[LightShadow] Hint log save blob unreadable, starting empty: " + exception.Message);
            }
            restoredPending = true;
        }
    }
}
