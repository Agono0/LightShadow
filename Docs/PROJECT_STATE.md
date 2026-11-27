# LIGHT SHADOW — Project State

**Snapshot date:** 2026-11-19 · **Branch:** `feat/premium-integration` (single integration PR pending) · **Main:** post-PR #4

Current-state snapshot for the adopted master production plan
(`Docs/MASTER_PLAN.md`). Keep this file current after every task; pair it with
the live backlog in `Docs/TASKS.md`.

## Status

- **A single integration PR from `feat/premium-integration` is being prepared**
  carrying premium phases P1–P5; open PRs #5 (`feat/blender-hero-props`) and #6
  (`feat/release-prep`) were closed as superseded. PRs #3
  (`chore/premium-pass-phase0`) and #4 (`feat/audio-mixer-snapshots`, = P1) are
  already merged into `main`.
- Branch protection requires the fast-checks plus the `build` workflow
  (Windows player + Android arm64/armv7 APKs) to be green on every PR; merges
  are squash, PR-only (AGENTS.md rule 9). The build workflow was green on the
  merged PRs #3/#4 (measured CI numbers in `Docs/PERFORMANCE.md`).
- The full game loop is playable end-to-end from generated scenes
  (`00_Bootstrap → 01_MainMenu → 02_School → 03_Ending`), now with the
  horror-systems layer, the Blender hero props, the real mixer/music and the
  filled Arabic question fields on the integration branch.

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
- **Quiz loop:** 60-question bank (Q_Geo/Hea/Lan/His/Mat/Sci) with Arabic
  fields filled 60/60 (AI translation, pending human/native review), 20 s
  timer, BrightIdeaMeter (+25/−10) driving flashlight range/intensity; bank
  expansion to 120 in progress (separate agent).
- **Shadow FSM:** Patrol → Stalk → Chase → Repelled plus Investigate, Search
  and ForcedCooldown states — NavMesh, LOS raycast, lit-zone exclusion, a
  NoiseEmitter noise bus with per-action hearing radii, the MenaceDirector
  pacing gauge, hiding v1 (hide spots, breath meter, Search checks) and the
  banish hold (2 s focused beam at charge ≥ 25 → 20 s retreat), all under the
  SM_Wraith_HD Blender silhouette.
- **Horror interaction layer:** crank minigame (hold + circular swipe), calm
  FX (vignette/heartbeat/beam tremor from the calm stat), 360° inspect view
  for story items, study-recap PNG export, Android haptics (light = correct,
  heavy = caught), dev console (editor builds only).
- **Hero props:** 14 Blender models wired through builder swap tables — 8
  story items (memory box + card, bookmark, exam paper, prism, photograph,
  programme, mug), player flashlight, Wraith HD, door grilles, chalkboards
  (incl. the QuizBoard board), gate crank, iron-gate segments — validated
  14/14 FBX + 12/12 materials in ValidateBatch.
- **Quality tiers:** Low/Medium/High/Ultra with auto-detect, per-tier URP
  assets, dynamic resolution, 120 FPS Windows / refresh-rate-clamped Android
  policy (D8).
- **Gamepad:** Input System polling layer alongside classic-Input KB/M (D7).
- **Art:** 60 Kenney CC0 mesh swaps live in 02_School, 8 Poly Haven 2K PBR
  surface sets (plaster/wood/tile/concrete/metal/door-wood/grass/fabric),
  light-probe grids, reflection probes, scene fog, dust motes.
- **Audio:** real packs for footsteps/quiz/door/power (Kenney CC0), Shadow
  moans (qubodup CC0), school-night ambience (tcarisland CC-BY), a real
  AudioMixer (`M_Main.mixer`, Master > Music/Ambience/SFX/Voice/Shadow/UI)
  with **six snapshots** (Explore/Stalk/Chase/Hide/MemoryFlash/Dawn) driven by
  the Shadow FSM, lit zones and memory beats; settings sliders write the
  exposed mixer params; Kevin MacLeod CC-BY 3.0 music bed + per-ending cues
  layered over the procedural AdaptiveMusic fallback.
- **Fonts:** bundled OFL/Apache fonts (Noto Naskh Arabic, Cairo, Reem Kufi,
  Inter, Special Elite) — Arabic renders offline, no OS-font dependency.
- **TTS:** SAPI narrator reads run narration, fragment memories and the ending
  (Windows; EN always, AR only when an Arabic OS voice exists).
- **CI:** build workflow (Windows + arm64/armv7 APKs in parallel, Library
  cached) and release job on tags; fast-checks gate docs/CI-only changes.

## Known live issues

- The single integration PR (`feat/premium-integration`) is pending — its own
  CI build run (and hence the integration APK size delta) has not completed at
  snapshot time; see `Docs/PERFORMANCE.md` for the honest state of the numbers.
- The Arabic question fields are AI-translated (60/60 filled) and **await a
  human/native review** before any release claim.
- Only a small fraction of the imported pack clips is actually used (15 SFX +
  1 ambience loop at last count, plus the 3 music tracks); the rest is dormant
  headroom, not a bug.
- GI lightmap bake is tooling-complete (`BakeLightingBatch`) but stalls
  headless on this machine; probes ship as the honest fallback (D11).
- Audio reverb/occlusion and the modular key-architecture pass are deferred
  (no commits; see `Docs/TASKS.md` open items).
- Question bank expansion to 120 is in progress on a separate agent's branch.

## Human-only pending

- **Question fact-check:** every question (the 60 shipped plus the 120-bank
  expansion in progress) still carries `factSource = NEEDS-FACT-CHECK`. A
  human must verify each against a real reference before any public release
  (AGENTS.md rule 5 — never claim review).
- **Arabic native review:** the EN/AR interface works and the Arabic question
  fields are filled (AI translation), but both need a native-speaker pass
  before they are called reviewed.
- **VO recording:** the Shadow, Nour and the mother are TTS/synthesised
  placeholders; human VO waits on recording — specs ready in
  `Docs/VO_RECORDING_SPECS.md` (96 files EN+AR).
- **PlayerSettings icon assignment:** the icon PNGs are rendered
  (`Art/Previews/icon_{1024,512,192,96}.png`) and the wiring steps are
  documented in `Art/Pipeline/ls_icon.py`, but the in-editor assignment is a
  human step (ProjectSettings untouched).
- **Store screenshots:** EN + AR device captures per store size sheets need
  Unity runs (`Docs/STORE_LISTING.md` has the text side done).
- **On-device profiling:** frame-rate/tier verification on real Android
  hardware remains human-assisted (`Docs/PERFORMANCE.md` methodology).
- **Store accounts & signing:** Google Play (and any other distribution)
  credentials, the signing keystore for the optional signed AAB job, and
  release tagging are human-only.

## Next

- Live backlog with owners: `Docs/TASKS.md` — phases P0 (housekeeping) →
  P5 (release prep), reconciled from the adopted master plan.
- Canon: `Docs/MASTER_PLAN.md` (read the reconciliation header first).
- Performance targets and measured numbers: `Docs/PERFORMANCE.md`.
- Decision log: `Docs/DECISIONS.md`; asset licenses: `Docs/ASSET_LICENSES.md`.
