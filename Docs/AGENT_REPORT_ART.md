# AGENT_REPORT_ART.md — Sub-Agent 1 (Art & Tech Art), Premium Pass P1

Date: 2026-10-30 · Branch: `feature/premium-pass` · Editor target: Unity 6000.4.0f1 / URP 17.4 / C# 9
Scope: everything in ROADMAP P1 — CC0 assets, import pipeline, art-swap pass, special visuals
(Shadow / hands / dust), lighting (probes + static flags + bake), post/VFX, license registry.
Constraints honored: no Unity runs, no git, no hand-edited `.unity/.prefab/.asset/.meta` YAML,
no edits outside the owned file list, `SceneBuilderScenes/UI` read via grep-then-window only.

---

## 1. Summary

The school stays a **generated** scene: the SceneBuilder keeps producing the validated greybox,
and three new editor partials/tools upgrade it headlessly:

- **ArtImporter.ImportAllBatch** — configures 24 Poly Haven 2K texture maps (sRGB/normal/linear,
  2048 Windows + 1024 Android override, mips + streaming; roughness maps get an inverted-alpha
  PNG twin that URP/Lit can read in the metallic/smoothness slot), configures all 578 Kenney FBX
  (units, no blendshapes/anims/colliders, medium compression, lightmap UV2), converts Unity's
  auto-generated FBX materials to URP/Lit so nothing is pink, builds `Mat_Art_*` surface
  materials + special materials, assembles `Shadow_Art` / `Hands_Art` / `DustMotes` prefabs,
  wires the smoke silhouette into `TheShadow.prefab` and the procedural hand into `Player.prefab`
  (both via `PrefabUtility.LoadPrefabContents`), and extends the Post_Night/Post_Dawn profiles
  (fog, bloom, film grain) through the VolumeProfile API.
- **SceneBuilder.ApplyArtPassBatch** (partial) — walks 02_School and 03_Ending and swaps visuals
  by object-name table: furniture groups get real Kenney meshes, walls/floors/ceilings get
  per-size tiled PBR materials, colliders/labels/counted objects untouched, fully idempotent,
  logs a per-family count.
- **SceneBuilder.ApplyLightingBatch + BakeLightingBatch** (partial) — static flags on non-moving
  architecture, light probe groups per wing + 3 baked reflection probes, async GI bake with
  explicit non-zero exit when baking is impossible (`-nographics`), per D11.

Nothing was downloaded that could not be license-verified: 4 Kenney CC0 packs (FBX) + 8 Poly Haven
CC0 texture sets ≈ 88 MB total (budget 100 MB). Quaternius moved to Google Drive folders and is
not automatable — logged in ASSET_LICENSES.md and skipped.

## 2. Files created / modified (by me, on disk)

**Created — code:**
- `Assets/_Project/Scripts/Art/ShadowSmoke.shader` — hand-written URP transparent shader: dark
  smoke silhouette, fresnel rim, soft vertex wobble, alpha pulse, fog support.
- `Assets/_Project/Scripts/Tools/Editor/ArtImporter.cs` — import pipeline + material builder +
  special prefab assembly + prefab rewiring + post tuning. Menu: `LightShadow/Art/1.`.
- `Assets/_Project/Scripts/Tools/Editor/SceneBuilderArt.cs` — partial `SceneBuilder`:
  `ApplyArtPass()` / `ApplyArtPassBatch()`. Menu: `LightShadow/Art/2.`.
- `Assets/_Project/Scripts/Tools/Editor/SceneBuilderArtLighting.cs` — partial `SceneBuilder`:
  `ApplyLighting()` / `ApplyLightingBatch()`, `BakeLightingBatch()`, `MarkStaticArchitecture()`,
  `ApplyLightingProbes()`. Menu: `LightShadow/Art/3.`.

