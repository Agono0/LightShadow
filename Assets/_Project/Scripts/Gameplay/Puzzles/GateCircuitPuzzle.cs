using LightShadow.Core;
using LightShadow.UI;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // Finale puzzle (plan, workstream 5): the gate console's wiring board.
    // Once all three fuse boxes hum, the gate still needs its feeds routed:
    // three wing feeds (library, science, main hall) into three bus terminals
    // (gate motor, dawn lights, gate crank). The console panel itself is
    // locked - it opens with the staff-room master key (wing puzzle 4), so
    // the finale needs an item from an earlier chapter, per the plan.
    //
    // Every right connection locks with a clunk; a wrong one sparks and runs
    // the framework's WrongAttempt() (meter penalty + PuzzleFailed noise
    // hook). Routing all three powers the gate through ExitGate.PowerFromPuzzle
    // (crank animation, escape zone, dawn chase lights).
    //
    // The console box sits under the gate root that also carries this
    // component, so the InteractionRaycaster resolves a hit to the puzzle.
    public class GateCircuitPuzzle : PuzzleBase
    {
        [Header("Puzzle")]
        [Tooltip("Feed labels, in panel order.")]
        public string[] feedsEn = { "Library feed", "Science feed", "Main hall feed" };
        public string[] feedsAr = { "تغذية المكتبة", "تغذية المختبر", "تغذية القاعة" };
        [Tooltip("Bus terminal labels, in panel order.")]
        public string[] terminalsEn = { "Gate motor", "Dawn lights", "Gate crank" };
        public string[] terminalsAr = { "محرك البوابة", "أضواء الفجر", "ذراع البوابة" };
        [Tooltip("Terminal index per feed, matching the arrays above.")]
        public int[] mapping = { 1, 2, 0 };

        [Header("Wiring")]
        [Tooltip("The gate this console powers.")]
        public ExitGate gate;
        [Tooltip("Puzzle id whose solve stands in for holding the master key.")]
        public string masterKeyPuzzleId = "Puzzle_MapLocker";
        public AudioClip clunkClip;
        public AudioClip sparkClip;

        // The console answers even before the fuses are restored, so it can
        // explain itself; ExitGate keeps its own "three fuse boxes" message
        // for the gate leaf.
        public override bool CanInteract => State == PuzzleState.Available;

        public bool Solved => State == PuzzleState.Solved;

        protected override void OnInteracted(GameObject interactor)
        {
            GameManager gm = GameManager.Instance;
            if (gm == null || !gm.CanPowerGate)
            {
                if (gm != null)
                {
                    gm.ShowMessage(
                        Localization.Current == GameLanguage.Arabic
                            ? Localization.Present("لوحة القواطع ميتة. ثلاثة صناديق مصاهر أولًا.")
                            : "The breaker panel is dead. Three fuse boxes first.",
                        4f);
                }
                return;
            }

            if (PuzzleManager.Instance != null && !PuzzleManager.Instance.IsSolved(masterKeyPuzzleId))
            {
                gm.ShowMessage(
                    Localization.Current == GameLanguage.Arabic
                        ? Localization.Present("غطاء اللوحة مقفل. مفتاح غرفة المعلمين الرئيسي سيفتحه.")
                        : "The panel cover is locked. The staff room master key would open it.",
                    4f);
                return;
            }

            CircuitUIController ui = CircuitUIController.Instance;
            if (ui == null || ui.IsOpen) return;

            ui.Show(
                displayNameEn, displayNameAr,
                feedsEn, feedsAr, terminalsEn, terminalsAr, mapping,
                NextHint,
                OnAllRouted,
                OnWrongPair);
        }

        void OnAllRouted()
        {
            Solve();
        }

        void OnWrongPair()
        {
            if (sparkClip == null) sparkClip = ProceduralSfx.Wrong;
            if (sparkClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(sparkClip);
            WrongAttempt();
        }

        protected override void OnSolved()
        {
            if (clunkClip == null) clunkClip = ProceduralSfx.Correct;
            if (clunkClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(clunkClip);
            if (gate != null) gate.PowerFromPuzzle();
        }

        // Save restore: if the night was saved with the gate already powered,
        // ExitGate.PowerFromSave rebuilds that state; nothing to redo here.
        protected override void OnRestoredSolved() { }
    }
}
