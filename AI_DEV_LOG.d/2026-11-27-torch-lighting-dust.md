# Torch smoothing + sway, beam dust, decorative relight (2026-11-27)

From the user's answered questions (WhatsApp 13:01) and the 12:49 brief.

- Torch: smooth-edged cone (inner spot angle 48 of 62, so the falloff is a
  gradient), moderate 11m range kept.
- TorchSway (new, on the flashlight rig): walk bob from real ground speed,
  turn lag on the beam, strafe lean, soft-spring settle. Standing still is
  still.
- BeamDust (new): 40 subtle motes drifting up the beam cone, emission gated
  on the light so a dead torch shows no dust. Mobile and PC.
- Corridor relight ("map lights off, only decorations emit light"): the two
  steady emergency lamps and the doorway fill are gone. What remains is
  decorative: one faulty stuttering lamp (LightFlicker gains dropouts),
  two moonlit window patches, and three breathing candles (library,
  classroom, staff room). The entrance checkpoint and the fuse-controlled
  wing lights stay as designed - the wing lights are the progression reward.
- Battery empty now genuinely means near-darkness, with only the
  candle/moon/faulty-lamp glows reading.
