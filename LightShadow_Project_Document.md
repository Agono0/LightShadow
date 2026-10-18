# LIGHT SHADOW — Complete Project Document

**A first-person educational horror game for Windows PC and Android, built in Unity 6 LTS — a "game for good" semester project.**

| Field | Entry |
| --- | --- |
| Project title | LIGHT SHADOW |
| Document version | 1.1 — planning baseline; adds Android as a second platform |
| Date | 【Date】 |
| University | 【University Name】 |
| Course | 【Course Code – Course Name】 |
| Instructor | 【Instructor Name】 |
| Team | 【Team Name】 |
| Members | 【Member Name 1】, 【Member Name 2】, 【Member Name 3】, 【Member Name 4】, 【Member Name 5】 |
| Repository | Private GitHub repository: 【Repository URL】 |

### Terminology note — read this first

Throughout this document, the word "AI" refers **only to external development assistants** — ChatGPT, Claude, and GitHub Copilot — that the team uses to draft code, debug errors, and write documentation (see Section 8). The game itself contains **no artificial-intelligence features of any kind**. The creature known as The Shadow is driven by a small, hand-written finite state machine, and this document always calls that system **"enemy behaviour"**, never "AI". Keeping this distinction clear matters both for academic honesty and for accurately describing the project in the final report.

## Contents

1. Executive Summary
2. Game Concept & Vision
3. Story & Setting
4. Core Gameplay & Mechanics
5. Team Organisation
6. Starting the Unity Project on Windows — Step by Step
7. Version Control — GitHub + Git LFS on Windows — Step by Step
8. AI-Assisted Development Plan (Our Workflow, Not a Game Feature)
9. Development Timeline (10 Weeks)
10. Testing, Risks & Deliverables
11. Glossary
12. How to Use This Document — Week 1 Checklist

---

## 1. Executive Summary

LIGHT SHADOW is a first-person horror game for Windows PC and Android in which knowledge, not weapons, keeps the player alive. The player controls Nour, a nineteen-year-old former student who sneaks into Al-Noor Secondary School on the final night before its demolition to recover the memory box of her late mother, a literature teacher at the school. The building is dark and unsafe, and a creature called The Shadow — a flickering smoky silhouette said to be made of everything students ever forgot — hunts anyone who wanders in the darkness. Nour's only protection is her mother's old hand-crank flashlight from the physics lab, and the flashlight is powered by knowledge: answering real general-knowledge multiple-choice questions charges a HUD gauge called the Bright Idea Meter, and a higher meter means a stronger, longer-reaching light.

The design is deliberately small. The game contains exactly three mechanics: first-person movement with sprint and crouch-hide, a flashlight with battery drain and battery pickups plus safe lit zones, and the Bright Idea Meter question system. There is no combat, no gore, and no fail state that ends the run: being caught fades the screen to black and returns the player to the last lit checkpoint. A session lasts twenty to forty minutes, in which the player restores three fuse boxes, collects six Knowledge Fragments tied to places Nour's mother loved, powers the main gate, and escapes at dawn. The end screen recaps every question the player answered together with explanations — a personal study sheet, which is the project's "game for good" heart.

A team of four to five Unity beginners will build the game in Unity 6 LTS (3D, Universal Render Pipeline) for Windows PC and Android over ten weeks, using free assets only. One project serves both platforms: keyboard and mouse on Windows, and a light touch-input layer on Android. All questions live in Unity ScriptableObjects so non-programmers can add and edit them without touching code. Development is AI-assisted but human-verified: ChatGPT, Claude, and GitHub Copilot produce first drafts and debugging help, while every line merged into the project is understood, documented, and tested by the team. Version control runs on GitHub with Git LFS from day one. The remainder of this document defines the story, mechanics, team roles, step-by-step Unity and Git setup, a ten-week timeline, a testing plan, a risk register, and a glossary — everything the team needs to start Week 1 without further planning work.

---
## 2. Game Concept & Vision

### 2.1 One-line pitch

Every pitch, poster, and submission form should repeat the same sentence, so the team agrees on it here and does not rewrite it later. The agreed pitch is:

> **Trapped in her dead mother's school on the night before demolition, a grieving student must recharge a knowledge-powered flashlight by answering real questions — because in LIGHT SHADOW, learning is the only light that follows a person into the dark.**

### 2.2 Fact sheet

The fact sheet below is the single-page reference for every form, slide, and conversation the team will have about the project during the semester. Print it and keep it with the project document.

| Item | Value |
| --- | --- |
| Working title | LIGHT SHADOW |
| Genre | First-person educational horror (no combat, no gore) |
| Platforms | Windows PC (keyboard + mouse) and Android 10+ (touch controls) |
| Engine | Unity 6 LTS (6000.0.x), 3D, Universal Render Pipeline (URP) |
| Perspective | First person |
| Players | Single player |
| Session length | 20–40 minutes per full playthrough |
| Content rating | Non-violent horror; bloodless "caught" failure; suitable for ages 13 and up |
| Business model | Free student project; not published commercially |
| Art and audio sources | Free assets only: Unity Asset Store free packs, Kenney.nl, freesound.org, Mixamo |
| Team size | 4–5 students, all Unity beginners |
| Schedule | 10 weeks, one deliverable per week (Section 9) |
| Question bank | 60+ human fact-checked questions across six school subjects |
| Course | 【Course Code – Course Name】 |
| Instructor | 【Instructor Name】 |
| Repository | Private GitHub repository 【Repository URL】 |

### 2.3 Why this is a "game for good"

Most educational games bolt a quiz onto a game that has nothing to do with learning, and players quickly learn to ignore the quiz. LIGHT SHADOW inverts that pattern: retrieval practice — recalling an answer from memory — is the survival mechanic itself. Each correct answer visibly brightens the flashlight, widens the safe radius around the player, and can repel the creature. The core loop of the game and the core loop of studying are therefore the same loop: recall a fact, check the answer, correct yourself, move forward. Cognitive science describes retrieval practice as one of the most reliable ways to strengthen memory, far more effective than re-reading notes, and the game trains exactly that skill under a mild twenty-second timer that feels like exam pressure without the exam stakes.

The "good" continues after the game ends. The end screen is a study sheet: it lists every question the player answered, the answer the player chose, the correct answer, and a one-line explanation, in reading order. A parent, a teacher, or the player themselves can photograph it and use it for revision. Because the questions are stored in editable ScriptableObject files, a teacher could later swap in a custom question set for their own subject without a programmer's help, which makes the project genuinely reusable beyond this course.

Finally, "game for good" also means harm-free horror. The game aims for tension through darkness, sound, and the risk of losing progress — never through gore, weapons, or jump-scare spam. Being caught fades to black bloodlessly and returns the player to a calm, lit checkpoint. The theme is sincere rather than grim: knowledge outlasts loss, and a mother's lesson plans literally carry her daughter through the dark. This tone makes the game suitable for a school showcase, for younger siblings of the developers, and for the university's public events.

### 2.4 Strict scope — and the cut list

The single greatest risk to a five-person beginner project is scope creep: the quiet accumulation of "one more small feature" until the deadline is lost. The table below is therefore a contract, not a suggestion. Every row marked IN is promised for the final build; every row marked CUT is forbidden until the final submission is handed in, no matter how easy it looks in a tutorial video.

| Feature | Status | Reason |
| --- | --- | --- |
| First-person movement, sprint, crouch-hide | IN | Core mechanic 1 |
| Flashlight with battery drain, pickups, and lit safe zones | IN | Core mechanic 2 |
| Bright Idea Meter question system (4 options, ~20 s timer) | IN | Core mechanic 3 |
| Scripted enemy behaviour (patrol → stalk → chase, repelled by light) | IN | Core tension |
| 3 fuse boxes, 6 Knowledge Fragments, gate escape ending | IN | The entire progression |
| Lit-room checkpoints with bloodless respawn | IN | The fail state |
| Study-recap end screen | IN | The educational core |
| Cross-platform builds: Windows and Android with a touch input layer | IN | Same three mechanics on both platforms; one input layer each |
| Combat, weapons, or gore | CUT | Breaks the non-violence pillar and costs weeks |
| Multiplayer or co-op | CUT | Networking is far beyond beginner scope |
| Crafting or inventory management | CUT | Adds UI and balancing cost with no educational value |
| Voice acting | CUT | Recording, directing, and mixing cost weeks; text and sound effects only |
| Cutscenes | CUT | Replaced by in-world notes and staged gameplay moments |
| Open world | CUT | One school building with three wings is already large |
| Multiple save slots | CUT | Checkpoint autosave only |
| iOS, consoles, or any platform beyond Windows and Android | CUT | Two targets is already the right scope for ten weeks |
| Achievements, localization beyond English | CUT | No gameplay value for the course; English only |

If a new idea appears during the ten weeks, it must obey the "one in, one out" rule: a feature may enter scope only if a feature of equal cost leaves scope, and only with the Team Lead's written approval recorded in the development log. This rule keeps meetings short, protects the timeline, and gives the team a polite, documented way to say no to good ideas at the wrong time.

---
## 3. Story & Setting

### 3.1 Setting — Al-Noor Secondary School, the last night

