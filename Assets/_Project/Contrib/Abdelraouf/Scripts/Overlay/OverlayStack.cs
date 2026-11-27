using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightShadow.Contrib.Abdelraouf.Overlay
{
    // Shared owner of "the player cannot move because an overlay is up".
    // Replaces the single static bool that every overlay set and cleared on its
    // own. Each system takes a token with Acquire and gives back only its own
    // with Release. PlayerController.ExternalInputLocked now reads this.
    //
    // Cleared only on scene unload and on respawn (ResetAll). Story reset does
    // not call it, so a live Journal token survives a story reset.
    public static class OverlayStack
    {
        static readonly OverlayLockSet set = new OverlayLockSet();
        static InputLockToken legacy;

        public const string LegacyOwner = "legacy-bool-setter";

        public static bool AnyLocked => set.Count > 0;
        public static int Count => set.Count;
        public static bool WantsFreeCursor => set.WantsFreeCursor;
        public static bool AnyDismissable => set.AnyDismissable;
        public static bool ClosedThisFrame => set.ClosedOnFrame(Time.frameCount);
        public static bool Holds(string owner) => set.Holds(owner);

        public static InputLockToken Acquire(string owner, bool wantsFreeCursor = false,
            bool dismissable = false, System.Action closeCallback = null)
        {
            return set.Acquire(owner, wantsFreeCursor, dismissable, closeCallback);
        }

        // Safe to call with null, a released token, or one dropped by ResetAll.
        public static void Release(ref InputLockToken token)
        {
            set.Release(token, Time.frameCount);
            token = null;
        }

        public static bool TryCloseTop()
        {
            return set.TryCloseTop();
        }

        // Respawn and scene unload only. invokeCallbacks lets visible overlays
        // close themselves first so no panel is left open with no lock.
        public static void ResetAll(bool invokeCallbacks)
        {
            legacy = null;
            int dropped = set.Clear(invokeCallbacks, Time.frameCount);
            if (dropped > 0) Debug.Log("[LightShadow] OverlayStack reset dropped " + dropped + " held lock(s).");
        }

        // Back-compat for the old PlayerController.ExternalInputLocked setter.
        // All unmigrated writers share one token, and "false" releases only
        // that token, never one held by a migrated overlay.
        public static void SetLegacyLock(bool locked)
        {
            if (locked)
            {
                if (legacy == null || !legacy.IsActive) legacy = set.Acquire(LegacyOwner, true, false, null);
            }
            else if (legacy != null)
            {
                set.Release(legacy, Time.frameCount);
                legacy = null;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Hook()
        {
            set.Clear(false, int.MinValue);
            legacy = null;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        // The overlays that held these tokens live in the scene being unloaded.
        static void OnSceneUnloaded(Scene scene)
        {
            ResetAll(false);
        }
    }
}
