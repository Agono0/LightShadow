# Ambient horror events (2026-11-27)

SFX_BuildingCreak, SFX_WindBrokenGlass and SFX_Breath_Scared were registered but
unused. AmbientHorrorEvents (created at runtime on School load, so no scene
edit) plays a creak or distant glass 6-12 m from Nour every 25-70 s and a
frightened breath (20 s cooldown) when the wraith enters Chase.

Unverified: frequency and levels not heard; may need tuning. Does not pause
with the pause menu beyond Time.timeScale (uses Time.time).