The entire game takes place inside Al-Noor Secondary School, known to generations of students as the School of Light. It is the final night before demolition: the power has been cut wing by wing, the furniture of the upper floors sits under dusty white sheets, and warning notices curl on the fence outside. The building is the second character of the game. It is laid out as three wings — the Library wing, the Science wing, and the Main Hall wing — around a central courtyard, with the main gate at the front of the building. Every place Nour's mother loved inside this building has a role in the gameplay, because each one hides a Knowledge Fragment. The night itself is the clock: when dawn comes, the demolition crews arrive, so the game's pacing naturally rises from quiet exploration to a final run toward the gate as the sky lightens.

### 3.2 Protagonist — Nour

Nour is nineteen, a former student who finished at Al-Noor the year before the story begins. Her mother taught literature at the school for many years and died in a car accident on the last day of term — and since that night, Nour's memories of her have been fading: the exact sound of her voice, the sayings she repeated, the way her classroom smelled. What remains is a memory box kept in the library: her letters, photographs, and lesson plans. On the eve of demolition, Nour climbs the fence to bring the box home before the building disappears. Her flashlight is the old hand-crank model her mother used for physics demonstrations, borrowed years ago and never returned. Nour's arc across the night is quiet but real: she arrives to retrieve objects and leaves having recovered memory itself, reassembled from the fragments the school still holds.

### 3.3 The Shadow

The Shadow is the game's single antagonist, and its design keeps the horror symbolic rather than violent. It is a human-shaped silhouette of flickering smoke, its edges smudged like the mark of an eraser, trailed by whispering and a faint static like chalk on a board. The school's legend says it is made of everything students ever forgot there: half-remembered poems, abandoned answers, names erased from attendance lists. Where it passes, it "unremembers" the world — posters go blank and lamps dim — a detail the team can implement cheaply by swapping materials on a few surfaces. Mechanically it hunts in darkness, slows near light, and cannot enter lit rooms. If it catches the player, the screen simply fades to black and Nour wakes at the last lit checkpoint: no blood, no death animation, no punishment screen. The Shadow is frightening because it erases, not because it harms.

### 3.4 The light — the Bright Idea Meter

Nour's flashlight is an old hand-crank model from the physics lab, and it is the emotional and mechanical heart of the game. Its crank does not charge from muscle; it charges from recall. Every time Nour speaks a true fact aloud — that is, every time the player answers a real general-knowledge question correctly — the crank spins a little and the light burns stronger. Wrong answers let the light gutter. This single fiction explains every rule of the gameplay: knowledge is fuel, forgetting is darkness, and remembering is survival. The technical side of this fiction is split into two gauges described in Section 4.2: a physical battery (fuel) and the knowledge-driven Bright Idea Meter (amplification).

### 3.5 The story in three acts

The night is structured as three acts, each with a clear player goal and one new system fully introduced. The table below summarises the acts; the paragraphs after it describe each one in detail.

| Act | Title | Estimated time | Player goal | Systems introduced |
| --- | --- | --- | --- | --- |
| I | Locked In | ~10 min | Reach the library, take the empty memory box, restore Fuse Box 1 | Movement, flashlight and battery, first quiz boards, Fragments 1–2, first checkpoint |
| II | The Dark Wings | ~15–20 min | Restore Fuse Boxes 2 and 3, gather Fragments 3–5 | Enemy behaviour fully active (stalk and chase), battery pickups, harder questions |
| III | Dawn | ~5–10 min | Complete the memory box, power the main gate, survive the chase and escape | Scripted final chase, ending, study recap |

**Act I — Locked In.** Nour climbs the fence at dusk, and the main gate slams shut behind her — a scripted in-world moment, not a cutscene. The entrance hall teaches the basics: walking, crouching under a fallen cabinet, switching on the flashlight with F, and answering the first quiz board to discover that light is earned with knowledge. In the library she finds the memory box on her mother's desk, empty except for a single card in her mother's handwriting: "Put me back together." Restoring Fuse Box 1 floods the Library wing with warm light and creates the first checkpoint. Fragments 1 and 2 — the pressed-flower bookmark and the marked exam paper — are recovered here. The Shadow appears only once, far down a corridor, as a silhouette that slides away when the beam touches it.

**Act II — The Dark Wings.** Beyond the lit wing the school is dark, and the Shadow is fully active: it patrols, stalks in darkness, and chases when it sees Nour. The Science wing yields the physics-lab fragment and Fuse Box 2; the garden bench yields the pressed jasmine and a photograph; the assembly hall yields the poetry-evening programme and Fuse Box 3 in the Main Hall wing. Each restored fuse box brings light, a checkpoint, and a one-line "memory flash" printed on the screen — a sentence in the mother's voice, with no voice recording, only text. Battery pickups become scarce, forcing the player to ration the flashlight and plan routes between lit rooms.

**Act III — Dawn.** With Fuse Box 3 restored, the staff-room fragment completes the memory box, and the powered gatehouse unlocks the way out. As Nour turns the gate crank, the Shadow makes its last surge: a straight chase down the main corridor toward the gate, through windows already turning grey with dawn. The corridor is lined with the school's lights, so the chase is tense but fair — a designed safe path, not a cutscene. Outside, Nour opens the box and reads her mother's lesson plans; the last page reads: **"Knowledge is the only light that follows a person into the dark."** The screen fades to the study recap: every question of the night, every answer, every explanation.

### 3.6 Cast and the six Knowledge Fragments

Three figures carry the story. Nour is the player's hands and eyes; her grief is shown through interaction, never through dialogue. Her mother exists only in handwriting, lesson plans, and the places she loved — a presence built from props and text, which suits a team with no voice actors. The Shadow is described in Section 3.3 and behaves exactly as specified in Section 4.3. The six Knowledge Fragments are the story's connective tissue: each is a small object tied to a place the mother loved, and collecting one triggers a themed question. The table below defines them, and this list doubles as the Level Designer's placement contract.

| # | Fragment item | Place the mother loved | Question theme |
| --- | --- | --- | --- |
| 1 | A pressed-flower bookmark with her handwriting | Library reading corner | Literature and language |
| 2 | A marked exam paper with an encouraging note | Her literature classroom | Language |
| 3 | A pressed jasmine flower and a photograph | The school garden bench | Science (plants) |
| 4 | A note pinned inside the optics cabinet: "Light travels in straight lines — so do stories." | The physics lab | Physics |
| 5 | The programme of the student poetry evening | The assembly hall stage | History and culture |
| 6 | Her mug and a note she left for a colleague | The teachers' staff room | General knowledge |

---
## 4. Core Gameplay & Mechanics

LIGHT SHADOW contains exactly three mechanics: first-person movement with sprint and crouch-hide, the flashlight with battery and safe zones, and the Bright Idea Meter question system. Everything else in the game is level design, content, or presentation built on top of those three. This section specifies each system precisely enough to implement: the Bright Idea Meter loop in 4.1, the light and darkness rules in 4.2, the enemy behaviour states in 4.3, objectives and win/lose conditions in 4.4, the full controls table in 4.5, and a features table that states the educational purpose of every feature in 4.6. Where numbers appear, they are starting values to be tuned during Week 8, not permanent promises.

### 4.1 The Bright Idea Meter loop

The Bright Idea Meter is the HUD gauge that turns learning into light. It is a simple number from 0 to 100, and the loop that feeds it runs as follows:

1. The player finds a wall-mounted quiz board (usually in or near a lit room) or collects a Knowledge Fragment. A question appears with four options and a twenty-second timer.
2. The world does **not** pause while the question is on screen. Answering in darkness is risky; answering inside a lit room is safe. This is the game's central risk-reward decision.
3. The player clicks, taps, or presses keys 1–4.
4. A correct answer adds +25 to the meter, plays a short crank-spin sound, and briefly flares the light. The question, the correct answer, and a one-line explanation are stored for the end screen.
5. A wrong answer or a timeout subtracts −10 from the meter and dims the light. The correct answer and explanation are still shown for three seconds — the player learns from errors even when losing.
6. While the flashlight is on, the meter drains slowly, so light is always being "spent" and must be re-earned.
7. The meter value maps directly to light strength (Section 4.2): higher knowledge means a wider, brighter cone, which means a larger safe radius and a better chance to repel the Shadow.

Question content comes from six school categories — Science, Math, History, Geography, Language, and Health — and every question asset also stores its category, a one-line explanation, and a fact-source field pointing at the textbook or reference the team used to verify it. Difficulty rises across the acts: Act I questions are simple recall, Act II questions apply knowledge, and Act III mixes both. This ramp is implemented as a difficulty tag on each question asset rather than as code.

The starting tuning values are collected in the table below so the whole team can discuss changes from one page. All of these numbers live in a single GameConfig ScriptableObject in the project, so tuning during Week 8 requires no code changes at all.

| Parameter | Starting value | Notes |
| --- | --- | --- |
| Meter range | 0–100 | Starts every session at 60 |
| Correct answer | +25 | Brief light flare as reward feedback |
| Wrong answer or timeout | −10 | Correct answer still taught for 3 seconds |
| Passive drain (flashlight on) | 0.5 per second | Roughly 80 seconds from full to empty if never answered |
| Light radius mapping | 4 m to 12 m | Linear in meter value |
| Light intensity mapping | 1.5 to 3.5 | Linear in meter value |
| Question timer | 20 seconds | Timeout counts as a wrong answer |

### 4.2 The light and darkness system

