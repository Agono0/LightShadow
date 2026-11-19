# LIGHT SHADOW

> **Her mother's old school, the night before it falls - a determined student recharges a knowledge-powered flashlight by answering real questions, because in LIGHT SHADOW, learning is the only light that follows a person into the dark.**

First-person educational horror for **Windows PC** and **Android 10+** — no combat, no gore.
Made with Unity 6 (Universal Render Pipeline). Single player, 20–40 minutes per playthrough.

Design document: [`LightShadow_Project_Document.md`](LightShadow_Project_Document.md) · AI-assistance log: [`AI_DEV_LOG.md`](AI_DEV_LOG.md)

**Premium-pass documentation** lives in [`Docs/`](Docs/): [`AUDIT.md`](Docs/AUDIT.md),
[`ROADMAP.md`](Docs/ROADMAP.md), [`DECISIONS.md`](Docs/DECISIONS.md), [`PROGRESS.md`](Docs/PROGRESS.md),
[`CHANGELOG.md`](Docs/CHANGELOG.md), [`PERFORMANCE.md`](Docs/PERFORMANCE.md),
[`ASSET_LICENSES.md`](Docs/ASSET_LICENSES.md), and the expanded story under [`Docs/Story/`](Docs/Story/).

## Status

The full gameplay loop is implemented and playable from the generated scenes:

- ✅ **Finishing pass 3 (23 Oct 2026)** — content validation `RESULT: PASS`, a play-mode smoke test
  (boot → menu → school, zero console errors), a fresh Windows build
  (`Builds/Windows/LightShadow.exe`, 125.3 MB) and the **first Android APK**
  (`Builds/Android/LightShadow.apk`, 45.7 MB, IL2CPP / ARM64 / API 29). See `AI_DEV_LOG.md`.


- first-person walk / sprint / crouch, keyboard + mouse and a touch layer;
- the hand-crank flashlight with battery drain, battery pickups and flashlight repulsion —
  plus an emergency **crank minigame** (hold + circular swipe) when the battery dies;
- the Bright Idea Meter (correct +25, wrong/timeout −10, passive drain, beam range 4–12 m);
- the Shadow's state machine (patrol → stalk → investigate → chase → search → repelled),
  NavMesh, line of sight, lit-room exclusion, **hearing** (a noise bus with per-action
  radii for sprinting, doors and wrong answers) and a **menace director** that paces
  sightings and guarantees calm after chases — plus a synthesised growl/stinger voice
  that follows its state;
- **hiding v1** (hide spots with a hold-breath meter while the Shadow searches) and the
  **banish hold** (keep the focused beam on it for 2 s to force a retreat);
- three fuse boxes, six Knowledge Fragments, seven quiz boards, seven batteries and the powered gate;
- lit-room checkpoints, bloodless caught/respawn, a scripted catch sequence (freeze, lunge, camera
  shake, whisper), dawn chase, ending and study-recap screen (now exportable as a printable PNG);
- a low-poly first-person hand with breathing, walk bob and crouch dip, plus small ambient motion
  on the pickups, lamps and the Shadow itself;
- a three-dimensional dawn ending: the camera looks through the opened gate at a rising sun while the
  recap fades in over the set;
