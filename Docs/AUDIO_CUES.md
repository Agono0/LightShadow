# LIGHT SHADOW — Audio Cue Sheet (premium pass, workstream B)

**Written against:** `main` @ `2dd3376` (2026-11-27) · **Owner of this doc:** audio/asset agent (Ahmed's side)
**Scope:** SFX + ambience + music cues. VO is workstream A (separate `Docs/VO_SCRIPT.md`).
Reverb zones / mixer wiring are B.4 and are coordinated separately.

## How cues reach the game (read before adding files)

- Runtime lookup is id-based: consumers call `SfxRegistry.Clip("SFX_...")` /
  `ClipVariant("SFX_...", n)` and **fall back to synthesised audio** when the id is
  missing (`SfxRegistry.cs`). The game never breaks when a file is absent — it just
  sounds procedural.
- Registration today is hard-coded in `AudioLibraryBootstrap.cs` from the fields of
  `Assets/_Project/Audio/AudioLibrary.asset` (audio-side asset). **New ids need a
  matching field + bootstrap line (Scripts = owner's side) — every cue below is marked:**
  - **drop-in** — an existing consumer already asks for this id/family; adding the
    file(s) + library entry is enough.
  - **new id** — needs one bootstrap/library line from the Scripts owner before it plays.
- Naming (AGENTS.md rule 3): `SFX_` / `MUS_` / `AMB_`, variant families `_0.._n`, English,
  no spaces.
- Import targets (MASTER_PLAN §8.4): short SFX ADPCM, Decompress On Load, mono;
  ambience/music Vorbis q60–70, Streaming; **dialogue −16 LUFS, SFX peaks −3 dBFS**.
  Current clips still use default import settings — fixing that is part of B.2.
- Mixer groups: `Master > Music / SFX / Ambience / Voice / Shadow / UI`
  (`M_Main.mixer`, D17). Snapshots: Explore / Stalk / Chase / Hide / Memory / Dawn.
- Every downloaded file is logged in `Docs/ASSET_LICENSES.md` **before** commit
  (AGENTS.md rule 4); CC-BY credit lines go to `Docs/CREDITS.md`.
- Approved sources (brief): Sonniss #GameAudioGDC (royalty-free, commercial, no
  attribution), Freesound **CC0 only**, OpenGameArt (CC0/CC-BY), Kenney (CC0).
  Music primaries already vetted in `Docs/SOURCING_FONTS_MUSIC.md` Part 2.
- **Headroom first:** 268 of the 287 imported ThirdParty clips are dormant
  (Kenney Interface/Impact/RPG). Cues that can be filled from packs we already
  own are marked `in-packs` — zero new license work.

## Existing wired cues (19 — keep or upgrade)

| id | files | consumer | status |
| --- | --- | --- | --- |
| SFX_QuizCorrect / SFX_QuizWrong | 2 ogg | QuizUI | keep (Kenney) |
| SFX_Click | 1 ogg | UI | keep |
| SFX_Pickup | 1 ogg | pickups | keep |
| SFX_Power | 1 ogg | fuse restored | keep |
| SFX_DoorCreak | 1 ogg | doors | keep, add variants |
| SFX_FootstepConcrete_0..3 | 4 ogg | Footsteps.cs | keep, extend |
| SFX_FootstepWood_0..1 | 2 ogg | Footsteps.cs | too few — extend to 4+ |
| SFX_ShadowMoan_0..2 | 3 wav | ShadowAudio (patrol/chase/stinger) | upgrade candidates (drop-in) |
| AMB_SchoolNight | 1 mp3 (CC-BY tcarisland) | AmbientDrone | keep |
| MUS_Bed_LongNoteOne | 1 mp3 (CC-BY Kevin MacLeod) | AdaptiveMusic bed | keep as Explore/Stalk bed |
| MUS_Ending_HopeGold / MUS_Ending_DawnlessGrey | 2 mp3 (CC-BY) | Ending cues | keep; 2 endings still missing cues |

## A. Player (global)

| cue id | sound | variants | loop | group | trigger / consumer | wiring | source | priority |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| SFX_FootstepConcrete_4..7 | terrazzo/concrete walk | +4 | no | SFX | Footsteps.cs | drop-in | in-packs / Sonniss | P1 |
| SFX_FootstepWood_2..3 | wood walk (stage, library) | +2 | no | SFX | Footsteps.cs | drop-in | in-packs / Sonniss | P1 |
| SFX_FootstepTile_0..3 | corridor tile walk | 4 | no | SFX | surface pick | new id (Footsteps.cs surface set) | Sonniss | P2 |
| SFX_FootstepCarpet_0..3 | carpet (library) | 4 | no | SFX | surface pick | new id | Sonniss / Freesound CC0 | P2 |
| SFX_FootstepGravel_0..3 | gravel (courtyard) | 4 | no | SFX | surface pick | new id | Sonniss | P2 |
| SFX_FootstepMetal_0..3 | metal stairs (back stairs) | 4 | no | SFX | surface pick | new id | Sonniss | P2 |
| SFX_FootstepCrouch_* / SFX_FootstepSprint_* | per-surface crouch + sprint sets | per §8.2 | no | SFX | speed/state | new id | derived from walk sets (pitch/gain) | P3 |
| SFX_Breath_Calm / _Scared / _Hold | breathing states | 3×2 | loop | Voice | Calm stat / hiding | new id | Freesound CC0 (record own later) | P2 |
| SFX_Heartbeat_0..1 | heartbeat (low Calm, Hide snapshot) | 2 | loop | SFX | Calm FX | new id | Freesound CC0 | P1 |
| SFX_ClothRustle_0..2 | cloth movement | 3 | no | SFX | crouch/turn | new id | Freesound CC0 | P3 |
| SFX_FlashlightClick | flashlight on/off | 1 | no | SFX | toggle | new id | in-packs (Interface) | P1 |
| SFX_FlashlightBuzz | low-charge buzz | 1 | loop | SFX | charge < threshold | new id | Freesound CC0 | P2 |
| SFX_Crank_0..2 | crank ratchet (manual charge, P3 minigame) | 3 | no | SFX | crank input | new id | Freesound CC0 / record | P2 |
| SFX_ChargeChime | soft warm charge-up | 1 | no | SFX | crank cap reached | new id | in-packs (Interface) | P3 |
| SFX_FlickerCrackle | beam flicker at 0 charge | 1 | loop | SFX | flicker state | new id | Freesound CC0 | P3 |

## B. World — per area

| cue id | sound | area | variants | loop | group | wiring | source | priority |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| SFX_DoorWoodCreak_0..2 | wooden classroom doors | all wings | 3 | no | SFX | new id (extend beyond single SFX_DoorCreak) | in-packs (RPG) / Sonniss | P1 |
| SFX_DoorSteelBang | double steel door | entrance/gatehouse | 1 | no | SFX | new id | in-packs (Impact metal) | P2 |
| SFX_DoorLockedRattle_0..1 | locked rattle | all wings | 2 | no | SFX | new id | Sonniss | P1 |
| SFX_LockerOpen / SFX_LockerClose | lockers | corridors | 2 ea | no | SFX | new id | Sonniss / in-packs (Impact metal) | P1 |
| SFX_LockerMuffled | inside-locker muffled bed | hiding v1 | 1 | loop | SFX | new id | Freesound CC0 | P3 |
| SFX_ChalkWrite_0..2 | chalk writing | chalkboards | 3 | no | SFX | new id | Freesound CC0 | P2 |
| SFX_ChalkErase | chalk erasing | chalkboards | 1 | no | SFX | new id | Freesound CC0 | P3 |
| SFX_GlassCrack / SFX_GlassShard_0..1 | prism / broken glass | science wing | 2 | no | SFX | new id | in-packs (Impact glass) | P2 |
| SFX_FuseClunk | fuse seats in box | fuse boxes ×3 | 1 | no | SFX | new id | in-packs (Impact) | P1 |
| SFX_PowerSurge | lights waking after fuse | fuse boxes ×3 | 1 | no | SFX | new id (SFX_Power stays as final confirm) | Freesound CC0 | P1 |
| SFX_FluorescentHum | fluorescent tube hum + flicker buzz | lit rooms | 2 | loop | Ambience | new id | Freesound CC0 | P2 |
| SFX_CeilingFanCreak | ceiling fan | assembly/classrooms | 1 | loop | Ambience | new id | Freesound CC0 | P3 |
| SFX_WindBrokenGlass | wind through broken windows | courtyard edge/corridors | 2 | loop | Ambience | new id | Freesound CC0 | P1 |
| SFX_BuildingCreak_0..2 | old building creaks (one-shots, random timer) | all interior | 3 | no | Ambience | new id | Freesound CC0 / Sonniss | P1 |
| SFX_PipeDrip | dripping pipe | toilets/science sink | 1 | loop | Ambience | new id | Freesound CC0 | P3 |
| AMB_CairoNight | distant traffic, dogs, far train | exterior bleed | 1 | loop | Ambience | new id | Freesound CC0 | P2 |
| AMB_Crickets | courtyard crickets | courtyard | 1 | loop | Ambience | new id | Freesound CC0 | P2 |
| SFX_SchoolBell | school bell (normal) | assembly hall / story beat | 1 | no | SFX | new id | Freesound CC0 | P2 |
| SFX_SchoolBell_Detuned | reversed/detuned bell (Shadow moment) | assembly hall | 1 | no | Shadow | new id | derived from bell (own processing) | P3 |
| SFX_GateSlam | iron gate slam (Act I opening) | forecourt | 1 | no | SFX | new id | in-packs (Impact metal heavy) | P1 |
| SFX_GateCrank_0..1 | gate crank (Act III exit) | gatehouse | 2 | no | SFX | new id | Freesound CC0 | P2 |
| SFX_CurtainRustle | stage curtains | assembly hall | 1 | no | SFX | new id | Freesound CC0 | P3 |
| SFX_ChairFold | folding chairs | assembly hall | 1 | no | SFX | new id | in-packs (Impact wood) | P3 |
| SFX_Kettle | staff-room kettle | staff room | 1 | loop | SFX | new id | Freesound CC0 | P3 |

## C. The Shadow

| cue id | sound | variants | loop | group | trigger / consumer | wiring | source | priority |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| SFX_ShadowMoan_0..2 (replace) | layered, closer, more human moans | 3 | no | Shadow | ShadowAudio patrol/chase/stinger | drop-in | OpenGameArt CC0 / own processed | P1 |
| SFX_Whisper_Burst (replace) | layered whispers (names list) | 2–3 | no | Shadow | PooledWhisperBurst.cs | drop-in (id already consumed) | record own / Freesound CC0 | P1 |
| SFX_ShadowBreath | low breath when near | 1 | loop | Shadow | proximity | new id | Freesound CC0 | P2 |
| SFX_ShadowHiss | recoil hiss on banish | 1 | no | Shadow | banish | new id | Freesound CC0 | P2 |
| SFX_ChalkScrape | long chalk scrape (sighting tell) | 1 | no | Shadow | scripted sighting | new id | Freesound CC0 | P2 |
| SFX_PageFlutter | page flutter burst | 1 | no | Shadow | scripted sighting | new id | Freesound CC0 | P3 |
| (roll-call voice) | processed human roll-call | — | — | Voice | assembly stage | workstream A (VO), not SFX | record | later |

## D. Puzzle sounds

| cue id | sound | variants | loop | group | wiring | source | priority |
| --- | --- | --- | --- | --- | --- | --- | --- |
| SFX_LockClick | photo-sensor lock release (optics) | 1 | no | SFX | new id | in-packs (Interface) | P1 |
| SFX_MirrorTurn_0..1 | mirror rotation on optics bench | 2 | no | SFX | new id | Freesound CC0 | P2 |
| SFX_Drawer_0..1 | card-catalogue / desk drawer | 2 | no | SFX | new id | Sonniss | P2 |
| SFX_PuzzleSuccess | warm confirm (distinct from quiz correct) | 1 | no | SFX | new id | in-packs (Interface) | P1 |
| SFX_PuzzleFail | dull wood knock (distinct from quiz wrong) | 1 | no | SFX | new id | in-packs (Impact wood) | P1 |
| SFX_CatalogueCard_0..1 | card flick / pin on clue board | 2 | no | SFX | new id | Freesound CC0 | P3 |

## E. Music and beds (B.3 — after core SFX)

| cue id | slot | loop | group | wiring | source | priority |
| --- | --- | --- | --- | --- | --- | --- |
| MUS_Drone_Library | per-wing drone: library (paper, dust) | loop | Music | new id | Pixabay primary `AMB_SchoolDreadA/B` (SOURCING doc, browser download) | P2 |
| MUS_Drone_Science | per-wing drone: science (hum, metal) | loop | Music | new id | Pixabay `AMB_SchoolSuspense` (AI-flagged — decide) or Sonniss | P2 |
| MUS_Drone_Courtyard | per-wing drone: courtyard (open air) | loop | Music | new id | Pixabay / FMA CC0 `dark-highway-master` alternate | P2 |
| MUS_ChasePulse | chase pulse | loop | Music | new id (Chase snapshot) | Pixabay `Chase` (leberch) — vetted | P1 |
| MUS_MemoryTheme | memory-flash solo piano | one-shot | Music | new id (Memory snapshot ducks all but Voice/Music) | Pixabay `Besides The Motion` — vetted | P2 |
| MUS_Ending_Bittersweet | Partial Memory ending | one-shot | Music | new id | Pixabay `Asagiri` — vetted | P1 |
| MUS_Ending_HollowCold | Hollow Escape ending | one-shot/soft loop | Music | new id | Pixabay `Cold Winter Night` — vetted | P1 |
| MUS_Menu | main-menu loop (PR #24 hook: `SfxRegistry.Clip("MUS_Menu")` via `AudioManager.PlayMusic`; no-ops until registered) | loop | Music | new id | ALIASES `MUS_MemoryTheme` - same shared clip (First Light Particles, Yoiyami, CC0); one asset, one register row; needs a `MUS_Menu` library field + bootstrap line from the Scripts owner pointing at the same clip GUID | P2 |
| AMB_DawnBirds / AMB_DawnCity / AMB_DawnCityFar | dawn set layers | loop | Ambience | new id | Pixabay SFX (SOURCING Part 3) — vetted | P2 |
| (procedural AdaptiveMusic layers) | tension/pulse/calm pads | — | Music | stays as fallback per brief | in-house | keep |

## F. UI (mostly covered)

| cue id | sound | wiring | source | priority |
| --- | --- | --- | --- | --- |
| SFX_Click / SFX_QuizCorrect / SFX_QuizWrong / SFX_Pickup | existing | wired | keep | — |
| SFX_UIPaperTap_0..1 | soft paper taps for menus/notes | new id | in-packs (Interface) | P3 |

## Import-setting fix list (part of B.2, audio-side .meta edits only)

- All current clips: default import (Decompress On Load, Vorbis q1, no platform
  overrides). Re-set per §8.4: short SFX → ADPCM mono Decompress On Load;
  ambience/music → Vorbis q60–70 Streaming; add Android ASTC-aligned overrides
  where size matters. No script changes needed.

## Not in this doc

- VO script sheet and recording kit → workstream A (`Docs/VO_SCRIPT.md`), held.
- Reverb zones / mixer snapshot tuning → B.4, coordinated with the Scripts owner
  (reverb zones exist on the open premium-integration PR and must not be duplicated).