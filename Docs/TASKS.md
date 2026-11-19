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
- [x] Commit the missing generated assets scenes reference (SM_Wraith,
      SM_BeamCone, reflection-probe bakes) after the ArtImporter rerun. *(P0)*
      — covered by the premium-pass-phase0 PR 3 (commit `3638000`).
- [x] Write the adopted-plan documentation set (MASTER_PLAN / PROJECT_STATE /
      TASKS / STYLE_GUIDE / CREDITS + README/ROADMAP updates). *(P0)*
      — covered by the premium-pass-phase0 PR 3.
- [ ] README/ROADMAP hygiene: statuses accurate, stale claims removed, links
      resolve. *(P0)*

## P1 — Fonts, AudioMixer and music

- [x] Bundle OFL fonts with fallback chains: Arabic body + display
      (Noto Naskh Arabic, Reem Kufi/Cairo class) and Latin UI/notes fonts
      (Inter, typewriter/handwriting styles); Arabic stops depending on OS
      fonts. *(P1)* — fonts bundled + wired in this PR
      (`feat/audio-mixer-snapshots`); Arabic titles use Noto Naskh Bold because
      Cairo/Reem Kufi lack the Presentation Forms-B glyphs the ArabicSupport
      shaper emits (SOURCING doc Part 1); OS-font fallback retained.
