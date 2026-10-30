# LIGHT SHADOW — Branching Script (premium pass)

Playable node script. This file is the human-readable twin of the content table in
`Assets/_Project/Scripts/Tools/Editor/SceneBuilderStory.cs`, which builds every node below
into `Assets/_Project/Data/Story/SN_*.asset` (idempotent, Q-style naming). English text is
the localization key; Arabic pairs live in `Scripts/Story/StoryStrings.cs`.

Conventions per node:

```
SN_id | trigger | speaker | timed? | flags/stats
TEXT (the mother's/Shadow's/Nour's line)
  - CHOICE label → set flags · stat deltas → next node
```

Stat deltas written as `B` boldness, `T` trust, `C` calm (range −1…+1 each). `req:` on a
choice means it is only offered when the flag list is satisfied. Chain links: when a node
has no choices it auto-advances (typewriter done + click/auto-advance) to its `→ next`.

---

## ACT I — LOCKED IN

### SN_Act1_01 · trigger `ActStart` · Nour
> The gate slams behind me. Of course it does. Schools only open all the way for the last
> time.

- "Steady. Count the windows that still hold light: three." → set `mood_steady`, C+0.2 → SN_Act1_01b
- "Walk like I'm only late for her class." → set `mood_role`, B+0.1 → SN_Act1_01b
- "Say sorry to the dark. Quietly, so it knows I'm not armed." → set `mood_apology`, T+0.2 → SN_Act1_01b

### SN_Act1_01b · Nour
> Three wings, three fuse boxes, six pieces of her. The crew comes at dawn. All right. One
> room at a time.

→ (chain end)

### SN_Act1_03 · trigger `FuseRestored 1` · Mother
> *(memory flash)* "First the wiring, then the reading. One room at a time — that is how
> you eat a school."

- "Stay in the light while it hums." → C+0.1 → SN_Act1_03a
- "Step back into the corridor. Look at the dark properly." → set `looked_back`, B+0.2 → SN_Act1_03b
- **[timed 8s]** "Turn the flashlight off. Just to hear what the dark says." → B+0.3, C−0.1 → SN_Act1_03b ·
  timeout: "The crank wins. The light stays on." → SN_Act1_03a

### SN_Act1_03a · Nour
> Warm light on white sheets. Her classroom smelled like this — chalk, jasmine, the inside
> of a book.

→ SN_Act1_02

### SN_Act1_03b · Shadow
> It stands at the end of the corridor the way a sentence stands when one word has been
> rubbed out. My beam touches it and it slides away. Politely.

→ SN_Act1_02

### SN_Act1_02 · Nour · *(the memory box card)*
> On her desk: the empty memory box, and one card in her handwriting. "Put me back
> together."

- "Slide the card into my pocket. Evidence first." → set `card_pocket`, B+0.1 → SN_Act1_02b
- **[timed 7s]** "Read it aloud. Like she's listening." → set `card_read_aloud`, T+0.3, C+0.1 → SN_Act1_02b ·
  timeout: "The words stay behind my teeth. Maybe next time." (set `card_pocket`, T−0.1) → SN_Act1_02b
- "Leave it on the desk. I'll take it when the box is full." → set `card_left` → SN_Act1_02b

### SN_Act1_02b · Nour
> Six fragments, three fuse boxes, one gate. I know the shape of this night. I have been
> studying for it my whole life.

→ (chain end)

### SN_Act1_04 · trigger `FragmentCollected 1` · Nour *(pressed-flower bookmark)*
> A pressed-flower bookmark, left in a book she never re-shelved. Her hand underneath:
> "Return by Thursday." I am four years late.

- "Press it between the lesson plans. Where it belongs." → set `bookmark_boxed`, C+0.2 → SN_Act1_04b
- "Keep it in my hand a while." → set `bookmark_kept`, T+0.1 → SN_Act1_04b

### SN_Act1_04b · Mother
> *(memory flash)* "A library card is a key that never rusts."

→ (chain end)

### SN_Act1_05 · trigger `FragmentCollected 2` · Nour *(marked exam paper — short flash)*
> My exam paper. Red pen across the top: "You see more than you think." She marked sixty
> papers that week. She remembered mine.

→ (chain end)

## ACT II — THE DARK WINGS

### SN_Act2_01 · trigger `FuseRestored 2` · Nour *(the lit window)*
> Fuse two hums awake. Through the lab window, on the lit floor: two shadows. Mine — and
> one more, edges smudged like an eraser was passed over it.

