# LIGHT SHADOW

> **Trapped in her dead mother's school on the night before demolition, a grieving student must recharge a knowledge-powered flashlight by answering real questions — because in LIGHT SHADOW, learning is the only light that follows a person into the dark.**

First-person educational horror for **Windows PC** and **Android 10+** — no combat, no gore.
Made with Unity 6 (Universal Render Pipeline). Single player, 20–40 minutes per playthrough.

Design document: [`LightShadow_Project_Document.md`](LightShadow_Project_Document.md) · AI-assistance log: [`AI_DEV_LOG.md`](AI_DEV_LOG.md)

## Engine version

Developed with **Unity 6000.4.0f1** (3D / URP, Force Text serialization, visible meta files).
The project document specifies “Unity 6 LTS 6000.0.x”; the installed editor is 6000.4.0f1 and is
the project standard — see the deviations note in `AI_DEV_LOG.md`. Everyone must use the same version.

## Open and run (first time)

1. Unity Hub → **Add** → select this folder → open it with Unity 6000.4.0f1.
2. In the menu bar: **LightShadow ▸ 1. Project Setup**.
3. When the tool asks, **restart Unity** (it sets Active Input Handling to *Both*).
4. **LightShadow ▸ 2. Generate Questions** — creates the starter question bank in
   `Assets/_Project/Data/Questions/` (drafts marked `NEEDS-FACT-CHECK` until human-verified).
5. **LightShadow ▸ 3. Build Scenes** — creates the four scenes and bakes the NavMesh.
6. Press **Play**. The game boots from `00_Bootstrap`.

## Build

- **Windows** — File ▸ Build Profiles ▸ Windows (default profile). The zipped build must run
  on a machine without Unity installed.
- **Android** — install the Android module (SDK & NDK Tools, OpenJDK), switch the Android
  profile, then set: Scripting Backend **IL2CPP**, Target Architecture **ARM64**,
  Minimum API Level **29**, **ASTC** compression, Default Orientation **Landscape Left + Right**
  (project document §6.8).

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

## Folder map (abridged — full tree in the project document §6.6)

```text
Assets/
├── _Project/           everything the team makes
│   ├── Scenes/         00_Bootstrap, 01_MainMenu, 02_School, 03_Ending
│   ├── Scripts/        Core · Player · Enemy · Gameplay · UI · Tools (editor-only)
│   ├── Prefabs/        Player · Enemy · Interactables · UI
│   ├── Data/Questions  QuestionData .asset files, one per question (Q_Sci_001 …)
│   └── Materials · Models · Textures · Animations · Audio · Lighting
└── ThirdParty/         imported free packs — NEVER edited in place
```

Naming conventions (`Mat_`, `SM_`, `SFX_`, `Q_Sci_001`, …) are defined in the project document §6.7.
