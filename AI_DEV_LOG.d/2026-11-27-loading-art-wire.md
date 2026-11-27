# Loading screen night art (2026-11-27)

- Byte-identical copy of T_UI_Loading_Night.png under Assets/_Project/Resources/UI/ with its own import meta, so LoadingScreen can Resources.Load it.
- LoadingScreen uses the art as the background and falls back to the old gradient if the load fails.
- Manifest row T_UI_Loading_Night_Res points at the original row; the copy is the one used at runtime.
- Not seen on a device.
