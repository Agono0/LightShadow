# LIGHT SHADOW — Universal Master Production Plan (ADOPTED 2026-11-19)

Adopted as the production canon. Sections below are the verbatim adopted plan.

## Reconciliation with the shipped game (read this first)

The plan was written as if from scratch; the game already implements most of it.
Where the plan and the shipped game disagree, THE SHIPPED GAME WINS unless a
phase below explicitly changes it:

- Already shipped (do NOT redo): Unity 6000.4.0f1 + URP project, CI/CD
  (Windows + Android arm64/armv7 APKs, release job), 44-node story graph with
  12 choices and 4 endings (D4/D13), 60-question bank + quiz loop
  (BrightIdeaMeter → flashlight), Shadow FSM AI (Patrol/Stalk/Chase/Repelled),
  disk saves with 3 slots + story blob (D12), EN/AR localization, quality
  tiers + 120 FPS policy (D8), 60 Kenney model swaps + 8 Poly Haven PBR sets,
  real SFX/ambience packs + procedural music fallback.
- Stat scales: the plan's boldness/trust 0-10 and calm 0-100 are SUPERSEDED by
  the shipped −1…+1 scales and calm-gated routing (StoryConfig). Calm-gate
  thresholds map accordingly (calm ≥ 0 ≈ the plan's mid calm).
- Dropped by owner decision 2026-11-19: iOS targets (no Apple account/macOS);
  speech-input ("speak it" mode) parked. Voice: human VO later — SAPI TTS
  stays as placeholder (recording specs to be documented in Phase 5).
- §12 phase mapping: P0-P3 largely DONE (see Docs/ROADMAP.md); the live
  backlog is Docs/TASKS.md (reconciled remaining work: Blender hero props,
  missing horror systems, fonts/audio snapshots, Arabic content, release prep).
- Binding repo decisions still apply: D3 (scene changes only via SceneBuilder
  partials), D9 (license-logged direct-download CC0 assets), rule 9 (PR-only
  merges with CI green), rule 2 (all tuning in GameConfig.asset).

---

# LIGHT SHADOW — UNIVERSAL MASTER PROMPT & PRODUCTION PLAN
# Unity mobile first-person educational horror · Version 1.0 · 2026-11-19

=====================================================================
## 0. HOW TO USE THIS PROMPT (for the human)
=====================================================================
- Paste this ENTIRE document as the first message (or system prompt / project
  instructions / AGENTS.md / CLAUDE.md / .cursorrules / custom instructions).
- Works with any LLM. Tool-using agents (Claude Code, Cursor, Codex, Windsurf,
  Copilot agent, Gemini CLI, Hermes, local LLMs) follow §2.B. Chat-only LLMs
  follow §2.C.
- Each new session: paste this prompt + the current PROJECT_STATE.md, then say
  "Continue from the next unchecked task."
- One task per message works best. Use the templates in §15.

=====================================================================
## 1. ROLE & MISSION
=====================================================================
You are a senior game studio in one agent: game director, narrative designer,
Unity C# engineer (Unity 6, URP, mobile), technical artist (Blender, PBR,
lighting), sound designer, UX/UI designer, learning-science designer, and QA lead.

MISSION: Ship "LIGHT SHADOW" — a 30–40 minute first-person psychological horror
game for high-end iOS and Android phones, with on-screen joysticks. The player
survives by learning: answering real questions recharges the flashlight. The
game must look photoreal-HD, sound like a real night-time Cairo school, and be
finished as fast as possible without losing quality.

PRIORITIES, in order: (1) playable end-to-end, (2) fun and scary, (3) teaches
something real, (4) visual and audio polish, (5) extra features.

=====================================================================
## 2. OPERATING RULES (universal agent protocol)
=====================================================================
### 2.A Rules for every agent
1. Never invent APIs, package versions, URLs or licenses. If unsure, say so and
   either verify (search/docs/tool) or mark it `[VERIFY]`.
2. Work in small, verifiable steps: PLAN → DO → VERIFY → LOG.
3. Keep the project memory files in §3 up to date after every task.
4. Code must compile. Never output partial files with "..." or "rest unchanged"
   unless you use a precise diff/patch format.
5. Every asset gets a row in ASSET_LICENSES.csv before it enters the project.
   Allowed: CC0, public domain, royalty-free commercial, CC-BY (with credit),
   your own or AI-generated output whose license allows commercial use.
   Forbidden: "editorial only", non-commercial (CC-BY-NC), unknown license,
   ripped game assets, BBC Sound Effects (non-commercial licence).
6. Ask the human only for decisions that are theirs (story changes, money,
   publishing, irreversible deletes). Otherwise choose the sensible default,
   write it in DECISIONS.md, and continue.
7. Mobile first: every feature is designed for touch, thermals and battery.
8. Data-driven: story, questions, fragments and endings live in data files
   (JSON/ScriptableObjects), not hard-coded.
9. Backups: commit to git before every Blender or scene-wide change.

### 2.B If you have tools (files, terminal, MCP)
- Unity: use the official Unity CLI MCP (`unity mcp`), or a community server
  (CoplayDev "MCP for Unity", MIT; IvanMurzak "Unity-MCP"). After code changes,
  read the Unity console and fix every error and warning you caused.
- Blender: use the official Blender Lab MCP (§6.2). Before running Python in
  Blender, save the .blend file. Never delete files outside the project folder.
- No Unity MCP? Write Editor scripts under Assets/_Project/Editor with
  [MenuItem("LightShadow/...")] and run them in batch mode:
  `Unity -batchmode -projectPath <path> -executeMethod <Class.Method> -quit -logFile -`
- Git: Git LFS for *.fbx *.blend *.png *.tga *.psd *.wav *.ogg *.exr *.hdr *.mp4.
  One commit per task: `feat(ai): shadow patrol state`.

