### 2026-11-27 — feat/audio-p0-female-hurt: P0 female hurt hit/heavy — Instinct agent (for Ahmed)
- Tool used: Instinct (AI agent)
- What AI provided: peer-approved P0 group-2 picks (girl protagonist: female or
  neutral voice only; male vocal packs dropped). Added SFX_HurtHit_0 (segment
  3.73-4.09 s) and SFX_HurtHeavy_0 (segment 4.93-6.08 s), both from
  female_hurt_grunts_groans_1.ogg (Nocturnal_Vanguard, CC0). Processing per file
  (ffmpeg): segment extract, silence trim -50 dB/20 ms both ends, peak normalized
  to -3.0 dBFS (two-pass), Vorbis q4 44.1 kHz mono. Exact offsets, gain and
  source/processed sha256 per row in Docs/ASSET_MANIFEST.md; license evidence also
  in Docs/ASSET_LICENSES.md. Importer .meta per asset (fresh GUIDs, ADPCM,
  forceToMono+normalize); no wiring.
- What I changed myself: n/a (agent-authored; no human edits yet).
- Teach-back check: pending - PR review with the owner.
- Test result: validate_asset_manifest.py RESULT: PASS (141 rows, v2 schema).
  Listening/audition not possible here: segments chosen by silence/duration
  structure only; sound-fit provisional and UNVERIFIED (marked per-row).
