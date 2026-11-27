# Work Claims

One row per claimed unit of work. Append in your PR before starting
(first claim wins). Status: claimed / in-review / done. See
Docs/EXTERNAL_AGENT_BRIEF.md for the protocol and the standing lane rules.

| date | agent | scope | files/areas | status |
| --- | --- | --- | --- | --- |
| 2026-11-27 | instinct-side | Redesign chain (health, doors/keys, enemy AI, torch, UI, stamina, throwables, pushables, ending cinematic, puzzle rewards, audio wiring) + CI/scene-regen + bump bot | Assets/_Project/Scripts/**, ProjectSettings/, Packages/, .github/ | claimed |
| 2026-11-27 | bro-side | Wave-2 audio, wraith model (SM_Wraith_GhostSkull), girl model + animations, ending VO recordings | Audio/, Models/, Materials/, Textures/, Prefabs/, Art/, register rows | claimed |
| 2026-11-27 | instinct-agent (UsefBebars) | Save-system robustness audit report (docs only, no code) | Docs/AUDIT_SAVE_SYSTEM.md, AI_DEV_LOG.d/ | in-review |
| 2026-11-27 | instinct-agent (UsefBebars) | Package 1: EditMode test expansion (test files only) | Assets/_Project/Tests/Editor/ | claimed |
| 2026-11-27 | instinct-agent (UsefBebars) | Package 2: Arabic/English localization coverage audit (report only) | Docs/reviews/localization-audit-2026-11-27.md | claimed |
| 2026-11-27 | instinct-agent (UsefBebars) | Package 3: performance review (report only) | Docs/reviews/performance-review-2026-11-27.md | claimed |
| 2026-11-27 | instinct-agent (UsefBebars) | Package 4: accessibility pass (report only) | Docs/reviews/accessibility-2026-11-27.md | claimed |
| 2026-11-27 | instinct-agent (UsefBebars) | Package 5: question bank expansion (content only, draft-marked) | Assets/_Project/Data/Questions/ (new Q_*.asset files) | claimed |
| 2026-11-27 | instinct-agent (UsefBebars) | Package 8: Level Logic Kit (read-only model + validator of the key/puzzle/gate chain; self-contained string ids; AI-prepared, not Unity-compiled) | Assets/_Project/Contrib/Usef/LevelLogic/, Docs/contrib/usef/LEVELLOGIC.md, AI_DEV_LOG.d/ | in-review |
| 2026-11-27 | abdelraouf-agent (Abdo1590) | Package 1: native-Arabic review of shipped Arabic (report only, AI-prepared, no native sign-off yet) | Docs/reviews/arabic-review-abdelraouf.md, AI_DEV_LOG.d/ | native review pending |
| 2026-11-27 | abdelraouf-agent (Abdo1590) | Package C1: lore journal (code, UI, 12 draft notes; AI-prepared, not Unity-compiled) | Assets/_Project/Contrib/Abdelraouf/, Docs/contrib/abdelraouf/JOURNAL_C1.md, AI_DEV_LOG.d/ | merged to main via #89 (mirror of his fork PR); not wired into the game yet, not seen in editor |
| 2026-11-27 | instinct-agent (UsefBebars) | Package 7: EditMode test expansion round 2 (test files only: PuzzleBase, PuzzleManager, RunResults, GameConfig sanity) | Assets/_Project/Tests/Editor/, AI_DEV_LOG.d/ | in-review |
