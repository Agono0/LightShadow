# UI/loading retheme: moonlit night replaces the browns (2026-11-27)

From the user's answered questions (WhatsApp 13:01, item 20): pick a
respectable theme and update the ugly brown textures on the menu, settings,
pause, loading and Android buttons.

- Theme: moonlit night - moon-silver text, slate panels, a moonstone-blue
  primary CTA, candle-amber kept only where fire actually is (windows,
  candles, glows).
- UiThemeTool button texture: slate fills + moon-silver border (the old
  muddy brown gradient is gone). The theme regenerates on every CI scene
  build (SceneBuilder calls UiThemeTool.Generate), so no asset commits.
- SceneBuilderUI + SceneBuilderStory palette constants rethemed together so
  menu, settings, pause, quiz and story panels stay consistent; MakeButton
  gains a cool accent tint so the moonstone CTA keeps its lift.
- LoadingScreen: beam sweep, school name and progress bar go moon-silver;
  lit windows stay warm.
- The round Android buttons (#50) already read neutral-dark; they inherit
  nothing brown.
