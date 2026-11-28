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

## Later folds and the user's second playtest list (same PR)

- Peer inputs folded as peer-prepared, uncompiled: Abdelraouf inventory blank-id test, Ziad H3 navmesh gate, Ahmed builder-story Arabic coverage test. Nine Arabic rows added for the builder story strings (not native-verified).
- Footsteps: root cause found in source. Footsteps and AmbientDrone both took the first AudioSource on the player root, and the drone rewrites its volume every frame, so PlayOneShot steps came out near zero. Steps now own a dedicated source, registered as SFX. The category scan no longer files every source on the drone's object under Ambience. Not heard on a device.
- Voice-over: all 44 story nodes have EN and AR clips in Resources/VO and the ids match. No code fault found. The earlier duplicate-GUID copies in Audio/VO (removed in this PR) are the leading suspect; unverified. VoiceoverPlayer now logs a warning when a clip is missing or cannot be played, so a device log shows the cause.
- Main gate is now a double gate: two leaves, one per pillar, covering the full opening; wing doors now cover the full 2 m doorway. Both rebuild with the scene builder. Not seen in the editor.
- Objectives: pure ObjectiveLines chain (fuses, key, wire console, crank, escape), fragments marked optional, panel peeks at start and after each fuse. Arabic rows added.
- Touch: joystick has a dead zone, an eased response and a floating origin; held thumb keeps steering. Mobile canvases use a 1600x900 reference (1.2x). Quiz card is a flat opaque panel with larger text, best-fit for long questions. Mobile button labels are larger.

## Cards that overflowed the mobile canvas (Ziad's read)
- New `CardFit` (UI): scales a card down only as far as needed to fit its parent with a 24 px margin. Added to the settings card (1120x1080), save-slots card (900x1020) and how-to card (1000x700). Settings on a 900 px canvas lands at about 0.79, so Title and Reset/Close are on screen. Fonts are not shrunk elsewhere.
- Android Back is Escape in Unity: traced MainMenuController (settings, save slots, how-to) and PauseMenu (settings) both close on Escape.
- Tests: card sizes against 1600x900, 1600x1000, 1600x1200, 1920x1080 and 2000x900. Not seen on a device.
- Follow-up to Ziad's recheck: the settings card is now compact instead of scaled: row pitch 64 to 52, arrows 56x48 (touch targets over 48 px), card 1080 to 840 tall, so it fits the 900 px phone canvas at scale 1. Both pages and the mobile reflow share the constants in SettingsPanelController. CardFit stays as the safety net.
- How-to card is now 1200x840 with a 1104x640 body at 22 px plus best fit (min 17), so the longer text cannot reach the BACK button. Height estimate pinned by a test (estimate only, not a render).
- Scene regeneration: build.yml runs SceneBuilder.BuildAndroidArm64Batch, BuildAndroidArmV7Batch and BuildWindowsBatch; each calls BuildAll() before packaging, so builder changes (CardFit, MobileUiScale) reach CI builds. The release job needs both build jobs and runs on v* tags, so tagged builds carry them too.
- Peer (Abdelraouf, peer-prepared, uncompiled): HUDObjectiveSubscriptionTests cherry-picked from mirror/abdelraouf-hud-objective-subscription-test @ 127141b.
- Verified his LOW finding: ApplyLoadedState refreshed the HUD before SaveApplied handlers (Inventory, PuzzleManager) restored, and those raise no item or puzzle events. New `SaveSystem.NotifySaveApplied` runs the handlers, then refreshes the HUD (null-checked); both restore paths use it. Test: handlers run and no HUD is fine. The refresh itself is not asserted (needs a scene).
- Peer (Ahmed, peer-prepared, uncompiled): SfxRegistryContractTests cherry-picked from mirror/ahmed-sfx-registry-contract-tests @ ad0bc32. His header note about the sparse gap is updated.
- SfxRegistry.RegisterMany now stores at the compacted index (sparse array no longer leaves holes ClipVariant can pick); SfxRegistrySparseTests pins it.
- Comments: Footsteps source is registered as SFX (not Ambience); VoiceoverPlayer comment and log now say story nodes may use SAPI while the ending shows subtitles only.
- DangerModel (pre-burst danger vs near-breath suppression): read, left unchanged, design question.
