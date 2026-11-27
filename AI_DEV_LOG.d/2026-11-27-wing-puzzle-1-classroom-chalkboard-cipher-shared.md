### 2026-11-27 — Wing puzzle 1: classroom chalkboard cipher + shared code-lock UI — Abdelrhman Mohamed
- Tool used: Instinct (AI coding agent)
- What AI provided: `CodeLockUIController` (digit-lock overlay: keyboard 0-9/Enter/Esc on
  desktop, +/- wheels on mobile, holds the story input lock while open, EN+AR), the
  `ChalkboardCipherPuzzle` (PuzzleBase subclass: chalk sums on the classroom board hide one
  digit each, the 4-digit code 6371 opens the teacher's desk lock; wrong codes squeal and cost
  Bright Idea through the framework's WrongAttempt), the `SceneBuilderUIPuzzles`/
  `SceneBuilderPuzzles` partials (code-lock panel built in code; cipher prop placed in
  BuildClassroom with chalk TextMesh, sliding drawer, and a battery payout that rides out with
  the drawer), `PuzzleManager` added to the school Systems object, and hand-authored fresh-GUID
  .meta files. SceneBuilderUI was made `partial` (one keyword) to host the new panel partial.
- What I changed myself: pending review — changes land through the PR workflow (AGENTS.md rule 9).
- Teach-back check: pending — PR body explains why the reward is parented to the puzzle root
  (the drawer is a scaled cube and would squash a child) and why the panel self-registers a
  static Instance instead of a GameManager field (zero wiring, same pattern as
  InteractionRaycaster).
- Test result: pending — CI compile + builds are the gate; no local Unity editor. Verified on
  paper: reward collider clears the open drawer for the interaction ray; drawer hangs below the
  open-frame teacher desk top (y 0.73-0.83 slab).
