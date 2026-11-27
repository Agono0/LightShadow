### 2026-11-27 — feat/audio-p0-throw-impacts-scrape: P0 throw whoosh/impacts + crate scrape — Instinct agent (for Ahmed)
- Tool used: Instinct (AI agent)
- What AI provided: peer-approved P0 group-3 picks. Added SFX_ThrowWhoosh_0
  (artisticdude swish-9.wav, CC0, swishes.zip), SFX_ThrowImpactSoft_0 (rubberduck
  wood_misc_01.ogg, CC0, 100-CC0-wood-metal-SFX.zip; peer chose the dry knock over
  a wet squelch), SFX_ThrowImpactHard_0 (rubberduck wood_hit_02.ogg, CC0, same zip)
  and SFX_CrateScrape_0 (AntumDeluge scrape-3.ogg, CC0, scrapes.zip). Processing
  per file (ffmpeg): mono downmix (0.5L+0.5R) where stereo, silence trim
  -50 dB/20 ms both ends, peak normalized to -3.0 dBFS (two-pass), Vorbis q4
  44.1 kHz mono. Exact gain, source sha256 and processed sha256 per row in
  Docs/ASSET_MANIFEST.md; license evidence also in Docs/ASSET_LICENSES.md.
  Importer .meta per asset (fresh GUIDs, ADPCM, forceToMono+normalize); no wiring.
- What I changed myself: n/a (agent-authored; no human edits yet).
- Teach-back check: pending - PR review with the owner.
- Test result: validate_asset_manifest.py RESULT: PASS (143 rows, v2 schema).
  Listening/audition not possible here: suitability of every clip is UNVERIFIED
  (marked per-row in human_check and notes).
