# Arabic review of shipped strings (package 1, report only)

Date: 2026-11-27. Author: AI assistant working for Abdelraouf Zidan (GitHub `Abdo1590`).

## Read this first (limits, stated honestly)

- **This report was written by Abdelraouf's AI assistant. Abdelraouf has not read it and has not signed off on it.** It is not a native-speaker review yet. Treat every row as a proposal until a native Arabic speaker confirms it.
- Abdelraouf is a native Arabic speaker and asked his assistant to do this work itself. He may review or correct it later in PR comments. Nothing here claims his review.
- Confidence labels used in every row:
  - **High**: a spelling, grammar or meaning error I am confident about. A native check should still confirm it.
  - **Medium**: wording that is probably wrong or unnatural. Likely right, but a native speaker may choose differently.
  - **Owner choice**: a style or consistency decision, not an error.
- **Verified facts**: line numbers, current text, and which English string each Arabic line pairs with. I read these from `main` on 2026-11-27 (commit `f0e2cb5`). Everything labelled "correction" or "issue" is my estimate.
- **Not verified**: how any string looks on screen (shaping, diacritics, bidi order of numbers and `%`). I did not run the game. No device or Unity build was used.
- Line numbers refer to the line that holds the Arabic text (for multi-line entries, the line with the Arabic). The earlier audit `Docs/reviews/localization-audit-2026-11-27.md` section 3 cites some lines one off (for example `Localization.cs:341` is really `342`, `StoryStrings.cs:233` is really `234`, `Localization.cs:386` is really `388`). Use the lines below.
- Story nodes: `Data/Story/SN_*.asset` contain English only. I found no Arabic in them. All story Arabic comes from `StoryStrings.cs` (checked: no Arabic script in any `SN_*.asset`). The plan's mention of "AR fields in SN assets" does not match the repo.
- Dialect: all shipped Arabic is Modern Standard Arabic (MSA). That is a sound choice for a school game, with a few colloquial slips listed below (`رجليك`, `نطّات`). I did not propose any Egyptian-dialect rewrite.
- Scope: report only. No strings, assets or code were changed.

## A. Confirmed from the earlier audit (section 3), with corrected line numbers

I agree with each of these. Rows here only add the verified line and a confidence label. The earlier audit's suggested text is fine unless I say otherwise.

