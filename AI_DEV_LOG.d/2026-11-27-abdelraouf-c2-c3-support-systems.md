# 2026-11-27 C2 hints + C3 sound captions (Abdelraouf, code packages C2 and C3)

- What: adds a hint service (3 levels per puzzle, EN+AR) and a sound caption service (`CaptionService.Raise(key, pos)`, EN+AR, direction marker, fade, queue limit) under `Assets/_Project/Contrib/Abdelraouf/`, an H2 module `abdelraouf.support-systems`, two data files, one note `Docs/contrib/abdelraouf/SUPPORT_SYSTEMS.md`, and two rows in `Docs/WORK_CLAIMS.md` (C2, C3).
- Who: written by an AI assistant working for Abdelraouf Zidan (GitHub Abdo1590) under `Docs/ABDELRAOUF_CODE_PLAN.md`. Abdelraouf has not read the code. Arabic is an AI draft with no native check.
- Why: packages C2 and C3 of the contributor plan.
- This is infrastructure only. Accessibility finding 1 stays open until the owner's side calls `CaptionService.Raise` from real audio sites and the result is verified in a run. Simulated cues do not prove live sound coverage.
- Verified: signatures read from main at 0701df6 (`PuzzleBase.puzzleId/State`, `SaveSystem.SetContrib/GetContrib`, `GameEvents.PuzzleSolved`, `ILevelContribution`). Puzzle ids confirmed as object names from `SceneBuilderPuzzles.cs`. Each new `.cs` has a hand-written `.meta` with a GUID grepped as unused. Both JSON files parse. Brace balance checked by script. No shared Scripts, scene or prefab file touched.
- Unverified: NOT compiled in Unity. No editor run, scene build or device run. UI layout, Arabic shaping, scene hook, puzzle discovery, save round trip and real audio coverage are all unchecked. Green CI does not prove them.
- Debug test rig is editor and development builds only, off by default, opt-in through a PlayerPrefs key, and never touches shipping gameplay.
- Requests to the owner's side: wire `CaptionService.Raise` at the audio call sites; read `HintService.HintsUsed` for the run summary; fix the stale C1 row in `Docs/WORK_CLAIMS.md` (not edited here).
