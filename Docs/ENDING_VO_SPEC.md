# Ending VO + Cinematic Spec (THE one ending)

Status: **request to bro's side** — exact script, chapter flow, shot list and
voice details for the single ending the user asked for: one great ending,
third-person cinematic, fully voiced (EN + AR).

This document is the complete brief. If a detail is not here, it follows
`Docs/VO_RECORDING_SPECS.md` and `Docs/Story/STORY_BIBLE.md`.

## 1. The decision

The four D13 endings (FullRecovery / PartialMemory / HollowEscape / Dawnless)
are **retired as player-facing endings**. Every run now ends with the same
cinematic: Nour escapes through the gate at dawn with the memory box, seen in
third person for the first time. The run's choices still matter afterwards —
the study recap (existing `RecapScreen`) still personalizes per run and keeps
its D13 tint — but there is exactly **one** ending sequence.

New story nodes `SN_Ending_01` … `SN_Ending_14` (14 lines, 4 speakers) carry
the script. The old `SN_End_*` nodes stay in the project as unused lore; no
scene or code change deletes anything.

## 2. Chapter flow into the ending

1. **Act 1** (`01_School`, arrival + first memories) → **Act 2** (library /
   memory flash, first Shadow confrontations) → **Act 3** (dawn run:
   `SN_Act3_04` choice of route, `SN_Act3_08` promise at the crank).
2. The player turns the gate crank (`GameManager.EscapePlayer()` fires, the
   bloodless escape fade begins — existing behaviour).
3. **NEW — cinematic** plays inside the escape fade: the third-person ending
   below, shots synced to the voiced lines. Player input is locked; HUD hidden.
4. Cinematic ends on the title card → existing `03_Ending` dawn screen →
   recap screen, unchanged.

## 3. Ending script — line sheet

Naming follows the line-ID convention (`Docs/VO_LINE_SHEET.md`):
clip id = `VO_<nodeId>` → `VO_SN_Ending_01.ogg` … `VO_SN_Ending_14.ogg`,
delivered to `Assets/_Project/Resources/VO/EN/` and `.../VO/AR/`.
Speakers: Nour (0), Mother (1), Shadow (2), Memory narrator (3).

| # | nodeId | Speaker | Shot | EN line | AR line |
|---|--------|---------|------|---------|---------|
| 1 | SN_Ending_01 | Memory | 1 | The gate closes behind her. The sound is small: a latch, not a full stop. | يُغلَق الباب خلفها. الصوت صغير: طَقطَقة مِزلاج، مش نقطة في آخر السطر. |
| 2 | SN_Ending_02 | Nour | 2 | Six pieces. Her handwriting. Her red pen. The mug with the crack she never threw away. | ست قطع. خط إيدها. قلمها الأحمر. الكوب اللي فيه شرخ ومقدرتش ترميه. |
| 3 | SN_Ending_03 | Nour | 2 | I came back for a person, and the school tried to keep the receipt. | رجعت عشان أدوّر على شخص، والمدرسة حاولت تحتفظ بالإيصال. |
| 4 | SN_Ending_04 | Mother | 3 | Knowledge is the only light that follows a person into the dark. | العِلم هو النور الوحيد اللي بيمشي مع الإنسان جوه الضلمة. |
| 5 | SN_Ending_05 | Memory | 4 | On the pavement she opens the box. Nothing inside glows. Nothing needs to. | على الرصيف بتفتح العلبة. مفيش حاجة جواها بتنوّر. ومفيش حاجة محتاجة. |
| 6 | SN_Ending_06 | Shadow | 5 | You are leaving with… less than you arrived. | إنتي خراجة بـ… أقل من اللي جيتي بيه. |
| 7 | SN_Ending_07 | Nour | 5 | No. I'm leaving with exactly what I came for. You're the one who's short. | لأ. أنا خراجة بالظبط باللي جيت عشانه. إنتي اللي ناقصك. |
| 8 | SN_Ending_08 | Memory | 6 | The sky over Al-Noor turns from black to grey to a thin, clean gold. | السما فوق النور بتحوّل من الأسود للرمادي لدهبي رفيع ونضيف. |
| 9 | SN_Ending_09 | Mother | 7 | Faster, habibti. The bell does not wait for the slow — and neither does the morning. | بسرعة يا حبيبتي. الجرس مش بيستنى البطيئين — والصبح برضه. |
| 10 | SN_Ending_10 | Nour | 7 | I keep my promise out loud, because the school is listening, and schools keep records. | بقول وعدي بصوت عالي، عشان المدرسة سامعة، والمدارس بتحتفظ بالسجلات. |
| 11 | SN_Ending_11 | Nour | 7 | I'll keep the light on. For the ones who come after. | هسيب النور والِّع. عشان اللي جايين بعديا. |
| 12 | SN_Ending_12 | Shadow | 8 | Then we will… remember you… differently. | إذن هنفـ… نفتكرك… بطريقة تانية. |
| 13 | SN_Ending_13 | Memory | 8 | She walks into the morning without looking back. The windows of the school hold the dark a moment longer — then let it go. | بتمشي في الصبح من غير ما تبص وراها. شبابيك المدرسة ماسكين الضلمة لحظة كمان — وبعدين يسيبوها. |
| 14 | SN_Ending_14 | Memory | 9 | Al-Noor keeps its records. Tonight, so did she. | النور بتحتفظ بسجلاتها. والليلة دي، هي كمان احتفظت. |

