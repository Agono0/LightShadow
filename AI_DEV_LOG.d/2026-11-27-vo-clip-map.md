---
pr: docs/vo-clip-map
date: 2026-11-27
agent: audio-assets-agent
---

## VO clip map (docs only)

Docs/VO_CLIP_MAP.md: maps the 77 VO line ids (sheet, PR #16) to the 154
generated EN/AR clips (PR #28), verified 1:1 against the PR28 tree. Records the
policy: all clips are GENERATED placeholders (never human, never
final), choice-variant clips stay but are UNWIRED/optional/not played, final
voices are human per VO_RECORDING_SPECS, SAPI fallback and playback wiring
remain the lead's scripts. Map rows point at the planned Assets/_Project/Resources/VO/ paths from PR #29
(pending dependency, labeled in the doc; current PR #28 paths are
Assets/_Project/Audio/VO/). No script/scene/prefab/AudioLibrary edits.
