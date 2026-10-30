using UnityEngine;

namespace LightShadow.Systems
{
    // Facade over the ObjectPool for the two effects wired this pass
    // (premium pass P2): the flashlight answer-flare and the Shadow's whisper
    // burst. Everything is lazy - templates build on first use, pools preload
    // a few instances, and each spawn is one pool Get with no allocation.
    //
    // Flip Enabled=false if a reviewer wants the effects off entirely; every
    // spawn site then becomes a no-op and the validated behaviour is unchanged.
    public static class PooledFx
    {
        public static bool Enabled = true;

        const int FlashPreload = 4;
        const int WhisperPreload = 4;

        static PooledFlashEffect flashTemplate;
        static PooledWhisperBurst whisperTemplate;

        // A short light flare at a world position (answer rewards, gate power).
        public static void SpawnFlash(Vector3 position, Color color, float seconds = 0f)
        {
            if (!Enabled) return;
            EnsureFlashTemplate();
            if (flashTemplate == null) return;

            PooledFlashEffect effect = ObjectPool.Get(flashTemplate, position, Quaternion.identity, null);
            if (effect != null) effect.Play(color, seconds);
        }

        // A spatial whisper burst at a world position (chase stinger accent).
        public static void SpawnWhisper(Vector3 position)
        {
            if (!Enabled) return;
            EnsureWhisperTemplate();
            if (whisperTemplate == null) return;

            PooledWhisperBurst burst = ObjectPool.Get(whisperTemplate, position, Quaternion.identity, null);
            if (burst != null) burst.Play();
        }

        // ---- Lazy templates (runtime-created, no scene assets) ----------------
        // Templates live under the persistent pool root and stay INACTIVE: the
        // pool clones them, and an inactive template can never leak a light or
        // a sound into the visible scene.

        static void EnsureFlashTemplate()
        {
            if (flashTemplate != null) return;
            GameObject go = new GameObject("FX_FlashTemplate");
            go.transform.SetParent(ObjectPool.Root, false);
            go.AddComponent<PooledFlashEffect>();
            go.SetActive(false);
            flashTemplate = go.GetComponent<PooledFlashEffect>();
            ObjectPool.Preload(flashTemplate, FlashPreload);
        }

        static void EnsureWhisperTemplate()
        {
            if (whisperTemplate != null) return;
            GameObject go = new GameObject("FX_WhisperTemplate");
            go.transform.SetParent(ObjectPool.Root, false);
            go.AddComponent<PooledWhisperBurst>();
            go.SetActive(false);
            whisperTemplate = go.GetComponent<PooledWhisperBurst>();
            ObjectPool.Preload(whisperTemplate, WhisperPreload);
        }
    }
}
