# SOURCING: Fonts + Music (premium pass) — LIGHT SHADOW

Research-only sourcing doc for the premium pass: embeddable fonts (OFL/Apache) and
commercial-usable music (CC0 / CC-BY only — **no NC licenses anywhere**). Every URL below was
resolved and verified on **2026-11-19** (HTTP 200/206 check or full page fetch; binary font/audio
links were range-requested to confirm they serve real files). **No files have been downloaded yet.**

Sourcing rules used (per `AGENTS.md` rule 4):
- Fonts must be OFL 1.1 or Apache 2.0 (both permit embedding in commercial games; OFL restricts
  only standalone font resale and Reserved-Font-Name derivatives).
- Music must be CC0 or CC-BY (attribution possible). Avoided: BBC Sound Effects, NC-licensed
  material, YouTube-audio-library rips.
- Every download must be logged in `Docs/ASSET_LICENSES.md` before the file is committed, and
  `.meta` files are committed next to assets (rules 4 and 7).

---

## PART 1 — FONTS

All five families verified in the `google/fonts` GitHub repo (license files present in-repo).
Raw `.ttf` links were range-checked (HTTP 206, `application/octet-stream`) on 2026-11-19.

| # | Font (weights) | Role | License (verified) | Stable download URL | Arabic shaping | Unity import note |
|---|---|---|---|---|---|---|
| 1a | **Noto Naskh Arabic** Regular + Bold | Arabic body text (quiz passages, story) | SIL OFL 1.1 — `OFL.txt` in repo: https://raw.githubusercontent.com/google/fonts/main/ofl/notonaskharabic/OFL.txt ("Copyright 2022 The Noto Project Authors") | Static Regular+Bold: <https://fonts.google.com/download?family=Noto%20Naskh%20Arabic> (zip contains `static/NotoNaskhArabic-Regular.ttf`, `static/NotoNaskhArabic-Bold.ttf` — verified via https://fonts.google.com/download/list?family=Noto%20Naskh%20Arabic). Raw variable: <https://raw.githubusercontent.com/google/fonts/main/ofl/notonaskharabic/NotoNaskhArabic%5Bwght%5D.ttf>. Hinted statics: <https://notofonts.github.io/arabic/fonts/NotoNaskhArabic/hinted/ttf/NotoNaskhArabic-Regular.ttf> and `-Bold.ttf`; release zip: <https://github.com/notofonts/arabic/releases/download/NotoNaskhArabic-v2.021/NotoNaskhArabic-v2.021.zip> | **Native** (OpenType init/medi/fina/rlig). Also maps legacy **Arabic Presentation Forms-B (U+FE70–FEFF)** ([openSUSE fontinfo](https://fontinfo.opensuse.org/fonts/NotoNaskhArabicBold.html)) — compatible with this repo's `ArabicSupport` display-time shaping | Use the **static** Regular/Bold TTFs (Unity imports variable fonts as the default instance only). TMP source font file, Character Set = **Dynamic (Unicode)**, include ranges U+0600–06FF + U+FE70–FEFF, multi-atlas enabled, atlas ≥ 1024×1024 for diacritics. Best default for `Localization` body text |
| 1b | **Cairo** Regular + Bold (+ SemiBold) | Arabic+Latin UI / titles | SIL OFL 1.1 — `OFL.txt` in repo: https://raw.githubusercontent.com/google/fonts/main/ofl/cairo/OFL.txt ("Copyright 2009 The Cairo Project Authors — github.com/Gue3bara/Cairo") | Statics (Regular…Black): <https://fonts.google.com/download?family=Cairo> (zip lists `Cairo-Regular.ttf`, `Cairo-SemiBold.ttf`, `Cairo-Bold.ttf` + `OFL.txt` — verified via https://fonts.google.com/download/list?family=Cairo). Raw variable: <https://raw.githubusercontent.com/google/fonts/main/ofl/cairo/Cairo%5Bslnt%2Cwght%5D.ttf> | **Native** OpenType Arabic shaping (Arabic/Farsi/Urdu glyph set), but the modern build **does not map legacy Presentation Forms-B (U+FE70–FEFF)** — a display-time shaper that outputs FE70+ codepoints (e.g. classic ArabicSupport pipelines) can show missing glyphs ([Unity Discussions: Arabic Fonts Help](https://discussions.unity.com/t/arabic-fonts-help), [TheGamingNest guide](https://thegamingnest.com/en/articles/0febb621-1188-4226-a4d1-68c2d3e4b2a4)). **Test with `ArabicSupport` before adopting for Arabic UI**; Naskh (1a) is the safe fallback | Import static `Cairo-Regular.ttf` + `Cairo-Bold.ttf` from the zip. Set as UI/title font; add Noto Naskh Arabic as TMP fallback for body |
| 1c | **Reem Kufi** Regular + Bold | Arabic display alternative (title cards, headers) | SIL OFL 1.1 (with Reserved Font Name) — `OFL.txt`: https://raw.githubusercontent.com/google/fonts/main/ofl/reemkufi/OFL.txt ("Copyright 2015-2022 The Reem Kufi Project Authors — github.com/aliftype/reem-kufi") | Statics: <https://fonts.google.com/download?family=Reem%20Kufi> (zip lists `static/ReemKufi-Regular.ttf` … `static/ReemKufi-Bold.ttf` — verified via https://fonts.google.com/download/list?family=Reem%20Kufi). Raw variable: <https://raw.githubusercontent.com/google/fonts/main/ofl/reemkufi/ReemKufi%5Bwght%5D.ttf> | **Native** (kufi-style GSUB shaping). Same **FE70 caveat as Cairo** — verify against `ArabicSupport` output before use on shaped Arabic strings | Display only — geometric kufi is poor for long body text. Single weight (Bold) is enough for title cards |
| 1d | **Special Elite** Regular | Typewriter notes / props (paper scraps, quiz-board notes) | **Apache License 2.0** — `LICENSE.txt` in repo: https://raw.githubusercontent.com/google/fonts/main/apache/specialelite/LICENSE.txt (verified "Apache License, Version 2.0, January 2004") | <https://raw.githubusercontent.com/google/fonts/main/apache/specialelite/SpecialElite-Regular.ttf> (single static weight; specimen: https://fonts.google.com/specimen/Special+Elite) | **None — Latin only.** Use only for English notes/props; any Arabic on the same object must use a separate Arabic font | Single static weight → ideal for a TMP **static atlas** (fixed character set = the actual note strings); keeps memory tiny for world-space `LocalizedWorldText` props |
| 1e | **Inter** Regular + SemiBold | Latin UI fallback (HUD numbers, settings) | SIL OFL 1.1 — `OFL.txt`: https://raw.githubusercontent.com/google/fonts/main/ofl/inter/OFL.txt ("Copyright 2020 The Inter Project Authors — github.com/rsms/inter") | Raw variable: <https://raw.githubusercontent.com/google/fonts/main/ofl/inter/Inter%5Bopsz%2Cwght%5D.ttf>. Statics: <https://fonts.google.com/download?family=Inter> (zip lists `Inter_18pt/24pt/28pt-Regular…Bold` + italics + `OFL.txt` — verified via https://fonts.google.com/download/list?family=Inter) | **None — Latin/Cyrillic/Greek only.** Every user-facing Arabic string must route to Cairo/Naskh (TMP fallback list) | Use `Inter_24pt-Regular.ttf` + `Inter_24pt-SemiBold.ttf` statics for crisp UI; add Cairo/Naskh as TMP fallbacks so `LocalizedText` renders both EN and AR (rule: both languages filled) |

