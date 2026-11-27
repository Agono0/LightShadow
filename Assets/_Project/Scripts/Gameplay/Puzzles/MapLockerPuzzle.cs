using LightShadow.Core;
using LightShadow.UI;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // Wing puzzle 4 (plan, workstream 5): the staff-room wall map and locker.
    // Six map pins each carry a country and a digit; the teacher's note names
    // her summer route city by city (Cairo, Beirut, Baghdad, Rabat). Reading
    // the matching countries' digits in route order gives the four-digit code
    // on the middle locker's lock. Solving slides the locker door aside and
    // reveals the master key. The staff-room key is paid separately by
    // Gameplay/PuzzleKeyRewards (and a spare lies outside the room: this locker
    // is INSIDE the staff room). A wrong code clanks the lock: WrongAttempt()
    // costs Bright Idea (GameConfig.puzzleWrongMeterPenalty) and raises
    // GameEvents.PuzzleFailed, the noise hook that can draw the Shadow.
    //
    // The lock plate, door and key sit under one root that carries this
    // component, so a ray hit on any part resolves to the puzzle through
    // InteractionRaycaster's GetComponentInParent.
    public class MapLockerPuzzle : PuzzleBase
    {
        [Header("Puzzle")]
        [Tooltip("Four digits, western numerals, in route order.")]
        public string code = "4729";
        [TextArea(2, 6)] public string noteEn =
            "Her summer route, the way she always told it:\nCairo first, then Beirut, then Baghdad, then Rabat.";
        [TextArea(2, 6)] public string noteAr =
            "طريق رحلتها الصيفية كما كانت ترويه دائمًا:\nالقاهرة أولًا، ثم بيروت، ثم بغداد، ثم الرباط.";

        [Header("World")]
        [Tooltip("The middle locker's door panel; slides aside on solve.")]
        public Transform door;
        [Tooltip("Inactive until solved; the master key inside the locker.")]
        public GameObject rewardRoot;
        [Tooltip("Door slide direction in the puzzle root's local space.")]
        public Vector3 doorOpenOffset = new Vector3(0f, 0f, 0.75f);
        public AudioClip clankClip;
        public AudioClip openClip;

        protected override void OnInteracted(GameObject interactor)
        {
            CodeLockUIController ui = CodeLockUIController.Instance;
            if (ui == null || ui.IsOpen) return;

            ui.Show(
                displayNameEn, displayNameAr,
                noteEn, noteAr,
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
            if (clankClip == null) clankClip = ProceduralSfx.Wrong;
            if (clankClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(clankClip);
            WrongAttempt();
        }

        protected override void OnSolved()
        {
            OpenWorld();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowMessage(
                    Localization.Current == GameLanguage.Arabic
                        ? "انزلق باب الخزانة جانبًا. بداخلها المفتاح الرئيسي."
                        : "The locker door slides aside. Inside, the master key.",
                    4f);
            }
        }

        // Save restore: door aside, key visible - no sound, no message, no
        // events (they fired in the original run).
        protected override void OnRestoredSolved()
        {
            OpenWorld();
        }

        void OpenWorld()
        {
            // Unlike the drawer puzzles, only the door moves: the key sits
            // inside the locker and is simply revealed.
            if (door != null) door.localPosition += doorOpenOffset;
            if (rewardRoot != null) rewardRoot.SetActive(true);
        }
    }
}
