using LightShadow.Core;
using LightShadow.UI;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // Wing puzzle 6 (plan, workstream 5): the stage curtain-and-lights rig in
    // the main hall. The rig plays a pattern of colors; the player repeats it
    // on the switch panel. Repeating the full pattern opens the electrician's
    // hatch on the stage lip, where a charged battery waits. A wrong switch
    // buzzes and runs the framework's WrongAttempt() (meter penalty +
    // PuzzleFailed noise hook - the plan has the Shadow patrolling the aisles
    // during this one, so mistakes carry risk).
    //
    // The rig lights, switch panel, hatch and reward all sit under one root
    // that carries this component, so the InteractionRaycaster resolves a hit
    // on any part to the puzzle.
    public class StageLightsPuzzle : PuzzleBase
    {
        [Header("Puzzle")]
        [Tooltip("Switch color names, matching the rig's lamps left to right.")]
        public string[] colorsEn = { "Red", "Amber", "Green", "Blue" };
        public string[] colorsAr = { "أحمر", "كهرمان", "أخضر", "أزرق" };
        [Tooltip("The pattern to repeat: switch indices, played in order.")]
        public int[] sequence = { 0, 2, 1, 3, 2, 0 };

        [Header("World")]
        [Tooltip("The electrician's hatch on the stage lip; slides aside on solve.")]
        public Transform hatch;
        [Tooltip("Inactive until solved; the battery in the hatch.")]
        public GameObject rewardRoot;
        [Tooltip("Hatch slide direction in the puzzle root's local space.")]
        public Vector3 hatchOpenOffset = new Vector3(0.5f, 0f, 0f);
        public AudioClip chimeClip;
        public AudioClip buzzClip;

        protected override void OnInteracted(GameObject interactor)
        {
            SequenceUIController ui = SequenceUIController.Instance;
            if (ui == null || ui.IsOpen) return;

            ui.Show(
                displayNameEn, displayNameAr,
                colorsEn, colorsAr, sequence,
                NextHint,
                OnPatternRepeated,
                OnWrongSwitch);
        }

        void OnPatternRepeated()
        {
            if (chimeClip == null) chimeClip = ProceduralSfx.Correct;
            if (chimeClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(chimeClip);
            Solve();
        }

        void OnWrongSwitch()
        {
            if (buzzClip == null) buzzClip = ProceduralSfx.Wrong;
            if (buzzClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(buzzClip);
            WrongAttempt();
        }

        protected override void OnSolved()
        {
            OpenWorld();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowMessage(
                    Localization.Current == GameLanguage.Arabic
                        ? "اهتزت فتحة الكهربائي مفتوحة على حافة المسرح - بداخلها بطارية."
                        : "The electrician's hatch rattles open on the stage lip - a battery inside.",
                    4f);
            }
        }

        // Save restore: hatch aside, battery visible, no sound, message or events.
        protected override void OnRestoredSolved()
        {
            OpenWorld();
        }

        void OpenWorld()
        {
            // Like the locker puzzle, only the hatch moves: the battery sits in
            // the compartment and is revealed.
            if (hatch != null) hatch.localPosition += hatchOpenOffset;
            if (rewardRoot != null) rewardRoot.SetActive(true);
        }
    }
}
