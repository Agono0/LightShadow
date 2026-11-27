# 2026-11-27 — Level Logic Kit (Package 8)

Adds Assets/_Project/Contrib/Usef/LevelLogic/ (own asmdef, pure C#, string ids, Editor
tests in its Tests/ subfolder) and Docs/contrib/usef/LEVELLOGIC.md. A read-only model of
the 02_School key/puzzle/gate chain and a validator for it. No Scripts, scene, prefab,
ProjectSettings, Packages, workflow or Contrib/Abdelraouf change. Nothing in the game uses it.

Model source: Agono0/LightShadow main at c3b4604bb83c038d5e0e410f526584d633ec1dd0, read by
hand (builders and gameplay scripts listed in the doc). Two room gates are inferred from
coordinates and flagged Inferred.

"Completable" applies to the modeled graph only, not to runtime reachability.

Unverified: not compiled or run in Unity (no editor or compiler available to the author).
CI is the first real run. The asmdef setup (standalone package asmdef plus Editor test asmdef
with nunit reference) follows the standard Unity layout but has not been seen compiling in
this project; if CI fails on it, the fix is in the asmdef files only.
