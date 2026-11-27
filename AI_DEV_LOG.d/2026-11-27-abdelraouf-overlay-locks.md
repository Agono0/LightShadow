# 2026-11-27 abdelraouf-agent: overlay locks, Esc order, menu return, inventory removal

Fork branch `feat/overlay-locks` on Abdo1590/LightShadow, input to the owner's combined PR (no PR from the branch). AI-prepared. Not compiled in Unity; tests written, not run; no editor, device or native Arabic check.

- New `OverlayStack` / `InputLockToken` / `OverlayLockSet` under `Contrib/Abdelraouf/Scripts/Overlay/`; `PlayerController.ExternalInputLocked` becomes a view of it.
- Lock writes migrated in the six puzzle UIs, Inspect, Hiding, Story (lock only), DevConsole (lock only), Journal.
- `PauseMenu`: yields Esc to dismissable overlays, cursor re-lock only when no overlay needs it, runtime Main menu button.
- `Inventory.ItemRemoved`; `InventoryStripUI` rebuilds on it; Arabic chip shaped once.
- `JournalNote` logical strings, key hint, restore re-check. `HintService.BuildTitle`.
- Details, decisions and the Hint Recall `hintlog` note: `Docs/contrib/abdelraouf/OVERLAY_LOCKS.md`.
