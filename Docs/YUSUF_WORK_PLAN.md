# Yusuf — starting work packages (LightShadow)

Welcome. Coordination is repo-only and follows Docs/EXTERNAL_AGENT_BRIEF.md:
**you OPEN pull requests, nothing more — only the user's own side can merge.**
Never push to `main`, never force-push. Claim each package in
`Docs/WORK_CLAIMS.md` (one row: date, name, scope, files, status) before
starting; first claim wins. Every PR carries an `AI_DEV_LOG.d/<date>-<slug>.md`
fragment. Required checks must be green. No deploys, releases, spending, or
changes to `ProjectSettings/`, `Packages/`, `.github/`.

Taken lanes you must not touch: `Assets/_Project/Scripts/**` (ours),
`Audio/ Models/ Materials/ Textures/ Prefabs/ Art/` (bro's), and the redesign
streams already in flight (health/checkpoints, doors/keys, enemy AI, torch,
UI retheme, stamina/footsteps, throwables, pushables, ending cinematic,
puzzle rewards, audio wiring).

## Package 1 — EditMode test expansion
- **Scope:** grow `Assets/_Project/Tests/Editor/` with tests for logic that
  lacks them: save/load round-trips (SaveSystem blobs), inventory edge cases,
  quiz/question validation, story flag math. Test files only — if a test
  reveals a production bug, report it in the PR body, do NOT fix `Scripts/`.
- **Files/areas:** `Assets/_Project/Tests/Editor/` (new files + their `.meta`).
- **Done when:** new tests pass in CI (test-editmode), zero production files
  touched, fragment explains coverage added.
- **Risk:** low. Main trap: editing production code "to make a test pass" —
  that crosses into our lane; report instead.

## Package 2 — Arabic/English localization coverage audit
- **Scope:** report (no code changes). Compare the English keys in
  `Localization.cs` against the Arabic dictionary, the story nodes' AR fields,
  and in-world text; list every missing or mismatched entry.
- **Files/areas:** report at `Docs/reviews/localization-audit-<date>.md`;
  read-only over `Scripts/Core/Localization.cs`, `Data/Story/`, builder text.
- **Done when:** every gap listed with file + line + suggested Arabic, so the
  fix PR is mechanical afterwards.
- **Risk:** low. Trap: fixing strings yourself — that is our lane; report only.

## Package 3 — Performance review report
- **Scope:** static analysis, report only. Count generated-geometry draw
  calls/tris from the SceneBuilder code, light counts per scene, Update-loop
  allocations in runtime code, audio clip memory settings vs the manifest
  targets. Prioritize findings by player impact.
- **Files/areas:** report at `Docs/reviews/performance-review-<date>.md`.
- **Done when:** a prioritized findings list with file/line references and
  concrete recommended values (not applied).
- **Risk:** low. Trap: estimates presented as measurements — label which is
  which honestly.

## Package 4 — Accessibility pass report
- **Scope:** compute text/background contrast ratios from the palette colors
  in the builder code, check minimum text sizes, subtitle coverage (incl. the
  ending cinematic spec), and readability for Arabic glyphs; compare against
  `AccessibilityStore` settings.
- **Files/areas:** report at `Docs/reviews/accessibility-<date>.md`.
- **Done when:** findings with measured ratios/sizes + recommended fixes.
- **Risk:** low.

## Package 5 — Question bank expansion (content)
- **Scope:** new curriculum-faithful quiz questions as
  `Assets/_Project/Data/Questions/Q_<topic>_<nnn>.asset`, following the exact
  field pattern of the existing `Q_Geo_*.asset` files (full Arabic fields,
  explanation + explanationAr, draft-marked for the human fact-check).
- **Files/areas:** `Assets/_Project/Data/Questions/` only.
- **Done when:** CI validation passes (the bank integrity check), every
  question carries both languages and an explanation, fragment notes sources.
- **Risk:** medium — factual accuracy. Every fact must be checkable; mark
  anything uncertain as draft with a note. Do not edit the question database
  code or existing questions.

## Package 6 — Save-system robustness audit
- **Scope:** report only. Read `SaveSystem` + every `*Json` blob writer/reader
  (story, puzzle, inventory, settings) and map what happens on: old saves
  missing new fields, corrupted JSON, version bumps, mid-scene restore.
- **Files/areas:** report at `Docs/reviews/save-robustness-<date>.md`.
- **Done when:** a table of save fields × failure modes × current behavior,
  with the worst gaps ranked.
- **Risk:** low. Pairs well with Package 1 (its findings become test cases).
