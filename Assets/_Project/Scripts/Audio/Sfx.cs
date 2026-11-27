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
        }

        public static void PlayAt(string id, Vector3 position, AudioClip fallback = null)
        {
            AudioClip clip = Pick(id);
            if (clip == null) clip = fallback;
            if (clip == null || AudioManager.Instance == null) return;
            AudioManager.Instance.PlaySfxAt(clip, position);
        }
    }
}
