### 2026-11-27 — feat/models-p1-props: P1 prop models + dust mote — Instinct agent (for Ahmed)
- Tool used: Instinct (AI agent)
- What AI provided: peer-approved P1 picks. Added 8 Quaternius GLBs (CC0,
  poly.pizza), all published UNCHANGED (sha256 per row): SM_Key (284 tris),
  SM_Medkit (268), SM_Crate (784), SM_Book (668), SM_Bottle (216, embedded
  Sushi_Atlas.png 512x512), SM_Brick (540, embedded Atlas.png 32x32), SM_DoorProp
  (2216), SM_TorchBody (610); plus T_DustMote.png (Kenney circle_05.png, CC0,
  downscaled to 128x128 RGBA, alpha preserved). Peer exceptions granted
  2026-11-27 and recorded: DoorProp 2216 tris over the <2000 limit; Brick 32x32
  atlas below the 512 minimum (unchanged, no upscaling, no quality claim);
  flat-color texture-less materials; off-spec sizes and non-base pivots
  (UNNORMALIZED per row). Geometry evidence (tris, world bbox, pivot) was parsed
  from the served GLB bytes in-browser; no importer or visual-fit claims.
  Importer .meta per asset (fresh GUIDs; model metas minimal like
  SM_Wraith_GhostSkull, texture meta from the existing TextureImporter
  template). No wiring, no scene/prefab/script changes.
- What I changed myself: n/a (agent-authored; no human edits yet).
- Teach-back check: pending - PR review with the owner.
- Test result: validate_asset_manifest.py RESULT: PASS (148 rows, v2 schema).
  In-editor import/scale/pivot checks outstanding (per-row human_check).