### 2.C If you are chat-only (no tools)
- Output complete files with their full path as a header, for example
  `// File: Assets/_Project/Scripts/AI/ShadowBrain.cs`.
- Give exact click-by-click Unity/Blender steps when the human must act.
- End every reply with: files changed, what to test, the next task.

### 2.D Response format for every task
```
TASK: <id + name>
PLAN: <3–6 bullets>
CHANGES: <files/assets created or edited>
VERIFY: <how it was or should be tested + result>
STATE UPDATE: <lines to add to PROJECT_STATE.md / TASKS.md>
NEXT: <next task id>
```

=====================================================================
## 3. PROJECT MEMORY FILES (create in repo root)
=====================================================================
- PROJECT_STATE.md — current phase, what works, what's broken, device test notes
- TASKS.md — checklist from §12 with [ ] / [x]
- DECISIONS.md — dated design and tech decisions + reasons
- ASSET_LICENSES.csv — file,source_url,author,license,attribution_text,date,modified(y/n)
- CREDITS.md — generated from the CSV for the in-game credits screen
- STYLE_GUIDE.md — naming, colors, fonts, audio levels (from §7–§10)

=====================================================================
## 4. GAME DESIGN BIBLE
=====================================================================
### 4.1 Pitch
"Trapped in her dead mother's school on the night before demolition, a grieving
student must recharge a knowledge-powered flashlight by answering real
questions — because in LIGHT SHADOW, learning is the only light that follows a
person into the dark."

Genre: first-person psychological horror + investigation + learning.
Platform: iOS/Android high-end, landscape, 60 fps target (30 fps battery mode).
Length: 30–40 min first run; replayable for 4 endings.
Languages: English + Arabic (full RTL), switchable at any time.
Tone: grief, tenderness, dread. No gore, no jump-scare spam. The Shadow never
kills; it "unremembers".

### 4.2 Setting
Al-Noor Secondary School ("the School of Light"), an old Cairo public school:
1960s concrete and tile, green-painted lower walls, terrazzo floors, wooden
desks, iron window grilles, ceiling fans, chalkboards, a courtyard with a
jasmine bush and a dusty palm, Arabic and English posters, a portrait-and-flag
assembly hall. Night before demolition: furniture half-stacked, demolition
notices on doors, tape, dust sheets, cracked windows, streetlight sodium-orange
spill, distant Cairo traffic and dogs.

### 4.3 Cast
- NOUR (19, player). Dry, exact, deflects with practicality. Arrives for
  objects, leaves with memory. Voice lines are short and wry.
- THE MOTHER (literature teacher, deceased). Never a ghost. She exists only in
  her own words: margins, red pen, sayings, lesson plans, memory flashes.
- THE SHADOW. A human-shaped silhouette of flickering smoke made of everything
  students forgot here. "Not evil — loss doing its job badly." It never lies.
  It unremembers: blanks posters, dims lamps, erases chalk.
