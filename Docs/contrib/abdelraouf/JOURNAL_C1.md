# C1 lore journal (Abdelraouf lane)

Status: draft, AI-prepared on behalf of Abdelraouf Zidan (GitHub Abdo1590). Not compiled in Unity and not run in the editor or on a device. The 12 notes are draft text for narrative review. The Arabic is an AI draft and has not been checked by a native speaker.

## What it is

- `JournalNote` (IInteractable): a paper the player aims at and presses E on. Adds the note id to the journal.
- `JournalProgress`: static set of found note ids. Saved through contract H1 under the key `journal` (SetContrib on SaveCaptured, GetContrib on SaveApplied).
- `JournalBook`: a uGUI screen built in code. J opens and closes it. List of found notes on one side, selected note on the other, English or Arabic. Mobile builds get a small Journal button.
- `JournalLevelContribution` (contract H2): adds the journal system and three placeholder pedestals with notes note_01 to note_03 just past the start point of 02_School.
- `Data/Journal/journal_notes.json`: 12 notes (EN + AR), all draft.

## Contracts used (read from main at acdc928 / 2aeef8a)

- `LightShadow.Core.SaveSystem.SetContrib/GetContrib`, `SaveCaptured`, `SaveApplied`
- `LightShadow.Tools.ILevelContribution { Id, Order, Build(LevelContext) }`, `LevelContext`
- `LightShadow.Gameplay.IInteractable { Prompt, CanInteract, Interact(GameObject) }`
- `Localization.Current`, `Localization.Present`, `Localization.CurrentFont`, `GameManager.ShowMessage`, `PlayerController.ExternalInputLocked`, `PauseMenu.IsOpen`

## Known limits and requests for the owner's side

- Placement: only note_01 to note_03 are placed, on placeholder pedestals. Notes 4 to 12 are in the data but not in any scene. A level module (C4) or the owner's side should place them.
- Esc: this module does not consume Esc. If the journal is open and the player presses Esc, PauseMenu opens too. A shared-file change in PauseMenu would fix it, so it is a request, not done here.
- Arabic text is wrapped by hand in logical order before shaping, because ArabicSupport reverses each line. Shaping, joining and line breaks are unchecked on screen.
- The pedestal and paper use default primitive materials. No art assets added.
- The J key and journal strings are not in the shared Localization table. The journal ships its own EN and AR strings.
- No tests were added (the shared test folder is outside the lane, and a test file inside the lane has no assembly definition to reference the test framework).
