# Overlay locks, Esc order, menu return (AI-prepared, fork-branch input)

Status: code on fork branch `feat/overlay-locks`, input to the owner's combined PR. No PR from this branch.
Not compiled in Unity. No EditMode test has been run. No editor, scene or device run. No Arabic review by a native speaker.

Base: Agono0/LightShadow main `7e963c9fa2cc499a144e49ec9311785982e151bc`.

## What it fixes (audit v3 ids, checked against main source)

| Id | Finding | Change |
| --- | --- | --- |
| 5.1 | One static bool lock, any Close clears another system's lock; Esc closes a puzzle and opens pause; pause Close re-locks the cursor under the Journal | `OverlayStack` tokens; pause yields Esc; pause re-locks only if no overlay wants the cursor |
| 5.2 | Pause has no way back to the main menu | `PauseMenu` Main menu button (built at runtime from the Quit button), autosave only if the active slot already holds a save, then `LoadingScreen.Transition(SceneNames.MainMenu)` |
| 5.3 | `Inventory.Remove` fires nothing, HUD chips go stale | `Inventory.ItemRemoved`; `InventoryStripUI` rebuilds on it and on language change |
| 5.7 | Journal notes only checked in `Start`; a restore that applies at once (scene already live) is missed | `JournalNote` re-checks on `JournalProgress.Changed` |
| 3.1 | Journal toast and prompt shaped, then shaped again by the HUD | logical strings only |
| 3.3 | Hint counter digits reversed in Arabic | counter placed outside the shaped name (`HintService.BuildTitle`) |
| 3.7 | Arabic journal toast had no key hint | toast now says J |
| 3.5 (strip) | Inventory chip text not shaped; icon indent LTR only | `InventoryStripUI.ChipText`, icon at the far edge in Arabic |

## API

`LightShadow.Contrib.Abdelraouf.Overlay`:

- `OverlayStack.Acquire(owner, wantsFreeCursor = false, dismissable = false, closeCallback = null)` returns an `InputLockToken`.
- `OverlayStack.Release(ref token)` is safe on null, on a released token and on a token dropped by `ResetAll`. It frees only that token.
- `OverlayStack.AnyLocked`, `WantsFreeCursor`, `AnyDismissable`, `ClosedThisFrame`, `Holds(owner)`, `TryCloseTop()`.
- `OverlayStack.ResetAll(invokeCallbacks)`: called on respawn (callbacks on, visible overlays close themselves) and on scene unload (callbacks off). Nothing else calls it. Story reset releases only the story token.
- `OverlayLockSet` is the same logic with no engine types, for EditMode tests.
- `PlayerController.ExternalInputLocked` is now a property: get is `OverlayStack.AnyLocked`; set maps to one shared legacy token and `false` frees only that token. Read-only call sites did not change.

Dismissable means Esc closes it, so pause does not open. Story dialogue holds a non-dismissable token, so it does not block pause (today's behaviour kept).

## Shared files touched (hunks only)

See the WORK_CLAIMS row. Lock call sites: CardSort, Circuit, CodeLock, Optics, Order, Sequence UI controllers (acquire in Open, release in `ReleaseLock`, Esc line unchanged), `InspectViewController`, `HidingSystem`, `StoryDirector` (lock only), `DevConsole` (lock only).

## How Esc ordering works

Unity does not define Update order between `PauseMenu` and an overlay. Either order is handled: if the overlay already closed this frame, `ClosedThisFrame` is true; if it has not run yet, `AnyDismissable` is true. This logic is covered by the pure rule `PauseMenu.ShouldYieldEscape`. The real two-script frame order is play mode behaviour and was not observed.

## Decisions taken (from the owner side)

- ResetAll only on scene unload and respawn.
- Autosave to menu only for a slot that already holds a save. `SaveSystem.CurrentSlot` defaults to 0 and nothing marks "no slot chosen"; a run started with PLAY and no slot would otherwise create slot 0 here. After a checkpoint autosave slot 0 does hold a save and is written.
- New-game story reset (audit 2.3) is not touched. `MainMenuController.StartNewNight` still does not call `StoryDirector.ResetForNewRun`; returning to the menu makes that path easier to reach.
- `LoadingScreen` is consumed as is (a second `Transition` while one runs is still dropped).

## Hint Recall persistence (note for the local integration, no code here)

The delivered Hint Recall branch (`feat/hint-recall`, head `9afbb65df14f1168619543c029caaf6460314100`) saves a second contributor key `hintlog`. The approved proposal said no save-format change. `HintLogRecord.Adopt` clears the record when the `HintService` instance changes, unless `restoredPending` is set, and `restoredPending` is set by `SaveApplied` and cleared only by the next `Adopt`. Sequence that leaks stale hints into a new run, read from source: load a save, leave to the menu before any hint is shown or save is captured (no `Adopt` ran), start a new game: the first hint event sees `owner == null` with `restoredPending == true` and keeps the loaded record. Quit to menu makes this reachable. Smallest patch, outside this branch: call `HintLogRecord.Clear()` on a new run (after `StoryDirector.ResetForNewRun` in `SaveSlotUI.StartRun(NewGame)` and in `MainMenuController.StartNewNight`), or clear `restoredPending` when the school scene unloads. EditMode tests to add on that branch (pure, no engine): restore then Adopt(A) keeps the record; restore, no Adopt, Clear, Adopt(B) is empty; Adopt(A), record, Adopt(B) with no restore is empty. Not done here by instruction.

## Unverified

Compile, EditMode results, play-mode Esc order, cursor behaviour, `sceneUnloaded` ordering against the next scene's Awake, pause card layout of the runtime button (placed 4 px under Quit, near the hint line), Arabic shaping of the chip, hint title and menu label, restore-timing behaviour of `JournalNote`.
