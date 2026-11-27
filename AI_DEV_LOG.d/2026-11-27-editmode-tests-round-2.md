# 2026-11-27 — EditMode tests round 2: puzzles, run results, config sanity

Package 7 of the UsefBebars work (Docs/WORK_CLAIMS.md row added in this PR).
Test files only, all under Assets/_Project/Tests/Editor/. No production file,
scene, prefab, ProjectSettings, Packages or workflow touched.

New files (each file header names the gap it covers and its limits):
- PuzzleBaseTests.cs (12 tests): state machine (Available/Locked/Solved), Solve
  fires PuzzleSolved once, WrongAttempt fires PuzzleFailed, RestoreSolved raises
  no event and runs no reward hook, progressive hints clamp on the last one,
  Arabic hint/name fallback to English. Uses a small test subclass in the file.
- PuzzleManagerTests.cs (9 tests): solved-id set, puzzleJson capture/apply round
  trip, apply replaces (not merges), null/empty/corrupt blob, restore on Register.
- RunResultsTests.cs (5 tests): CopyFrom copies, replaces, is not an alias, null
  clears; Reset clears.
- GameConfigSanityTests.cs (2 tests): GameConfig.asset loads; puzzleWrongMeterPenalty >= 0.

Not done on purpose:
- PuzzleDataSweep (instantiating each shipped puzzle): Unity does not run Awake for
  plain MonoBehaviours in EditMode, so an Awake-safety check cannot be made in
  EditMode; skipped as agreed.
- The six concrete wing puzzles: their solve logic sits inside OnInteracted with
  scene/UI wiring, so it needs a production seam (Scripts is not our lane).
- WrongAttempt's Bright Idea Meter penalty branch needs a live GameManager.

Unverified: no Unity editor or C# compiler available to the author. Nothing was
compiled or run locally. CI fast-checks and test-editmode are the first real run;
if a test fails there, treat the test as wrong before the production code.
