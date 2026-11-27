# Music wiring: six workstream-B tracks into cues (2026-11-27)

Wires the six peer-cleared CC0 tracks (#40) into the cues per
Docs/AUDIO_CUES.md, all registry-first and null-safe (missing clip =
unchanged game):

- MUS_Menu: already consumed by #24's menu hook; live now that the library
  carries the clip (aliases MUS_MemoryTheme, same clip GUID).
- MUS_MemoryTheme: one-shot on FragmentCollected/FuseRestored from
  AudioSnapshotController's memory-flash overlay via new
  AudioManager.PlayMusicOnce; AdaptiveMusic re-asserts the bed after it.
- MUS_ChasePulse: AdaptiveMusic swaps the music source from the bed to the
  chase loop while the pulse envelope dominates (>0.5), then swaps back.
- MUS_Ending_HopeAlt: EndingController registry fallback - PartialMemory and
  HollowEscape only; never replaces HopeGold/DawnlessGrey. Endings with a
  serialized endingMusic entry keep it.
- AMB_DawnBirds: dawn ambience loop in the ending scene.
- AMB_SchoolDreadA/B: registered for audition, not yet consumed.

AudioLibrary.asset GUIDs assigned for all six (wiring is ours; no scene or
prefab YAML touched). AudioImportTool re-run reproduces the same links.
