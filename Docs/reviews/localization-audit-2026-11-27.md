# Arabic/English localization coverage audit (2026-11-27)

Package 2 of `Docs/YUSUF_WORK_PLAN.md`. Report only: no code, string or asset was changed. Base: `main` at `bf14cd2`.

## Method and limits

Static review, no Unity run. A script parsed both translation tables (`Localization.cs` `Arabic`, 131 entries; `StoryStrings.cs` `Story`, 147 entries), the 85 story strings in `Data/Story/SN_*.asset` (node text, choice text, `localizationKeys`), every `Tr/TrPresent/Format` call with a literal, and the literals passed to the `SceneBuilder*` UI/world-text helpers. Hits were then checked by hand against the code that displays them. Line numbers are for the base commit.

Convention used: the English sentence is the key; a missing key falls back to English in Arabic mode. Strings that carry their own Arabic next to the English (puzzle UIs, `SetLocalized(en, ar)`, `Localize(label, "...")`, how-to-play body, ending body) count as covered.

Not covered: the Arabic wording of those inline puzzle strings and of the question bank was not reviewed line by line, and no on-device shaping/RTL layout check was done. Arabic suggestions below are mine and need a native-speaker pass before use. Wording points (section 3) are judgment calls, not facts.

## 1. What is fine

- All 85 story-node strings (text, choices) have an Arabic entry.
- No placeholder (`{0}`), newline-count or rich-text tag mismatch between English key and Arabic value in either table.
- No Latin words left in Arabic values; every value contains Arabic.
- 4 keys exist in both tables (harmless: `StoryStrings` wins, then falls back to `Localization`).

## 2. Missing or mismatched entries (English shows in Arabic mode)

| # | File:line | English as shipped | Why it misses | Suggested Arabic |
| - | --- | --- | --- | --- |
| 1 | `SceneBuilderUI.cs:236` | `HEALTH` (HUD bar label) | No key. `Health` (category) exists but keys are case-sensitive | `الصحة` |
| 2 | `SceneBuilderUI.cs:312` | `LIGHT` (mobile button) | No key | `المصباح` |
| 3 | `SceneBuilderUI.cs:314` | `RUN` (mobile button) | No key | `ركض` |
| 4 | `SceneBuilderUI.cs:316` | `CROUCH` (mobile button) | No key | `انحناء` |
| 5 | `SceneBuilderUI.cs:322` | `TASKS` (mobile button) | No key. `OBJECTIVES` is `المهام` | `المهام` |
| 6 | `SceneBuilderUI.cs:330` | `MEDKIT` (mobile button, 84 px, font 12) | No key | `إسعاف` (short, fits the button) |
| 7 | `SceneBuilderUI.cs:202` | `A game for good  ·  no combat, no gore` (loading subtitle) | Table only has the longer menu line ending `... one night, one flashlight` | `لعبة من أجل الخير  ·  بلا قتال أو دماء` |
| 8 | `SceneBuilderUI.cs:568`, `SettingsPanelController.cs:81` | `FPS counter` (settings row) | No key | `عداد الإطارات` |
| 9 | `SceneBuilderStory.cs:861`, `SettingsPanelController.cs:86` | `Touch control size` (settings row) | No key | `حجم أزرار اللمس` |
| 10 | `LoadingScreen.cs:156` | `Knowledge is the only light that follows a person into the dark.` | Table key (`Localization.cs:336`) includes the straight quotes, so lookup misses. `StoryStrings.cs:265` has a different sentence (`Go on, then. ...`) | Add the unquoted key: `المعرفة هي النور الوحيد الذي يتبع المرء في العتمة.` (or change the call to the quoted key) |
| 11 | `SettingsPanelController.cs:355` | `Close` | Table has `CLOSE`, not `Close` | Add `Close` : `إغلاق` |
| 12 | `GameManager.cs:293` | `It struck her - she is barely standing. Find a medkit.` | No key | `أصابتها الضربة - بالكاد تقف على قدميها. ابحث عن حقيبة إسعاف.` |
| 13 | `MedkitPickup.cs:14` | `Take the medkit` (prompt) | No key | `خذ حقيبة الإسعاف` |
| 14 | `MedkitPickup.cs:25` | `Medkit packed. Press H when she needs it.` | No key | `حُفظت حقيبة الإسعاف. اضغط H حين تحتاجها.` |
| 15 | `PlayerHealth.cs:83`, `TouchButton.cs:132` | `Medkit used. Back on her feet.` | No key | `استُخدمت حقيبة الإسعاف. عادت إلى قدميها.` |
| 16 | `PlayerHealth.cs:87` | `No medkit. They are rare - some puzzles hide one.` | No key | `لا توجد حقيبة إسعاف. هي نادرة - بعض الألغاز تخبئ واحدة.` |
| 17 | `LitZone.cs:19` (default chalk line) | `The light holds. This feels safe.` | No key (only used when a zone passes no line) | `النور صامد. هنا أمان.` |
| 18 | `SceneBuilderScenes.cs:666` | `White sheets, cold instruments. She patched a hundred scraped knees here.` (infirmary checkpoint line) | No key | `أغطية بيضاء وأدوات باردة. هنا ضمّدت مئة ركبة مجروحة.` |
| 19 | `SceneBuilderScenes.cs:543` | `INFIRMARY` (world sign) | No key | `العيادة` |
| 20 | `SceneBuilderScenes.cs:724` | `"Put me back together."` (memory card, world text) | Builder passes the string with straight quotes; table key `Put me back together.` has none | Existing `أعدني كما كنت.` once the key matches (add quoted key, or drop the quotes in the builder) |
| 21 | `GameSettings.cs:497` | `Low (auto)` etc. suffix `(auto)` appended in English | Raw concatenation, not translated | `(تلقائي)` (key `Auto` is `تلقائي`) |
| 22 | `SettingsPanelController.cs:421` | `40 m · tier` suffix `· tier` | Raw concatenation | `· حسب المستوى` |
| 23 | `Localization.cs:324` (`Very High`) | key exists but no code path found that uses it | Likely unused | none, confirm before deleting |

