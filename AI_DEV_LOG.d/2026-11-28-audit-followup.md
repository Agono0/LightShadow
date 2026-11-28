# 2026-11-28 — Audit follow-up bundle (Agono0 / AI-prepared)

One PR from main bc7e6e1 (#129 merged). NOT compiled or run in the editor
by the author; CI compiles it. Nothing seen on a device. Arabic strings are
not native-verified.

Fixes (each its own commit, with pure-seam tests where possible):
- ShadowAudio: the ambusher stays silent until its burst (near-breath guard
  excludes Ambush) - NearBreathAllowed.
- K1 medkits: per-prop saved identity (Medkit_Infirmary / Medkit_Staff),
  old saves hide one kit, a second pick while carrying is refused (EN/AR).
- V1: PerformanceManager no longer forces vSyncCount 0 over the player's
  vSync-on setting (OwnsFrameCadence).
- Ending cinematic: line hold = max(budget, clip + 0.6 s), capped 14 s, so
  long VO is not cut by the next line (LineSeconds, VoiceoverPlayer.LastPlayedSeconds).
- B1/B2: Windows menu build regenerates scenes; validation expects 6 lit
  zones and checks puzzles, medkits and spare keys; ValidateBatch exits 1
  on FAIL (CI does not call it).
- T1: a deliberate E-drop is a placing-down (first impact quiet, no noise,
  no shatter); throws unchanged.
- M1: touch look is ignored and the look finger dropped while input is locked.
- N2 (Ziad): inspect overlay title/body/close centred (CenteredOffsets).
- H2: inspect backdrop alpha 0.94 -> 0.5 so the camera-parented item shows
  through. ScreenSpaceCamera was rejected (depth-test against world, needs a
  device to verify). Dimmed, not crisp.
- S1 wrong-answer routing test (Abdelraouf, folded as-is).
- 8.1: 12 unused built-in Unity modules removed from manifest + lock (cloth,
  vr, xr, vehicles, wind, tilemap, terrainphysics, unityanalytics,
  adaptiveperformance, accessibility, vectorgraphics, video). terrain and
  physics2d stay (required by URP core / kept conservatively). Build risk:
  CI will tell.
- 8.2: activeInputHandler stays "both": legacy Input is used in 27 files and
  the Input System in GamepadInput.
- D3: stale Assets/_Project/Audio/VO removed. All 154 clips were byte-identical
  copies of Resources/VO (docs say they moved there in #28) and shared 157
  GUIDs. MetaGuidUniquenessTests now guards duplicates. Touches the Audio
  lane at the user's instruction.
- 7.2: haptics On/Off row (mobile only) appended as settings row 27.
- Save notice dialog reads CLOSE with a single button instead of YES/NO.

Not done / reasons: 2.7 StoryTeller already cancels in-flight speech; a
persistent PowerShell process is a rewrite that cannot be verified here. NavMesh
projection on restore, F2, F6 and enemy state in saves need a device or a
product decision.
