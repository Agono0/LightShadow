### 2026-11-27 — Wing puzzle 4: staff-room map and locker (plan workstream 5) — Abdelrhman Mohamed
- Tool used: Instinct (AI coding agent)
- What AI provided: `MapLockerPuzzle` (six numbered country pins on a wall map plus a route
  note - Cairo, Beirut, Baghdad, Rabat - give the 4-digit code 4729 on the middle locker;
  solving slides the locker door aside and reveals the master key; wrong codes run
  WrongAttempt), reusing the shared `CodeLockUIController` overlay with the note as the
  sequence text in English and Arabic, and the staff-room placement (map board + pins on the
  north wall, door and lock plate on Lockers_2, a placeholder key prop until bro's model
  batch lands).
- What I changed myself: pending review — changes land through the PR workflow (AGENTS.md rule 9).
- Teach-back check: pending — PR body explains why only the door moves on solve (the key is
  inside the locker and is revealed, not carried) and why country labels stay in English as
  diegetic map text while the lock UI is fully translated.
- Test result: pending — CI compile + builds are the gate; no local Unity editor. Verified on
  paper: code 4729 = Egypt 4, Lebanon 7, Iraq 2, Morocco 9 in the note's route order; the
  door offset (0,0,0.75) slides the 0.7-deep door clear of the locker front; save restore
  reopens the door without replaying events or rewards.
