# Two-hit health, checkpoint respawn, medkits (2026-11-27)

From the user's answered questions (WhatsApp 13:01): fixed medium difficulty;
first strike takes half health, the second ends her and she respawns at the
current chapter's checkpoint; health bar on the HUD; healing only through
rare medkits (puzzle rewards).

- PlayerHealth (new, on the player rig): 2 hits, Changed/Hit/Died events,
  TryUseMedkit, ResetHealth on the checkpoint wake; H uses a kit on desktop.
- GameManager.EnemyHitPlayer: a survived strike is scare-only (sting, haptics,
  meter penalty, red HUD pulse) with no respawn; a lethal strike runs a
  slower 2s fade into the existing checkpoint wake, which now refills health.
  Rigs without PlayerHealth keep the old one-hit CaughtPlayer flow.
- EnemyBehaviour: after a strike the wraith enters ForcedCooldown (withdraws
  from the wing), so the second hit is never instant.
- HUD: red HEALTH bar beside the battery gauge, painted on subscribe; red
  flash on a survived hit.
- MedkitPickup (new gameplay prop): adds "medkit" to the inventory (one
  carried at a time); use with H or the new round MEDKIT touch button in the
  mobile utility row.
- Still to come in later batches: medkit placements as puzzle rewards, the
  death animation proper (needs the character model), key/door system.
