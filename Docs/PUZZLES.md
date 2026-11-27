# LIGHT SHADOW — Puzzle framework (workstream 5 of the finish-the-game plan)

**Date:** 2026-11-27 · **Status:** framework landed, wing puzzles queued

Quiz boards stay as they are. On top of them every wing gets a real puzzle
that uses its school subject, pays out something the player needs, and can be
heard by the Shadow when the player fumbles (Granny-style risk).

## Framework (this PR)

- `Scripts/Gameplay/Puzzles/PuzzleBase.cs` — abstract `IInteractable`: states
  (Locked/Available/Solved), progressive localized hints (EN + AR fields),
  `Solve()` / `WrongAttempt()` flow, save restore hooks. Interaction goes
  through the existing `InteractionRaycaster`, so keyboard and touch both work.
- `Scripts/Gameplay/Puzzles/PuzzleManager.cs` — scene registry + save bridge.
  Solved ids ride `SaveData.puzzleJson` via the same capture/apply events the
  story system uses. A fresh scene means a fresh puzzle state, so new games
  are clean.
- `GameEvents`: `PuzzleSolved(id)`, `PuzzleFailed(id)`, `PuzzleHintUsed(id)`,
  additive only (the frozen contract keeps its names).
- `GameConfig.puzzleWrongMeterPenalty` — optional Bright Idea Meter penalty
  per wrong move (0 = off; rule 2: no literals).
- Noise: `PuzzleFailed` is the stable hook the noise bus (`NoiseEmitter`, in
  flight with the premium-integration PR) subscribes to for wrong-move noise.

## The seven puzzles (next PRs, two per batch)

1. **Classroom — chalkboard cipher (Math).** Number sequence on the board gives
   a 4-digit desk-lock code. Wrong tries make the chalk squeal.
2. **Library — card catalogue sort (Language/History).** Sort index cards by
   clue into the right drawers; the hidden key card drops.
3. **Science lab — optics bench (Science).** Rotate mirrors and a prism to route
   the beam onto a sensor; the cabinet opens with a battery.
4. **Staff room — map and locker (Geography).** Pins on the wall map plus a note
   give a country order that opens the master-key locker.
5. **Infirmary — first-aid order (Health).** Put the first-aid steps in order on
   the cabinet board; the drawer holds a fuse.
6. **Stage — curtain and lights (Memory).** Repeat the stage-light color
   pattern on the switch panel while the Shadow patrols the aisles.
7. **Finale — fuse and gate circuit (all subjects).** The three fuse boxes plus
   a wiring puzzle; items from earlier wings required, then the dawn chase.

## Working agreements

- Puzzle UI lives in the `SceneBuilderUI` partials like the rest of the
  interface (rule 1: no hand-edited scenes/prefabs).
- While a puzzle UI is open it holds the input lock the same way the story
  system does (`PlayerController.ExternalInputLocked`).
- Each wing puzzle PR names the finding IDs / plan rows it closes and says
  what was verified (CI compile only, until a human editor run happens).