Notes for Part 1:
- Google Fonts zips contain the `OFL.txt` for each family — copy it next to the imported font
  (see checklist) **and** add a row to `Docs/ASSET_LICENSES.md`. OFL FAQ: <https://openfontlicense.org> (HTTP 200, verified).
- The FE70 issue only affects **legacy presentation-form shapers**. If the premium pass swaps
  `ArabicSupport` for a proper OpenType shaping pipeline, Cairo/Reem Kufi become fully safe.

---

## PART 2 — MUSIC (10 primary candidates, 4 alternates)

All 10 primary candidates are on **Pixabay Music** under the **Pixabay Content License** —
verified per track page (each page states: *"Free for use under the Pixabay Content License"*),
free MP3 download, no attribution required, commercial use allowed, embedding in a game allowed
(not "standalone" redistribution). License summary (verified): <https://pixabay.com/service/license-summary/> —
it allows free commercial use and modification; prohibits only selling/redistributing content on a
standalone basis and uses with recognizable brands. Full Content License is linked from that page
and from every track page (<https://pixabay.com/service/license/> is bot-protected to scripts —
open it in a browser when downloading).

### Tension beds (night-school interior) — 3

| Track | Artist | Page URL | License / attribution | Why it fits | Duration | Loopability |
|---|---|---|---|---|---|---|
| Dark Ambient Soundscape Dreamscape | SolarFLEX | https://pixabay.com/music/ambient-dark-ambient-soundscape-dreamscape-569591/ | Pixabay Content License; no attribution required (optional credit below) | Tagged Dark/Dreamy/Mysterious, Floating/Slow ambient-electronic — a sustained dread bed under flashlight exploration | 4:17 | Drone-style pad; loops cleanly with a short crossfade seam (verify seam in editor) |
| Dark Ambient | The_Mountain | https://pixabay.com/music/suspense-dark-ambient-509934/ | Pixabay Content License; no attribution required | Genre-tagged Suspense/Crime Scene; Dark, Mysterious, Suspense; slow-floating — darker, more "stalker" than SolarFLEX | 4:08 | Long evolving bed; loop with crossfade. **Page flags it as AI-generated** (still CC-licensed; note in ASSET_LICENSES.md if used) |
| Dark Dusk Ambience | drmseq | https://pixabay.com/music/ambient-dark-dusk-ambience-506464/ | Pixabay Content License; no attribution required | Modular-synth textures, Dreamy/Mysterious, Floating/Slow — most "empty hallway at dusk" of the three; 6-min bed survives long exploration segments | 6:00 | Ambient texture, no beat; loops with crossfade |

