# EditMode tests + fuse display names (2026-11-27)

Audit 2 step 3 follow-up: first EditMode test batch for pure logic, in
Assets/_Project/Tests/Editor (predefined editor assembly, no asmdefs to
rewire - the player build skips Editor folders, CI compiles them):

- InventoryTests: add/has/remove, null/empty guards, ItemAdded fires once per
  new id, save-blob round trip through the real (reflection-invoked) capture/
  apply handlers, corrupt blob resets without throwing, localized display
  names with id fallback.
- AccessibilityStoreTests: the settings setters WRAP at both ends (pinning
  that so a refactor cannot turn rows into dead ends), instant typewriter at
  max speed, quality tier out-of-range falls back to auto.
- LocalizationTests: English pass-through, Arabic known/unknown key behaviour,
  null/empty safety.

build.yml gains a test-editmode job (game-ci/unity-test-runner, same license
secrets, code-changes gate and docs-only skip like the build jobs, Library
cache) so the batch runs on every code PR. Also: Inventory display names for
the "fuse" item from the puzzle-5 payout swap (the HUD strip falls back to
the raw id without them).
