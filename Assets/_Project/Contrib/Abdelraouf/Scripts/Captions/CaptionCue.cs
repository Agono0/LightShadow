using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.Captions
{
    // One caption as stored in Data/Captions/captions.json. keyEn is the
    // English key callers pass to CaptionService.Raise.
    [Serializable]
    public class CaptionCue
    {
        public string keyEn = "";
        public string textEn = "";
        public string textAr = "";
        public float seconds = 2.5f;
    }

    [Serializable]
    class CaptionFile
    {
        public bool draft = true;
        public List<CaptionCue> cues = new List<CaptionCue>();
    }

    // The cue keys the first version ships. Pass these (or any key present in
    // captions.json) to CaptionService.Raise.
    public static class CaptionKeys
    {
        public const string FootstepsNear = "Footsteps nearby";
        public const string Whisper = "Whisper";
        public const string DoorSlam = "Door slam";
        public const string PowerHum = "Power hum";
        public const string ShadowGrowl = "Shadow growl";
    }

    // Parses and looks up cues. Pure data, no scene access.
    public static class CaptionCatalog
    {
        static List<CaptionCue> cache;
        static string cachedSource;

        public static IReadOnlyList<CaptionCue> Load(string json)
        {
            if (cache != null && cachedSource == json) return cache;
            var result = new List<CaptionCue>();
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    CaptionFile file = JsonUtility.FromJson<CaptionFile>(json);
                    if (file != null && file.cues != null)
                    {
                        for (int i = 0; i < file.cues.Count; i++)
                        {
                            CaptionCue cue = file.cues[i];
                            if (cue != null && !string.IsNullOrEmpty(cue.keyEn)) result.Add(cue);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("[LightShadow] Caption data unreadable, only raw keys will show: " + exception.Message);
                }
            }
            cache = result;
            cachedSource = json;
            return cache;
        }

        public static CaptionCue Find(string json, string keyEn)
        {
            if (string.IsNullOrEmpty(keyEn)) return null;
            IReadOnlyList<CaptionCue> all = Load(json);
            for (int i = 0; i < all.Count; i++)
            {
                if (string.Equals(all[i].keyEn, keyEn, StringComparison.OrdinalIgnoreCase)) return all[i];
            }
            return null;
        }
    }
}
