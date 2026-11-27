# LIGHT SHADOW — Progress Log (Premium Pass)

Running log so any interrupted session can resume. Newest entries at the bottom.
See also: `Docs/ROADMAP.md` (plan), `Docs/DECISIONS.md` (why), `Docs/CHANGELOG.md` (what changed).

## 2026-10-30 — Session 1

- [x] Read all root docs (README, project document §1–12, AI_DEV_LOG) + project dev skill.
- [x] Created branch `feature/premium-pass` from `main` @ 49e22e5.
- [x] Environment audit: Unity 6000.4.0f1 batch-mode available, no editor running, LFS 3.7.1,
      ThirdParty empty, Models/Textures empty, 23 flat materials, 2 quality levels, 4 scenes.
- [x] **Found & fixed critical C1:** missing `Assets/Settings/Build Profiles/Windows.asset`
      blocked every batch run. Added `BuildProfileTool.cs`; stale Library pointer backed up and
      removed; profiles regenerating via `BuildProfileTool.EnsureBatch`.
- [x] Wrote `Docs/AUDIT.md`, `Docs/ROADMAP.md`, `Docs/DECISIONS.md`, this file, `Docs/CHANGELOG.md`,
      `Docs/ASSET_LICENSES.md`, `Docs/PERFORMANCE.md`, root `AGENTS.md`.
- [ ] Baseline `ValidateBatch` PASS re-verified (in flight).
- [ ] Commit Phase 0.
- [x] Launched Sub-Agent 1 (ART), Sub-Agent 2 (ENGINEERING), Sub-Agent 3 (NARRATIVE) in parallel.
      Agent contract: write files only — **no Unity runs, no git**, disjoint file ownership
      (ART: Models/Textures/Materials/ThirdParty/SceneBuilderArt; ENG: Scripts Core/Player/Enemy/
      Gameplay + Systems, Packages/manifest, SceneBuilderSystems; NARRATIVE: Scripts UI/Story/Audio,
      Docs/Story, SceneBuilderStory). All Unity + git work happens here after.
      Contracts pinned in briefs: GameEvents (10 events), SaveSystem (3 slots, storyJson blob),
      PlayerController.ExternalInputLocked static. Reports land in Docs/AGENT_REPORT_*.md.

## 2026-10-30 — Session 1 continued (integration)

- [x] Sub-Agent 2 (ENGINEERING) complete → `Docs/AGENT_REPORT_ENG.md`; committed as
      `feat(engineering)` (event bus, saves, tiers, gamepad, pooling, cinematic shots).
- [x] Sub-Agent 1 (ART) complete → `Docs/AGENT_REPORT_ART.md`; committed as `feat(art)`
      (4 Kenney packs + 8 Poly Haven sets, import pipeline, swap pass, lighting, shader).
- [x] Sub-Agent 3 (NARRATIVE) complete → `Docs/AGENT_REPORT_NARRATIVE.md`; committed as
      `feat(narrative)` (44-node story graph, dialogue UI, 4 endings, saves UI, page-B
      accessibility rows, code mixer, adaptive music).
- [x] Integration compile fixes (verified against installed package sources):
      `Gamepad.leftStickButton`; `UnityEngine.Object.FindFirstObjectByType` in static class +
      `using System` ambiguity → full qualification; `ModelImporterMaterialImportMode/
      Location.External`; removed removed-APIs (`generateColliders`, `generateLightmapUVs`,
      `bounceIntensity`); `LightProbeUsage.BlendProbes`; `FilmGrainLookup.Medium2`;
      `ParticleSystemShapeType.Sphere`; `ReflectionProbe` enums (needs `UnityEngine.Rendering`)
      + `size`; `LightingSettings` is not a ScriptableObject in Unity 6 (`new` + `ao`);
      URP 17.4 has **no Fog volume override** → per-scene `RenderSettings` fog via
      `ArtImporter.ApplySceneFog(bool isDawn)`; `ScriptableObject.CreateInstance` qualifiers.
- [x] Pipeline (all exit 0): BuildAllBatch → ArtImporter.ImportAllBatch → BuildStoryBatch →
      ApplyArtPassBatch → ApplyLightingBatch → **ValidateBatch RESULT: PASS**.
- [x] Builder wiring added so rebuilds can never ship greybox: `BuildAll()` now calls
      `ApplyArtPass(); ApplyLighting(); ConfigureQualityForBuild();` and `BuildSchoolScene`
      builds the cinematic rigs.
