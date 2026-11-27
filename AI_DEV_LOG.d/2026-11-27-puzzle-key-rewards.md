# 2026-11-27 — Keys through puzzles

The classroom and staff room keys are earned now, not found in corridors:
`PuzzleKeyRewards` listens to `GameEvents.PuzzleSolved` and grants
`classroom_key` (chalkboard cipher), `staff_key` (map locker) and a `medkit`
(first-aid restock) through Inventory, with the same pickup sfx + "Taken"
toast a physical pickup gives. The two loose key pickups are removed from the
scene builder. Grants are idempotent (skip when the item is already carried).
EditMode tests guard the reward map.
