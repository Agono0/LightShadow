# LIGHT SHADOW — VO Recording Specs

Briefing for the future human voice-recording session that replaces the SAPI
TTS narrator (`Scripts/Core/StoryTeller.cs`) with real performances. Scope:
the story graph only — the 44 `StoryNode` assets under
`Assets/_Project/Data/Story/`, in **English and Egyptian Arabic** (the two
languages the game ships in).

Sources: character voices from `Docs/Story/STORY_BIBLE.md` §3–§4; chain and
normalization from `Docs/STYLE_GUIDE.md` §3 and `Docs/MASTER_PLAN.md` §8.2–§8.4;
line counts grepped from the `SN_*.asset` files (`speaker` field, enum in
`Scripts/Story/StoryNode.cs`: Nour=0, Mother=1, Shadow=2, System=3 — System is
displayed as **MEMORY** in `StoryStrings.cs`).

---

## 1. Who is being recorded

| Speaker | Display name in game | Lines (per language) | Words (EN) | Casting note |
| --- | --- | --- | --- | --- |
| **Nour** | NOUR | 22 | 466 | Lead. Young woman, 19. |
| **Mother** | MOTHER | 7 | 95 | Remembered voice of a teacher, 40s. |
| **Shadow** | THE SHADOW | 9 | 233 | Character voice; post-processed in engine. |
| **Memory** | MEMORY | 6 | 382 | Narrator (stage directions + the four ending texts). |
| **Total** | | **44** | **1176** | ×2 languages = **88 delivered lines** |

One node = one line (each `StoryNode` has a single `text` field). Choice
options, quiz strings and HUD text are **not** voiced — players read them; do
not include them in the session.

## 2. Character voice profiles (from STORY_BIBLE §3–§4 — non-negotiable register)

### Nour (protagonist, 22 lines)
- **Who she is:** nineteen; finished at Al-Noor the year before; climbed this
  fence a hundred times as a pupil. Competent in the dark; studies grief the
  way she studies everything — quietly, thoroughly; deflects with
  practicality.
- **Sound:** young-adult female, close and dry. First person, present tense,
  **dry and exact** — she names what she sees. Not theatrical; the emotion
  leaks through control rather than performance.
- **Pace:** measured, slightly clipped, ~135–145 wpm; slower and rawer at the
  confrontation nodes (`SN_Act2_04_*`, `SN_Act3_03_*`), almost conversational
  elsewhere.
- **Arc to ride across the session:** arrives practical, leaves willing to say
  her mother's name out loud. Take the Act 3 lines warmer and less defended
  than Act 1.

### Mother (7 lines)
- **Who she is:** not a ghost — handwriting, a red pen, a mug. Only ever
  *remembered speech*: sayings, lesson lines, marginalia. **She never responds
  to Nour directly.**
- **Sound:** adult female, 40s, a teacher's voice: **warm, precise, a little
  formal, always about looking carefully at the world.** Literary but never
  flowery. Reading her own written words aloud is the right frame — quotes,
  not dialogue.
- **Pace:** unhurried, deliberate, ~120–130 wpm; micro-pauses where a written
  sentence would breathe. Slight smile in the voice on her sayings.

### Shadow (9 lines)
- **Who it is:** the school's forgetting. It never lies — it *unremembers*:
  sentences with words missing, names repeated wrong, courteous menace. At
  enough calm it becomes almost pitiable.
- **Sound:** gender-ambiguous, low, calm — courteous, never monstrous. The
  menace is politeness plus wrongness, not power.
- **Pace:** slow, even, ~110–120 wpm; hold the hollow gaps where words are
  missing (EN) / trailing dashes (AR) — do not fill them.
- **Production:** record clean and neutral; the chalk-static layering and
  whisper doubling are engine-side (plan §8.2 "roll-call voice (processed
  human voice)"). Deliver one dry, controlled performance.

