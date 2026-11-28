# Unity pass report (2026-11-28)

The first session that opened LIGHT SHADOW in the real editor (Unity 6000.4.0f1, Windows 11, DX12) and
played it. Branch `fix/unity-pass`, stacked on `feat/followup-restore-story-player` (PR #131).
Everything below marked **seen** was observed in the editor (Play mode, Game view 2560x1440) or in a
batch log. Anything marked **not verified** was not seen.

How I played: the editor GUI in Play mode, driven by real keyboard and mouse input sent to the Unity
window (F torch, I inventory, W walk, Esc, clicks), plus a new editor-only remote
(`Scripts/Tools/Editor/PlaytestRemote.cs`) that reads commands from `Temp/playtest_cmd.txt`
(teleport, call a method, read a field, take a Game view screenshot, dump lights/audio/UI). The remote
needs no scene change and ships in no build. Usage is at the top of the file.

## 1. Validator and tests

| Check | Result |
| --- | --- |
| `BuildAllBatch` | exit 0 (seen) |
| `ValidateBatch` | **RESULT: PASS** after fixes (it failed on arrival, see below) |
| EditMode tests | **573 / 573 passed** (was 569 / 570 on arrival) |
| Windows / Android player builds | **not run** (session ended at the owner's request; see section 7) |

Validator failures found on arrival, both validator/builder bugs, both fixed:
- The DawnChase lamp exemption checked only the light's direct parent, but `AddLamp` nests it as
  `DawnChase/Lamp/Light`, so all four dawn lamps failed the 1.0 intensity cap.
- "expected 6 fragments, found 7": the infirmary fragment from #26 is a seventh, spare one. The
  validator now asks for at least `GameManager.FragmentTarget`. The "every fragment is found" message
  played again on the seventh pickup; it now plays only on the pickup that reaches 6.

Test failure on arrival: `PlayerModelImportTests.ModelsImportWithMarkers` (torch Muzzle missing),
explained in section 2. The relaxed hand-grip test had been **passing without checking anything**: it
returned early because no marker existed.

## 2. Model import findings (Models/Hero OBJs)

Seen with `PlaytestRemote.ModelReportBatch`:
- **Unity's OBJ importer ignores `o Name` lines.** Each file imported as ONE mesh named `default`.
  None of the four models had a `Grip` or `Muzzle` child, and the 8 mm marker cubes rendered as part
  of the visible mesh (the Muzzle cube stuck out in front of the lens).
- Unity splits OBJs only on `g` group lines. Each `o X` line now has a matching `g X` line (the only
  edit to the files). After that, every part is its own child: torch `Grip` (0,0,0), `Muzzle`
  (0,0,0.200); hand grips Unity x = -0.034 (right) / +0.034 (left), z 0.050. **Unity mirrors X**, as
  the modeller said. Child pivots are all identity at the origin, so reading markers from renderer
  bounds (as the code does) is correct.
- Second bug: `MarkerInModel` zeroes its `out` value on failure, and the builder assigned the
  spec default *before* the call. With no markers the beam therefore started at the **fist**, not the
  lens. Defaults are now applied after the lookup; the hand grip falls back to the measured spec point.
- The OBJ importer then wrote one junk `.mat` per group. New `PlayerRigModelImport` postprocessor
  imports the rig OBJs with no materials (the builder restyles them with the palette anyway).
- In first person the fist wraps the torch handle (screenshot 04/05). With the torch light kept off the
  player layer, the hand reads as a dark silhouette unless a lamp is near; that matches the reference.
- Normals and UVs import (vertex, normal and uv counts match). The body is ShadowsOnly. Not checked in
  depth: z-fighting at the cuff, where the torch tail enters the sleeve by about 2 cm.
- **For Ziad:** make `generate_models.py` write a `g <name>` line after every `o <name>` line, or the
  next export breaks again. `PlayerModelImportTests` now fails loudly if that happens.

## 3. Per-issue table

| # | Issue | Real? | Fixed? | Evidence / notes |
| --- | --- | --- | --- | --- |
| 1 | Dark map lit grey everywhere | **Real** | Fixed | Default reflection source "Skybox" with no skybox + never-baked probes lit every Lit surface. Mean frame luma 63.5 -> 36.0 with reflections off, ~29 with lamps off too. School now has no environment reflections; validator guards it. |
| 2 | Torch beam (owner: circular, wide, warm, big radius; reference video) | Tuned | Done | Amber spot 100/55 degrees, generated round cookie (`T_TorchCookieRound`), range 8..22 m, intensity 8..20 across the meter, light off the player layer (it burned the torch head white). Screens 04/05. |
| 3 | Big white wedge in front of the camera | **Real** | Fixed | Additive "haze cone" was rotated to point straight up, ignored the torch switch, and from first person drew a solid wedge. Removed (reference has none). Glowing lens added instead (`Mat_TorchLens`, shown only while lit). |
| 4 | Dust motes drawn as squares | **Real** | Fixed | URP 17 particle `_Blend` 1 is Premultiply, not Additive (2); beam dust also used the Built-in `Default-ParticleSystem` material. |
| 5 | Empty dark bar across the middle of the screen | **Real** | Fixed | `CaptionUI` put Image + Text on one GameObject (one Graphic allowed): NRE at CaptionUI.cs:71 in Awake, the first row stayed visible empty, **no caption ever showed**. Contrib/Abdelraouf. |
| 6 | Inventory does not list items | Not reproduced | n/a | Key and medkit listed (screen 06). Battery is never an item (goes straight into the torch) - design question. |
| 6b | Inventory detail pane / Use / Close cut off | **Real** | Fixed | Left-edge x values on centre-anchored rects pushed them past the right edge. Contrib/Abdelraouf. |
| 7 | Question after close-up; cursor | Cursor OK, **question bug real** | Fixed | Close relocks and hides the cursor (seen). But the **first question of every session vanished**: the quiz controller sits on its panel, which ships inactive, so its Awake ran inside `ShowQuestion` and hid it again. The fragment stayed "running" forever, never counted and could not be picked again. |
| 8 | Chapter-1 voice-over silent | **Real (two bugs)** | Fixed | (a) `BuildAll` never ran the story pass: no StoryDirector, no dialogue UI, **no story at all** in generated scenes (also on main). Now runs in BuildAll; validator requires it. (b) VO imports with preload off; `AudioSource.Play` on a still-loading clip did nothing. `PlayVoice` now waits for the data. Seen: `VO_SN_Act1_01` plays its full 7.4 s. |
| 9 | VO typing animation (owner) | - | Done | Existing typewriter now paces to the voice clip length (tested). Screen 10. |
| 10 | Question fade-in (owner) | - | Done | Quiz card eases in, `GameConfig.questionFadeSeconds`. Not captured on screen. |
| 11 | Menu/quiz textures unreadable, "stunning glass" UI (owner) | **Real** | Done (most screens) | Old button art put bracket caps over quiz answers ("3.=42", screen 08). New generated horror glass kit (`T_UI_Glass_Panel/Button`) on menus, quiz, pause, saves, story choices. Screens 09/10. **Still old:** dialogue box background (brown), Contrib inventory window, caption rows, puzzle panels drawn with their own colours. |
| 12 | Dialogue text ran under AUTO-ADVANCE | **Real** | Fixed | Body/timer width reduced to stop before the buttons. |
| 13 | Footsteps silent | Code path OK in editor | No change | With move input, steps play through the SFX group at about 0.3-0.45 scale (seen via `PlayStep`). If still inaudible on a device, check mix levels. Ambience is 0 when calm **by design** (`DangerModel.Mix`). |
| 14 | RP tier GUID churn | **Real** | Fixed | Every BuildAll deleted and re-copied `RP_Low/Medium/High/Ultra`, giving new GUIDs. Now refreshed in place. Phantom "FilmGrain MISSING" log removed (URP 17 has no separate shader). |
| 15 | "Android" build profile shows as **Web** in Build Profiles | **Real (seen in UI)** | **Not fixed** | `Assets/Settings/Build Profiles/Android.asset` has `m_BuildTarget: 13` but `m_PlatformId 84a3bb9e...`; the editor shows it as a Web profile. `BuildProfileTool.AndroidPlatformId` is probably the WebGL GUID. Find the real Android platform GUID in 6000.4 (create an Android profile in the Build Profiles window and read its asset) and fix the tool. Android builds may still work (they use BuildTarget), unverified. |
| 16 | Point-light shadow atlas | Real (warning) | Not fixed | Console spam: "Reduced additional punctual light shadows resolution by 2 to make 18 shadow maps fit in the 4096 atlas". Lamps with soft shadows each cost 6 maps. Turn shadows off on minor lamps or raise the atlas on PC. |
| 17 | Fragment close-up shows no 3D item | Real | Not fixed | `InspectViewController.ShowFrom` takes the fragment's first MeshFilter (old greybox visual?), nothing is visible. Narration subtitle also overlaps the CLOSE button (screen 07). |
| 18 | Doors, Shadow FSM, NavMesh, catch/respawn, save/load F5/F6, pause, Arabic, gate/dawn/ending, Android simulator | - | **Not verified** | No time. The Shadow was seen patrolling on the NavMesh (agent on mesh, 1.5 m/s). |
| 19 | Editor-only harness notes | - | - | Editing scripts during Play makes this editor recompile and reload mid-play (statics wiped). Do not judge statics after that. With saves on disk, PLAY opens the slot panel; use New Game. |

## 4. Screenshots (Docs/UnityPass/, 1280x720 JPG)

01 main menu (before) · 02 school on arrival, grey-lit · 03 white wedge bug · 04/05 final torch ·
06 inventory with clipped detail pane (before fix) · 07 fragment close-up · 08 quiz with the old
bracket buttons · 09 main menu with the glass kit · 10 Chapter-1 dialogue typing with the voice.
Full-resolution captures are in `Logs/Playtest/` on the test machine (not committed).

## 5. Performance (editor, not a player build)

Editor Play mode, 2560x1440, RTX-class GPU: 62-90 FPS, 11-16 ms. School active point lights: 21
(well under the 90 budget). Shadow atlas downscaling, see #16. **No profiler capture, GC or draw-call
numbers were taken.** Do that in a development build.

## 6. Build results

Not run in this session. Run `BuildWindowsBatch`, `BuildAndroidArm64Batch`, `BuildAndroidArmV7Batch`
next (AGENTS.md). Check issue #15 first.

## 7. Design questions for the owner

1. 7 fragments for a target of 6: keep the spare, or remove one?
2. Should batteries appear in the inventory, or stay instant torch charge?
3. The torch starts switched off (F to turn on). Intended?
4. Calm ambience is fully silent. Keep it, or add a low bed?
5. The Bright Idea meter drains to 0 in about 4 minutes without answers (seen). Is that the intended pace?
6. Torch: any further tuning (width, warmth, brightness)? Close surfaces (about 1 m) get very bright at full meter.
