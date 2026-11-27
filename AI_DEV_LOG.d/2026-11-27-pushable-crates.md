# Movable crates + door-bake fix (2026-11-27)

From the user's answered questions (WhatsApp 13:01, item 18): breakable and
movable objects. Breakables shipped with the throwables; this adds the
movables.

- PlayerPush (new, on the rig): walking into a light rigidbody shoves it,
  force scaled by mass so crates feel heavy.
- PushableProp (new): a hard scrape or impact makes a positional noise the
  wraith can hear - blocking a path with furniture is a gamble, not a free
  wall. Three crates placed (corridor, classroom, physics lab).
- Fix on the doors stream: door panels, handles and crate bodies now build
  on layer 0. Box() defaults to LevelLayer, which the NavMesh bake collects
  - a baked-in panel would have sealed every doorway permanently.
