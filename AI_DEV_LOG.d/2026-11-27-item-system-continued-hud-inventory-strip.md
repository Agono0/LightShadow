### 2026-11-27 — Item system, continued: HUD inventory strip — Abdelrhman Mohamed
- Tool used: Instinct (AI coding agent)
- What AI provided: `InventoryStripUI` (one chip per carried item under the HUD gauges,
  hidden while empty, rebuilds on pickup and on save load), the strip wiring in
  `BuildSchoolUI`, and the Inventory catalog (`DisplayName(id)` EN/AR, `Carried` enumeration)
  the strip reads — names live in Inventory because ItemPickup props can be inactive until
  revealed, so their own fields are not a reliable registry.
- What I changed myself: pending review — lands through the PR workflow (AGENTS.md rule 9).
- Teach-back check: pending — the catalog placement is the one judgment call (inactive props
  never run Awake/Start, so a prop-held name registry would miss the master key until its
  locker opens; the alternative, a ScriptableObject catalog, is hand-authored YAML, which the
  repo rules route through builders instead).
- Test result: pending — CI compile + builds; no local Unity editor. The strip self-hides on
  empty inventories, so saves without items render exactly as before.
