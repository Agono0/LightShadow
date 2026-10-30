# Engineering Agent Report — Sub-Agent 2 (ENGINEERING, GAMEPLAY & PERFORMANCE)

Date: 2026-10-30 · Branch: `feature/premium-pass` · Scope: P2 (ROADMAP) · Decisions applied: D4–D14, D16
Everything below was written as files only; no Unity process was run, no git command was run, per D10.

---

## 1. Summary

Delivered the full P2 engineering slice:

- **GameEvents** static event bus with the exact frozen contract (Section 4), raised from the
  nine existing gameplay methods, all guarded.
- **Input lock contract** (`PlayerController.ExternalInputLocked`) honoured by keyboard, the
  flashlight toggle key and the interact key; gravity/crouch/camera smoothing keep running.
  **GamepadInput** polls `Gamepad.current` through the Input System (D7: classic Input core
  untouched, `Active Input Handling` stays Both) and drives the same Move/Look/Sprint/Crouch
  methods the touch layer uses; Start opens the same pause the Esc key uses. Zero per-frame
  allocation.
- **PerformanceManager** (DontDestroyOnLoad singleton, self-bootstrapping via
  `RuntimeInitializeOnLoadMethod` and created by BootstrapLoader if absent): four tiers
  Low/Medium/High/Ultra with manual override (`PlayerPrefs "ls_qualityTier"`, -1 = auto) and
  platform auto-detection; tier applies by swapping the QualitySettings level (Mobile/PC
  URP asset — the SSAO on/off decision) then retuning the live URP asset (renderScale
  .65/.75/.9/1.0, shadowDistance 0/20/40/60, MSAA off/off/2x/2x, soft shadows
  off/off/on/on via reflection-guarded internal setter); D8 frame-rate policy (Windows
  120 FPS + vSync 0; Android display refresh clamped 60–120 with 60 fallback); dynamic
  resolution (camera flag + URP 17.4 core `DynamicResolutionHandler` per-tier min/max);
  one honest boot summary log line; `TierApplied` event; `SetTierOverride(int)` for the
  settings card.
- **ObjectPool** (generic, prefab-keyed, preload/Get/Release, no per-call allocation) +
  **PooledFx** facade with the two example wirings: a pooled flashlight flare on correct
  answers and a pooled whisper burst on Shadow chase stingers. Both are one-line additive
  hooks and both are kill-switchable with `PooledFx.Enabled = false`.
- **SaveSystem** with the exact frozen contract (Section 4): 3 JSON slots under
  `Application.persistentDataPath/lightshadow/saves/`, autosave on every checkpoint and on
  gate power, `Capture()`/`Restore()` (deferred restore across scene loads), session-scoped
  `playSeconds`, `SaveCaptured`/`SaveApplied` hooks for the Narrative blob.
  `GameManager.ApplyLoadedState(SaveData)` restores state after scene load (Section 5).
- **Cinemachine 3.1.4** added to the manifest; scripted shots only (gameplay camera stays the
  existing FP rig). Editor partial `SceneBuilderSystems.cs` builds three dormant
  CinemachineCamera rigs (catch / dawn / gate) with `CinemachinePositionComposer` damping +
  a *disabled* `CinemachineBrain` on the player camera; runtime `CinematicDirector.PlayShot(id)`
  positions the rig for the live moment, raises its priority, holds, then hands the camera
  back. The whole runtime path is reflection-gated: if Cinemachine were ever missing the
  game logs a warning and keeps playing. Catch coroutine, DawnRiser and `GameManager.Escape`
  call it additively; the existing CameraShake is untouched.
- **Perf hygiene audit** of owned files (Section 1.1): two fixes applied, rest verified clean.
- **Docs/PERFORMANCE.md** filled: precise targets, tier table, methodology, MEASURED-BY-LEAD
  rows, honest D8 statement, D16 (no Adaptive Performance) statement.

### 1.1 Perf hygiene audit results (owned files)

| Finding | Fix |
| --- | --- |
| `GameManager.GetMeter()` ran `GetComponentInChildren` on every quiz answer/caught penalty | Cached in a private field; behaviour identical, one lookup per scene |
| `FlashlightBattery.Update` invoked the `OnBatteryChanged` UnityEvent every frame while lit | Deduplicated: invoke only when the fraction moved > 0.5 % or hit 0 |
| `PlayerController`/`BrightIdeaMeter`/`DawnRiser`/`EnemyBehaviour` hot paths | Verified: no `new`, no LINQ, no foreach-on-arrays, components cached in Awake/Start already |
| New systems | All zero-alloc per frame by construction (ObjectPool reuses instances, GamepadInput reuses Vector2 fields and polls only, PerformanceManager works only on boot/tier-change) |

