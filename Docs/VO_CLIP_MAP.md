# LIGHT SHADOW - VO clip map (generated voice, docs only)

Status: DOCUMENTATION ONLY. No script, scene, prefab or AudioLibrary changes by
this PR; playback wiring is the lead's work. This doc maps the voice line ids
from `Docs/VO_LINE_SHEET.md` (PR #16) to the generated clip files in PR #28
(head 07781623f33437aff11639c9cba37ea13ba678db), and records the agreed usage
policy.

Update (2026-11-27): the generated voice is ACCEPTED for this batch. Ahmed
(bro's owner) confirmed via the lead that human recording is no longer a gate
for shipping these lines. The policy below is updated accordingly.

## Policy

- All 154 clips (77 EN + 77 AR) are GENERATED synthetic voice. They are
  accepted as the shipped voice for this batch (Ahmed, 2026-11-27, via the
  lead). Any UI, doc or credit that mentions them must say "generated" or
  "synthetic voice".
- Human recordings are a possible FUTURE replacement only: not scheduled, not
  promised, and no longer a release gate. `Docs/VO_RECORDING_SPECS.md` stays
  in the repo as the ready-made briefing IF such a session ever happens.
- Choice-variant clips (`_C1`/`_C2`/`_C3` suffixes) stay in the repo UNWIRED
  and optional, per the recording spec. They exist so the folder layout
  already matches the line sheet; the lead decides if they are ever
  referenced.
- Story-node clips are live per the lead's playback mapping (#29 and its
  follow-ups); they are the shipped voice unless/until an optional future
  human session replaces them.
- Fallback stays the existing SAPI narrator for any line without a clip; the
  mapping data and fallback logic are the lead's scripts (`StoryTeller.cs`),
  not this doc's PR.

## Path status (settled)

PR #29 merged: the clips live at `Assets/_Project/Resources/VO/{EN,AR}/` for
Resources loading (moved from `Assets/_Project/Audio/VO/{EN,AR}/` in PR #28,
head 07781623f33437aff11639c9cba37ea13ba678db).

## Verification (2026-11-27, against PR #28 tree)

- 154 clip files total: 77 under `Assets/_Project/Resources/VO/EN/`, 77 under
  `Assets/_Project/Resources/VO/AR/`; the EN and AR id sets are identical.
- Every id below appears 1:1 in `Docs/VO_LINE_SHEET.md` at PR #16 head
  add8f905c8dcffb7f0e8c9641a6cf2cb2108bc1f (77/77 match, no missing, no extra).
- Naming convention: `VO_<nodeId>.ogg` for story node lines,
  `VO_<nodeId>_C<n>.ogg` for choice variants of that node.

## Map

| Clip id | EN path | AR path | Kind | Status |
| --- | --- | --- | --- | --- |
| VO_SN_Act1_01 | Assets/_Project/Resources/VO/EN/VO_SN_Act1_01.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_01.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_01_C1 | Assets/_Project/Resources/VO/EN/VO_SN_Act1_01_C1.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_01_C1.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_01_C2 | Assets/_Project/Resources/VO/EN/VO_SN_Act1_01_C2.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_01_C2.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_01_C3 | Assets/_Project/Resources/VO/EN/VO_SN_Act1_01_C3.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_01_C3.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_01b | Assets/_Project/Resources/VO/EN/VO_SN_Act1_01b.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_01b.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_02 | Assets/_Project/Resources/VO/EN/VO_SN_Act1_02.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_02.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_02_C1 | Assets/_Project/Resources/VO/EN/VO_SN_Act1_02_C1.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_02_C1.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_02_C2 | Assets/_Project/Resources/VO/EN/VO_SN_Act1_02_C2.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_02_C2.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_02_C3 | Assets/_Project/Resources/VO/EN/VO_SN_Act1_02_C3.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_02_C3.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_02b | Assets/_Project/Resources/VO/EN/VO_SN_Act1_02b.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_02b.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_02t | Assets/_Project/Resources/VO/EN/VO_SN_Act1_02t.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_02t.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_03 | Assets/_Project/Resources/VO/EN/VO_SN_Act1_03.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_03.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_03_C1 | Assets/_Project/Resources/VO/EN/VO_SN_Act1_03_C1.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_03_C1.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_03_C2 | Assets/_Project/Resources/VO/EN/VO_SN_Act1_03_C2.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_03_C2.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_03_C3 | Assets/_Project/Resources/VO/EN/VO_SN_Act1_03_C3.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_03_C3.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_03a | Assets/_Project/Resources/VO/EN/VO_SN_Act1_03a.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_03a.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_03b | Assets/_Project/Resources/VO/EN/VO_SN_Act1_03b.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_03b.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_04 | Assets/_Project/Resources/VO/EN/VO_SN_Act1_04.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_04.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_04_C1 | Assets/_Project/Resources/VO/EN/VO_SN_Act1_04_C1.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_04_C1.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_04_C2 | Assets/_Project/Resources/VO/EN/VO_SN_Act1_04_C2.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_04_C2.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_04b | Assets/_Project/Resources/VO/EN/VO_SN_Act1_04b.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_04b.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act1_05 | Assets/_Project/Resources/VO/EN/VO_SN_Act1_05.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act1_05.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_01 | Assets/_Project/Resources/VO/EN/VO_SN_Act2_01.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_01.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_01_C1 | Assets/_Project/Resources/VO/EN/VO_SN_Act2_01_C1.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_01_C1.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_01_C2 | Assets/_Project/Resources/VO/EN/VO_SN_Act2_01_C2.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_01_C2.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_01_C3 | Assets/_Project/Resources/VO/EN/VO_SN_Act2_01_C3.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_01_C3.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_01_anger | Assets/_Project/Resources/VO/EN/VO_SN_Act2_01_anger.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_01_anger.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_01_calm | Assets/_Project/Resources/VO/EN/VO_SN_Act2_01_calm.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_01_calm.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_01_pass | Assets/_Project/Resources/VO/EN/VO_SN_Act2_01_pass.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_01_pass.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_02 | Assets/_Project/Resources/VO/EN/VO_SN_Act2_02.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_02.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_02_C1 | Assets/_Project/Resources/VO/EN/VO_SN_Act2_02_C1.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_02_C1.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_02_C2 | Assets/_Project/Resources/VO/EN/VO_SN_Act2_02_C2.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_02_C2.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_02_C3 | Assets/_Project/Resources/VO/EN/VO_SN_Act2_02_C3.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_02_C3.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_02b | Assets/_Project/Resources/VO/EN/VO_SN_Act2_02b.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_02b.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_02t | Assets/_Project/Resources/VO/EN/VO_SN_Act2_02t.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_02t.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_03 | Assets/_Project/Resources/VO/EN/VO_SN_Act2_03.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_03.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_03_C1 | Assets/_Project/Resources/VO/EN/VO_SN_Act2_03_C1.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_03_C1.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_03_C2 | Assets/_Project/Resources/VO/EN/VO_SN_Act2_03_C2.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_03_C2.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_03b | Assets/_Project/Resources/VO/EN/VO_SN_Act2_03b.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_03b.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_04 | Assets/_Project/Resources/VO/EN/VO_SN_Act2_04.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_04.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_04_C1 | Assets/_Project/Resources/VO/EN/VO_SN_Act2_04_C1.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_04_C1.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_04_C2 | Assets/_Project/Resources/VO/EN/VO_SN_Act2_04_C2.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_04_C2.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_04_C3 | Assets/_Project/Resources/VO/EN/VO_SN_Act2_04_C3.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_04_C3.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_04_line | Assets/_Project/Resources/VO/EN/VO_SN_Act2_04_line.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_04_line.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_04_pass | Assets/_Project/Resources/VO/EN/VO_SN_Act2_04_pass.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_04_pass.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_04_shaken | Assets/_Project/Resources/VO/EN/VO_SN_Act2_04_shaken.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_04_shaken.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act2_04_stand | Assets/_Project/Resources/VO/EN/VO_SN_Act2_04_stand.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act2_04_stand.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_01 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_01.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_01.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_01_C1 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_01_C1.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_01_C1.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_01_C2 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_01_C2.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_01_C2.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_01_C3 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_01_C3.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_01_C3.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_01b | Assets/_Project/Resources/VO/EN/VO_SN_Act3_01b.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_01b.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_02 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_02.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_02.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_03 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_03.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_03.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_03_C1 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_03_C1.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_03_C1.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_03_C2 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_03_C2.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_03_C2.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_03_C3 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_03_C3.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_03_C3.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_03_bold | Assets/_Project/Resources/VO/EN/VO_SN_Act3_03_bold.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_03_bold.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_03_forgave | Assets/_Project/Resources/VO/EN/VO_SN_Act3_03_forgave.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_03_forgave.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_03_pass | Assets/_Project/Resources/VO/EN/VO_SN_Act3_03_pass.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_03_pass.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_03_shaken | Assets/_Project/Resources/VO/EN/VO_SN_Act3_03_shaken.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_03_shaken.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_04 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_04.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_04.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_04_C1 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_04_C1.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_04_C1.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_04_C2 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_04_C2.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_04_C2.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_04t | Assets/_Project/Resources/VO/EN/VO_SN_Act3_04t.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_04t.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_05 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_05.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_05.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_06 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_06.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_06.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_07 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_07.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_07.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_08 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_08.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_08.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_08_C1 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_08_C1.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_08_C1.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_08_C2 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_08_C2.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_08_C2.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_08_C3 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_08_C3.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_08_C3.ogg | story node line | generated placeholder - unwired |
| VO_SN_Act3_09 | Assets/_Project/Resources/VO/EN/VO_SN_Act3_09.ogg | Assets/_Project/Resources/VO/AR/VO_SN_Act3_09.ogg | story node line | generated placeholder - unwired |
| VO_SN_End_Dawnless | Assets/_Project/Resources/VO/EN/VO_SN_End_Dawnless.ogg | Assets/_Project/Resources/VO/AR/VO_SN_End_Dawnless.ogg | story node line | generated placeholder - unwired |
| VO_SN_End_FullRecovery | Assets/_Project/Resources/VO/EN/VO_SN_End_FullRecovery.ogg | Assets/_Project/Resources/VO/AR/VO_SN_End_FullRecovery.ogg | story node line | generated placeholder - unwired |
| VO_SN_End_HollowEscape | Assets/_Project/Resources/VO/EN/VO_SN_End_HollowEscape.ogg | Assets/_Project/Resources/VO/AR/VO_SN_End_HollowEscape.ogg | story node line | generated placeholder - unwired |
| VO_SN_End_PartialMemory | Assets/_Project/Resources/VO/EN/VO_SN_End_PartialMemory.ogg | Assets/_Project/Resources/VO/AR/VO_SN_End_PartialMemory.ogg | story node line | generated placeholder - unwired |
