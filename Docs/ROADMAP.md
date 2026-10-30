# LIGHT SHADOW — Premium Pass Roadmap

**Owner:** Lead TD · **Branch:** `feature/premium-pass` · **Last updated:** 2026-10-30

Priority order: fix criticals → art/visual identity → performance & platform targets →
story depth → integration gates. Criticals (audit C1, build profiles) were fixed first.

## P0 — Criticals & infrastructure (done first)

- [x] C1: repair missing build profiles (`BuildProfileTool.cs`) so any batch run is possible again.
- [x] Audit + roadmap + decisions + progress docs; `AGENTS.md` rebuilt (was missing).
- [x] Verify baseline `ValidateBatch` PASS again on the repaired project.

## P1 — Art & visual identity (Sub-Agent 1: ART)

- [x] Source CC0 models for the school kit — 4 Kenney packs (furniture/nature/graveyard/
      blocky-characters, 578 FBX); Quaternius skipped (Google-Drive-gated, logged).
- [x] CC0 PBR texture sets — 8 Poly Haven 2K sets (24 maps) for plaster/wood/tile/concrete/
      metal/wood-door/grass/fabric.
- [x] Material pipeline: `ArtImporter.ImportAllBatch` — platform-correct texture imports
      (2048 PC / 1024 Android), roughness→smoothness twins, URP/Lit `Mat_Art_*` materials.
- [x] Model integration: `SceneBuilderArt.ApplyArtPass` — idempotent name-keyed swaps
      (60 `ArtModel_` meshes live in 02_School), colliders/labels/counted objects untouched.
- [x] Shadow visual upgrade (smoke-silhouette shader on a real humanoid + halo) and
      first-person hands (procedural grip — no auth-free CC0 arm exists; documented).
- [x] Lighting: static flags, light-probe grids per wing, 3 reflection probes, scene fog;
      GI bake tooling shipped (`BakeLightingBatch`) — batch bake stalled on this machine and
      is documented as an open follow-up (D11 honest fallback: probes ship regardless).
- [x] Post/VFX: post-profile fog/bloom/grain tuning, dust motes; flashlight flare deferred
      (LensFlare asset authoring — documented follow-up).
- [x] `Docs/ASSET_LICENSES.md` — every asset logged (name, URL, license, author).

## P2 — Engineering & performance (Sub-Agent 2: ENGINEERING)

- [x] Quality tiers Low/Medium/High/Ultra with auto-detection, override pref, settings row,
      RP asset variants (RP_Low…Ultra) built for the build pipeline.
- [x] 120 FPS path: Windows 120 + vSync 0; Android display-refresh clamp 60–120 with 60
      fallback; honest per-device framing in `Docs/PERFORMANCE.md` (D8).
- [x] Dynamic resolution via `DynamicResolutionHandler`; tier retune of the live URP asset
      (render scale/MSAA/shadow distance/soft shadows); static batching flags in scenes.
- [x] ObjectPool + PooledFx (flash flare, whisper burst) with kill switch; zero-alloc audit
      (2 fixes: cached GetMeter, battery event dedup).
- [x] Gamepad support through the Input System (gameplay polling layer; D7 additive).
- [x] Cinemachine 3.1.4: dormant catch/dawn/gate rigs + reflection-gated runtime shots;
      gameplay FP rig untouched between shots (D6).
- [x] Save system: 3 JSON slots + settings, autosave at checkpoints/gate, `ApplyLoadedState`,
      storyJson blob hooks; documented restore semantics.
- [x] Event bus (`GameEvents`) with frozen contract, raised from 9 gameplay sites.
- [x] `Docs/PERFORMANCE.md`: targets, methodology, measured rows (Lead-filled).

## P3 — Narrative, UI/UX & audio (Sub-Agent 3: NARRATIVE)

- [x] `StoryDirector`: SO-graph branching system (nodes, choices, timed choices, flags,
      boldness/trust/calm stats) — zero story text in runtime code.
- [x] Story content under `Docs/Story/` (bible + 44-node script) built into 44 `SN_*.asset`
      + `SO_StoryConfig`; consequence spine (`knows_backstairs` dawn shortcut).
- [x] Dialogue UI: typewriter, speaker tag, hover/press choice feedback, timed-choice bar,
      controller navigation, quiz-priority hold.
- [x] Endings wiring: 4 endings (D13) via `SO_StoryConfig` thresholds; per-ending
      EndingController texts/tints; recap ending row + REPLAY.
- [x] Save-slot UI (3 slots + continue/delete-confirm), loading screen with progress.
- [x] Accessibility: text speed, subtitle size, colourblind-safe palettes, auto-advance,
      skip-hold (settings page B, 11 rows).
- [x] Audio: code-level category mixer (D15) with 5 sliders, AdaptiveMusic tension layers.
- [x] Question bank: 60 questions retained; accuracy feeds the ending formula.

## P4 — Integration, polish & gates (Lead TD)

- [x] Merge agent work, resolve API drift (7 fixes verified against package sources), compile clean.
- [x] `BuildAllBatch` + `ValidateBatch` **RESULT: PASS** with all content integrated.
- [x] No pink materials (FBX materials converted to URP/Lit at import); greybox visuals
      replaced in shipped scenes (colliders remain by design).
- [ ] Windows build + Android build (IL2CPP, ARM64) both succeed — in flight.
- [ ] Docs finalized (PERFORMANCE numbers, CHANGELOG, PROGRESS, AI_DEV_LOG entries).
- [ ] Final commit + report; merge `feature/premium-pass` → `main`.
