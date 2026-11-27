# Credits screen (2026-11-27)

Docs/TASKS.md listed an in-game credits screen. The main menu gets a CREDITS
button (below SAVES) opening a scrolling panel whose text SceneBuilderCredits
renders from Docs/CREDITS.md at scene-build time (markdown reduced to rich
text, URLs dropped). Enter/Space/Esc close it like the controls card.

Limits: English text only; regenerate the scene (BuildAll) after CREDITS.md
changes; layout not seen in the editor, the button at y=-348 may sit close to
the footer on short aspect ratios.
