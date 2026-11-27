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
  Windows rebuild succeeded (Builds/Windows - 21 Oct, 13:19).

### 2026-10-21 - Finishing pass 2: graphics, motion, catch, story plaques and the dawn ending - Abdelrhman Mohamed
- Tool used: opencode (DeepSeek V4.1 Flash Free) coding agent
- What AI provided: the graphics settings row set (shadow distance, MSAA, render scale) applied to
  the URP asset, the URP/quality upgrade (4096 shadow maps, 4x MSAA, 8 additional lights), a global
  post-processing stack with `Post_Night`/`Post_Dawn` profiles, the `ShadowMotion`, `PickupMotion`,
  `CameraShake`, `DawnRiser` and `LightFlicker` components, the enemy catch/lunge/reset sequence,
  in-world story plaques for every room, the reworked narrator voice selection with SAPI pauses and
  a PowerShell `System.Speech` fallback, and the rebuilt 3D dawn ending driven by `EndingController`.
- What I changed myself: reviewed each builder diff before rebuilding, kept the dawn lamps exclusive
  to `DawnRiser` (no `LightFlicker` on the same lights), removed the opaque ending background so the
  3D set is visible behind a translucent scrim, and kept the world plaques as TextMesh instead of
  adding a cutscene system.
- Teach-back check: pending - walk a teammate through how `BuildPostProfile` rebuilds profiles
  idempotently, and how the catch coroutine hands control back via `ResetToPatrol`.
- Test result: batch compile clean; **3. Build Scenes** rebuilt all four scenes and the prefabs;
  **5. Validate Content** returns `RESULT: PASS` (3 fuse boxes, 6 fragments, 7 batteries, 7 boards,
  5 lit zones, 1 gate, 60 questions, 976 NavMesh vertices, 0 missing scripts); Windows build
  succeeded (`Builds/Windows/LightShadow.exe`, 125.3 MB, 21 Oct 15:58).

### 2026-10-23 — Finishing pass 3: console validation, play-mode smoke test and the first Android APK — Abdelrhman Mohamed
- Tool used: opencode (GLM 5.3 Flash) driving the Unity editor over MCP
- What AI provided: an editor-side validation replay (same `ValidateScenes` walk as the menu item,
  but reported to the console instead of a modal dialog), a play-mode smoke test (boot → main menu →
  `Play()` → school), and the Windows + Android build runs. No gameplay code changed.
- What I changed myself: decided a conventional lightmap bake is still impossible - the fuse-card
  room lights start inactive and are switched/intensity-animated at runtime and URP treats
  non-directional mixed lights as realtime, so the bake is deferred to the art pass; reverted the
  two editor-serialization artifacts this session produced (a `_Color` sync on `Mat_Eye` and boot
  scene text/image normalisations) instead of committing them; added no scripts and no assets.
- Teach-back check: pending — walk a teammate through what the validation pass counts and where
  each number comes from before the alpha freeze.
