### 2026-11-27 - ambientCG CC0 textures: floors family - Instinct agent (for Ahmed)
- Tool used: Instinct (AI agent)
- What AI provided: downloaded WoodFloor065B, Planks009, Fabric028 from ambientCG (CC0 verified per-set page +
  site license 2026-11-27), imported albedo/normal/roughness into
  Assets/_Project/Textures, generated smoothness twins (roughness inverted to alpha,
  Textures/Art/Generated), cloned 3 material(s) per STYLE_GUIDE with Android 1K
  overrides cloned from the Concrete set metas, and logged the sets in
  Docs/ASSET_LICENSES.md.
- What I changed myself: n/a (agent-authored; no human edits yet).
- Teach-back check: pending - PR review with the owner.
- Test result: albedo contact-sheet visually verified for the wing-1/2 staging set;
  NO Unity on this machine - material look, tiling and normal strength need a human
  check in the editor (noted per-PR). Manifest register rows intentionally deferred
  to a single register PR after #15 lands (v2 schema), per coordination.
