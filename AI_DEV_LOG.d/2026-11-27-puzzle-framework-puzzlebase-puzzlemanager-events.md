### 2026-11-27 — Puzzle framework (PuzzleBase, PuzzleManager, events, save blob) — Abdelrhman Mohamed
- Tool used: Instinct (AI coding agent)
- What AI provided: the framework design and code — `PuzzleBase` (states, hints EN/AR,
  solve/wrong-attempt flow, restore hooks), `PuzzleManager` (registry + save bridge over
  the SaveCaptured/SaveApplied events), additive `GameEvents` puzzle events, the
  `GameConfig.puzzleWrongMeterPenalty` field, and `Docs/PUZZLES.md` with the seven-puzzle plan.
- What I changed myself: pending review — lands through the PR workflow.
- Teach-back check: pending — Docs/PUZZLES.md explains the restore-timing choice
  (manager keeps the id set; puzzles pick it up when they register).
- Test result: static only; CI build workflow is the compile gate. No scene changes.
