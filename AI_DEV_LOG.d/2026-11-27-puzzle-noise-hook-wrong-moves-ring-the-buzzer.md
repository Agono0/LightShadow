# Puzzle noise hook: wrong moves ring the buzzer (2026-11-27)

The plan's noise hook for puzzle wrong moves (4.12): PuzzleBase already
raised `GameEvents.PuzzleFailed(puzzleId)` on every wrong move, but nothing
listened, so mistakes were silent and the Shadow never reacted. NoiseEmitter
now subscribes PuzzleFailed alongside QuestionAnswered and rings the same
buzzer radius (`config.wrongAnswerNoiseRange`, 6 m default) through the
existing noise bus. One listener, no per-puzzle wiring: every wrong move in
every wing puzzle is now audible to the Shadow's FSM.
