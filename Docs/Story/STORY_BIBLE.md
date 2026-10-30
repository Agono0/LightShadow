# LIGHT SHADOW — Story Bible

Narrative design for the premium pass (sub-agent 3, NARRATIVE). Source of truth for tone,
characters, stats, flags and the four endings. The playable node-by-node script lives in
`Docs/Story/SCRIPT.md`; the same content is compiled into `StoryNode` assets by
`Tools/Editor/SceneBuilderStory.cs`. Nothing here is implemented in runtime C# — every line
of dialogue is data (`Assets/_Project/Data/Story/SN_*.asset`), per decision D4.

Anchors: project document §2.3 (game for good, sincere-not-grim), §3 (story & cast),
`Docs/DECISIONS.md` D4 (custom SO graph), D12 (saves), D13 (four endings).

---

## 1. Premise in one breath

Nour, nineteen, climbs the fence of her dead mother's school on the night before demolition
to carry the memory box home. The school has locked its last light inside fuse boxes and
paper; the Shadow — everything students ever forgot here — wants the night to stay empty.
Every fact Nour speaks aloud makes her flashlight burn brighter. Retrieval is survival.

## 2. Tone rules (non-negotiable)

1. **Grief is shown through objects and verbs, never stated.** Nobody says "I miss her".
   Nour pours cold tea out of a mug before she packs it. That is the whole speech.
2. **The horror erases, it never harms.** No blood, no threats of violence. The Shadow's
   menace is blank posters, half-remembered names, a voice like chalk under a door.
3. **Hope-forward (doc §2.3).** The night ends with knowledge carried out of a falling
   building. Even the worst ending is quiet, not cruel: the school keeps her memories
   gently, "the way a library keeps a book nobody borrowed".
4. **Restrained register.** Short sentences. Concrete nouns: chalk, jasmine, red pen,
   crank flashlight, white sheets over furniture. No fantasy filler, no mysticism beyond
   the one established symbol (the Shadow = forgetting).
5. **The mother exists only in her own words** — lesson-plan margins, marks on exams,
   sayings Nour half-remembers. Her voice is warm, precise, a little formal, and always
   about looking carefully at the world.

## 3. The three voices (speaker enum in `StoryNode`)

| Speaker | Sound | Function |
| --- | --- | --- |
| **Nour** | First person, present tense, dry and exact. Underneath: a student who studied grief the way she studies everything — quietly, thoroughly. | Inner voice; names what she sees; deflects with practicality. |
| **Mother** | Only remembered speech — sayings, lesson lines, marginalia. Never responds to Nour directly. | Memory flashes at fuse boxes and fragments; the moral light. |
| **Shadow** | The school's forgetting. Sentences with words missing, names repeated wrong, a courteous menace. It never lies, it just... unremembers. | Temptation and mirror. Talking to it is talking to erasure itself. |
| **System** | Stage directions and objective echoes ("The library hums awake."). | Used sparingly; never poetic on purpose. |

## 4. Characters

- **Nour (protagonist, player).** Finished at Al-Noor the year before. She is competent in
  the dark — she climbs the fence she climbed a hundred times as a pupil. Her arc: arrives
  to retrieve *objects*, leaves having recovered *memory*. She does not become fearless;
  she becomes willing to say her mother's name out loud in a building that erases names.
- **The mother's presence.** Not a ghost. Handwriting, a red marking pen, a mug, lesson
  plans, six small objects (doc §3.6). Her lines are quoted from the things she actually
  left behind — every memory flash ties to a fragment's place.
- **The Shadow (doc §3.3).** Human-shaped, smoke-edged, trailed by whisper and chalk
  static. Legend: made of everything students ever forgot. It is not evil; it is *loss
  doing its job badly*. If Nour speaks to it with enough calm, it becomes almost pitiable:
  it has been standing in an empty school being forgotten right back.

## 5. Themes

- **Memory is an act, not a possession.** Each fragment must be *done something with*
  (read aloud, boxed, left in its place) — remembering is a choice the player makes.
- **Knowledge-as-light** (the core fiction): facts are the fuel; saying them aloud is
  the crank. The story never contradicts the mechanic: the Shadow always backs away from
  a spoken truth.
- **Grief as stewardship.** Nour's job is not to "move on" but to *carry*: out of the
  building, past the gate, into the morning.
- **A school that erases vs. a school that keeps.** The demolition is scheduled; the
  memory box is not. Which one survives is the player's doing.

## 6. Story stats (ENG-agnostic; live in `StoryDirector` state, cross saves as storyJson)

| Stat | Range | Rises when Nour… | Story use |
| --- | --- | --- | --- |
| **boldness** | −1 … +1 | shouts into the dark, opens rooms lights-off, answers the Shadow | Unlocks the back-stairs route (skips dawn-corridor dialogue = real gameplay advantage during the chase); angers the Shadow when calm is low; colors ending text. |
| **trust** | −1 … +1 | leaves things where they grew, treats the Shadow as the school's forgetting, keeps promises | Softens Shadow lines later; required to *confront* rather than banish. |
| **calm** | −1 … +1 | counts and orders things, sits at the bench, reads aloud correctly | Gates the safe outcomes of risky moments (moment 5 and 8 consult calm before applying anger). |

