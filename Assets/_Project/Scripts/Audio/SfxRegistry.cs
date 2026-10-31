using System.Collections.Generic;
using UnityEngine;

namespace LightShadow.Audio
{
    /// <summary>
    /// Runtime registry mapping sound ids ("SFX_QuizCorrect", "AMB_SchoolNight",
    /// ...) to real imported clips. Populated once by AudioLibraryBootstrap;
    /// consumers (ProceduralSfx, AmbientDrone, ShadowAudio, Footsteps) ask here
    /// FIRST and fall back to their synthesised clips when an id is missing —
    /// the game sounds real when the library ships and still works headlessly.
    /// </summary>
    public static class SfxRegistry
    {
        static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        public static void Register(string id, AudioClip clip)
        {
            if (string.IsNullOrEmpty(id) || clip == null) return;
            clips[id] = clip;
        }

        public static void RegisterMany(string id, AudioClip[] array)
        {
            if (array == null || array.Length == 0) return;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null) continue;
                clips[id + "_" + i] = array[i];
            }
        }

        public static AudioClip Clip(string id)
        {
            AudioClip clip;
            return clips.TryGetValue(id, out clip) ? clip : null;
        }

        /// <summary>Random pick among "id_0".."id_{count-1}"; null when the
        /// family is absent. Never allocates.</summary>
        public static AudioClip ClipVariant(string id, int count)
        {
            int index = Random.Range(0, count);
            return Clip(id + "_" + index);
        }

        public static bool HasAny(string id, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (clips.ContainsKey(id + "_" + i)) return true;
            }
            return false;
        }

        public static void Clear() => clips.Clear();
    }
}
