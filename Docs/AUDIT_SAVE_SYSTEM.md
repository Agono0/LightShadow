# Save-system robustness audit

Read-only audit of `Assets/_Project/Scripts/Core/SaveSystem.cs` and its callers
(`CheckpointManager`, `GameManager.ApplyLoadedState`, `SaveSlotUI`, `PuzzleManager`,
`Inventory`, `StoryDirector`), against `main` @ e8b6342. Docs-only: no code was
changed and nothing was run in Unity. Every finding comes from reading the source;
the "verify" column says what a human should confirm in the editor or on a device.

Agent: Instinct AI agent, working for the repo collaborator UsefBebars.

## Findings

| id | severity | area | finding | suggested fix | verify |
| --- | --- | --- | --- | --- | --- |
| S1 | High | Write safety | `SaveToSlot` calls `File.WriteAllText` straight onto the slot file. If the app is killed or the device loses power mid-write (common on Android when the OS reclaims the app), the slot is left truncated. There is no backup copy, so the player loses that slot's progress. | Write to `slotN.json.tmp`, then `File.Replace` / move over the real file; keep one `.bak` of the previous good save and fall back to it on load. | Kill the process during a save in a test build; check the file. |
| S2 | High | Save trigger | The only autosave call site is `CheckpointManager` (lit-zone checkpoints). There is no save when the app is paused or backgrounded (no `OnApplicationPause` / `OnApplicationQuit` hook for saves; the only focus hook is in `PlayerController`). On Android, a player who is backgrounded and killed loses everything since the last checkpoint. | Save on `OnApplicationPause(true)` while in the School scene, using the same atomic write as S1. | Background the app on a device, kill it, relaunch, load. |
| S3 | Medium | Exceptions | `Capture()` runs outside any try/catch, and `SaveCaptured?.Invoke` is a multicast call. If one subscriber (story, puzzle, inventory) throws, the remaining subscribers are skipped, no save is written, and the exception goes up into `CheckpointManager`'s checkpoint code. The save then silently never happens. | Invoke each handler separately inside try/catch (`GetInvocationList`), log the failure, and still write the rest of the blob. | EditMode test with a throwing subscriber. |
| S4 | Medium | Load validation | `LoadFromSlot` accepts whatever `JsonUtility.FromJson` returns. An empty `{}` file or a file with unrelated keys deserializes to a default `SaveData` (null `sceneName`, null `savedAtUtc`). `HasSave` is then true, the slot shows as used, and `Restore` only logs "empty save" and does nothing, so the player presses LOAD and nothing happens. | After parsing, reject data with an empty `sceneName` or an unknown `version`, and treat it as a corrupt slot (fall back to `.bak`, else show the slot as empty/corrupt). | EditMode test with `{}` and a truncated file. |
| S5 | Medium | NewestSlot | `NewestSlot` keeps `newestStamp` null when the first readable slot has no `savedAtUtc`. The condition `newestStamp == null` is then true for every later slot, so the last readable slot wins regardless of dates. A stamp-less (S4-style) file therefore decides which slot PLAY offers. | Skip slots with an empty stamp, or parse `savedAtUtc` to a `DateTime` and compare that. | EditMode test with three slot files. |
| S6 | Medium | Restore timing | In `Restore`, when the active scene already matches but `game.player` is not spawned yet, the code sets `pendingRestore` and waits for `SceneManager.sceneLoaded`. That event already fired for this scene, so the restore may never run and the save is dropped without a message. `SaveSlotUI` calls `Restore` right after starting the load, so this depends on whether the scene is active at that moment. | When the scene matches but the player is missing, retry on the next frames (coroutine with a timeout) and log a warning if it never lands. | Load a slot from the menu on a slow device; confirm position and fuses apply. |
| S7 | Medium | Stale positions | `ApplyLoadedState` teleports the player to the saved `position` with no check. The redesign streams change scene layout daily (doors, props, builder output), so an older save can place the player inside a wall, under the floor, or outside the NavMesh. NaN or huge values in a damaged file are also applied as is. | Check the position is finite; project it onto the NavMesh (`NavMesh.SamplePosition`) and fall back to the last checkpoint position if no hit. Store a `layoutVersion` in `SaveData` and warn on mismatch. | Load an old save after a scene rebuild. |
| S8 | Low | Versioning | `version` is written as 1 and never read. New fields are additive and old files deserialize with defaults, which is fine today, but a save from a newer build loaded by an older one is accepted silently, and a future non-additive change has no migration hook. | Add a `const int CurrentVersion`, reject `version > CurrentVersion`, and add a `Migrate(SaveData)` stub. | n/a |
| S9 | Low | Value clamps | `meterValue`, `batteryFraction` and `yaw` are not range-checked in `SaveSystem`. `ApplyLoadedState` clamps fuse and fragment counts and `RestoreBattery` clamps the battery, so the main risk is a NaN in `meterValue` or `yaw` from a damaged file. | Reject non-finite floats in the S4 validation step. | EditMode test with NaN. |
| S10 | Low | Slot reads | `NewestSlot` fully reads and parses up to three files each time it is called (menu PLAY). It is cheap now, but the story/puzzle/inventory blobs will grow. | Cache slot summaries (scene, stamp, playSeconds) once per menu open. | n/a |
| S11 | Info | Logging | `Autosave` logs with `Debug.Log` on every checkpoint, which AGENTS.md's quality bar discourages. | Drop it or gate it behind a debug flag. | n/a |
| S12 | Info | Known | AUDIT_2 A4 (fuses and fragments restored by scene order, not identity) still stands; `playSeconds` is session time only and the slot file is overwritten with it. Both are documented already. | Identity-keyed restore (follow-up). | n/a |

## What is already solid

- Slot index bounds are checked on every public entry point.
- All file I/O is wrapped in try/catch and failures return false or null instead of throwing (the gap is S3, which is outside the I/O calls).
- Story, puzzle and inventory blobs each catch bad JSON and fall back to a fresh state.
- Stale deferred restores are cleared on new game (AUDIT_2 A3).

## Suggested order

S1 + S2 + S4 together (one small code PR: atomic write, backup, pause save, validation), then S3, S5, S6, S7. S1 to S5 are testable as EditMode tests with a temp directory, which fits `Assets/_Project/Tests/Editor/`. This PR changes no code; the code lane (`Assets/_Project/Scripts/**`) belongs to the owner's side, so these are left for them to schedule.
