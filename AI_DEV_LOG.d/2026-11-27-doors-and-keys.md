# Interactive doors, keys, medkit placements (2026-11-27)

From the user's answered questions (WhatsApp 13:01): doors open/close/lock
with keys; the wraith can pass a door only after Nour freed it herself.

- DoorController (new): open/close swing on a smoothstepped hinge, locked
  state keyed to an inventory item, prompts for every state. While Nour has
  never freed a door a carving NavMeshObstacle keeps it out of the wraith's
  paths; once she has, the obstacle lifts and a closed-but-freed door swings
  for the wraith as it arrives. Door roots stay off the LevelLayer so the
  baked NavMesh keeps the doorway walkable.
- All six corridor wing gaps get real doors (replacing the two decor boxes):
  library/infirmary/physics/garden unlocked, classroom locked with the
  classroom key, staff room locked with the staff key.
- Key chain, soft-lock safe: library hides the classroom key, classroom
  hides the staff key, staff room keeps its master key. Keys are plain
  pickups for now - moving them behind puzzle solves is the
  keys-through-puzzles follow-up the brief asks for.
- Two medkits placed (infirmary shelf, staff room); MedkitPickup gains the
  same load persistence as ItemPickup; inventory display names for the new
  items in EN+AR.
- Placeholder door audio (click/power/wrong synths) until bro's door clips.
