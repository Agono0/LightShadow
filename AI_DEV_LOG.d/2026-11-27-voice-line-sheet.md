### 2026-11-27 — Docs/VO_LINE_SHEET.md (voice line sheet, workstream A) — Instinct agent (for Ahmed)
- Task: A-workstream voice kit deliverable - a per-line recording sheet for VO casting.
- What AI provided: Docs/VO_LINE_SHEET.md with 77 lines extracted from the repo's
  actual story-node inventory - the 44 Assets/_Project/Data/Story/SN_*.asset nodes
  (+ SO_StoryConfig.asset reconciliation for the 44 vs 45 file count), NOT from
  Docs/Story/SCRIPT.md, which has drifted from the assets. Line IDs VO_<nodeId> and
  VO_<nodeId>_C<n> in asset order, grouped by act, with speaker (StoryNode
  .StorySpeaker enum), suggested emotion (casting hint only), EN text, AR text and a
  max-seconds budget (EN words / 2.6 wps, min 2s; timed choices also carry their
  in-game cap). Arabic resolved in the runtime lookup order (StoryStrings.cs first,
  then Localization.cs) with whitespace-normalized matching for YAML folding.
  Docs-only: nothing wired; VO_RECORDING_SPECS.md (PR #7 scope) untouched.
- What I changed myself: n/a (agent-authored; no human edits yet).
- Teach-back check: pending - PR review with the owner.
- Test result: AR present for 77/77 lines. First pass used a hand-rolled parser and
  reported 4 MISSING (the SN_End_* ending nodes) - WRONG: peer re-check with a real
  YAML parser and exact decoded matching found zero missing. Corrected: assets are
  now parsed with a real YAML parser (Unity !u! tags) and StoryStrings/Localization
  with full C# string-literal decoding (regular + verbatim literals), matched
  exactly against decoded asset text. No MISSING markers remain; no Scripts edits
  were made at any point.
