using LightShadow.Contrib.Abdelraouf.Overlay;
using LightShadow.Player;
using LightShadow.UI;
using UnityEngine;

namespace LightShadow.Core
{
    // Safety net for the desktop cursor. In gameplay with no overlay, pause
    // menu, question or close-up open, the cursor must be locked and hidden.
    // If anything left it free (playtest 2026-11-28: after closing a
    // collected item's close-up the cursor stayed on screen and look and move
    // stopped), this puts it back. It never touches the cursor while any
    // overlay holds it, and never on mobile.
    public class CursorGuard : MonoBehaviour
    {
        const float GraceSeconds = 0.35f;
        float freeSince = -1f;

        // Pure decision, tested without a scene.
        public static bool ShouldRelock(bool mobile, bool inGameplay, bool anyOverlayLocked,
            bool pauseOpen, bool questionOpen, bool inspectOpen, bool cursorFree)
        {
            if (mobile || !inGameplay) return false;
            if (anyOverlayLocked || pauseOpen || questionOpen || inspectOpen) return false;
            return cursorFree;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Ensure()
        {
            if (FindFirstObjectByType<CursorGuard>() != null) return;
            GameObject host = new GameObject("CursorGuard");
            host.AddComponent<CursorGuard>();
            // Scenes load in single mode (Bootstrap, menu, school): without this the
            // guard died with the first scene and never ran in gameplay.
            DontDestroyOnLoad(host);
        }

        void Update()
        {
            GameManager gm = GameManager.Instance;
            bool inGameplay = gm != null;
            bool questionOpen = gm != null && gm.quizUI != null && gm.quizUI.IsOpen;
            bool cursorFree = Cursor.lockState != CursorLockMode.Locked || Cursor.visible;

            bool wrong = ShouldRelock(Application.isMobilePlatform, inGameplay, OverlayStack.AnyLocked,
                PauseMenu.IsOpen, questionOpen, InspectViewController.IsOpen, cursorFree);

            if (!wrong) { freeSince = -1f; return; }
            if (freeSince < 0f) freeSince = Time.unscaledTime;
            if (Time.unscaledTime - freeSince < GraceSeconds) return;

            // End-of-game screens free the cursor on purpose without an overlay
            // token, and only exist outside the playable scene. Leave those alone.
            if (FindFirstObjectByType<PlayerController>() == null
                || FindFirstObjectByType<RecapScreen>() != null
                || FindFirstObjectByType<EndingController>() != null)
            {
                freeSince = -1f;
                return;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            freeSince = -1f;
        }
    }
}
