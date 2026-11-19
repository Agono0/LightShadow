# LIGHT SHADOW — Project State

**Snapshot date:** 2026-11-19 · **Branch:** `chore/premium-pass-phase0` · **Main:** `928bc77`

Current-state snapshot for the adopted master production plan
(`Docs/MASTER_PLAN.md`). Keep this file current after every task; pair it with
the live backlog in `Docs/TASKS.md`.

## Status

- `main` is at `928bc77`; branch protection requires the fast-checks plus the
  `build` workflow (Windows player + Android arm64/armv7 APKs) to be green on
  every PR; merges are squash, PR-only (AGENTS.md rule 9).
- CI/CD is live: automated Windows and Android builds on every PR, artifacts
  uploaded, and a release job on version tags.
- The full game loop is playable end-to-end from generated scenes
  (`00_Bootstrap → 01_MainMenu → 02_School → 03_Ending`).

## What works

- **Story:** 44-node branching graph (D4) with 12 authored choice moments,
  timed choices, boldness/trust/calm stats (−1…+1), calm-gated routing, and a
  consequence spine (`knows_backstairs` dawn shortcut, `left_all` gate epilogue).
- **Story UI:** typewriter dialogue box, speaker tags, timed-choice bar,
  controller navigation, quiz-priority hold.
- **Endings:** all 4 endings (Full Recovery / Partial Memory / Hollow Escape /
  Dawnless) selected by `SO_StoryConfig` thresholds (D13), each with its own
  texts and dawn tint on the 3D ending set.
- **Saves:** 3 disk slots + settings under `Application.persistentDataPath`
  (D12), autosave at checkpoints and the gate, story state crosses saves via
  the storyJson blob; save-slot UI with continue/delete-confirm and a loading
  screen.
- **Quiz loop:** 60-question bank (Q_Geo/Hea/Lan/His/Mat/Sci), 20 s timer,
  BrightIdeaMeter (+25/−10) driving flashlight range/intensity.
- **Shadow FSM:** Patrol → Stalk → Chase → Repelled with NavMesh, LOS raycast,
  lit-zone exclusion, noise ranges, procedural smoke-silhouette body and voice.
- **Quality tiers:** Low/Medium/High/Ultra with auto-detect, per-tier URP
  assets, dynamic resolution, 120 FPS Windows / refresh-rate-clamped Android
  policy (D8).
- **Gamepad:** Input System polling layer alongside classic-Input KB/M (D7).
- **Art:** 60 Kenney CC0 mesh swaps live in 02_School, 8 Poly Haven 2K PBR
  surface sets (plaster/wood/tile/concrete/metal/door-wood/grass/fabric),
  light-probe grids, reflection probes, scene fog, dust motes.
- **Audio:** real packs for footsteps/quiz/door/power (Kenney CC0), Shadow
  moans (qubodup CC0), school-night ambience (tcarisland CC-BY), code-level
  category mixer with 5 sliders (D15) and AdaptiveMusic tension layers;
  procedural fallback keeps music working with zero external files.
- **TTS:** SAPI narrator reads run narration, fragment memories and the ending
  (Windows; EN always, AR only when an Arabic OS voice exists).
- **CI:** build workflow (Windows + arm64/armv7 APKs in parallel, Library
  cached) and release job on tags; fast-checks gate docs/CI-only changes.

## Known live issues

- Scenes reference generated art assets not yet committed on this branch:
  `SM_Wraith` (procedural Shadow mesh), `SM_BeamCone` (flashlight beam cone)
  and the reflection-probe bakes. The `ArtImporter` rerun is in flight on this
  branch; the generated assets must be committed before merge (see
  `Docs/TASKS.md` P0).
- The CI release job failed on tag `v0.1.0-ci-test` because of a leftover
  x86_64 APK rename loop. The fix is already on this branch (`Docs/TASKS.md`
  P0 marks it done).
- No bundled fonts — Arabic display depends on OS-installed fonts.
  Bundling OFL Arabic/Latin fonts is Phase 1 in `Docs/TASKS.md`.
- Music is 100% procedural (AdaptiveMusic synthesis); the planned real music
  cues/per-wing drones are Phase 1.
- Only 16 of the 287 audio clips in the imported packs are actually used
  (15 SFX + 1 ambience loop); the rest are dormant headroom, not a bug.
- GI lightmap bake is tooling-complete (`BakeLightingBatch`) but stalls
  headless on this machine; probes ship as the honest fallback (D11).

## Human-only pending

- **Question fact-check:** all 60 questions still carry
  `factSource = NEEDS-FACT-CHECK`. A human must verify each against a real
  reference before any public release (AGENTS.md rule 5 — never claim review).
- **Arabic native review:** the EN/AR interface works, but story/question
  Arabic needs a native-speaker pass; question Arabic fields
  (`questionAr`/`optionsAr`/`explanationAr`) are unfilled (Phase 4).
- **VO recording:** the Shadow, Nour and the mother are TTS/synthesised
  placeholders; human VO waits on recording (specs due in Phase 5).
- **Store accounts:** Google Play (and any other distribution) credentials and
  signing keys are human-only.

## Next

- Live backlog with owners: `Docs/TASKS.md` — phases P0 (housekeeping) →
  P5 (release prep), reconciled from the adopted master plan.
- Canon: `Docs/MASTER_PLAN.md` (read the reconciliation header first).
- Performance targets and measured numbers: `Docs/PERFORMANCE.md`.
- Decision log: `Docs/DECISIONS.md`; asset licenses: `Docs/ASSET_LICENSES.md`.
