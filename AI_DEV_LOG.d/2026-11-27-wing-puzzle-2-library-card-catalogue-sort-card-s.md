### 2026-11-27 — Wing puzzle 2: library card catalogue sort + card-sort UI — Abdelrhman Mohamed
- Tool used: Instinct (AI coding agent)
- What AI provided: `CardSortUIController` (one clue card at a time, four drawer buttons,
  keys 1-4 on desktop, input lock + cursor handling like the quiz/code-lock overlays, EN+AR,
  self-registered static Instance), the `CatalogueSortPuzzle` (PuzzleBase subclass: four books
  to file into Poetry/Stories/History/Science; a wrong drawer rattles and runs the framework
  WrongAttempt; filing all four drops the return tray with a key card and a battery), the
  `BuildCardSortPanel` editor builder in `SceneBuilderUIPuzzles`, the `CreateCatalogueSort`
  placement in `SceneBuilderPuzzles` (cabinet against the library west wall, four drawer
  fronts, sliding return tray, battery payout parented to the puzzle root so it slides out
  with the tray without being squashed by the tray's scaled-cube transform), the placement
  call in `BuildLibrary`, and fresh-GUID .meta files for both new scripts.
- What I changed myself: pending review — changes land through the PR workflow (AGENTS.md rule 9).
- Teach-back check: pending — PR body explains the card content choice (four canonical Arabic
  works across the four school subjects, per the Language/History wing theme) and why drawer
  button labels are set at runtime instead of LocalizedText (they are dynamic per puzzle).
- Test result: pending — CI compile + builds are the gate; no local Unity editor. Verified on
  paper: tray and battery share the puzzle root so one offset slides both; the battery collider
  clears the open tray for the interaction ray; drawer fronts sit proud of the cabinet face.