- **[timed 8s]** "Call her name into the wing." → B+0.4 → calm-gated: `C ≥ 0` → SN_Act2_01_calm · `C < 0` → SN_Act2_01_anger ·
  timeout: "The name stays where names are safe." → SN_Act2_01_pass
- "Count the fuse wires. Order is a kind of courage." → C+0.3 → SN_Act2_01_pass
- "Keep walking. Windows lie at this hour." → → SN_Act2_01_pass

### SN_Act2_01_calm · Shadow
> It answers with a name. Almost mine. The vowels are worn like stairs. "…N-oor. Nour."
> It has been practicing.

→ (chain end)

### SN_Act2_01_anger · Shadow
> The whisper folds in on itself, chalk-under-a-door, static rising. Somewhere down the
> wing a poster goes blank. *(set `angered_shadow`)*

→ (chain end)

### SN_Act2_01_pass · Nour
> The science wing is awake. One fuse box left, and the staff room that remembers her red
> pen.

→ (chain end)

### SN_Act2_02 · trigger `FragmentCollected 3` · Nour *(garden bench — jasmine and photograph)*
> The garden bench. A jasmine pressed flat in her book of poems, and a photograph of this
> exact bench, her hand caught in the corner of the frame.

- **[timed 9s]** "Sit for one minute. The Shadow slows near light; grief slows near benches." → set `sat_bench`, C+0.4 ·
  timeout: "I take the photograph and keep standing. Standing is easier." (C+0.1) → SN_Act2_02b
- "Take the photograph. Leave the jasmine where it grew." → set `left_jasmine`, T+0.3 → SN_Act2_02b
- "Both in the box. This bench will not exist by Friday." → set `took_all`, B+0.1 → SN_Act2_02b

### SN_Act2_02b · Mother
> *(memory flash)* "Water the jasmine on Thursdays. Talk to it. Plants are students that
> never interrupt."

→ (chain end)

### SN_Act2_03 · trigger `FragmentCollected 4` · Nour *(optics cabinet)*
> Pinned inside the optics cabinet, in her lesson-plan voice: "Light travels in straight
> lines — so do stories."

- **[timed 8s]** "Read it aloud, exactly as written. Precision is a kind of prayer." → set `optics_aloud`, C+0.3, T+0.1 ·
  timeout: C−0.1 → SN_Act2_03b
- "Copy it onto my hand. Ink is portable." → set `optics_copied`, B+0.1 → SN_Act2_03b

### SN_Act2_03b · Mother
> *(memory flash)* "…and so do stories. Straight lines, even through walls. Especially
> through walls."

→ (chain end)

### SN_Act2_04 · trigger `FragmentCollected 5` · Shadow *(the roll call — key branch)*
> It stands in the assembly hall aisle with a roll call in its mouth. Names, most of them
> worn past reading. One it has kept too well: mine.

- **[timed 10s]** "Answer it. 'That one's mine. You can't have it.'" → B+0.4 → calm-gated:
  `C ≥ 0` → SN_Act2_04_stand (set `confronted_shadow`, `knows_backstairs`, T+0.3) ·
  `C < 0` → SN_Act2_04_shaken (set `angered_shadow`, C−0.3) ·
  timeout: "Too slow. The roll call moves on." → SN_Act2_04_pass
- "Recite her lesson line: 'Light travels in straight lines.'" → set `recited_lesson`, C+0.3, T+0.2 → SN_Act2_04_line
- "Hide behind the folded chairs. Let it pass." → set `hid_passed`, C+0.1, B−0.2 → SN_Act2_04_pass

### SN_Act2_04_stand · Shadow *(shortcut branch)*
> "…kept. Kept it. Kept—" It folds my name carefully, like paper, and sets it down between
> us. Down the aisle: a door I never once used. The back stairs. I know the way now.
> *(sets `knows_backstairs` — the dawn shortcut)*

→ (chain end)

### SN_Act2_04_shaken · Shadow
> My voice hits it and comes back wrong, missing its middle. The whisper sharpens. A lamp
> dims somewhere without being touched.

→ (chain end)

### SN_Act2_04_line · Nour
> I say it like a student who did the reading. Straight lines, even through walls. The
> Shadow thins to a smudge and is gone down the dark aisle.

→ (chain end)

### SN_Act2_04_pass · Nour
> Chalk static. Folded chairs. It passes close enough to read the absent look on its
> absent face. I hold still until the school forgets me again.

→ (chain end)

## ACT III — DAWN

### SN_Act3_01 · trigger `FuseRestored 3` · Nour *(the mug)*
> Fuse three. The staff room lights up row by row, and there is her mug beside the sink,
> and a note she left a colleague: "Back Thursday — the kettle forgives you."

