# LIGHT SHADOW — Project Audit (Premium Pass)

**Date:** 2026-10-30 · **Branch audited:** `main` @ 49e22e5 · **Auditor:** Lead TD (AI-assisted, logged in `AI_DEV_LOG.md`)

## 1. Executive summary

The project is a **fully playable** Unity 6000.4.0f1 / URP 17.4 educational horror game with a
complete gameplay loop, both platform builds produced once (Windows 125.3 MB, Android APK 45.7 MB)
and a clean `ValidateBatch` history. However it ships as a **greybox**: zero 3D models, zero
textures, no branching narrative, no save system, no quality tiers, and no gamepad support.
One **critical blocker** was found and fixed on day 1 (missing build profile, §3.1).

## 2. Method

- Read all root docs (`README.md`, `LightShadow_Project_Document.md` 901 lines, `AI_DEV_LOG.md`).
- Filesystem inventory of `Assets/` (counts, sizes, empties).
- `ProjectSettings` inspection (quality levels, player settings, URP assets).
- `Editor.log` + `Logs/*.log` console review.
- Unity batch runs: `ValidateBatch` (baseline verification).
- Grep-level architecture review of `Assets/_Project/Scripts/` (59 scripts) per house rules
  (no blind full-file reads of the large editor builders).

## 3. Findings

### 3.1 Critical (fixed immediately)

| ID | Finding | Fix |
| --- | --- | --- |
| C1 | **Missing build profile asset.** `ValidateBatch` aborted with exit 1: *“Failed to read the specified build profile 'Assets/Settings/Build Profiles/Windows.asset'”*. The profile existed on 23 Oct (builds succeeded) but was never committed and is gone from disk. Any batch build/validation is impossible until restored. | Added `Tools/Editor/BuildProfileTool.cs` (`LightShadow ▸ 7. Repair Build Profiles`, batch entry `BuildProfileTool.EnsureBatch`) that recreates `Windows.asset` + `Android.asset` idempotently via `BuildProfile.CreateAsset`. Profiles now exist and are committed. |

### 3.2 Placeholder / greybox content (the main work of this pass)

| ID | Finding |
| --- | --- |
| P1 | `Assets/_Project/Models/` contains **only `.gitkeep`** — zero real 3D models. The entire school (walls, floors, desks, lockers, doors, lamps, gate) is built from Unity primitives (cubes) by `SceneBuilderScenes.cs`. |
| P2 | `Assets/_Project/Textures/` contains **only `.gitkeep`** — zero textures. All 23 materials are flat-colour URP/Lit with no albedo/normal/metal-smoothness maps. |
| P3 | `Assets/_Project/Prefabs/` are primitive-based (player capsule + hand cubes, shadow silhouette, interactable cubes). No imported meshes exist anywhere; `Assets/ThirdParty/` is empty. |
| P4 | No lightmaps / GI bake: fuse-room lights toggle at runtime (documented in AI log pass 3), so no lighting data exists; no light probes, no reflection probes. |
| P5 | First-person hand is boxes; the Shadow is a primitive silhouette — no skinned meshes, no animations, no IK anywhere. |

### 3.3 Performance

| ID | Finding |
| --- | --- |
| F1 | Quality settings define only **two** levels (`Mobile`, `PC`) — no Low/Medium/High/Ultra tiers, no auto-detection, no dynamic resolution, no Adaptive Performance use. |
| F2 | No `Application.targetFrameRate` handling for 120 Hz displays (Windows high-refresh or Android 120Hz); vSync count is 0 at both levels but frame rate is uncapped rather than driven. |
| F3 | No object pooling; pickups/labels are static so impact is low, but there is no pool infrastructure for the planned VFX/ambience spawns. |
| F4 | No profiling evidence at all (`Docs/PERFORMANCE.md` does not exist yet). |
| F5 | Builds folder holds ~1.6 GB of stale diagnostic builds (DiagA–F, Minimal, NoHud…) — local only, not in git; candidates for cleanup (not blocking). |

### 3.4 Missing systems (against the premium-pass targets)

| ID | Finding |
| --- | --- |
| M1 | **No save/load.** Checkpoints are per-session only; nothing persists story state to disk. |
| M2 | **No branching narrative system.** Story is delivered through static TextMesh plaques; no choices, flags, stats, or multiple endings — `GameManager.Escape()` has a single fixed path into `03_Ending`. |
| M3 | **No gamepad support** (classic Input for KB/M + custom touch layer; new Input System package present but unused at runtime). |
| M4 | **No dialogue UI** (typewriter, choice buttons), no accessibility options (text speed, subtitle size, colourblind palette), no skip/auto. |
| M5 | **No audio mixer groups/sliders** — procedural audio plays at fixed volumes; settings card has no audio rows. |
| M6 | No Cinemachine (camera work is manual script); no Addressables (acceptable at 4 scenes — decision logged); no Timeline. |
| M7 | `AGENTS.md` (project map referenced by the token-optimized dev skill) is **missing** from the repo root. |

### 3.5 Hygiene / risk notes

- Editor.log exceptions traced to Unity's own AI-assistant package (SuperProxy network errors) —
  editor-side noise, **not** game errors; game console was verified clean in pass 3.
- `hdrp-nightfall` branch exists (old experiment); untouched.
- Question bank: 60 assets, all `factSource: NEEDS-FACT-CHECK` (human fact-check remains a
  documented human task, out of scope for code).
- `.gitignore`/`.gitattributes` follow the doc; LFS installed (3.7.1).
- 02_School.unity is a 1.5 MB generated YAML — all scene edits **must** go through the
  `SceneBuilder*.cs` partials + rebuild (house rule).

## 4. What already passes (keep, don't break)

- Full loop: movement/sprint/crouch, flashlight+battery, Bright Idea Meter (60 seeded questions),
  Shadow 4-state FSM on NavMesh, 3 fuse boxes / 6 fragments / gate / dawn chase / recap.
- EN/AR interface, settings card (resolution/quality/vsync/brightness/sensitivity/language/narrator),
  touch layer that hides on desktop, SAPI narrator (Windows), procedural audio (no audio files needed).
- Batch tooling: `BuildAllBatch`, `ValidateBatch`, `BuildWindowsBatch`, `BuildAndroidBatch` —
  the verification backbone for this pass.
- All gameplay tuning lives in `GameConfig.asset`; difficulty ramp via question tags.

## 5. Verdict

**Proceed with the premium pass.** Critical item C1 fixed; the pass concentrates on (in priority
order): real 3D art + PBR + lighting (P1–P5), performance tiers + 120 Hz + pooling + profiling (F1–F5),
save system + branching story + endings (M1, M2), gamepad + UI/UX + accessibility (M3–M6).
Roadmap: `Docs/ROADMAP.md`. Decisions: `Docs/DECISIONS.md`.
