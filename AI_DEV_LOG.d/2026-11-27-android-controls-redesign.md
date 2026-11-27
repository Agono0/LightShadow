# Android controls redesign (2026-11-27)

From the user's 12:49 brief: bigger joystick, round semi-transparent buttons,
and a size setting.

- Joystick grows to a 300px pad with a 112px throw and a 124px handle.
- All eight touch buttons are now round (builtin Knob sprite, no slicing) and
  semi-transparent, so they stop reading as desktop UI: E/LIGHT at 124px,
  RUN/CROUCH at 104px, and a TASKS / PAUSE / CRANK / BREATH utility row at
  84px along the bottom edge.
- New Settings row "Touch control size" (page B, hidden on desktop) cycles
  85% / 100% / 120% / 140%, persisted as LS_ControlSize.
- New TouchControlsScaler on the mobile root captures the builder layout as
  the 100% baseline and re-scales every button, the joystick pad, handle,
  throw radius and corner offsets whenever the setting changes.