- "Pour the cold tea out first. Then take the mug." → set `poured_tea`, C+0.2, T+0.2 → SN_Act3_01b
- "Take the mug. The tea can be tomorrow's problem." → set `took_mug`, B+0.1 → SN_Act3_01b
- "Leave everything. The room remembers her better with it here." → set `left_all`, T+0.2 → SN_Act3_01b

### SN_Act3_01b · Mother
> *(memory flash)* "Keep the light on for the ones who come after."

→ (chain end)

### SN_Act3_02 · trigger `GatePowered` · System
> The gate crank is live. The corridor to the front doors is a lit runway. The Shadow
> knows it too.

→ (chain end)

### SN_Act3_03 · trigger `FragmentCollected 6` · Shadow *(the main hall — full confrontation)*
> Between me and the gate it has stopped pretending to be smoke. It is the school's whole
> forgetting, standing up: blank posters, erased names, a poem with its middle rubbed out.

- **[timed 10s]** "Say her name back to it. 'Her name was—'" → T+0.3 → calm-gated:
  `C ≥ 0` → SN_Act3_03_forgave (set `confronted_shadow`, `forgave_shadow`) ·
  `C < 0` → SN_Act3_03_shaken (set `confronted_shadow`, C−0.2) ·
  timeout: "The name won't come. Feet, then. Use feet." → SN_Act3_03_pass
- "Erase it first. Swing the beam like an answer." → set `erased_first`, B+0.4, T−0.2 → SN_Act3_03_bold
- "Walk past. It cannot enter the lit corridor." → set `walked_past`, C+0.1 → SN_Act3_03_pass

### SN_Act3_03_forgave · Shadow
> "…we kept everything. We kept—" It holds its shape one moment longer, the way a class
> holds its breath before the bell. Then it lets the morning go.

→ (chain end)

### SN_Act3_03_shaken · Shadow
> The name comes out with holes in it. The Shadow leans close — courteous, terrible — and
> takes the holes as its fee.

→ (chain end)

### SN_Act3_03_bold · Nour
> Light is a straight line and I am the line. It comes apart like chalk dust, and the dust
> is gone before it lands.

→ (chain end)

### SN_Act3_03_pass · Nour
> It stands aside the way furniture stands aside in a school you no longer attend. I keep
> to the light. I do not run. Not yet.

→ (chain end)

### SN_Act3_04 · trigger `DawnStarted` · Nour · **[timed 9s]**
> The windows are turning grey. The gate is one lit runway away, and my legs are a
> student's legs: they remember every corridor of this school.

- "Run the lit corridor. Straight lines only." → set `dawn_direct`, C+0.2 → SN_Act3_05
- req: `knows_backstairs` — "The back stairs I never used. Faster, darker. Mine." → set `dawn_bold`, B+0.3 → SN_Act3_07 *(the shortcut: skips the two corridor beats and their input lock)*
- timeout: "Decide with your feet." (set `dawn_direct`, C−0.1) → SN_Act3_05

### SN_Act3_05 · System
> The corridor lights come on ahead of me, one by one, like roll call.

→ SN_Act3_06

### SN_Act3_06 · Mother
> *(memory flash)* "Faster, habibti. The bell does not wait for the slow — and neither
> does the morning."

→ SN_Act3_08

### SN_Act3_07 · Nour *(back-stairs route)*
> The back stairs take the dark in jumps: three flights, a door with a push-bar, and the
> front hall roaring up to meet me.

→ SN_Act3_08

### SN_Act3_08 · Nour *(the crank — promise moment; `left_all` locks out the warm promises)*
> The crank is cold and eager. Before I turn it: one promise, out loud, because the school
> is listening, and schools keep records.

- "I'll keep the light on. For the ones who come after." → set `promised_light`, T+0.2 → SN_Act3_09
- "I'll come back with a key, not a fence to climb." → set `promised_return`, B+0.1 → SN_Act3_09
- req: `left_all` — "Say nothing. Some things should stay in the building." → set `promised_silence` → SN_Act3_09

### SN_Act3_09 · Mother
> *(memory flash — the last page of her lesson plans)* "Go on, then. Knowledge is the only
> light that follows a person into the dark."

→ (chain end) *(the player turns the crank; `EscapeStarted` fires; the ending is chosen)*

## ENDINGS (D13) — chosen at `EscapeStarted`, or at the second catch during dawn

