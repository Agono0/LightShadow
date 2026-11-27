### 2026-11-27 — feat/audio-tired-breath-tinyworlds-steps: tired breath + TinyWorlds real footsteps — Instinct agent (for Ahmed)
- Tool used: Instinct (AI agent)
- What AI provided: peer-approved wave-2 audio picks. Added SFX_Breath_Tired
  (mikeask "breathing tired.wav", CC0) and 7 TinyWorlds real-recorded steps
  (wood01-03, leaves01-02, gravel, mud02, CC0, from [kdd]DifferentSteps_0.zip).
  Processing per file (ffmpeg): mono downmix (0.5L+0.5R), leading/trailing
  silence trim at -50 dB with 20 ms keep, peak normalized to -3.0 dBFS,
  encoded Vorbis q5 44.1 kHz mono ogg. Exact gain, source sha256 and
  processed sha256 are recorded per row in Docs/ASSET_MANIFEST.md.
  No .meta files committed (per task scope: no GUID assignment) - Unity
  generates them on first open; importer targets are ADPCM mono per row spec.
  ASSET_LICENSES.md not updated (outside granted paths for this PR);
  full license evidence is in the manifest rows.
- What I changed myself: n/a (agent-authored; no human edits yet).
- Teach-back check: pending - PR review with the owner.
- Test result: validate_asset_manifest.py RESULT: PASS (138 rows, v2 schema).
  Listening/audition not possible here: suitability of every clip is
  UNVERIFIED (marked per-row in human_check and notes). mud02 trims to a
  0.07 s transient; wood01-03 sources are dominated by ~70 Hz thump and may
  need EQ. Fantozzi sand-to-grass mapping held pending a human audition.
