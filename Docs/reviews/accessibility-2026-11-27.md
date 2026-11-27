# Accessibility review (report only), 2026-11-27

Package 4 of `Docs/YUSUF_WORK_PLAN.md`. Written by an AI agent working for Yusuf. Nothing here is applied. No production file was changed.

## How to read this

- **[M] Measured/computed**: contrast ratios computed with the WCAG 2.x formula from the colour constants in the builder code; font sizes and coverage read from source or from the font files.
- **[E] Estimate**: depends on a value that was not available (real device DPI, the generated UI panel gradient, in-scene backgrounds). A hypothesis to check on a device.
- Unity was not available, so nothing was seen on screen. No Arabic rendering was inspected visually; the Arabic section covers font coverage and settings and needs a native-speaker visual check.
- Contrast backgrounds: flat colours `PanelDark` (0.055, 0.055, 0.065), `ButtonDark`, `ButtonWarm` and the dialogue panel (0.045, 0.045, 0.06). The real panels use a generated gradient sprite (`MakePanel`, `SceneBuilderUI.cs:940`), so the real background differs somewhat [E].
- Base commit: `main` at bf14cd2. The ending cinematic work (PRs #59, #60, #61) is open and not merged, so the ending findings describe `main`, not those branches.

## Summary

| # | Finding | Severity |
|---|---|---|
| 1 | Some spoken narration has no matching on-screen caption; sound-only gameplay cues have no caption | High |
| 2 | UI text is small on phones and small windows: 14 to 16 px reference text is about 6 dp on a phone [E] | High |
| 3 | The subtitle size setting changes only the dialogue body | Medium |
| 4 | Colourblind palette 1 (blue/orange): Good and Bad have a luminance ratio of 1.01 | Medium |
| 5 | Main-menu footer is 3.91:1 on the dark panel; a few dim colours fall under 4.5:1 on buttons | Medium |
| 6 | Arabic: font coverage is good; no size compensation; the planned ending lines use diacritics | Medium |

## 1. Subtitle coverage

Subtitles are the dialogue box (`Story/DialogueUIController.cs`).

- Story node lines show in the dialogue box and are also spoken (`Story/StoryDirector.cs:323-326`: a generated clip, or the narrator as fallback). Covered [M].
- Opening narration, `Core/GameManager.cs:103`, is spoken ("The school remembers her mother. Nour must carry what is precious out before dawn."). The message shown just before (line 98) is a different sentence, so the spoken line has no caption [M].
- Gate powered, `Core/GameManager.cs:212`: spoken "The gate is powered. The Shadow is right behind you. Run for the dawn." Shown: "The gate is powered. Run." This is a shortened message, not a caption [M].
- Knowledge fragment, `Gameplay/KnowledgeFragment.cs:49-52`: same text shown and spoken. Covered [M].
- Ending on `main`, `UI/EndingController.cs:132-149`: the body text is on screen. The closing narrator line (second localization key) is spoken but not displayed [M].
- `Systems/CinematicRunner.cs` and `Systems/CinematicDirector.cs` have no text or caption code [M].
- Ending cinematic spec: `Docs/ENDING_VO_SPEC.md` is not on `main`. It is in open PR #59 (branch `feat/ending-vo-spec`). It lists 14 lines (`SN_Ending_01` to `_14`) in EN and AR and says the planned `CinematicEndingController` plays each line "with subtitles" and tolerates missing clips (section 7). That is the right design. This review could not check it against code because that controller is not on `main`. When #59/#60/#61 land, check that every one of the 14 lines is displayed for as long as it is spoken and that the Shadow's broken lines ("You are leaving with…") are not cut short.
- Sound-only gameplay cues (Shadow whispers, moans, footsteps, ambience changes; `Docs/AUDIO_CUES.md`): no caption or visual indicator system was found in `Scripts/**` [M for the code search, E for gameplay impact]. The Shadow-proximity cues are a gameplay hint a deaf or hard-of-hearing player cannot get.
- Narration exists only on Windows (SAPI) or as generated clips; elsewhere the narrator stays silent (`Core/StoryTeller.cs:11-14`). Players there only get lines that are on screen, so the uncaptioned lines above are lost to them [M].

**Recommend:** when `StoryTeller.Speak` or `VoiceoverPlayer.Play` runs with text that is not already on screen (`GameManager.cs:103,212`, `EndingController.cs:146`), show that sentence in the message strip or a caption bar. Add short captions for Shadow-proximity cues, for example "[whisper nearby]".

## 2. Minimum text sizes

Canvas: `ScaleWithScreenSize`, reference 1920x1080, match 0.5 (`SceneBuilderUI.cs:863-867`) [M]. Scale = sqrt((w/1920) x (h/1080)): 1.00 at 1920x1080, 0.67 at 1280x720, 1.12 at 2400x1080.

Sizes in the reference space [M]:
- 14 px: AUTO-ADVANCE button (`SceneBuilderStory.cs:819`)
- 15 px: HOLD TO SKIP (`SceneBuilderStory.cs:813`), pause hint (`SceneBuilderUI.cs:492`), all puzzle footers (`SceneBuilderUIPuzzles.cs:75,147,214,293,360,423`)
- 16 px: menu footer, quiz footer (`SceneBuilderUI.cs:439,694,1032`)
- 17 px: save-slot info and DELETE (`SceneBuilderStory.cs:665,682`)
- 18 to 19 px: menu eyebrow, chips, many buttons, loading line, puzzle counters and progress
- 20 to 24 px: most body text, objectives, quiz timer
- Dialogue body is `24 x SubtitleScale`, scale 0.85 / 1.0 / 1.3 (`Story/AccessibilityStore.cs:53-65`, `DialogueUIController.cs:158`): about 20 / 24 / 31 px.

On screen [E]:
- 1280x720 window: 15 px text draws at about 10 px, 18 px at 12 px, 24 px at 16 px.
- Phone at 2400x1080, assuming 420 dpi (2.6 px per dp): 15 px reference is about 17 px, about 6.4 dp. Dialogue body is about 10 dp at Normal, 8.7 dp at Small, 12.8 dp at Large. Common Android guidance is 12 sp as the smallest readable size.

**Recommend:** a floor of 18 reference px for all text. Raise the dialogue base from 24 to 28 and the scales to 1.0 / 1.25 / 1.5. Raise the skip and auto buttons to 18. Check on a real phone first.

## 3. Settings versus coverage

`AccessibilityStore` (`Story/AccessibilityStore.cs`): text speed (0 to 3, with instant), subtitle size (0 to 2), colourblind mode (0 to 2), auto-advance, skip-hold (0.4 / 0.8 / 1.5 s), quality tier.

- Subtitle size applies only to the dialogue body (`DialogueUIController.cs:158`). Speaker name, choices, message strip, objectives, quiz and puzzle text stay the same [M]. The settings list calls it "Subtitle size" (`UI/SettingsPanelController.cs:83`), which fits dialogue only.
- Colourblind mode affects only elements the story pass owns (dialogue states, choices, timer bar, save slots, loading bar), per the comment at `AccessibilityStore.cs:121-127`. World labels are not touched [M].
- No setting for general UI scale, reduced flicker (lamps flicker by design, `Core/LightFlicker.cs`), or sound captions [M]. Reduced flicker is worth considering for photosensitive players [E].
- Text speed, auto-advance and skip hold give good coverage for reading-speed and motor needs [M].

## 4. Contrast ratios (computed)

WCAG: 4.5:1 for normal text, 3:1 for large text and UI components.

| Colour (where) | On PanelDark | On ButtonDark | Verdict |
|---|---|---|---|
| Warm (1.0, 0.86, 0.62), titles and values | 14.60 | 11.37 | pass |
| Paper (0.90, 0.88, 0.80), body | 14.62 | 11.38 | pass |
| Dialogue default speaker (0.93, 0.91, 0.82) | 15.67 | 12.21 | pass |
| Speaker Shadow (0.72, 0.66, 0.80) | 8.74 | 6.81 | pass |
| Speaker System, menu label (0.68, 0.65, 0.60) | 7.97 | 6.21 | pass |
| StoryDim (0.62, 0.60, 0.56) | 6.80 | 5.29 | pass |
| Dim, UI (0.55, 0.55, 0.58), hints and footers | 5.79 | 4.51 | pass, barely on buttons |
| Loading line (0.55, 0.53, 0.49) | 5.41 | 4.21 | fails on buttons only |
| Main-menu footer (0.45, 0.44, 0.42), 16 px | 3.91 | 3.05 | fail |
| Timer hot (0.95, 0.45, 0.32) | 6.74 | 5.25 | pass |

Button labels use Paper, which gives 7.23:1 on `ButtonWarm` (0.36, 0.25, 0.13) [M].

Colourblind palettes, text on `PanelDark` [M]:

| Slot | Standard | Blue/orange | High contrast |
|---|---|---|---|
| Good | 11.93 | 8.96 | 15.42 |
| Bad | 5.78 | 8.89 | 7.29 |
| Active | 14.60 | 12.81 | 18.87 |
| Neutral | 5.79 | 5.79 | 9.76 |
| Warning | 11.38 | 12.36 | 15.33 |

All pass on `PanelDark`. On `ButtonDark`, Standard Bad and Neutral are 4.50 and 4.51, at the limit. On `ButtonWarm`, several fall below 4.5 (Standard Bad 2.86, Neutral 2.87, Blue/orange Good 4.43 and Bad 4.40). Whether a palette colour is ever drawn on `ButtonWarm` was not traced.

Good against Bad (ratio between the two colours) [M]: Standard 2.06, **Blue/orange 1.01**, High contrast 2.12. Sky blue and orange have almost the same luminance, so that pair differs by hue only. It helps red-green colour blindness, but a player with low vision or a washed-out screen cannot tell them apart by brightness. Quiz feedback also carries words, which is a non-colour cue [E, not traced for every use].

**Recommend:** raise the main-menu footer to about (0.62, 0.60, 0.56), about 6.8:1 on `PanelDark`. In blue/orange, darken Bad to about (0.80, 0.42, 0.10) or lighten Good to about (0.70, 0.85, 1.0), and re-run the formula to confirm the gap. Pair every Good/Bad colour with a word or icon.

Not computed: text over the 3D scene with no panel behind it (interaction prompt, message strip, FPS text, `SceneBuilderUI.cs:253-265`). The background changes with lighting, so any number would be invented. Check these on device in the darkest and brightest rooms.

## 5. Arabic glyph readability

From the font files [M]:
- `T_NotoNaskhArabic-Regular` covers 140 of 141 code points in Arabic Presentation Forms-B (U+FE70 to U+FEFC), all basic letters used, Arabic-Indic digits, ASCII, and « » × ° § · … and curly quotes. This matches the project's choice (comment at `SceneBuilderUI.cs:96-98`) because the `ArabicSupport` shaper emits Presentation Forms-B.
- `T_Cairo-Regular` covers 89 of 141, which confirms why Cairo is not used for Arabic body text. `T_Inter-Regular` has none.
- Missing from Naskh: `→` and `±`. In source they appear only in code comments and Latin strings; no Arabic line was found with them. Low risk.
- All 47 distinct Arabic base letters found in `Scripts/**` strings exist in the font.
- `Localization.ArabicFont` (`Core/Localization.cs:60-79`) falls back to OS fonts, then to the English font. If the last fallback is reached, Arabic shows as empty boxes [E; depends on the build].

Diacritics: the ending spec in PR #59 writes the AR lines with tashkeel (for example `يُغلَق`, `طَقطَقة`) and in Egyptian colloquial. Marks above and below letters are the first thing to clip with `lineSpacing` 1.08 (`SceneBuilderUI.cs:966`) and tight button heights. This is a concern to check, not a measured fault [E].

Not verified, needs a native-speaker visual check:
- Arabic has no size compensation. Naskh at the same px size usually reads smaller than Inter, so Arabic body text may need about 10 to 15 percent more size [E, general typography, not measured here].
- Right alignment is set in dialogue and ending (`DialogueUIController.cs:151,159`, `EndingController.cs:137`); not every panel was checked.
- Mixed Arabic and Latin lines (key names such as "[E]").

**Recommend:** in Arabic mode, scale body text by about 1.1 and raise `lineSpacing` to about 1.2, then check on screen with the diacritic-heavy ending lines. Keep Naskh.

## Suggested order

1. Captions for spoken lines with no on-screen text, and sound-cue captions (finding 1).
2. Minimum text size and dialogue scale (findings 2 and 3).
3. Blue/orange palette separation and menu footer colour (findings 4 and 5).
4. Arabic visual pass with a native speaker, including the ending lines.
5. After #59/#60/#61 land, re-check ending subtitles against the 14-line spec.

## Not done

- No screenshots or on-device checks.
- No contrast number for text over the 3D scene or over the generated panel gradient.
- Menus and HUD only; world labels (`TextMesh`, about 26 `CreateWorldText` calls) were not measured.
