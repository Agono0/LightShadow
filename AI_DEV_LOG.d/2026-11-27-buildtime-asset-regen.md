# 2026-11-27 — CI builds regenerate scenes + audio library (important fix)

Found while wiring bro's wave-2 audio: the committed .unity scenes were last
regenerated in #7, and CI builds players straight from them — every
SceneBuilder-side change since (doors/keys, enemy roam, torch relight, UI
retheme, throwables, crates, the ending cinematic rig) was NOT in the CI
artifacts. Same class of problem for AudioLibrary.asset: bro's new clips
never reach the SfxRegistry without serialized reference edits.

Fix, both halves:
- RunAndroidAbiBatch / BuildWindowsBatch now call BuildAll() before
  packaging, so every CI player build regenerates the scenes from the
  builder code. Local menu builds already did this.
- New AudioLibrarySync (editor): fills AudioLibrary arrays/singles from the
  SFX folder by naming convention (SFX_<Family>_<n>, SFX_<Single>) — bro's
  clip drops wire themselves. Runs inside BuildAll() and from the
  LightShadow/Sync Audio Library menu item. AudioLibrary gained
  footstepGravel/Leaves/Mud + breathTired; bootstrap registers them.

The scene-runtime wiring for the new surfaces/breath (Footsteps.cs,
PlayerBreathing.cs) is a separate PR stacked on the feature chain, since
those files' current versions live there.