**Created — content (extracted from verified CC0 zips):**
- `Assets/ThirdParty/KenneyFurnitureKit/` — 140 FBX + License.txt
- `Assets/ThirdParty/KenneyNatureKit/` — 329 FBX + License.txt
- `Assets/ThirdParty/KenneyGraveyardKit/` — 91 FBX + License.txt
- `Assets/ThirdParty/KenneyBlockyCharacters/` — 18 FBX + License.txt
- `Assets/_Project/Textures/Art/<Surface>/T_<Surface>_{Albedo,Normal,Roughness}.jpg` for
  PlasterWall, WoodFloor, CorridorTile, Concrete, MetalLocker, DoorWood, Grass, Fabric (24 files).

**Modified — docs:**
- `Docs/ASSET_LICENSES.md` — registry filled (table below mirrors it).
- `Docs/AGENT_REPORT_ART.md` — this report.

**Created by my editor code when the Lead runs it (not on disk yet — no Unity runs from my side):**
- `Assets/_Project/Materials/Art/Mat_Art_*.mat`, `M_ShadowSmoke.mat`, `M_Art_DustMote.mat`,
  `M_Art_SmokePuff.mat`, and per-size `Materials/Art/Tiled/Mat_Art_*_T*x*y.mat` variants.
- `Assets/_Project/Textures/Art/Generated/T_<Surface>_Smoothness.png` (inverted roughness) and
  `Generated/T_ParticleSoft.png` (radial particle sprite).
- `Assets/_Project/Prefabs/Art/Shadow_Art.prefab`, `Hands_Art.prefab`, `DustMotes.prefab`.
- `Assets/_Project/Data/LightingSettings.asset`, `Data/ReflectionProbes/Reflection_*.exr`.
- Prefab edits through code: `Prefabs/Enemy/TheShadow.prefab` (smoke silhouette added under
  Visual; primitive renderers hidden, transforms kept for ShadowMotion; ember eyes kept) and
  `Prefabs/Player/Player.prefab` (procedural hand installed under the camera `Hand` node;
  Palm/Thumb/Forearm/TorchBody renderers hidden; FirstPersonHands untouched).
- Scene edits through code: `02_School.unity`, `03_Ending.unity` (art swap, dust motes, probes,
  static flags).
- Unity-generated `.meta` files for everything above — commit them (AGENTS rule 7).

## 3. Pack / URL / license table (mirror of Docs/ASSET_LICENSES.md)

| Asset / Pack | Source URL | License | Author | Used for |
| --- | --- | --- | --- | --- |
| Kenney Furniture Kit (140 FBX) | https://kenney.nl/assets/furniture-kit | CC0 1.0 | Kenney | desks, chairs, bookcases, cabinets, benches, rugs, cardboard box |
| Kenney Nature Kit (329 FBX) | https://kenney.nl/assets/nature-kit | CC0 1.0 | Kenney | bushes, planter foliage, garden trees |
| Kenney Graveyard Kit (91 FBX) | https://kenney.nl/assets/graveyard-kit | CC0 1.0 | Kenney | lightposts, iron fence, pines |
| Kenney Blocky Characters (18 FBX) | https://kenney.nl/assets/blocky-characters | CC0 1.0 | Kenney | The Shadow humanoid silhouette |
| grey_plaster 2K | https://polyhaven.com/a/grey_plaster | CC0 1.0 | Rob Tuytel | Mat_Art_PlasterWall / Mat_Art_Ceiling |
| old_wood_floor 2K | https://polyhaven.com/a/old_wood_floor | CC0 1.0 | Guillaume Monsergent | Mat_Art_WoodFloor |
| floor_tiles_02 2K | https://polyhaven.com/a/floor_tiles_02 | CC0 1.0 | Rob Tuytel | Mat_Art_CorridorTile (reserved, see §6) |
| brushed_concrete 2K | https://polyhaven.com/a/brushed_concrete | CC0 1.0 | Dario Barresi, Dimitrios Savva | Mat_Art_Concrete / Mat_Art_Stone |
| metal_plate_02 2K | https://polyhaven.com/a/metal_plate_02 | CC0 1.0 | Rob Tuytel | Mat_Art_MetalLocker / Mat_Art_GateMetal |
| dark_paneled_wood 2K | https://polyhaven.com/a/dark_paneled_wood | CC0 1.0 | Dimitrios Savva | Mat_Art_DoorWood |
| leafy_grass 2K | https://polyhaven.com/a/leafy_grass | CC0 1.0 | Charlotte Baglioni | Mat_Art_Grass |
| quatrefoil_jacquard_fabric 2K | https://polyhaven.com/a/quatrefoil_jacquard_fabric | CC0 1.0 | colormass, Rico Cilliers | Mat_Art_Fabric |

