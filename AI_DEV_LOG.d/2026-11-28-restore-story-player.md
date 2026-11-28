# 2026-11-28 follow-up: restore, enemy saves, narrator worker, player rig

Written by the execution agent for the user (Ag). Nothing here was seen in the editor or on a device; CI is the only compile and test run.

## Done in this PR (so far)
- F6: loading a save no longer replays the opening line and narration. `SaveSystem.RestorePending` (still waiting) or `ConsumeRestoredFlag()` (already applied) make `GameManager.Start` skip the intro. A new game clears the flag through `ClearPendingRestore`.
- NavMesh projection on restore: the saved x/z is projected onto the NavMesh within 1.5 m (`SaveSystem.TryProjectToNavMesh`), y stays as saved. No NavMesh, no change.
- Enemy state in saves: the wraith's position and the menace gauge are stored under the contrib key "enemy" (`EnemySaveState`, written and read by `GameManager` on SaveCaptured/SaveApplied). On load it returns patrolling at the saved spot, only when that spot is at least 10 m from the player, and the gauge is clamped to 90% of max so a load never parks it at once. Old saves have no key and are untouched.
- StoryTeller (audit 2.7): `PowerShellSpeaker` keeps one PowerShell alive and sends it lines. An interrupt kills it and the next line starts a new one. If it cannot start or fails 3 times, the old per-line path is used. Windows only, never run on Windows here.
- VO: `VoiceoverPlayer` asks for clip data before playing (clips import with preload off), records `LastStatus` and logs each line's first play with its length and load state. Missing-clip and no-AudioManager warnings stay. Chapter-1 recheck: ids, folders (Resources/VO/EN and AR), the voice source (2D, own category) and the builder wiring all line up from source. No code cause found, so the logged line is the way to see it on a device.
- Tests: RestoreFollowupTests (blob round trip, bad input, placement distance, flag lifecycle, projection without a NavMesh).

## Deliberately left
- DangerModel weight and ambush design: not clearly wrong, left.
