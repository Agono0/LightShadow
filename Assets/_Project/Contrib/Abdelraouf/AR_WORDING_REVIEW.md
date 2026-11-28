# Arabic wording review: StoryStrings volume rows + 8 fact-check entries

> **AI-drafted wording input - preliminary - NATIVE REVIEW REQUIRED - NOT
> teacher-verified - not a release approval.** The reviewer is not a native
> Arabic expert and makes no native-expert claim. Every proposal below needs a
> native speaker's pick before any string changes. This file proposes wording
> only; it changes no production, factSource or translation strings.

Date: 2026-11-28. Base: PR129 branch `feat/audit-fixes` tip
`f16605faab607d310e3b688bfed6b49245017308`.

## A. New StoryStrings settings rows (commit 2de18cb)

`Assets/_Project/Scripts/Story/StoryStrings.cs` lines 111-112, both already
marked "AI-drafted, not native-reviewed" in the source:

| English key | Current Arabic | AI proposal | Notes |
| --- | --- | --- | --- |
| Voice volume | صوت الراوي | Keep صوت الراوي as primary; alternative صوت السرد | The `VoiceVol` mixer group carries more than narration: breathing SFX and the roll-call voice route through it per `Docs/AUDIO_CUES.md`. The dialogue speaker names are نور / الأم / الظل / ذاكرة, so no "راوي" speaker exists on screen. Sibling rows drop the word "volume" (الموسيقى، المؤثرات، الصوت المحيط), so a house-style alternative is السرد alone. Native pick required. |
| Shadow volume | صوت الظل | Keep صوت الظل | Consistent with THE SHADOW → الظل (speaker tag, same file). Sibling-consistent alternative: الظل alone. |

Both rows are currently UNRESOLVED pending native review; the file's own
comments already say so.

## B. The 8 explanation-PARTIAL fact-check entries

These are the 8 rows whose **explanation** verdict is PARTIAL in
`quiz_factcheck.csv` (Q_Geo_017 is option-PARTIAL and outside this set). For
each: the current `explanationAr`, wording notes, and an AI-drafted proposal.
Where the underlying English fact is still in dispute, the Arabic wording is
marked **HOLD** - final wording must wait for the settled English, and nothing
below settles the fact question.

| ID | Current explanationAr | Wording notes (AI) | Proposal (AI, needs native review) |
| --- | --- | --- | --- |
| Q_Geo_001 | المحيط الهادي تغطي مساحته أكثر من مساحة جميع المحيطات الأخرى مجتمعة. | Spelling: المحيط الهادي should be المحيط الهادئ (hamza on هادئ). Word order is also convoluted (subject, then a fronted verb with a pronoun back-reference). | تغطي مساحة المحيط الهادئ أكثر من مساحة جميع المحيطات الأخرى مجتمعة. **HOLD** for the fact question: the cited NOAA page states a different comparison (larger than all continents' landmass), so the English may change. |
| Q_Geo_011 | المحيط المتجمد الشمالي هو الأصغر والأبرد، ويحيط بالقطب الشمالي. | Acceptable rendering. Note only: the "coldest" (الأبرد) detail is not covered by the cited NOAA page; English may be trimmed. | Keep; **HOLD** on الأبرد pending the English decision. |
| Q_Geo_012 | يصب الأمازون نحو خُمس مياه أنهار الأرض كلها في البحر. | خُمس correctly mirrors "a fifth". The English figure itself is under source dispute (USGS ~15% vs Britannica ~one-fifth; see QUIZ_SOURCE_ADDENDUM.md). | Keep structure; **HOLD** the numeral until the English figure settles (a USGS-aligned wording would become نحو ١٥٪ instead of خُمس). |
| Q_Geo_016 | يعزف السكان الأصليون في أستراليا الديجيريدو منذ أكثر من ألف عام. | "أكثر من" = "more than", but the English says "at least" (ما لا يقل عن). The age claim itself is not covered by the cited Britannica page. | يعزف السكان الأصليون في أستراليا الديجيريدو منذ ما لا يقل عن ألف عام. **HOLD** pending the fact decision on the age claim. |
| Q_Hea_003 | الصابون يرفع الدهون والأوساخ التي تحتمي بها الجراثيم، فتُغسل مع الماء. | "تحتمي بها الجراثيم" flips the English agency: the English says the oils and dirt TRAP the germs, not that the germs shelter in them. "يرفع" is a literal calque of "lifts". | الصابون يزيل الدهون والأوساخ التي تعلق بها الجراثيم فتُشطف مع الماء. |
| Q_Hea_006 | ثمار الحوامض كالبرتقال غنية بفيتامين ج الذي يدعم جهاز المناعة ويساعد الجسم على امتصاص الحديد. | Acceptable; one long sentence carries three claims. Note: "يدعم جهاز المناعة" is the detail the cited MedlinePlus page does not cover. | Split: ثمار الحوامض كالبرتقال غنية بفيتامين ج. وهو يدعم جهاز المناعة ويساعد الجسم على امتصاص الحديد. **HOLD** the immune-support clause pending the fact decision. |
| Q_Sci_007 | المشتري هو أكبر الكواكب؛ يتسع في داخله أكثر من 1,300 أرض. | The numeral "1,300" uses Latin digits with a comma; in Arabic text a comma reads as the DECIMAL separator (1,300 → 1.3). The figure itself is under source dispute (NASA pages say both 1,000 and "more than 1,300"; see QUIZ_SOURCE_ADDENDUM.md). | Write the numeral without a separator (1300) or in Arabic-Indic digits (١٣٠٠). **HOLD** the figure until the English settles. |
| Q_Sci_008 | جاذبية الأرض تجذب القمر نحوها، وحركته الجانبية تمنعه من السقوط في داخلها. | "السقوط في داخلها" is awkward for "falling in"; "حركته الجانبية" is understandable but "sideways motion" is really orbital tangential motion. Note: the cited Britannica gravity page does not cover the sideways-motion clause. | جاذبية الأرض تجذب القمر نحوها، وحركته المدارية الجانبية تمنعه من السقوط فيها. **HOLD** the second clause pending the fact decision. |

## Method and limits

- Wording judgments are AI-drafted against Modern Standard Arabic conventions;
  none of them is a native-speaker judgment.
- Factual claims are explicitly out of scope here: HOLD rows wait on the
  fact-check resolution (see `QUIZ_FACTCHECK_REPORT.md` and
  `QUIZ_SOURCE_ADDENDUM.md`), and this file does not settle any of them.
- No string, asset, factSource or translation file was edited.