### Chase pulse — 1

| Track | Artist | Page URL | License / attribution | Why it fits | Duration | Loopability |
|---|---|---|---|---|---|---|
| Chase | leberch | https://pixabay.com/music/main-title-chase-254539/ | Pixabay Content License; no attribution required | Tagged Restless/Suspense/Energetic with Chasing-Running-Fast movement; orchestral brass + percussion build — reads as pursuit without being combat-y (fits "no combat" tone) | 1:36 | Fast pulse; loops on the beat (verify seam). Human-made (no AI flag) — avoided AI-flagged "Chase Pulse" by Mohamed_hassan for this slot |

### Memory-flash tender theme — 1

| Track | Artist | Page URL | License / attribution | Why it fits | Duration | Loopability |
|---|---|---|---|---|---|---|
| Besides The Motion (Emotional Piano) | leberch | https://pixabay.com/music/solo-piano-besides-the-motion-emotional-piano-615166/ | Pixabay Content License; no attribution required | Solo piano; Dreamy/Relaxing/Hopeful/Bright, Elegant/Slow — warm enough for a memory flash, not saccharine. (No decent CC0/CC-BY **oud** track was found license-verified; solo piano was the preferred fallback in the brief) | 3:08 | One-shot cue for memory cutscene; not looped (fade out) |

### Ending cues — 4 (ending = 3D dawn set + study recap)

| Mood | Track | Artist | Page URL | License / attribution | Why it fits | Duration | Loopability |
|---|---|---|---|---|---|---|---|
| Warm gold hopeful | Hopeful Piano | PaulYudin | https://pixabay.com/music/wedding-hopeful-piano-156908/ | Pixabay Content License; no attribution required | Tagged Bright/Hopeful/Uplifting/Sentimental, Romantic — the "earned a bright idea" gold tone for the good ending | 2:01 | One-shot cue (ends resolved); no loop |
| Pale bittersweet | Asagiri -Nostalgic Piano | HarumachiMusic | https://pixabay.com/music/solo-piano-asagiri-nostalgic-piano-494120/ | Pixabay Content License; no attribution required | Solo piano, Nostalgia genre; Dreamy/Hopeful/Sentimental/Emotional — "Asagiri" (朝霧, morning mist) is literally a pale-dawn piece: bittersweet recap ending | 2:03 | One-shot cue; no loop |
| Hollow cold | Cold Winter Night – Ambient Background Music | Clavier-Music | https://pixabay.com/music/ambient-cold-winter-night-ambient-background-music-385726/ | Pixabay Content License; no attribution required | Ambient/Mystery, Dreamy/Mysterious/Suspense, Floating/Slow — an emptied, cold-after-the-fact drone for the hollow ending | 4:20 | Ambient bed; can loop softly under recap text |
| Dawnless grey | Dark Depression | Ashot_Danielyan | https://pixabay.com/music/ambient-dark-depression-116110/ | Pixabay Content License; no attribution required | Ambient + Low Drones, Drama Scene; Dark/Sad — heavy grey drone for the "dawn never came" ending | 2:46 | Drone; loops or fades to silence. Note: artist page restricts redistributing the *raw file* to streaming platforms — in-game use is unaffected |