- [x] 1007-file integration commit (scenes with 60 ArtModel_ swaps, Mat_Art_* materials,
      Shadow_Art/Hands_Art/DustMotes prefabs, 44 story assets, RP_Low/Medium/High/Ultra).
- [x] GI bake attempt: found and fixed the `-batchmode -quit` vs `BakeAsync` false-success
      race; re-ran without `-quit`. The CPU lightmapper then **stalled** (near-idle process,
      no bake progress for 15+ min) — killed, documented per D11: probes + reflection probes
      + scene fog + static flags ship; lightmap bake remains an open follow-up (works from
      the editor UI: LightShadow ▸ Art ▸ 3, or `BakeLightingBatch` outside batchmode).
- [ ] Windows + Android builds (quality gate) — in flight.

## 2026-10-30 — Session 2 (player-reported fixes, merged to main)

- [x] FPS counter: `FpsCounter` on the HUD top-right (fps + ms, colour-coded),
      settings row "FPS counter" (default on), no per-frame allocation.
- [x] Graphics settings refactor: GameSettings = single graphics writer;
      every row reads LIVE applied values; one Quality tier row (Auto/Low/
      Medium/High/Ultra); Shadows/AA/Render-scale are overrides that return
      to tier on Reset; PerformanceManager slimmed to detect+frame policy.
      Fixed the old fight where defaults re-asserted 4x MSAA over the tier.
- [x] Modern UI theme: `UiThemeTool` generates rounded 9-slice panel/button,
      glow, painted night backdrop (skyline + moon + fog + grain); builders
      restyled; menus/cards/dialogue inherit through shared helpers.
- [x] **Low-FPS root cause found in built-player forensics:** the URP asset
      shipped with GPU Resident Drawer enabled whose resources were absent —
      `CreatePipeline()` threw every frame ("Blitter is already initialized"
      x781 per 22 s), the pipeline never cached. Drawer disabled, SSAO
      feature disabled (resources also stripped + iGPU cost), Intel-iGPU
      tier heuristic added (was mis-read as Ultra). Boot smoke now **0
      errors / 0 warnings / tier=High targetFps=120**.
- [x] URP post shaders pinned to Always Included (post stack ships; passes
      still skip gracefully on Intel drivers that cannot compile UberPost —
      documented in PERFORMANCE.md).
- [x] Windows + Android rebuilt; committed 667fdee + 442315d; merged to main.

## 2026-11-19 — Session 3 — premium integration

All premium phases (P1–P5) landed on the stacked branches and were folded into a
**single integration PR** from `feat/premium-integration`. Open PRs #5
(`feat/blender-hero-props`) and #6 (`feat/release-prep`) were closed as
superseded; merged PRs #3 (`chore/premium-pass-phase0`) and #4
(`feat/audio-mixer-snapshots`, = P1) are already in `main`.

### What landed (per phase)