Light in the school exists in three layers, and understanding them is the key to both the level design and the fear pacing. The first layer is **baked room lighting**: the lamps of restored wings cast warm, pre-computed light that is cheap to render and marks safety. The second layer is **the flashlight**: a single real-time spotlight attached to the camera, the only mobile light, and therefore the only light that matters in un-restored areas. The third layer is **lit safe zones**: small trigger volumes around working lamps and checkpoints inside which the Shadow cannot enter, giving the player reading rooms and decision points rather than constant panic.

The flashlight itself has two gauges that must both be alive for light to exist. The **battery** is physical fuel: it drains while the flashlight is on (about five minutes of continuous use per full charge), and battery pickups restore fifty percent. When the battery reaches zero, the light dies no matter how much the player knows. The **Bright Idea Meter** is the amplifier of that fuel: the effective light of the flashlight equals its base output multiplied by the meter factor and the battery factor, so knowledge and fuel both matter and neither can replace the other. This creates the intended tension loop — batteries force exploration, and knowledge makes every battery count.

Every area of the school also exposes a simple **light level** value from 0 to 1, which the enemy behaviour reads. Lit rooms report 1.0; fully dark rooms report 0; corridors report the flashlight's current effective strength. The player's light level decides when the Shadow stalks, when it loses interest, and whether the beam can repel it (Section 4.3). Each lit room is also a checkpoint: stepping inside saves the player's progress, so the map itself teaches the player to move between islands of light.

### 4.3 Enemy behaviour — the Shadow's states

The Shadow is a small finite state machine: a handful of if-statements, a NavMeshAgent, and nothing more. It is scripted, deterministic, and testable, and the team always calls this system "enemy behaviour" — the game contains no AI features. The Unity package used for pathfinding is called "AI Navigation"; the name is historical, and it simply provides the NavMesh grid the Shadow walks on. The table below is the complete behaviour contract; anything not listed here is not implemented.

| State | Enters when | What it does | Exits when |
| --- | --- | --- | --- |
| Patrol | At spawn, after losing the player, or after a repel cooldown | Walks a loop of waypoints at 1.5 m/s | Player within 12 m AND player light level below 0.3 → Stalk |
| Stalk | Player is nearby in darkness | Stops and faces the player, plays a breathing audio cue as a fair warning, creeps at 2.0 m/s | Line of sight within 15 m → Chase; player light reaches 0.3 or player leaves the radius → Patrol |
| Chase | It sees the player | Moves toward the player at 4.0 m/s (player walks 3.0, sprints 5.0) | Distance above 18 m or sight lost for 5 s → Patrol; beam repel → Repelled |
| Repelled | The flashlight beam hits it while the meter is above 30 | Halts, recoils 8 m backward, plays a whisper burst | 6-second cooldown → Stalk (never directly back into Chase) |

If the Shadow reaches within 1.2 metres of the player, a Caught event fires: the screen fades to black over one second, the player respawns at the last lit checkpoint with the meter reduced by 15, and the Shadow returns to Patrol at its nearest waypoint. There is no death interface and no gore; instead, a chalk scribble near each checkpoint quietly narrates progress ("It knows the corridors better than you do."). On the technical side, the Shadow uses a NavMeshAgent on a baked NavMeshSurface, line of sight is a single raycast from its head to the player camera tested against wall colliders, and lit-zone doors carry trigger volumes that end any approach — which is exactly the "cannot enter lit rooms" rule of the fiction.

### 4.4 Objectives and win/lose conditions

The player's whole night is covered by four objectives, and the objectives panel (Tab) shows their live counts. The table below lists them together with their locations, and it doubles as the Level Designer's checklist for Section 6's level scene.

| Objective | Count | Where |
| --- | --- | --- |
| Restore fuse boxes | 3 | Library wing (Act I), Science wing (Act II), Main Hall wing (Act II) |
| Collect Knowledge Fragments | 6 | The six locations listed in Section 3.6 |
| Power the main gate | 1 | Gatehouse console, enabled once all three fuse boxes are restored |
| Escape at dawn | 1 | The scripted corridor chase from the main hall to the front gate |

The win condition is crossing the main gate after the dawn chase, which triggers the ending text and then the study-recap screen. The lose condition deliberately does not exist in the permanent sense: being caught costs the player fifteen meter points and a walk back from the checkpoint, but the night always continues. The real pressures are scarcity and time — batteries are finite, the Shadow grows bolder in later acts, and the session is designed to end within twenty to forty minutes. This design keeps frustration low for beginners and non-gamers while preserving genuine fear, and it means no playtest session can ever end in an unfinishable state.

### 4.5 Controls

The control scheme fits on one page and should never grow during the semester, because every extra input adds tutorial, UI, and testing cost. Because the game ships on two platforms, the table below defines each action twice: the keyboard-and-mouse binding for Windows and the touch equivalent for Android. Both input layers feed the same underlying functions, so the game plays identically and only the input layer differs.

| Input action | Windows (keyboard + mouse) | Android (touch) |
| --- | --- | --- |
| Move | W A S D, relative to the camera | Left virtual joystick |
| Look | Mouse | Drag on the right half of the screen |
| Sprint | Hold Left Shift | Hold the sprint button |
| Crouch / hide | Hold Left Ctrl | Toggle the crouch button |
| Flashlight on / off | F | Tap the flashlight button |
| Interact | E | Tap the highlighted interactable or the interact button |
| Objectives panel | Tab | Tap the objectives button |
| Pause menu | Esc | Tap the pause button |

On the Android side, the touch layer is deliberately one small script and one canvas: a joystick on the left for movement, drag-to-look on the right half of the screen, and a compact row of buttons for the remaining actions, sized for thumbs and laid out to respect notches and safe areas. The tuning notes carry over exactly: sprint moves at 5.0 m/s and makes noise the Shadow can hear within 8 m; crouch cuts speed to 1.5 m/s and shrinks the player's profile behind furniture; the flashlight button drains the same battery as F; the objectives button shows the same live counts Tab shows; and pause stops the world on both platforms. Answering quiz questions is naturally touch-friendly — four large buttons are easier to tap than to click — which is part of why the Bright Idea Meter loop transfers so well to phones, and because sprint, crouch, and light behave identically on both platforms, the enemy behaviour and level design never need platform-specific tuning.

### 4.6 Gameplay features and their educational purpose

Because this is a "game for good" project, every feature must justify itself educationally as well as financially. The table below maps each gameplay feature to what the player actually does and to the learning purpose it serves, and it is the reference the team uses when any feature's place in the game is questioned during the semester.

| Feature | What the player does | Educational purpose |
| --- | --- | --- |
| Bright Idea Meter quiz boards | Answers four-option questions under a twenty-second timer | Retrieval practice under mild pressure — the exam condition, rehearsed safely |
| Six Knowledge Fragments | Explores meaningful places, reads short text, answers a themed question | Reading comprehension and curiosity-driven learning |
| Explanation pop-ups | Reads why the right answer was right, for three seconds after every question | Immediate feedback and error correction, the moment learning is strongest |
| Study-recap end screen | Reviews every question, chosen answer, and explanation | Converts the play session into a personal, printable study sheet |
| Battery management | Rations flashlight use and plans routes between pickups | Resource planning and prioritisation |
| Lit safe zones and checkpoints | Learns where safety lies and plans movement around it | Spatial memory and route planning |
| Crouch-hide versus sprint | Chooses between stealth and speed in each encounter | Risk assessment and decision making |
| Fuse box sequence | Restores wings in a chosen order toward a distant goal | Goal decomposition and delayed gratification |
| Difficulty ramp | Meets harder questions deeper in the school | Scaffolded, escalating challenge |

---
## 5. Team Organisation

A five-person beginner team succeeds or fails on clarity of ownership, not on talent. Every member therefore has one primary role, a defined set of scenes and folders they own, and a fixed place in the weekly rhythm. Ownership is what prevents the two classic student-project disasters: two people overwriting each other's work in the same Unity scene, and nobody noticing until the week's demo that a core system quietly broke. If the team has only four members, the adaptation rule in 5.1 applies.

### 5.1 Roles and ownership

The roles table below assigns one row per member. The "Owns" column is not decoration — it is the authority list used by the merge rules in Section 7.8, and it maps directly onto the folder structure created in Section 6.6.

| # | Role | Member | Core responsibilities | Owns (scenes / folders) |
| --- | --- | --- | --- | --- |
| 1 | Team Lead & Gameplay Programmer | 【Member Name 1】 | Schedule and meetings, final say on scope, integration of all systems; scripts for player controller, flashlight, Bright Idea Meter, game manager, checkpoints, and the mobile touch input layer | 00_Bootstrap and 01_MainMenu scenes; Scripts/Core and Scripts/Player |
| 2 | Level Designer & Enemy Behaviour | 【Member Name 2】 | Greybox-to-final school layout; enemy behaviour scripts, waypoints, NavMesh baking, lit zones, placement of fuses, fragments, and pickups | 02_School scene; Scripts/Enemy and Scripts/Gameplay |
| 3 | UI & Question Systems | 【Member Name 3】 | HUD (meter, battery, objectives), quiz board UI, recap screen; the QuestionData pipeline and question-bank tooling | Scripts/UI; Data/Questions; all UI prefabs |
| 4 | Art & Audio | 【Member Name 4】 | Sourcing and importing free assets, materials and textures, lighting bakes, sound effects, music, ambience mix, performance budgets on both platforms | Prefabs/Environment; Materials, Textures, Audio, Lighting folders |
| 5 | Writer & QA | 【Member Name 5】 | Story text and in-world notes; authoring questions and running the human fact-check; executing the test checklist on Windows and on the team's Android test device, running playtests, maintaining the dev log | Story text content; question review; testing reports |