### Alternates (verified, keep in reserve)

| Track | Artist | Page URL | License / attribution | Fit | Duration |
|---|---|---|---|---|---|
| Long Note One | Kevin MacLeod (incompetech.com) | Track page: https://incompetech.com/music/royalty-free/index.html?isrc=USUAN1100418 · Direct MP3 (verified 206): https://incompetech.com/music/royalty-free/mp3-royaltyfree/Long%20Note%20One.mp3 | CC BY (3.0-era file; incompetech licensing page says "No charge. Requires that you credit the music" — https://incompetech.com/music/royalty-free/licenses/). Credit: `"Long Note One" Kevin MacLeod (incompetech.com) Licensed under Creative Commons: By Attribution 4.0 http://creativecommons.org/licenses/by/4.0/` — confirm the version shown on the track page at download time (Commons documents CC BY 3.0 for 2014-era files: https://commons.wikimedia.org/wiki/Category:Soundtrack_music_from_Incompetech) | From the *Horror Soundscapes* album; Dark/Intense/Suspenseful/Unnerving (basses+violins) — first alternate tension bed | 7:20 |
| Anguish | Kevin MacLeod | Track page: https://incompetech.com/music/royalty-free/index.html?isrc=USUAN1400047 · MP3 (verified 206): https://incompetech.com/music/royalty-free/mp3-royaltyfree/Anguish.mp3 | CC BY (3.0 documented on Commons file page: https://commons.wikimedia.org/wiki/File:Anguish_(ISRC_USUAN1400047).mp3; check track page). Credit: `"Anguish" Kevin MacLeod (incompetech.com) Licensed under Creative Commons: By Attribution 3.0/4.0 http://creativecommons.org/licenses/by/<version>/` | Somber/calming cinematic — alternate dawnless-grey ending cue | 3:59 |
| Frozen Star | Kevin MacLeod | Track page: https://incompetech.com/music/royalty-free/index.html?isrc=USUAN1100356 · MP3 (verified 206): https://incompetech.com/music/royalty-free/mp3-royaltyfree/Frozen%20Star.mp3 | CC BY (see Commons: https://commons.wikimedia.org/wiki/File:Frozen_Star_(ISRC_USUAN1100356).mp3; check track page). Credit: `"Frozen Star" Kevin MacLeod (incompetech.com) Licensed under Creative Commons: By Attribution <version> http://creativecommons.org/licenses/by/<version>/` | Calming/Mystical/Relaxed/Uplifting — alternate warm-hope or pale ending cue | 3:41 |
| dark-highway-master | John Bartmann | https://freemusicarchive.org/music/John_Bartmann/100-ambient-atmospheric-soundtracks-straylight-drones-collection/dark-highway-master | **CC0 1.0 Universal** (stated on FMA page; verified). No attribution required (courtesy: `"dark-highway-master" John Bartmann — CC0 1.0, freemusicarchive.org`) | Album *100 Ambient Atmospheric Soundtracks: Straylight Drones Collection* — first alternate tension bed; CC0 = zero license risk | 4:04 |

Optional credit line for Pixabay tracks (not required, good practice):
`"«Track»" by «Artist» — pixabay.com/music, Pixabay Content License`.

License reference links (all verified HTTP 200):
CC BY 4.0 <https://creativecommons.org/licenses/by/4.0/> · CC BY 3.0 <https://creativecommons.org/licenses/by/3.0/> ·
CC0 <https://creativecommons.org/publicdomain/zero/1.0/> · Pixabay summary <https://pixabay.com/service/license-summary/>.

Content-ID note: several Pixabay tracks are "Content ID Registered" — that affects YouTube uploads
of game *footage*, not distribution of the game itself. Record it in `Docs/ASSET_LICENSES.md` if a
track is adopted.

---

## PART 3 — Dawn ambience (CC0-style beds, optional quick wins)

