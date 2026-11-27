# 2026-11-27 GhostSkull wraith: glTFast + WraithAnimator

NOT SEEN IN THE EDITOR. Scale, pivot, clip names and look are unverified; nothing here has run in Unity except CI.

Facts checked on main: SM_Wraith_GhostSkull.glb (blob ae565c43, from #65) holds clips Death 0.667 s, Fast_Flying 0.833, Flying_Idle 1.167, Headbutt 1.167, HitReact 0.458, No/Punch/Yes 1.167, named "CharacterArmature|<name>". Unity could not import .glb at all (no glTF package, bare .meta), and the enemy had no animation code.

Changes
- Packages: com.unity.cloud.gltfast 6.21.0 is already in main (earlier commit), so no package change here; CI proves the import.
- Enemy/WraithAnimator.cs (+ meta, fresh GUID): reads EnemyBehaviour.CurrentState, cross-fades legacy clips found by name suffix. Patrol/Search = Flying_Idle; Stalk/Investigate/Chase = Fast_Flying; Ambush = Fast_Flying burst then one Headbutt; Repelled = HitReact. Missing clips are skipped.
- SceneBuilderPrefabs: BuildEnemyPrefab instantiates the model under Visual, auto-fits to 1.5 m tall hovering 0.35 m, hides the primitives and Shadow_Art. If the model does not load or has no renderers, the primitive wraith stays exactly as before.

Fallback: a failed import never breaks the build. Open risks: glTFast may import clips as non-legacy (WraithAnimator sets legacy at runtime), the model may face the wrong way, size and pivot are auto-fit guesses.