### Names that go into translated templates stay English

`Localization.Format("Collect {0}", fragmentName)` (`KnowledgeFragment.cs:26`), `Format("Hide in {0}", displayName)` and `Format("Leave {0}", ...)` (`HideSpot.cs:44-45`) translate only the template. The name argument is passed raw, so the Arabic prompt becomes `اجمع the pressed-flower bookmark` or `اختبئ في the locker`. `ItemPickup` and `PuzzleBase` already avoid this with `displayNameAr`; `KnowledgeFragment` and `HideSpot` have no Arabic field. Fix is mechanical: add an Arabic field (or translate the argument with `Tr`) and fill:

| File:line | English name | Suggested Arabic |
| --- | --- | --- |
| `SceneBuilderScenes.cs:728` | the pressed-flower bookmark | فاصل الكتاب بالزهرة المضغوطة |
| `SceneBuilderScenes.cs:767` | the note in the optics cabinet | الملاحظة في خزانة البصريات |
| `SceneBuilderScenes.cs:800` | the marked exam paper | ورقة الامتحان المصحَّحة |
| `SceneBuilderScenes.cs:828` | the pressed jasmine and photograph | الياسمين المضغوط والصورة |
| `SceneBuilderScenes.cs:849` | her mug and the note for a colleague | كوبها والورقة الموجهة لزميلة |
| `SceneBuilderScenes.cs:873` | the hand-drawn health chart | مخطط الصحة المرسوم بخط اليد |
| `SceneBuilderScenes.cs:901` | the poetry-evening programme | برنامج أمسية الشعر |
| `HideSpot.cs:19` | the locker | الخزانة |
| `SceneBuilderScenes.cs:1016,1019` | the corridor locker | خزانة الممر |
| `SceneBuilderScenes.cs:1022` | the teacher's desk | مكتب المدرّسة |
| `SceneBuilderScenes.cs:1025` | the staff-room desk | مكتب غرفة المعلمين |
| `SceneBuilderScenes.cs:1028,1031` | the stage curtain | ستارة المسرح |

### Intentionally English (not gaps)

Map pin labels `EGYPT 4` ... `MOROCCO 9` (`SceneBuilderPuzzles.cs:186`): documented in the builder as diegetic map text. Mobile buttons `E` and `II` are symbols. `Mouse X/Y` are Input axis names.

## 3. Arabic wording that looks wrong or inconsistent (judgment calls)

