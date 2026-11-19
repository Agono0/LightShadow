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

### 2026-11-19 — P2 Blender hero-prop pipeline (Art/pipeline + Assets/_Project/Models/Hero) — Abdelrhman Mohamed
- Tool used: ZCode (AI coding agent, GLM) driving Blender 5.3 headless (`blender -b -P`).
- What AI provided: the full pipeline — `Art/pipeline/` (`ls_common.py` registry/helpers,
  `ls_props.py` procedural builders, `ls_export.py` Unity-ready FBX exporter, `ls_preview.py`
  EEVEE turntable renderer, `run_all.cmd` driver); 14 procedural props per the mission list
  (8 story items incl. the crank flashlight with pivot-ready Crank node, SM_Wraith_HD
  smoke silhouette, door with iron grille, chalkboard with tray, gate crank, iron gate);
  112 turntable preview frames; the `HeroModels` import-settings table in `ArtImporter.cs`
  with a UV1 presence check; tri-count manifest at `Art/pipeline/manifest.json`.
- What I changed myself: kept every prop under its tri budget (small ≤ 3k, hero ≤ 30k) by
  construction instead of decimating; chose `bake_space_transform=True` for single-mesh props
  (master plan §7.4 "Apply Transform on") but `False` for the flashlight FBX so the Crank
  child keeps its axle pivot for the P3 crank minigame; named Blender materials after the
  existing `Mat_Art_*` Unity assets for name-for-name remap; caught and fixed three
  Blender 5.3 API breaks (`cap_ends`→`end_fill_type`, removed `use_anim`, spot light `size`)
  and a cube-scale halving bug (5.3 `size=1` cube spans ±0.5) by round-trip verifying the
  exported FBX in Blender; did NOT touch the SceneBuilderArt swap tables (lead wires swaps).
- Teach-back check: pending — walk the lead through `run_all.cmd` (build → export → render),
  the manifest tri counts, and why the flashlight FBX must keep Apply Transform off.
- Test result: `ls_export` RESULT: PASS — 14/14 FBX exported, all within budget (largest:
  SM_Wraith_HD 13,728 tris of 30k; smallest: SM_Story_Prism 52 of 3k); FBX re-import
  verifies UV0+UV1 on every mesh, correct material names, upright axes, and the Crank pivot
  at (-0.03, 0, 0.135) with identity rotation; 8 turntable PNGs per prop in Art/Previews/;
  Unity runs intentionally left to the lead (no Unity.exe invoked).

### 2026-11-19 — P2 hero-prop wiring (SceneBuilder partials + ArtImporter) — Abdelrhman Mohamed
- Tool used: ZCode (AI coding agent, GLM) — code-only pass, no Unity.exe runs (lead verifies).
- What AI provided: the full swap wiring for the 14 Blender FBX models — `LoadModelAsset` now
  resolves `Assets/_Project/Models/Hero` beside the Kenney packs; an `AddHeroModelChild` helper
  that instantiates hero FBX on an unscaled carrier under scaled greybox hosts (uniform
  world-space height, metre offsets, defensive collider removal, layer sync) and rebinds every
  renderer onto the canonical `Mat_*`/`M_*` art materials through the new
  `ArtImporter.HeroMaterialMap` (FBX-embedded names verified 1:1 against
  `Art/pipeline/manifest.json`); `BuildStoryMaterials` for the three FBX slots that had no
  factory asset (`Mat_Story_RedInk` red/smooth, `Mat_Story_Glass` transparent/smoothness 0.92,
  `Mat_Story_LED` emissive amber with `_EMISSION`); scene swaps — MemoryBox →
  `SM_Story_MemoryBox` + card on the lid, six fragment instances → their hero story items
  (paper placeholder darkened, kept as fallback), `Door_Classroom`/`Door_Staff` decor hosts →
  `SM_Arch_DoorGrille`, classroom chalkboard host → `SM_Arch_Chalkboard`, ExitGate console →
  `SM_Arch_GateCrank` with the wheel hub on the interact point, forecourt fence gap extended
  with `SM_Arch_IronGate` sections; prefab swaps — `SM_Wraith_HD` inside Shadow_Art (marker
  `ArtModel_SM_Wraith_HD`, procedural mesh kept as fallback, M_ShadowSmoke + SmokeHalo
  untouched), `SM_Player_Flashlight` at the shared hand anchor (`FlashlightHandAnchor`
  compensates the tail origin), QuizBoard board → hero chalkboard; `ValidateBatch` now reports
  14/14 hero FBX assets, per-model swap counts, null material slots, and the story-material
  looks; AI_DEV_LOG + TASKS ticked.