| # | File:line | Current Arabic | Issue | Correction | Confidence |
| - | --- | --- | --- | --- | --- |
| A1 | Localization.cs:244 | `الإجابات الخاطئة وانتهاء الوقت تخفت الشعاع` | `تخفت` is intransitive (it dims). The sentence needs a transitive verb | `تُخفِت الشعاع` (or `تُضعف الشعاع`) | High |
| A2 | Localization.cs:259, 264, 271, 398; StoryStrings.cs:234, 261 | `المفتاح جاهز`, `مفتاح البوابة ميت`, `مفتاح البوابة صار حيًا`, `المفتاح بارد ومتشوق` | The English word is "crank" (hand lever). `مفتاح` means key or switch. A child will think of a key | `ذراع البوابة` / `الذراع جاهزة` / `الذراع باردة` (I checked that these four lines all mean the same object) | Medium |
| A3 | StoryStrings.cs:162 | `اللافاة تنتصر` | `اللافاة` is not an Arabic word for a crank | `ذراع الشحن تنتصر` | High |
| A4 | Localization.cs:392 | `واحمل ذكرياتها معك إلى الوطن` | `الوطن` is "homeland". English means home | `إلى البيت` | High |
| A5 | Localization.cs:342 | `يتحول السماء` | `السماء` is feminine | `تتحول السماء` (StoryStrings.cs:125 already has `تتحول`) | High |
| A6 | Localization.cs:374 | `ما زال الطباشير يفوح منه رائحة الياسمين` | Ungrammatical (the verb `يفوح` takes the scent as subject) | `ما زالت رائحة الياسمين تفوح من الطباشير` | High |
| A7 | StoryStrings.cs:97 | `مدة التخطيط المطول` | `التخطيط` means planning. English: skip hold time | `مدة الضغط للتخطي` | High |
| A8 | StoryStrings.cs:72 | `لا حفظيات بعد` | Invented plural | `لا توجد عمليات حفظ بعد` | High |
| A9 | StoryStrings.cs:164 | `هكذا كانت تشمّ صفها - طباشير وياسمين ودفء الكتب` | Says she smelled the class. English: the class smelled like this. Also `دفء الكتب` ("warmth of books") is not "the inside of a book" | `هكذا كانت رائحة صفها - طباشير وياسمين وداخل كتاب` | High |
| A10 | StoryStrings.cs:181 | `صنّفت ستين ورقة` | `صنّف` = classify. English: marked | `صحّحت ستين ورقة` | High |
| A11 | StoryStrings.cs:212, 214 | `ودفّة أسماء`, `تمرّ الدفّة` | `دفّة` = rudder or book cover. English: roll call. Line 255 already says `كنداء الأسماء` | `ونداء أسماء في فمه` / `يمرّ النداء` | High |
| A12 | StoryStrings.cs:222 | `حتى يصير بسمة` | `بسمة` = smile. English: smudge | `حتى يصير لطخة` | High |
| A13 | StoryStrings.cs:238 | `استعملي القدمان` | Object of the verb takes the accusative | `استعملي القدمين` | High |
| A14 | StoryStrings.cs:242 | `كما تحبس فصلٌ أنفاسه` | `فصل` is masculine, and the pronoun `أنفاسه` is already masculine | `كما يحبس فصلٌ أنفاسه` | High |
| A15 | StoryStrings.cs:248 | `كما تنزاح الأثاث` | `الأثاث` is masculine | `كما ينزاح الأثاث` | High |
| A16 | StoryStrings.cs:250 | `تصفرّ النوافذ رمادية` | `تصفرّ` = turns yellow. English: turning grey | `يميل لون النوافذ إلى الرمادي` | High |
| A17 | StoryStrings.cs:257 | `أسرع يا حبيبتي` | Addressed to a girl (Nour) | `أسرعي يا حبيبتي` | High |
| A18 | StoryStrings.cs:157, 159, 160, 213 and others | Mixed masculine and feminine imperatives for Nour (`اهدئي` vs `أرجع`, `أنظر`, `أطفئ`, `أجبه`) | Voice is inconsistent inside the same node set. Some choices are first person ("I") in English and some are commands | Pick one. If feminine imperative: `ارجعي`, `انظري`, `أطفئي`, `أجيبيه` | Owner choice |
| A19 | Localization.cs:247-271, 401 etc. | Masculine imperatives in UI (`أعد`, `اهرب`, `اجمع`) | Generic masculine for UI is normal MSA. Just make it deliberate. The story addresses Nour as female | No change needed unless the owner wants feminine UI | Owner choice |

## B. New findings (not in the earlier audit)

### B1. Errors I am confident about