Failed/aborted attempts (logged per brief): Quaternius direct zips (now Google-Drive-gated —
attempted furniture/animated-zombie/animated-man pages), Kenney "furniture-kit-2" slug (404,
does not exist), Sketchfab/Mixamo/freesound (login-gated, excluded by D9). Kenney's download
endpoint needs a session cookie + browser UA — the working curl flow is documented in
ASSET_LICENSES.md for reproducibility.

## 4. Integration instructions for the Lead (exact order)

```bash
U="C:/Program Files/Unity/Hub/Editor/6000.4.0f1/Editor/Unity.exe"

# 1) Rebuild scenes/prefabs/profiles exactly as before (unchanged pipeline)
"$U" -batchmode -quit -nographics -projectPath . -executeMethod LightShadow.Tools.SceneBuilder.BuildAllBatch -logFile Logs/build.log

# 2) Import art: textures, models, materials, special prefabs, prefab rewiring, post profiles
"$U" -batchmode -quit -nographics -projectPath . -executeMethod LightShadow.Tools.ArtImporter.ImportAllBatch -logFile Logs/artimport.log

# 3) Art swap pass (edits 02_School + 03_Ending in place; idempotent)
"$U" -batchmode -quit -nographics -projectPath . -executeMethod LightShadow.Tools.SceneBuilder.ApplyArtPassBatch -logFile Logs/artpass.log

# 4) Static flags + light/reflection probes
"$U" -batchmode -quit -nographics -projectPath . -executeMethod LightShadow.Tools.SceneBuilder.ApplyLightingBatch -logFile Logs/lighting.log

# 5) GI bake — WITHOUT -nographics (exits 1 with one clear log line if run without graphics)
"$U" -batchmode -quit -projectPath . -executeMethod LightShadow.Tools.SceneBuilder.BakeLightingBatch -logFile Logs/bake.log

# 6) Validation gate (must print RESULT: PASS)
"$U" -batchmode -quit -nographics -projectPath . -executeMethod LightShadow.Tools.SceneBuilder.ValidateBatch -logFile Logs/validate.log

# 7) Builds as usual
"$U" -batchmode -quit -nographics -projectPath . -executeMethod LightShadow.Tools.SceneBuilder.BuildWindowsBatch -logFile Logs/win.log
"$U" -batchmode -quit -nographics -projectPath . -executeMethod LightShadow.Tools.SceneBuilder.BuildAndroidBatch -logFile Logs/android.log
```

Grep each log for `RESULT:|error CS|Exception|ART PASS|ART IMPORT|LIGHTING BAKE` — never read
full logs. Commit the generated `.meta` files next to the new assets.

**Optional one-line wiring** (Lead applies; keeps rebuilds self-consistent so a future
BuildAllBatch cannot ship greybox scenes again). In `SceneBuilder.BuildAll()` directly after
`BuildEndingScene(palette);` add:

```csharp
// Premium-pass art: re-apply the visual swap + probes after every rebuild.
ApplyArtPass();
ApplyLighting();
```

If this wiring is applied, steps 3–4 collapse into step 1 (step 2 must still have run once so the
materials/prefabs exist). The GI bake (step 5) always stays a separate command because it exits
the process and needs a graphics device.

## 5. Keying strategy — what got swapped vs. skipped

