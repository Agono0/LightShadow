# Abdelraouf — starting work packages (LightShadow)

Welcome. Coordination is repo-only and follows Docs/EXTERNAL_AGENT_BRIEF.md:
**you OPEN pull requests, nothing more — only the user's own side can merge.**
Never push to `main`, never force-push. Claim each package in
`Docs/WORK_CLAIMS.md` (one row: date, name, scope, files, status) before
starting; first claim wins. Every PR carries an `AI_DEV_LOG.d/<date>-<slug>.md`
fragment. Required checks must be green. No deploys, releases, spending, or
changes to `ProjectSettings/`, `Packages/`, `.github/`.

Taken lanes you must not touch: `Assets/_Project/Scripts/**` (ours),
`Audio/ Models/ Materials/ Textures/ Prefabs/ Art/` (bro's),
`Assets/_Project/Tests/Editor/` and `Assets/_Project/Data/Questions/`
(Yusuf's), and the redesign streams already in flight (health/checkpoints,
doors/keys, enemy AI, torch, UI retheme, stamina/footsteps, throwables,
pushables, ending cinematic, puzzle rewards, audio wiring).

Pick your starter pair first (packages 1 and 2), then choose from 3-5.
One PR per package, one package at a time.

## Package 1 — Native-Arabic review (starter)
- **Scope:** report (no string or asset changes). Review the shipped Arabic:
  the `Arabic` table in `Scripts/Core/Localization.cs`, `StoryStrings.cs`,
  the story nodes' AR fields in `Data/Story/SN_*.asset`, the suggested Arabic
  in `Docs/reviews/localization-audit-2026-11-27.md` (section 2), and the 12
  draft questions (`Q_Geo_019+`, all `NEEDS-FACT-CHECK`). Flag grammar,
  dialect vs MSA choices, unnatural phrasing, and anything a child player
  would misread.
- **Files/areas:** report at `Docs/reviews/arabic-review-<date>.md`.
- **Done when:** every finding has file + line + current text + your
  correction, so the fix PR (ours) is mechanical.
- **Risk:** low. Trap: editing the strings yourself — that crosses into
  other lanes; report only.

## Package 2 — On-device playtest report (starter)
- **Scope:** install the latest CI Android artifact (Actions → latest green
  build on `main` → artifacts), play from the main menu through the school
  to the ending, and report. Bugs, crashes, unreadable text, touch-target
  problems, performance feel, anything confusing. Repro steps per finding.
- **Files/areas:** report at `Docs/reviews/playtest-<date>.md`. No repo
  files change besides the report and your dev-log fragment.
- **Done when:** the report covers menu, settings, checkpoint/save flow,
  at least one puzzle, and the ending, with device model + Android version.
- **Risk:** low. This is the project's biggest gap: nothing is
  device-verified yet. Honest "I could not test X" beats a guess.

## Package 3 — Narrative continuity audit
- **Scope:** report only. Walk the story graph in `Data/Story/SN_*.asset`
  and `Scripts/Story/`: dangling or unreachable nodes, flag set/read
  mismatches, choice text that contradicts earlier outcomes, ending
  triggers vs `GameManager` state.
- **Files/areas:** report at `Docs/reviews/narrative-audit-<date>.md`.
- **Done when:** each issue names the node ids involved and the conflict.
- **Risk:** low.

## Package 4 — Balance and tuning survey
- **Scope:** report only. Read the tuning constants from source (battery
  drain rates, enemy speeds, health and medkit economy, stamina costs,
  throwable counts) and tabulate them against the intended difficulty in
  the design docs. Recommend values; do not change them.
- **Files/areas:** report at `Docs/reviews/balance-<date>.md`.
- **Done when:** one table of current values with file/line, plus a
  recommended-values column with one line of reasoning each.
- **Risk:** low.

## Package 5 — Player-facing docs
- **Scope:** markdown only. A player README quickstart (what the game is,
  controls on touch and keyboard, how saves work), plus a short FAQ.
  Tone: warm, simple, no spoilers past the first scene.
- **Files/areas:** `Docs/PLAYER_GUIDE.md` (new), README section if needed.
- **Done when:** a new player can install, start, and understand the loop
  without asking anyone.
- **Risk:** low.
