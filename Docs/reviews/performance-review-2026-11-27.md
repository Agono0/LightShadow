# Performance review (report only), 2026-11-27

Package 3 of `Docs/YUSUF_WORK_PLAN.md`. Written by an AI agent working for Yusuf. Nothing here is applied. No production file was changed.

## How to read this

- **[M] Measured**: counted directly from committed files (scene YAML, `.meta` files, source, `ffprobe` durations).
- **[E] Estimate**: computed or inferred. Not profiled. Unity was not available, so no frame time, draw-call or memory number in this file came from a running build.
- The committed `02_School.unity` comes from an older commit. CI regenerates scenes, so the scene counts may differ from a fresh build. Treat them as indicative.
- The only real measurements on record are the CI build sizes in `Docs/PERFORMANCE.md`. On-device FPS is still pending there.

## Prioritized findings

### 1. Three long music and ambience files load fully into RAM (high impact, memory) [M sizes, E RAM]

`Docs/ASSET_MANIFEST.md` specifies Vorbis streaming for music and ambience. These files are imported as DecompressOnLoad, so they sit in RAM as PCM. PCM size is duration x sample rate x channels x 2 bytes, computed from `ffprobe` durations.

| File | On disk | Length | Est. PCM in RAM |
|---|---|---|---|
| `Audio/Ambience/MUS_Bed_LongNoteOne.mp3` | 17.8 MB | 440 s | about 77.6 MB |
| `Audio/Music/MUS_Ending_HopeGold.mp3` | n/a | 221 s | about 42.5 MB |
| `Audio/Music/MUS_Ending_DawnlessGrey.mp3` | n/a | 239 s | about 42.2 MB |
| `Audio/Ambience/AMB_SchoolNight.mp3` | n/a | 68.6 s | about 12.1 MB |

About 175 MB in total [E]. On a mid-range phone this is likely the largest single memory cost, and it is paid at scene load.
**Recommend:** load type Streaming, compression Vorbis, quality about 0.6 to 0.7 (matches the manifest) for all four. Streaming is already correct for `AMB_DawnBirds`, `AMB_SchoolDreadA/B`, `MUS_ChasePulse`, `MUS_Drone_Library`, `MUS_Ending_HopeAlt`, `MUS_MemoryTheme`. The two ending tracks play in one scene, so also consider whether both need to exist at once.

### 2. `SFX_Whisper_Burst.wav` is a 192 s file used as a one-shot (high impact, size and memory) [M]

33.9 MB on disk, ADPCM mono, DecompressOnLoad, about 17 MB PCM in RAM [E]. It is 192 s long. The manifest itself notes "words may need trimming".
**Recommend:** trim to the real burst (a few seconds) in the asset lane, or set Streaming if it must stay long. This alone is about 22% of the Android arm64 build (88 MB, CI ledger in `PERFORMANCE.md`) if the file is stored as-is [E].

### 3. Shadowed point lights in the school scene (high impact, GPU) [M counts, E cost]

- `02_School.unity`: 27 lights. 23 are point lights with soft shadows, 3 point lights without, 1 directional moon with soft shadows [M].
- Source: `Scripts/Tools/Editor/SceneBuilder.cs` `AddLamp` (about line 814) calls `CreatePointLight` (lines 797-812), which sets `LightShadows.Soft` whenever `shadows` is true. The default is true, and 23 `AddLamp` calls in `SceneBuilderScenes.cs` use the default (lines 94-95, 567-628). Only lines 174-183 pass `false`.
- URP asset: PC additional-light shadow atlas 4096, per-object light limit 8. Mobile atlas 2048, limit 4, soft shadows off [M].
- A point light needs 6 shadow slices [E]. 23 point lights need up to 138 slices, far more than the atlas holds at 256 to 1024 px tiers. URP will drop or downscale shadows for lights that do not fit. The visible result is lamp shadows popping in and out, and GPU time spent on shadow passes that are not shown [E].
- Each lamp also carries a `LightFlicker` (`Scripts/Core/LightFlicker.cs:25-31`), which changes intensity every frame. That invalidates any caching of that light's shadow data [E].

**Recommend (values to try, then measure):**
- Keep shadows on only the 3 to 4 lamps nearest story beats (the two `-30` z entrance lamps at lines 567-568 are good candidates) and set `shadows=false` for the rest.
- Mobile tier: all point-light shadows off, keep the flashlight spot and the moon.
- PC High/Ultra: at most 6 shadowed point lights in view.
- Atlas 4096 PC is fine once the count drops. On mobile keep 2048.

### 4. Always-on soft-shadow flashlight spot (medium impact, GPU) [M]

`Scripts/Tools/Editor/SceneBuilderPrefabs.cs:101-110`: spot light, range 11, angle 62, `LightShadows.Soft`. It is probably the most visible light in the game, so the shadow is worth keeping on PC. On mobile the URP asset disables soft shadows, so it falls back to hard shadows, which is fine.
**Recommend:** keep as is on PC. On the Low tier consider `LightShadows.None` if the device misses 30 FPS [E].

### 5. Shadow casters: 328 of 346 renderers cast shadows (medium impact) [M]

