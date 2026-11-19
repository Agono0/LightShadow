# LIGHT SHADOW — Reconciled Task Backlog

**Last updated:** 2026-11-19 · **Branch:** `chore/premium-pass-phase0`

The live backlog reconciled from the adopted master plan
(`Docs/MASTER_PLAN.md`): everything already shipped (see `Docs/PROJECT_STATE.md`,
`Docs/ROADMAP.md`) is NOT re-listed here. Each task is one line and names its
owning phase. `[x]` = done; `[ ]` = pending. Verification for every phase:
`ValidateBatch` (`RESULT: PASS`) for runtime/editor changes, `BuildAllBatch`
first when scene/prefab layout changes, and the PR `build` workflow green
before merge (AGENTS.md, Git workflow).

## P0 — Housekeeping (branch `chore/premium-pass-phase0`)

- [x] Fix the release job x86_64 APK rename loop (leftover code broke the
      `v0.1.0-ci-test` release) — fixed on this branch. *(P0)*
- [x] Tag cleanup for `v0.1.0-ci-test`. *(P0)*
- [ ] Commit the missing generated assets scenes reference (SM_Wraith,
      SM_BeamCone, reflection-probe bakes) after the ArtImporter rerun. *(P0)*
- [ ] Write the adopted-plan documentation set (MASTER_PLAN / PROJECT_STATE /
      TASKS / STYLE_GUIDE / CREDITS + README/ROADMAP updates). *(P0)*
- [ ] README/ROADMAP hygiene: statuses accurate, stale claims removed, links
      resolve. *(P0)*

## P1 — Fonts, AudioMixer and music

- [ ] Bundle OFL fonts with fallback chains: Arabic body + display
      (Noto Naskh Arabic, Reem Kufi/Cairo class) and Latin UI/notes fonts
      (Inter, typewriter/handwriting styles); Arabic stops depending on OS
      fonts. *(P1)*
- [ ] Replace the code-level category mixer with a real AudioMixer asset
      (Master > Music/SFX/Ambience/Voice/UI) and wire the existing settings
      sliders to it (supersedes D15's interim mixer). *(P1)*
- [ ] AudioMixer snapshots (Explore / Stalk / Chase / Hide / Memory / Dawn)
      driven from the Shadow FSM and story moments. *(P1)*
- [ ] Real music: per-wing drones + ending cues from a licensed/CC0 source
      (D9 logged in ASSET_LICENSES) with the procedural AdaptiveMusic kept as
      fallback. *(P1)*

## P2 — Blender hero props + key architecture

- [ ] Set up the planned repo-root `Art/` tree (Sources / Pipeline /
      Previews) for Blender work outside `Assets/` (plan §6.1). *(P2)*
- [ ] Blender hero props per the master-plan list (chalkboard, card catalogue,
      optics bench, circuit puzzle, 3 fuse boxes, story items incl. the
      memory box and the crank flashlight) using the §7.4 cleanup recipe
      (tri budgets, UVs, LODs, naming). *(P2)*
- [ ] Key architecture pass: replace remaining primitive architecture with
      modular kit pieces (green-dado walls, iron-grille windows, staircases,
      courtyard arches); tiled PBR stays on primitives elsewhere. *(P2)*
- [ ] Rebuild scenes via the SceneBuilder partials after each art batch and
      keep the 60 existing swaps intact (D3). *(P2)*

## P3 — Horror & interaction systems

- [ ] Noise bus: `NoiseEmitter` events (sprint/walk/crouch/door/drop/wrong
      buzzer) with per-action hearing radii feeding the Shadow's hearing. *(P3)*
- [ ] Shadow `Investigate` state: go to the last heard noise, then resume
      patrol; tunes with the noise bus. *(P3)*
- [ ] Crank minigame: hold + circular swipe emergency charge capped low and
      loud (radii above), per plan §4.6. *(P3)*
- [ ] Menace director: gauge 0–100 that paces sightings and forces a
      cool-down exit at 100; guarantees calm after chases (plan §4.12). *(P3)*
- [ ] Hiding v1: hide spots (lockers/desks/curtains) with hold-breath when the
      Shadow is close; Search checks 1–2 spots. *(P3)*
- [ ] Banish rebalance: hold the focused beam 2 s with charge ≥ 25 → 20 s
      retreat (GameConfig fields, no literals). *(P3)*
- [ ] Calm FX: heartbeat, vignette and beam-tremor driven by the calm stat. *(P3)*
- [ ] 360° inspect view for story items (Inspectable close-up). *(P3)*
- [ ] Study-recap PNG export (shareable/printable from the recap screen). *(P3)*
- [ ] Haptics: light on correct answers, heavy on caught (Android). *(P3)*
- [ ] Dev console: set flags/stats, teleport, skip to act, toggle AI (editor
      builds only). *(P3)*

## P4 — Arabic content + credits

- [ ] Fill Arabic question fields (`questionAr`/`optionsAr`/`explanationAr`)
      for all 60 questions; display path unchanged (Localization +
      ArabicSupport). *(P4)*
- [ ] Credits screen wired to `Docs/CREDITS.md` content (in-game screen, EN/AR
      chrome). *(P4)*

## P5 — Performance, release prep and VO specs

- [ ] Performance doc refresh from CI build numbers + any device profiling
      that lands (Docs/PERFORMANCE.md); on-device profiling remains
      human-assisted. *(P5)*
- [ ] App icons (Windows + Android, all densities). *(P5)*
- [ ] Store assets: screenshots, store text EN/AR, privacy statement. *(P5)*
- [ ] VO recording specs: mic/performance/format/normalization requirements
      (dialogue −16 LUFS, SFX peaks −3 dBFS) so SAPI TTS can be swapped for
      human VO later. *(P5)*
- [ ] Optional signed AAB job for Google Play (requires owner signing
      config). *(P5)*
