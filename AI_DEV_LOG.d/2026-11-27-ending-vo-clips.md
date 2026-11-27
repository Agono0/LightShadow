### 2026-11-27 — feat/ending-vo-clips: ending VO, 14 lines x EN/AR — Instinct agent (for Ahmed)
- Tool used: Instinct (AI agent)
- What AI provided: SYNTHETIC generated-voice clips (TTS), user-approved batch.
  28 clips: VO_SN_Ending_01..14.ogg in Assets/_Project/Resources/VO/{EN,AR}/,
  named per Docs/ENDING_VO_SPEC.md §3 (clip id VO_<nodeId>, language by
  directory). Voices are SYNTHETIC (generated TTS, same casting as the
  previously accepted generated batches: Memory narrator, Nour, Mother, Shadow),
  NOT human recordings; the spec's recording chain (VO_RECORDING_SPECS.md §3)
  describes the human-recording target - these clips are the accepted generated
  placeholder batch per the accepted batch policy. Format per the settled batch
  agreement: mono 24 kHz Vorbis .ogg (q4), one take per line, loudness and
  trimming untouched from generation. Importer .meta per clip (fresh GUIDs,
  copied from the existing VO_SN_Act1_01.ogg.meta template). No manifest rows
  (VO clips are tracked outside the register), VO_CLIP_MAP.md untouched, no
  wiring, no script/scene changes. Clips carry placeholder/slate-free audio
  only; no runtime, voice-quality or date claims.
- What I changed myself: n/a (agent-authored; no human edits yet).
- Teach-back check: pending - PR review with the owner.
- Test result: 28/28 output files verified against source WAV durations and
  recorded processed sha256 (sha256 list retained); all clips mono 24 kHz
  Vorbis. Listening/audition not possible here: voice quality UNVERIFIED.
