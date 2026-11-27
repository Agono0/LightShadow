### 2026-11-27 — Per-wing LIT color grading (plan §10 two lighting states) — Abdelrhman Mohamed
- Tool used: Instinct (AI coding agent)
- What AI provided: `SceneBuilderPostWing` partial with `BuildWingLitProfile` (a shared warm
  ColorAdjustments-only volume profile, rebuilt every run like the night/dawn profiles) and
  `AddWingLitGrade` (a local Volume + trigger BoxCollider, Default layer, priority 2 above the
  global night grade, 3 m blend), the committed `Post_WingLit.asset` shell matching how
  Post_Night/Post_Dawn are committed (editor code repopulates it at build time), and four
  volume placements bounded to the LitZone extents of Library, Physics, Main Hall and Staff
  (the staff room shares the hall's fuse group, so it gets its own child volume). Volumes ride
  the fuse-controlled group active state: no runtime code, no save handling.
- What I changed myself: pending review — changes land through the PR workflow (AGENTS.md rule 9).
- Teach-back check: pending — PR body explains why only ColorAdjustments overrides (URP blends
  per-property, so global bloom/vignette/grain carry through) and why the volumes need no
  FuseBox changes (they activate with the lights group).
- Test result: pending — CI compile + builds are the gate; no local Unity editor. Verified on
  paper: volume bounds copied from the wing LitZone position+center+size; priority 2 sits above
  the global night volume's 1; Default layer is inside every camera volume mask.
