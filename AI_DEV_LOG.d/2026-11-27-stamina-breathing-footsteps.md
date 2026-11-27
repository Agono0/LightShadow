# Stamina sprint, labored breathing, surface footsteps (2026-11-27)

From the user's answered questions (WhatsApp 13:01, items 3-4): stamina
sprint with a better controller overall; labored breathing when running or
scared; surface-dependent footsteps.

- PlayerController: about 5s of sprint, then she is winded until ~45%
  recovered (no bar by design - the breathing is the tell). Planar velocity
  now eases toward its target, so starts and stops stop feeling binary.
- PlayerBreathing (new, on the rig): a synthesized shaped-noise breath loop
  whose volume and rate follow sprint exertion and wraith proximity (14m);
  calm is silent. Placeholder until bro's recorded breathing ships.
- Footsteps: raycasts the floor material - garden grass swallows the step
  (soft low-volume wood-set approximation until a grass pack ships); indoors
  stays the concrete/wood mix.
- Still needing bro (relayed through main): the girl's 3D model, the full
  animation set, the visible body/hands and third-person cinematics - code
  hooks are ready for them.
