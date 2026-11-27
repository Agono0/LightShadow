# 2026-11-27 — EditMode tests: SaveData blob + QuestionData validation

Package 1 of Docs/YUSUF_WORK_PLAN.md (claimed in #72). Test files only.

Added under Assets/_Project/Tests/Editor/:
- SaveDataTests.cs: SaveData JSON round trip (all fields), safe defaults, an old
  save missing puzzleJson/inventoryJson, unknown fields ignored, corrupt JSON
  throws from JsonUtility (why LoadFromSlot needs its try/catch), SaveToSlot /
  HasSave / LoadFromSlot / DeleteSlot guards for null and out-of-range slots,
  Capture() is null outside the school scene. Does not touch the persistent
  data folder.
- QuestionDataTests.cs: IsValid / HasArabic rules on in-memory instances, plus a
  sweep of every shipped Q_*.asset (valid, both languages, explanations present,
  four distinct options).

Inventory round-trip and corrupt-blob tests already exist (InventoryTests), so
they were not duplicated. Story flag math is not covered: StoryDirector is a
MonoBehaviour with scene wiring, so it needs a separate approach.

No production files touched. Not run locally (no Unity available to the
author); verified by reading the production code and checking the asset sweep
rules against all current question assets with a script. CI test-editmode is
the first real run.
