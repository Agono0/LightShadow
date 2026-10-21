# AI-Assistance Development Log — LIGHT SHADOW

Per Section 8.3 of the project document, every AI-assisted script or document gets an entry here,
so the team's transparency is provable. This log is submitted with the final deliverables.
The game itself contains **no AI features** — this log is about the development method only.

## Entry template (copy for each entry)

```text
### 【Date】 — 【File or task】 — 【Member Name】
- Tool used: (ChatGPT / Claude / Copilot)
- What AI provided: (e.g. first draft of the battery drain loop)
- What I changed myself: (e.g. removed jumping, fixed crouch height, added comments)
- Teach-back check: (one sentence: what I explained to a teammate, and to whom)
- Test result: (e.g. played 10 minutes, no Console errors, battery drains as expected)
```

---

## Entries

### 2026-10-19 — Week 1 scaffold: Section 6.6 folder tree, README.md, this log — Abdelrhman Mohamed
- Tool used: opencode (AI coding assistant)
- What AI provided: first-draft README and this log's structure; the commands that build the
  Section 6.6 folder tree; Git LFS cleanup commands for the initial commit.
- What I changed myself: checked every folder and file name against Section 6.6; kept the
  official Unity `.gitignore`/`.gitattributes` templates instead of the document's copies
  (they are a superset and already LFS-ready); decided the question bank starts with 24 drafts
  marked `NEEDS-FACT-CHECK`.
- Teach-back check: pending — this entry is docs/scaffold only; the teach-back (doc §8.3 rule 1)
  is completed with a teammate before the first gameplay script is merged.
- Test result: `git status` clean after commit; no `Library/`, `Temp/`, or `Logs/` content tracked
  (checked with `git ls-files`).

### 2026-10-21 — SceneBuilder, prefabs, four scenes, UI and NavMesh — Abdelrhman Mohamed
- Tool used: DeepSeek Harness (AI coding agent)
- What AI provided: `Assets/_Project/Scripts/Tools/Editor/SceneBuilder*.cs`, the prefab and UI
  factories, the school greybox layout, the URP materials, the NavMesh bake, the bootstrap /
  main-menu / ending flow, and the validation batch method.
- What I changed myself: fixed the TextMesh facing (all world labels were mirrored), moved the
  NavMesh bootstrap from `Awake` to `Start` so the baked data is registered first, disabled the
  enemy's `NavMeshAgent` until `EnemyBehaviour.Start`, set Active Input Handling to Both, reviewed
  every generated label/room position, and checked the build settings order (Bootstrap → Menu →
  School → Ending).
- Teach-back check: pending — the scene builder is editor tooling; before the next merge I will
  walk a teammate through `BuildAll`, `ValidateScenes`, and how to rebuild after a layout change.
- Test result: Unity 6000.4.0f1 batch compile clean; `LightShadow ▸ 5. Validate Content` returns
  `RESULT: PASS` (3 fuse boxes, 6 fragments, 7 batteries, 7 boards, 5 lit zones, 1 gate, 60 valid
  questions, 976 NavMesh vertices); Windows player launched and the menu → school → flashlight
  loop was smoke-tested by screenshot.

### 2026-10-21 — Question bank growth and the runtime/audio pass — Abdelrhman Mohamed
- Tool used: DeepSeek Harness (AI coding agent)
- What AI provided: the additional question seeds (60 total), `ProceduralSfx` and `AmbientDrone`,
  `RunResults`, `BootstrapLoader`, `MainMenuController`, `EndingController`, and the
  `GameManager.Escape` → `03_Ending` handoff.
- What I changed myself: verified every seeded fact against general knowledge, kept every
  `factSource` at `NEEDS-FACT-CHECK` instead of claiming a human review, lowered the shadow
  emission so it reads as a silhouette, raised the fuse-room light intensities, and kept the
  synthesised audio quiet (ambient loop at 0.14) so it does not mask the enemy cues.
- Teach-back check: pending — the writer/QA owner must run the human fact-check (doc §8.3 rule 3)
  before the bank is called verified.
- Test result: 60 `Q_*.asset` files created, database links all 60 across 6/6 categories; batch
  validation PASS; Windows build succeeded (`Builds/Windows/LightShadow.exe`); procedural audio
  starts without exceptions in the player log.

### 2026-10-21 — Finishing pass: settings card, EN/AR interface, narrator, shadow voice, hand — Abdelrhman Mohamed
- Tool used: DeepSeek Harness + opencode (GLM 5.3 Flash) co-authoring agent
- What AI provided: finished wiring of the loose systems (`GameSettings`, `Localization`,
  `SettingsPanelController`, `StoryTeller`, `ShadowAudio`, `BrightnessController`,
  `FirstPersonHands`), the display-time localization of the HUD and quiz overlay and study recap,
  the fixed settings-row colours (previously the values could render dark on the dark card), and
  the numpad answer keys.
- What I changed myself: reviewed every diff, confirmed the Arabic shaping is applied only once
  (idempotent `ArabicSupport.Fix`), kept questions English-only per the fallback design, removed
  the leftover debug log line, decided the settings card re-asserts colours on every refresh
  instead of trusting prefab state, and decided Android stays out of this round (Windows only).
- Teach-back check: pending — will walk a teammate through how the language swap re-renders
  open UI via `Localization.LanguageChanged` before the next merge.
- Test result: batch compile clean; `LightShadow ▸ 5. Validate Content` returns `RESULT: PASS`;
  Windows rebuild succeeded (Builds/Windows — 21 Oct, 13:19).

---

## Deviations from the project document (tracked here on purpose)

1. **Editor version** — doc §6.3 specifies Unity 6 LTS (6000.0.x); the installed editor on this
   machine is **6000.4.0f1**, which is now the project version. All members must match it
   (doc §6.3 golden rule: everyone on the same version).
2. **`.gitignore` / `.gitattributes`** — the doc §7.5 files were replaced by the official Unity
   templates: same exclusions plus LFS patterns for images, models, audio and binaries.
   Verified working (URP.png is committed as an LFS pointer).
3. **Question bank start size** — doc §9 Week 4 asks for 30 questions and §9 Week 8 for 60+.
   This implementation seeds **60** questions (6 categories × 10), each marked
   `factSource: NEEDS-FACT-CHECK`; the human fact-check of doc §8.3 still has to run before the
   alpha freeze. The `QuestionGenerator` never overwrites an existing (reviewed) asset.
4. **Editor tooling folder** — added `Assets/_Project/Scripts/Tools/Editor/` for the menu tools
   (Project Setup, Generate Questions, Build Scenes, Build Windows, Validate Content, Build
   Android); the §6.6 tree has no tooling slot.
5. **Art and audio** — art is still the greybox described in doc §9 Week 7. Instead of shipping
   silence, the build now synthesises a quiet ambience loop and the interaction tones at runtime
   (`AmbientDrone.cs`, `ProceduralSfx.cs`); no audio files or packages were added.
6. **Checkpoints are per-session** — the doc's "checkpoint autosave only" is implemented as the
   in-run checkpoint (caught → last lit zone). Writing checkpoints to disk was not added because
   a 20–40 minute run does not need it and multiple save slots are on the §2.4 cut list.
7. **Validation tooling** — `LightShadow ▸ 5. Validate Content` is an extra non-shipping check
   that asserts scene/object counts, wiring, the 60-question bank and the baked NavMesh. It is
   the evidence used for the test results in the entries above.
