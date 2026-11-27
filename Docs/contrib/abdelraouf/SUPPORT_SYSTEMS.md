# C2 hints and C3 sound captions (Abdelraouf lane)

Status: draft, infrastructure only. AI-prepared on behalf of Abdelraouf Zidan (GitHub Abdo1590). Not compiled in Unity, no editor run, no scene build, no device run. The Arabic text is an AI draft, not checked by a native speaker, and the Arabic hint text is copied from the Arabic already in `SceneBuilderPuzzles.cs`. Green CI is not proof that any of this works.

**Accessibility finding 1 (`Docs/reviews/accessibility-2026-11-27.md`) stays OPEN.** This PR ships the caption service and the display. Nothing in the game calls it yet. Simulated cues from the test rig do not prove live sound coverage. The finding can close only after the owner's side wires real audio call sites and someone verifies them in a run.

## What is in the PR (all under `Assets/_Project/Contrib/Abdelraouf/` unless noted)

- `Scripts/Hints/HintService.cs`, `HintEntry.cs`, `HintUI.cs` (C2)
- `Scripts/Captions/CaptionService.cs`, `CaptionCue.cs`, `CaptionUI.cs` (C3)
- `Scripts/Support/SupportBootstrap.cs`: adds both services under one object, plus the debug test rig
- `Editor/SupportLevelContribution.cs`: H2 module `abdelraouf.support-systems`, adds the object `Contrib_SupportSystems` to the school scene through the scene builder. No scene or prefab YAML is edited by hand.
- `Data/Hints/hints.json`, `Data/Captions/captions.json`
- Namespaces `LightShadow.Contrib.Abdelraouf.Hints` and `.Captions`. No asmdef. Hand-written `.meta` files with fresh GUIDs checked by grep.

## C2 hints

- Three levels per puzzle: 1 nudge, 2 clue, 3 answer. Each trigger moves up one level and never past 3.
- Triggers: the H key, or `idleSeconds` (75) without player input. `cooldownSeconds` (20) between any two hints. Both are public fields.
- Text: EN and AR per level in `hints.json`. Arabic falls back to English when the field is empty. The 7 puzzle rows reuse the three hint lines the scene builder already sets on each puzzle, so no new spoilers were written.
- Count: `HintService.HintsUsed` (static int) is the number of hints shown this run. `HintService.HintShown(puzzleId, level)` is a static event. `RunResults` is not edited. The owner's side can read the count for the end-of-run summary.
- Save: H1 `SaveSystem.SetContrib/GetContrib`, key `hints`, JSON `{total, ids[], levels[]}`.
- Reset: `GameEvents.PuzzleSolved(id)` removes that puzzle's level and hides the banner. The total count stays. A fresh `HintService.Awake` (new scene load) resets everything; a save restore then overwrites it.
- Never edits `PuzzleManager` or `PuzzleBase`. `PuzzleBase.NextHint()` and `GameEvents.PuzzleHintUsed` are not used and not counted here, so a game that also calls `NextHint()` has a separate count.

### Puzzle discovery contract (read from main at 0701df6)

- `LightShadow.Gameplay.PuzzleBase` has public `string puzzleId` and public `State` (`Locked`, `Available`, `Solved`). `PuzzleBase.Awake` sets `puzzleId = name` when it is empty.
- `SceneBuilderPuzzles.cs` never sets `puzzleId`, so the live ids are the GameObject names: `Puzzle_ChalkboardCipher`, `Puzzle_CatalogueSort`, `Puzzle_OpticsBench`, `Puzzle_MapLocker`, `Puzzle_StageLights`, `Puzzle_GateCircuit`, `Puzzle_FirstAidOrder`. `hints.json` is keyed by exactly these.
- Current puzzle = nearest `PuzzleBase` (found with `FindObjectsByType`, rescanned once a second) whose `State == Available`, which has a row in `hints.json`, within `maxDistance` (8 m) of the main camera.
- Risk to document: if the owner's side renames a puzzle object or sets an explicit `puzzleId`, `hints.json` must change to match. Nothing in this PR invents a shared API. The PR only reads the two public members above.
- `Docs/PUZZLES.md` lists puzzles by name, not by id, so it is not an id source.

## C3 captions

- API: `CaptionService.Raise(string englishKey, Vector3 worldPos)`, static, safe to call at any time. It does nothing when captions are off or no service exists in the scene.
- Keys: English strings from `captions.json`. Shipped: `Footsteps nearby`, `Whisper`, `Door slam`, `Power hum`, `Shadow growl` (constants in `CaptionKeys`). An unknown key shows the key text itself.
- Direction: angle of the cue against the main camera's flat forward. Within 45 degrees of straight ahead: no marker. Beyond 135 degrees: `v` (behind). Otherwise `<` or `>`. Plain ASCII on purpose, so it renders in any font.
- Fade and limits: up to 3 captions on screen, 4 waiting (oldest waiting cue dropped), the same key within 0.75 s ignored, 0.6 s fade.
- Languages: EN and AR (`Localization.Present` shaping). Arabic falls back to English.
- Setting: `CaptionService.Enabled` (public static bool), stored in the PlayerPrefs int `abdelraouf.captions.enabled`. **Default ON.** To expose it in a menu, bind a toggle to this property. No shared menu is edited here.

### Request to the owner's side (not done in this PR)

Call `CaptionService.Raise(...)` from the audio call sites (footsteps, whisper, door slam, power hum, Shadow growl) in shared code. That wiring stays on the owner's side. `AudioManager` exposes no events this module can observe without edits, so nothing is observed automatically.

## Debug test rig

- Compiled only in the editor and development builds (`#if UNITY_EDITOR || DEVELOPMENT_BUILD`). Release builds do not contain it.
- Off by default and opt-in: it does nothing unless the PlayerPrefs int `abdelraouf.support.testrig` is set to 1.
- When on: F9 raises the next simulated cue from a rotating direction (caption shows `[SIM]`), F10 shows the next level of a built-in stub hint (no count, no save).
- It only calls the public caption API and a stub hint. It does not touch puzzles, saves, enemies, audio or shipping gameplay.

## Not verified

Compile, UI layout and fonts, Arabic shaping and wrapping (including where the `<` and `>` markers land in Arabic), the scene builder hook, puzzle discovery at runtime, idle detection on touch devices, save and load round trip, and any real audio coverage.

## Known limits

- No mobile Hint button yet. `HintService.RequestHint()` is public so one can be wired.
- No settings UI for captions.
- No tests added (the shared test folder is outside the lane).
- Idle detection reads `Input.anyKey`, mouse movement and touches. Gamepad-only play is not detected as activity.
