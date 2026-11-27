using System;

namespace LightShadow.Core
{
    // Static event bus for the whole night. Gameplay raises here when something
    // story-worthy happens, and narrative/audio/UI subscribe without any of the
    // three knowing about the others (premium-pass decision D4/D5). Everything is
    // raised with null-conditional guards so an event with no listeners costs nothing.
    //
    // The contract below is frozen: the Narrative agent codes against these exact
    // names. Add new events freely, never rename or remove these.
    public static class GameEvents
    {
        // (collected, total). Total is GameManager.FragmentTarget = 6.
        public static event Action<int, int> FragmentCollected;

        // (restored, total). Total is GameManager.FuseTarget = 3.
        public static event Action<int, int> FuseRestored;

        // All three fuse boxes are live and the exit gate has power.
        public static event Action GatePowered;

        // The Shadow caught Nour (before the checkpoint fade).
        public static event Action PlayerCaught;

        // A lit-zone checkpoint was claimed. Payload is the chalk line / zone name.
        public static event Action<string> CheckpointReached;

        // A quiz question was answered. Payload: was the answer correct?
        public static event Action<bool> QuestionAnswered;

        // Bright Idea Meter moved. Payload: normalized 0-1.
        public static event Action<float> MeterChanged;

        // The dawn chase begins (DawnRiser wakes the horizon).
        public static event Action DawnStarted;

        // Nour escaped through the gate; GameManager.Escape() runs the ending.
        public static event Action EscapeStarted;

        // The Shadow changed FSM state. Payload: (int)LightShadow.Enemy.EnemyBehaviour.State
        // (Patrol 0, Stalk 1, Chase 2, Repelled 3, Investigate 4, ForcedCooldown 5,
        // Search 6). Raised from EnemyBehaviour.EnterState.
        public static event Action<int> EnemyStateChanged;

        // A wing puzzle was solved. Payload: puzzle id (PuzzleBase.puzzleId).
        public static event Action<string> PuzzleSolved;

        // A wing puzzle registered a wrong move. Payload: puzzle id. The noise
        // bus hooks here so mistakes can be heard.
        public static event Action<string> PuzzleFailed;

        // The player asked for a puzzle hint. Payload: puzzle id.
        public static event Action<string> PuzzleHintUsed;
        // The player made a noise the Shadow may hear (P3 noise bus). Payload:
        // the hearing radius in metres (GameConfig noise fields). The listener
        // decides what to do; NoiseEmitter is the usual raiser.
        public static event Action<float> NoiseEmitted;

        // Calm moved (P3, plan 4.7). Payload: normalized 0-1 where 0 is the
        // worst calm and 1 the best; the story stat lives on the -1..1 scale
        // and maps (calm + 1) / 2. Raised by StoryDirector on every change.
        public static event Action<float> CalmChanged;

        // ---- Raise helpers -----------------------------------------------------
        // Each one-liner guards its event so callers never need to.

        public static void RaiseFragmentCollected(int collected, int total)
        {
            FragmentCollected?.Invoke(collected, total);
        }

        public static void RaiseFuseRestored(int restored, int total)
        {
            FuseRestored?.Invoke(restored, total);
        }

        public static void RaiseGatePowered()
        {
            GatePowered?.Invoke();
        }

        public static void RaisePlayerCaught()
        {
            PlayerCaught?.Invoke();
        }

        public static void RaiseCheckpointReached(string displayName)
        {
            CheckpointReached?.Invoke(displayName);
        }

        public static void RaiseQuestionAnswered(bool correct)
        {
            QuestionAnswered?.Invoke(correct);
        }

        public static void RaiseMeterChanged(float normalized)
        {
            MeterChanged?.Invoke(normalized);
        }

        public static void RaiseDawnStarted()
        {
            DawnStarted?.Invoke();
        }

        public static void RaiseEscapeStarted()
        {
            EscapeStarted?.Invoke();
        }

        public static void RaiseEnemyStateChanged(int state)
        {
            EnemyStateChanged?.Invoke(state);
        }

        public static void RaisePuzzleSolved(string puzzleId)
        {
            PuzzleSolved?.Invoke(puzzleId);
        }

        public static void RaisePuzzleFailed(string puzzleId)
        {
            PuzzleFailed?.Invoke(puzzleId);
        }

        public static void RaisePuzzleHintUsed(string puzzleId)
        {
            PuzzleHintUsed?.Invoke(puzzleId);
        public static void RaiseNoiseEmitted(float radius)
        {
            NoiseEmitted?.Invoke(radius);
        }

        public static void RaiseCalmChanged(float normalized)
        {
            CalmChanged?.Invoke(normalized);
        }
    }
}