- What I changed myself: chose an unscaled carrier because a child of a scaled greybox cube
  inherits its per-axis scale and a yaw would swap the compensation axes; derived every anchor
  from the exporter conventions (`set_base_origin` bottom-centre, Blender (x,z,-y) → Unity
  mapping, Forward -Z/Up Y) instead of eyeballing — e.g. the classroom chalkboard needs yaw
  180 because the FBX chalk face points +Z while that wall faces -Z into the room, and the
  gate crank needs yaw 90 so the wheel plane (axle along Blender X) faces the player with the
  wall plate flush against the console front; kept the beam cone/cookie on the Flashlight
  light object completely untouched; made every swap optional so a missing FBX degrades to the
  existing primitive/Kenney stand-in; kept prefab GUIDs stable (Shadow_Art rebuild keeps the
  marker-based skip; Hands_Art is not deleted — the restyle migrates it in place).
- Teach-back check: pending — walk the lead through the swap table entries, the fallback
  behaviour per hero model, and why `ImportAllBatch` must run before `BuildAllBatch` (fresh
  prefab builds embed Shadow_Art/flashlight only when the assets already exist).
- Test result: Roslyn (C# 9) parse check clean on all five edited editor scripts; Unity
  intentionally not run — lead to run `ImportAllBatch` then `BuildAllBatch` then
  `ValidateBatch` and grep for `RESULT: PASS` (see the wiring report for expected lines).

### 2026-11-19 — Phase 3 horror systems — Abdelrhman Mohamed
- Tool used: ZCode (AI coding agent, GLM) implementing Docs/TASKS.md P3 + Docs/MASTER_PLAN.md
  §4.6/§4.7/§4.11/§4.12 on branch `feat/horror-systems` (stacked PR; Lead runs the Unity batches
  and git push).
- What AI provided: `NoiseEmitter` + `GameEvents.NoiseEmitted` (per-action hearing radii:
  sprint 10 / walk 4 / crouch 1 / crank 12 / wrong-answer 6 m) and the Shadow `Investigate`
  state (walk to the noise, sweep look-around, resume Patrol — replaces the old stop-and-face
  sprint hack); `FlashlightCrank` (hold + circular-gesture accumulator over mouse/touch/left
  stick, charge capped at crankMaxCharge, ratchet SFX + 12 m noise per tick, SFX_Crank
  library-first with a synthesised fallback); `MenaceDirector` (gauge 0–100, forced
  ForcedCooldown 45–60 s at the cap, 30 s post-chase grace consumed by the Chase decision,
  dawn multiplier); hiding v1 (`HideSpot` registry + `HidingSystem` enter/exit hop, hold-breath
  meter draining story Calm, breath/release noises, `Search` state that checks 1–2 random spots
  and never the occupied one on the first pass, `EnemySight` hidden check with the
  watched-entry exemption); banish rebalance (focused beam hold 2 s at meter ≥ 25 → 20 s
  retreat costing 25 meter; old instant-repel fields deprecated); `CalmEffectsController`
  (high-priority runtime Volume override vignette + heartbeat pacer + `CameraShake.SetTremor`
  floor, fed by the new `GameEvents.CalmChanged` raised from StoryDirector's single
  `ChangeCalm` funnel); `InspectViewController` (self-building 360° close-up, drag/swipe orbit,
  auto-close); RecapScreen SAVE PNG export; `HapticsService` (Handheld.Vibrate wrappers,
  light/heavy hooks); `DevConsole` (backquote + DEBUG chip behind the new LS_DebugMode flag;
  setflag/givemeter/teleport/skipact/toggleai/timescale); scene-builder wiring for all of it
  (player prefab components, MenaceDirector on Systems, CRANK/BREATH touch buttons, SAVE PNG
  button, 6 hide spots + 2 corridor lockers) and ValidateBatch additions.
- What I changed myself: kept `sprintNoiseRange`/`repelRecoil`/`repelCooldown`/
  `repelMeterThreshold` serialized but deprecated (existing GameConfig assets keep loading) and
  used new spec-valued fields instead; mapped the plan's "hold-breath drains Calm 2/s" (0–100
  scale) onto the implemented -1..1 story stat as `breathCalmPerSecond = 0.05` with the unit
  mismatch documented in GameConfig comments; routed Investigate/Search into the existing Stalk
  audio snapshot; made the desk hide plates full height so the exit ray works at standing eye
  height; added ExitCurrent input paths for all three input families (touch Interact reroute,
  gamepad A poll, desktop E) because the interact ray cannot see the spot from inside the prop.
- Teach-back check: pending — walk a teammate through the noise bus contract
  (NoiseEmitter → GameEvents → FSM) and the banish hold-vs-tap input split on the LIGHT button.