| # | File:line | Current Arabic | Issue | Correction | Confidence |
| - | --- | --- | --- | --- | --- |
| B1.1 | StoryStrings.cs:152 | `لتعرف أن يديّ خاويستان` | Typo in the dual of `خاوية` | `خاويتان` | High |
| B1.2 | StoryStrings.cs:157 | `أبقي في النور وهو يطن` | `أبقي` is "keep (something)". To mean "stay" the imperative is `ابقي` (connecting hamza) | `ابقي في النور وهو يطن` (the plain `أبقي` at StoryStrings.cs:232 and 262, "keep the light on", is correct) | High |
| B1.3 | StoryStrings.cs:125 | `يشاهد النور يغادر دون أن يحمله` | Meaning is wrong. English: watches the light walk away *without her* (the Shadow is left behind). Arabic says "without carrying it" | `يشاهد النور يغادر دونها` | High |
| B1.4 | StoryStrings.cs:125 | `الليلة رافق نورًا كل الطريق إلى الخارج` | `نورًا` is read as "a light" (common noun), not the name Nour. The pun in English ("followed Nour") is lost and the sentence reads oddly | `الليلة تبعها النور حتى خرجت إلى الصباح` (drops the name pun) | Medium |
| B1.5 | StoryStrings.cs:176 | `يُعاد يوم الخميس` | "Return by Thursday" is a deadline. Arabic reads "is returned on Thursday" | `يُعاد قبل يوم الخميس` | Medium |
| B1.7 | StoryStrings.cs:193 | `تفقد ملصقةٌ كلَّ ما عليها` | `ملصقة` is not the usual word. Poster = `ملصق` (masculine) | `يفقد ملصقٌ كلَّ ما عليه` | High |
| B1.8 | StoryStrings.cs:195 | `جناح العلوم صحي` | `صحي` reads "healthy / health-related". English: awake | `استيقظ جناح العلوم` | High |
| B1.9 | StoryStrings.cs:212 | `أسماء، أغلبها بالي حتى تعذّر قراءتها. اسمًا واحدًا حفظه` | `تعذّر قراءتها` should agree with the feminine subject. `اسمًا` is accusative with no verb governing it | `حتى تعذّرت قراءتها. اسمٌ واحد حفظه أكثر من اللازم: اسمي` | High |
| B1.10 | StoryStrings.cs:236 | `توقف عن تظاهره بأنه دخان` | Wrong form after `عن` | `توقف عن التظاهر بأنه دخان` | High |
| B1.11 | StoryStrings.cs:146 | `تُصفق البوابة خلفي بعنف` | Vowel mark makes it passive and the verb for a slamming door is `صفق` / `يصفق` active. English: the gate slams | `تصفق البوابة خلفي بعنف` (or `تُغلَق البوابة خلفي بعنف`) | Medium |
| B1.12 | Localization.cs:404 | `على نور أن تحمل ما هو عزيز قبل الفجر` | Reads fine, slightly stiff. English: carry what is precious | `ما هو عزيز عليها` is smoother | Owner choice |
| B1.13 | StoryStrings.cs:125 | list `فاصل الكتاب، ورقة الامتحان، الياسمين، الورقة المثبّتة، البرنامج، الكوب` | English list is bookmark, paper, jasmine, note, programme, mug. `الورقة المثبّتة` ("the pinned sheet") is an odd rendering of "note" | `الملاحظة` | Medium |

### B2. Wording that a child could misread, or a word that is the wrong register