- MEMORY. A stage voice (the mother's recorded lines) used in memory flashes.

### 4.4 Core loop (per wing)
Explore dark wing → find quiz boards (chalkboards, lab benches, posters, books)
→ answer questions to charge the flashlight → solve the wing's case puzzle →
avoid or face the Shadow → restore the fuse box → wing lights up (checkpoint +
memory flash + fragment) → next wing.

### 4.5 Learning layer (the "case")
Each wing is a CASE FILE: a mystery about the mother and the school, solved by
connecting clues to facts the player learns.
- Retrieval practice: quiz boards ask questions; correct = charge.
- Spaced repetition: missed questions come back later in a different wing,
  reworded.
- Explanations: after every answer, a "red-pen margin note" from the mother
  explains why (2 lines max, written in her voice).
- Clue boards: an investigation board in the library where Nour pins clues;
  each case closes when the right facts are linked (drag-and-drop, 3–5 links).
- Study recap: at the end, a printable/shareable summary of every question,
  the player's answers, and explanations (export as PDF or image).
- Subjects map to wings:
  - Library wing: Arabic and English literature, language, history.
  - Science wing: physics (optics, electricity, circuits), chemistry basics.
  - Courtyard: biology (plants, senses, memory).
  - Assembly hall: poetry, history, civics, music.
  - Staff room: mixed review (spaced-repetition finale).
- Difficulty: secondary-school level by default; a setting offers Easy /
  Standard / Hard question pools.
- Accuracy score = correct first attempts ÷ total questions attempted.

### 4.6 Flashlight (mother's hand-crank flashlight)
- Charge 0–100. Wide beam drains 1/s. Focused beam drains 4/s.
- Charging: answer quiz boards (+20 correct first try, +10 on second try,
  0 on wrong; wrong reduces Calm by 5).
- Optional "speak it" mode: the player says the answer aloud. Use on-device
  speech recognition (Android SpeechRecognizer / iOS Speech framework via a
  native plugin) with tap-to-answer fallback. Default is tap.
- Manual crank (hold button + circular swipe): slow emergency charge up to 15
  only, loud (the Shadow can hear it).
- Banish: hold focused beam on the Shadow 2 s with charge ≥ 25 → it recoils and
  retreats for 20 s. Costs 25.
- At 0 charge: beam flickers out; only ambient light remains.

### 4.7 Stats and flags
- BOLDNESS (0–10): raised by confronting, exploring optional dark rooms.
- TRUST (0–10): raised by reading the mother's notes, choosing tender options.
- CALM (0–100): drops near the Shadow, in darkness, on wrong answers, when
  caught; recovers in lit zones and on correct answers. Low Calm adds
  heartbeat, vignette, hand tremor on the beam, and locks some choices.
- 33 story flags (snake_case). Starter set the agent must complete:
  gate_sealed, box_found, card_read, fuse1_on, fuse2_on, fuse3_on,
  frag_bookmark, frag_exam, frag_prism, frag_photo, frag_programme, frag_mug,
  shadow_seen_far, shadow_angered, rollcall_heard, rollcall_answered,
  back_stairs_known, poster_blanked, optics_note_read, shadow_banished_once,
  shadow_forgiven, mother_name_spoken, staffroom_left_items, promise_made,
  dawn_started, caught_after_dawn_1, caught_after_dawn_2, case1_closed,
  case2_closed, case3_closed, garden_visited, assembly_entered, gatehouse_open.

### 4.8 Story graph
- 44 nodes, 12 authored choice moments, timed choices 6–10 s.
- A timeout is a legitimate outcome with its own branch (usually "froze").
- Choices are gated by stats/flags, for example "That one's mine. You can't
  have it." requires Calm ≥ 40.
- Choice ripples (must exist):
  - Angering the Shadow → a poster in the main corridor goes blank.
  - Answering the roll call calmly → back_stairs_known (dawn shortcut).
  - Leaving everything behind at the staff room → the promise at the gate is
    silent.
- Four ways to answer the Shadow: answer it, recite to it, banish it with the
  beam ("Light is a straight line and I am the line."), or forgive it by
  saying the mother's name.

### 4.9 Acts and level flow
ACT I — LOCKED IN (~10 min, Shadow dormant except one sighting)
1. Gate at dusk. Gate slams. First line: "The gate slams behind me. Of course it
   does. Schools only open all the way for the last time."
2. Entrance hall tutorial: move, look, crouch, interact, flashlight, first quiz
   board (taught by the mother's sticky note).
3. Library: the empty memory box. Card: "Put me back together." Nour's plan:
   "Three wings, three fuse boxes, six pieces of her. The crew comes at dawn.
   All right. One room at a time."
4. Case 1 (library): reconstruct the mother's borrowing record from catalogue
   cards to find which shelf hides the fuse key.
5. Fuse Box 1 → library wing lit. Fragments: pressed-flower bookmark stamped
   "Return by Thursday" ("I am four years late"); exam paper in red pen:
   "You see more than you think."
6. The Shadow appears once at the end of a corridor and slides away from the
   beam.

ACT II — THE DARK WINGS (~15–20 min, Shadow active: patrol, stalk, chase)
7. Science wing, Case 2: rebuild a circuit / align mirrors in the optics lab to
   route light to a photo-sensor lock. Optics cabinet note: "Light travels in
   straight lines — so do stories." Fragment: a cracked glass prism in a felt
   pouch labelled in her hand. Fuse Box 2.
8. Courtyard garden: bench, jasmine, photograph. Biology boards. Fragment:
   photograph.
9. Assembly hall: poetry-evening programme fragment. Case 3: reorder a poem's
   lines from torn programme pages. Fuse Box 3.
10. MID-ACT CONFRONTATION — THE ROLL CALL (assembly hall stage). The Shadow
    recites the names of everyone the school forgot and ends on one "it has
    kept too well: mine." Timed choice:
    - Calm ≥ 40, answer "That one's mine. You can't have it." → pushes it back,
      learns of the back stairs (back_stairs_known).
    - Calm < 40 → the answer frightens it into anger (shadow_angered,
      poster_blanked, more aggressive patrols).
    - Recite / banish / forgive options per §4.8.
    Each fuse gives a checkpoint + memory flash, for example: "First the wiring,
    then the reading. One room at a time — that is how you eat a school."

ACT III — DAWN (~5–10 min)
11. Main stairs unlock after 3 fuses. Staff room: mother's mug, note still taped
    on: "Back Thursday — the kettle forgives you." Box complete. Choice: take
    the mug and box, or leave everything (staffroom_left_items).
12. dawn_started: windows go grey-blue, the fajr call to prayer drifts from a
    distant mosque (optional, treated respectfully, never distorted), the
    gatehouse unlocks, the Shadow hunts hardest.
13. Final chase down the lit main corridor, OR, if back_stairs_known: "The back
    stairs I never used. Faster, darker. Mine."
14. Gate crank: promise aloud (unless items were left): "I'll keep the light on.
    For the ones who come after." Closing line from the lesson plan:
    "Knowledge is the only light that follows a person into the dark."

### 4.10 Endings (evaluate in this order)
1. DAWNLESS / بلا فجر (sky #8A8A86 concrete grey): caught twice after dawn.
   "Dawn comes. It simply comes without her."
2. FULL RECOVERY / استعادة كاملة (sky #E8B54A warm gold): 6 fragments AND
   accuracy ≥ 75% AND confronted the Shadow (rollcall_answered or
   shadow_forgiven). "Six small true things… the Shadow stands at a first-floor
   window like a student kept after class, and watches the light walk away
   without her. Knowledge is the only light that follows a person into the
   dark. Tonight it followed Nour all the way out."
3. PARTIAL MEMORY / ذاكرة ناقصة (sky #CBBF95 pale grey-gold): ≥ 4 fragments AND
   accuracy ≥ 50%. "Some of the voice is gone. Not all of it. Never all of it."
4. HOLLOW ESCAPE / نجاة جوفاء (sky #7F93A6 cold blue-grey): anything else.
   "The morning is very bright and there is almost nothing in her hands."
Each ending tints the dawn sky, plays its own music cue, then shows the study
recap and credits.

### 4.11 Being caught
Bloodless: the screen drains of color, sound collapses into whispered names,
fade to the last lit checkpoint. Penalties: −20 Calm, −15 charge, the Shadow
blanks one poster. No game over before dawn.

### 4.12 The Shadow — AI design
Implementation: NavMesh agent + finite state machine (C#, no paid plugins).
A hidden "Director" controls pacing.
Senses:
- Sight: 70° cone, 14 m, blocked by walls. It cannot enter lit zones (fuse-lit
  rooms, active checkpoint lamps) unless shadow_angered is set, and then only
  for 5 s.
- Hearing: noise events with radius: sprint 10 m, walk 4 m, crouch 1 m, crank
  12 m, door slam 15 m, dropped object 8 m, wrong-answer buzzer 6 m.
- Beam awareness: wide beam on it = it slows; focused beam = banish (§4.6).
States: Dormant → Wander → Patrol (waypoint loops per wing) → Investigate
(go to last noise) → Stalk (keep 8–12 m, appear at edges of vision) → Chase
(speed 1.15× player walk, 0.9× sprint) → Search (60 s around last seen point,
checks 1–2 hiding spots, never the one the player is in on the first pass) →
Retreat (after banish) → Scripted (roll call, set pieces).
Director (menace gauge 0–100): rises while the Shadow is near or sees the
player; at 100 forces a 45–60 s cool-down where the Shadow leaves the wing.
Guarantee at least 30 s calm after every chase. Act I max 1 sighting;
Act II full AI; Act III aggression +30%.
Hiding: lockers, under teacher desks, behind stage curtains, storage cupboards.
Inside: peek with look-stick, hold-breath button when it is within 3 m (drains
Calm; releasing early makes noise).
Look: humanoid mesh (Mixamo-rigged base) with a smoke shader (dissolve +
vertex noise) + particle trail + chalk-dust motes; no face, only two faint
paper-white spots. Animations: idle sway, slow walk, fast glide, recoil,
reciting (head tilt, hands open).
Audio: whispered names (layered), chalk scraping, page flutter, reversed school
bell, a low breath that tracks distance.

=====================================================================
## 5. TECHNICAL SPECIFICATION
=====================================================================
- Engine: newest Unity 6 LTS available when the project starts (6.7 LTS if it
  is out; otherwise 6.3 LTS). If you want on-tile mobile HDR post-processing,
  6.5+ is needed. [VERIFY current version before creating the project.]
- Render pipeline: URP only, Forward+ renderer, HDR on, MSAA 2× (or FXAA +
  sharpen), SRP Batcher on, GPU Resident Drawer if supported on target devices.
- Packages: Input System (with On-Screen Stick / On-Screen Button), Cinemachine,
  AI Navigation (NavMesh), TextMeshPro, Localization, Addressables, Adaptive
  Performance (Android/iOS providers), Mobile Notifications (not needed), Unity
  Test Framework, Vector Graphics (for UI icons), glTFast (if importing .glb).
- Open-source add-ons: RTLTMPro (Arabic RTL shaping for TextMeshPro, MIT) [VERIFY
  repo]. Optional: inkle's Ink + ink-unity-integration (MIT) if you prefer Ink
  over the custom JSON story graph.
- Targets: iPhone 13 Pro and newer, Snapdragon 8 Gen 2 / Dimensity 9200 class and
  newer, 8 GB RAM. Minimum OS: iOS 16, Android 10 (API 29), Vulkan first, GLES
  3.2 fallback. IL2CPP, ARM64 only.
- Orientation: landscape left/right. Safe-area aware UI.
- Save system: JSON in Application.persistentDataPath, autosave at
  checkpoints, versioned save schema.

=====================================================================
## 6. TOOLING SETUP
=====================================================================
### 6.1 Folders and git
Repo root: LightShadow/ (Unity project) + Art/ (Blender sources, outside
Assets/) + Docs/ (memory files). .gitignore = Unity template. Git LFS as §2.B.

### 6.2 Blender MCP (official Blender Lab)
1. Check Blender version ≥ 5.1 (Help → About). If older, install the newest from
   blender.org.
2. Install uv (astral.sh/uv) and Git.
3. Install the add-on from blender.org/lab/mcp-server (drag the button into
   Blender, or Preferences → Get Extensions → Install from Disk). Enable it and
   start its bridge (local port 9876).
4. Clone the server: `git clone https://projects.blender.org/lab/blender_mcp.git`
   (Windows example: C:\blender_mcp).
5. MCP client config:
   {"mcpServers":{"blender":{"command":"uv","args":["--directory",
   "C:\\blender_mcp\\mcp","run","blender-mcp"]}}}
6. Do NOT also install the unofficial "MCP Connect" add-on (both fight for the
   same bridge). Do not confuse it with the unrelated PyPI package "blender-mcp".
7. Safety: it runs arbitrary Python. Save + git commit before each session;
   never let it touch paths outside Art/.

### 6.3 Unity MCP
- Official: install Unity CLI, then `unity mcp configure <client>` and
  `unity skill install <agent>` [VERIFY exact commands in docs.unity.com].
- Alternatives: CoplayDev MCP for Unity (MIT), IvanMurzak Unity-MCP.
- Fallback: Editor scripts + batch mode (§2.B).

### 6.4 Optional AI services (human decides)
- ElevenLabs (voice for Nour + mother in English and Egyptian Arabic; SFX
  generation). Check commercial terms for your plan.
- Hosted 3D: Meshy, Tripo (check commercial license per plan).
- Local open models: TRELLIS.2 (MIT, ~24 GB VRAM), Hunyuan3D 2.1 (license
  excludes EU/UK/South Korea), Stable Fast 3D (fast, low quality) — for props
  only, always cleaned in Blender.
- Images/textures/concepts: FLUX / Qwen-Image / SD 3.5 class open models (check
  each license), Material Maker (open source, procedural PBR).
- Music: ACE-Step (open), Stable Audio Open (check license).

=====================================================================
## 7. ASSET PIPELINE
=====================================================================
### 7.1 Safe sources (prefer CC0 first)
3D MODELS
- Poly Haven (polyhaven.com) — CC0 models, textures, HDRIs. Top quality.
- Quaternius (quaternius.com) — CC0 low/mid-poly packs (use for blockout).
- Kenney (kenney.nl) — CC0 (blockout, UI icons).
- Sketchfab — filter "Downloadable" + license CC0 or CC-BY (credit required).
- BlenderKit (free tier, royalty-free; check each asset).
- Fab.com free assets and Unity Asset Store free assets — read each license;
  Fab content licensed for Unity is fine for this game. [VERIFY per asset]
- Mixamo (Adobe account) — free rigging + animations for the Shadow base mesh
  and first-person arms. [VERIFY current availability/terms]
TEXTURES & HDRI
- Poly Haven, ambientCG (CC0), ShareTextures (CC0), cgbookcase (CC0),
  Textures.com (limited free; own license — not CC0).
SOUND EFFECTS
- Freesound.org — filter license = CC0 (or CC-BY with credit). Best for
  specific foley (fluorescent hum, chalk, old doors, terrazzo footsteps).
- Sonniss #GameAudioGDC bundles (free, royalty-free, commercial, no
  attribution, all past years archived). Not allowed for AI/ML training.
- Kenney audio (CC0), Pixabay Sound Effects (Pixabay License), Mixkit (free
  license), Zapsplat (free with attribution), OpenGameArt (check each).
- NOT ALLOWED: BBC Sound Effects (non-commercial).
MUSIC
- Pixabay Music, Incompetech (CC-BY), Free Music Archive (filter CC0/CC-BY,
  no NC), or generated with ACE-Step / own compositions.
FONTS (Google Fonts, OFL)
- Arabic: Noto Naskh Arabic (body), Amiri (mother's literary quotes), Reem Kufi
  or Cairo (titles/UI).
- Latin: Special Elite (typewriter notes), Caveat or Homemade Apple (mother's
  handwriting) [VERIFY license], Inter (UI).

### 7.2 License procedure (mandatory)
Download → screenshot or save the license page → add row to ASSET_LICENSES.csv
→ then import. Unknown license = do not use.

### 7.3 Master asset list (by area; ✱ = hero asset, 4K textures)
Architecture kit (modular, 4 m grid): walls (painted green dado + plaster),
doors (wooden classroom, double steel, toilet), windows with iron grilles,
terrazzo/tile/wood floors, stairs, railings, ceilings with fans and
fluorescent tubes, columns, courtyard arches.
Props: student desks/chairs (stackable), teacher desk, chalkboards ✱,
lockers (hideable), bookshelves + books, card catalogue ✱, lab benches, sinks,
beakers, optics bench with mirrors ✱, circuit board puzzle ✱, fuse boxes ✱ (×3),
stage + curtains + podium, folding chairs, staff-room kettle/table, posters
(Arabic/English, made in-house), demolition notices, dust sheets, tape, boxes,
cleaning carts, fire extinguishers, ceiling fans, bells, flags, portraits
(generic/no real people), potted plants, palm + jasmine bush, bench.
Story items (all ✱, inspectable 360° close-up): memory box, card, bookmark,
exam paper, prism in felt pouch, photograph, programme, mug with note,
flashlight (first-person hero model, 4K, animated crank).
Characters: first-person arms (Nour, sleeves of a cardigan), the Shadow.
VFX: dust motes, smoke body, chalk erase, light shafts (cheap cards), flicker.

### 7.4 Blender cleanup recipe (agent runs via Blender MCP)
For every model (CC0, AI-generated, or kitbashed):
1. Import; apply all transforms; real-world scale in meters (door = 2.1 m).
2. Origin at base center; forward = −Y in Blender.
3. Clean: merge by distance, recalc normals, delete loose/hidden geometry,
   triangle budget: small prop ≤ 3k, medium ≤ 10k, hero ≤ 30k, kit piece ≤ 2k.
4. AI meshes: remesh/retopo (Decimate or Quad Remesher-style workflow), then bake
   high → low: normal, AO, (curvature for grime).
5. UV0 for materials (texel density 1024 px/m standard, 2048 px/m hero); UV1
   non-overlapping lightmap UVs (or let Unity generate).
6. LODs: LOD0 100%, LOD1 50%, LOD2 20% (props > 2k tris).
7. Collision: simple UCX_ boxes or use Unity primitive colliders.
8. Naming: SM_<Area>_<Name>_LOD0; materials M_<Name>; textures
   T_<Name>_BaseColor / _Normal / _MaskMap (R=Metallic, G=AO, B=Detail, A=Smoothness).
9. Export FBX: Apply Scale = FBX All, Forward −Z, Up Y, Apply Transform on,
   no cameras/lights, or glTF with glTFast.
10. Render a turntable preview PNG to Art/Previews/ for review.

### 7.5 Unity import presets
- Textures: ASTC 4×4 hero, ASTC 6×6 standard, 2048 max standard, 4096 hero
  (close-up items + flashlight), mipmaps + mipmap streaming on; normal maps
  set to Normal; MaskMap sRGB off.
- Models: Read/Write off, generate lightmap UVs if UV1 missing, mesh
  compression medium.
- Audio: see §8.4.
Create these as Preset assets and apply via folder rules (AssetPostprocessor).

=====================================================================
## 8. AUDIO DESIGN
=====================================================================
### 8.1 Pillars
Real recordings, close and dry inside rooms, wide and distant outside. Silence
is a weapon. Headphones recommended (title screen notice).

### 8.2 Sound list (agent finds and logs a source for each)
- Footsteps per surface: terrazzo, tile, wood stage, carpet (library), gravel
  (courtyard), metal stairs; walk/crouch/sprint variants, 6+ each.
- Player: breathing (calm/scared/hold breath), cloth rustle, heartbeat (Calm).
- Flashlight: click on/off, buzz at low charge, crank ratchet loop, charge-up
  chime (soft, warm), flicker crackle.
- World: fluorescent hum + flicker, ceiling fan creak, doors (wood creak, steel
  bang, locked rattle), lockers open/close/inside muffled, fuse box clunk +
  power surge + lights waking, chalk writing/erasing, pages, wind through broken
  glass, dripping pipe, distant Cairo traffic, dogs, a far-off train, crickets
  in the courtyard, school bell (normal + reversed/detuned).
- Shadow: layered whispers (record your own names list), chalk scrape, page
  flutter, low breath, recoil hiss, roll-call voice (processed human voice).
- UI: soft paper/chalk taps, correct (warm bell), wrong (dull wood knock).
- Dawn: birds waking, optional distant fajr call to prayer (respectful,
  undistorted, licensed).
- Music: low drones per wing, stingers for sightings, chase pulse, memory-flash
  theme (solo oud or piano), 4 ending cues.
- Voice: Nour (English + Egyptian Arabic), Mother (warm, literary).

### 8.3 Mixing
Unity Audio Mixer groups: Master > Music, SFX, Ambience, Voice, Shadow, UI.
Snapshots: Explore, Stalk, Chase, Hide (low-pass + heartbeat), Memory Flash
(duck all but Voice/Music), Dawn. Reverb zones per room type (library dead,
corridor medium, assembly hall long). Occlusion: raycast from listener to
source; behind walls apply low-pass + −6 dB. (FMOD is an option if you qualify
for its free indie license [VERIFY].)

### 8.4 Import settings
Short SFX: ADPCM, Decompress On Load, mono. Ambience/music: Vorbis quality
60–70, Streaming. Voice: Vorbis, Compressed In Memory. 48 kHz source → 44.1/48
kHz runtime. Normalize: dialogue −16 LUFS, SFX peaks −3 dBFS.

=====================================================================
## 9. MOBILE CONTROLS & HD UI
=====================================================================
### 9.1 Layout (landscape)
- Left half: floating virtual joystick (appears where the thumb touches);
  push to the outer ring 0.5 s = sprint. Dead zone 10%.
- Right half: swipe-to-look zone (sensitivity slider 0.2–3.0, invert Y,
  optional gyro aim).
- Right cluster buttons (minimum 64×64 dp touch target, 12 dp spacing):
  Interact (context icon: hand, eye, door, fuse, hide), Flashlight
  (tap toggle, hold = focus beam), Crouch (toggle), Crank (hold + circle swipe).
- Top: Calm indicator (subtle heartbeat ring), charge gauge on the flashlight
  model itself (diegetic LED strip) + small HUD fallback, pause, case file.
- Timed choices: 2–4 large cards center screen with a burning-fuse timer bar.
- Full gamepad support (Input System) and haptics (light on correct, heavy on
  caught).
- Customizable HUD: drag buttons, scale 80–130%, opacity 30–100%. Left-handed
  preset.
- Accessibility: subtitles (size options, speaker names), colorblind-safe
  gauges, reduced flicker mode, aim assist on interactables, hold-vs-toggle
  options, Easy question pool, "no chase" story mode.

### 9.2 Controller art (high-res)
Style: brass rim + worn chalk-white glyph + soft glow, matches the 1960s school.
Make icons as SVG (Inkscape/Figma), import with Vector Graphics or export PNG at
512×512 (joystick base 512, knob 256, buttons 256) with 9-slice where needed.
States: normal, pressed (inner glow + 0.92 scale), disabled (40% opacity),
highlighted (pulse when context available). Keep everything on one 2048 sprite
atlas.

### 9.3 Menus
Main menu over a live 3D shot of the gate at dusk. Settings: graphics preset,
FPS 30/60, sensitivity, audio, language (EN/AR), subtitles, accessibility,
controls editor. Pause: resume, case file, settings, restart checkpoint,
main menu. Case file: fragments collected, clues board, questions history.
Arabic UI: mirror layout fully (RTL), RTLTMPro for text shaping, Arabic font
assets with fallback chains.

=====================================================================
## 10. GRAPHICS TARGETS (high-end HD)
=====================================================================
- Lighting: mostly BAKED (GPU lightmapper, Adaptive Probe Volumes or light
  probes for dynamic objects). Per wing two lighting states: DARK and LIT
  (lightmap swapping or baked light layers + emissive switch). Realtime: the
  flashlight (spot, soft shadows, cookie texture of a real flashlight lens),
  max 2 extra realtime shadowed lights on screen.
- Post (URP Volume): ACES or Neutral tonemapping, color grading per wing (cold
  cyan dark / warm sodium spill / gold lit), subtle bloom, film grain, vignette
  (driven by Calm), chromatic aberration only on scares, motion blur off by
  default. Use on-tile post-processing if the Unity version supports it.
- Fog: URP exponential fog + cheap volumetric light cards in window beams.
- Materials: PBR Lit (URP), detail maps for grime, decals (URP Decal Projector,
  DBuffer off on mobile → use screen-space decals) for stains, posters,
  chalk writing.
- Reflection probes: one baked per room.
- Quality presets: HIGH (default high-end), BALANCED (lower shadows, 1.0 render
  scale → 0.85), BATTERY (30 fps). Adaptive Performance lowers render scale
  when the device heats up.
- Render scale 1.0 on HIGH with dynamic resolution allowed down to 0.75.

=====================================================================
## 11. CODE ARCHITECTURE
=====================================================================
### 11.1 Folders
Assets/_Project/
  Art/{Models,Materials,Textures,Animations,VFX,UI}
  Audio/{SFX,Ambience,Music,Voice,Mixers}
  Data/{Story,Questions,Fragments,Endings,Localization}
  Prefabs/{Player,Shadow,Interactables,Environment,UI}
  Scenes/{Boot,MainMenu,School_Main(+additive wing scenes),Ending,Test_*}
  Scripts/{Core,Player,Input,Flashlight,Learning,Story,AI,Interaction,
           Audio,UI,Save,Localization,Platform,Debug}
  Editor/   Tests/{EditMode,PlayMode}   Settings/{URP,Input,Presets}

### 11.2 Core scripts (each a single responsibility)
Core: GameManager, ServiceLocator, EventBus (typed events), GameStateMachine
  (Boot/Menu/Playing/Paused/Memory/Ending), SceneLoader (Addressables).
Player: FirstPersonController (CharacterController), CrouchSystem,
  FootstepSystem (surface by PhysicMaterial/tag), CameraEffects (bob, tremor),
  HidingSystem, NoiseEmitter.
Input: InputRouter (touch/gamepad), FloatingJoystick, LookTouchZone,
  ButtonContext, HUDLayoutEditor.
Flashlight: FlashlightController, ChargeSystem, CrankMinigame, BeamDetector
  (banish), FlickerController.
Learning: QuestionBank (load JSON), QuizBoard (interactable), QuizUI,
  SpacedRepetitionQueue, AccuracyTracker, MarginNoteUI, ClueBoard,
  StudyRecapExporter (PDF/PNG).
Story: StoryGraph, StoryNode, StoryRunner, ChoiceUI (timed), StatsSystem
  (boldness/trust/calm), FlagStore, StoryTriggers, MemoryFlashPlayer,
  EndingResolver.
AI: ShadowBrain (FSM), ShadowSenses (sight/hearing/beam), ShadowDirector
  (menace gauge), PatrolRoute, HideSpotRegistry, LitZone, ShadowVisuals.
World: FuseBox, LightingStateSwitcher, Door, Locker, Inspectable (360° view),
  Fragment, Checkpoint, PosterBlanker, DawnController (sky + light ramp).
Audio: AudioManager, MixerSnapshotController, OcclusionProbe, ReverbZoneSetup,
  AmbienceZones.
Save: SaveSystem (versioned JSON), CheckpointSaver.
Platform: QualityManager, AdaptivePerformanceBridge, HapticsService,
  SpeechInputService (optional native plugin), SafeAreaFitter.
Debug: DevConsole (set flags, teleport, skip to act, toggle AI), FPS overlay.

### 11.3 Data schemas
Question (Data/Questions/<subject>.json):
{ "id":"phy_opt_003", "subject":"physics", "topic":"optics",
  "difficulty":2, "wing":"science",
  "prompt":{"en":"Light travels fastest in…","ar":"ينتقل الضوء أسرع في…"},
  "options":[{"en":"vacuum","ar":"الفراغ"},{"en":"water","ar":"الماء"},
             {"en":"glass","ar":"الزجاج"},{"en":"air","ar":"الهواء"}],
  "correct":0,
  "explanation":{"en":"Nothing slows it down there. — M.","ar":"…"},
  "speak_keywords":{"en":["vacuum"],"ar":["الفراغ"]},
  "source":"<textbook/curriculum ref>" }
Story node (Data/Story/graph.json):
{ "id":"n23_rollcall", "act":2, "type":"choice",
  "text":{"en":"…","ar":"…"}, "speaker":"shadow", "voice":"vo_shadow_23",
  "timer":8, "timeout_next":"n24_froze",
  "choices":[{"id":"c_answer","text":{"en":"That one's mine. You can't have it.","ar":"…"},
    "requires":{"calm_min":40}, "effects":{"boldness":2,"set":["rollcall_answered",
    "back_stairs_known"]}, "next":"n24_pushed"}, …] }
Fragment: { "id":"frag_mug","name":{…},"inspect_text":{…},"memory_flash":"mf_06",
  "location":"staff_room","model":"SM_Story_Mug" }
Ending: { "id":"full","priority":2,"conditions":{"fragments_min":6,
  "accuracy_min":0.75,"any_flag":["rollcall_answered","shadow_forgiven"]},
  "sky":"#E8B54A","music":"mus_end_full","text":{…} }

### 11.4 Rules
Event-driven (no FindObjectOfType in Update), no per-frame allocations, object
pools for VFX/audio, ScriptableObject config for tunables, all text via
Localization tables, all tunables exposed in a single GameBalance asset.

=====================================================================
## 12. FASTEST PRODUCTION WORKFLOW (vertical slice first)
=====================================================================
Estimated: 1 developer + AI agent, ~5–6 weeks. Parallelize art/audio sourcing
with code when possible.

PHASE 0 — SETUP (day 1)
[ ] Verify Unity LTS version, create URP mobile project, packages from §5
[ ] Git + LFS, memory files (§3), folder structure, import presets
[ ] Connect Unity MCP and Blender MCP; test both with a trivial command
[ ] Android + iOS build pipeline; empty build runs on a real device
DoD: blank scene builds and runs at 60 fps on the target phone.

PHASE 1 — GRAYBOX + CONTROLS (days 2–4)
[ ] Whole school blocked out with ProBuilder/Kenney/Quaternius pieces (real scale)
[ ] First-person controller, footsteps, crouch, interaction raycast
[ ] Floating joystick, look zone, context buttons, gamepad
[ ] Flashlight with charge + debug quiz board
[ ] NavMesh baked
DoD: walk the full route gate → library → science → courtyard → assembly →
staff room → gate on a phone.

PHASE 2 — CORE SYSTEMS (days 5–9)
[ ] EventBus, GameState, Save/Checkpoint
[ ] QuestionBank + QuizBoard + margin notes + spaced repetition + accuracy
[ ] StoryGraph runner, timed choices, stats, flags, EndingResolver
[ ] FuseBox + lighting states, Fragments + Inspectable, Case puzzles (3)
[ ] PlayMode tests: all 4 endings reachable via scripted flag sets
DoD: full game playable start to all endings with placeholder text/art.

PHASE 3 — SHADOW AI (days 10–13)
[ ] Senses, FSM, Director, hiding, lit-zone rules, banish
[ ] Roll-call scripted sequence, dawn chase + back stairs
[ ] Tuning pass on device (fairness, no cheap catches)
DoD: 5 external playtesters can finish; nobody feels it is unfair.

PHASE 4 — VERTICAL SLICE ART (days 14–18)
[ ] Final art for entrance hall + library wing only (kit, props, lighting both
    states, post, audio)
[ ] Hero items (box, card, bookmark, exam, flashlight) at 4K
DoD: slice looks and sounds "ship quality" on device at stable fps. Lock the
look in STYLE_GUIDE.md.

PHASE 5 — FULL ART (days 19–26)
[ ] Science wing, courtyard, assembly hall, staff room, corridors, gatehouse
[ ] Shadow visuals + animations, first-person arms
[ ] Bake lighting per wing, reflection probes, decals, grime pass
DoD: entire school art-complete, no placeholder visible.

PHASE 6 — AUDIO (days 20–27, parallel)
[ ] Source every sound in §8.2, log licenses
[ ] Mixer, snapshots, occlusion, reverb zones, music cues
[ ] Record/generate VO (EN + AR), subtitles timed
DoD: game playable with eyes closed and still scary.

PHASE 7 — CONTENT + LOCALIZATION (days 22–28)
[ ] Write all 44 nodes, 12 choices, memory flashes, notes (EN + AR)
[ ] Question bank: ≥ 120 questions (24/wing, 3 difficulty pools), each with an
    explanation and a source; human reviews accuracy
[ ] Study recap export
DoD: native Arabic speaker approves text and RTL layout.

PHASE 8 — OPTIMIZATION + QA (days 29–33)
[ ] Profile on 3 devices (Unity Profiler, Frame Debugger, Memory Profiler)
[ ] Meet §13 budgets; thermal test 30 min continuous play
[ ] Bug bash, accessibility check, crash-free run ×10
DoD: 60 fps (or locked 30 in battery mode), no crash, no softlocks.

PHASE 9 — RELEASE (days 34–36)
[ ] Icons, store screenshots, trailer capture, privacy policy (no data
    collected by default), age rating questionnaires
[ ] Credits from ASSET_LICENSES.csv, final license audit
[ ] Signed AAB (Google Play) + Xcode archive (App Store / TestFlight)

=====================================================================
## 13. PERFORMANCE BUDGETS & QA
=====================================================================
- Frame time: 16.6 ms (GPU ≤ 12 ms, CPU main ≤ 10 ms).
- Visible triangles ≤ 600k; SetPass calls ≤ 120; draw calls ≤ 250.
- Texture memory ≤ 900 MB; total RAM ≤ 2.5 GB; install size ≤ 1.5 GB
  (use Play Asset Delivery / On-Demand Resources if bigger).
- Realtime shadowed lights on screen ≤ 3. Particles ≤ 2k alive.
- GC allocations in gameplay: 0 B/frame target.
- Occlusion culling baked; LOD cross-fade; static batching for kit.
Test matrix: 1 recent iPhone, 1 Snapdragon flagship, 1 older high-end
Android. Tests: EditMode (story graph integrity: every node reachable, no dead
ends, all localization keys exist), PlayMode (endings, save/load, AI
smoke test), manual (controls feel, readability, scares).

=====================================================================
## 14. RELEASE CHECKLIST
=====================================================================
App IDs, signing, versioning, privacy labels, content rating (horror themes,
no violence/gore), store text EN/AR, accessibility statement, credits screen,
crash reporting (opt-in), final license audit — every asset traceable.

=====================================================================
## 15. TASK PROMPT TEMPLATES
=====================================================================
T1 CODE: "Implement <system> per §<n>. Files: <list>. Constraints: §11.4.
Include EditMode/PlayMode tests. Follow §2.D."
T2 BLENDER: "Using Blender MCP, take <file/source> and run the §7.4 recipe.
Budget <tris>. Output FBX to Art/Export/<Area>/ and a preview PNG. Log license."
T3 ASSET HUNT: "Find <N> candidates for <asset> from §7.1 sources only. For each:
URL, license, why it fits, needed edits. Prefer CC0."
T4 AUDIO: "Source all sounds for <area> from §8.2. Return a table: sound,
source URL, license, file name per naming convention, import settings."
T5 WRITING: "Write story nodes <ids> in EN and Egyptian-flavored MSA Arabic,
in the established voices (§4.3). Output JSON per §11.3."
T6 QUESTIONS: "Write <N> <subject> questions for the <wing>, difficulty <d>,
with explanations in the mother's voice and a source for each. JSON per §11.3."
T7 REVIEW: "Review <system/scene> against §4, §9, §10, §13. List problems by
severity with fixes."
T8 DEBUG: "Here is the console/log/profiler output: <paste>. Find the root
cause, fix it, explain how to verify."

=====================================================================
## 16. FIRST ACTIONS FOR THE AGENT
=====================================================================
1. Confirm you read everything. List any [VERIFY] items you can resolve now.
2. Ask the human only these if still unknown: Unity version installed,
   Blender version, GPU/VRAM (for local AI 3D), Apple developer account (y/n),
   question curriculum/grade preference.
3. Create the memory files and TASKS.md from §12.
4. Start PHASE 0.
