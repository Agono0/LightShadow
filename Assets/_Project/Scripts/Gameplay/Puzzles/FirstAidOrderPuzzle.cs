using LightShadow.Core;
using LightShadow.UI;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // Wing puzzle 5 (plan, workstream 5): the infirmary's first-aid board. Five
    // first-aid steps hang on the cabinet board out of order; tapping them in
    // the right sequence (wash, pressure, clean, dress, raise and check) opens
    // the cabinet drawer. A step out of sequence snaps the board's clip shut
    // and runs the framework's WrongAttempt() (meter penalty + PuzzleFailed
    // noise hook).
    //
    // Payout: the plan's "opens the drawer with a fuse", paid as a real fuse
    // ITEM now that the item system exists (the builder wires a fuse prop with
    // an ItemPickup as rewardRoot; fuse boxes themselves still restore on
    // touch - the gate circuit consumes carried items).
    //
    // The cabinet, drawer and reward sit under one root that carries this
    // component, so the InteractionRaycaster resolves a hit on any part to
    // the puzzle.
    public class FirstAidOrderPuzzle : PuzzleBase
    {
        [Header("Puzzle")]
        [Tooltip("The five steps in CORRECT order; the overlay shows them scrambled.")]
        public string[] stepsEn =
        {
            "Wash your hands",
            "Press on the wound to stop the bleeding",
            "Clean the wound with clean water",
            "Dress it with a sterile bandage",
            "Raise the arm and check the bandage",
        };
        public string[] stepsAr =
        {
            "اغسل يديك",
            "اضغط على الجرح لإيقاف النزيف",
            "نظّف الجرح بماء نظيف",
            "ضمّده بضمادة معقمة",
            "ارفع الذراع وتفقّد الضمادة",
        };
        [Tooltip("Button order as shown on the board: indices into the steps arrays.")]
        public int[] displayOrder = { 2, 0, 4, 1, 3 };

        [Header("World")]
        [Tooltip("The cabinet's drawer; slides out on solve.")]
        public Transform drawer;
        [Tooltip("Inactive until solved; the battery in the drawer.")]
        public GameObject rewardRoot;
        [Tooltip("Drawer slide direction in the puzzle root's local space.")]
        public Vector3 drawerOpenOffset = new Vector3(0.55f, 0f, 0f);
        public AudioClip snapClip;
        public AudioClip openClip;

        protected override void OnInteracted(GameObject interactor)
        {
            OrderUIController ui = OrderUIController.Instance;
            if (ui == null || ui.IsOpen) return;

            ui.Show(
                displayNameEn, displayNameAr,
                stepsEn, stepsAr, displayOrder,
                NextHint,
                OnOrderComplete,
                OnStepOutOfOrder);
        }

        void OnOrderComplete()
        {
            if (openClip == null) openClip = ProceduralSfx.Correct;
            if (openClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(openClip);
            Solve();
        }

        void OnStepOutOfOrder()
        {
            if (snapClip == null) snapClip = ProceduralSfx.Wrong;
            if (snapClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(snapClip);
            WrongAttempt();
        }

        protected override void OnSolved()
        {
            OpenWorld();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowMessage(
                    Localization.Current == GameLanguage.Arabic
                        ? Localization.Present("انزلق درج الخزانة مفتوحًا. بداخله مصهر.")
                        : "The cabinet drawer slides open. Inside, a fuse.",
                    4f);
            }
        }

        // Save restore: drawer out, reward visible, no sound, message or events.
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
