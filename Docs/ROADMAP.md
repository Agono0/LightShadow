# LIGHT SHADOW — Premium Pass Roadmap

**Owner:** Lead TD · **Branch:** `feature/premium-pass` · **Last updated:** 2026-10-30

Priority order: fix criticals → art/visual identity → performance & platform targets →
story depth → integration gates. Criticals (audit C1, build profiles) were fixed first.

## P0 — Criticals & infrastructure (done first)

- [x] C1: repair missing build profiles (`BuildProfileTool.cs`) so any batch run is possible again.
- [x] Audit + roadmap + decisions + progress docs; `AGENTS.md` rebuilt (was missing).
- [ ] Verify baseline `ValidateBatch` PASS again on the repaired project.

## P1 — Art & visual identity (Sub-Agent 1: ART)

- [ ] Source CC0 models (Kenney/Quaternius/Poly Haven) for the school kit: walls/floor dressing,
      desks, chairs, lockers, shelves, doors, lamps, fuse boxes, gate, fences, foliage.
- [ ] CC0 PBR texture sets (Poly Haven / ambientCG) for the recurring surfaces: plaster wall,
      classroom floor, corridor tile, wood, chalkboard, metal, concrete.
- [ ] Material pipeline: editor tool that imports textures with correct platform settings
      (ASTC for Android, BC7/DXT for Windows), builds URP/Lit PBR materials, replaces the
      23 flat-colour materials without renaming (GUIDs preserved).
- [ ] Model integration: extend the SceneBuilder with an art-swap layer — generated props use
      imported meshes instead of primitives; primitives remain only as invisible colliders.
- [ ] First-person hand upgrade (real mesh + grip animation), Shadow visual upgrade
      (smoke silhouette shader/soft particles on a real silhouette mesh).
- [ ] Lighting: attempt baked GI for static architecture (moon + permanent fixtures),
      light probes + reflection probes per wing, keep runtime fuse lights realtime.
- [ ] Post/VFX polish: fog/dust motes, flashlight flare, dawn god-rays; volumetric look where affordable.
- [ ] `Docs/ASSET_LICENSES.md` — every downloaded asset logged (name, URL, license, author).

## P2 — Engineering & performance (Sub-Agent 2: ENGINEERING)

- [ ] Quality tiers Low/Medium/High/Ultra (URP asset tiers + QualitySettings) with hardware
      auto-detection on first launch and manual override in the settings card.
- [ ] 120 FPS path: per-platform `Application.targetFrameRate`, Windows uncapped-vsync option,
      Android 120Hz request + stable-60 mid-range tier; document device reality.
- [ ] Dynamic resolution, SRP Batcher/instancing/static batching audit, LODs on heavy props,
      culling layers, texture streaming check.
- [ ] Object pooling service (VFX, ambience, pickups) + zero-allocation hot-path audit.
- [ ] Gamepad support through the Input System (menus + gameplay), touch layer unchanged.
- [ ] Cinemachine 3: cinematic catch/dawn/ending shots with damping + collision, gameplay
      camera stays the existing first-person rig.
- [ ] Save system: 3 slots + settings, JSON under `Application.persistentDataPath`, persists
      story flags/fragments/fuses/meter/battery/position; autosave at checkpoints; menu UI hooks.
- [ ] Event bus (`GameEvents`) so narrative/audio/UI react without hard coupling.
- [ ] Profiling evidence + before/after numbers in `Docs/PERFORMANCE.md`.

## P3 — Narrative, UI/UX & audio (Sub-Agent 3: NARRATIVE)

- [ ] `StoryDirector`: ScriptableObject-graph branching story system (nodes, choices, timed
      choices, flags, stats), no hard-coded story logic in scripts.
- [ ] Story content under `Docs/Story/`: expanded acts, Nour's inner monologue, memory flashes,
      branching moments with consequences, reputation/stat tracking, **multiple endings**.
- [ ] Dialogue UI: typewriter box, choice buttons with hover/press feedback, timed-choice ring.
- [ ] Endings wiring: ending determined by story state (fragments, accuracy, shadow encounters);
      recap screen per ending.
- [ ] Save-slot UI, loading screen, safe-area audit, 16:9 / 19.5:9 / tablet layouts.
- [ ] Accessibility: text speed, subtitle size, colourblind-safe palette, skip/auto options.
- [ ] Audio: mixer groups (Music/Ambience/SFX/UI) with volume sliders, tension-reactive
      procedural music layer, UI sounds; voice-ready dialogue hooks.
- [ ] Question bank: keep 60 verified-format questions; story-consequence quiz weighting.

## P4 — Integration, polish & gates (Lead TD)

- [ ] Merge agent work, resolve conflicts, compile clean.
- [ ] `BuildAllBatch` + `ValidateBatch` PASS; zero console errors on boot (play-mode smoke).
- [ ] No pink materials; no placeholder primitives visible in shipped scenes (colliders ok).
- [ ] Windows build + Android build (IL2CPP, ARM64) both succeed.
- [ ] Quality gates checklist from the brief; docs finalized (PERFORMANCE, LICENSES, CHANGELOG).
- [ ] Final commit + report; merge `feature/premium-pass` → `main`.