- [x] Replace the code-level category mixer with a real AudioMixer asset
      (Master > Music/SFX/Ambience/Voice/UI) and wire the existing settings
      sliders to it (supersedes D15's interim mixer). *(P1)* — built by
      `AudioMixerTool.EnsureBatch` (M_Main.mixer + Shadow/Voice groups), sliders
      drive exposed group params via `VolumeCategories` (D15 follow-up spec).
- [x] AudioMixer snapshots (Explore / Stalk / Chase / Hide / Memory / Dawn)
      driven from the Shadow FSM and story moments. *(P1)* — six snapshots +
      Master low-pass; driven by `GameEvents.EnemyStateChanged`,
      `LitZone.PlayerInLitZone` and the fragment/fuse/dawn events via
      `AudioSnapshotController` (transition seconds in GameConfig).
- [x] Real music: per-wing drones + ending cues from a licensed/CC0 source
      (D9 logged in ASSET_LICENSES) with the procedural AdaptiveMusic kept as
      fallback. *(P1)* — Kevin MacLeod CC-BY 3.0 bed + ending cues imported and
      wired in this PR; Pixabay primaries pending manual download (bot-protected
      pages need a browser session); per-wing drones remain procedural.

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
- [x] Rebuild scenes via the SceneBuilder partials after each art batch and
      keep the 60 existing swaps intact (D3). *(P2 — hero FBX swaps wired in
      SceneBuilderArt/Prefabs + ArtImporter, greybox fallbacks everywhere,
      ValidateBatch reports 14/14 hero FBX + per-model swap counts)*

## P3 — Horror & interaction systems

- [x] Noise bus: `NoiseEmitter` events (sprint/walk/crouch/door/drop/wrong
      buzzer) with per-action hearing radii feeding the Shadow's hearing. *(P3)*
- [x] Shadow `Investigate` state: go to the last heard noise, then resume
      patrol; tunes with the noise bus. *(P3)*
- [x] Crank minigame: hold + circular swipe emergency charge capped low and
      loud (radii above), per plan §4.6. *(P3)*
- [x] Menace director: gauge 0–100 that paces sightings and forces a
      cool-down exit at 100; guarantees calm after chases (plan §4.12). *(P3)*
- [x] Hiding v1: hide spots (lockers/desks/curtains) with hold-breath when the
      Shadow is close; Search checks 1–2 spots. *(P3)*
- [x] Banish rebalance: hold the focused beam 2 s with charge ≥ 25 → 20 s
      retreat (GameConfig fields, no literals). *(P3)*
- [x] Calm FX: heartbeat, vignette and beam-tremor driven by the calm stat. *(P3)*
- [x] 360° inspect view for story items (Inspectable close-up). *(P3)*
- [x] Study-recap PNG export (shareable/printable from the recap screen). *(P3)*
- [x] Haptics: light on correct answers, heavy on caught (Android). *(P3)*
- [x] Dev console: set flags/stats, teleport, skip to act, toggle AI (editor
      builds only). *(P3)*
- [x] Reverb zones + occlusion (premium-pass brief §8.3, deferred from the
      mixer PR): builder-generated `AudioReverbZone` per interior room
      (Library/PhysicsLab/Classroom/Garden/StaffRoom = Room, Corridor/Entrance
      = MediumRoom, MainHall = ConcertHall; distances from the room bounds)
      and `AudioOcclusionProbe` on the player camera — interval linecasts
      muffle blocked 3D sources (lazy low-pass 2400 Hz + −6 dB dip,
      GameConfig fields). *(P3)*

## P4 — Arabic content + credits

- [x] Fill Arabic question fields (`questionAr`/`optionsAr`/`explanationAr`)
      for all 60 questions; display path unchanged (Localization +
      ArabicSupport). *(P4)* - AI translation (Egyptian-flavored MSA, mother's
      voice), pending human/native review; factSource stays NEEDS-FACT-CHECK.
      Filled in place by Art/pipeline/fill_arabic_questions.py (append-only
      YAML edit, GUIDs/meta untouched); ValidateBatch now asserts the fields
      (SceneBuilderValidation "Question Arabic fields: 60/60").
- [x] Question bank expansion 60 -> 120: 60 new school-level questions
      (Q_<Cat>_011+), 12 per wing with 4 easy / 5 standard / 3 hard each
      (Library, Science wing, Courtyard, Assembly hall, Staff room - wing
      mapping per MASTER_PLAN section 4.5). *(P4)* - AI-authored EN+AR pairs
      via Art/pipeline/add_120_questions.py (append-only: the legacy 60 stay
      byte-identical, no .meta authored - Unity generates them on import, and
      QuestionDatabase relinks automatically through GenerateQuestionsSilent
      on the next editor run). Every factSource stays NEEDS-FACT-CHECK (rule
      5); the source=curriculum designation lives in the authoring table
      (QuestionData has no serialized `source` field). Verifier extended to
      120: Art/pipeline/verify_arabic_questions.py -> RESULT: PASS. Human
      fact-check still required before release.
- [ ] Credits screen wired to `Docs/CREDITS.md` content (in-game screen, EN/AR
      chrome). *(P4)*

## P5 — Performance, release prep and VO specs

- [ ] Performance doc refresh from CI build numbers + any device profiling
      that lands (Docs/PERFORMANCE.md); on-device profiling remains
      human-assisted. *(P5)*
- [x] App icons (Windows + Android, all densities): emblem pipeline
      `Art/Pipeline/ls_icon.py` (Blender EEVEE headless, STYLE_GUIDE palette)
      renders `Art/Previews/icon_{1024,512,192,96}.png`; PlayerSettings
      wiring steps are documented in the script header — the in-editor
      assignment itself remains a human step (ProjectSettings untouched).
      *(P5)*
- [x] Store text EN/AR + privacy statement + age-rating questionnaire notes
      (Docs/STORE_LISTING.md). *(P5)*
- [ ] Store screenshots (EN + AR devices, per-store size sheets) — need
      Unity runs. *(P5)*
- [x] VO recording specs (Docs/VO_RECORDING_SPECS.md): per-character voice
      profiles, dry 48 kHz/24-bit chain, dialogue −16 LUFS normalization,
      `VO_<character>_<nodeId>_<lang>.wav` naming, and the per-speaker
      line-count table grepped from the 44 SN_ assets (Nour 22, Mother 7,
      Shadow 9, Memory 6 + 4 ending epilogues). *(P5)*
- [ ] Optional signed AAB job for Google Play (requires owner signing
      config). *(P5)*
