using System.Collections.Generic;
using UnityEngine;

namespace LightShadow.Core
{
    // Plays the generated voice clips (finish-the-game plan, phase 8). Clips
    // live under Resources/VO/<locale>/<lineId> where lineId is the line
    // sheet's canonical VO_<nodeId>[_Cn] id and locale follows the current
    // language. A missing clip (a line with no recording, or clips not
    // shipped) returns false so the caller can fall back to StoryTeller's
    // SAPI narration; the narrator toggle gates both paths.
    public static class VoiceoverPlayer
    {
        static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        static readonly HashSet<string> missing = new HashSet<string>();
        static string loadedLocale = "";

        static string Locale => Localization.Current == GameLanguage.Arabic ? "AR" : "EN";

        static void EnsureLocale()
        {
            if (loadedLocale == Locale) return;
            loadedLocale = Locale;
            clips.Clear();
            missing.Clear();
            AudioClip[] all = Resources.LoadAll<AudioClip>("VO/" + loadedLocale);
            for (int i = 0; i < all.Length; i++) clips[all[i].name] = all[i];
        }

        public static bool Play(string lineId)
        {
            if (string.IsNullOrEmpty(lineId)) return false;
            if (!GameSettings.NarratorEnabled) return true; // narrator off: no VO, no SAPI fallback
            EnsureLocale();

            AudioClip clip;
            if (missing.Contains(lineId) || !clips.TryGetValue(lineId, out clip))
            {
                // Cache misses too: a line without a clip should not rescan
                // the dictionary every time it shows.
                missing.Add(lineId);
                return false;
            }

            if (AudioManager.Instance == null) return false;
            AudioManager.Instance.PlayVoice(clip);
            return true;
        }

        public static void Stop()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.StopVoice();
        }
    }
}
