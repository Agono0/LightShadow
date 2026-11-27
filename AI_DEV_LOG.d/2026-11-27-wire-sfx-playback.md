# Wire the landed SFX into gameplay (2026-11-27)

The P0 clips were registered in the AudioLibrary but nothing played them;
doors, throws, crates and pickups still used synthesised placeholders.

- New `Audio/Sfx` helper: asks SfxRegistry for an id (random variant when a
  family has several) and falls back to the old synthesised clip when the
  library is absent, so headless runs behave exactly as before.
- Wired: door open/close/unlock/locked rattle, key and generic pickups,
  throw whoosh, soft/hard throw impact, glass crack on breakables, crate
  scrape.
- Not wired here: hurt (clips arrive with #92), wraith roam/stinger/breath and ambient events (next PR).
- Unverified: nothing heard in the editor or on a device; clip levels and
  loudness vs the mixer are untested.