| # | File:line | Current Arabic | Issue | Correction | Confidence |
| - | --- | --- | --- | --- | --- |
| B2.1 | Localization.cs:313 | `مقياس الرسم` (Render scale) | `مقياس الرسم` is the standard term for map scale. In graphics settings it confuses | `دقة العرض` | Medium |
| B2.2 | Localization.cs:310 | `تزامن الإطارات` (VSync) | Literal. The usual Arabic term is `المزامنة الرأسية` | `المزامنة الرأسية` | Medium |
| B2.3 | Localization.cs:314 | `أصلي` (Native, resolution) | Reads as "original / genuine" | `حسب الجهاز` or `الدقة الأصلية` | Medium |
| B2.4 | Localization.cs:308 | `نافذة` (Windowed) | A bare noun, next to `ملء الشاشة` | `وضع النافذة` | Owner choice |
| B2.5 | Localization.cs:319 | `متوقف` (Off) | `متوقف` = stopped. For a switch, `معطّل` or `إيقاف` is normal. (`مفعّل` for On is fine) | `معطّل` | Medium |
| B2.6 | Localization.cs:267 | `مات ضوء المصباح` | `مات` is blunt and colloquial. English: the crank light died | `انطفأ ضوء المصباح` | Medium |
| B2.7 | Localization.cs:271, 394 | `ميت` / `ميتان` (dead fuse box, dead crank) | Same register issue. A dead device is `معطّل` | `معطّل` / `معطّلان` | Medium |
| B2.8 | Localization.cs:268 | `تم الوصول إلى نقطة الحفظ` | `تم` + noun passive is discouraged and sounds like a form. Short HUD toast | `وصلتَ إلى نقطة الحفظ` or `تم حفظ تقدمك` | Owner choice |
| B2.9 | Localization.cs:301-303 vs StoryStrings.cs:67 | `الفصل الأول/الثاني/الثالث` (Act I-III) and `الفصل {0}` (CHAPTER {0}) | Same Arabic word for Act and Chapter. Players cannot tell them apart | Keep `الفصل` for Act, use `الجزء {0}` or `المرحلة {0}` for Chapter (owner to pick) | Owner choice |
| B2.10 | Localization.cs:248, 254-255, 260, 265, 271, 275, 394; StoryStrings.cs:154, 174, 185, 195, 228 | `صناديق مصاهر` (fuse boxes) | `مصهر` is also "smelter / furnace", and a child may not know it for "fuse". The term is also inconsistent: StoryStrings.cs:185 says `الصندوق الثاني`, StoryStrings.cs:228 says `المصهر الثالث` | Pick one: `صندوق الفيوزات` is what players say in Egypt and is easiest to read. `صندوق المصهرات` is the formal choice. Use it everywhere | Owner choice |
| B2.11 | Localization.cs:248, 256-257, 400; StoryStrings.cs:68, 174 | `شذرات` (Knowledge Fragments) | `شذرات` is a literary word (bits, nuggets). Understandable but formal | `قطع المعرفة` or `أجزاء المعرفة` | Owner choice |
| B2.12 | Localization.cs:388 vs StoryStrings.cs:232, 262 | `أبقِ النور مشتعلًا لمن يأتون بعدك` and `لمن يأتون من بعد` | Same quoted line from the mother in two different wordings | Use one wording in all three places (the `بعدك` version is closer to a direct quote) | Owner choice |
| B2.13 | StoryStrings.cs:92 | `لوحة آمنة لعمى الألوان` | `آمنة` (safe) is odd here | `لوحة ألوان مناسبة لعمى الألوان` | Medium |
| B2.14 | Localization.cs:378 | `ضغطتها الأسبوع الذي بدأت فيه المدرسة` | `ضغط` is "pressed" but for a flower between pages the usual word is `جفّفتها` or `كبستها` | `جفّفتها بين الصفحات في الأسبوع الذي بدأت فيه الدراسة` | Medium |
| B2.15 | StoryStrings.cs:215 | `أرتل سطر درسها` | `رتّل` is used for reciting scripture | `أردد سطر درسها` | Medium |
| B2.16 | StoryStrings.cs:253 | `قرّري برجليك` | `رجليك` is colloquial, while line 238 uses MSA `القدمين` | `قرّري بقدميكِ` | Medium |
| B2.17 | StoryStrings.cs:259 | `يأخذني الدرج الخلفي عبر العتمة نطّات` and `باب بمزلاج ضغط` | `نطّات` is colloquial (MSA: `قفزات`). `مزلاج` = latch or bolt. English: push-bar door | `قفزات` / `باب بقضيب دفع` | Medium |
| B2.18 | StoryStrings.cs:261 | `قبل أن ألويه`, `بارد ومتشوق` | `ألوي` = twist, not turn. `متشوق` is a weak match for "eager" | `قبل أن أديرها`, `باردة ومتلهفة` | Medium |
| B2.19 | StoryStrings.cs:135 | `خرجت بقبضة محدودة` | `محدودة` = limited. English: a handful | `خرجت بحفنة` | Medium |
| B2.20 | StoryStrings.cs:135 | `الصناديق الخفيفة سهلة الجري بها`, `تتذكر نصف تذكر` | Both read awkwardly | `الجري بالصناديق الخفيفة سهل، وهنا المشكلة` / `تتذكر بشكل ناقص أن هناك شيئًا قصدت إحضاره` | Medium |
| B2.21 | StoryStrings.cs:140 | `السماء تقوم بعمل الصباح`, `سيارات الفرق`, `لا كيدٍ بل كخفوت` | `سيارات` is vague (vans: `شاحنات`). `بل كخفوت` is unclear for "hush". `تقوم بعمل الصباح` is a literal rendering | `شاحنات الفرق`, `بل كهمسٍ خافت`, `السماء تؤدي عملها الصباحي` | Medium |
| B2.22 | StoryStrings.cs:130 | `قريبًا من الصدر` | English is "close, and careful", not "close to the chest" | `قريبًا منها، وبحذر` | Owner choice |
| B2.23 | StoryStrings.cs:71 | `الكتابة فوق هذه الخانة؟` | Short and ambiguous as a prompt | `هل تريد استبدال محتوى هذه الخانة؟` | Owner choice |
| B2.24 | Localization.cs:236 | `لعبة من أجل الخير` ("A game for good") | Stiff for a title line | `لعبة هدفها الخير` | Owner choice |

