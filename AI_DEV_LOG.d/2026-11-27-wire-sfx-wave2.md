# Bootstrap and wire SFX wave 2 (2026-11-27)

Scripts side of #111: AudioLibrary gets nine families (chalkWrite, crank,
flashlightBuzz, gateCrank, glassShard, mirrorTurn, powerSurge, shadowHiss,
uiPaperTap), AudioLibraryBootstrap registers them, AudioLibrarySync maps the
ids (the Sync fills the asset fields by filename at build time, no .asset edit).

Wired through the Sfx helper (synth fallback kept): flashlight crank ratchet
(SFX_Crank), fuse restore (SFX_PowerSurge), exit gate power-up (SFX_GateCrank),
optics mirror rotate (SFX_MirrorTurn), breakable shatter (SFX_GlassShard).

Not wired (no obvious event yet): SFX_ShadowHiss (waits on the wraith audio PR),
SFX_FlashlightBuzz (one-shot, no low-battery flicker hook), SFX_ChalkWrite
(provisional, fit unverified), SFX_UIPaperTap.
This branch carries the #98 and #111 commits so CI sees the Sfx helper and the
clips; it squashes cleanly once those land.
Unverified: nothing heard in editor or on device.
