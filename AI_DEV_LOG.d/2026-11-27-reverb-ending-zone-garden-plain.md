# Reverb: ending zone + garden preset + TASKS wording (2026-11-27)

Folds the decisions from Docs/REVERB_ZONES.md (bro's audit PR) into the
builder: the dawn/ending scene gets one `Plain` AudioReverbZone over the
pavement and gate (it had none, so 3D sources ran dry), the open-air Garden
moves from `Room` to `Plain`, and Docs/TASKS.md now names the real preset
(`StoneCorridor`; Unity's enum has no `MediumRoom`). The library stays `Room`
on purpose: shelved rooms are dead-ish, not anechoic. Zone generation stays
idempotent and bake-safe (Default-layer triggers, structural falloff).
