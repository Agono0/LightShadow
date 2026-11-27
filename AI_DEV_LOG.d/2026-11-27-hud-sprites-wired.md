# Wire the night HUD sprites (2026-11-27)

Uses the HUD art from #105: health bar gets the sliced T_UI_HealthFrame_Night
(16 px border, confirmed in its meta) with the T_UI_HealthFill_Night fill; the
inventory strip shows T_UI_Icon_Key/Medkit_Night beside carried keys and the
medkit. Both fall back to the old plain look when the sprites are missing.
The inventory strip moved from y=-120 to -150 to clear the taller bar.

Not done: T_UI_Icon_Checkpoint_Night (no checkpoint HUD element exists yet) and
the loading art from #107 (the loading screen is built at runtime, so the PNG
needs a Resources path or a serialized reference first).
Unverified: not seen in the editor; the 16 px border exceeds the bar height so
Unity scales it down, and the text indent next to icons is a spaces hack.
