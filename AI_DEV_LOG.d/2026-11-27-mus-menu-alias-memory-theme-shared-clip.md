# MUS_Menu alias: menu plays the memory theme (2026-11-27)

Docs/AUDIO_CUES.md row: MUS_Menu aliases MUS_MemoryTheme - one shared clip
(Yoiyami "First Light Particles", CC0), one asset, one register row. The
Scripts-owned half: AudioLibrary gets a `memoryTheme` field and
AudioLibraryBootstrap registers both ids at the same clip. Register() skips
null, so the menu stays silent until the audio import pass assigns the field
in AudioLibrary.asset (Audio/ is the peer's lane; the GUID wiring lands with
their music PR follow-up). #24's menu hook already calls
SfxRegistry.Clip("MUS_Menu") via AudioManager.PlayMusic and no-ops safely.
