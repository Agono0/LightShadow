using LightShadow.Contrib.Abdelraouf.Captions;
using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Audio
{
    /// <summary>
    /// One-line playback for registered library clips. Asks SfxRegistry for the
    /// id (random variant when the family has several) and falls back to the
    /// supplied synthesised clip when the library did not load, so gameplay
    /// code never needs to care whether the real clip shipped.
    /// </summary>
    public static class Sfx
    {
        // Sound captions: which library ids raise which caption cue. Only ids
        // with a matching cue in captions.json are listed. CaptionService is
        // a no-op when absent or switched off.
        static string CaptionFor(string id)
        {
            switch (id)
            {
                case "SFX_PowerSurge": return CaptionKeys.PowerHum;
                case "SFX_GateSlam":
                case "SFX_DoorSteelBang": return CaptionKeys.DoorSlam;
                case "SFX_ShadowHiss":
                case "SFX_ShadowMoan":
                case "SFX_WraithRoam":
                case "SFX_WraithAmbushStinger": return CaptionKeys.ShadowGrowl;
                default: return null;
            }
        }

        static void Caption(string id, Vector3 position)
        {
            string key = CaptionFor(id);
            if (key != null) CaptionService.Raise(key, position);
        }

        static Vector3 ListenerPosition()
        {
            Camera cam = Camera.main;
            return cam != null ? cam.transform.position : Vector3.zero;
        }

        public static AudioClip Pick(string id)
        {
            int count = SfxRegistry.VariantCount(id);
            return count > 0 ? SfxRegistry.ClipVariant(id, count) : SfxRegistry.Clip(id);
        }

        public static void Play(string id, AudioClip fallback = null, float volume = 1f)
        {
            AudioClip clip = Pick(id);
            if (clip == null) clip = fallback;
            if (clip == null || AudioManager.Instance == null) return;
            AudioManager.Instance.PlaySfx(clip, volume);
            Caption(id, ListenerPosition());
        }

        public static void PlayAt(string id, Vector3 position, AudioClip fallback = null)
        {
            AudioClip clip = Pick(id);
            if (clip == null) clip = fallback;
            if (clip == null || AudioManager.Instance == null) return;
            AudioManager.Instance.PlaySfxAt(clip, position);
            Caption(id, position);
        }
    }
}