- Test result: `ValidateScenes` returns `RESULT: PASS` (3 fuse boxes, 6 fragments, 7 batteries,
  7 boards, 5 lit zones, 1 gate, 60/60 questions linked, NavMesh 976 vertices, player spawn on the
  NavMesh, 0 missing scripts, 127 localization keys). Play-mode smoke test: bootstrap menu shows
  the title, subtitle and four buttons; `Play()` loads `02_School`; player at the lit entrance
  `(0.0, 0.0, -29.5)` with HUD active; corridorS render as intended; **zero console errors** across
  the run. Windows build succeeded (125.3 MB, `Builds/Windows/LightShadow.exe`, scene data rebuilt;
  the `.exe` launcher itself is reused byte-identical by Unity's incremental pipeline). Android
  build succeeded for the first time in this environment (`Builds/Android/LightShadow.apk`,
  45.7 MB, IL2CPP / ARM64 / API 29).

### 2026-10-30 — Premium pass P0: audit, docs, build-profile repair — Abdelrhman Mohamed (Lead TD orchestration)
- Tool used: ZCode orchestration (GLM-5.3 Flash session) + three specialized sub-agents; every
  entry below is part of the same orchestrated pass on branch `feature/premium-pass`.
- What AI provided: full-project audit (`Docs/AUDIT.md`), roadmap/decisions/progress/changelog/
  performance/license docs, and the diagnosis of critical C1 — a missing
  `Assets/Settings/Build Profiles/Windows.asset` that aborted every batch Unity run — plus
  `Tools/Editor/BuildProfileTool.cs` (idempotent profile recreation + active-profile reselect).
- What I changed myself: verified the audit findings against the filesystem and batch logs,
  chose repair-over-rewrite (never hand-writing asset YAML), backed up and reset the stale
  machine-local `Library/EditorUserBuildSettings.asset` pointer, re-verified the baseline
  `ValidateBatch` returns `RESULT: PASS`.
- Teach-back check: pending — will walk a teammate through why the profile pointer lives in
  machine-local Library state and how `BuildProfileTool.EnsureBatch` recovers a boot failure.
- Test result: baseline `ValidateBatch` RESULT: PASS (exit 0) restored; build profiles committed.

### 2026-10-30 — Premium pass P1: art sourcing, import pipeline, art-swap pass, lighting — Abdelrhman Mohamed
- Tool used: Claude agent (Sub-Agent 1, ART & TECH ART) under Lead TD orchestration
- What AI provided: the CC0 asset sourcing and verification (4 Kenney packs + 8 Poly Haven PBR
  sets, ~88 MB, every URL/license logged in Docs/ASSET_LICENSES.md), the headless import
  pipeline (`ArtImporter.cs`: texture/model importer settings, URP/Lit material builder with
  roughness-to-smoothness conversion, Shadow/Hands/DustMotes prefab assembly, TheShadow and
  Player prefab rewiring, Post_Night/Post_Dawn tuning via the VolumeProfile API), the art-swap
  layer over the generated scenes (`SceneBuilderArt.cs`: name-keyed mesh/material swaps,
  idempotent, validator-safe), the lighting pass (`SceneBuilderArtLighting.cs`: static flags,
  light probes per wing, 3 baked reflection probes, async bake batch), and the hand-written
  ShadowSmoke shader.
- What I changed myself (Lead): reviewed the swap table against real scene names; fixed the
  Unity 6 API drift the agent could not compile-check (ModelImporter material enums, removed
  `generateColliders`/`generateLightmapUVs`/`bounceIntensity`, `LightProbeUsage`,
  `FilmGrainLookup.Medium2`, `ParticleSystemShapeType.Sphere`, `ReflectionProbe` enums via
  `UnityEngine.Rendering`, `LightingSettings` no longer a ScriptableObject, `boxSize`→`size`);
  replaced the nonexistent URP 17.4 Fog volume override with per-scene `RenderSettings` fog
  (night/dawn palettes); chose the final batch order.
- Teach-back check: pending — walk a teammate through `ApplyArtPass` keying strategy and the
  bake command's graphics requirement before merge.
- Test result: Lead-verified via batch — BuildAllBatch/ImportAllBatch/ApplyArtPassBatch/
  ApplyLightingBatch all exit 0; ValidateBatch RESULT: PASS; 60 `ArtModel_` swaps live in
  02_School, dust motes + probes present.

### 2026-10-30 — Premium pass P2: event bus, saves, quality tiers, gamepad, pooling, cinematic shots — Abdelrhman Mohamed
- Tool used: Claude agent (Sub-Agent 2, ENGINEERING) under Lead TD orchestration
- What AI provided: first drafts of `GameEvents`, `SaveSystem`, `PerformanceManager`,
  `ObjectPool`+`PooledFx`, `GamepadInput`, `CinematicDirector`, the `SceneBuilderSystems`
  editor partial, and every additive hook listed in `Docs/AGENT_REPORT_ENG.md` §2.
- What I changed myself (Lead): fixed `Gamepad.leftStickButton` (1.19 member name) and the
  static-class `UnityEngine.Object.FindFirstObjectByType` call; confirmed the frozen
  GameEvents/SaveSystem contracts against the Narrative agent's usage; verified all runtime
  Cinemachine use is reflection-gated so a package problem cannot hard-fail the game.
- Teach-back check: pending — how the tier system swaps the Mobile/PC URP asset and why SSAO
  rides the asset swap instead of a runtime toggle.
- Test result: Lead-verified via batch — compile clean, ValidateBatch RESULT: PASS; runtime
  tier/boot-summary behaviour to be play-verified on device (documented in PERFORMANCE.md).

### 2026-10-30 — Premium pass P3: branching story, dialogue UI, endings, save slots, accessibility, audio — Abdelrhman Mohamed
- Tool used: Claude agent (Sub-Agent 3, NARRATIVE/UI/UX/AUDIO) under Lead TD orchestration
- What AI provided: `Docs/Story/STORY_BIBLE.md` + `SCRIPT.md` (44-node branching graph, 12
  authored moments, boldness/trust/calm stats, timed choices, 4 endings per D13);
  `Scripts/Story/*` (StoryNode/StoryConfig/StoryEvents/StoryStrings/AccessibilityStore/
  StoryDirector/DialogueUIController); `Scripts/Audio/VolumeCategories` + `AdaptiveMusic`
  (D15 code-level mixer, layered procedural score); `Scripts/UI/SaveSlotUI` + `LoadingScreen`;
  additive settings page B; recap ending row + REPLAY; per-ending `EndingController` texts;
  `SceneBuilderStory.cs` (44 SN_*.asset content table + scene decoration).
- What I changed myself (Lead): fixed `ScriptableObject.CreateInstance` qualifiers in the
  builder; verified the storyJson persistence contract against `SaveSystem.SaveCaptured/
  SaveApplied`; confirmed the PlayerPrefs-only quality-tier row avoids a compile dependency;
  own the final story text review.
- Teach-back check: pending — walk the team through `SO_StoryConfig` thresholds and the
  storyJson shape before merge (SCRIPT.md maps 1:1 to the built assets).
- Test result: Lead-verified via batch — BuildStoryBatch exit 0, 44 story assets + config
  built and wired into the scenes; ValidateBatch RESULT: PASS.

### 2026-10-30 — Premium pass P4: integration, gates, builds — Abdelrhman Mohamed (Lead TD orchestration)
- Tool used: ZCode orchestration; all Unity batch runs executed and verified by the Lead.
- What AI provided: none directly — integration sequencing, compile-error triage (7 distinct
  Unity-6 API drifts across agent files, each verified against installed package sources),
  batch pipeline ordering (BuildAll → Import → Story → ArtPass → Lighting → Validate →
  Bake → platform builds).
- What I changed myself: the `BuildAll` wiring (art pass + lighting + quality variants +
  cinematic rigs run after every scene rebuild so a rebuild can never ship greybox); discovered
  and fixed the `-batchmode -quit` vs `BakeAsync` race that produced a false-success bake;
  re-ran the bake without `-quit`.
- Teach-back check: pending — the batch pipeline order lives in `AGENTS.md` and the reports.
- Test result: see the final section of this log and `Docs/PERFORMANCE.md` for the gate results.

### 2026-10-31 — Round 3: player-reported bug fixes + horror presentation pass — Abdelrhman Mohamed (Lead TD orchestration)
- Tool used: ZCode orchestration (GLM-5.3 Flash) with 3 read-only explore agents for the
  catch/respawn trace, UI-systems audit and audio-pipeline map; all fixes applied and
  verified by the Lead via batch + built-player smoke.
- What AI provided: root-cause traces (cinematic camera corruption = "reversed controls";
  FpsCounter absent from scenes + self-disable; no OnApplicationFocus cursor handling),
  the catch→respawn timeline, the BuildQuiz/LoadingScreen/ProceduralSfx seams, and first
  drafts of the wraith mesh, AudioImportTool/SfxRegistry, Footsteps, MenuCameraDrift/
  MenuTitleFX, LoadingTheme, and the darkness/cookie/beam values.
- What I changed myself: found the biggest bug of the pass while verifying the FPS fix —
  `BuildCinematicRigs` re-opened the STALE school scene over the unsaved fresh build
  (every school-scene builder change since round 1 had been silently discarded); fixed
  the save order. Chose the procedural wraith over unlicense-verifiable model pages;
  picked CC0/CC-BY audio sources with a library-first seam so synthesis stays as
  fallback; reviewed all downloaded files against the license table.
- Teach-back check: pending — walk a teammate through the save-order bug (why rigs
  must run after the first SaveScene) and the library-first audio seam.
- Test result: BuildAllBatch exit 0; ValidateBatch RESULT: PASS; Windows build +
  Android APK (64.5 MB) exit 0; built-player boot smoke 0 errors, audio library
  registered, tier/fps policy active. Feel-based items (darkness taste, audio mix,
  menu drift) await the player's hands.

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

### 2026-11-19 — AGENTS.md PR-only workflow + GitHub CI fast-checks — Abdelrhman Mohamed
- Tool used: ZCode (AI coding agent)
- What AI provided: the "Git workflow (PR-only)" and "Code quality bar" sections and hard
  rule 9 in `AGENTS.md`; new `.github/workflows/validate.yml` (CI gate: asset `.meta`
  pairing, merge-conflict marker scan, required-files/editor-version pin).
- What I changed myself: scoped the meta check to skip `.gitkeep` placeholders; kept full
  Unity validation (`ValidateBatch`) as a local pre-merge gate rather than CI (no Unity
  license secret configured for Actions); chose squash-merge to keep `main` linear.
- Teach-back check: pending — docs/workflow change only; explain the PR-only gate to the
  next human or agent collaborator before their first contribution.
- Test result: CI check logic dry-ran locally against `main` (20 `.gitkeep` files excluded,
  0 conflict markers, required files present, editor pin matches 6000.4.0f1); the same
  checks run as GitHub Actions on this PR.

### 2026-11-19 — CI/CD player-build pipeline (Windows + per-ABI Android) — Abdelrhman Mohamed
- Tool used: ZCode (AI coding agent, GLM) with web research on GameCI / GitHub Actions.
- What AI provided: `.github/workflows/build.yml` — parallel jobs building a Windows player
  (StandaloneWindows64 via `unityci/editor:ubuntu-6000.4.0f1-windows-mono-4.0.0`) and separate
  APKs per ABI (arm64-v8a / armeabi-v7a, one matrix job each via
  `game-ci/unity-builder@v4` + `buildMethod`), per-job `Library` caches, artifacts, and a
  tag-triggered `release` job. Also fixed `SceneBuilder.cs` batch builds exiting 0 on failed
  `BuildPlayer` (now throw → exit 1; menu dialogs catch) and added per-ABI batch entry
  points (`BuildAndroidArm64Batch`, `BuildAndroidArmV7Batch` → `Builds/Android/<abi>/LightShadow.apk`).
- What I changed myself: chose GitHub-hosted runners (public repo = free unlimited minutes);
  approved the strict merge gate (all build checks required per PR); supplied the Personal
  license as the `UNITY_LICENSE` secret from this machine's `Unity_lic.ulf`.
- Teach-back check: pending — explain the pipeline (matrix per ABI, one editor invocation per
  ABI, Library cache keys) to the next collaborator.
- Test result: local `ValidateBatch` compiles the new entry points (`RESULT: PASS`). First CI
  run: Windows + arm64-v8a + armeabi-v7a all built green (player 129.7 MB, APKs ~64 MB each);
  the initial x86_64 job failed with `UnityException: x86-64 (Magic Leap) support is now
  limited` — Unity 6000.4 deprecates that target, so the x86_64 matrix entry and batch method
  were removed and the pipeline corrected on this branch.



### 2026-11-19 — Docs/SOURCING_FONTS_MUSIC.md (premium-pass font + music sourcing) — Abdelrhman Mohamed
- Tool used: ZCode (AI coding agent, GLM) with WebSearch/WebFetch license-and-URL verification.
- What AI provided: verified sourcing doc — 5 font families (Noto Naskh Arabic, Cairo, Reem Kufi,
  Special Elite, Inter) with exact download URLs and license-file locations; 10 primary + 4
  alternate music candidates (Pixabay Content License / Incompetech CC-BY / FMA CC0) mapped to
  tension beds, chase, memory theme, and the four ending moods; 3 dawn-ambience SFX beds; a
  download checklist with T_/MUS_/AMB_ target filenames. No files were downloaded.
- What I changed myself: flagged The_Mountain's track as AI-generated and Mohamed_hassan's
  "Chase Pulse" was skipped for the chase slot for that reason; chose the Pixabay primary set over
  Incompetech where mood tags verified better; marked the Cairo/Reem Kufi presentation-forms (FE70)
  caveat as a required in-editor test against ArabicSupport before adoption.
- Teach-back check: pending — walk the team through the Pixabay Content License limits (no
  standalone redistribution; no attribution required) vs CC-BY credit lines before any download PR.
- Test result: research only — every listed URL resolved HTTP 200/206 or rendered a page on
  2026-11-19 (verification appendix in the doc); repo untouched except this doc + log entry.

### 2026-11-19 — Premium pass P0: release fix, missing art assets, adopted-plan docs — Abdelrhman Mohamed
- Tool used: ZCode (AI coding agent, GLM) with 3 parallel sub-agents (docs set, audio-mixer
  spec, font/music sourcing).
- What AI provided: release-job root cause (leftover x86_64 rename loop aborted the tag run
  before the release step) and the fix in build.yml; re-ran ArtImporter + BuildAllBatch so
  `SM_Wraith.asset`, `SM_BeamCone.asset`, `M_Art_Beam.mat`, `T_FlashlightCookie.png` and the
  rewired Shadow/Player prefabs now exist and are committed (scenes previously referenced
  missing files); docs set — `Docs/MASTER_PLAN.md` (adopted universal production plan with
  reconciliation header), `PROJECT_STATE.md`, `TASKS.md` (P0-P5 reconciled backlog),
  `STYLE_GUIDE.md`, `CREDITS.md`; README "Known limitations" replaced with an accurate
  Status section; ROADMAP P4 updated; `AGENT_REPORT_AUDIO_MIXER.md` (real AudioMixer +
  snapshots implementation spec, D15 follow-up); `SOURCING_FONTS_MUSIC.md` (license-verified
  font + music candidates with live URLs).
- What I changed myself: chose "shipped game wins" reconciliation policy (stat scales,
  story graph, tuning stay); dropped iOS and speech input per owner decision; verified
  ValidateBatch RESULT: PASS after the rebuild; kept Assets/.github untouched by sub-agents.
- Teach-back check: pending — explain the reconciliation policy and the new Docs set to the
  next collaborator.
- Test result: `ValidateBatch` RESULT: PASS (0 compile errors) with regenerated meshes
  wired; release job fix verified by log diagnosis; CI build workflow re-runs on this PR.

### 2026-11-19 — Premium pass P1: fonts + real AudioMixer + music — Abdelrhman Mohamed
- Tool used: ZCode (AI coding agent, GLM) implementing `Docs/AGENT_REPORT_AUDIO_MIXER.md`
  nearly verbatim on branch `feat/audio-mixer-snapshots` (stacked PR; Lead runs the Unity
  batches and git push).
- What AI provided: `AudioMixerTool.cs` (reflection builder for `M_Main.mixer` — bus tree with
  duck children, Master low-pass, six snapshots, 14 exposed params — plus `ValidateMixer()`);
  `AudioSnapshotController.cs` (persistent + MemoryFlash overlay snapshots, LitZone poll);
  `VolumeCategories` mixer bridge (append-only Voice/Shadow categories, SetMixer/GroupFor,
  user-param SetFloat, scan overrides for Shadow/Ambience/whisper sources); the
  `GameEvents.EnemyStateChanged` raise in `EnemyBehaviour.EnterState`; GameConfig snapshot
  transition fields; §4.5 touch-points (AdaptiveMusic mixer bypass fix + real-bed layer,
  ShadowAudio/PooledWhisperBurst Shadow routing, `PlaySfxAt` pooled one-shot);
  builder attachments (bootstrap + school Systems) and mixer validation checks; font bundling
  and wiring (SceneBuilderUI font getters, UIFontBinder roles, `Localization.SetFonts`);
  music plumbing (AudioLibrary ids, AdaptiveMusic bed, per-ending cues on EndingController);
  ASSET_LICENSES/CREDITS/TASKS/CHANGELOG/DECISIONS(D17) updates.
- What I changed myself: verified every internal UnityEditor.Audio API against the spec's
  reference-source citations before writing the reflection calls; chose Noto Naskh Bold (not
  Cairo/Reem Kufi) for Arabic titles because those fonts lack the Presentation Forms-B glyphs
  ArabicSupport emits (SOURCING doc caveat); kept the legacy per-source volume path for
  headless/direct-play so loudness is unchanged; confirmed the three music tracks' CC-BY 3.0
  versions on Wikimedia Commons (ISRC-matched file pages) after ID3 tags revealed the staged
  files are Kevin MacLeod tracks renamed to semantic slots; rejected the staged
  `MUS_Bed_DarkHighway.mp3` (FMA login-page HTML, not audio).
- Teach-back check: pending — explain the SetFloat/snapshot orthogonality rule (sliders own the
  seven *Vol params; snapshots own the ducks + low-pass) and the manual listening checklist
  (spec §7.2) to the next collaborator.
- Test result: pending Lead batch run — expected `AudioMixerTool.EnsureBatch` → `RESULT: PASS`
  (mixer), `BuildAllBatch` rebuild, then `ValidateBatch` → `RESULT: PASS`; NOT yet run by the
  agent (single-Unity-instance rule).

### 2026-11-27 — Mobile stretch fix: native-resolution guard + safe-area fitter — Abdelrhman Mohamed
- Tool used: Instinct (AI coding agent)
- What AI provided: diagnosis that `GameSettings.ApplyResolution` forced a picked resolution on
  Android (framebuffer stretched to fill a different-aspect display); the mobile gate in
  `ApplyResolution`; `SafeAreaFitter` (fits canvas content to `Screen.safeArea`); the
  `SceneBuilderUI.CreateCanvas` change parenting every canvas's content under a `SafeArea` child.
- What I changed myself: pending review — changes land through the PR workflow (AGENTS.md rule 9).
- Teach-back check: pending — explained in the PR body (why forcing a resolution distorts on 20:9
  phones, and why the safe-area child only insets on notched devices).
- Test result: pending — CI `build` workflow (Windows player + arm64/armv7 APKs) is the gate;
  no local Unity editor available.

### 2026-11-27 — SaveSystem deferred-restore fix (audit A3) — Abdelrhman Mohamed
- Tool used: Instinct (AI coding agent)
- What AI provided: `SaveSystem.ClearPendingRestore` and the new-game hook in
  `SaveSlotUI.StartRun`, so a deferred restore from an earlier Load attempt cannot apply
  a stale save to a fresh run; the AUDIT_2.md status flip for finding A3.
- What I changed myself: pending review — lands through the PR workflow.
- Teach-back check: pending — repro steps are in the PR body and AUDIT_2.md.
- Test result: static reasoning only (no editor); CI build workflow is the gate.
