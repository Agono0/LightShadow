# LIGHT SHADOW — Asset License Registry

Rule (brief + D9): every downloaded asset is logged here with name, source URL, license and
author **before it is used in the project**. Only licenses that permit commercial use are allowed.
Downloads land in `Assets/ThirdParty/<pack>/` and are never edited in place (doc §6.6).

All entries below were downloaded and verified on **2026-10-30** (premium pass, P1 art). Every
pack ships its original `License.txt` inside its folder under `Assets/ThirdParty/`.

| Asset / Pack | Source URL | License | Author | Imported at | Used for |
| --- | --- | --- | --- | --- | --- |
| Kenney Furniture Kit (140 FBX) | https://kenney.nl/assets/furniture-kit | CC0 1.0 | Kenney (kenney.nl) | 2026-10-30 | desks, chairs, bookcases (shelves), cabinets, benches, rugs, cardboard box — classroom/library/staff/entrance props |
| Kenney Nature Kit (329 FBX) | https://kenney.nl/assets/nature-kit | CC0 1.0 | Kenney (kenney.nl) | 2026-10-30 | bushes/planter foliage, garden trees |
| Kenney Graveyard Kit (91 FBX) | https://kenney.nl/assets/graveyard-kit | CC0 1.0 | Kenney (kenney.nl) | 2026-10-30 | lightposts, iron fence segments, pines — forecourt/garden dressing |
| Kenney Blocky Characters (18 FBX) | https://kenney.nl/assets/blocky-characters | CC0 1.0 | Kenney (kenney.nl) | 2026-10-30 | The Shadow's humanoid silhouette mesh (tallest of the 18, wrapped in M_ShadowSmoke) |
| grey_plaster (2K JPG: diff/nor_gl/rough) | https://polyhaven.com/a/grey_plaster | CC0 1.0 | Rob Tuytel | 2026-10-30 | Mat_Art_PlasterWall (walls, lintels), Mat_Art_Ceiling (tinted) |
| old_wood_floor (2K JPG) | https://polyhaven.com/a/old_wood_floor | CC0 1.0 | Guillaume Monsergent | 2026-10-30 | Mat_Art_WoodFloor (school floor, stage) |
| floor_tiles_02 (2K JPG) | https://polyhaven.com/a/floor_tiles_02 | CC0 1.0 | Rob Tuytel | 2026-10-30 | Mat_Art_CorridorTile (reserved; corridor floors currently share the wood floor) |
| brushed_concrete (2K JPG) | https://polyhaven.com/a/brushed_concrete | CC0 1.0 | Dario Barresi, Dimitrios Savva | 2026-10-30 | Mat_Art_Concrete, Mat_Art_Stone (planters, ending pillars) |
| metal_plate_02 (2K JPG) | https://polyhaven.com/a/metal_plate_02 | CC0 1.0 | Rob Tuytel | 2026-10-30 | Mat_Art_MetalLocker (lockers, lab benches), Mat_Art_GateMetal (exit gate, forecourt gate) |
| dark_paneled_wood (2K JPG) | https://polyhaven.com/a/dark_paneled_wood | CC0 1.0 | Dimitrios Savva | 2026-10-30 | Mat_Art_DoorWood (chalkboard frames) |
| leafy_grass (2K JPG) | https://polyhaven.com/a/leafy_grass | CC0 1.0 | Charlotte Baglioni | 2026-10-30 | Mat_Art_Grass (garden ground) |
| quatrefoil_jacquard_fabric (2K JPG) | https://polyhaven.com/a/quatrefoil_jacquard_fabric | CC0 1.0 | colormass, Rico Cilliers | 2026-10-30 | Mat_Art_Fabric (stage backdrop) |

Download volume: Kenney packs 21.5 MB zips + Poly Haven 67 MB (24 × 2K JPG) ≈ 88 MB, inside the
≤ 100 MB brief budget. Kenney packs were extracted selectively (only `Models/FBX format/` +
`License.txt`; the isometric preview PNGs, GLB and OBJ duplicates were left out).

## Source license notes (verified before downloading)

| Source | License terms | Automation notes |
| --- | --- | --- |
| Kenney (kenney.nl) | CC0 1.0 (all asset packs) | Direct zip downloads, no auth. Automated: GET the asset page for the hex `data-download-id`, GET `/download-link/<id>` (requires a session cookie from the page visit + browser UA) → signed URL → zip. Verified working in a headless curl flow on 2026-10-30. |
| Quaternius (quaternius.com) | CC0 1.0 (Ultimate/pack content) | NOT automatable anymore: pack pages now link to Google Drive folders (session/JS gated). Attempted 2026-10-30 on furniture / animated-zombie / animated-man — no direct zip exposed; skipped per D9. |
| Poly Haven (polyhaven.com) | CC0 1.0 (textures/HDRI/models) | Public API `api.polyhaven.com` (assets/files/info) + CDN `dl.polyhaven.org`. All 8 texture sets verified to ship diff/nor_gl/rough 2K JPGs before download. |
| ambientCG (ambientcg.com) | CC0 1.0 | Direct zips via `ambientcg.com/get?file=…`. Not needed — Poly Haven covered all 8 surfaces. |
| OpenGameArt (opengameart.org) | Per-asset (CC0/CC-BY/CC-BY-SA) | Only CC0/CC-BY assets allowed; CC-BY-SA rejected (contagion risk). Not needed. |
| Pixabay | Pixabay Content License | Only where direct download works without auth. Not needed. |
| Sketchfab / Mixamo / freesound | Various | Login-gated — excluded by the brief (D9). |

## Attribution list (only needed for CC-BY entries; CC0 requires none)

- (empty — all adopted assets are CC0)
