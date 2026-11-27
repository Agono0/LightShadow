### 2026-11-27 — Item system: Inventory + ItemPickup, master key first (plan phase 1) — Abdelrhman Mohamed
- Tool used: Instinct (AI coding agent)
- What AI provided: `Inventory` (a static, scene-independent carried-item set with an
  inventoryJson blob on SaveSystem's capture/apply events, the storyJson/puzzleJson pattern;
  old saves deserialize empty), `ItemPickup` (a prop becomes a carried item on interact and
  hides itself on load when already carried), the master key as the first real item (pickup
  on the key root, nearer to the ray hit than the locker's solved puzzle, with a collidable
  ring), and the gate console now checks Inventory.Has("master_key") with the puzzle-solved
  fallback for older saves. SaveSystem.cs gains one additive string field (shared file,
  message-first honored in the PR report).
- What I changed myself: pending review — changes land through the PR workflow (AGENTS.md rule 9).
- Teach-back check: pending — PR body explains the Start-order reasoning (an inactive key
  runs its ItemPickup.Start only when revealed, after the inventory blob is applied) and why
  fuse boxes stay touch-to-restore for now (a fuse-item flow is a design change of its own).
- Test result: pending — CI compile + builds are the gate; no local Unity editor. Verified on
  paper: additive SaveData field defaults to "" on old saves; ItemAdded event guarded;
  key pickup resolves before the locker puzzle via nearest-ancestor IInteractable.
