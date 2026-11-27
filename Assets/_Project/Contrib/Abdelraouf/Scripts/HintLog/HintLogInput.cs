using LightShadow.Core;
using LightShadow.Player;
using LightShadow.UI;
using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.HintLog
{
    // Input polling for the Hint Recall screen. Approved mapping (2026-11-27):
    // L toggles the screen; inside it Up/Down arrows pick a puzzle and L
    // closes. Esc is NOT consumed here: the owner's side owns Esc close-first
    // and modal ordering and can call HintLogUI.Close() first.
    //
    // Hint request key: not hard-coded. RequestHintPressed() is virtual and
    // reads HintRequest.RequestKey (default None, so no key does anything
    // until the owner's side sets one). HintService keeps reading its own
    // hintKey field as before; the owner rebinds that field or sets
    // HintRequest.RequestKey. Do not set both to the same key (harmless, the
    // cooldown absorbs the second request, but pointless).
    public class HintLogInput : MonoBehaviour
    {
        void Awake()
        {
            if (GetComponent<HintLogUI>() == null) gameObject.AddComponent<HintLogUI>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                if (HintLogUI.IsOpen) HintLogUI.Instance.Close();
                else if (CanOpenNow()) HintLogUI.EnsureExists().Open();
                return;
            }

            if (HintLogUI.IsOpen)
            {
                HintLogUI ui = HintLogUI.Instance;
                if (ui == null) return;
                if (Input.GetKeyDown(KeyCode.UpArrow)) ui.MoveSelection(-1);
                if (Input.GetKeyDown(KeyCode.DownArrow)) ui.MoveSelection(1);
                return;
            }

            if (RequestHintPressed()) HintRequest.TryRequest();
        }

        // Override or set HintRequest.RequestKey to bind a keyboard request.
        public virtual bool RequestHintPressed()
        {
            KeyCode key = HintRequest.RequestKey;
            return key != KeyCode.None && Input.GetKeyDown(key);
        }

        // Refuse to open while anything else owns input or an overlay is up.
        static bool CanOpenNow()
        {
            if (PlayerController.ExternalInputLocked) return false;
            if (PauseMenu.IsOpen || InspectViewController.IsOpen) return false;
            GameManager gm = GameManager.Instance;
            if (gm == null) return false;
            if (gm.quizUI != null && gm.quizUI.IsOpen) return false;
            return true;
        }
    }
}
