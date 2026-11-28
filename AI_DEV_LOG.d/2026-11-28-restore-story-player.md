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

## Player/torch rig
- `SceneBuilderPlayerRig.ApplyPlayerRigModels` looks in Models/Hero for SM_Torch, SM_Player_Hand_R/L, SM_Player_Body (OBJ or FBX; `LoadModelAsset` now accepts both). Each is optional; when missing, the primitive hand and torch stay as built. Markers named "Grip" and "Muzzle" inside the torch mesh set where it sits in the hand and where the beam starts (`PlayerRigFit`, tested). Bad markers are ignored. The body is shadow-only in first person.
- Walk bob, breathing, crouch dip and torch sway already exist (FirstPersonHands, TorchSway) and carry whatever model is attached.
- Physics: the player keeps its CharacterController as the mover and now also has a kinematic, rotation-frozen Rigidbody (`PlayerRigFit.ConfigureBody`), independent of the model.
- No model files are in the repo yet. Nothing in this section has been seen in the editor.
- F2 (120 Hz frame rate) needed no change: `PerformanceManager` already handles it.

## Delivered models (peer-prepared, never opened in Unity)
- SM_Player_Body, SM_Player_Hand_L/R and SM_Torch (OBJ, from the contrib branch contrib/ziad-models-player-torch @ a680a2d, real files taken from the delivery zip since the branch copies are LFS pointers) are in Models/Hero with fresh .meta files. The rig uses the right hand only in first person; its Grip marker is where the torch's Grip marker lands. The torch Muzzle sets the beam origin. Marker cubes are hidden. Materials are replaced with the project palette.
- PlayerModelImportTests checks left/right after Unity's X mirror, forward axis, markers, and outward normals; if CI fails there, the import (not the rig) needs a flip.
- Still crude: blocky humanoid body, hands are long tapered boxes with a block of fingers (not curled around the handle), torch is a plain tube with no head ring or lens bezel, one flat colour per model, no textures.
