# LIGHT SHADOW — Changelog (Premium Pass)

## [0.6.0-premium-integration] — 2026-11-19 — Premium integration: audio, horror, Arabic, release prep

Everything below ships through one integration PR (`feat/premium-integration`,
supersedes closed PRs #5/#6; PRs #3/#4 already merged to `main`). Features are
user-visible; review flags are stated honestly.

### Added
- **Bundled fonts (Arabic renders offline now)**: Noto Naskh Arabic (R/B), Cairo (R/B),
  Reem Kufi (R/B), Inter (R/SemiBold), Special Elite — OFL/Apache 2.0, license texts shipped in
  `Assets/_Project/Fonts/Licenses/`; wired through builder-created references (no Resources
  folder); Arabic no longer depends on OS-installed fonts, with OS fallbacks retained.
- **Real AudioMixer** (D17, supersedes D15's interim code mixer):
  `AudioMixerTool` generates `M_Main.mixer` (Master > Music/Ambience/SFX/Voice/Shadow/UI with
  snapshot-owned duck children + Master low-pass) via Unity's editor serialization pipeline;
  `ValidateMixer()` batch-asserts it (`RESULT: PASS`). Settings sliders keep working unchanged —
  they now write the seven exposed user params through `VolumeCategories`.
- **Audio snapshot choreography** (premium-pass brief §8.3): **six snapshots** — Explore, Stalk,
  Chase, Hide, MemoryFlash, Dawn — driven by the Shadow FSM (new `GameEvents.EnemyStateChanged`),
  lit-zone muffle (900 Hz while claimed), fragment/fuse memory beats (world ducks around the
  narrator for 6 s) and the dawn swell; all transition seconds live in `GameConfig`.
- **Music cues**: Kevin MacLeod CC-BY 3.0 bed ("Long Note One") layered under the procedural
  AdaptiveMusic pads during Explore/Stalk; per-ending one-shot cues (Frozen Star → FullRecovery,
  Anguish → DawnlessGrey) wired on the ending screen; credits + license rows updated.
- **14 Blender hero props in the scenes**: SM_Story_{MemoryBox, Card, Bookmark, ExamPaper,
  Prism, Photograph, Programme, Mug}, SM_Player_Flashlight, SM_Wraith_HD (the Shadow's new HD
  silhouette), SM_Arch_{DoorGrille, Chalkboard, GateCrank, IronGate} — modelled in
  `Art/Sources/HeroProps.blend`, imported via a `HeroModels` table with story materials
  (Mat_Story_RedInk/Glass/LED), and wired through builder swap tables (8 chalkboards incl.
  QuizBoard, memory box + card, 6 story items, door grilles, gate crank, iron-gate segments,
  the player's flashlight); validation asserts 14/14 FBX + 12/12 materials + swap counts.
- **Horror & interaction systems** (premium-pass brief P3, all eleven):
  - noise bus — `NoiseEmitter` events (sprint/walk/crouch/door/drop/wrong buzzer) with
    per-action hearing radii feeding the Shadow's hearing;
  - Shadow `Investigate` state — goes to the last heard noise, then resumes patrol;
  - crank minigame — hold + circular swipe emergency charge, capped low and loud;
  - MenaceDirector — 0–100 pacing gauge that spaces sightings, forces a cool-down exit at 100
    and guarantees calm after chases;
  - hiding v1 — hide spots (lockers/desks/curtains), hold-breath meter when the Shadow is
    close, Shadow `Search` checks 1–2 spots, hiding state cleared on catch/checkpoint;
  - banish rebalance — hold the focused beam 2 s at charge ≥ 25 → 20 s retreat
    (GameConfig fields, no literals);
  - calm FX — vignette, heartbeat and beam tremor driven by the calm stat;
  - 360° inspect view for story items (inspectable close-up);
  - study-recap PNG export (shareable/printable from the recap screen);
  - haptics — light on correct answers, heavy on caught (Android);
  - dev console — set flags/stats, teleport, skip to act, toggle the Shadow's behaviour
    (editor builds only).
- **Arabic questions 60/60**: `questionAr`/`optionsAr`/`explanationAr` filled for all 60
  questions (AI translation, pending human/native review; `factSource` stays
  `NEEDS-FACT-CHECK`); ValidateBatch asserts the fields; bank expansion to 120 is in progress.
- **Release assets**: flashlight-emblem app icon pipeline (Blender EEVEE headless →
  1024/512/192/96 PNG; PlayerSettings assignment remains a human step), EN/AR store listing
  with IARC + privacy notes (`Docs/STORE_LISTING.md`), VO recording specs
  (`Docs/VO_RECORDING_SPECS.md`, 96 files EN+AR).

### Changed
- The Shadow's audio sources (breathing, one-shots, growl/stinger, pooled whispers) route to a
  dedicated Shadow bus; AmbientDrone is recategorized to Ambience; `AudioManager.PlaySfxAt` no
  longer uses `AudioSource.PlayClipAtPoint` (its temp sources bypassed categories and the mixer).
- `EnemyBehaviour.EnterState` raises `GameEvents.EnemyStateChanged(int)` — one raise covers every
  FSM transition including the post-catch reset; the FSM gains Investigate/Search/ForcedCooldown
  states alongside Patrol/Stalk/Chase/Repelled.
- Nine review fixes across the horror systems (hiding-state reset on respawn, F tap-vs-crank
  split on desktop, touch hold respects the input lock, one shared sight Linecast per frame
  between the FSM and MenaceDirector, calm-FX volume-profile teardown, crank ratchet clock reset,
  recap Debug.Log removal, comment alignment, defensive `VolumeCategories.SetMixer` routing).

### Fixed
- `(integration)` PadHeld defined via the Input System runtime; unshadowed MakeButton label.

## [0.5.0-premium] — 2026-10-31 — Round 3: bug fixes + horror presentation

### Fixed
- **Reversed controls / stuck camera after being caught**: the cinematic shot never
  restored the main camera's local pose (and a failed brain-enable left it pinned).
  Snapshot/finally-restore + brain re-fetch + `PlayerController.RespawnReset()` at
  the checkpoint + per-zone spawn yaw (were identity: woke facing world +Z).
- **Mouse escaping the window**: focus-regain re-lock + guarded re-assert (desktop).
- **FPS counter never appeared** — and the root cause was bigger: `BuildCinematicRigs`
  opened the STALE school scene from disk over the fresh unsaved build, silently
  discarding every school-scene builder change since round 1. Save-order fixed;
  FpsCounter also no longer disables itself (was dead-unsubscribing after first off).
- Quiz timer bar variable, `Screen.resolutions` null in headless batch, two Unity-6
  API drifts (cookie importer, duplicate Image AddComponent).

### Added
- **Real audio**: CC0 packs (Kenney Interface/Impact/RPG Audio, qubodup Ghost Moans)
  + CC-BY ambience (tcarisland, attributed) — library-first with synthesised
  fallbacks; new footstep system; Shadow voice per FSM state; `AudioImportTool`.
- **Procedural wraith mesh** (SM_Wraith) replacing the blocky Shadow silhouette.
- **Darkness pass**: near-black ambient, dimmed lamps/moon, flashlight cookie +
  additive beam cone, brighter ember eyes.
- **Live 3D menu diorama**: corridor + flickering lamp + drifting Shadow + camera
  breathing + title flicker (first menu animation).
- **Loading screen redesign**: gradient night, sweeping searchlight, styled bar, tip.
- **Quiz popup redesign**: real timer bar (hot near timeout), themed chrome.



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