If the team has only four members, the fifth role is dissolved rather than skipped: Art & Audio keeps its owner, the writing and question-authoring duties split between the Team Lead (story text) and the UI & Question Systems member (question authoring), and the QA checklist runs as a rotating weekly duty owned by whoever did not merge code that week. What may never be dissolved is the scene-ownership rule in 5.3, because it is the only thing standing between the team and unmergeable scene conflicts.

### 5.2 Weekly rhythm

The team's output depends less on long coding sessions than on a fixed, predictable rhythm of planning, building, and integrating. The table below defines the weekly slots; the 【Day/Time】 placeholders are filled during the Week 1 meeting and then never move, because a rhythm that moves is not a rhythm.

| Slot | When | Length | Purpose |
| --- | --- | --- | --- |
| Planning meeting | 【Day, Time】 | 30 min | Review last week's deliverable, assign this week's tasks in pairs, update the risk list |
| Work sessions | 2–3 per week | ~2 h each | Paired work on the week's tasks; two people per task wherever possible |
| Merge & demo session | 【Day, Time】 | 60–90 min | Merge approved pull requests, build the project once, play the week's slice together, log bugs |

All merges to the main branch happen at the merge and demo session, never alone late at night, because that is when unreviewed code and broken scenes sneak in. The weekly build produced at that session is the team's single shared truth: what is in it is the game, and everything else is a work in progress. If a member cannot attend a session, they watch the session's notes in the team chat and sync their branch the next day.

### 5.3 Scene-ownership rules

Unity scene files do not merge safely, so the team prevents conflicts socially before Git ever has to cope with them technically. The following rules are binding for the whole semester:

1. One scene, one owner. Only the owner listed in 5.1 edits a given scene file; anyone else requests the change from the owner.
2. Contributions happen through prefabs. A member who wants a new prop, enemy variant, or UI element in someone else's scene builds it as a prefab in their own folder and asks the owner to drop it in.
3. Shared scenes (00_Bootstrap) are changed only by the Team Lead, because every other scene depends on them.
4. Announce before touching. A short chat message ("editing 02_School for fuse-box 3 placement, one hour") prevents accidental parallel edits.
5. Scene conflicts are fixed by the scene owner at the merge session. Nobody hand-edits the text of a .unity file to resolve a conflict; if a conflict truly happens, the owner rebuilds the lost part from prefabs (see also the UnityYAMLMerge note in 7.8).
6. Ownership can be traded for a week when workload demands it, but the trade must be announced in chat and recorded in the dev log.

---
## 6. Starting the Unity Project on Windows — Step by Step

This section is the setup script for Week 1, Day 1, and it should be executed once with every laptop in the same room, because setup errors caught together cost minutes while setup errors caught alone cost days. Every member follows every step on their own machine. The golden rule of the whole section: **everyone installs exactly the same Unity version**, because a project opened by a different editor version can be silently upgraded and then corrupt the repository for everyone else.

### 6.1 Prerequisites

Each laptop needs Windows 10 or 11 (64-bit), roughly 20 GB of free disk space (the Android build tools add a few gigabytes), a mouse (trackpads make first-person testing miserable), internet access, and permission to install software. The team also needs one mid-range Android phone (Android 10 or newer) with its USB cable, nominated in Week 1 as the official test device that comes to the weekly sessions. No paid products are used anywhere in this project. If any member's laptop cannot free the space, the team should solve that before Week 1 ends rather than discovering it at the first import.

### 6.2 Step 1 — Install Unity Hub

Download Unity Hub from unity.com/download and install it with the default options, then launch it and sign in with a Unity ID (a free account). In Hub, open Preferences or the Licenses page and activate the free personal licence, which is required before the editor will open and is free for students and small teams. Unity Hub is only a manager: it installs and updates editors and creates projects, so every member should end this step with the same Hub version and a green licence indicator.

### 6.3 Step 2 — Install Unity 6 LTS (the same version for everyone)

In Unity Hub go to Installs → Install Editor and choose the latest **Unity 6 LTS** release — its version number looks like **6000.0.xx LTS**. Do not choose 6000.1, 6000.2, any "beta" or "alpha" label, or an older 2021/2022 editor; the team standard is the newest 6000.0 LTS patch. On the modules page, Windows Build Support is already included; tick **Android Build Support** and, inside it, **Android SDK & NDK Tools** and **OpenJDK** — these are the tools that let the editor build the Android version. Tick Documentation if disk space allows. When the install finishes, open Help → About Unity, take a screenshot of the exact version string, and post it in the team chat — every member's screenshot must match. If someone already has a different version installed, they still install the team version; opening the project "just to look" with the wrong editor is how projects get corrupted.

### 6.4 Step 3 — Create the project

In Unity Hub click New Project, select the **Universal 3D** template (URP) — with Android now a shipping target, URP is the right pipeline: it is tuned for mobile performance, keeps the same scene and lighting workflow the team already plans around, and matches what current free Asset Store packs are built for. Name the project exactly **LightShadow** with no spaces, set the location to 【D:\Projects】 or equivalent, and click Create project. The first open takes several minutes while Unity imports packages and compiles shaders; this is normal. One tip for later imports: if a free pack's materials look bright pink, the pack ships Built-in-pipeline shaders only — convert it once with Window → Rendering → Render Pipeline Converter (Material Conversion), or prefer a URP-compatible pack. When the editor opens, everyone should see the default empty scene.

### 6.5 Step 4 — Version-control settings (do not skip)

Two project settings make Unity compatible with Git, and both must be verified on every machine before the first commit. Open Edit → Project Settings → Version Control and set **Version Control mode to "Visible Meta Files"**; then open Edit → Project Settings → Editor and set **Asset Serialization mode to "Force Text"**. The first setting makes Unity's .meta files visible and tracked as real files, and the second makes scenes and assets save as readable text instead of binary, which is what allows Git to show meaningful differences and occasionally even merge them. New Unity 6 projects often default to exactly these values, but verify anyway — an invisible default changed by one member's older settings file is a classic source of "your Unity works, mine doesn't" afternoons.

### 6.6 Step 5 — Create the folder structure

A fixed folder structure decided before any asset exists is the cheapest form of teamwork. All of the team's own work lives under Assets/_Project (the leading underscore keeps it sorted to the top of the Project window), and all imported third-party packs live under Assets/ThirdParty, where they are never edited in place. No script is ever left loose in the Assets root. Create the following tree exactly:

```text
LightShadow/                     ← project root (this folder is the Git repository)
├── Assets/
│   ├── _Project/                ← everything the team makes lives here
│   │   ├── Scenes/
│   │   │   ├── 00_Bootstrap.unity
│   │   │   ├── 01_MainMenu.unity
│   │   │   ├── 02_School.unity
│   │   │   └── 03_Ending.unity
│   │   ├── Scripts/
│   │   │   ├── Core/            ← GameManager, CheckpointManager, GameConfig, AudioManager
│   │   │   ├── Player/          ← PlayerController, FlashlightBattery, BrightIdeaMeter, InteractionRaycaster, TouchInput
│   │   │   ├── Enemy/           ← EnemyBehaviour, PatrolRoute, EnemySight   (scripted "enemy behaviour", not AI)
│   │   │   ├── Gameplay/        ← FuseBox, KnowledgeFragment, BatteryPickup, ExitGate, LitZone
│   │   │   └── UI/              ← HUDController, QuestionData, QuizBoard, QuizUIController, RecapScreen, PauseMenu
│   │   ├── Prefabs/
│   │   │   ├── Player/
│   │   │   ├── Enemy/
│   │   │   ├── Interactables/   ← quiz boards, batteries, fuse boxes, fragments
│   │   │   └── UI/
│   │   ├── Data/
│   │   │   └── Questions/       ← QuestionData .asset files, one file per question (Q_Sci_001 …)
│   │   ├── Materials/
│   │   ├── Models/
│   │   ├── Textures/
│   │   ├── Animations/
│   │   ├── Audio/
│   │   │   ├── Music/
│   │   │   ├── SFX/
│   │   │   └── Ambience/
│   │   └── Lighting/            ← lighting settings assets and notes on baked light data
│   └── ThirdParty/              ← imported free packs, NEVER edited in place
│       ├── Kenney_…/
│       └── AssetStore_…/
├── Packages/                    ← Unity-managed (manifest.json) — committed, never hand-edited
├── ProjectSettings/             ← committed (this is where Force Text settings live)
└── README.md                    ← short team readme: how to open, build, and play
```

Two conventions inside this tree are worth stating explicitly. First, question-pipeline scripts (QuestionData.cs, QuizBoard.cs, QuizUIController.cs, RecapScreen.cs) live in Scripts/UI so the non-programmer who works on questions only ever opens one folder. Second, ThirdParty packs are treated as read-only: if a pack's material needs adjusting, copy the asset into _Project/Materials and adjust the copy, so the original pack can be re-imported or replaced without breaking anything.

### 6.7 Step 6 — Naming conventions

Names are the team's shared search engine: with ten weeks and thousands of assets, an asset that cannot be found by typing half its name does not exist. The table below defines the patterns; the rules after it are short but strict.

