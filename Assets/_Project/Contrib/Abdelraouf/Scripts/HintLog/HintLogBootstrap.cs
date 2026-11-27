using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.HintLog
{
    // Creates the hint log host (and, on mobile, the RequestHint button) after
    // each scene load so L works without scene wiring. The log refuses to open
    // outside gameplay (no GameManager). The owner's side may instead place
    // the host through HintLogLevelContribution; the Instance singletons keep
    // either route from duplicating.
    public static class HintLogBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Ensure()
        {
            HintLogUI ui = HintLogUI.EnsureExists();
            if (ui.GetComponent<HintLogInput>() == null) ui.gameObject.AddComponent<HintLogInput>();
            if (Application.isMobilePlatform) HintTouchButton.EnsureExists();
        }
    }
}