---

## 2. Files created / modified

### New files

| File | Contents |
| --- | --- |
| `Assets/_Project/Scripts/Core/GameEvents.cs` | Static event bus, frozen contract |
| `Assets/_Project/Scripts/Core/SaveSystem.cs` | 3-slot JSON save system, Capture/Restore/Autosave, session timer |
| `Assets/_Project/Scripts/Player/GamepadInput.cs` | Gamepad polling layer (Input System), self-attaches to the player rig |
| `Assets/_Project/Scripts/Systems/PerformanceManager.cs` | Quality tiers, auto-detect, frame-rate policy, dynamic resolution, boot log |
| `Assets/_Project/Scripts/Systems/ObjectPool.cs` | Generic prefab-keyed component pool |
| `Assets/_Project/Scripts/Systems/PooledFx.cs` | Effect facade over the pool (flash + whisper templates, kill switch) |
| `Assets/_Project/Scripts/Systems/PooledFlashEffect.cs` | Pooled unshadowed point-light flare |
| `Assets/_Project/Scripts/Systems/PooledWhisperBurst.cs` | Pooled spatial whisper (clip synthesised once at runtime) |
| `Assets/_Project/Scripts/Systems/CinematicDirector.cs` | Reflection-gated scripted shots (PlayShot, holds, handback) |
| `Assets/_Project/Scripts/Systems/CinematicRunner.cs` | Coroutine host + Cinemachine reflection helpers |
| `Assets/_Project/Scripts/Tools/Editor/SceneBuilderSystems.cs` | NEW partial of `LightShadow.Tools.SceneBuilder`: `ConfigureQualityForBuild()` (RP_Low/Medium/High/Ultra variants via CopyAsset + SerializedObject), `BuildCinematicRigs()` (3 dormant CM cams + disabled brain in 02_School), menu item **LightShadow/8. Quality Tiers + Cinematic Rigs** |
| `Docs/AGENT_REPORT_ENG.md` | This report |

Note: Unity generates the new `.meta` files (and `Scripts/Systems.meta`, `GamepadInput.cs.meta`, etc.)
on the Lead's first editor run — commit them alongside.

### Modified files (every edit is additive; no existing member renamed/removed)

| File | Anchor point → added code |
| --- | --- |
| `Core/GameManager.cs` | usings block → `+LightShadow.Gameplay`, `+LightShadow.Systems`. `GetMeter()` → cached `cachedMeter` field (same signature). `ApplyQuizResult` end → `GameEvents.RaiseQuestionAnswered(correct);`. `OnFuseRestored` end → `RaiseFuseRestored(FusesRestored, FuseTarget);`. `OnFragmentCollected` end → `RaiseFragmentCollected(FragmentsCollected, FragmentTarget);`. `OnGatePowered` end → `RaiseGatePowered(); SaveSystem.Autosave();`. `Escape()` after `escaped = true;` → `CinematicDirector.PlayShot(CinematicDirector.ShotGate); GameEvents.RaiseEscapeStarted();`. `CaughtPlayer` after `respawning = true;` → `GameEvents.RaisePlayerCaught();`. After `WakeAtCheckpoint` → NEW `public void ApplyLoadedState(SaveSystem.SaveData data)`. |
| `Core/CheckpointManager.cs` | `SetCheckpoint` end → `GameEvents.RaiseCheckpointReached(...); SaveSystem.Autosave();` |
| `Core/BootstrapLoader.cs` | `Awake` end → `Systems.PerformanceManager.EnsureCreated();` (after `GameSettings.LoadAndApply()` so the tier base applies first and settings-card fine controls stay on top) |
| `Player/PlayerController.cs` | class top → `public static bool ExternalInputLocked;`. `Awake` end → `GamepadInput.EnsureOn(gameObject);`. `Update()` → lock branch zeroes move/sprint and skips `ReadKeyboardAndMouse()`, then `UpdateCrouch()`/`UpdateMovement()` always run (gravity + pose kept) |
| `Player/BrightIdeaMeter.cs` | `AddCorrect` end → pooled flare `Systems.PooledFx.SpawnFlash(...)`. `Notify()` → `GameEvents.RaiseMeterChanged(n);` after the existing UnityEvent |
| `Player/FlashlightBattery.cs` | fields → `lastBatteryNotified`. `Update` → F key gated by `ExternalInputLocked`; battery HUD event dedup. After `AddBattery` → NEW `public void RestoreBattery(float fraction, bool on)` |
| `Player/InteractionRaycaster.cs` | `Update` → E key gated by `ExternalInputLocked` |
| `Gameplay/DawnRiser.cs` | `OnEnable` end → `Core.GameEvents.RaiseDawnStarted(); Systems.CinematicDirector.PlayShot(Systems.CinematicDirector.ShotDawn);` |
| `Gameplay/FuseBox.cs` | after `Interact` → NEW `public void RestoreFromSave()` (visuals only, no count/message/checkpoint) |
| `Gameplay/KnowledgeFragment.cs` | after `CollectSequence` → NEW `public void RestoreFromSave()` (hide + disable collider, no count) |
| `Gameplay/ExitGate.cs` | after `Power()` → NEW `public void PowerFromSave()` (state only, no sound/count, leaf lands open) |
| `Enemy/EnemyBehaviour.cs` | after `StartCoroutine(CatchSequence());` → `Systems.CinematicDirector.PlayShot(ShotCatch);` |
| `Enemy/ShadowAudio.cs` | chase/repelled stinger block → `Systems.PooledFx.SpawnWhisper(transform.position);` |
| `Packages/manifest.json` | + `"com.unity.cinemachine": "3.1.4",` (alphabetical slot after ai.navigation) |
| `Docs/PERFORMANCE.md` | Targets expanded precisely, Methodology added, MEASURED BY LEAD table, D8/D16 honesty notes |

