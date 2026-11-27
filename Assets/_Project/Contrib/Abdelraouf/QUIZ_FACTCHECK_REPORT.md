# Quiz Fact-Check Report (Input for LightShadow Localization Audit)

**Status: AI-drafted research input, preliminary. Native and expert review needed. NOT teacher-verified. This is not a release approval.**

Prepared by an AI assistant as input to the LightShadow audit workstream. Every verdict below is a claim to be reviewed, not a final editorial decision.

## Scope

- 132 quiz questions, all currently marked `factSource: NEEDS-FACT-CHECK` in `Assets/_Project/Data/Questions/Q_*.asset` (per audit v3 section 7.1: 132 of 133 questions carry this marker).
- Geography 20, Health 18, History 24, Language 24, Math 22, Science 24 = 132.
- Checked the marked correct answer (option verdict) and the shown explanation text (explanation verdict) separately, because an explanation can claim more than its answer.

## Method

1. Each question was checked against public reference sources (Britannica, Merriam-Webster, NASA, NOAA, CDC, WHO, NIDDK, NHLBI, NIH ODS, MedlinePlus, ADA, USGS, National Geographic Education, AMNH, British Museum, OpenStax, The Physics Classroom, Purdue OWL, Cambridge Dictionary, Grammarly blog, mathsisfun.com, LibreTexts Open Music Theory, BBC Science Focus, ASU Ask A Biologist, Healthline, MacTutor).
2. A verdict cites ONLY source pages that were actually opened and read during the check. Sources that failed to load are not cited; an alternative opened source was used instead.
3. Math answers were independently recomputed (exact integer/fraction arithmetic) in addition to citing definitional pages.
4. Verdict values:
   - **SUPPORTED** - the cited sources confirm the claim.
   - **PARTIAL** - the core claim is confirmed but an asserted detail is not covered by the cited sources, or the cited figure differs (details in the notes column).
   - **CONTRADICTED** - the cited sources say the claim is wrong.
   - **UNRESOLVED** - no opened source could settle the claim.

## Results

- Option verdicts: **131 SUPPORTED, 1 PARTIAL, 0 CONTRADICTED, 0 UNRESOLVED** (of 132).
- Explanation verdicts: **124 SUPPORTED, 8 PARTIAL, 0 CONTRADICTED, 0 UNRESOLVED** (of 132).

| Category | Questions | Option SUPPORTED | Option PARTIAL | Explanation SUPPORTED | Explanation PARTIAL |
|---|---|---|---|---|---|
| Geography | 20 | 19 | 1 | 16 | 4 |
| History | 24 | 24 | 0 | 24 | 0 |
| Health | 18 | 18 | 0 | 16 | 2 |
| Language | 24 | 24 | 0 | 24 | 0 |
| Math | 22 | 22 | 0 | 22 | 0 |
| Science | 24 | 24 | 0 | 22 | 2 |

No CONTRADICTED and no UNRESOLVED items.

## Items flagged PARTIAL (need reviewer attention)

| Question | Field | Issue |
|---|---|---|
| Q_Geo_001 | explanation | NOAA states the Pacific is the largest water mass on the planet and 'larger than the landmass of all the continents combined'; the explanation's stronger claim ('more area than all the other oceans combined') is not stated on the cited page. |
| Q_Geo_011 | explanation | NOAA confirms the Arctic is the smallest ocean; the explanation's 'coldest' claim is not covered by the cited page. |
| Q_Geo_012 | explanation | USGS confirms the Amazon is the world's largest river by discharge, but states about 15 percent of global river discharge, not the explanation's 'about a fifth'. |
| Q_Geo_016 | explanation | Britannica confirms Aboriginal Australian origin; the explanation's age claim ('at least a thousand years') is not stated on the cited page. |
| Q_Geo_017 | option | NatGeo: the needle points to the north MAGNETIC pole, which differs from true geographic north (magnetic declination). 'The North Pole' is the intended school-level answer but is imprecise. |
| Q_Hea_003 | explanation | CDC: soap lather forms micelles that trap and remove germs, chemicals and dirt, better than water alone; the explanation's 'oils' wording is not on the cited page. |
| Q_Hea_006 | explanation | MedlinePlus: citrus is a good vitamin C source and it helps absorb iron; the explanation's 'supports the immune system' claim is not on the cited page. |
| Q_Sci_007 | explanation | NASA confirms Jupiter is the largest planet but says 1,000 Earths could fit inside it, not the explanation's 1,300. |
| Q_Sci_008 | explanation | Britannica: the force that keeps the Moon in orbit is gravity. The explanation's orbital-mechanics clause ('sideways motion keeps it from falling in') is not stated on the cited page. |

## Per-question detail

The full row-per-question table (question text, marked answer, both verdicts, source URLs, notes) is in `quiz_factcheck.csv` alongside this report.

## Source coverage and limits

- Sources cited: 130 distinct pages, each opened and read during this check.
- Two reference pages failed to load and were replaced by opened alternatives: The Physics Classroom "Conductors and Insulators" (insulator support taken from the same site's "Requirements of a Circuit" page) and "Sound is a Mechanical Wave" (replaced by Britannica "sound").
- Q_Mat_011 and Q_Mat_015 are pure integer arithmetic; they were verified by recomputation only and cite no external page.
- Merriam-Webster's "tall" entry extraction did not include its inflection line; the comparative claim additionally cites the Grammarly comparative-adjectives article, which was opened.
- Health and science verdicts are against general reference sources at school level; they are not a medical or curriculum review.
- This report does not modify the question assets. The `factSource` marker updates are the maintainer's decision.

## What was not checked

- The Arabic translations inside the assets (`questionAr`, `optionsAr`, `explanationAr`) were not language-reviewed here; see `AR_TABLE_PROPOSALS.md` for the separate missing-string proposals.
- The 133rd question (already fact-checked upstream) was out of scope.
- Distractor options were assessed only as far as needed to confirm the marked answer is the best answer; no claim that every distractor is perfectly wrong.

