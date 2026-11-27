# 2026-11-27 contributor hooks H1 + H2

- Agent: instinct-side, requested so Abdelraouf's lane (Docs/ABDELRAOUF_CODE_PLAN.md) can persist state and add level content without editing shared files.
- H1: `SaveSystem.SaveData.contrib` (list of key/json entries) plus `SetContrib` / `GetContrib`. Old saves load with an empty list. Two EditMode tests (`SaveContribTests`).
- H2: `ILevelContribution` + `LevelContext` in `Scripts/Tools/Editor/SceneBuilderContrib.cs`; `BuildAll` runs every implementation by reflection on the final school scene, then saves it. Static root on LevelLayer, dynamic root on layer 0. A throwing module logs an error and the build continues.
- Not verified: nothing was compiled or run in Unity locally; CI is the only check. With zero implementations the hook is a no-op (returns before opening the scene).