**Strategy:** the SceneBuilder builds everything through `Box()/Table()/Chair()` factories with
distinct English names (`Wall`, `Floor`, `Shelf_W1`, `Desk_0_0`, `Chair_1_2`, `Lockers_1`,
`Bench_1`, `BenchSeat`, `Bush`, `Planter_0`, `MemoryBox`, `Stage`, `StageBackdrop`,
`LampPost_0`, gate children `PillarL/PillarR/Panel/Bar0..4/Console`...). The art pass keys on
**object-name patterns** (exact-match families first, then prefixes; `BenchSeat` before
`Bench_`; physics-lab `Bench_N` vs main-hall `Bench_r_c` distinguished by parent name). Prefab
instances (QuizBoard/FuseBox/Battery/KnowledgeFragment/Player/TheShadow) are never descended
into — the ExitGate instance is styled in place by name, the Shadow/Player prefabs are restyled
at the prefab-asset level. TextMesh labels (`Label_*`, `Story_*`, `Notice`, `MemoryCard`) are
skipped by a TextMesh component check.

**Swapped (visual only, colliders/transforms/counted components untouched):**
- Walls, lintels, ceilings, Floor, GardenGrass, Stage, StageBackdrop → tiled PBR material
  variants (per-size, quantized to a 0.5 m grid so texture density stays true to world scale).
- `Desk_*`, `TeacherDesk`, `StaffTable`, `ReceptionDesk`, `ReadingTable`, `MotherDesk`,
  `Chair_*` → Kenney desk/table/chairDesk meshes as `ArtModel_*` children (greybox child
  renderers hidden, seat collider kept).
- `Shelf_*` → bookcaseOpen; `Cabinet` → bookcaseClosedDoors; `Lockers_*` + lab `Bench_*` →
  brushed-metal texture on the box; hall `Bench_r_c` → bench mesh (backs hidden); garden
  `BenchSeat/Back/LegL/LegR` → one bench mesh.
- `Bush` spheres → plant_bush meshes; `Planter_*` → stone texture + bush on top; `Rug` →
  rugRectangle; `MemoryBox` → cardboardBoxClosed; ending `PillarL/R` → stone; `LampPost_*` →
  lightpost mesh; ending gate `Bar_*/RailTop/RailBottom` + school ExitGate structure →
  Mat_Art_GateMetal.
- The Shadow prefab → Kenney blocky humanoid (tallest of 18) in M_ShadowSmoke + dark particle
  halo; ember eyes and ShadowMotion arm transforms preserved.
- Player hand → procedural `Hands_Art` (Root→Forearm→Hand, fingers curled around the torch,
  same torch footprint as before).
- Decor added: 2 garden trees, 2 garden lampposts, 2 forecourt pines, 10 iron-fence segments
  (gate gap kept open), 3 DustMotes instances (entrance / corridor / main hall).

**Skipped (on purpose):** interactable prefabs (QuizBoard, FuseBox, BatteryPickup,
KnowledgeFragment — their flat palette colors read as intentional signage/indicators), lamp
`Fixture` boxes (Mat_Lamp is the functional glow), `Beaker_*` glass, `DawnPlane`, Sun/Sky/dawn
wiring in the ending, all TextMesh labels, all lights, all colliders.

**Idempotency:** material swaps check `sharedMaterial.name.StartsWith("Mat_Art")`; model swaps
check for an existing `ArtModel_*` child; probes/dust/decor check for their named roots. A second
run re-applies nothing and re-reports counts honestly.

**Validator safety:** nothing is deleted or renamed; no counted component is touched; no scripts
are added to scenes (no "missing script" risk); model instances carry no colliders so the
NavMesh (built from PhysicsColliders on LevelLayer) is unaffected; the exit-gate leaf keeps its
transform so `ExitGate` animation is intact.

## 6. Risks / limitations (honest list)

1. **Blind-compiled shader.** `ShadowSmoke.shader` is hand-written HLSL and has never been
   compiled by Unity (no Unity runs allowed). If it has a compile error, ArtImporter logs
   `[ArtImporter] Shader not found` and falls back to a dark transparent URP/Lit so the rest of
   the pipeline still works; fix the shader and re-run.
2. **Model facing.** Kenney FBX authoring orientation is guessed (desks/tables y=0, garden bench
   rotated 90°). After the first visual check, at most a 90/180° yaw constant per family may
   need flipping — each is a single local-rotation line in `SceneBuilderArt`.