### B3. Technical notes for the owner (not language errors, estimates only)

- Localization.cs:287 `نَفَس` carries diacritics. The code shapes text with `ArabicSupport.Fix`. Some legacy shapers draw diacritics detached. Worth checking on a device.
- Localization.cs:209 shows seconds as `ث` after a number. It is a common abbreviation, fine, but numbers in Arabic text may flip order under the shaper (for example `{0}/{1}` and `({2}%)` at Localization.cs:331). I did not run it, so this is a check to make, not a finding.
- Numerals: all digits are Western (0-9). That is common in Egypt. Just confirm it is a deliberate choice.
- Localization.cs:358 and 364 (`أعدني كما كنت`) are consistent with each other. The quoted-key mismatch (audit section 2, item 20) is a code issue, not language.

## C. The suggested Arabic in the earlier audit, section 2

Reviewed the Arabic column of `Docs/reviews/localization-audit-2026-11-27.md` section 2 (the English column is theirs, not reviewed). Line numbers in that table refer to files outside my package, so I did not re-check them.

| Audit item | Suggested Arabic | My view | Confidence |
| - | --- | --- | --- |
| 1 (HEALTH) | `الصحة` | OK | High |
| 2 (LIGHT, mobile button) | `المصباح` | OK | High |
| 3 (RUN) | `ركض` | OK | High |
| 4 (CROUCH) | `انحناء` | Acceptable. `انحناء` = bending over. For crouching, `جلوس` or `انخفاض` is closer | Medium |
| 5 (TASKS) | `المهام` | OK | High |
| 6 (MEDKIT) | `إسعاف` | OK | High |
| 7 (subtitle) | `لعبة من أجل الخير  ·  بلا قتال أو دماء` | OK. See B2.24 | High |
| 8 (FPS counter) | `عداد الإطارات` | OK | High |
| 9 (Touch control size) | `حجم أزرار اللمس` | OK | High |
| 10, 11, 20 (key fixes) | existing strings | OK | High |
| 12 | `أصابتها الضربة - بالكاد تقف على قدميها. ابحث عن حقيبة إسعاف.` | OK. The English is third person ("her"), the second sentence is a command. Same mix as the English | High |
| 13-16 | `خذ حقيبة الإسعاف`, `حُفظت حقيبة الإسعاف...`, `استُخدمت...`, `لا توجد...` | OK. Note `حقيبة الإسعاف` here vs `إسعاف` on the button (item 6). Pick one | High |
| 17 | `النور صامد. هنا أمان.` | Acceptable. `النور صامد` reads as "the light is resilient". `النور ثابت` is closer to "holds" | Medium |
| 18 | `أغطية بيضاء وأدوات باردة. هنا ضمّدت مئة ركبة مجروحة.` | OK. `ضمّدت` is correct. `أغطية` could be `ملاءات` | High |
| 19 | `العيادة` | OK (school infirmary) | High |
| 21 | `(تلقائي)` | OK | High |
| 22 | `· حسب المستوى` | OK | Medium |
| Name table | `فاصل الكتاب بالزهرة المضغوطة`, `الملاحظة في خزانة البصريات`, `ورقة الامتحان المصحَّحة`, `الياسمين المضغوط والصورة`, `كوبها والورقة الموجهة لزميلة`, `مخطط الصحة المرسوم بخط اليد`, `برنامج أمسية الشعر`, `الخزانة`, `خزانة الممر`, `مكتب المدرّسة`, `مكتب غرفة المعلمين`, `ستارة المسرح` | All fine. One point: the templates are `اجمع {0}` and `اختبئ في {0}`, so the names must be in the form that follows the verb. All of these work. `مكتب المدرّسة` could be `مكتب المعلمة` (more natural in Egypt) | High |

