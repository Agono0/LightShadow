# Level Logic Kit (Package 8, UsefBebars)

A read-only, self-contained model of the key/puzzle/gate chain in 02_School, plus a
validator that answers one question about that model: can the goal be reached, and
which steps are single points of failure? It does not read or change the scene, and
nothing in the game reads it.

Folder: `Assets/_Project/Contrib/Usef/LevelLogic/` (own asmdef
`LightShadow.Contrib.Usef.LevelLogic`, `noEngineReferences`, `autoReferenced: false`, no
reference to Assembly-CSharp). Tests live in its `Tests/` subfolder (Editor-only asmdef).

## What "completable" means here

`GoalReachable` is true when a forward simulation of the MODELED graph reaches the goal
token `goal:gate_powered`. It is **not** a claim that the running game is completable:
the model is hand-written from code, it knows nothing about walls, NavMesh, colliders,
UI, quizzes or runtime bugs, and it goes stale when the game changes.

## Model source and provenance

Read by hand from Agono0/LightShadow `main` at commit
`c3b4604bb83c038d5e0e410f526584d633ec1dd0` (merged code only, not open PRs):

| File | What was read |
| --- | --- |
| `Scripts/Tools/Editor/SceneBuilderDoors.cs` | 6 wing doors (2 locked: `classroom_key`, `staff_key`), key pickups |
| `Scripts/Tools/Editor/SceneBuilderPuzzles.cs` | 7 puzzle names, reward pickups (4 batteries, `fuse`, `master_key`) |
| `Scripts/Gameplay/Puzzles/GateCircuitPuzzle.cs` | needs CanPowerGate and (`master_key` or MapLocker solved) |
| `Scripts/Gameplay/ExitGate.cs`, `FuseBox.cs`, `Core/GameManager.cs` | gate routes through the circuit puzzle; 3 fuse boxes, no item needed |

Each step in `SchoolLevelModel.cs` carries its source file and a Verified or Inferred flag.

## Result at that commit

Goal reachable. One valid order: pickups, `Key_Staff` -> `Door_Staff` -> `Puzzle_MapLocker`
(master key) and the three fuse boxes -> `Puzzle_GateCircuit` -> `ExitGate`.

Single points of failure (removing any one blocks the goal): `Key_Staff`, `Door_Staff`,
`Puzzle_MapLocker`, `Fuse_Library`, `Fuse_Physics`, `Fuse_MainHall`, `Puzzle_GateCircuit`,
`ExitGate`.

Observations (facts about the model, not recommendations):
- The `fuse` item from `Puzzle_FirstAidOrder` and the `battery` rewards are not required by
  any modeled step. FuseBox has no item requirement at this commit.
- `Key_Classroom`, `Door_Classroom` and `Puzzle_ChalkboardCipher` are off the critical path.
- Fragments (6) are not required by the gate.

## Coverage limits

- Room gating for `Puzzle_ChalkboardCipher` and `Puzzle_MapLocker` is **Inferred** from world
  coordinates versus door positions, not from a code rule. Where each key pickup sits is not
  modeled.
- Not modeled: quiz boards, knowledge fragments, medkits and health, the checkpoint system,
  the enemy, stamina, throwables, pushables, the ending cinematic, and unmerged work.
- The redesign streams (doors/keys, puzzle rewards, enemy AI) are in flight. When a stream
  lands, this model needs a manual update.
- A soft drift check in the tests logs a warning (it never fails the run) when a modeled
  name is missing from the scene-builder source.

## Integration needs

None to merge. For later use by the owners: call `LevelGraphValidator.Validate(
SchoolLevelModel.Build())` from a test or editor tool; to adopt it, they would add an asmdef
reference from their side. This package adds no scene object and no menu entry, and does not
use the `ILevelContribution` hook.

## Verification status

AI-prepared. Not compiled or run in Unity locally (no editor or C# compiler available to the
author). CI fast-checks, test-editmode and the player builds are the first real run.