NOT touched (per ownership rules): any `.unity/.prefab/.asset/.meta` YAML, `Scripts/UI/`,
`Scripts/Story/`, `Scripts/Art/`, other Tools/Editor files, `ProjectSettings/*`,
`ASSET_LICENSES.md`, `AI_DEV_LOG.md` (draft entry below for the Lead to paste).

---

## 3. Integration instructions for the Lead (in order)

1. **Open Unity once normally (or run ValidateBatch)** so the Package Manager resolves
   Cinemachine 3.1.4 and generates all new `.meta` files.
2. **Compile check:** `ValidateBatch` (grep `error CS`). All Cinemachine-typed code lives in
   the editor partial (compiles only when the package resolves) and in reflection at runtime,
   so a manifest problem surfaces as one file's compile error, not scattered ones.
3. **One-liner A — quality asset variants.** Inside `BuildAllBatch()` (SceneBuilder.cs,
   after `string summary = BuildAll();`) add:
   `LightShadow.Tools.SceneBuilder.ConfigureQualityForBuild();`
   (Idempotent: deletes+recopies the four `Assets/Settings/RP_*.asset` variants each run.)
4. **One-liner B — cinematic rigs.** Inside `BuildSchoolScene(...)` (SceneBuilderScenes.cs,
   just before `SaveScene(scene, SchoolPath);`) add:
   `SceneBuilder.BuildCinematicRigs();`
   Both one-liners are OPTIONAL for correctness: menu item **LightShadow ▸ 8. Quality Tiers +
   Cinematic Rigs** runs both standalone, and `CinematicDirector` recreates missing rigs at
   runtime through reflection, so the game works either way.
5. **Batch verify + build:**
   ```bash
   U="C:/Program Files/Unity/Hub/Editor/6000.4.0f1/Editor/Unity.exe"
   "$U" -batchmode -quit -nographics -projectPath . -executeMethod LightShadow.Tools.SceneBuilder.ValidateBatch -logFile Logs/validate.log
   "$U" -batchmode -quit -nographics -projectPath . -executeMethod LightShadow.Tools.SceneBuilder.BuildAllBatch -logFile Logs/build.log
   "$U" -batchmode -quit -nographics -projectPath . -executeMethod LightShadow.Tools.SceneBuilder.BuildWindowsBatch -logFile Logs/win.log
   "$U" -batchmode -quit -nographics -projectPath . -executeMethod LightShadow.Tools.SceneBuilder.BuildAndroidBatch -logFile Logs/android.log
   grep -E "RESULT:|error CS|Exception" Logs/*.log
   ```
