### 2026-11-27 — Audio ID map: SfxRegistry.VariantCount, dynamic footsteps, library fields — Abdelrhman Mohamed
- Tool used: Instinct (AI coding agent)
- What AI provided: `SfxRegistry.VariantCount` (family sizes tracked at RegisterMany, cleared
  with Clear), Footsteps switched from hardcoded variant constants to registry counts with a
  concrete fallback when no wood variants exist, and the AudioLibrary/AudioLibraryBootstrap
  entries for the agreed cue IDs (footstep carpet/grass, door open/close, paper/cloth/glass
  arrays, and 14 world-interaction singles) so bro's audio packs drop in without script edits.
- What I changed myself: pending review — lands through the PR workflow.
- Teach-back check: pending — the PR body lists the exact ID contract both sides code against.
- Test result: static only; CI build workflow is the compile gate. Missing clips keep the
  synthesized fallback, so the game never goes silent.
