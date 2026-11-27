using LightShadow.Core;
using LightShadow.UI;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // Wing puzzle 1 (Docs/PUZZLES.md): the classroom chalkboard sums each hide
    // one rubbed-away digit; the four missing digits, read top to bottom, open
    // the lock on the teacher's desk. Solving slides the drawer and reveals a
    // battery. A wrong code squeals the chalk: WrongAttempt() costs Bright Idea
    // (GameConfig.puzzleWrongMeterPenalty) and raises GameEvents.PuzzleFailed,
    // the noise hook that can draw the Shadow (Granny-style risk).
    //
    // The whole prop (lock plate, drawer, hidden reward) sits under one root
    // that carries this component, so a ray hit on any part resolves to the
    // puzzle through InteractionRaycaster's GetComponentInParent.
    public class ChalkboardCipherPuzzle : PuzzleBase
    {
        [Header("Puzzle")]
        [Tooltip("Four digits, western numerals, top to bottom as the chalk lines read.")]
        public string code = "6371";
        [TextArea(2, 6)] public string sequenceEn = "7 × 8 = 5_\n9 × 4 = _6\n11 × 7 = 7_\n9 × 9 = 8_";
        // The sums stay in western numerals in both languages: the lock wheels
        // show 0-9, and the chalk arithmetic is language-neutral. The Arabic UI
        // around them (title, hints, feedback) is fully translated.
        [TextArea(2, 6)] public string sequenceAr = "7 × 8 = 5_\n9 × 4 = _6\n11 × 7 = 7_\n9 × 9 = 8_";

        [Header("World")]
        [Tooltip("Slides toward the player when the lock opens.")]
        public Transform drawer;
        [Tooltip("Inactive until the puzzle is solved; usually the battery inside the drawer.")]
        public GameObject rewardRoot;
        public AudioClip squealClip;
        public AudioClip openClip;

        // How far the drawer slides out of the desk front on solve.
        static readonly Vector3 DrawerOpenOffset = new Vector3(0f, 0f, -0.34f);

        protected override void OnInteracted(GameObject interactor)
        {
            CodeLockUIController ui = CodeLockUIController.Instance;
            if (ui == null || ui.IsOpen) return;

            ui.Show(
                displayNameEn, displayNameAr,
                sequenceEn, sequenceAr,
                code,
                NextHint,
                OnCodeAccepted,
                OnCodeRejected);
        }

        void OnCodeAccepted()
        {
            if (openClip == null) openClip = ProceduralSfx.Correct;
            if (openClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(openClip);
            Solve();
        }

        void OnCodeRejected()
        {
            if (squealClip == null) squealClip = ProceduralSfx.Wrong;
            if (squealClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(squealClip);
            WrongAttempt();
        }

        protected override void OnSolved()
        {
            OpenWorld();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowMessage(
                    Localization.Current == GameLanguage.Arabic
                        ? Localization.Present("انزلق الدرج مفتوحًا. بداخله بطارية ما زالت مشحونة.")
                        : "The drawer slides open. A battery, still holding a charge.",
                    4f);
            }
        }

        // Save restore: the drawer and the reward go straight to their solved
        // state - no sound, no message, no events (they fired in the original run).
        protected override void OnRestoredSolved()
        {
            OpenWorld();
        }

        void OpenWorld()
        {
            // Drawer and reward share the puzzle root as parent, so the same
            // offset slides both out of the desk front together.
            if (drawer != null) drawer.localPosition += DrawerOpenOffset;
            if (rewardRoot != null)
            {
                rewardRoot.transform.localPosition += DrawerOpenOffset;
                rewardRoot.SetActive(true);
            }
        }
    }
}
