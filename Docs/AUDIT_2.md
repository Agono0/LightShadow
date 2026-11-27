# LIGHT SHADOW — Audit 2 (finish-the-game pass)

**Date:** 2026-11-27 · **Base:** `main` @ `2dd3376` · **Author:** Instinct agent (static review)

Second full audit, after the user's finish-the-game brief. Complements
`Docs/AUDIT.md` (premium pass). This pass is **static only**: no Unity editor
is available to the auditors, so every finding is from source reading and the
CI `build` workflow is the compile gate. Treat each item's "repro idea" as a
hypothesis until someone runs it in the editor or on a device.

## Method

- Read the core loop end-to-end: `GameManager`, `SaveSystem`,
  `CheckpointManager`, `EnemyBehaviour`, `QuizUIController`, `GameSettings`,
  `PerformanceManager`, `BootstrapLoader`, UI builders.
- Pattern scans over all runtime scripts: event subscribe/unsubscribe balance,
  `GetComponent`/`Camera.main`/`FindObjectOfType` in hot paths, coroutine
  usage, `Time.timeScale` writes, direct `PlayerPrefs` writes, platform guards.
- Mobile-stretch root-cause hunt (user-reported: "looks stretched on mobile").

## Findings

| # | Severity | Area | Finding | Status |
|---|----------|------|---------|--------|
| A1 | High | Mobile | `GameSettings.ApplyResolution` ran `Screen.SetResolution` on Android. Forcing a picked resolution makes Android scale the framebuffer to fill a different-aspect display — the stretched look the user reported. | **Fixed** (`fix/mobile-stretch`): mobile renders native only |
| A2 | Medium | Mobile | No safe-area handling; notches/rounded corners could clip HUD text and touch controls. | **Fixed** (same PR): `SafeAreaFitter` + `SafeArea` content root on every generated canvas |
| A3 | Medium | Saves | `SaveSystem.Restore` defers with `pendingRestore` when the saved scene is not loaded. If the player instead starts a new game (or never loads that scene), the stale `pendingRestore` stays subscribed and can apply a dead save to a later run of the same scene; a save whose scene never loads is silently dropped. Repro: autosave in school → menu → new game → later enter school. | **Fixed** in the saves pass: `SaveSystem.ClearPendingRestore` drops the deferred restore when a new game starts, so a stale save cannot land in a fresh run |
| A4 | Low | Saves | `ApplyLoadedState` restores the *first N* fuse boxes / fragments by scene order, not the exact ones collected. Counts and difficulty are exact; pickup identity is not. Documented in `GameManager`, kept here so it is not forgotten. | Known limitation; identity-keyed restore is a follow-up |
| A5 | Low | UI | Resolution/fullscreen settings rows still render on mobile, where they are now no-ops (A1). | **Fixed** in this pass: `SettingsPanelController` hides the resolution and fullscreen rows on mobile, where they were no-ops after A1 |
| A6 | Low | UI | With safe-area insets, full-screen menu dimmers inset too instead of staying full-bleed behind the notch strip. Cosmetic, notched phones only. | Open; per-element safe-area (backgrounds full-bleed, controls inset) is a polish follow-up |
| A7 | Info | Audio | `StoryTeller` (SAPI/PowerShell narrator) is guarded `UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN`: Android has no narrator voice at all. | Superseded by the generated-VO workstream (recorded clips play on both platforms) |
| A8 | Info | Events | Subscribe/unsubscribe audit of `AdaptiveMusic`, `DialogueUIController`, `QuizUIController`, `FpsCounter`, `VolumeCategories`, `PerformanceManager`: balanced; no leaks found. | Clean |
| A9 | Info | Timing | All `Time.timeScale` writes pair 0 (pause) with 1 (resume) across menu/recap/ending/bootstrap paths; quiz timer uses scaled time and freezes under pause — consistent. | Clean |
| A10 | Info | Content | All 60 questions still carry `factSource = NEEDS-FACT-CHECK`; Arabic question fields unfilled. Human-only per AGENTS.md rule 5. | Tracked in `Docs/TASKS.md` P4 |

## Hot paths reviewed

`EnemyBehaviour` (FSM, NavMesh snap fallback), `PlayerController`,
`FlashlightBattery`, `BrightIdeaMeter`, `AdaptiveMusic`,
`AudioSnapshotController`. No per-frame allocations or unbounded lookups found
beyond the already-cached patterns (`GetMeter` cache, event dedup). Deeper
profiling needs a device run (human-only).

## Next audit passes (queued with the workstreams)

1. Puzzle framework and item system get a design review before implementation.
2. Lighting/menu/graphics changes re-run this audit's pattern scans on the
   touched files.
3. Every fix PR names the finding IDs it closes.