Flags are plain strings (canonical list in SCRIPT.md §10). The **boldness stat the brief
asks for maps to calm-gated risk**: a risky choice with `calm >= 0` calms/softens the
Shadow; the same choice with `calm < 0` angers it (later Shadow lines change tone). All
effects are story-side — no gameplay code is touched.

## 7. Branching shape (12 authored moments)

Placed at natural beats, per the brief: after each fuse box, at each fragment, before the
gate. Full text in SCRIPT.md. One-line map:

1. **The Gate** (Act I start) — how she crosses the threshold.
2. **The Card** (memory box) — "Put me back together."
3. **Fuse Box 1** — the first flood of light; stay or step out.
4. **Fragment 1** (bookmark) — what to do with a pressed flower.
5. **The Lit Window** (Science wing, Fuse 2) — two shadows in a lit room.
6. **The Bench** (garden, Fragment 3) — sit, take, or leave.
7. **The Optics Cabinet** (Fragment 4) — "Light travels in straight lines."
8. **The Shadow Speaks** (mid Act II) — half-erased names; confrontation fork. *Key
   branch: grants `knows_backstairs` shortcut or locks later dialogue.*
9. **The Mug** (staff room, Fuse 3 / Fragment 6) — the smallest human choice.
10. **The Main Hall** (before the crank) — the full confrontation: forgive, erase, pass.
11. **Dawn** (chase begins) — one timed breath before the run.
12. **The Gate Crank** (escape) — which promise she makes; colors the ending epilogue.

Consequence spine (the "one branch opens a shortcut or locks a dialogue" requirement):
moment 8's bold route sets `knows_backstairs` → at `DawnStarted` the director **skips the
two dawn-corridor dialogue nodes** (fewer input-lock interruptions inside the timed chase —
a real, honest shortcut). Moment 9's "leave everything" sets `left_all` → the gate
epilogue (moment 12) is **locked to its terse variants**; the warm lines are blocked by
flag check in the graph.

## 8. The four endings (decision D13)

Selection runs once, at `EscapeStarted` (or at the second dawn catch), from live state.
The formula constants live in the story config asset (`SO_StoryConfig`), not in code.

| Ending | Trigger formula (defaults; tuned in SO_StoryConfig) | Meaning |
| --- | --- | --- |
| **Full Recovery** | fragments == 6 AND accuracy ≥ 0.75 AND flag `confronted_shadow` | The box is complete and she read it aloud to the thing that erases names. Hopeful, quiet, warm dawn. |
| **Partial Memory** | fragments ≥ 4 AND accuracy ≥ 0.5 | The box has gaps she can name. Bittersweet; she keeps what she carried. |
| **Hollow Escape** | escaped, otherwise | She is outside and lighter than she came. The gate closes on an empty box. |
| **Dawnless** | caught twice after `DawnStarted` (not a game-over — the run *ends* here) | The crews arrive while she still stands at the gate. The school keeps her memories for her, gently, until it doesn't exist. |

Accuracy = correct quiz answers / asked questions, tracked by `StoryDirector` from
`GameEvents.QuestionAnswered` across the whole run (crosses saves inside storyJson).

Each ending defines: title key, body text (EN/AR), dawn-tint override for the 03_Ending
sky/sun, one narrator line, and a recap name. EndingController picks the tint additively;
the existing dawn animation and study recap are untouched.

## 9. Timed choices

Timed nodes show a shrinking bar (`timeSeconds` 6–10 s per node). Hesitation matters
where hesitation is *in character*: reading her mother's card aloud (moment 2), holding
the beam on the Shadow (moment 8), the first step of the dawn run (moment 11). A timeout
is a legitimate choice — it routes to `timeoutNextId`, usually the cautious outcome, and
dents `calm` slightly. No timeout ever kills or softlocks.

## 10. Localization policy

- **Keys are the English sentences** (same convention as `Localization.cs`), so English
  display needs no lookup. Arabic pairs live in `StoryStrings.cs` (story content) and the
  base `Localization.cs` table (chrome that already existed).
- All new UI chrome (dialogue box, save slots, loading screen, settings rows) is registered
  EN/AR in `StoryStrings.cs` and displayed through `Localization.Present` +
  `ArabicSupport` shaping with `Localization.CurrentFont`, matching the existing path.
- Deviation, documented: `Localization` has no runtime registration API, so story strings
  keep their own table instead of patching Core (Core is out of my file scope). The
  display path is identical.
- Speaker names ("NOUR", "MOTHER", "THE SHADOW", "MEMORY") and ending titles are keyed.
- The Shadow's Arabic lines lose their missing-word effect slightly (elisions read as
  trailing dashes instead of hollow gaps); accepted and noted.
