using LightShadow.Core;
using LightShadow.Player;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // Base class for every wing puzzle (finish-the-game plan, workstream 5).
    // A puzzle is a world object the player interacts with through the existing
    // InteractionRaycaster (E on desktop, the Interact touch button on mobile),
    // so every subclass is mobile-ready by construction.
    //
    // Flow: Locked | Available -> the player interacts -> the subclass runs its
    // UI/logic -> Solve() or WrongAttempt(). Solving raises GameEvents.PuzzleSolved
    // and records the id with the PuzzleManager, which carries solved ids across
    // saves. Wrong moves raise GameEvents.PuzzleFailed - the stable hook the
    // noise bus listens to once it lands, so a mistake can summon the Shadow
    // (Granny-style risk, per the plan).
    //
    // Localization: subclasses fill EN and AR fields side by side; Arabic picks
    // the AR field when set and falls back to EN otherwise (repo quality bar:
    // every user-facing string exists in both languages).
    public abstract class PuzzleBase : MonoBehaviour, IInteractable
    {
        public enum PuzzleState { Locked, Available, Solved }

        [Header("Identity")]
        [Tooltip("Stable id used in events and saves, e.g. classroom_cipher. Defaults to the object name.")]
        public string puzzleId = "";
        public string displayNameEn = "Puzzle";
        public string displayNameAr = "";

        [Header("Hints (progressive, localized)")]
        [TextArea] public string[] hintsEn = new string[0];
        [TextArea] public string[] hintsAr = new string[0];

        [Header("Wiring")]
        public GameConfig config;

        public PuzzleState State { get; private set; } = PuzzleState.Available;

        int hintIndex;

        public string DisplayName =>
            Localization.Current == GameLanguage.Arabic && !string.IsNullOrEmpty(displayNameAr)
                ? displayNameAr
                : displayNameEn;

        // IInteractable: a solved puzzle stops prompting; a locked one says why.
        public virtual string Prompt => State == PuzzleState.Available ? DisplayName : "";
        public virtual bool CanInteract => State == PuzzleState.Available;

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(puzzleId)) puzzleId = name;
            if (config == null && GameManager.Instance != null) config = GameManager.Instance.config;
        }

        protected virtual void Start()
        {
            if (PuzzleManager.Instance != null) PuzzleManager.Instance.Register(this);
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract) return;
            OnInteracted(interactor);
        }

        // The subclass opens its UI / runs its step. Input locking while a
        // puzzle UI is open follows the story system's pattern
        // (PlayerController.ExternalInputLocked), set by the subclass's UI.
        protected abstract void OnInteracted(GameObject interactor);

        protected void Solve()
        {
            if (State == PuzzleState.Solved) return;
            State = PuzzleState.Solved;
            if (PuzzleManager.Instance != null) PuzzleManager.Instance.MarkSolved(puzzleId);
            GameEvents.RaisePuzzleSolved(puzzleId);
            OnSolved();
        }

        // Rewards (gate key, fuse, battery) wire up here per puzzle.
        protected virtual void OnSolved() { }

        protected void WrongAttempt()
        {
            GameEvents.RaisePuzzleFailed(puzzleId);
            BrightIdeaMeter meter = GameManager.Instance != null ? GameManager.Instance.GetMeter() : null;
            float penalty = config != null ? config.puzzleWrongMeterPenalty : 0f;
            if (meter != null && penalty > 0f) meter.RemoveMeter(penalty);
            OnWrongAttempt();
        }

        protected virtual void OnWrongAttempt() { }

        // Progressive hints: each call returns the next hint and never runs past
        // the last one. Arabic pool wins when filled; otherwise English.
        public string NextHint()
        {
            string[] pool = Localization.Current == GameLanguage.Arabic && hintsAr != null && hintsAr.Length > 0
                ? hintsAr
                : hintsEn;
            if (pool == null || pool.Length == 0) return "";
            string hint = pool[Mathf.Min(hintIndex, pool.Length - 1)];
            if (hintIndex < pool.Length - 1) hintIndex++;
            GameEvents.RaisePuzzleHintUsed(puzzleId);
            return Localization.Present(hint);
        }

        public void Lock()
        {
            if (State != PuzzleState.Solved) State = PuzzleState.Locked;
        }

        public void Unlock()
        {
            if (State == PuzzleState.Locked) State = PuzzleState.Available;
        }

        // Save restore, called by the PuzzleManager when the id is in the blob.
        public void RestoreSolved()
        {
            if (State == PuzzleState.Solved) return;
            State = PuzzleState.Solved;
            OnRestoredSolved();
        }

        // Rebuild just the world state a solved puzzle implies (open drawer,
        // taken item). No events, no rewards: those already fired in the run
        // the save came from.
        protected virtual void OnRestoredSolved() { }
    }
}
