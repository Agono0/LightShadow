# LIGHT SHADOW

> **Trapped in her dead mother's school on the night before demolition, a grieving student must recharge a knowledge-powered flashlight by answering real questions — because in LIGHT SHADOW, learning is the only light that follows a person into the dark.**

First-person educational horror for **Windows PC** and **Android 10+** — no combat, no gore.
Made with Unity 6 (Universal Render Pipeline). Single player, 20–40 minutes per playthrough.

Design document: [`LightShadow_Project_Document.md`](LightShadow_Project_Document.md) · AI-assistance log: [`AI_DEV_LOG.md`](AI_DEV_LOG.md)

## Status

The full gameplay loop is implemented and playable from the generated scenes:

- first-person walk / sprint / crouch, keyboard + mouse and a touch layer;
- the hand-crank flashlight with battery drain, battery pickups and flashlight repulsion;
- the Bright Idea Meter (correct +25, wrong/timeout −10, passive drain, beam range 4–12 m);
- the Shadow's four-state behaviour (patrol → stalk → chase → repelled), NavMesh, line of sight and lit-room exclusion — plus a synthesised growl/stinger voice that follows its state;
- three fuse boxes, six Knowledge Fragments, seven quiz boards, seven batteries and the powered gate;
- lit-room checkpoints, bloodless caught/respawn, dawn chase, ending and study-recap screen;
- a low-poly first-person hand with breathing, walk bob and crouch dip;
- **60 seeded questions** across Science, Math, History, Geography, Language and Health;
- English/Arabic interface (all UI, no localization packages) and a settings card
  (resolution, fullscreen, quality, v-sync, brightness, sensitivity, language, narrator);
- a procedural ambience drone and synthesised interaction sounds (no audio assets required).

The level is a deliberately readable **greybox**; art, textures, recorded audio and lighting bakes are the
remaining production pass described in the project document. Every seeded question is still marked
`NEEDS-FACT-CHECK` until a human verifies it against a real reference (doc §8.3).

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

- **Resolution / Fullscreen / Quality / VSync** — applied live, saved to PlayerPrefs;
- **Brightness** — URP post-exposure lift plus a small ambient-light lift, so the flashlight
  silhouette still reads even with post-processing off (mobile preset);
- **Mouse sensitivity** — multiplies the configured mouse turn rate;
- **Language** — English or Arabic. All menus, the HUD, the quiz overlay, prompts, messages,
  objectives, the ending and the study recap switch immediately, with right-to-left text and an
  OS Arabic font at display time. Question text stays English unless an Arabic translation is
  filled into the `questionAr`/`optionsAr`/`explanationAr` fields of a question asset;
- **Narrator voice** — on Windows the run, fragment memories and the ending are read aloud by
  the operating system voice (SAPI, off the main thread); the option turns it off. Arabic text
  is only spoken when an Arabic voice is installed, so the language is never mispronounced.

## Build

- **Windows** — **LightShadow ▸ 4. Build Windows Player**, or File ▸ Build Profiles ▸ Windows.
  Output: `Builds/Windows/LightShadow.exe` (already produced and smoke-tested).
- **Android** — install the Android module (SDK & NDK Tools, OpenJDK), then
  **LightShadow ▸ 6. Build Android APK**. The project is already set to IL2CPP / ARM64 /
  API 29 / landscape, ASTC. If the module is missing the menu item says so instead of failing.

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

## Known limitations / next pass

- Greybox geometry and unlit/dark rooms; the art and lighting-bake pass is still the Week 7 deliverable.
- Questions are AI-assisted drafts. A human must verify each `factSource` before release.
  Question text itself is English; the `questionAr`/`optionsAr`/`explanationAr` fields exist for
  the hand-translation pass.
- Android has not been run on a physical test device in this environment; the Windows build was
  smoke-tested. The touch UI is present and hides itself on desktop.
- No cutscenes, voice acting audio files, multiple save slots or localization packages, by design
  (doc §2.4 cut list). Interface localization (EN/AR) and the SAPI narrator are code-side only.
- Checkpoints are per-session; there is no disk save yet. The game is 20–40 minutes, so this is
  the documented “checkpoint autosave only” scope rather than multiple slots.
