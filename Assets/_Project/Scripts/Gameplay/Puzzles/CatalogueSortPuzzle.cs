using LightShadow.Core;
using LightShadow.UI;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // Wing puzzle 2 (Docs/PUZZLES.md): the library card catalogue. Four index
    // cards, each naming a famous book, must be filed into the right subject
    // drawer (Poetry, Stories, History, Science). Every right card files with a
    // click; a wrong drawer rattles shut and runs the framework's
    // WrongAttempt() (meter penalty + PuzzleFailed noise hook). Filing all
    // four drops the return tray: a librarian's key card, and behind it a
    // charged battery.
    //
    // The cabinet, drawer fronts, tray and reward all sit under one root that
    // carries this component, so the InteractionRaycaster resolves a hit on
    // any part to the puzzle.
    public class CatalogueSortPuzzle : PuzzleBase
    {
        [Header("Puzzle")]
        [TextArea(1, 3)] public string[] cluesEn =
        {
            "The Mu'allaqa of Imru' al-Qais - \"Stop, let us weep\"",
            "The Days - Taha Hussein's memoir of his village school",
            "Al-Tabari's History - prophets, kings and caliphs",
            "The Canon of Medicine - Ibn Sina",
        };
        [TextArea(1, 3)] public string[] cluesAr =
        {
            "معلقة امرئ القيس - «قفا نبكِ»",
            "الأيام - سيرة طه حسين عن مدرسة قريته",
            "تاريخ الطبري - أنبياء وملوك وخلفاء",
            "القانون في الطب - ابن سينا",
        };
        [Tooltip("Drawer index per card, matching drawersEn/drawersAr.")]
        public int[] correctDrawers = { 0, 1, 2, 3 };
        public string[] drawersEn = { "Poetry", "Stories", "History", "Science" };
        public string[] drawersAr = { "الشعر", "القصص", "التاريخ", "العلوم" };

        [Header("World")]
        [Tooltip("The return tray; slides out of the cabinet front on solve.")]
        public Transform tray;
        [Tooltip("Inactive until solved; the battery in the return tray.")]
        public GameObject rewardRoot;
        [Tooltip("Tray slide direction in the puzzle root's local space.")]
        public Vector3 trayOpenOffset = new Vector3(0.34f, 0f, 0f);
        public AudioClip fileClip;
        public AudioClip rattleClip;
        public AudioClip dropClip;

        protected override void OnInteracted(GameObject interactor)
        {
            CardSortUIController ui = CardSortUIController.Instance;
            if (ui == null || ui.IsOpen) return;

            ui.Show(
                displayNameEn, displayNameAr,
                cluesEn, cluesAr, drawersEn, drawersAr, correctDrawers,
                NextHint,
                OnAllFiled,
                OnWrongDrawer);
        }

        void OnAllFiled()
        {
            if (dropClip == null) dropClip = ProceduralSfx.Correct;
            if (dropClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(dropClip);
            Solve();
        }

        void OnWrongDrawer()
        {
            if (rattleClip == null) rattleClip = ProceduralSfx.Wrong;
            if (rattleClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(rattleClip);
            WrongAttempt();
        }

        protected override void OnSolved()
        {
            OpenWorld();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowMessage(
                    Localization.Current == GameLanguage.Arabic
                        ? Localization.Present("سقطت بطاقة المكتبة في الصينية - وخلفها بطارية مشحونة.")
                        : "A librarian's key card drops into the tray - and behind it, a charged battery.",
                    4f);
            }
        }

        // Save restore: tray out, reward visible, no sound, no message, no events.
        protected override void OnRestoredSolved()
        {
            OpenWorld();
        }

        void OpenWorld()
        {
            // Tray and reward share the puzzle root as parent, so one offset
            // slides both out of the cabinet front together.
            if (tray != null) tray.localPosition += trayOpenOffset;
            if (rewardRoot != null)
            {
                rewardRoot.transform.localPosition += trayOpenOffset;
                rewardRoot.SetActive(true);
            }
        }
    }
}
