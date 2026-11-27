---
pr: docs/reverb-zones
date: 2026-11-27
agent: audio-assets-agent
---

## Reverb zone spec (docs only)

Docs/REVERB_ZONES.md: audit of the existing builder-generated AudioReverbZone
setup in 02_School (8 zones, table + falloff verified against
SceneBuilderScenes.cs and the baked scene), plus five flagged gaps/deviations
for lead decision (03_Ending has zero zones; "library dead" vs Room preset;
TASKS.md's nonexistent "MediumRoom" name; Garden's indoor preset; no
reverbZoneMix policy). No scene, prefab, mixer or script edits. Peer approved
this doc path in advance.