- Test result: pending Lead batch run — expected `BuildAllBatch` (scenes/prefabs rebuilt:
  new lockers bake into the NavMesh) then `ValidateBatch` → `RESULT: PASS` with the new
  Hide spots / MenaceDirector / player-component checks; NOT yet run by the agent
  (no Unity.exe per task rules).

### 2026-11-19 — P3 horror-systems review fixes (feat/horror-systems, worktree LS-p3) — Abdelrhman Mohamed
- Tool used: ZCode (AI coding agent, GLM) fixing the quality-review findings on the same
  stacked branch; Lead runs the Unity batches and git push.
- What AI provided: the nine review fixes, each its own conventional commit —
  (1) `HidingSystem.ResetState()` (clears IsHidden/CurrentSpot/EnteredUnderWatch, resets the
  breath meter, releases the input lock, NO teleport) called from `PlayerController.RespawnReset`
  and `GameManager.WakeAtCheckpoint`, plus a `!HidingSystem.IsHidden` exclusion on the
  EnemyBehaviour catch check so the Shadow can no longer grab a hidden player through the spot
  wall (Search's witnessed-entry found-path is untouched); (2) FlashlightBattery's desktop F
  toggle now waits for key release and is skipped when `FlashlightCrank.IsCranking` consumed the
  press, so tap = toggle / hold = crank no longer fight on one key; (3) TouchButton's
  hold-to-focus honours `ExternalInputLocked` (no new focus while locked, press dropped and
  started focus released when the lock engages); (4) `EnemySight.HasLineOfSightCached()`
  frame-stamped lazy cache shared by EnemyBehaviour and MenaceDirector — one sight Linecast per
  frame instead of two (witnessed-entry check keeps the uncached call on purpose); (5)
  CalmEffectsController.OnDestroy detaches and destroys the runtime VolumeProfile it created in
  Awake (was leaked every scene load); (6) GameEvents.EnemyStateChanged comment now lists the
  full state payload (ForcedCooldown 5, Search 6); (7) RecapScreen per-export Debug.Log removed;
  (8) FlashlightCrank ratchet clock resets to 0 when idle — no instant ratchet on re-engage;
  (9) defensive port from main: `VolumeCategories.SetMixer` runs `ApplyAll()` after group
  resolution so sources registered before mixer activation land on their buses.
- What I changed myself: deviated from the reviewer's literal fix for finding 2 — gating the F
  toggle on `FlashlightCrank.IsHeld` as suggested would have killed the desktop flashlight
  toggle entirely, because `IsHeld` includes `KeyboardHeld` (GetKey F), which is already true on
  the GetKeyDown frame and F is the only desktop toggle (gamepad X / touch button cover the
  other platforms). Implemented the hold-vs-tap split instead, matching the LIGHT touch button's
  existing pattern; no new GameConfig fields were needed (no new tunables introduced anywhere).
- Teach-back check: pending — walk a teammate through why the hiding statics must be cleared on
  the respawn path (EnemySight reads the static) and the F tap-vs-crank decision tree.
- Test result: pending Lead batch run — expected `BuildAllBatch` + `ValidateBatch` →
  `RESULT: PASS` (validation asserts the touched systems: player components, MenaceDirector,
  hide spots); NOT run by the agent (no Unity.exe per task rules).

### 2026-11-19 - Task P4: Arabic question content for all 60 questions - Abdelrhman Mohamed
- Tool used: ZCode (AI coding agent, GLM) - task P4 of Docs/TASKS.md.
- What AI provided: full Arabic translation table (questionAr/optionsAr/explanationAr for all 60
  Q_*.asset questions) in Egyptian-flavored MSA with the mother's voice for explanations; the fill
  script `Art/pipeline/fill_arabic_questions.py` (append-only YAML edit after `factSource`, asserts
  correctIndex and untouched EN bytes, mimics Unity's YAML quoting, refuses strings that the
  ArabicSupport display shim would word-reverse - e.g. multi-word English runs or a number last
  inside parentheses); a ValidateBatch assertion counting Arabic-complete questions
  (SceneBuilderValidation: "Question Arabic fields: 60/60").
- What I changed myself: kept every correctIndex and all EN bytes untouched (git diff: 420
  insertions, 0 deletions, GUIDs/.meta identical); adapted the Language items whose on-screen
  display would break - Lan_002/Lan_004 prompts re-anchored to Arabic sentences (same grammar
  skill, same correctIndex), Lan_003/Lan_009 options kept pure-English so the shim skips them;
  flagged those adaptations for teacher review.
- Teach-back check: pending - walk a teammate through one .asset diff (7 appended lines per file)
  and the ArabicSupport token-order rule the script enforces.
- Test result: script re-parses all 60 assets with YAML and round-trips every Arabic field (0
  empty); empty-field grep = 0; factSource stays NEEDS-FACT-CHECK on all 60 (never claimed
  review). Unity ValidateBatch to be run by the lead (no Unity run in this task). Human/native
  Arabic review still required before release.

### 2026-11-19 — Premium pass P5: app-icon pipeline, store listing, VO recording specs — Abdelrhman Mohamed
- Tool used: ZCode (AI coding agent, GLM) on a dedicated release-prep worktree
  (`LS-p5`, branch `feat/release-prep`).
- What AI provided: `Art/Pipeline/ls_icon.py` — a Blender 5.3 EEVEE headless
  pipeline that builds the flashlight emblem (stylized silhouette + warm beam
  cone on the STYLE_GUIDE gold-on-dark palette, #E8B54A on #0B0F16) and renders
  `Art/Previews/icon_{1024,512,192,96}.png` natively per size; `Docs/STORE_LISTING.md`
  (EN/AR titles within limits, short + full descriptions, keyword candidates,
  IARC age-rating answer sheet, no-data privacy blurb); `Docs/VO_RECORDING_SPECS.md`
  (per-character voice profiles from STORY_BIBLE §3-4, dry 48 kHz/24-bit chain
  normalized to −16 LUFS, `VO_<character>_<nodeId>_<lang>.wav` naming, and the
  per-speaker line-count table grepped from the 44 `SN_*.asset` files —
  Nour 22 / Mother 7 / Shadow 9 / Memory 6, plus the 4 ending-epilogue echoes
  found in `EndingController.localizationKeys[1]` = 96 delivered files EN+AR).
- What I changed myself: iterated the icon render three times from visual checks
  (rotated the cylinders onto the beam axis, made emissive planes black-base and
  specular-free after a diagnostic render showed the area lights washing the
  backdrop grey, smoothed the beam to four nested cones); kept ProjectSettings
  untouched and documented the PlayerSettings wiring path in the script header
  instead; verified the 96 px render stays readable; kept TASKS.md honest by
  splitting the store-assets item (screenshots need Unity runs, this pass had
  none) rather than marking it done.
- Teach-back check: pending — walk the team through the icon pipeline header
  (how Unity consumes the previews) and the VO integration point
  (StoryDirector.cs:314 + the empty Voice group in the M_Main mixer).
- Test result: no Unity runs by design; Blender headless exits 0 and wrote all
  four PNGs (verified visually at 1024/192/96); speaker tallies re-derived
  twice (22+7+9+6 = 44 nodes); docs-only changes so the CI fast-checks are the
  gate for this branch.

### 2026-11-19 - Task P4 follow-up: question bank expansion 60 -> 120 - Abdelrhman Mohamed
- Tool used: ZCode (AI coding agent, GLM) - P4 follow-up item in Docs/TASKS.md.
- What AI provided: 60 new school-level questions (Q_<Cat>_011+, Sci/Mat/His/Geo/Lan/Hea) each
  with EN prompt, 4 EN options, correctIndex, mother-voice EN explanation, and the full Arabic
  set (questionAr/optionsAr/explanationAr, Egyptian-flavored MSA, single-token Latin islands for
  the ArabicSupport shim); the authoring script Art/pipeline/add_120_questions.py (writes assets
  in the exact serialized QuestionData format, gates wing quotas, numbering continuity, display
  safety and duplicate prompts, refuses to touch existing files) and the verifier extension
  Art/pipeline/verify_arabic_questions.py (120 files; legacy 60 byte-identical to the branch
  base, new 60 fully fielded).
- What I changed myself: assigned the wing mapping and the 4 easy / 5 standard / 3 hard quota per
  wing (MASTER_PLAN section 4.5: Library, Science wing, Courtyard, Assembly hall, Staff room);
  verified every fact against general knowledge and reworded His_016's prompt after the
  duplicate-prompt gate flagged a collision with His_004; kept every factSource at
  NEEDS-FACT-CHECK (rule 5), recorded the source=curriculum designation in the authoring table
  because QuestionData has no serialized `source` field, and authored no .meta files (Unity
  generates them on import; QuestionDatabase relinks via GenerateQuestionsSilent on the next
  editor run - SceneBuilder.cs calls it before building the school).
- Teach-back check: pending - walk a teammate through the wing-to-id mapping table in
  add_120_questions.py and the append-only guarantee (git diff shows 0 deletions on the legacy
  assets).
- Test result: python -I add_120_questions.py --apply created 60 assets (folder count 120), each
  round-tripped via YAML (EN+AR fields, 4 options, correctIndex, category, difficulty);
  python -I verify_arabic_questions.py -> "RESULT: PASS - 120-question bank intact: legacy 60
  untouched at the base ref, new 60 carry all EN+AR fields (4 options each), factSource
  untouched, no duplicate prompts." Unity ValidateBatch to be run by the lead (no Unity run in
  this task); human fact-check of all 120 still required before release.