In `02_School.unity`, 328 of 346 renderers cast shadows, and nearly all are cubes (320 cubes, 5 spheres, 3 cylinders; 12 tris per cube, about 3,840 tris) [M]. Geometry cost is small. The cost is the number of draw calls and shadow-pass submissions: with several shadowed lights each caster is drawn again per light that sees it [E]. 33 unique materials and 117 prefab instances across 33 prefabs give some batching room, but with SRP Batcher the main lever is fewer shadow-casting objects.
**Recommend:** set casting to Off for small props and for faces not visible to any light (wall back faces, ceiling trim). Do this in the builder after finding 3 is applied, and re-measure before more changes.

### 6. Per-frame allocations in `Update` (low to medium impact, GC) [M locations, E cost]

A scan of `Update`, `LateUpdate`, `FixedUpdate` and `OnGUI` bodies in `Scripts/**` (excluding editor tools) found very little. No `new` of reference types, LINQ, `Find*`, `Camera.main` or `SendMessage` in any per-frame body. The hits:

| Where | What | Cost | Recommend |
|---|---|---|---|
| `UI/QuizUIController.cs:60` | `timerText.text = Localization.TimerLabel(timeLeft)` every frame while a quiz is open | A string and a text-mesh rebuild per frame [E] | Update only when the displayed whole second changes (`Mathf.CeilToInt(timeLeft)` differs from last) |
| `UI/QuizUIController.cs:68` | `timerText.font = Localization.CurrentFont` every frame | Re-assigning the same font can mark the text dirty [E] | Assign only when the language changes |
| `UI/QuizUIController.cs:63-65` | `new Color(...)` | Struct, no allocation. Fine. | None |
| `Player/InteractionRaycaster.cs:56` | `"[E] " + current.Prompt` every frame while looking at an interactable | One string per frame [E] | Cache the formatted prompt and rebuild only when `current` changes. Also `hud.SetPrompt` runs every frame, so make it skip equal strings |
| `UI/FpsCounter.cs:61` | String concat plus `ToString("0.0")` | Runs only each `UpdateInterval`, not per frame. Debug overlay. | None |
| `Player/GamepadInput.cs:114` | `GetComponent<FlashlightBattery>()` | Only on a button press and only while null. Fine. | None |
| `Story/DialogueUIController.cs:369,427` | `Localization.Present(fullText)` | Runs on skip, not per frame. Fine. | None |
| `Core/LightFlicker.cs:25-31` | `Mathf.PerlinNoise` and a light intensity write per lamp per frame | 23+ lamps, cheap math, but see finding 3 | Update at 20 to 30 Hz for lamps far from the player |

Other lookups found (not in per-frame bodies): `FindFirstObjectByType<ExitGate>` at `Core/GameManager.cs:164,402` and `Systems/CinematicDirector.cs:289`; `GameObject.Find` at `Systems/CinematicDirector.cs:236`. They run on events, so low risk.
Physics queries per frame are all single, non-allocating calls: `Player/InteractionRaycaster.cs:70` (Raycast), `Enemy/EnemySight.cs:40` and `Audio/AudioOcclusionProbe.cs:144` (Linecast). Note: the raycaster runs every frame; every 2 to 3 frames would be enough for a prompt [E].

### 7. Small SFX import settings differ from the manifest (low impact) [M]

12 small SFX ogg files are Vorbis, not the manifest's ADPCM mono. `SFX_ShadowMoan_2.wav` is Vorbis and non-mono against the spec. 47 others match. The sizes are small, so this is cleanup, not a fix for a problem.
**Recommend:** align them with the manifest, or update the manifest if Vorbis was a choice.

### 8. Texture import settings look sound (no action) [M]

75 textures under `Textures/Art` (273 MB on disk). 71 of 75 use compression, mipmaps on, and an Android max size override of 1024 (PC 2048). 4 files have Android max 2048 with compression and no mipmaps. None is marked readable. The largest sources are normal maps around 9 MB as JPG on disk; the import size is what matters at runtime, not file size.
**Recommend:** check the 4 exceptions (no mipmaps on a 3D surface causes shimmer and wastes bandwidth). The name of each is not listed here. They are in `Textures/Art` and can be found by searching `.meta` files for `enableMipMap: 0`. Texture is the asset lane's area, so this is a note, not a change.

### 9. Audio on disk is large relative to the build (context) [M]

Audio on disk is about 154 MB: SFX 37, Ambience 42, Music 65, VO 11 (154 VO ogg files, 77 AR and 77 EN, not listed in the manifest). Builds are Android arm64 88 MB and Windows 152 MB (CI ledger in `PERFORMANCE.md`). Fixing findings 1 and 2 is the biggest size lever available.

## Suggested order of work

1. Findings 1 and 2: asset import settings and one trim. Biggest memory gain, no code.
2. Finding 3: lamp shadows, in the scene builder. Biggest GPU gain.
3. Finding 6: the quiz timer and prompt string, both small.
4. Profile on a real mid-range Android before touching 4, 5 and 7. Use the methodology in `Docs/PERFORMANCE.md`.

## Not done

- No profiler, frame-time, draw-call or memory capture. Every cost marked [E] is a hypothesis to verify.
- Generated geometry in the other builders was not counted. Only `02_School.unity` was parsed. `01_MainMenu` has 2 shadowed point lights; `03_Ending` has 4 point lights and 1 directional, all with soft shadows [M].
- Shader cost and post-processing were not reviewed.
