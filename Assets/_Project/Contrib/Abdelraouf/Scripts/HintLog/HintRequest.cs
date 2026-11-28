using LightShadow.Contrib.Abdelraouf.Hints;
using LightShadow.Core;
using LightShadow.Player;
using LightShadow.UI;
using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.HintLog
{
    // The one entry point for "ask for a hint" from touch, from a key, or from
    // the owner's side (for example a gamepad button). It calls the existing
    // HintService.RequestHint() and adds nothing to the hint rules: cooldown,
    // nearest-puzzle choice and level progression stay in HintService.
    //
    // KEY BINDING: nothing here hard-codes a key. HintService already reads
    // its own public hintKey field (default H on main, which clashes with the
    // medkit key); the owner's side rebinds that field or sets RequestKey
    // below. RequestKey defaults to None, so this module adds no keyboard
    // request by default. If RequestKey equals HintService.hintKey both fire
    // in one frame; the cooldown in HintService makes the second a no-op.
    public static class HintRequest
    {
        // Optional extra request key, read by HintLogInput. None = disabled.
        public static KeyCode RequestKey = KeyCode.None;

        // True when a hint was shown. When none was shown a short message
        // explains the two reasons HintService can refuse (it does not say
        // which; see Docs).
        public static bool TryRequest()
        {
            HintService service = HintService.Instance;
            if (service == null) return false;
            if (PlayerController.ExternalInputLocked || PauseMenu.IsOpen) return false;
            if (!HintService.Enabled) return false;

            bool shown = service.RequestHint();
            if (!shown && GameManager.Instance != null)
            {
                bool arabic = Localization.Current == GameLanguage.Arabic;
                string text = arabic
                    ? Localization.Present("لا يوجد تلميح الآن. اقترب من لغز أو انتظر قليلًا.")
                    : "No hint right now. Get closer to a puzzle or wait a moment.";
                GameManager.Instance.ShowMessage(text, 2.5f);
            }
            return shown;
        }
    }
}