- **P1 — fonts + AudioMixer + music** (merged via PR #4): `M_Main.mixer`
  (Master > Music/Ambience/SFX/Voice/Shadow/UI with duck children + Master
  low-pass), six snapshots (Explore/Stalk/Chase/Hide/MemoryFlash/Dawn) driven
  by the Shadow FSM, lit-zone muffle and memory beats; bundled OFL fonts
  (Noto Naskh Arabic R/B, Cairo, Reem Kufi, Inter, Special Elite — Arabic now
  renders offline, no OS-font dependency); Kevin MacLeod CC-BY 3.0 music bed
  + per-ending cues under the procedural AdaptiveMusic fallback.
- **P2 — Blender hero props** (14 models wired): `Art/` tree (Sources/
  Pipeline/Previews) with `HeroProps.blend` committed; 14 FBX — SM_Story_
  {MemoryBox, Card, Bookmark, ExamPaper, Prism, Photograph, Programme, Mug},
  SM_Player_Flashlight, SM_Wraith_HD, SM_Arch_{DoorGrille, Chalkboard,
  GateCrank, IronGate} — imported with a `HeroModels` table, story materials
  (Mat_Story_RedInk/Glass/LED), a hero swap table in SceneBuilderArt/Prefabs
  (8 chalkboards incl. QuizBoard, memory box + card, 6 story items, 2 door
  grilles, gate crank, 4 iron-gate segments, player flashlight, Wraith HD on
  the Shadow), scenes rebuilt. ValidateBatch asserts **14/14 hero FBX +
  12/12 materials + per-model swap counts**.
- **P3 — horror & interaction systems** (all 11 systems + wiring + nine
  review fixes): NoiseEmitter noise bus with per-action hearing radii → Shadow
  `Investigate` state; crank minigame (hold + circular swipe, capped and loud);
  MenaceDirector pacing gauge (0–100, forced cool-down, calm guarantee);
  hiding v1 (hide spots, breath meter, Shadow `Search` state, hiding state
  cleared on catch/checkpoint, no catch through the spot); banish rebalance
  (hold the focused beam 2 s at charge ≥ 25 → 20 s retreat, GameConfig fields);
  calm FX (vignette, heartbeat, beam tremor); 360° inspect view for story
  items; study-recap PNG export; Android haptics (light = correct, heavy =
  caught); dev console (editor builds only). Nine review fixes on top
  (hiding-state reset, F tap-vs-crank split, touch hold vs input lock, shared
  sight Linecast cache, calm-FX VolumeProfile teardown, EnemyStateChanged
  comment, recap Debug.Log removal, crank ratchet clock, defensive
  `VolumeCategories.SetMixer` port).
- **P4 — Arabic questions 60/60**: `questionAr`/`optionsAr`/`explanationAr`
  filled for all 60 questions (AI translation, Egyptian-flavoured MSA in the
  mother's voice; append-only YAML edit, GUIDs untouched); ValidateBatch now
  asserts "Question Arabic fields: 60/60". **Pending human/native review**;
  `factSource` stays `NEEDS-FACT-CHECK`. Expansion of the bank to 120 questions
  is **in progress** (separate agent).
- **P5 — release assets**: flashlight-emblem app icon pipeline
  (`Art/Pipeline/ls_icon.py`, Blender EEVEE headless → 1024/512/192/96 PNG;
  PlayerSettings assignment documented, still a human step); EN/AR store
  listing + IARC notes + privacy blurb (`Docs/STORE_LISTING.md`); VO recording
  specs (`Docs/VO_RECORDING_SPECS.md`, 96 delivered files EN+AR per the
  line-count table).

### Verification results

- `ValidateBatch` **RESULT: PASS** on the integration branch (asserts the P2
  hero FBX/material/swap counts and the P4 Arabic 60/60 fields, plus the
  P1 mixer wiring; scenes rebuilt after layout changes).
- CI `build` workflow green on every merged PR: #3 and #4. Job wall-clock
  measured via the API on the last green runs (runner-load dependent):
  Windows 7m12s–8m57s, arm64 9m45s–18m28s, armv7 8m44s–16m08s
  (runs 44592065316, 44608154090, 44611064124); at merge time PRs #3/#4 were
  reported around arm64 9m36s / armv7 9m29s / windows 7m48s–8m57s.
- CI artifact sizes (run 44608154090, PR #4 = P1 content, zipped artifacts):
  arm64-v8a 88,040,419 B (88.0 MB), armeabi-v7a 86,145,163 B (86.1 MB),
  Windows 152,043,678 B (152.0 MB) — full table and the delta vs the 73.7 MB
  APK baseline recorded in `Docs/PERFORMANCE.md`.
- No Unity runs were used for this documentation pass (docs-only, fast-checks
  are the gate).

### Remaining open items

- **The single integration PR itself** — pending merge (branch
  `feat/premium-integration`; its own CI build run not started at the time of
  writing).
- **Human-only:** question fact-check (60 + the 120 expansion), Arabic native
  review, VO recording, PlayerSettings icon assignment, store screenshots
  (need Unity runs), on-device profiling, signed AAB (owner keystore), store
  accounts.
- **Deferred (not started, no commits found):** audio reverb/occlusion pass
  (checked `feat/horror-systems` — nothing landed); remaining master-plan P2
  props (optics bench, circuit puzzle, 3 fuse boxes, card catalogue) and the
  modular key-architecture pass; GI lightmap bake (needs a graphics-device
  editor run); Pixabay music primaries (bot-protected downloads).

## Resume notes for the next session

1. Check `git log` + this file; the working branch is the stacked
   `feat/docs-final` (docs reconciliation) to be folded into the single
   integration PR from `feat/premium-integration`.
2. Verify `Assets/Settings/Build Profiles/{Windows,Android}.asset` exist; if a batch run ever
   aborts with "Failed to read the specified build profile", rerun
   `Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod LightShadow.Tools.BuildProfileTool.EnsureBatch`.
3. Verification commands (see `AGENTS.md`): `ValidateBatch`, `BuildAllBatch`,
   `BuildWindowsBatch`, `BuildAndroidBatch`.
4. Quality gates live in the premium-pass brief; open items are reconciled in
   `Docs/TASKS.md` (human-only + deferred lists at the bottom).
