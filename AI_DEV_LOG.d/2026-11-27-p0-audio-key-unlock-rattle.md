### 2026-11-27 — feat/audio-p0-key-unlock-rattle: P0 key/unlock/rattle SFX — Instinct agent (for Ahmed)
- Tool used: Instinct (AI agent)
- What AI provided: peer-approved P0 group-1 picks. Added SFX_KeyPickup_0
  (rubberduck keys_02.ogg, CC0, from 100-CC0-wood-metal-SFX.zip), SFX_DoorUnlock_0
  (rubberduck lock_open_01.ogg, CC0, same zip) and SFX_DoorLockedRattle_0
  (pauliuw trying_to_unlock.mp3, CC0). Processing per file (ffmpeg): mono downmix
  (0.5L+0.5R) where stereo, silence trim -50 dB/20 ms both ends, peak normalized
  to -3.0 dBFS (two-pass), Vorbis q4 44.1 kHz mono. Exact gain, source sha256 and
  processed sha256 per row in Docs/ASSET_MANIFEST.md; license evidence also added
  to Docs/ASSET_LICENSES.md. Importer .meta committed per asset (fresh GUIDs,
  AudioImporter ADPCM, forceToMono+normalize, from the existing SFX template)
  because repo fast-checks enforce meta pairing; no wiring or references.
- What I changed myself: n/a (agent-authored; no human edits yet).
- Teach-back check: pending - PR review with the owner.
- Test result: validate_asset_manifest.py RESULT: PASS (142 rows, v2 schema).
  Listening/audition not possible here: suitability of every clip is UNVERIFIED
  (marked per-row in human_check and notes).
