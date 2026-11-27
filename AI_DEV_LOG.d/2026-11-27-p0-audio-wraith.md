### 2026-11-27 — feat/audio-p0-wraith: P0 wraith roam/ambush/breath — Instinct agent (for Ahmed)
- Tool used: Instinct (AI agent)
- What AI provided: peer-approved P0 group-4 picks (provisional, all listening
  UNVERIFIED). Added SFX_WraithRoam_0 (segment 0.00-7.00 s of Ruhinre
  whispers_in_the_fog.ogg, CC0; the source is a continuous whisper bed with no
  silence gaps at -30 dB/300 ms and a flat RMS envelope, so the segment is the
  file head to 7.00 s, boundaries arbitrary and chosen by duration only, with
  150 ms fade-in + 400 ms fade-out), SFX_WraithAmbushStinger_0 (congusbongus
  abyss.ogg full 7.54 s, CC0) and SFX_WraithBreathNear_0 (qubodup
  ghostbreath.flac, CC0, ONE-SHOT per peer ruling; loop seam unverified).
  Processing per file (ffmpeg): mono downmix (0.5L+0.5R), silence trim
  -50 dB/20 ms (except roam: fades instead), peak normalized to -3.0 dBFS
  (two-pass), Vorbis q4 44.1 kHz mono. Exact offsets, gains and source/processed
  sha256 per row in Docs/ASSET_MANIFEST.md; license evidence also in
  Docs/ASSET_LICENSES.md. Importer .meta per asset (fresh GUIDs, ADPCM,
  forceToMono+normalize); no wiring.
- What I changed myself: n/a (agent-authored; no human edits yet).
- Teach-back check: pending - PR review with the owner.
- Test result: validate_asset_manifest.py RESULT: PASS (142 rows, v2 schema).
  Listening/audition not possible here: every clip UNVERIFIED (marked per-row).