Selection (constants in `SO_StoryConfig`, defaults):
`FullRecovery` = fragments == 6 AND accuracy ≥ 0.75 AND `confronted_shadow`;
`PartialMemory` = fragments ≥ 4 AND accuracy ≥ 0.5; else `HollowEscape`;
`Dawnless` = caught twice after `DawnStarted` (checked first — it interrupts).

### SN_End_FullRecovery · "FULL RECOVERY" · dawn tint: warm gold
> The gate closes behind her. The sky over the school turns from black to grey to a thin,
> clean gold.
>
> On the pavement she opens the box: bookmark, paper, jasmine, note, programme, mug. Six
> small true things. She reads the lesson plans to the last page — the one line she has
> been climbing toward all night.
>
> At a first-floor window the Shadow stands like a student kept after class, and watches
> the light walk away without her.
>
> Knowledge is the only light that follows a person into the dark. Tonight it followed
> Nour all the way out.

Narrator: "Six pieces, every answer held. The box is full, and the morning lets her carry it."

### SN_End_PartialMemory · "PARTIAL MEMORY" · dawn tint: pale grey-gold
> The gate closes. The dawn is ordinary and enormous.
>
> The box is not full. She can name the missing pieces — which is its own kind of
> remembering — and the pieces she has, she has read aloud, checked, and corrected, the
> way her mother taught her to study.
>
> Some of the voice is gone. Not all of it. Never all of it.
>
> She carries the box home the way you carry a lit lamp through wind: close, and careful.

Narrator: "She saved what she could hold. Some of the voice is gone; not all of it."

### SN_End_HollowEscape · "HOLLOW ESCAPE" · dawn tint: cold blue-grey
> The gate closes behind her with a sound like a full stop.
>
> She came for six pieces of a person and left with a handful. The box is light. Light
> boxes are easy to run with; that is the trouble.
>
> Behind her, the building opens its arms to the crews and gives up what it kept, and she
> will spend years half-remembering that there was something she meant to bring.
>
> The morning is very bright and there is almost nothing in her hands.

Narrator: "The gate, the morning, the road. The box barely weighs anything at all."

### SN_End_Dawnless · "DAWNLESS" · dawn tint: concrete grey, sun barely up
> The corridor ends at the gate and the gate does not open fast enough. The crews' vans
> turn the corner. The sky does its morning work.
>
> The Shadow reaches her not as a hand but as a hush: names gone quiet, a night folded up
> like a finished exam.
>
> She wakes on the pavement with the box still shut, and the school — patient, gentle,
> terrible — keeps her mother's memory the way a library keeps a book nobody borrowed.
>
> Dawn comes. It simply comes without her.

Narrator: "The night ran out. The school keeps what she could not carry."

## Flag lexicon (canonical)

`mood_steady` `mood_role` `mood_apology` · `card_pocket` `card_read_aloud` `card_left` ·
`looked_back` · `bookmark_boxed` `bookmark_kept` · `angered_shadow` · `sat_bench`
`left_jasmine` `took_all` · `optics_aloud` `optics_copied` · `recited_lesson` `hid_passed`
`confronted_shadow` `knows_backstairs` · `poured_tea` `took_mug` `left_all` ·
`forgave_shadow` `erased_first` `walked_past` · `dawn_direct` `dawn_bold` ·
`promised_light` `promised_return` `promised_silence`

Consequence spine: `knows_backstairs` unlocks the dawn shortcut choice and skips
SN_Act3_05/06 (less input lock inside the chase — a real gameplay advantage).
`left_all` unlocks the terse "say nothing" promise at the crank and is echoed by the
Hollow Escape / Dawnless epilogues (an empty box reads differently than a full one).

## Asset-graph addendum (build notes)

Three timeout outcomes are authored as explicit nodes so their flags/stats survive
timed misses (they are small Nour beats, chained onward):

- `SN_Act1_02t` — "The words stay behind my teeth. Maybe next time."
  (enter: set `card_pocket`, trust −0.1) → SN_Act1_02b
- `SN_Act2_02t` — "I take the photograph and keep standing. Standing is easier."
  (enter: calm +0.1) → SN_Act2_02b
- `SN_Act3_04t` — "Decide with your feet."
  (enter: set `dawn_direct`, calm −0.1) → SN_Act3_05

Calm-gated routing is data, not code: choices carry `nextIfCalmNegative`, so the
"call her name" / "answer it" / "say her name back" branches route to their harsh
variants whenever `calm < 0` at choice time. Runtime resolution lives in
StoryDirector.AdvanceFrom; no story text exists in any runtime C# file.