- story plaques in every room (the night's history is read in the world, not in a cutscene), and a
  **360° inspect view** for the story items you pick up;
- **60 seeded questions** across Science, Math, History, Geography, Language and Health —
  with Arabic question text filled for all 60 (AI translation, pending human review);
- English/Arabic interface (all UI, no localization packages, bundled Arabic fonts) and a
  settings card (resolution, fullscreen, quality, v-sync, brightness, sensitivity, language, narrator);
- a real AudioMixer with six tension snapshots (Explore/Stalk/Chase/Hide/MemoryFlash/Dawn),
  a licensed music bed with per-ending cues, real SFX/ambience packs and haptic feedback on
  Android (correct answers tick, being caught thuds).

The level now ships with a **premium art pass**: 60 real mesh swaps (Kenney CC0 furniture/foliage/
fence/silhouette), Poly Haven PBR surfaces, gripped first-person hands, dust motes,
light/reflection probes and scene fog — plus **14 Blender hero props** (the memory box and card,
the exam paper, prism and photograph, the chalkboards, the gate crank, the iron-gate segments and
the player's flashlight, with a higher-detail Wraith silhouette on the Shadow), a **branching story
system** (44-node graph, timed choices, stats, 4 endings), 3-slot saves, quality tiers
(Low/Medium/High/Ultra with auto-detect), 120 FPS targeting, gamepad support and accessibility
options. Full records: [`Docs/`](Docs/).
Every seeded question is still marked `NEEDS-FACT-CHECK` and the Arabic translations await a
native-speaker review until a human signs them off (doc §8.3); the GI lightmap bake is
tooling-complete but pending an editor-UI run.

## Engine version

Developed with **Unity 6000.4.0f1** (3D / URP, Force Text serialization, visible meta files).
The project document specifies “Unity 6 LTS 6000.0.x”; the installed editor is 6000.4.0f1 and is
the project standard — see the deviations note in `AI_DEV_LOG.md`. Everyone must use the same version.

## Open and run (first time)

1. Unity Hub → **Add** → select this folder → open it with Unity 6000.4.0f1.
2. In the menu bar: **LightShadow ▸ 1. Project Setup**.
3. If the tool asks, **restart Unity** (it sets Active Input Handling to *Both*).
4. **LightShadow ▸ 2. Generate Questions** — creates/links the question bank in
   `Assets/_Project/Data/Questions/` (existing reviewed questions are never overwritten).
5. **LightShadow ▸ 3. Build Scenes** — creates the four scenes, the prefabs, the HUD and the NavMesh.
6. Press **Play**. The game boots from `00_Bootstrap`.

The generated content is already committed, so a fresh clone can usually skip straight to **Play**.
Re-run **3. Build Scenes** after changing `SceneBuilder*.cs`, the level layout or the tuning asset.

## Options (Settings card)

The Settings card opens from the main menu and from the pause menu (Esc ▸ Settings):

- **Resolution / Fullscreen / Quality / VSync** - applied live, saved to PlayerPrefs;
- **Shadows / Anti-aliasing / Render scale** - applied to the active URP asset at runtime, so a
  weaker machine can drop shadow distance, MSAA and resolution without leaving the game;
- **Brightness** - URP post-exposure lift plus a small ambient-light lift, so the flashlight
  silhouette still reads even with post-processing off (mobile preset);
- **Mouse sensitivity** — multiplies the configured mouse turn rate;
- **Language** — English or Arabic. All menus, the HUD, the quiz overlay, prompts, messages,
  objectives, the ending and the study recap switch immediately, with right-to-left text using
  the bundled Arabic fonts (Noto Naskh Arabic / Cairo / Reem Kufi — no OS font install needed).
  All 60 questions have Arabic question/option/explanation text filled in
  (`questionAr`/`optionsAr`/`explanationAr`), AI-translated and pending a human native review;
- **Narrator voice** — on Windows the run, fragment memories and the ending are read aloud by
  the operating system voice (SAPI, off the main thread); the option turns it off. Arabic text
  is only spoken when an Arabic voice is installed, so the language is never mispronounced.

## Build

- **Windows** — **LightShadow ▸ 4. Build Windows Player**, or File ▸ Build Profiles ▸ Windows.
  Output: `Builds/Windows/LightShadow.exe`. CI build artifacts: the Windows player folder
  uploads at ~152 MB (fonts + music included; see `Docs/PERFORMANCE.md`).
- **Android** — install the Android module (SDK & NDK Tools, OpenJDK), then
  **LightShadow ▸ 6. Build Android APK**. The project is already set to IL2CPP / ARM64 /
  API 29 / landscape, ASTC. If the module is missing the menu item says so instead of failing.
  Output: `Builds/Android/LightShadow.apk`. CI per-ABI artifacts (zipped): arm64-v8a
  ~88 MB, armeabi-v7a ~86 MB after the premium audio/font pass — the integration PR's own
  build may shift these; measured numbers live in `Docs/PERFORMANCE.md`.

Command-line equivalents (used by the automated verification):

```powershell
Unity.exe -batchmode -projectPath . -executeMethod LightShadow.Tools.SceneBuilder.BuildAllBatch
Unity.exe -batchmode -projectPath . -executeMethod LightShadow.Tools.SceneBuilder.ValidateBatch
Unity.exe -batchmode -projectPath . -executeMethod LightShadow.Tools.SceneBuilder.BuildWindowsBatch
Unity.exe -batchmode -projectPath . -executeMethod LightShadow.Tools.SceneBuilder.BuildAndroidBatch
```

**LightShadow ▸ 5. Validate Content** checks the generated scenes, object counts, wiring, the
60-question bank and the baked NavMesh, and prints `RESULT: PASS` or a list of concrete problems.

## Controls

| Action | Keyboard + mouse | Touch |
| --- | --- | --- |
| Move | W A S D | left joystick |
| Look | Mouse | drag on the right half of the screen |
| Sprint | hold Left Shift | hold the sprint button |
| Crouch | hold Left Ctrl | toggle the crouch button |
| Flashlight | F | flashlight button |
| Interact | E | interact button |
| Answer a question | 1 2 3 4 | answer buttons |
| Objectives | Tab | objectives button |
| Pause | Esc | pause button |

The main menu also starts on **Enter / Space**. The quiz overlay does not pause the world: answering in
the dark is the game's central risk.

## How the loop works

1. Explore with the flashlight. The **battery** is fuel and drains in about five minutes; pickups restore 50%.
2. Read a quiz board (or collect a Knowledge Fragment) and answer within 20 seconds.
3. A correct answer adds **+25** to the Bright Idea Meter and flares the beam; a wrong answer or timeout
   removes **10** and shows the correction for three seconds.
4. The meter maps directly to beam range (4–12 m) and intensity (1.5–3.5). Above 30 the beam can repel
   the Shadow; inside a lit zone the Shadow cannot enter and the room becomes a checkpoint.
5. Restore all **three fuse boxes**, collect all **six fragments**, power the gate at the gatehouse console,
   then survive the dawn corridor and walk through the gate.
6. The ending scene shows the **study recap**: every question, your answer, the correct answer and the
   explanation — a printable study sheet built from the run.

## Folder map (abridged — full tree in the project document §6.6)

```text
Assets/
├── _Project/               everything the team makes
│   ├── Scenes/             00_Bootstrap, 01_MainMenu, 02_School, 03_Ending
│   ├── Scripts/            Core · Player · Enemy · Gameplay · UI · Tools (editor-only)
│   ├── Prefabs/            Player · Enemy · Interactables
│   ├── Data/               GameConfig, QuestionDatabase, 60 Q_*.asset, NavMesh_School
│   ├── Materials/          Mat_* URP materials generated by the scene builder
│   └── Audio · Models · Textures · Animations · Lighting
├── Settings/               URP PC / Mobile pipeline assets and volume profile
└── ThirdParty/             imported free packs — NEVER edited in place
```

Naming conventions (`Mat_`, `SM_`, `SFX_`, `Q_Sci_001`, …) are defined in the project document §6.7.

## Tuning

All gameplay numbers live in one asset: `Assets/_Project/Data/GameConfig.asset`.
Drain rates, meter rewards, enemy speeds, catch distance and question timer can be changed there
without touching code. Re-run **3. Build Scenes** only when the layout changes; existing scenes read
the asset at runtime.

## Status & limitations

- **Premium-pass art is integrated:** 60 Kenney mesh swaps plus Poly Haven PBR
  surface sets are live in the shipped scenes, and the **14 Blender hero props**
  are wired (story items, chalkboards, gate crank, iron-gate segments, the
  player flashlight and the Wraith HD silhouette). The modular key-architecture
  pass and the remaining master-plan props (optics bench, circuit puzzle, fuse
  boxes, card catalogue) are deferred (see [`Docs/TASKS.md`](Docs/TASKS.md)).
- **The horror-systems layer is landed** — noise/hearing with an Investigate
  state, crank minigame, menace director, hiding v1, banish hold, calm FX,
  360° inspect view, recap PNG export, Android haptics and an editor-builds dev
  console — carried by the single integration PR from
  `feat/premium-integration` (supersedes the closed PRs #5/#6).
- **The branching story shipped:** a 44-node graph with 12 authored choice
  moments, timed choices, 4 endings and 3-slot disk saves (details in
  [`Docs/PROJECT_STATE.md`](Docs/PROJECT_STATE.md)).
- **Audio and typography are premium now:** a real AudioMixer with six tension
  snapshots, licensed music cues, SFX/ambience packs and bundled OFL fonts, so
  Arabic renders offline.
- **Questions remain `NEEDS-FACT-CHECK`** pending human review; the Arabic
  question fields are filled 60/60 by AI translation and await a native-speaker
  review — nothing here claims human sign-off (AGENTS.md rule 5).
- **Builds are automated in CI** — Android arm64/armv7 APKs and the Windows
  player build on every PR, with artifacts/releases; measured sizes and build
  times are in [`Docs/PERFORMANCE.md`](Docs/PERFORMANCE.md); on-device
  performance profiling is still pending.
- Full detail lives in [`Docs/`](Docs/): [`PROJECT_STATE.md`](Docs/PROJECT_STATE.md),
  [`TASKS.md`](Docs/TASKS.md), [`PERFORMANCE.md`](Docs/PERFORMANCE.md).
