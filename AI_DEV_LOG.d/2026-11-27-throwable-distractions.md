# Carry/throw props + distraction throws (2026-11-27)

From the user's answered questions (WhatsApp 13:01, item 17): throw/carry
objects and distraction throws.

- ThrowableProp (new): E picks up / sets down, F or the new THROW touch
  button hurls the prop forward. A rigidbody does the flight; the primitive
  keeps its collider and the root carries a small sphere for impacts.
- The distraction is POSITIONAL: EnemyBehaviour.HearNoiseAt reports where
  the prop landed, not where Nour stands (the player noise bus always
  reports at her feet), so a thrown bottle genuinely pulls the hunt away.
- Breakables (bottles, the lab beaker) shatter on a hard landing: louder
  noise, four fading shards, placeholder synth until bro's smash clip.
  Soft props (cups) thud quietly and can be rethrown.
- E never double-fires: while a prop is carried the InteractionRaycaster
  defers, and E puts the prop down.
- Five props placed: library bottle, infirmary cup, corridor bottle,
  physics-lab beaker, staff-room cup.
- Breakable/movable furniture-scale objects (item 18) build on this next.
