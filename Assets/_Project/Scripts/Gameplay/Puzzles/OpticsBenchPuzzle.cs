using LightShadow.Core;
using LightShadow.UI;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // Wing puzzle 3 (plan, workstream 5): the science-lab optics bench. Two
    // mirrors and a prism sit on the bench between a torch emitter and a
    // sensor. Each element rotates in 90-degree steps from the overlay; when
    // all three match the bench's calibration the beam lands on the sensor,
    // the lab cabinet's drawer slides open, and a charged battery rides out.
    // A wrong test fizzles at the emitter and runs the framework's
    // WrongAttempt() (meter penalty + PuzzleFailed noise hook).
    //
    // The bench rig, drawer and reward all sit under one root that carries
    // this component, so the InteractionRaycaster resolves a hit on any part
    // to the puzzle.
    public class OpticsBenchPuzzle : PuzzleBase
    {
        [Header("Puzzle")]
        [Tooltip("Element names on the bench, in beam order.")]
        public string[] elementsEn = { "Mirror A", "Prism", "Mirror B" };
        public string[] elementsAr = { "المرآة أ", "المنشور", "المرآة ب" };
        [Tooltip("Solved orientation per element, in 90-degree steps (0-3).")]
        public int[] solution = { 1, 2, 3 };

        [Header("World")]
        [Tooltip("The lab cabinet's drawer; slides out on solve.")]
        public Transform drawer;
        [Tooltip("Inactive until solved; the battery in the drawer.")]
        public GameObject rewardRoot;
        [Tooltip("Drawer slide direction in the puzzle root's local space.")]
        public Vector3 drawerOpenOffset = new Vector3(-0.55f, 0f, 0f);
        public AudioClip alignClip;
        public AudioClip fizzleClip;

        protected override void OnInteracted(GameObject interactor)
        {
            OpticsUIController ui = OpticsUIController.Instance;
            if (ui == null || ui.IsOpen) return;

            ui.Show(
                displayNameEn, displayNameAr,
                elementsEn, elementsAr, solution,
                NextHint,
                OnBeamLanded,
                OnBeamFizzled);
        }

        void OnBeamLanded()
        {
            if (alignClip == null) alignClip = ProceduralSfx.Correct;
            if (alignClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(alignClip);
            Solve();
        }

        void OnBeamFizzled()
        {
            if (fizzleClip == null) fizzleClip = ProceduralSfx.Wrong;
            if (fizzleClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(fizzleClip);
            WrongAttempt();
        }

        protected override void OnSolved()
        {
            OpenWorld();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowMessage(
                    Localization.Current == GameLanguage.Arabic
                        ? Localization.Present("أصاب الشعاع المستشعر - انزلق درج الخزانة مفتوحًا.")
                        : "The beam hits the sensor - the lab cabinet's drawer slides open.",
                    4f);
            }
        }

        // Save restore: drawer out, reward visible, no sound, no message, no events.
        protected override void OnRestoredSolved()
        {
            OpenWorld();
        }

        void OpenWorld()
        {
            // Drawer and reward share the puzzle root as parent, so one offset
            // slides both out of the cabinet front together.
            if (drawer != null) drawer.localPosition += drawerOpenOffset;
            if (rewardRoot != null)
            {
                rewardRoot.transform.localPosition += drawerOpenOffset;
                rewardRoot.SetActive(true);
            }
        }
    }
}