## D. The 12 draft questions (Arabic only, facts not reviewed)

Files: `Assets/_Project/Data/Questions/Q_Sci_023, Q_Sci_024, Q_Mat_021, Q_Mat_022, Q_His_023, Q_His_024, Q_Geo_019, Q_Geo_020, Q_Lan_023, Q_Lan_024, Q_Hea_017, Q_Hea_018`. (The plan says "Q_Geo_019+"; the batch is 12 files across six categories.) They stay `NEEDS-FACT-CHECK`. I did not fact-check anything and I did not change `factSource`.

| # | File:line | Current Arabic | Issue | Correction | Confidence |
| - | --- | --- | --- | --- | --- |
| D1 | Q_Lan_023.asset:26-31 | `أي من هذه الكلمات اسم؟` with options `بسرعة / طاولة / سعيد / يركض` | **Trap in translation.** In Arabic grammar `اسم` covers nouns, adjectives and verbal nouns. `سعيد` is an `اسم` too, and `بسرعة` is a noun in the genitive. A student who knows Arabic grammar could argue two answers. Translated from English "noun", so it tests English grammar in Arabic | Rework the Arabic question to match the Arabic category, for example ask for the word that names a thing, or replace the options with clear nouns versus clear verbs and particles (`طاولة / يركض / في / هل`) | High |
| D2 | Q_His_024.asset:32 | `وكان العلماء يقرأون اليونانية` | Spelling: the hamza sits on a waw | `يقرؤون` | High |
| D3 | Q_Mat_022.asset:26-32 | `سم مربع` | Understandable. Textbooks often write `سم²` or `سنتيمتر مربع` | `سم²` (owner choice) | Owner choice |
| D4 | Q_Hea_017.asset:26 | `كم من الوقت يُنصح بتنظيف الأسنان بالفرشاة في كل مرة، وفق النصائح الشائعة لأطباء الأسنان؟` | Long for a timed question | `كم مدة تفريش الأسنان الموصى بها في كل مرة؟` | Owner choice |
| D5 | the remaining questions (Sci_023, Sci_024, Mat_021, Geo_019, Geo_020, His_023, Lan_024, Hea_018) | | No Arabic problem found on my read | none | Medium |

## E. What I did not review

- Any Arabic outside the two files and the 12 question assets above. The `Q_*.asset` questions shipped before this batch were out of scope for this package.
- The visual result (fonts, shaping, layout) on any device.
- Facts inside the questions.
- English text.

## F. Suggested next step for the owner

Most High rows are mechanical one-line edits in `Localization.cs` and `StoryStrings.cs`. The Owner-choice rows (voice of imperatives, `fuse` and `fragment` terms, Act vs Chapter) need a decision before the fix PR. A native Arabic speaker (Abdelraouf, or anyone he asks) should read sections A, B1 and D1 first.
