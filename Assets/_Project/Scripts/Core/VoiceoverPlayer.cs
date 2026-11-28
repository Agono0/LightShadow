using System.Collections.Generic;
using UnityEngine;

namespace LightShadow.Core
{
    // Plays the generated voice clips (finish-the-game plan, phase 8). Clips
    // live under Resources/VO/<locale>/<lineId> where lineId is the line
    // sheet's canonical VO_<nodeId>[_Cn] id and locale follows the current
    // language. A missing clip (a line with no recording, or clips not
    // shipped) returns false. Story nodes then fall back to StoryTeller's
    // SAPI narration; the ending cinematic only shows its subtitle (no
    // speech). The narrator toggle gates the voice and the SAPI path.
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
            // Clips load one at a time on first use (audit 4.4): no bulk load
            // spike, and the old locale's clips are dropped here and unloaded.
            Resources.UnloadUnusedAssets();
        }

        // Length in seconds of the clip the last Play() started; 0 when no clip
        // played (missing clip, narrator off, empty id). Lets timed sequences
        // wait for the voice instead of cutting it (audit: ending VO overruns).
        public static float LastPlayedSeconds { get; private set; }

        public static bool Play(string lineId)
        {
            LastPlayedSeconds = 0f;
            if (string.IsNullOrEmpty(lineId)) return false;
            if (!GameSettings.NarratorEnabled) return true; // narrator off: no VO, no SAPI fallback
            EnsureLocale();

            AudioClip clip;
            if (missing.Contains(lineId)) return false;
            if (!clips.TryGetValue(lineId, out clip))
            {
                clip = Resources.Load<AudioClip>("VO/" + loadedLocale + "/" + lineId);
                if (clip != null) clips[lineId] = clip;
            }
            if (clip == null)
            {
                // Cache misses too: a line without a clip should not rescan
                // the dictionary every time it shows.
                missing.Add(lineId);
                Debug.LogWarning("[LightShadow] No voice clip for " + lineId + " (" + loadedLocale + ", Resources/VO/" + loadedLocale + "). Caller decides: story nodes may use SAPI, the ending shows subtitles only.");
                return false;
            }

            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("[LightShadow] Voice clip " + lineId + " loaded but there is no AudioManager to play it.");
                return false;
            }
            AudioManager.Instance.PlayVoice(clip);
            LastPlayedSeconds = clip.length;
            return true;
        }

        public static void Stop()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.StopVoice();
        }
    }
}
