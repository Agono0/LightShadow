# 2026-11-27 — Unified ending cinematic: code hooks

`EndingCinematicController` (Scripts/UI) plays the single ending cinematic
from Docs/ENDING_VO_SPEC.md at the top of 03_Ending: 14 script lines
(EN + AR) with `VO_SN_Ending_XX` clip ids through VoiceoverPlayer, a plain
camera rail between builder-placed waypoints (no Cinemachine dependency),
per-shot line pacing, subtitle fallback when a clip is missing, and an
Esc/Space/Return skip. `EndingController` runs it before the dawn screen when
a rig is present (`HasRig`); with no waypoints the scene is untouched. The
camera's pose is restored afterwards so the dawn sequence is unaffected.
EditMode tests guard the line table (14 lines, unique ids, both languages,
speaker channels). Scene waypoints, the placeholder third-person rig and the
subtitle wiring land in the follow-up builder PR.
