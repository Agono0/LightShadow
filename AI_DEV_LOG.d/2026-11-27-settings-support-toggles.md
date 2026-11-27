# Settings toggles for sound captions and hints (2026-11-27)

- Settings page B gets two rows: "Sound captions" (right column, row 6) and "Hints" (left column, row 7), both On/Off.
- HintService gets a static Enabled flag (PlayerPrefs key abdelraouf.hints.enabled, default on) that gates the H key, the idle nudge and RequestHint. It was missing before.
- Idle detection now also reads the Horizontal and Vertical axes, so a gamepad stick counts as activity (known gap from the C2 hand-off).
- Scenes must be rebuilt through the SceneBuilder for the new rows to appear; nothing was rebuilt or seen in the editor.
- New labels "Sound captions" and "Hints" have no Arabic translation yet and fall back to English.
