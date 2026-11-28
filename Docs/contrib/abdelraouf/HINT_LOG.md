# Hint Recall (Abdelraouf lane)

Status: delivered as a fork-branch input to the owner's combined PR (2026-11-27 agreement; no standalone PR, no size claim). AI-prepared on behalf of Abdelraouf Zidan (GitHub Abdo1590). **Not compiled in Unity, no editor run, no scene build, no device run. Arabic strings are AI drafts, not native-checked.**

A fullscreen log of hints the player already unlocked, built entirely in code (no prefab, no scene YAML): a puzzle list on the left, the unlocked levels of the selected puzzle on the right. Plus a mobile RequestHint button.

## Files

- `Assets/_Project/Contrib/Abdelraouf/Scripts/HintLog/HintLogModel.cs` - pure view model: rows, selection, the spoiler rule.
- `.../HintLogRecord.cs` - remembers the highest level shown per puzzle (see "Why a record"); saves under contributor key `hintlog`.
- `.../HintLogUI.cs` - the screen; static `EnsureExists()/Toggle()/Open()/Close()/IsOpen`.
- `.../HintLogInput.cs` - L toggles; Up/Down select; optional request key (see Key binding).
- `.../HintRequest.cs` - `HintRequest.TryRequest()` and `HintRequest.RequestKey`.
- `.../HintTouchButton.cs` - mobile RequestHint button.
- `.../HintLogBootstrap.cs` - creates the host after scene load (button only on mobile).
- `Assets/_Project/Contrib/Abdelraouf/Editor/HintLogLevelContribution.cs` - optional H2 hook (`abdelraouf.hint-log`, order 130).
- `Assets/_Project/Contrib/Abdelraouf/Tests/Editor/HintLogModelTests.cs`, `HintLogRecordTests.cs` - EditMode tests.

## Spoiler rule

A level is shown only if it was unlocked. The unlocked count per puzzle is `max(HintService.LevelFor(id), HintLogRecord.Level(id))`, clamped to `0..HintService.MaxLevel`. A puzzle at 0 has no row. `HintLogModel.TextFor` and `UnlockedTexts` refuse any level above the unlocked count, so the "answer" text of a puzzle at level 2 is never read by the UI. Locked levels show only a count ("1 more hint is still locked").

## Why a record

`HintService` keeps shown levels in a private static map and removes a puzzle's entry on `GameEvents.PuzzleSolved`. After a solve `LevelFor(id)` is 0 again, so without help the log would forget solved puzzles. `HintLogRecord` listens to `HintService.HintShown`, keeps the highest level per puzzle, and saves it through `SaveSystem.SaveCaptured/SaveApplied` with `SetContrib/GetContrib` (key `hintlog`). It is cleared when the live `HintService` instance changes (fresh scene), unless a save restore just filled it. That ordering was read from source, not observed in the editor.

## Key binding (H conflicts with medkit)

Nothing in this module hard-codes H. `HintService` still reads its own public `hintKey` field (default `KeyCode.H` on main); the owner's side rebinds that field. For an extra request key, set `HintRequest.RequestKey` (default `KeyCode.None`, i.e. no key) or subclass `HintLogInput` and override `RequestHintPressed()`. No shared bindings are edited. L (log toggle) is the only key this module binds, as approved.

## Mobile button

`HintTouchButton` calls `HintRequest.TryRequest()`, which calls `HintService.RequestHint()`. It is a separate component because `TouchButton.Kind` is a shared enum with no hint entry. It shows only on mobile platforms (`showOnDesktop` is for editor checks) and hides while the input lock, pause menu, hint log or a quiz is up, or when hints are off or no `HintService` exists. Default anchor is bottom right, `(-40, 260)`, 110x110. The owner's side owns the mobile HUD layout and can move it, or add a `TouchButton.Kind` that calls `HintRequest.TryRequest()` and drop this component. When `RequestHint()` returns false a short message is shown through `GameManager.ShowMessage`.

## API gaps (checked against main at 7e963c9)

1. `HintService.RequestHint()` returns a bool only. It does not say whether the cooldown or "no puzzle nearby" refused, so the message covers both.
2. `HintService` has no public list of unlocked levels and no solved-puzzle history; `LevelFor(id)` is cleared on solve. Hence `HintLogRecord`.
3. Hint text is reachable only through the `HintService.Instance.hintsJson` TextAsset (assigned by the level builder). Without a live `HintService` the log is empty.
4. No display title for a puzzle exists in the hint data; the name is derived from the puzzle id (`Puzzle_ChalkboardCipher` -> `Chalkboard Cipher`).
5. `TouchButton.Kind` has no hint entry (shared file, not edited).
6. `PlayerController.ExternalInputLocked` is a shared static bool without ownership. This screen refuses to open while it is set and clears it only if it set it. It cannot detect a system that sets the lock after the screen opened, and it cannot guarantee another system's lock is never cleared by that system's own close path. No ownership guarantee is claimed.

## Owner's side

- Esc/Start close-first, modal ordering and shared lock integration: call `HintLogUI.Close()` first when open.
- Rebind the hint request key (or set `HintRequest.RequestKey`).
- Place or restyle the mobile button; confirm an `EventSystem` exists.
- Editor/device pass: layout, fonts, Arabic shaping, touch feel.
