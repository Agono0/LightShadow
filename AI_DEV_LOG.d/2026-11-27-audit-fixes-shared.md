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
- Not done here: auto-rotation and activeInputHandler left as is (landscape-only
  autorotation, both input systems in use); 132/133 quiz questions still need a
  teacher fact-check, so no release-readiness claim; Arabic strings are not
  native-verified.