3. **Kenney per-color materials are per-FBX, not deduped.** Unity generates one .mat per FBX
   material inside `Assets/ThirdParty/<Pack>/Materials/` and I convert them in place to URP/Lit
   (they import on the built-in Standard shader = pink under URP; these files are Unity import
   output, not pack content — source FBX untouched). That is several hundred tiny materials;
   SRP Batcher still batches, but a color-keyed dedupe pass is a documented follow-up if mobile
   batches run high.
4. **Smoothness from roughness is fully wired** (inverted-alpha PNG in the metallic/smoothness
   slot, `_SmoothnessTextureChannel = MetallicAlpha`) — no constant-value fallback needed for the
   8 PBR surfaces; only constant materials (chalkboard, skin, torch) use fixed smoothness.
5. **Baked GI requires a graphics device.** `BakeLightingBatch` under `-nographics` exits 1 with
   one clear log line (per D11 this is an accepted outcome — probes + reflection probes ship
   regardless; lightmap resolution 24 keeps the bake cheap when it does run).
6. **Reflection probe bake** is per-probe with warnings on failure (gameplay degrades to ambient
   only); it is not fatal to the batch.
7. **Hands are procedural, not a CC0 arm** — no auth-free CC0 arm/hand model was available from
   the allowed sources (Quaternius Drive-gated, Sketchfab/Mixamo login). The delivered
   Root→Forearm→Hand hierarchy is the wiring point for ENG.
8. **CorridorTile is built but not assigned** — the corridor shares the wood-floor look with
   classrooms; one line in `ApplyArchitectureMaterial` (`Floor` case is global) or a per-room
   check switches it if desired.
9. **Wall/ceiling tiling uses per-size material variants** (~dozens of small materials,
   deterministic and idempotent) instead of a runtime world-UV component — chosen because
   MaterialPropertyBlocks would break SRP Batcher compatibility.
10. **Character choice for the Shadow** (tallest blocky character) is heuristic; if the silhouette
    reads wrong in play mode, swap the file name in `PickTallestCharacter`.
11. **Post tuning is additive and idempotent**; DepthOfField is only disabled if a previous run
    added it (absent = off in gameplay by construction). Flashlight flare / dawn god-rays are
    NOT implemented (needs LensFlare asset authoring) — documented follow-up.
12. **Everything is unverified by Unity** by design — first batch run may surface small compile
    errors; all batch methods follow the project's try/catch + exit-code pattern so failures are
    loud and isolated.

## 7. Draft AI_DEV_LOG.md entry (Lead pastes into AI_DEV_LOG.md)

```text
### 2026-10-30 — Premium pass P1: art sourcing, import pipeline, art-swap pass, lighting setup — Abdelrhman Mohamed
- Tool used: Claude agent (Lead TD orchestration)
- What AI provided: the CC0 asset sourcing and verification (4 Kenney packs + 8 Poly Haven PBR
  sets, ~88 MB, every URL/license logged in Docs/ASSET_LICENSES.md), the headless import
  pipeline (ArtImporter.cs: texture/model importer settings, URP/Lit material builder with
  roughness-to-smoothness conversion, Shadow/Hands/DustMotes prefab assembly, TheShadow and
  Player prefab rewiring, Post_Night/Post_Dawn fog/bloom/grain via the VolumeProfile API), the
  art-swap layer over the generated scenes (SceneBuilderArt.cs: name-keyed mesh/material swaps,
  idempotent, validator-safe), the lighting pass (SceneBuilderArtLighting.cs: static flags,
  light probes per wing, 3 baked reflection probes, async bake batch with honest -nographics
  failure), and the hand-written ShadowSmoke shader (dark silhouette + rim + wobble).
- What I changed myself: (Lead TD — fill after merge review) verified the license table,
  reviewed the swap table against the actual scene names, chose the final batch order and the
  BuildAll wiring, ran all Unity batch commands and validated results.
- Teach-back check: pending — walk-through of ApplyArtPass keying strategy and the bake
  command's graphics requirement scheduled before the P4 merge.
- Test result: not run — Lead verifies via batch (BuildAllBatch → ImportAllBatch →
  ApplyArtPassBatch → ApplyLightingBatch → BakeLightingBatch → ValidateBatch must print
  RESULT: PASS).
```
