# Apply the night UI textures (2026-11-27)

#96 landed T_UI_Panel/Button/Glow/Backdrop_Night, but SceneBuilderUI still
loaded the older UI_* files, so the retinted frames were not used anywhere.
The four sprite getters now prefer the T_UI_*_Night file and fall back to UI_*.

Unverified: not seen in the editor. The night backdrop is 1024x512 (the old one
960x540), so it stretches slightly differently; 9-slice borders come from the
imported metas (32 panel, 48 button).