Kenney's audio packs (Interface/Impact/UI/Jingles/RPG/Digital/Sci-fi/Voiceover —
<https://kenney.nl/assets/category:Audio>) are all **CC0** (verified on the pack page:
<https://kenney.nl/assets/ui-audio> — "License: Creative Commons CC0"), but they contain **no
birds/city ambience**, so the dawn bed comes from Pixabay Sound Effects (same Content License):

| Track | Artist | Page URL | License | Why it fits | Duration |
|---|---|---|---|---|---|
| Morning Birdsong Ambience | STRACHszydło | https://pixabay.com/sound-effects/nature-morning-birdsong-ambience-544142/ | Pixabay Content License (quoted on page); free MP3 | Long dawn-chorus bed for the 3D dawn set | 7:20 |
| Ambience city daytime birds traffic sound effect | lwdickens (freesound_community) | https://pixabay.com/sound-effects/city-ambience-city-daytime-birds-traffic-61955/ | Pixabay Content License (quoted on page); free MP3 | Birds **+** traffic in one layer — the "school is waking up" mix | 3:23 |
| City-Above-Far-Ambience-Car | CliffordJohnson | https://pixabay.com/sound-effects/city-city-above-far-ambience-car-479129/ | Pixabay Content License (quoted on page); free MP3 | Distant-city wash for layering under the birds at low volume | 3:50 |

---

## Download checklist (ordered — do NOT run yet; downloads are a separate task)

Every downloaded file gets a `Docs/ASSET_LICENSES.md` row (rule 4) and its `.meta` committed
beside it (rule 7). Fonts go to `Assets/_Project/Fonts/` (new folder), license texts to
`Assets/_Project/Fonts/Licenses/`. Audio follows the existing folders
(`Audio/Music/`, `Audio/Ambience/`; precedent: `AMB_SchoolNight.mp3`).

1. **Fonts** (download in this order; rename after download):
   - <https://fonts.google.com/download?family=Noto%20Naskh%20Arabic> → unzip → `static/NotoNaskhArabic-Regular.ttf`, `static/NotoNaskhArabic-Bold.ttf`
     → `Assets/_Project/Fonts/T_NotoNaskhArabic-Regular.ttf`, `T_NotoNaskhArabic-Bold.ttf`
   - <https://fonts.google.com/download?family=Cairo> → `Cairo-Regular.ttf`, `Cairo-Bold.ttf`
     → `T_Cairo-Regular.ttf`, `T_Cairo-Bold.ttf`
   - <https://fonts.google.com/download?family=Reem%20Kufi> → `static/ReemKufi-Bold.ttf` → `T_ReemKufi-Bold.ttf`
   - <https://raw.githubusercontent.com/google/fonts/main/apache/specialelite/SpecialElite-Regular.ttf> → `T_SpecialElite-Regular.ttf`
   - <https://fonts.google.com/download?family=Inter> → `Inter_24pt-Regular.ttf`, `Inter_24pt-SemiBold.ttf` → `T_Inter-Regular.ttf`, `T_Inter-SemiBold.ttf`
   - Copy each zip's `OFL.txt` (Special Elite: Apache `LICENSE.txt`) → `Assets/_Project/Fonts/Licenses/`
2. **Music — beds** → `Assets/_Project/Audio/Ambience/`:
   - <https://pixabay.com/music/ambient-dark-ambient-soundscape-dreamscape-569591/> → `AMB_SchoolDreadA.mp3`
   - <https://pixabay.com/music/ambient-dark-dusk-ambience-506464/> → `AMB_SchoolDreadB.mp3`
   - <https://pixabay.com/music/suspense-dark-ambient-509934/> → `AMB_SchoolSuspense.mp3` (AI-generated flag — decide before use)
3. **Music — cues** → `Assets/_Project/Audio/Music/`:
   - <https://pixabay.com/music/main-title-chase-254539/> → `MUS_ChasePulse.mp3`
   - <https://pixabay.com/music/solo-piano-besides-the-motion-emotional-piano-615166/> → `MUS_MemoryTheme.mp3`
   - <https://pixabay.com/music/wedding-hopeful-piano-156908/> → `MUS_Ending_HopeGold.mp3`
   - <https://pixabay.com/music/solo-piano-asagiri-nostalgic-piano-494120/> → `MUS_Ending_Bittersweet.mp3`
   - <https://pixabay.com/music/ambient-cold-winter-night-ambient-background-music-385726/> → `MUS_Ending_HollowCold.mp3`
   - <https://pixabay.com/music/ambient-dark-depression-116110/> → `MUS_Ending_DawnlessGrey.mp3`
