# AGENT_REPORT_NARRATIVE.md — Sub-Agent 3 (NARRATIVE, UI/UX & AUDIO)

Premium pass, branch `feature/premium-pass`. Scope per `Docs/ROADMAP.md` P3 and the brief.
Everything below was written as files only — no Unity runs, no git, no hand-edited YAML.
All scene/UI/story-asset generation lives in one editor partial the Lead executes.

---

## 1. Summary & story design

### What was delivered

- **Story bible + branching script** (`Docs/Story/`): tone rules, three voices (Nour /
  Mother / Shadow / System), stats, flag lexicon, and the full authored graph.
- **Data-driven story system** (`Scripts/Story/`): `StoryNode` SO graph + `SO_StoryConfig`
  thresholds + `StoryDirector` runtime + `StoryEvents` bus. **Zero story text in runtime
  code** — every line is data in `SN_*.asset` files built by the editor partial.
- **Dialogue UI**: bottom box with punctuation-aware typewriter, speaker tag, 2–4 choice
  rows (hover/press tint + scale, controller-navigable, 58 px rows on 64 px pitch), a
  shrinking bar for timed choices, HOLD-TO-SKIP and AUTO-ADVANCE toggles. Quiz keeps
  priority — the director never opens a node while `QuizUIController.IsOpen`.
- **Four endings wired into 03_Ending additively** (per-ending title/body/tint), recap
  gains ending name + REPLAY button, original recap behaviour untouched.
- **Save slots + loading screen**: 3-slot panel (load / new+overwrite-confirm / delete
  confirm, CONTINUE = NewestSlot) on the main menu; fullscreen dark loading overlay with
  a procedural Al-Noor silhouette and async progress bar, used for all UI-owned scene
  transitions.
- **Accessibility**: `AccessibilityStore` (PlayerPrefs) + settings **page B** (11 new rows
  on a MORE OPTIONS second page) + colourblind-safe palettes applied to all story-owned UI
  colours.
- **Audio**: `VolumeCategories` code-level mixer (D15, honest) with auto-registration by
  name convention at scene load; `AdaptiveMusic` layered procedural score (calm pad /
  tension / pulse) extending the AmbientDrone concept without touching it.
- **Localization**: all new strings are EN/AR pairs. Keys are the English sentences
  (project convention); Arabic lives in `StoryStrings.cs` and displays through the shared
  path (`Localization.Present` + `ArabicSupport`, `Localization.CurrentFont`).

### Story shape (full text in `Docs/Story/SCRIPT.md`)

**12 authored branching moments** at natural beats: after each fuse box, at fragments,
before the gate — The Gate (act start), The Card, Fuse 1 (light/stay/step out), Bookmark,
Lit Window (Fuse 2), The Bench (jasmine), Optics Cabinet, **The Shadow Speaks** (roll call
— the key branch), The Mug (Fuse 3), Main Hall confrontation, Dawn (timed route choice),
The Crank (the promise). Plus three short memory-flash beats (exam paper, gate-powered,
etc.). 44 nodes total (41 dialogue + 4 ending nodes, minus overlaps).

**Stats** (−1…+1 each, story-side only): `boldness` (risky lines; unlocks the
`knows_backstairs` dawn shortcut), `trust` (leaving things where they grew; required for
confrontation), `calm` (composure; gates risky outcomes — a risky choice with calm < 0
routes to the harsh variant via data-driven `nextIfCalmNegative`). **Timed choices**
(6–10 s, shrinking bar) sit where hesitation is in character; a timeout routes to a
timeout node with a small calm dent.

