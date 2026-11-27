### 2026-11-27 — VO wiring: clips play from the story director (plan phase 8) — Abdelrhman Mohamed
- Tool used: Instinct (AI coding agent)
- What AI provided: `VoiceoverPlayer` (Resources lookup `VO/<locale>/<lineId>`, locale from
  the current language, misses cached, narrator toggle gates it), an `AudioManager.voiceSource`
  channel (`PlayVoice` interrupts itself, one line at a time), the school-scene VoiceSource
  wiring, and the story hooks: a node plays `VO_<nodeId>` with SAPI as the fallback when no
  clip exists, a picked choice plays its `VO_<nodeId>_C<n>` inner-voice line (silence when
  unrecorded — the player already read what they tapped), and EndConversation stops the voice.
  Clips moved `Assets/_Project/Audio/VO` -> `Assets/_Project/Resources/VO` for runtime loading.
- What I changed myself: pending review — lands through the PR workflow (AGENTS.md rule 9).
- Teach-back check: pending — PR body explains the C<n> index mapping (raw choice index + 1,
  matching the sheet's "asset order") and why choice misses stay silent while node misses fall
  back to SAPI (a missing node line means no voice at all otherwise; a missing choice line
  still has its text on screen).
- Test result: pending — CI compile + builds are the gate; no local Unity editor. Verified on
  paper: NarratorEnabled gates both paths; locale switch clears the clip cache; Resources move
  keeps every .meta (no GUID churn).

Amendment (bro review + reverb zone spec): choice lines (VO_*_C<n>) are
unwired per the recording spec - node lines only are voiced; the generated
choice clips stay in Resources as an optional layer. Voice source now runs
dry (`reverbZoneMix = 0`) so VO reads as inner voice inside reverb zones;
music/SFX keep the default wet mix.
