# 2026-11-27 - Audit fixes: Agono0 share (combined PR)

Scope: S1 bugs, F0 soft-lock, F24, key conflicts, world text, darkness, haptics,
plus folded peer inputs (Abdelraouf inventory/proximity text and hint recall,
Ahmed UI kit). Written by an AI agent. Nothing was compiled or run in the Unity
editor or on a device; CI is the first compile.

- Calm audio: DangerModel + AudioDirector scale SFX, ambience and ambient events
  by danger; quieter breathing (BreathingModel); default volume 8 -> 6;
  MUS_MemoryTheme one-shot removed; dead AdaptiveMusic removed.
- First-hit freeze: EnemyBehaviour catching watchdog + CatchRecovery; the catch
  cinematic call removed; EnemyHitPlayer returns bool.
- World text: depth-tested LightShadow/WorldText shader and
  Resources/ProximityText/DepthTestedTextMaterial.mat; WorldTextFader fades by
  distance and facing. Contrib ProximityText labels are skipped.
- Darkness: moon directional and moon/window/garden fills removed; night exposure
  lowered; scene validation now checks light budget (no directional, <=90 points).
- Keys: medkit H -> M (H is hints); F is owned by a carried prop (throw), not
  torch toggle/crank, for that press.
- F0: map board and pins moved inside the staff room (z 25.78); spare staff_key
  on the infirmary and library shelves; Contrib/Usef SchoolLevelModel updated to current builders (1.2) with a soft-lock test.
- F24: StoryDirector maps offered slot to the real choice index.
- Haptics opt-out preference (HapticsService.Enabled); settings toggle not yet wired.
- Saves: atomic write with .bak fallback (AtomicFile); player health persisted via contrib key "player_health"
  (old saves restore full health); New Game resets story + hint log.
- Localization: removed double shaping of pickup/reward/solve messages, door lock template, unlock template,
  quiz answer line shaped once, typewriter shaped per word, degree sign LTR, puzzle key hints kept in Arabic, 2 mistranslations
  fixed, 8 new Arabic entries (AI-drafted by a peer, NOT native-reviewed).
- Systems/audio: cinematic type lookup cached, pool double-release guard, dawn state cleared per run, ending music on
  the Music group, voiceover on the Voice category and loaded per clip, scene scan no longer re-registers sources,
  StoryTeller speech process cancelled on Stop/interrupt, crank decorative accumulator removed.
- Inventory: medkit use handler registered, BAG touch button, fuse description made honest (no progression use).
- Story: linked-node loop guard is per conversation (2.2).
- Torch: beam originates at the torch lens in the hand (SceneBuilderPrefabs). Takes effect when the builder
  regenerates the Player prefab; not seen in the editor.
- Folded peer inputs (each mirrored to mirror/* at the exact head): Ahmed ui-fixes-input, hud-fixes-input,
  ui-tests-input, settings-tests-input, audio-assets-input (AMB_NightWindDrips, UNWIRED, not auditioned);
  Abdelraouf overlay-locks, overlay-esc-fix, quiz-factcheck-input (report only; no factSource promoted).
- Packages: unused packages dropped from manifest.json; packages-lock.json not edited.
- Not done here (open): settings sliders for Voice/Shadow, kit skin wiring, save restore by identity, enemy state in saves,
  "Not done here:" auto-rotation and activeInputHandler left as is (landscape-only
  autorotation, both input systems in use); 132/133 quiz questions still need a
  teacher fact-check, so no release-readiness claim; Arabic strings are not
  native-verified.