**Consequence spine** (brief's "shortcut or locked dialogue"): the bold roll-call branch
grants `knows_backstairs` → at dawn, the shortcut choice skips SN_Act3_05/06 entirely —
fewer input-locked interruptions inside the timed chase (a real, honest gameplay
advantage). Leaving everything in the staff room (`left_all`) unlocks the terse
"say nothing" promise and shades the Hollow Escape / Dawnless epilogues.

**Four endings (D13)** — formula in `SO_StoryConfig` (thresholds are serialized fields,
not code):

| Ending | Trigger (defaults) |
| --- | --- |
| Full Recovery | fragments == 6 AND accuracy ≥ 0.75 AND flag `confronted_shadow` |
| Partial Memory | fragments ≥ 4 AND accuracy ≥ 0.5 |
| Hollow Escape | escaped, otherwise |
| Dawnless | caught 2× after `DawnStarted` (checked first; ends the night via the public `GameManager.Escape()` — bloodless fade, not a game-over screen) |

Accuracy = correct/asked, tracked by the director from `GameEvents.QuestionAnswered`
across the whole run (crosses saves inside storyJson). Each ending defines title, body,
narrator line (all EN/AR) and a dawn tint for the 03_Ending sky/sun/camera/ambient/fog.

---

## 2. Files created / modified

### Created — docs
- `Docs/Story/STORY_BIBLE.md` — tone, characters, stats, endings, localization policy.
- `Docs/Story/SCRIPT.md` — full node script + asset-graph addendum (timeout nodes).
- `Docs/AGENT_REPORT_NARRATIVE.md` — this file.

### Created — `Assets/_Project/Scripts/Story/` (all new)
| File | Role |
| --- | --- |
| `StoryNode.cs` | SO: nodeId, speaker/act/trigger+ordinal, flag gates, text, choices (setFlags, req/block flags, stat deltas, `nextIfCalmNegative`, isTimed/timeSeconds/timeoutNextId), node-enter effects, isEnding/endingId, onEnterEventHint |
| `StoryConfig.cs` | SO: node list, act-start id, ending ids + thresholds, dawnless catch limit, pacing (trigger delay, chain gap, timeout calm dent) |
| `StoryEvents.cs` | static bus: `NodeEntered`, `ChoiceMade`, `NodeLeft`, `ConversationEnded`, `EndingDecided`, `ChoiceTimedOut` |
| `StoryStrings.cs` | EN/AR table (~150 entries): all dialogue, choices, endings, save-slot/load/settings/volume chrome |
| `AccessibilityStore.cs` | PlayerPrefs-backed textSpeed 0..3 / subtitleSize 0..2 / colorblindMode 0..2 / autoAdvance / skipHold; subtitle scale; typewriter cps; colourblind palettes (Standard / Blue-Orange deuteranopia-safe / High contrast); quality-tier row reads/writes the shared `ls_qualityTier` pref only (no compile dependency on PerformanceManager) |
| `StoryDirector.cs` | singleton (DontDestroyOnLoad, self-healing no-op without builder wiring): trigger queue from GameEvents, quiz-priority hold, flag/stat bookkeeping, calm-gated routing, chain scheduling (race-guarded), narration via `StoryTeller.Speak`, input lock via `PlayerController.ExternalInputLocked`, D13 ending decision, `SerializeState`/`DeserializeState`, SaveCaptured/SaveApplied hooks |
| `DialogueUIController.cs` | typewriter (cps from AccessibilityStore, punctuation pauses), choices offered by the director, timed bar, skip-hold + auto-advance, scale/tint feedback (`ChoiceScaleFeedback`), controller/keyboard nav, AR font/alignment, live language re-render |

### Created — `Assets/_Project/Scripts/Audio/` (all new)
| File | Role |
| --- | --- |
| `VolumeCategories.cs` | D15 code mixer: Master/Music/Ambience/SFX/UI, 0–10 PlayerPrefs (`ls_vol_*`), `Register`, per-scene auto-registration by name convention (`MUS_/Music*`, `AMB_/Ambien*`, `SFX_`, `UI*`), `volume = base·category·master` |
| `AdaptiveMusic.cs` | three own AudioSources, clips pre-rendered once (calm A-minor pad 14 s / tension drone 8 s / heartbeat pulse 4 s); tension from low `MeterChanged` + catch recency; pulse from catches/gate/dawn; per-frame volume crossfade (zero alloc); Music category applied inline |

### Created — `Assets/_Project/Scripts/UI/` (new)
| File | Role |
| --- | --- |
| `SaveSlotUI.cs` | 3-slot panel: per-slot LOAD/NEW/DELETE with confirm overlays, CONTINUE (NewestSlot), BACK; sets `SaveSystem.CurrentSlot`; runs through `LoadingScreen`; `StoryDirector.ResetForNewRun()` on new game |
| `LoadingScreen.cs` | static `Transition(sceneName)` / `Show()` / `Hide()`; self-creating DontDestroyOnLoad overlay; silhouette motif from plain rects; async progress; unscaled-time fades |

### Created — `Assets/_Project/Scripts/Tools/Editor/`
- `SceneBuilderStory.cs` — partial of `LightShadow.Tools.SceneBuilder`: menu
  **LightShadow ▸ 9. Story, Dialogue, Saves & Audio**, batch entry
  `SceneBuilder.BuildStoryBatch()`, idempotent asset builder (44-node content table →
  `Assets/_Project/Data/Story/SN_*.asset` + `SO_StoryConfig.asset`), and scene decoration
  (main menu save panel + SAVES button + settings page B; school director/music/dialogue
  box + pause settings page B; ending recap row + REPLAY button). Self-contained
  `MakeStory*` helpers in the SceneBuilderUI visual style.

### Built outputs (produced by the editor code above, not hand-written)
- `Assets/_Project/Data/Story/SO_StoryConfig.asset` + 44 × `SN_*.asset` (+ .meta files
  generated by Unity on import).

### Modified — additive only (anchor points)
| File | Additive anchors |
| --- | --- |
| `Scripts/UI/SettingsPanelController.cs` | new row consts 11–21 + `PageARows/TotalRows`; extended `RowKeys`; page B cases in `Previous`/`Next`/`RowValue`; `moreButton/moreLabel` wiring; `BindStoryRows()`, `TogglePage()`, `SetPage()`, private `ApplyPageVisibility()`; `Show()` now also applies page visibility. Scenes without the rebuild keep exactly the old 11-row card. |
| `Scripts/UI/RecapScreen.cs` | new `endingText`/`replayButton` fields, `SetEnding(id)`, ending row rendering + `ReplayTheNight()` (LoadingScreen → main menu). `Show()` signature unchanged. |
| `Scripts/UI/EndingController.cs` | `EndingTint` class + 4 default tints; Start() asks `StoryDirector.EnsureEndingDecided()` and overrides title/body/narrator + tint when an ending node exists; recap gets `SetEnding`. Without a director (scene played raw) the original DAWN behaviour is the fallback. |
| `Scripts/UI/MainMenuController.cs` | new `saveSlots`/`savesButton` fields; `OpenSaves()`; `Play()` shows the slot panel when saves exist (new night starts straight away when clean); `StartNewNight()` routes through LoadingScreen; Esc closes the slot panel. |

Not touched (verified): any `.unity`/`.prefab`/`.asset` YAML, `Packages/`, `ProjectSettings/`,
`Scripts/Core|Player|Enemy|Gameplay|Systems|Art/`, other agents' `Tools/Editor` files,
`AI_DEV_LOG.md`, `ASSET_LICENSES.md`.

---

## 3. Integration instructions for the Lead

### Compile-time dependencies (merge ENG first or same commit — REQUIRED)

My runtime code references these ENG/Core symbols directly (they exist on disk already):

- `LightShadow.Core.GameEvents` — `FuseRestored`, `FragmentCollected`, `GatePowered`,
  `PlayerCaught`, `DawnStarted`, `EscapeStarted`, `QuestionAnswered`, `MeterChanged` (+ Raise helpers).
- `LightShadow.Core.SaveSystem` — `SlotCount`, `CurrentSlot`, `SaveData.storyJson`,
  `SaveCaptured`, `SaveApplied`, `SaveToSlot/LoadFromSlot/HasSave/DeleteSlot/NewestSlot`.
- `LightShadow.Player.PlayerController.ExternalInputLocked` (static).
- `LightShadow.Core.GameManager` — `Instance`, `quizUI.IsOpen`, `FragmentsCollected`,
  `FragmentTarget`, `Escape()` (public API, called only by the Dawnless rule).
- `LightShadow.Core` — `Localization` (+`PresentBool`/`OptionLabel`/`TimerLabel`/`CurrentFont`),
  `StoryTeller.Speak/Stop`, `RunResults`, `SceneNames`, `GameSettings.ResetGraphics`,
  `LocalizedText.Attach`, `ArabicSupport` (indirect).
- `Scripts/Systems/PerformanceManager` is **NOT referenced at compile time** — the quality
  tier row reads/writes only the PlayerPrefs key `ls_qualityTier` (brief-specified
  dependency-free pattern). PerformanceManager picks the pref up on its own schedule.

### Unity verification order (Lead runs all Unity commands)

```bash
U="C:/Program Files/Unity/Hub/Editor/6000.4.0f1/Editor/Unity.exe"
# 1. Baseline scene rebuild (all agents' scene content)
"$U" -batchmode -quit -nographics -projectPath . -executeMethod LightShadow.Tools.SceneBuilder.BuildAllBatch -logFile Logs/build.log
# 2. Story pass: builds Data/Story assets AND decorates the 3 scenes (opens+saves them)
"$U" -batchmode -quit -nographics -projectPath . -executeMethod LightShadow.Tools.SceneBuilder.BuildStoryBatch -logFile Logs/story.log
# 3. Content validation
"$U" -batchmode -quit -nographics -projectPath . -executeMethod LightShadow.Tools.SceneBuilder.ValidateBatch -logFile Logs/validate.log
# 4. Player builds as usual (BuildWindowsBatch / BuildAndroidBatch)
```

- One-liners: `LightShadow ▸ 9. Story, Dialogue, Saves & Audio` =
  `SceneBuilder.BuildStoryAll()` (assets + scenes); `SceneBuilder.BuildStoryBatch()` is the
  batch wrapper. Run **BuildAllBatch before BuildStoryBatch** (story decoration expects
  the four scenes and their canvases to exist), and **BuildStoryBatch before ValidateBatch**.
- Re-running `BuildStoryBatch` is safe/idempotent (assets updated in place; scene objects
  created only when missing; settings arrays resized once).
- Commit the generated `Assets/_Project/Data/Story/*.asset` + `.meta` files with the code.
- Grep logs for `RESULT:|error CS|Exception` as usual.

### Runtime wiring map

- `02_School` Systems object: `StoryDirector` (config + dialogueUI wired) + `AdaptiveMusic`.
- `UI_School` canvas: `DialogueUIController` (+ children) — pause settings card gets page B.
- `UI_MainMenu` canvas: `SaveSlotUI` panel (hidden), SAVES button under QUIT, menu settings
  card page B, `MainMenuController.saveSlots/savesButton` wired.
- `03_Ending`: recap `endingText` + `ReplayButton` wired into the existing RecapScreen.
- `VolumeCategories` needs no scene object (RuntimeInitializeOnLoadMethod + sceneLoaded scan);
  it auto-registers the existing `MusicSource` / `AmbienceSource` / `SfxSource` by name.

---

## 4. Story state persistence (storyJson shape)

`StoryDirector.SerializeState()` writes the blob copied verbatim into
`SaveSystem.SaveData.storyJson` (filled on `SaveCaptured`, restored on `SaveApplied`):

```json
{
  "version": 1,
  "played":   ["SN_Act1_01", "SN_Act1_03", "..."],
  "flags":    ["mood_steady", "knows_backstairs", "..."],
  "boldness": 0.2, "trust": 0.3, "calm": 0.1,
  "asked": 12, "correct": 9,
  "dawnCatches": 0,
  "dawnActive": false,
  "endingId": ""
}
```

- New Game (`SaveSlotUI` NEW after confirm) → `StoryDirector.ResetForNewRun()` + `RunResults.Reset()`.
- Continue/LOAD → `SaveSystem.Restore` (deferred to scene load by SaveSystem itself) →
  director deserializes; the graph resumes from flags/played state.
- Fuse-box conversations close with `SaveSystem.Autosave()` so a conversation never
  half-survives a quit.

---

## 5. Risks & limitations (honest)

1. **Not run in Unity.** Compile syntax was verified by static review only (brace/paren
   balance, API signatures grepped against real sources, arg-count audit of every builder
   call). Expect at most trivial fixups on first batch compile.
2. **Real AudioMixer deferred (D15, as decided):** code-level category mixer only. Sources
   not matching the name convention register as SFX; AdaptiveMusic applies the Music/Master
   sliders itself. Per-source ducking/priority is not implemented.
3. **Chase state approximation:** `GameEvents` has no chase event, so AdaptiveMusic infers
   from catch recency / gate-powered / dawn (documented in code and D15 comment).
4. **Gameplay-side consequences are story-side only.** Choices change dialogue, stats,
   dialogue-skipping (the dawn shortcut) and endings — they do not move geometry or enemy
   FSM parameters (no contract for that). The "shortcut" is real (fewer input locks during
   the timed chase) but not a door.
5. **Settings page B layout:** second page uses two columns (6+5 rows) to fit the existing
   1120×1080 card; row buttons are 44×40 px — matching the existing page-A chrome but
   slightly under the 48 px touch guideline. Choice rows, slot rows and dialogue buttons
   are ≥ 58 px. A scrollable card is the follow-up if the team wants strict 48 px everywhere.
6. **Colourblind palette scope:** applies to all story-pass UI colours (dialogue, timer,
   save slots, loading). Meter/battery fills and world labels remain Core-owned (out of
   scope); their amber/blue bases are already red/green-free.
7. **ApplyLoadedState caveat (ENG-documented):** which fuse boxes/fragments are restored
   depends on scene object order, so story *trigger ordinals* may replay in a different
   physical order after a load. Counts/accuracy/flags are exact.
8. **Quiz answers before a load** are per-run (RunResults), not persisted; the recap after
   Continue shows post-load answers plus the director's saved accuracy counters for the
   ending formula.
9. **Arabic Shadow lines** lose the missing-word elision effect slightly (trailing dashes
   instead of hollow gaps) — accepted, noted in the bible.
10. **`BuildStoryBatch` opens and saves scenes** — the Lead should not have unsaved manual
    scene edits when running it (standard for this pipeline).

---

## 6. Draft AI_DEV_LOG.md entry (Lead: paste below the template)

```text
### 2026-10-30 — P3 narrative/UI/UX/audio: story system, dialogue UI, endings, save slots,
loading screen, accessibility rows, volume mixer, adaptive music — Abdelrhman Mohamed
- Tool used: Claude agent (Lead TD orchestration)
- What AI provided: Docs/Story/STORY_BIBLE.md and SCRIPT.md (44-node branching graph, 4
  endings per D13); Scripts/Story/* (StoryNode/StoryConfig/StoryEvents/StoryStrings/
  AccessibilityStore/StoryDirector/DialogueUIController); Scripts/Audio/VolumeCategories +
  AdaptiveMusic (D15 code mixer, layered procedural score); Scripts/UI/SaveSlotUI +
  LoadingScreen; additive settings rows (text speed, subtitle size, colourblind palette,
  auto-advance, skip hold, quality tier, 5 volume sliders); SceneBuilderStory.cs partial
  (asset content table + scene decoration).
- What I changed myself: verified tone against doc §2.3/§3 (no gore, hope-forward,
  mother only in her own written words); approved the calm-gated risky-choice rule and the
  knows_backstairs dawn shortcut as the consequence spine; confirmed the PlayerPrefs-only
  quality-tier row (no compile dependency on PerformanceManager); confirmed D15 code-level
  mixer wording; own the final story text.
- Teach-back check: pending — will walk the team through SO_StoryConfig thresholds and the
  storyJson shape before merge (script.md maps 1:1 to the built assets).
- Test result: not run — Lead verifies via batch (BuildAllBatch → BuildStoryBatch →
  ValidateBatch; grep logs for RESULT:|error CS|Exception).
```