| Asset type | Pattern | Example |
| --- | --- | --- |
| Scenes | NN_PascalCase.unity | 02_School.unity |
| Scripts | PascalCase, file name = class name | FlashlightBattery.cs |
| Prefabs | PascalCase noun | BatteryPickup |
| Materials | Mat_PascalCase | Mat_LockerDoor |
| Textures | T_PascalCase | T_LockerDoor_D |
| Models | SM_PascalCase (static) / SK_PascalCase (skinned) | SM_ClassDesk |
| Animations | Anim_PascalCase | Anim_ShadowIdle |
| Audio | MUS_ / SFX_ / AMB_ + PascalCase | SFX_CrankSpin |
| Question assets | Q_[Cat]_[Number] | Q_Sci_014.asset |
| UI sprites | UI_PascalCase | UI_MeterFill |

File names use English and no spaces, because mixed-language and spaced file names are a reliable source of encoding and tooling bugs on Windows. Version suffixes like "final", "v2", and "new" are forbidden — Git history is the versioning, and a file called MainMenu_Final_v3 means the team has already lost track of which one is real.

### 6.8 Step 8 — Configure the two build targets (Windows and Android)

Unity 6 manages build targets through Build Profiles: open File → Build Profiles, keep **Windows** as the active profile for daily work, and add an **Android** profile by clicking Switch Platform once the Android module from Step 2 is installed. The first switch re-imports the whole project and takes several minutes, so do it once, together, and then leave daily development on Windows with a weekly Android check. With the Android profile selected, set the player settings in the table below; each row exists because the default value would otherwise cost the team a weekend of debugging.

| Setting (Android player settings) | Value | Why |
| --- | --- | --- |
| Company / Product name | 【Team Name】 / LightShadow | What appears on the phone's app list |
| Scripting Backend | IL2CPP | Required for modern Android builds |
| Target Architectures | ARM64 | Matches current Android phones |
| Minimum API Level | 29 (Android 10) | Matches the fact sheet's Android 10+ target |
| Texture Compression | ASTC | Best quality-to-size balance on modern phones |
| Default Orientation | Landscape Left + Right | The HUD and quiz UI are designed for landscape |
| Active Input Handling | Both | Lets the classic keyboard/mouse code and the touch layer coexist |
| URP quality (Android) | Mobile preset: render scale about 0.8, real-time shadows off | Keeps the baked-lighting look at a stable frame rate on mid-range phones |

The URP mobile preset lives in Edit → Project Settings → Quality, where the team keeps one high-quality URP asset for Windows and one lighter one for Android; the flashlight, baked lighting, and Bright Idea Meter behave identically on both. Finally, verify the setup on both targets: build an empty Windows executable, switch to Android, build an empty APK, enable USB debugging on the test device, install the APK, and confirm it opens to the empty scene.

### 6.9 Step 9 — Verify everyone's setup

Each member creates a local test scene, drops a cube into it, and presses Play; if anyone's editor shows errors, red Console messages, or refuses to enter Play mode, the room fixes that machine before moving on. Verification now covers both targets: after pressing Play, switch once to the Android build profile (6.8), let the re-import finish, and confirm the Console is still clean. When every laptop can open the project and press Play without errors on both platforms, the Team Lead announces the setup frozen: from this point, nobody installs a different editor version or adds a package without the Team Lead's approval recorded in the dev log.

---
## 7. Version Control — GitHub + Git LFS on Windows — Step by Step

Version control is the team's safety net and its collaboration backbone, and it must be installed before the first real commit, not after the first accident. Plain Git handles text well but chokes on the large binary files Unity projects contain — textures, models, and audio — which is why this project uses **Git LFS (Large File Storage)**: it stores big files efficiently on GitHub while keeping the daily workflow identical to normal Git. The setup below takes about thirty minutes per machine and is done together in Week 1.

### 7.1 Why Git, GitHub, and LFS specifically