6. **Manifest diff** (single line):
   ```diff
       "com.unity.ai.navigation": "2.0.16",
   +   "com.unity.cinemachine": "3.1.4",
       "com.unity.collab-proxy": "2.11.4",
   ```
   3.1.4 is the verified latest 3.1.x line for Unity 6 (Unity's own changelog for
   `com.unity.cinemachine@3.1` lists 3.1.4 released 2025-06-10 for Unity 6; the line continues
   to 3.1.7). If resolution ever fails on your setup, downgrade the pin to `3.1.2` — no code
   change is needed (runtime Cinemachine use is reflection-only).
7. **For the Narrative agent (Sub-Agent 3):**
   - Dialogue lock: set `LightShadow.Player.PlayerController.ExternalInputLocked = true/false`
     (pause stays available during dialogue by design).
   - Story blob: subscribe `LightShadow.Core.SaveSystem.SaveCaptured` → write `data.storyJson`;
     subscribe `SaveSystem.SaveApplied` → read `data.storyJson` back. Save/load UI uses
     `CurrentSlot`, `SaveToSlot/LoadFromSlot/HasSave/DeleteSlot/NewestSlot`, and for tier UI
     `PerformanceManager.TierLabel/SetTierOverride` + `TierApplied`.

---

## 4. Contract confirmation (frozen, as specified)

**GameEvents** — `Scripts/Core/GameEvents.cs` implements exactly:

```csharp
public static class GameEvents {
    public static event System.Action<int,int> FragmentCollected; // (collected, total 6)
    public static event System.Action<int,int> FuseRestored;      // (restored, total 3)
    public static event System.Action GatePowered;
    public static event System.Action PlayerCaught;
    public static event System.Action<string> CheckpointReached;  // lit-zone display name
    public static event System.Action<bool> QuestionAnswered;     // correct?
    public static event System.Action<float> MeterChanged;        // normalized 0-1
    public static event System.Action DawnStarted;
    public static event System.Action EscapeStarted;
    public static void RaiseFragmentCollected(int c,int t);       // + one Raise* per event
}
```

Raise sites (all null-conditional via `?.Invoke`): `GameManager.ApplyQuizResult`,
`OnFuseRestored`, `OnFragmentCollected`, `OnGatePowered`, `CaughtPlayer`, `Escape`;
`CheckpointManager.SetCheckpoint` → `CheckpointReached` (payload = the chalk line, or
"Checkpoint reached." when empty); `BrightIdeaMeter.Notify` → `MeterChanged` (fires only on
0.1 % change, same dedup the HUD already uses); `DawnRiser.OnEnable` → `DawnStarted`.

**SaveSystem** — `Scripts/Core/SaveSystem.cs` implements exactly:
`SlotCount = 3`; `CurrentSlot { get; set; }` (default 0); nested `[Serializable] SaveData`
with fields `version=1, sceneName, position, yaw, meterValue, batteryFraction, flashlightOn,
fusesRestored, fragmentsCollected, gatePowered, storyJson="", playSeconds, savedAtUtc`;
`SaveToSlot(int, SaveData)` (JSON → `Application.persistentDataPath/lightshadow/saves/slot{n}.json`),
`LoadFromSlot(int)`, `HasSave(int)`, `DeleteSlot(int)`,
`NewestSlot()` (lexicographic max of ISO-8601 `savedAtUtc`, -1 when none). Extras beyond the
contract (additive only): `Capture()`, `Autosave()`, `Restore(SaveData)`,
`SaveCaptured`/`SaveApplied` events, session `playSeconds` clock.

---

## 5. What restores from a save — and what does not (honest)

Restored by `GameManager.ApplyLoadedState` (runs after the saved scene loads):

- fuse/fragment/gate **counts** (drives quiz difficulty via `CurrentDifficulty`), gate +
  dawn-chase state (gate leaf open, `dawnChaseRoot` active, Shadow aggressive), meter value,
  battery fraction, flashlight on/off, player position + yaw, HUD objectives refreshed.
- Pickup **mirroring**: the first N `FuseBox`/`KnowledgeFragment` objects found in the scene
  are set to restored/collected. **Which** boxes/fragments may differ from the original run —
  scene object order is not stored in the save. Counts and difficulty are exact; specific
  pickup identity is not. (A stable pickup-ID list would be a D12 follow-up if needed.)

Not restored, on purpose, stated openly:

- **Quiz answer history** (`answers` / recap) — per-run study data, not saved (the ending
  scene reads it from the live run only).
- **The Shadow's exact position/FSM state** — it restarts at patrol; saves happen at lit-zone
  checkpoints, so this is a feature, not a loss.
