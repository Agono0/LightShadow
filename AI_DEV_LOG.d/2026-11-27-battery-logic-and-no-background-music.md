# Battery/torch logic fixes + background music removed (2026-11-27)

User-reported fixes (his 12:49 brief), the unambiguous items only:

- Torch dying: the drain/empty path no longer depends on a wired GameConfig
  (default 240s / 4x focus drain when unwired); at 0 the beam switches off,
  focus clears, and the empty event fires - every frame path enforces it.
- Battery bar: HUD paints meter and battery bars immediately on subscribe
  (the start-of-scene invoke could land before the HUD listened, leaving a
  stale bar); AddBattery keeps the repaint step in sync; BatteryPickup falls
  back to FindAnyObjectByType so a pickup can never vanish without charging.
- Background music removed (user decision): AdaptiveMusic is no longer added
  to the school scene, the menu no longer loops MUS_Menu, and GameManager
  stops any looping music on scene start so nothing bleeds in. Event music
  stays: the memory-flash piano one-shot and the ending themes. Classes and
  registrations remain for a possible return.
