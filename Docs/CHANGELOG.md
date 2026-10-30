# LIGHT SHADOW — Changelog (Premium Pass)

All notable changes to code, scenes, docs, and settings since `main` @ 49e22e5.
Format follows Keep a Changelog; newest entries at the bottom.

## [0.4.0-premium] — 2026-10-30 — Integration: art, story, systems land in the game

### Added
- Real 3D art in the generated scenes: 60 `ArtModel_` mesh swaps (Kenney furniture/foliage/
  fence/silhouette), tiled PBR wall/floor/ceiling materials, dust motes, garden decor,
  iron fence, smoke-silhouette Shadow, procedural gripped hands.
- Branching story system: 44-node graph (12 authored moments, timed choices, stats),
  4 endings, dialogue UI with typewriter + hover/press feedback, EN/AR story strings.
- Save/load: 3 slots, autosave at checkpoints/gate, main-menu slot UI, loading screen.
- Quality tiers Low/Medium/High/Ultra + RP asset variants + auto-detection + settings row;
  120 FPS frame policy; dynamic resolution; gamepad support; object-pooled FX.
- Lighting: static flags, light-probe grids, 3 baked reflection probes, scene fog
  (night/dawn), GI bake batch (honest `-nographics` failure).
- Settings page B: text speed, subtitle size, colourblind palette, auto-advance, skip hold,
  quality tier, 5 volume sliders; recap gains ending row + REPLAY.

### Fixed
- Seven Unity-6 API drifts across agent code (see `Docs/PROGRESS.md` list) — all verified
  against installed package sources.
- `-batchmode -quit` raced `Lightmapping.BakeAsync` (false-success bake); tool now runs
  without `-quit` and exits itself on the bakeCompleted callback.

## [0.1.0-premium] — 2026-10-30 — Phase 0: audit & infrastructure

### Fixed
- **Critical C1:** missing `Assets/Settings/Build Profiles/Windows.asset` made every batch
  Unity run abort at startup (`Failed to read the specified build profile`). Added
  `Assets/_Project/Scripts/Tools/Editor/BuildProfileTool.cs`
  (`LightShadow ▸ 7. Repair Build Profiles`, batch entry `LightShadow.Tools.BuildProfileTool.EnsureBatch`)
  that recreates `Windows.asset` + `Android.asset` idempotently via the Unity 6
  `UnityEditor.Build.Profile` API and re-selects the active profile. Stale machine-local
  `Library/EditorUserBuildSettings.asset` pointer backed up to `*.bak-premium` and reset.

### Added
- `Docs/AUDIT.md` — full project audit (findings C1, P1–P5, F1–F5, M1–M7).
- `Docs/ROADMAP.md` — prioritized premium-pass roadmap (P0–P4).
- `Docs/DECISIONS.md` — decision log D1–D14.
- `Docs/PROGRESS.md` — resumable progress log.
- `Docs/CHANGELOG.md` — this file.
- `Docs/ASSET_LICENSES.md` — asset license registry (populated by the art pass).
- `Docs/PERFORMANCE.md` — performance ledger (populated by the engineering pass).
- `AGENTS.md` — rebuilt project map (was referenced by tooling but missing).
- `Docs/Story/` — narrative workspace for the expanded story.

### Changed
- (docs) `README.md` gains a Docs/ section pointer.