4. **Dawn ambience** → `Assets/_Project/Audio/Ambience/`:
   - <https://pixabay.com/sound-effects/nature-morning-birdsong-ambience-544142/> → `AMB_DawnBirds.mp3`
   - <https://pixabay.com/sound-effects/city-ambience-city-daytime-birds-traffic-61955/> → `AMB_DawnCity.mp3`
   - <https://pixabay.com/sound-effects/city-city-above-far-ambience-car-479129/> → `AMB_DawnCityFar.mp3`
5. **Only if a primary track is rejected during review** — pull the matching alternate from the
   Part 2 alternates table and paste its credit line into `Docs/ASSET_LICENSES.md`.
6. After import: set TMP font assets (Dynamic charset, Arabic ranges U+0600–06FF + U+FE70–FEFF,
   multi-atlas), bind via `UIFontBinder`, and run `ValidateBatch` (grep `RESULT: PASS`).
   Audio: import as-is (.mp3, precedent `AMB_SchoolNight.mp3`); loop-point trims happen in-editor
   (`AudioSource.loop = true` is config/`GameConfig`-driven per rule 2 — no magic numbers).

---

## Verification appendix (2026-11-19)

- Raw font `.ttf` links + notofonts statics: HTTP 206/200, `application/octet-stream` or `font/ttf`.
- `fonts.google.com/download/list?family=...` endpoints: JSON manifests verified (Cairo, Noto Naskh
  Arabic, Reem Kufi, Inter — statics + `OFL.txt` present).
- notofonts release zip `NotoNaskhArabic-v2.021.zip`: HTTP 206.
- Every Pixabay track/SFX page listed: fetched; each shows "Free for use under the Pixabay Content
  License" + free MP3 download.
- Incompetech MP3 direct links: HTTP 206. Track pages (`index.html?isrc=...`) are JS-rendered —
  resolved (HTTP 200) but metadata was verified via Wikimedia Commons file pages + web search.
- FMA track page: fetched; "CC0 1.0 Universal License" stated on page.
- Canonical license pages (CC0/CC-BY-3.0/CC-BY-4.0/openfontlicense.org): HTTP 200.
  Pixabay full-license URL returns 403 to scripts (bot protection) — verify in a browser at download time.

## Sources

- https://github.com/google/fonts/tree/main/ofl/notonaskharabic · .../ofl/cairo · .../ofl/reemkufi · .../ofl/inter · .../apache/specialelite
- https://raw.githubusercontent.com/google/fonts/main/ofl/notonaskharabic/OFL.txt · /ofl/cairo/OFL.txt · /ofl/reemkufi/OFL.txt · /ofl/inter/OFL.txt · /apache/specialelite/LICENSE.txt
- https://fonts.google.com/download/list?family=Noto%20Naskh%20Arabic · ?family=Cairo · ?family=Reem%20Kufi · ?family=Inter
- https://notofonts.github.io/arabic/fonts/NotoNaskhArabic/hinted/ttf/NotoNaskhArabic-Regular.ttf · -Bold.ttf
- https://github.com/notofonts/arabic/releases/latest (v2.021) · release zip URL above
- https://fontinfo.opensuse.org/fonts/NotoNaskhArabicBold.html (Presentation Forms-B coverage)
- https://discussions.unity.com/t/arabic-fonts-help · https://thegamingnest.com/en/articles/0febb621-1188-4226-a4d1-68c2d3e4b2a4 (TMP presentation-forms limitation)
- Pixabay: https://pixabay.com/service/license-summary/ · all track/SFX URLs in the tables
- Incompetech: https://incompetech.com/music/royalty-free/licenses/ · track pages + MP3 URLs above
- Wikimedia Commons (KM license documentation): https://commons.wikimedia.org/wiki/Category:Soundtrack_music_from_Incompetech · Anguish · Dreamy Flashback file pages
- FMA: https://freemusicarchive.org/music/John_Bartmann/100-ambient-atmospheric-soundtracks-straylight-drones-collection/dark-highway-master
- Kenney: https://kenney.nl/assets/category:Audio · https://kenney.nl/assets/ui-audio (CC0 statement)
- CC licenses: https://creativecommons.org/licenses/by/4.0/ · /by/3.0/ · /publicdomain/zero/1.0/ · https://openfontlicense.org