| Where | Current Arabic | Issue | Suggested |
| --- | --- | --- | --- |
| `Localization.cs:259,264,270,397`, `StoryStrings.cs:233,260` | `مفتاح البوابة` / `المفتاح جاهز` / `المفتاح بارد` | "crank" rendered as `مفتاح` (key/switch). The game's crank is a hand lever | `ذراع البوابة` / `(الذراع جاهزة)` / `الذراع باردة ومتشوقة. قبل أن أديرها` |
| `StoryStrings.cs:162` | `اللافاة تنتصر` | `اللافاة` is not a word for the crank | `ذراع الشحن تنتصر. يبقى النور مشتعلًا.` |
| `Localization.cs:391` | `... إلى الوطن` | "home" is `الوطن` (homeland) | `... إلى البيت` |
| `Localization.cs:341` | `يتحول السماء` | `السماء` is feminine. `StoryStrings.cs:124` already has `تتحول` | `تتحول السماء` |
| `Localization.cs:373` | `ما زال الطباشير يفوح منه رائحة الياسمين` | ungrammatical | `ما زالت رائحة الياسمين تفوح من الطباشير` |
| `Localization.cs:243` | `تخفت الشعاع` | `تخفت` is intransitive | `تُخفِت الشعاع` |
| `StoryStrings.cs:97` | `مدة التخطيط المطول` | `التخطيط` means planning. Skip hold time | `مدة الضغط للتخطي` |
| `StoryStrings.cs:72` | `لا حفظيات بعد` | invented plural | `لا توجد عمليات حفظ بعد` |
| `StoryStrings.cs:163` | `هكذا كانت تشمّ صفها` | says she smelled the class; English says the class smelled like this | `هكذا كانت رائحة صفها - طباشير وياسمين وداخل كتاب` |
| `StoryStrings.cs:180` | `صنّفت ستين ورقة` | `صنّف` = classify. English: marked | `صحّحت ستين ورقة` |
| `StoryStrings.cs:211,214,223` | `دفّة أسماء` / `تمرّ الدفّة` | roll call; `StoryStrings.cs:254` already uses `نداء الأسماء` | `نداء الأسماء` throughout |
| `StoryStrings.cs:221` | `حتى يصير بسمة` | `بسمة` = smile. English: smudge | `حتى يصير لطخة` |
| `StoryStrings.cs:238` | `استعملي القدمان` | after the verb it takes the accusative | `استعملي القدمين` |
| `StoryStrings.cs:241` | `كما تحبس فصلٌ أنفاسه` | `فصل` is masculine | `كما يحبس فصلٌ أنفاسه` |
| `StoryStrings.cs:247` | `كما تنزاح الأثاث` | `الأثاث` is masculine | `كما ينزاح الأثاث` |
| `StoryStrings.cs:249` | `تصفرّ النوافذ رمادية` | `تصفرّ` = turns yellow. English: turning grey | `تميل النوافذ إلى الرمادي` |
| `StoryStrings.cs:256` | `أسرع يا حبيبتي` | addressed to a girl | `أسرعي يا حبيبتي` |
| `StoryStrings.cs:147-160,213-216` | mix of feminine imperative (`اهدئي`, `عُدّي`, `أبقي`) and first person / masculine (`أرجع`, `أنظر`, `أطفئ`, `أجبه`) for English imperatives | inconsistent voice inside the same node set. Nour is female | pick one (feminine imperative suggested): `ارجعي`, `انظري`, `أطفئي`, `أجيبيه` |
| HUD and how-to-play (`Localization.cs:247,267,401` etc.) | masculine imperative (`أعد`, `اهرب`, `اجمع`) while the story uses feminine for Nour | gender of address differs between UI and story | decision for the owner: generic masculine UI is common; just keep it deliberate |

## 4. Suggested fix order

1. Items 1-11 and 19-20 in section 2 (visible on every Arabic run, one-line key additions).
2. Fragment and hide-spot names (mixed-language prompts).
3. Medkit and health messages (items 12-18): only appear in the health redesign flow, which is in flight on your side; add keys with that work.
4. Wording fixes in section 3, after a native review.

All fixes live under `Assets/_Project/Scripts/**` (your lane), so none were applied here.