Callback note: lines 4 and 9 quote `SN_Act3_09` / `SN_Act3_06` deliberately —
remembered speech returning at the gate. They are **new recordings** (different
emotional register: quieter, warmer), not re-used clips.

## 4. Third-person cinematic shot list (~70 s, camera rail, no cuts faster than 4 s)

| Shot | Time | Framing | Action | Lines |
|------|------|---------|--------|-------|
| 1 | 0–7 s | Close-up, over-shoulder from behind | Hands on the crank; gate cracks open; first exterior light spills in | 1 |
| 2 | 7–18 s | Low wide from inside the corridor | Nour steps through the gate — **first full third-person reveal** — dawn rim light on her silhouette | 2–3 |
| 3 | 18–26 s | Slow crane pull-back above the gate | School dark behind her; sky greying | 4 |
| 4 | 26–33 s | Close-up, pavement level | She kneels and opens the memory box; ordinary contents | 5 |
| 5 | 33–44 s | Her face, first time fully lit | Shadow's voice from the doorway behind; she answers without turning | 6–7 |
| 6 | 44–49 s | Cut to third-floor window | The wraith's outline in the dark glass, almost gone | 8 |
| 7 | 49–60 s | Slow dolly behind her | She walks away down the street; promise said out loud | 9–11 |
| 8 | 60–66 s | Reverse wide, school facade | Windows hold the dark, then release it | 12–13 |
| 9 | 66–70 s | Title card on dawn sky | LIGHT & SHADOW → fade to existing dawn screen | 14 |

## 5. Voice details (per line)

Profiles per `VO_RECORDING_SPECS.md` §2 are non-negotiable. Line-specific
direction:

| # | Speaker | Direction |
|---|---------|-----------|
| 1 | Memory | Quiet epilogue register; ~130 wpm. Flat, kind. |
| 2 | Nour | Inventory cadence, one beat per item; warmest take of the session. |
| 3 | Nour | Dry, almost amused. The "receipt" lands as a joke she only half enjoys. |
| 4 | Mother | Written-words register, unhurried, slight smile; this is a quote, not dialogue. |
| 5 | Memory | Steady. "Nothing needs to" gets the smallest emphasis. |
| 6 | Shadow | Courteous, slow (~110 wpm); hold the gap at the ellipsis — do not fill it. |
| 7 | Nour | The one firm line in the cinematic. No heat — certainty. |
| 8 | Memory | Unhurried; let "thin, clean gold" breathe. |
| 9 | Mother | Same register as line 4; a remembered push forward, not an order. |
| 10 | Nour | Spoken to the school, slightly projected, like reading into a record book. |
| 11 | Nour | Quiet. "For the ones who come after" almost to herself. |
| 12 | Shadow | Pitiable, not menacing; trailing gaps; the school's forgetting losing its grip. |
| 13 | Memory | Longest line; steady, then soften on "let it go". |
| 14 | Memory | Final line of the game. Full stop. Warmth allowed. |

Recording chain, file format, takes, slates, levels: exactly
`VO_RECORDING_SPECS.md` §3 (48 kHz / 24-bit mono WAV, 3 takes, dry).

## 6. Deliverables requested from bro's side

1. **VO:** lines 1–14 × EN + AR (28 finished clips), named per §3, into
   `Assets/_Project/Resources/VO/{EN,AR}/`.
2. **Girl character model + animations** (already on your list): needed clips
   for the cinematic — idle, walk (slow), turn-head, crank-turn (shot 1),
   kneel/box-open (shot 4). Placeholder capsule stands in until the model ships.
3. **Wraith silhouette** (already on your list): static window outline for
   shot 6 is enough for this cinematic.

## 7. Code hooks on our side (already in progress)

- `CinematicEndingController`: plays the shot list on a camera rail during the
  escape fade, locks input, hides HUD, plays each line's VO with subtitles,
  tolerates missing clips (line still displays — generated placeholder clips
  per the accepted batch policy can be swapped in until real recordings ship).
- `EndingSequence` asset: ordered shots × lines with per-shot camera paths and
  durations, so timing can be tuned without code changes.
- A temporary third-person rig (placeholder character) so the cinematic is
  testable end-to-end before the girl model lands.