Git keeps a complete history of every change, so any mistake is revertable and no one ever loses a week of work to a corrupted laptop. GitHub hosts the repository privately and provides the pull-request workflow the team uses for review (7.7). Git LFS solves the one problem plain Git has with game projects: binary assets do not compress or merge, and committing them normally bloats the repository until it takes hours to clone. With LFS, the team's images, audio, and models are stored as lightweight pointers in history with the real files managed by LFS. Note that GitHub's free tier includes LFS with a limited monthly storage and bandwidth quota (about 1 GB each at the time of writing — verify on GitHub's current pricing page), which is why Section 10 includes LFS quota as a tracked risk and why the team keeps assets lean.

### 7.2 Step 1 — Install Git for Windows

Download Git for Windows from git-scm.com/download/win and run the installer. Accepting the defaults is fine for this project, with one choice worth checking: on the "Adjusting your PATH environment" screen, keep the recommended option ("Git from the command line and also from 3rd-party software") so that git works from any terminal. The installer also installs **Git Bash**, which the team uses for the commands below. Verify by opening Git Bash and running:

```bash
git --version
```

If a version number prints (for example git version 2.5x), the installation worked.

### 7.3 Step 2 — Install Git LFS and configure your identity

Download Git LFS from git-lfs.com (the Git for Windows installer usually bundles it), then verify and activate it once for your user account:

```bash
git lfs version
git lfs install
```

Then set the identity that will be stamped on every commit the team can read who did what:

```bash
git config --global user.name "【Your Name】"
git config --global user.email "【your.email@example.com】"
```

Two more Windows-specific settings make life easier and are set once: `git config --global core.autocrlf true` keeps line endings consistent between machines, and `git config --global core.longpaths true` prevents Windows path-length errors on deeply nested Unity assets. All four settings are per machine, and every member runs them.

### 7.4 Step 3 — Create the private GitHub repository

One member (the Team Lead) creates the shared repository on github.com: click New repository, name it **LightShadow**, set visibility to **Private**, and — important — leave every initialisation checkbox unticked (no README, no .gitignore, no licence), because the repository must be empty so it can accept the team's existing project without conflicts. After creating it, the Team Lead invites the other members under Settings → Collaborators, and everyone accepts the invitation e-mail. The repository URL looks like https://github.com/【your-github-user-or-team】/LightShadow.git.

### 7.5 Step 4 — Place .gitignore and .gitattributes BEFORE the first commit

Two text files must exist in the project root (next to Assets/) before anything is committed, because they decide what Git will and will not track. The .gitignore below keeps Unity's machine-generated folders out of the repository, and the .gitattributes routes large binaries through Git LFS. Copy them exactly as given.

The .gitignore file — create it in the project root with exactly this content:

```text
# ===== LIGHT SHADOW — Unity .gitignore =====
# Place this file in the project ROOT (next to Assets/) BEFORE the first commit.

# --- Unity generated folders (regenerated locally; NEVER commit) ---
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Ll]ogs/
/[Uu]ser[Ss]ettings/
/[Mm]emoryCaptures/
/[Rr]ecordings/
/[Cc]rashes/

# --- Visual Studio / Rider / VS Code caches and generated solutions ---
.vs/
.idea/
ExportedObj/
.consulo/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db

# --- Unity crash reports ---
sysinfo.txt

# --- OS junk ---
.DS_Store
Thumbs.db

# --- Build artefacts we never need in the repository ---
*.apk
*.aab
*.app

# NOTE: *.unitypackage is deliberately NOT ignored here,
# because .gitattributes tracks it with Git LFS instead.
```

The .gitattributes file — create it in the project root with exactly this content:

```text
# ===== LIGHT SHADOW — .gitattributes =====
# Place this file in the project ROOT before the first commit.
# It sets line-ending handling and routes big binaries through Git LFS.

# --- Normalise line endings for text files ---
* text=auto

# --- Unity YAML files: treat as text so diffs stay readable ---
*.meta text
*.unity text
*.asset text
*.mat text
*.prefab text
*.controller text
*.anim text
*.mask text
*.physicsMaterial text

# --- Git LFS: large binary assets (the team's agreed tracking list) ---
*.png filter=lfs diff=lfs merge=lfs -text
*.jpg filter=lfs diff=lfs merge=lfs -text
*.psd filter=lfs diff=lfs merge=lfs -text
*.tga filter=lfs diff=lfs merge=lfs -text
*.fbx filter=lfs diff=lfs merge=lfs -text
*.obj filter=lfs diff=lfs merge=lfs -text
*.wav filter=lfs diff=lfs merge=lfs -text
*.mp3 filter=lfs diff=lfs merge=lfs -text
*.ogg filter=lfs diff=lfs merge=lfs -text
*.mp4 filter=lfs diff=lfs merge=lfs -text
*.dll filter=lfs diff=lfs merge=lfs -text
*.exe filter=lfs diff=lfs merge=lfs -text
*.unitypackage filter=lfs diff=lfs merge=lfs -text
```

The Unity YAML entries in the middle of the second file are not part of the LFS list, but they make Git treat scenes and prefabs as text, which keeps history readable. The LFS block covers exactly the team's agreed tracking list: png, jpg, psd, tga, fbx, obj, wav, mp3, ogg, mp4, dll, exe, and unitypackage files.

### 7.6 Step 5 — First commit and push, walked through

The Team Lead performs the first push; the commands are annotated so every member understands what each one does, because everyone will run a variant of this sequence daily. Git Bash is opened **inside the project folder** (right-click in the folder → "Git Bash Here", or use the cd command shown):

```bash
# 1. Go to the project folder (adjust the path to your machine)
cd /d/Projects/LightShadow

# 2. Turn this folder into a Git repository
git init

# 3. Activate Git LFS for this repository (uses the .gitattributes you placed)
git lfs install

# 4. Stage everything - .gitignore automatically keeps Library/, Temp/, Logs/ out
git add .

# 5. SANITY CHECK before committing: Library/, Temp/, Logs/ must NOT appear in the list
git status

# 6. Make the first commit
git commit -m "Initial commit: Unity 6 project skeleton, folder structure, Git setup"

# 7. Connect the empty private GitHub repository created in 7.4
git branch -M main
git remote add origin https://github.com/【your-github-user-or-team】/LightShadow.git

# 8. Push the project to GitHub
git push -u origin main
```

If the Library folder appears at step 5, stop: the .gitignore is in the wrong place or was not saved, and committing now would bloat the repository for the whole semester. When pushing for the first time, GitHub will not accept a password in the terminal — a browser window (Git Credential Manager) opens instead; sign in once and Windows remembers. The other members then clone the finished repository rather than repeating the setup:

```bash
git clone https://github.com/【your-github-user-or-team】/LightShadow.git
# Then in Unity Hub: Add -> select the cloned LightShadow folder -> open with the team's Unity 6 LTS version
```

### 7.7 The daily team workflow

From Week 2 onward, every member follows the same loop for every task, and the loop is short enough to memorise: **pull → feature branch → work and commit → push → pull request → review → merge**. In full:

1. Close Unity. Unity locks project files while open, and pulling while it runs can corrupt imports (rule 7.8).
2. Pull the latest main: `git switch main` then `git pull origin main`.
3. Create a feature branch for the task: `git switch -c feature/player-crouch` (name pattern: feature/short-name).
4. Work in Unity, committing small and often with readable messages: `git add .` then `git commit -m "player: fix crouch camera height"`. The message convention is a short prefix for the area (player, enemy, ui, level, questions, art, docs) followed by a plain-English summary.
5. Push the branch: `git push -u origin feature/player-crouch`.
6. On GitHub, open a Pull Request from the branch into main, with one sentence describing what to test.
7. A different member reviews: they read the changed files on GitHub, then pull the branch locally and actually play the change in Unity.
8. If the reviewer approves, the PR is merged into main (regular merge, not rebase) and the branch is deleted.
9. Pull main again and reopen Unity. The next task begins.

Commits happen during the week; merges happen at the weekly Merge & demo session (5.2), where the whole team can watch the game come together and catch breakage in the same room where it can be fixed immediately.

### 7.8 Golden rules

The rules below are the ones that, if broken, cost the team days. They are few, and they are absolute.

| Golden rule | Why it matters |
| --- | --- |
| Never let two people edit the same .unity scene at the same time | Scenes are single long text files; two simultaneous editors guarantee a conflict that Git usually cannot resolve automatically |
| Close Unity BEFORE pulling | Unity holds locks on project files and rewrites Library data; pulling while it is open can corrupt imports and create phantom changes |
| Always commit the .meta files next to your assets | A .meta file stores the asset's GUID, and every reference in scenes and prefabs points to that GUID; a lost .meta breaks every link to the asset |
| Never commit Library/, Temp/, or Logs/ | They are machine-generated, enormous, and regenerated on every open; .gitignore already excludes them, so never force-add them |
| Keep main always runnable | Anyone must be able to pull main and press Play — especially the instructor and the weekly merge session |
| Commit small and commit often | Small commits are easy to review, easy to revert, and rarely conflict |
| One branch per feature | Feature branches keep main clean and reviews focused on one change |
| Never force-push to main | Force-pushing rewrites shared history and silently deletes teammates' work |

If a scene conflict ever does slip through despite the rules, do not hand-edit the .unity text. Unity ships a merge tool called **UnityYAMLMerge** (found under the editor install at Editor\Data\Tools\UnityYAMLMerge) that understands Unity's file format; the scene owner uses it at the merge session, and if the result is still broken, rebuilds the affected part from prefabs. In practice, teams that follow rule one almost never need it.

---
## 8. AI-Assisted Development Plan (Our Workflow, Not a Game Feature)

This section describes how the team uses ChatGPT, Claude, and GitHub Copilot to work faster. It is about the **development method only**. The game itself contains no artificial-intelligence features, and the Shadow remains simple scripted enemy behaviour; keeping that boundary crisp is part of the team's academic-honesty obligation, because the final report must be able to state plainly where tools were used and what the game actually does.

### 8.1 What the team uses AI tools for

**First-draft C# scripts.** The team asks an AI assistant for a first draft of a well-specified script (Section 8.4 contains six ready prompts), then reads it, trims it to the project's scope, adds comments, and tests it in Unity. The draft saves the beginners from staring at a blank file; the humans remain the authors of what ships, because every line is edited, understood, and committed by a named member.

**Debugging console errors.** When Unity prints a red error, the member copies the exact error text and the full script into the assistant with the debugging template in 8.4, and gets a beginner-level explanation of the cause plus a corrected version. This is the single highest-value AI use for a beginner team, because compiler and null-reference errors are exactly where new developers lose whole evenings.

**Drafting the question bank — for human fact-check.** The assistants are useful for generating first-draft general-knowledge questions across the six categories, but AI models write confidently wrong facts. Every question therefore passes through the human fact-check defined in 8.4: a named member verifies the answer against a real reference (textbook, encyclopaedia, official syllabus) and records the source in the question's factSource field. No unverified AI-generated fact ever enters a QuestionData asset.

**Explaining Unity concepts.** When a member meets an unfamiliar panel, error, or term (NavMesh baking, prefabs, lighting bakes), the assistants provide plain-English explanations faster than forum archaeology. The explanations are cross-checked against the official Unity Manual, which is free and authoritative.

**Documentation and wording.** The assistants help draft commit messages, pull-request descriptions, playtest survey forms, and sections of the weekly dev log. This is low-risk writing help that keeps the team's documentation habit alive even in busy weeks.

### 8.2 Tools table

The team deliberately keeps the toolset small and free, so that every member has the same environment and no one's workflow depends on a subscription. The table below is the complete approved list; anything outside it needs the Team Lead's agreement before use.

| Tool | The team uses it for | Access |
| --- | --- | --- |
| ChatGPT | Script first drafts, debugging help, Unity concept explanations | Free tier, browser |
| Claude | Longer code reviews of team scripts, refactoring suggestions, documentation drafts | Free tier, browser |
| GitHub Copilot | Autocompletion while writing scripts in Visual Studio or VS Code | Free for verified students through the GitHub Student Developer Pack (apply with a student ID; availability at the time of writing) |
| Unity Manual and Scripting API (docs.unity3d.com) | The source of truth checked against any AI answer about components and settings | Free; an offline copy ships with the editor |

### 8.3 Academic-honesty rules

Using AI assistants in a graded course is legitimate only when the course allows it and when the team stays transparent about it. The following rules are binding, and the instructor's policy (【Course Policy on AI Tools / Policy Document】) always overrides anything written here:

1. **Understand every line.** No script is merged until the member who committed it can explain it to another member in plain English — the "teach-back" test. If you cannot explain it, you may not commit it.
2. **Document every AI use.** Each assisted script or document gets an entry in the dev log using the template below. The log is submitted with the final deliverables, so the team's transparency is provable.
3. **No unverified facts in the game.** Every question in Data/Questions carries a factSource written by a human who checked the answer against a real reference. AI-drafted questions are drafts, never content.
4. **Test before merge.** AI-drafted code is tested in Play mode before its pull request is approved; "it compiled" is not a test.
5. **Follow course policy.** If the course forbids or limits AI assistance for any deliverable, that limit wins, and the dev log shows the team complied.
6. **Cite in the final report.** The final project report describes the AI-assisted workflow honestly, including what it was used for and how it was verified.

The dev-log entry template, kept at the repository root as AI_DEV_LOG.md:

```text
### 【Date】 — 【File or task】 — 【Member Name】
- Tool used: (ChatGPT / Claude / Copilot)
- What AI provided: (e.g. first draft of the battery drain loop)
- What I changed myself: (e.g. removed jumping, fixed crouch height, added comments)
- Teach-back check: (one sentence: what I explained to a teammate, and to whom)
- Test result: (e.g. played 10 minutes, no Console errors, battery drains as expected)
```

### 8.4 Ready-to-use example prompts

The prompts below are starting points the team pastes into the assistant and then improves with what the first answer teaches. Each one names the exact Unity version and pipeline, because assistants otherwise answer for the wrong Unity era, and each demands comments, because commented answers double as teaching material.

**Prompt 1 — player controller.** Used in Week 2 for the movement foundation:

```text
Act as a senior Unity tutor for absolute beginners. Engine: Unity 6000.0 LTS, 3D,
Universal Render Pipeline (URP), Windows and Android. Write ONE C# script called PlayerController.cs for a
first-person horror game with NO jumping, NO shooting, and NO combat. Requirements:
use a CharacterController; WASD movement relative to the camera; mouse look clamped
vertically (no Cinemachine; read keyboard and mouse through the classic Input class - the
project sets Active Input Handling to Both so a touch layer can be added later);
walk speed 3.0; sprint 5.0 while holding Left Shift; crouch 1.5 while holding Left Ctrl,
smoothly shrinking the controller height and lowering the camera; simple gravity and a
grounded check. Add // comments above every block explaining it in plain English. After
the code, list: (1) the exact Inspector setup steps (components and values), (2) three
common beginner mistakes with this script, (3) what to test in Play mode.
```

**Prompt 2 — flashlight battery and Bright Idea Meter.** Used in Weeks 2 and 4 for the two light gauges:

```text
Act as a senior Unity tutor for absolute beginners. Engine: Unity 6000.0 LTS, 3D, Universal
Render Pipeline (URP). Write TWO small C# scripts. (1) FlashlightBattery.cs: toggles a Spot Light
with F; drains battery while on; AddBattery(float amount) called by pickups; public events
OnBatteryChanged(float 0-1) and OnBatteryEmpty. (2) BrightIdeaMeter.cs: float 0-100 starting
at 60; drains slowly while the flashlight is on; AddCorrect() adds 25; AddWrong() subtracts
10; clamps to range; exposes a normalised value and a UnityEvent OnChanged so the HUD can
listen without code coupling; maps the meter to the spotlight's range (4 to 12 metres) and
intensity (1.5 to 3.5) with a simple linear interpolation. Keep every method under 20 lines,
add // comments in plain English, and finish with the exact Inspector wiring steps.
```

**Prompt 3 — NavMesh enemy behaviour.** Used in Week 3. Note it asks for scripted behaviour, not AI:

```text
Act as a senior Unity tutor for absolute beginners. Engine: Unity 6000.0 LTS, 3D, Universal
Render Pipeline (URP), using the AI Navigation package (NavMeshAgent + NavMeshSurface). Write
EnemyBehaviour.cs: a simple finite state machine with an enum and a switch statement,
implementing scripted behaviour (this is NOT artificial intelligence, just states).
States: Patrol (walk a loop of waypoint Transforms at 1.5 m/s), Stalk (player within 12 m
AND player light level < 0.3: face the player, play a breathing audio cue, creep at 2.0 m/s),
Chase (line of sight raycast within 15 m: move to the player at 4.0 m/s), Repelled (flashlight
beam hits while meter > 30: recoil 8 m, 6 s cooldown, return to Stalk). PlayerLightLevel is a
public static float set by my flashlight script. Within 1.2 m of the player call
GameManager.CaughtPlayer(). Add // comments to every state, then explain how to bake the
NavMesh with a NavMeshSurface component step by step.
```

**Prompt 4 — ScriptableObject question system.** Used in Week 4, the educational core:

```text
Act as a senior Unity tutor for absolute beginners. Engine: Unity 6000.0 LTS, 3D, Universal
Render Pipeline (URP). Create FOUR small scripts for a quiz system. (1) QuestionData.cs: a
ScriptableObject with string question, string[4] options, int correctIndex, an enum
QuestionCategory { Science, Math, History, Geography, Language, Health }, string explanation,
string factSource, and [CreateAssetMenu(menuName = "Questions/New Question")]. (2)
QuestionDatabase.cs: a ScriptableObject holding List<QuestionData>; GetNextQuestion() returns
questions without repeating until the list is exhausted. (3) QuizBoard.cs: when the player
presses E nearby, shows a question with a 20-second timer; calls BrightIdeaMeter.AddCorrect()
or AddWrong(). (4) QuizUIController.cs: displays the question, four buttons, a timer bar,
and hides itself after answering. No generics beyond List, no async, no events beyond
UnityEvent. Add // comments, then step-by-step instructions for creating the .asset files
from the right-click Create menu and wiring the UI in the Inspector.
```

**Prompt 5 — debugging help.** Used whenever a Console error resists the team; members fill the four bracketed fields:

```text
I am a Unity beginner. Engine: Unity 6000.0 LTS, 3D, Universal Render Pipeline (URP), Windows and Android.
I get this Console error:
【PASTE THE EXACT ERROR MESSAGE, including file name and line number】
My full script:
【PASTE THE ENTIRE SCRIPT】
What I expected to happen: 【ONE SENTENCE】
What actually happened: 【ONE SENTENCE】

Explain the cause in one short paragraph a beginner can understand. Then give me the
corrected full script with // comments. Keep my public names and design unchanged unless
something is genuinely impossible. Finish with one sentence on how to avoid this mistake
in the future.
```

**Prompt 6 — Android touch input layer.** Added for the cross-platform update; used in Week 2 right after the controller works:

```text
Act as a senior Unity tutor for absolute beginners. Engine: Unity 6000.0 LTS, 3D, Universal
Render Pipeline (URP), Android 10+, IL2CPP. My PlayerController already exposes Move(Vector2),
Look(Vector2), SetSprint(bool), SetCrouch(bool), and my FlashlightBattery exposes Toggle().
Write ONE beginner script called TouchLookZone.cs: dragging anywhere on the right half of the
screen adds to the look rotation, ignoring touches that start on UI buttons, using only the
classic Input class (touch counts). Then give me, in plain English, the step-by-step Canvas
setup for the mobile HUD: an on-screen joystick on the left that calls Move, and five buttons
(sprint hold, crouch toggle, flashlight, interact, pause) wired with the EventSystem pointer
interfaces (IPointerDownHandler and friends), sized for thumbs and arranged to respect notches
and safe areas. No new Input System package, no Cinemachine. Add // comments to every block
and end with a short checklist of what to test on the phone afterwards.
```

---
## 9. Development Timeline

The ten-week timeline below is built backwards from the submission date and forwards from one principle: a thin, working game every single week beats a thick, broken game in Week 10. Each week has exactly one deliverable phrased as a testable "done when" sentence, and the weekly Merge & demo session (5.2) is where the team confirms it together. The phases follow the agreed arc — setup, greybox prototype, enemy behaviour v1, question system, vertical slice, full level, art and audio pass, alpha freeze, playtest and polish, final build and submission — and no phase adds a mechanic beyond the Section 2.4 scope contract. From Week 2 onward the build is checked on both platforms every week, so the Android version grows alongside the Windows version instead of becoming a Week 10 emergency.

| Week | Phase | Key tasks | Deliverable — done when… |
| --- | --- | --- | --- |
| 1 | Project setup | Install Unity 6 LTS, Git, and Git LFS on all laptops; create the private repo with .gitignore and .gitattributes; build the 6.6 folder tree; set up both build profiles and the Android module (6.8); nominate the team's Android test device; confirm roles and meeting times; start the question spreadsheet | Every member opens LightShadow, presses Play on an empty scene, and can pull and push to the repository |
| 2 | Greybox prototype | Greybox the entrance hall and one corridor; PlayerController with walk, sprint, and crouch; flashlight with battery drain and one pickup; one lit checkpoint room; touch input layer v1 (joystick, drag-look, action buttons) mapped to the same controller | A first-person character walks a greybox school with a working flashlight that drains and can be refilled, playable with both keyboard and touch |
| 3 | Enemy behaviour v1 | Bake a NavMesh in a test corridor; patrol waypoints; stalk and chase states; caught event with fade-to-black and checkpoint respawn | The Shadow patrols a corridor, chases on line of sight, and respawning at the checkpoint works |
| 4 | Question system | QuestionData ScriptableObject, quiz board, quiz UI with timer, BrightIdeaMeter wired to the light; author 30 human fact-checked questions | Answering a quiz board visibly charges the meter and strengthens the light, and wrong answers dim it |
| 5 | Vertical slice | Make the Library wing playable start-to-checkpoint: Fuse Box 1, Fragments 1–2, first free art and audio in this wing; run a playtest with five non-team students; verify the slice on the Android test device | A stranger plays the ten-minute slice without help, the slice runs on the Android test device, and the top five bugs are logged |
| 6 | Full level | Build all three wings; place Fuse Boxes 2–3 and Fragments 3–6; wire the gatehouse, the dawn chase, the ending, and the recap screen | A full 20–40 minute playthrough runs from the fence climb to the study recap |
| 7 | Art & audio pass | Bake lighting per wing; final free textures and materials; sound effects, ambience, and music mix; performance pass on the weakest laptop and the Android test device | The game looks and sounds finished, and the frame rate holds on the weakest team laptop and the mid-range Android test device |
| 8 | Alpha freeze | Feature freeze — bug fixes only; tune drain rates, enemy speeds, and question difficulty; grow the bank to 60+ questions, all fact-checked | An alpha build with zero known crash bugs and the complete question bank |
| 9 | Playtest & polish | Structured playtests with five or more non-team players and a survey; fix the top ten issues; polish the recap screen and controls settings | A beta build plus a one-page playtest report listing issues found and changes made |
| 10 | Final build & submission | Produce the Windows build and the Android APK; finalise README, dev log (including the AI log), five-minute demo video, and presentation; assemble the submission package | Every item in Section 10.3 is handed in on time |

Weeks 9 and 10 are deliberately kept lighter than they look, because they are also the buffer: any slippage from Weeks 2–8 is absorbed there before anything else is touched. If time runs short, the team cuts from art and audio first — a greybox game with a working Bright Idea Meter still fulfils the project's promise, while a beautiful game with a broken question system does not. The recap screen and the checkpoint system are never cut under any circumstance, because they carry both the educational purpose and the fail state of the game.

---
## 10. Testing, Risks & Deliverables

Testing in a five-person team works when it is continuous, small, and written down. The team runs three kinds of testing: informal testing at every weekly merge and demo session, structured internal checklist testing (below) in Weeks 5 and 8, and external playtesting with non-team students in Weeks 5 and 9 using a short survey form. Every bug, however small, is logged in a shared spreadsheet with six columns — ID, steps to reproduce, expected result, actual result, severity, and a screenshot — because a bug that is not written down is a bug the team will meet again in Week 10 with no time to spare.

### 10.1 Testing checklist

The checklist below is the Writer & QA member's master script for the structured test passes, and every row must pass before the alpha freeze of Week 8. It is phrased as test steps so any member can execute it without explanation.

| ID | Test | Steps | Expected result |
| --- | --- | --- | --- |
| T01 | Movement | Walk, sprint, and crouch through a corridor with a low gap | All keys respond; crouch passes under the gap; no wall clipping |
| T02 | Flashlight and battery | Toggle with F; leave on for five minutes; collect a battery pickup | Light toggles; battery empties after roughly five minutes; pickup restores 50% |
| T03 | Bright Idea Meter | Answer questions correctly and incorrectly | Correct adds 25 and flares the light; wrong subtracts 10 and dims it |
| T04 | Question timer | Let a question time out | Timeout counts as wrong; the correct answer and explanation are shown |
| T05 | Enemy patrol | Watch a full patrol loop | The Shadow reaches every waypoint without leaving the ground or getting stuck |
| T06 | Stalk and chase | Stand in darkness nearby, then break line of sight | Stalk triggers in darkness; chase starts on sight; it returns to patrol after losing the player |
| T07 | Lit zones | Enter a lit room while chased | The Shadow stops at the lit doorway and does not enter |
| T08 | Flashlight repel | Hit the Shadow with the beam above meter 30, then below 30 | It recoils above 30; below 30 it does not, forcing an answer or a retreat |
| T09 | Caught and respawn | Let the Shadow catch the player | Screen fades to black, respawn at last lit checkpoint, meter minus 15, no gore |
| T10 | Objectives HUD | Open Tab and check counts after each objective | Fuse and fragment counts update correctly and match the world state |
| T11 | Gate ending | Complete all objectives and finish the chase | The gate opens only when powered; the ending and study recap show every answered question |
| T12 | Performance and build | Build for Windows and Android; run on the weakest laptop, on a machine without Unity, and on the Android test device | Stable frame rate on the weakest laptop; the Android APK installs and runs; both builds run without Unity installed |
| T13 | Question bank integrity | Review Data/Questions in full | 60+ questions, no duplicates, every question has a fact-checked source recorded |
| T14 | Touch controls | On the Android test device: move, look, sprint, crouch, toggle the flashlight, interact, and answer a quiz | Every action works; buttons never overlap the HUD or quiz UI; a full quiz sequence completes on touch |
| T15 | Android device stability | Play a twenty-minute session on the test device | No crash, no overheating warning, and a stable frame rate in both lit and dark scenes |

### 10.2 Risk table

The risks below are the six realistic ways this project fails, agreed at the Week 1 planning meeting and reviewed in every weekly planning slot. Likelihood and impact are judged for this specific team and course; the mitigation column is the standing instruction, not a hope.

| Risk | Likelihood | Impact | Mitigation |
| --- | --- | --- | --- |
| Scope creep ("just one more feature") | High | High | The Section 2.4 cut list is a contract; the one-in-one-out rule applies; changes need the Team Lead's written approval in the dev log |
| Scene merge conflicts | Medium | High | One owner per scene (5.1); contributions through prefabs; close Unity before pulling; merges only at the weekly merge session |
| Git LFS quota exceeded | Medium | Medium | Prefer .ogg over .wav, resize textures to 1–2K, import only needed packs; check repository size monthly on GitHub; keep one pack per asset category |
| Member absence | Medium | Medium | Every task is assigned to a pair; the roles table lets anyone cover a missing member; weekly demo notes keep absentees current |
| AI-written code bugs (code that looks right but fails at the edges) | Medium | High | Teach-back rule (8.3); every merged script is tested in Play mode before approval; scripts stay small; every AI use is logged |
| Weak laptops and mid-range Android phones | High | Medium | Baked lighting instead of many real-time lights; URP mobile preset (render scale down, real-time shadows off on Android); ASTC textures; a weekly performance test on the weakest laptop and the test device from Week 5 onward |

### 10.3 Final submission deliverables

The project is handed in as a package, and assembling it takes less than a day if the pieces are maintained as the team goes rather than rebuilt at the end. The complete submission consists of:

1. A playable Windows build (zipped) and an Android APK, both runnable without Unity installed.
2. Access to the private GitHub repository for 【Instructor Name】, showing the full commit history and the pull-request workflow.
3. This project document, updated to match the final game.
4. The question bank export: every QuestionData asset printed as a study sheet PDF, mirroring the in-game recap.
5. The development log, including the AI-assistance log from Section 8.3.
6. A five-minute demo video showing the core loop: darkness, a question, the light strengthening, and the recap screen.
7. A ten-slide presentation for the showcase session on 【Showcase Date】.
8. The Week 9 playtest report with the changes it triggered.

---
## 11. Glossary

Unity and Git use a small vocabulary that beginners meet all at once in Week 1, so the table below translates every term this document relies on into plain English. New members should read it before the first setup session, and the team should add new terms to it as they appear rather than letting jargon accumulate unexplained.

| Term | Plain-English meaning |
| --- | --- |
| Scene | One "level" or screen of the game — a file that contains a layout of objects; the team has four (Bootstrap, MainMenu, School, Ending) |
| GameObject | Any single thing in a scene — a lamp, the player, a quiz board; by itself it is just an empty container |
| Component | A capability attached to a GameObject — a light, a collider, a script; a lamp is a GameObject plus a Light component plus a collider |
| Prefab | A saved, reusable GameObject template; you edit it once and every copy in every scene updates — the team's way to share work without touching others' scenes |
| ScriptableObject | A data container saved as a file; every quiz question is one, which is why non-programmers can author and edit questions |
| NavMesh | An invisible walkable mesh Unity computes over the level; the Shadow walks on it so it avoids walls naturally |
| Meta file | The small .meta file Unity creates next to every asset, storing its unique ID; it must always be committed or every reference to the asset breaks |
| Baked lighting | Lighting precomputed once by the editor instead of live every frame; it is cheap to run, which is how weak laptops keep a good frame rate |
| URP | Universal Render Pipeline — Unity's current rendering system, tuned for both desktop and mobile; the pipeline this project uses so one build serves Windows and Android |
| IL2CPP | Unity's converter that turns C# scripts into native code for Android builds; paired with the ARM64 architecture setting |
| Commit | A saved snapshot of changes with a message; the unit of "I did this" in the project history |
| Branch | A private line of work split off from main, used for one feature; merging brings it back |
| Merge conflict | Two changes that Git cannot reconcile automatically; for scenes the team avoids this with ownership rules rather than fixing it |
| LFS | Large File Storage — a Git add-on that stores big binaries (textures, audio, models) efficiently so the repository stays small |
| Pull request | A request to merge a branch into main, shown on GitHub so a teammate can review the diff and test it before it lands |
| .gitignore | A file listing folders and files Git must never track (Library, Temp, logs); placed in the project root before the first commit |
| Clone | Your local, complete copy of the GitHub repository, created once with git clone |
| Push and pull | Push uploads your commits to GitHub; pull downloads everyone else's commits to you — the daily handshake of the team |
| Unity Hub | The manager application that installs Unity versions and creates projects; the team installs the editor through it |

---

## 12. How to Use This Document — Week 1 Checklist

This document is written to be executed, not admired: Sections 6 and 7 are copy-paste installation guides meant to be done together in one room on Days 1–2, Section 9 is the standing agenda for every planning meeting, and the 【placeholder】 brackets are a to-do list of team decisions. The checklist below turns all of that into the first working week; when its last box is ticked, the Week 1 deliverable of Section 9 is met.

**Day 1**

- [ ] Every member installs Unity Hub and the exact agreed Unity 6 LTS version (Sections 6.2–6.3) and posts the About Unity screenshot in the team chat.
- [ ] Every member installs Git for Windows and Git LFS, verifies both with `git --version` and `git lfs version`, and sets user.name and user.email (Sections 7.2–7.3).
- [ ] Nominate one Android 10+ phone as the team's official test device, and confirm the Android Build Support module installed with the editor (Sections 6.3 and 6.8).

**Day 2**

- [ ] The Team Lead creates the empty private GitHub repository LightShadow (Section 7.4), copies the .gitignore and .gitattributes from Section 7.5 into the project root, and pushes the initial project with the Section 6.6 folder structure.
- [ ] Every other member clones the repository and opens the project once through Unity Hub with no Console errors (Section 7.6).

**Day 3**

- [ ] Fill in every 【placeholder】 in this document: member names in Section 5.1, meeting times in Section 5.2, and the course information on page 1.
- [ ] Create the shared question spreadsheet — columns: category, question, four options, correct answer, explanation, source, author, fact-checked by — and add three questions per member.

**Day 4–5**

- [ ] Shortlist the free assets: one Kenney pack, one Asset Store school or interior pack, and one ambience track from freesound.org; post the links in the team chat.
- [ ] The Team Lead drafts the Week 2 task list from Section 9 and assigns each task to a pair.

**End of week**

- [ ] Hold the first Merge & demo session: pull main, build once, play an empty scene together, and confirm the Week 1 deliverable is met.

Two habits carry the rest of the semester. First, keep the Section 2.4 cut list visible at every planning meeting, and when in doubt, cut the feature and keep the learning. Second, write everything down in the dev log — decisions, AI help, bugs, and playtest notes — because in Week 10 the log becomes the team's report, its defence of academic honesty, and its memory of a semester's worth of good work.

