# 2026-11-27 — Ending cinematic rig (03_Ending)

SceneBuilderEndingCinematic builds the nine camera-rail shots from
Docs/ENDING_VO_SPEC.md over the existing dawn set, a placeholder third-person
Nour (capsule figure; bro's girl model swaps in at the controller's subject
reference), a wraith silhouette placeholder in a third-floor window (shot 6),
and the cinematic subtitle line on the ending canvas. EndingCinematicController
gained per-shot subject waypoints (subjectFrom/subjectTo) lerped alongside the
camera. All waypoints are plain transforms; framing and pacing are tuned by
moving transforms, not code. Placeholders are deliberate: real character model,
animations and VO come from bro's side per the spec.
