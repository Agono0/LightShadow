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

## Resume notes for the next session

1. Check `git log` + this file; the branch is `feature/premium-pass`.
2. Verify `Assets/Settings/Build Profiles/{Windows,Android}.asset` exist; if a batch run ever
   aborts with "Failed to read the specified build profile", rerun
   `Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod LightShadow.Tools.BuildProfileTool.EnsureBatch`.
3. Verification commands (see `AGENTS.md`): `ValidateBatch`, `BuildAllBatch`,
   `BuildWindowsBatch`, `BuildAndroidBatch`.
4. Quality gates live in the premium-pass brief; status tracked at the bottom of `Docs/ROADMAP.md` (P4).