- **`storyJson`** — copied verbatim by the engine; Narrative applies it via `SaveApplied`.
- **`playSeconds`** counts the current play session, not a lifetime total.
- Autosave fires on checkpoint entry and gate power only — there is no timer-based autosave
  (deliberate: dying players should not be able to save-scum the Shadow's memory).

---

## 6. Risks and fallbacks chosen

| Risk | Chosen fallback |
| --- | --- |
| Cinemachine 3.1.4 headless-untestable | Runtime side uses **zero** compile-time Cinemachine references — everything goes through reflection (`CinematicRunner.Cine`); missing package/changed API ⇒ one warning, gameplay unaffected. Editor rigs use real API in ONE editor file; a compile break there is visible immediately and isolated. Manifest pin can drop to 3.1.2 without code changes. |
| Camera handback after a shot | The disabled-`CinemachineBrain` approach: brain is OFF between shots so the validated FP rig owns the camera 100 % of the time; shots are hold-N-seconds accents inside fades, exit is a hard cut back to the rig. Existing `CameraShake` untouched (shake plays before the fade; shot completes after it). |
| `softShadowsSupported` has an internal setter in URP 17.4 (verified in package source) | Runtime flip via reflection, caught + warned; the editor variants set it properly via SerializedObject. `renderScale/msaaSampleCount/shadowDistance/shadowCascadeCount` have public setters (verified against the installed URP 17.4 source). |
| URP 17.4 removed the RP-asset shadow-distance-fade / dynamic-res fields the brief assumed | Fade left at asset defaults (documented); dynamic-res min/max goes through the core `DynamicResolutionHandler` instead (public API, guarded). Editor variants therefore only tune renderScale/MSAA/shadowDistance/softShadows. |
| SSAO runtime toggle not scriptable | Encoded by asset choice: Low/Med = Mobile renderer (no SSAO), High/Ultra = PC renderer (SSAO), exactly as the brief permits. |
| GameSettings vs PerformanceManager fighting over the URP asset | Ordering: bootstrap applies GameSettings fine controls first, then the tier (base); user's settings-card actions afterwards win until the next boot. Documented in both files. |
| `QualitySettings.SetQualityLevel(..., true)` at boot | Only swaps between the two existing named levels ("Mobile"/"PC"); name-matched, so reordering levels is safe; if names are missing the level is left alone. |
| EventSystem with Input System package present | `EnsureEventSystem()` still uses `StandaloneInputModule` and `Active Input Handling = Both` (verified: `activeInputHandler: 2`), which is a valid combination — uGUI clicks keep working; gamepad polling never needs the EventSystem. If a later switch to `InputSystemUIInputModule` happens, it is a SceneBuilder one-liner, not a code change. |
| New-file `.meta` generation | Left to Unity (never hand-written); the Lead commits them on the first editor run. |
| Pooled FX double-lighting/sound overlap | Flare is unshadowed, 0.5 s, tiny range; whisper is one-shot spatial; both behind `PooledFx.Enabled` kill switch; hook lines are single-line and trivially removable. |

---

## 7. Draft AI_DEV_LOG.md entry

```text
### 2026-10-30 — P2 engineering: event bus, gamepad, quality tiers, pooling, saves, Cinemachine shots — Sub-Agent 2 (ENGINEERING)
- Tool used: Claude agent (Lead TD orchestration)
- What AI provided: first drafts of GameEvents, SaveSystem, PerformanceManager, ObjectPool,
  GamepadInput, CinematicDirector, the SceneBuilderSystems editor partial, and every additive
  hook listed in the engineering report (Docs/AGENT_REPORT_ENG.md §2).
- What I changed myself: verified every API against the installed URP 17.4 package source
  (public setters, internal soft-shadow setter, removed shadow-fade field) and against
  ProjectSettings (quality level names, activeInputHandler: 2) before writing; kept every
  existing member untouched and additive-only; chose reflection gating for all runtime
  Cinemachine use so the game cannot hard-fail; moved dynamic-res min/max from the (removed)
  asset fields to the core DynamicResolutionHandler; documented honestly what saves restore
  and what does not (pickup identity vs counts).
- Teach-back check: pending — to be walked through with a teammate before merge (per doc §8.3
  rule 1): how the tier system swaps the Mobile/PC URP asset and why SSAO rides the asset
  swap instead of a runtime toggle.
- Test result: not run — Lead verifies via batch (ValidateBatch / BuildAllBatch / builds per
  Docs/PERFORMANCE.md Methodology).
```