### Memory (narrator, 6 lines)
- **What it is:** stage directions and objective echoes ("The library hums
  awake."), plus the four ending texts — the longest lines in the game.
- **Sound:** neutral, low, unhurried narrator. **Never poetic on purpose.**
  This is the voice of the school itself, not of a character.
- **Pace:** ~130–135 wpm; steady. The ending nodes (80–98 words each) should
  feel like a quiet epilogue — warmth only in the Full Recovery text.

## 3. Recording chain (both languages)

| Item | Spec |
| --- | --- |
| Room | Quiet, treated room; noise floor ≤ −60 dBFS; no HVAC/PC fans audible. Capture 30 s of room tone per session. |
| Microphone | Large-diaphragm condenser (or best available), cardioid, pop filter, 15–20 cm working distance, slightly off-axis for plosives. |
| Chain | Mic → clean preamp → interface. **Record dry:** no EQ, no compression, no gating, no reverb. Processing happens offline at delivery. |
| Format | **48 kHz / 24-bit, mono WAV** (BWF). One file per line (§5), plus room tone per session. |
| Levels | Raw peaks between **−12 and −6 dBFS** (no clipping; leave headroom). Do not normalize at capture. |
| Slate | Verbal slate on a separate track/file per line: `"<character>, <nodeId>, <lang>, take <n>"`. |
| Takes | Minimum 3 takes per line (1 straight + 1 softer + 1 with the direction note below); number them `<name>_tNN` before delivery. |
| Direction | Deliver the reading per §2. If the actor asks "bigger or smaller?", the answer is always smaller — this script works under playing, not over it. |
| Arabic | Same chain; native Egyptian Arabic speaker reading Egyptian-flavored MSA. Do not "act" the Arabic differently — same register, same restraint (STORY_BIBLE §10). |

## 4. Integration point (for whoever wires the clips in)

- Every story line reaches audio through exactly one call:
  `StoryDirector.cs:314` — `StoryTeller.Speak(StoryStrings.Get(node.text), true)`.
  The future hook is a `nodeId` + current-language lookup of the delivered WAV
  (`AudioSource.PlayOneShot` / scheduled clip) that **suppresses the SAPI call
  when a clip exists**; `StoryTeller` stays as the fallback for missing lines.
  `node.speaker` gives the channel, `node.nodeId` the file name — both already
  on the asset.
- Routing: the **Voice group in the M_Main AudioMixer** (Master > Voice, now
  shipped with the P1 mixer work — `Assets/_Project/Audio/Mixers/M_Main.mixer`)
  is waiting for these clips. The VO `AudioSource` routes there via
  `VolumeCategories.GroupFor(VolumeCategories.Category.Voice)` (the same bridge
  every other source uses), so the mixer snapshots can duck everything but
  Voice/Music at memory-flash and chase beats (plan §8.3).
- Other `StoryTeller.Speak` call sites (`GameManager` quiz feedback,
  `KnowledgeFragment` fragment lines, `EndingController` epilogue echo) are
  UI strings — out of scope for this session; they can keep TTS or get a
  later pickup session.
- Speaker display names come from `StoryStrings.cs` (`NOUR`, `MOTHER`,
  `THE SHADOW`, `MEMORY`) — use those exact tokens in slates to avoid
  ambiguity between the actor and the editor.

## 5. File naming and delivery

```
VO_<character>_<nodeId>_<lang>.wav
   <character> = Nour | Mother | Shadow | Memory   (display tokens above)
   <nodeId>    = exact StoryNode.nodeId, e.g. SN_Act1_03, SN_End_FullRecovery
   <lang>      = EN | AR
```

Examples: `VO_Nour_SN_Act1_01_EN.wav`, `VO_Mother_SN_Act1_03_AR.wav`,
`VO_Shadow_SN_Act2_04_stand_EN.wav`, `VO_Memory_SN_End_Dawnless_AR.wav`.

- Delivery = **one preferred take per line, named exactly as above**, plus a
  `/raws/` folder with all slated alternates and room tone.
- Character folders or a flat folder both fine — the file names carry all
  metadata.
- Normalization at delivery (not capture): **−16 LUFS integrated** per line,
  true peak ≤ **−1.0 dBTP**, mono, 48 kHz WAV 24-bit (STYLE_GUIDE §3, plan
  §8.4).
- Unity import (set by the integrator, per plan §8.4): **Vorbis,
  Compressed In Memory**, quality ≈ 70, Force To Mono **off** (already mono),
  Preload Audio Data **on** — voice plays instantly on node enter and must not
  stream. 48 kHz source runs natively (runtime resample 44.1/48 is lossless
  enough per §8.4).

## 6. Line counts per node (studio scoping table)

Grepped from `speaker` + `text` in the 44 `SN_*.asset` files. Word counts are
the English text (Arabic runs similar ±10%); "w" = words in that line.

### Act 1 (11 lines · Nour 151 w, Mother 28 w, Shadow 29 w)

| Node ID | Speaker | EN w |
| --- | --- | --- |
| SN_Act1_01 | Nour | 19 |
| SN_Act1_01b | Nour | 21 |
| SN_Act1_02 | Nour | 17 |
| SN_Act1_02b | Nour | 23 |
| SN_Act1_02t | Nour | 9 |
| SN_Act1_03 | Mother | 19 |
| SN_Act1_03a | Nour | 18 |
| SN_Act1_03b | Shadow | 29 |
| SN_Act1_04 | Nour | 21 |
| SN_Act1_04b | Mother | 9 |
| SN_Act1_05 | Nour | 23 |

### Act 2 (14 lines · Nour 156 w, Mother 26 w, Shadow 122 w)

| Node ID | Speaker | EN w |
| --- | --- | --- |
| SN_Act2_01 | Nour | 28 |
| SN_Act2_01_anger | Shadow | 20 |
| SN_Act2_01_calm | Shadow | 19 |
| SN_Act2_01_pass | Nour | 18 |
| SN_Act2_02 | Nour | 28 |
| SN_Act2_02b | Mother | 14 |
| SN_Act2_02t | Nour | 10 |
| SN_Act2_03 | Nour | 18 |
| SN_Act2_03b | Mother | 12 |
| SN_Act2_04 | Shadow | 28 |
| SN_Act2_04_line | Nour | 28 |
| SN_Act2_04_pass | Nour | 26 |
| SN_Act2_04_shaken | Shadow | 21 |
| SN_Act2_04_stand | Shadow | 34 |

### Act 3 (15 lines · Nour 159 w, Mother 41 w, Shadow 82 w, Memory 34 w)

| Node ID | Speaker | EN w |
| --- | --- | --- |
| SN_Act3_01 | Nour | 32 |
| SN_Act3_01b | Mother | 10 |
| SN_Act3_02 | Memory | 20 |
| SN_Act3_03 | Shadow | 31 |
| SN_Act3_03_bold | Nour | 24 |
| SN_Act3_03_forgave | Shadow | 28 |
| SN_Act3_03_pass | Nour | 26 |
| SN_Act3_03_shaken | Shadow | 23 |
| SN_Act3_04 | Nour | 26 |
| SN_Act3_04t | Nour | 4 |
| SN_Act3_05 | Memory | 14 |
| SN_Act3_06 | Mother | 16 |
| SN_Act3_07 | Nour | 24 |
| SN_Act3_08 | Nour | 23 |
| SN_Act3_09 | Mother | 15 |

### Endings (4 lines · Memory 348 w)

| Node ID | Speaker | EN w |
| --- | --- | --- |
| SN_End_Dawnless | Memory | 87 |
| SN_End_FullRecovery | Memory | 98 |
| SN_End_HollowEscape | Memory | 83 |
| SN_End_PartialMemory | Memory | 80 |

### Ending epilogue echo (4 bonus lines · Memory 56 w)

`EndingController.cs` speaks one extra narrator line at the dawn screen —
`endingNode.localizationKeys[1]` (an epilogue, not the node body). These are
not nodes, so they use a fixed epilogue id instead of an `SN_` id:

| Epilogue id | Ending | EN w | File |
| --- | --- | --- | --- |
| EP_FullRecovery | Full Recovery | 15 | `VO_Memory_EP_FullRecovery_<lang>.wav` |
| EP_PartialMemory | Partial Memory | 16 | `VO_Memory_EP_PartialMemory_<lang>.wav` |
| EP_HollowEscape | Hollow Escape | 13 | `VO_Memory_EP_HollowEscape_<lang>.wav` |
| EP_Dawnless | Dawnless | 12 | `VO_Memory_EP_Dawnless_<lang>.wav` |

(If a fifth generic fallback is ever wanted: the controller's hardcoded
default "The gate closes behind Nour…" line plays only when
`localizationKeys[1]` is missing — all four endings set it, so it is optional.)

### Session math

| Speaker | Lines | ×2 languages | Finished EN audio @ profile pace |
| --- | --- | --- | --- |
| Nour | 22 | 44 | ~3.3 min |
| Mother | 7 | 14 | ~0.8 min |
| Shadow | 9 | 18 | ~2.1 min |
| Memory | 6 + 4 epilogues | 20 | ~3.2 min |
| **Total** | **48** | **96** | **~9.4 min EN** (AR ~10.5 min) |

With slates and 3 takes per line, budget **a half-day (≈4 h) studio session**
for two actors (one as Nour, one covering Mother + Shadow + Memory, split into
character-passes; or 3 h if a single dual-role actor covers Mother and Memory
only while Nour and Shadow book separately). Record all EN first, then AR, so
the director's ear stays in one language per block.

## 7. Delivery QC checklist

- [ ] 96 files, names match §5 exactly (nodeId copied from the assets, not retyped).
- [ ] Each file: 48 kHz, 24-bit, mono, WAV; peaks within −12…−6 dBFS pre-normalization.
- [ ] Delivered takes measure −16 LUFS integrated / ≤ −1.0 dBTP.
- [ ] No clipping, no audible plosives/pops, no mouth clicks on the preferred takes.
- [ ] Room tone files included per session (30 s each).
- [ ] Shadow takes are dry and unprocessed (processing is engine-side).
- [ ] spot-check: every SN_ id from §6 exists exactly once per language.
