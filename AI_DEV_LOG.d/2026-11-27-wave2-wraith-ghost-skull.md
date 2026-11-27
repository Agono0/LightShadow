### 2026-11-27 — feat/wraith-ghost-skull: Ghost Skull wraith model (Quaternius, CC0) — Instinct agent (for Ahmed)
- Tool used: Instinct (AI agent)
- What AI provided: peer-picked Ghost Skull (Ultimate Monsters) as the wraith
  hero model. Fetched the exact served GLB from the model's poly.pizza page
  inside a browser session (sha256 d1e2268fe9621583ff879d6e6e3ace80b0a6a2373ec0139a8139c88fe6e4d942,
  232,384 bytes) and published it UNCHANGED at
  Assets/_Project/Models/SM_Wraith_GhostSkull.glb with a DefaultAsset .meta
  (fresh GUID; CI requires meta pairing). One manifest v2 row
  (id=family=SM_Wraith_GhostSkull, type model, status imported,
  human_check outstanding) and ASSET_LICENSES.md prose entry.
  Verified from the GLB header: mesh Ghost_Skull, 3,144 tris, 51 nodes,
  1 skin (CharacterArmature), materials Ghost_Main/Ghost_Secondary,
  0 embedded textures, 8 clips (Death, Fast_Flying, Flying_Idle, Headbutt,
  HitReact, No, Punch, Yes). No scripts, scenes, prefabs, packages, wiring.
- What I changed myself: n/a (agent-authored; no human edits yet).
- Teach-back check: pending - PR review with the owner.
- Test result: validate_asset_manifest.py RESULT: PASS (131 rows, v2 schema).
  No Unity here: scale/pivot (GLB uses quantized attributes), material look
  and clip motion are UNVERIFIED and need an in-editor check.
