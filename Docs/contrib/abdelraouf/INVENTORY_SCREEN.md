# Inventory screen (Abdelraouf lane)

Status: delivered as a fork-branch input to the owner's combined PR (2026-11-27 agreement; below the 5k standalone-PR rule, no standalone PR opened). AI-prepared on behalf of Abdelraouf Zidan (GitHub Abdo1590). **Not compiled in Unity, no editor run, no scene build, no device run. Arabic strings are AI drafts, not native-checked.**

A fullscreen inventory screen, built entirely in code (no prefab, no scene YAML): dim backdrop, item grid, detail pane (name + description = the inspect view), Use and Close buttons, EN/AR labels with `Localization.CurrentFont`.

## Files

- `Assets/_Project/Contrib/Abdelraouf/Scripts/InventoryScreen/InventoryScreenModel.cs` - pure view model: stable sorted snapshot of carried ids + grid selection (no UnityEngine).
- `.../InventoryItemCatalog.cs` - descriptions (EN/AR), icon keys, and the use-handler registry `Register(id, Func<bool>)`.
- `.../InventoryScreenUI.cs` - the screen itself; static `EnsureExists()/Toggle()/Open()/Close()/IsOpen`.
- `.../InventoryScreenInput.cs` - I key + gamepad Y toggle; arrows/D-pad/left stick select; Enter/A use; I/B close.
- `.../InventoryScreenBootstrap.cs` - auto-creates the host after scene load.
- `Assets/_Project/Contrib/Abdelraouf/Editor/InventoryLevelContribution.cs` - optional H2 hook (`abdelraouf.inventory-screen`) placing the host via SceneBuilder.
- `Assets/_Project/Contrib/Abdelraouf/Tests/Editor/InventoryScreenModelTests.cs`, `InventoryItemCatalogTests.cs` - EditMode tests.

## Contracts agreed 2026-11-27 and how they are honored

1. **Provisional snapshot.** `Inventory` on main has `Has/Add/Remove/Carried/DisplayName` and `ItemAdded` only - no `ItemRemoved`, no ordered snapshot. The model copies `Carried` and sorts with `StringComparer.Ordinal` (stable while the id set is unchanged). While open, the screen refreshes on `ItemAdded`, `SaveApplied`, `ItemUsed`, on open, and via a cheap count+hash poll (0.25 s). **When the owner's `ItemRemoved` and ordered snapshot API land, subscribe to `ItemRemoved`, feed the snapshot into `Refresh`, and delete the sort and the poll.** Signatures were pending; they are not referenced anywhere.
2. **Input.** I key + gamepad Y approved and implemented. The touch HUD button is the owner's side: it only needs to call `InventoryScreenUI.Toggle()`. Esc/Start close-first guard is the owner's side (PauseMenu): call `InventoryScreenUI.Close()` when open before opening pause. This screen never consumes Escape and never clears another system's lock: it refuses to open while `PlayerController.ExternalInputLocked` is already held and clears the lock only when it set it itself. **Known limit of the shared static bool: a system that sets the lock after this screen opened cannot be detected, so close-first ordering in PauseMenu matters.** Quiz note: `QuizUIController` has no static accessor and never takes the input lock (by design); its open state is read via `GameManager.Instance.quizUI.IsOpen` before opening.
3. **Inspect decision.** `InspectViewController` is NOT reused: its `Hide()` clears `ExternalInputLocked` unconditionally, which would drop this screen's lock. The detail pane is the inspect view.
4. **Catalog.** Ships descriptions, icon keys (`key` / `medkit` / `generic`, same heuristic as `InventoryStripUI`) and `Register(id, Func<bool>)`. **No gameplay effects ship in this module - the owner's side registers real handlers at startup**, e.g. medkit: `InventoryItemCatalog.Register("medkit", () => PlayerHealth.Instance != null && PlayerHealth.Instance.TryUseMedkit());`. An item is removed and `ItemUsed` fired only when the handler reports consumption.
5. **Assembly layout.** No asmdefs: `LightShadow.Core.Inventory`, `Localization`, `PlayerController` live in `Assembly-CSharp`, which an asmdef cannot reference (the only contrib asmdef in the tree, `Contrib/Usef/LevelLogic`, is engine-free, `autoReferenced: false`, and is invoked by reflection). Tests sit in `Contrib/Abdelraouf/Tests/Editor/` without an asmdef, so they compile into `Assembly-CSharp-Editor` exactly like the existing `Tests/Editor/` suite.

## Wiring left to the owner's side

- Touch HUD button calling `InventoryScreenUI.Toggle()`.
- Esc/Start close-first guard in PauseMenu calling `InventoryScreenUI.Close()` first.
- `Register(...)` real use handlers at startup.
- Optional: run the H2 contribution, or rely on the runtime bootstrap.
- Editor/device pass: layout, fonts, Arabic shaping, gamepad feel.

## Source contracts read on main at 7e963c9

- `Inventory` (109 lines): API as in section 1.
- `PlayerController.ExternalInputLocked`: static bool, no ownership; `PauseMenu.IsOpen` static; `InspectViewController.IsOpen` static.
- `QuizUIController` (221 lines): scene-wired MonoBehaviour, `IsOpen` instance property, `ShowQuestion(QuestionData, Action<bool,int>)`; uses no input lock; number keys 1-4 stay live while open - the close-first guard should cover quiz vs inventory overlap.
- `InteractionRaycaster` (96 lines): E key gated by `ExternalInputLocked`; gamepad A gated the same way in `GamepadInput`; while this screen holds the lock, world interaction is frozen.
- `GamepadInput` (131 lines): polls `Gamepad.current`; Start = pause even while locked (hence the owner's close-first guard); Y/buttonNorth unused on main.
