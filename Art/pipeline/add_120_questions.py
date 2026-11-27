# -*- coding: utf-8 -*-
"""Author 60 NEW question assets (LIGHT SHADOW bank grows 60 -> 120, follow-up
to task P4's Arabic fill).

Method:
  - This script ONLY CREATES new files Q_<Cat>_011+.asset in
    Assets/_Project/Data/Questions. It refuses to run if any target file
    already exists, and never opens an existing Q_*.asset for writing
    (append-only work; the original 60 stay byte-identical, GUIDs/.meta
    untouched).
  - File format copies the exact serialized skeleton of an existing asset
    (same m_Script guid -> QuestionData, same header fields, LF endings).
    The asset's own GUID lives in its .meta, which Unity generates on the
    next import - we must not author .meta files.
  - QuestionDatabase.asset is NOT edited here: new assets have no GUID until
    Unity imports them, so the database cannot reference them yet. The
    SceneBuilder calls QuestionGenerator.GenerateQuestionsSilent() before
    building the school (SceneBuilder.cs), and RebuildDatabase() re-scans the
    folder and sorts by name - the next Unity run links all 120 automatically.
  - Each entry carries source = "curriculum" (school-level fact, not a game
    invention). QuestionData.cs has no serialized `source` field, so the
    designation lives in this table and is asserted here; factSource stays
    NEEDS-FACT-CHECK (AGENTS.md rule 5) until a human reviews the facts.

Difficulty = wing quota: every wing gets exactly 4 easy (ActI) / 5 standard
(ActII) / 3 hard (ActIII) questions - 12 per wing, 60 total:
  Library      (Lan lit/language + His):           Lan_011-016, His_011-016
  Science wing (Sci physics/chem + Mat applied):   Sci_011-016, Mat_011-016
  Courtyard    (Sci bio + Hea body/senses + Geo):  Sci_017-020, Hea_011-014, Geo_011-014
  Assembly hall (His history/civics + Lan poetry + Geo music/culture + Mat
                music-math):                       His_017-020, Lan_017-020,
                                                   Geo_015-016, Mat_017-018
  Staff room   (mixed review, all six subjects):   Sci_021-022, Mat_019-020,
                                                   His_021-022, Geo_017-018,
                                                   Lan_021-022, Hea_015-016

Display-safety gate (Assets/_Project/Scripts/Core/ArabicSupport.cs, same rule
as fill_arabic_questions.py): inside any string that contains Arabic, every
Latin/digit island must be a single whitespace-free token ("8-10", "25%",
"5cm", «mouse»); pure-Latin strings are fine because the runtime shim skips
them. The script refuses to write a string that breaks the rule.

Usage:  python -I add_120_questions.py [--apply]
        (without --apply it only checks and prints what it would create)
"""

import os
import re
import sys

import yaml

QUESTIONS_REL = os.path.join("Assets", "_Project", "Data", "Questions")
FACT_SOURCE = "NEEDS-FACT-CHECK"
CATEGORIES = {"Sci": 0, "Mat": 1, "His": 2, "Geo": 3, "Lan": 4, "Hea": 5}
EXISTING_PER_CATEGORY = 10  # the original bank: _001.._010 per category

# id -> {wing, cat, diff (1=ActI/2=ActII/3=ActIII), q, o(4), ci, e, qa, oa(4), ea}
# Every entry: source = "curriculum". Arabic uses Egyptian-flavored MSA with the
# mother's voice for explanations; terminology follows the Egyptian secondary
# curriculum (الحدقة، المرآة المقعرة، الانكسار، النتح، التعويه، الشرط ...).
E = {
    # ================================================= Library wing =========
    # Lan_011-016 + His_011-016: literature, language, history.
    "Lan_011": {"wing": "Library", "cat": "Lan", "diff": 1,
        "q": "What is the plural of the noun \"mouse\"?",
        "o": ["mouses", "mice", "mousees", "mices"], "ci": 1,
        "e": "\"Mouse\" has the irregular plural \"mice\", just like \"child\" becomes \"children\".",
        "qa": "ما جمع الاسم «mouse»؟",
        "oa": ["mouses", "mice", "mousees", "mices"],
        "ea": "الاسم «mouse» جمعه تكسير «mice»، مثل «child» الذي جمعه «children»."},
    "Lan_012": {"wing": "Library", "cat": "Lan", "diff": 1,
        "q": "In the sentence \"The old wall creaked loudly,\" which word is an adjective?",
        "o": ["The", "old", "creaked", "loudly"], "ci": 1,
        "e": "Adjectives describe nouns: \"old\" tells us what kind of wall it was.",
        "qa": "في جملة «الجدار القديم أزّز بصوت عالٍ»، أي كلمة تعمل صفة؟",
        "oa": ["الجدار", "القديم", "أزّز", "بصوت عالٍ"],
        "ea": "الصفة تصف الاسم؛ فكلمة «القديم» تخبرنا أي نوع من الجدار كان."},
    "Lan_013": {"wing": "Library", "cat": "Lan", "diff": 2,
        "q": "What is the comparative form of the adjective \"tall\"?",
        "o": ["tallest", "taller", "more tall", "most tall"], "ci": 1,
        "e": "Short adjectives add an ending: tall, taller, tallest.",
        "qa": "ما صيغة التفضيل للصفة «tall»؟",
        "oa": ["tallest", "taller", "more tall", "most tall"],
        "ea": "الصفة القصيرة تُضيف مقطعًا للتفضيل؛ فتفضيل «tall» هو «taller»."},
    "Lan_014": {"wing": "Library", "cat": "Lan", "diff": 2,
        "q": "Which word joins the sentences \"I wanted to play outside. It was raining.\"?",
        "o": ["so", "but", "or", "because"], "ci": 1,
        "e": "\"But\" joins two ideas that clash: the wish to play and the falling rain.",
        "qa": "أي أداة تصل بين «أردتُ اللعب في الخارج» و«كانت تمطر»؟",
        "oa": ["لذلك", "لكن", "أو", "لأن"],
        "ea": "«لكن» تصل بين فكرتين متعارضتين؛ فالرغبة في اللعب ونزول المطر فكرتان متضادتان."},
    "Lan_015": {"wing": "Library", "cat": "Lan", "diff": 2,
        "q": "Which sentence is punctuated correctly?",
        "o": ["Where is my bag.", "Where is my bag?", "Where is my bag!", "where is my bag?"],
        "ci": 1,
        "e": "A question needs a question mark at the end and a capital letter at the start.",
        "qa": "أي جملة علامات الترقيم فيها صحيحة؟",
        "oa": ["Where is my bag.", "Where is my bag?", "Where is my bag!", "where is my bag?"],
        "ea": "السؤال يحتاج علامة استفهام في آخره وحرفًا كبيرًا في أوله."},
    "Lan_016": {"wing": "Library", "cat": "Lan", "diff": 3,
        "q": "In \"The wind whispered through the trees,\" giving the wind a human action is an example of...",
        "o": ["a simile", "personification", "a fact", "a command"], "ci": 1,
        "e": "Personification gives human actions to non-human things; the wind cannot really whisper.",
        "qa": "في جملة «همسَ الريح بين الأشجار»، إسناد فعل إنساني إلى الريح مثال على...",
        "oa": ["تشبيه", "تشخيص", "حقيقة", "أمر"],
        "ea": "إسناد فعل إنساني كالهمس إلى الريح يسمى تشخيصًا؛ فالريح غير عاقل ولا تهمس حقًا."},
    "His_011": {"wing": "Library", "cat": "His", "diff": 1,
        "q": "In which year did the First World War begin?",
        "o": ["1914", "1918", "1939", "1945"], "ci": 0,
        "e": "The First World War began in 1914 and ended in 1918.",
        "qa": "في أي عام بدأت الحرب العالمية الأولى؟",
        "oa": ["1914", "1918", "1939", "1945"],
        "ea": "بدأت الحرب العالمية الأولى عام 1914 وانتهت عام 1918."},
    "His_012": {"wing": "Library", "cat": "His", "diff": 3,
        "q": "Which Renaissance artist painted the ceiling of the Sistine Chapel in Rome?",
        "o": ["Leonardo da Vinci", "Michelangelo", "Raphael", "Donatello"], "ci": 1,
        "e": "Michelangelo painted the Sistine Chapel ceiling between 1508 and 1512.",
        "qa": "أي فنان من عصر النهضة رسم سقف كنيسة سيستين في روما؟",
        "oa": ["ليوناردو دافينشي", "ميكيلانجلو", "رفائيل", "دوناتيلو"],
        "ea": "رسم ميكيلانجلو سقف كنيسة سيستين بين عامي 1508 و1512."},
    "His_013": {"wing": "Library", "cat": "His", "diff": 2,
        "q": "The word \"algebra\" and the study of equations trace back to which scholar's famous book?",
        "o": ["Al-Khwarizmi", "Ibn Battuta", "Al-Razi", "Ibn Khaldun"], "ci": 0,
        "e": "Al-Khwarizmi's ninth-century book \"Al-Jabr\" gave algebra its name.",
        "qa": "أي عالم مسلم وضع أسس علم الجبر في كتابه المشهور؟",
        "oa": ["الخوارزمي", "ابن بطوطة", "الرازي", "ابن خلدون"],
        "ea": "كتاب الخوارزمي «الجبر والمقابلة» في القرن التاسع الميلادي أعطى علم الجبر اسمه."},
    "His_014": {"wing": "Library", "cat": "His", "diff": 2,
        "q": "The House of Wisdom, where scholars translated books into Arabic in the Islamic Golden Age, stood in which city?",
        "o": ["Cairo", "Baghdad", "Damascus", "Cordoba"], "ci": 1,
        "e": "Baghdad's House of Wisdom gathered translators and scientists in the ninth century.",
        "qa": "بيت الحكمة، الذي تُرجمت فيه الكتب إلى العربية في العصر الذهبي الإسلامي، كان في أي مدينة؟",
        "oa": ["القاهرة", "بغداد", "دمشق", "قرطبة"],
        "ea": "اجتمع في بيت الحكمة ببغداد المترجمون والعلماء في القرن التاسع الميلادي."},
    "His_015": {"wing": "Library", "cat": "His", "diff": 1,
        "q": "The Great Library, one of the most famous libraries of the ancient world, was in which city?",
        "o": ["Alexandria", "Athens", "Rome", "Babylon"], "ci": 0,
        "e": "The Great Library of Alexandria in Egypt collected scrolls from across the ancient world.",
        "qa": "المكتبة الكبرى، إحدى أشهر مكتبات العالم القديم، كانت في أي مدينة؟",
        "oa": ["الإسكندرية", "أثينا", "روما", "بابل"],
        "ea": "جمعت المكتبة الكبرى في الإسكندرية بمصر اللفائف العلمية من أنحاء العالم القديم."},
    "His_016": {"wing": "Library", "cat": "His", "diff": 3,
        "q": "Which of these four events happened earliest?",
        "o": ["The fall of Constantinople", "Columbus reaching the Americas",
              "The Mongol sack of Baghdad", "The French Revolution"], "ci": 2,
        "e": "Order: Baghdad fell to the Mongols in 1258, Constantinople fell in 1453, Columbus sailed in 1492, the French Revolution began in 1789.",
        "qa": "أي من هذه الأحداث وقع أولًا؟",
        "oa": ["سقوط القسطنطينية", "وصول كولومبوس إلى أمريكا",
               "اجتياح بغداد على يد المغول", "الثورة الفرنسية"],
        "ea": "الترتيب: اجتياح بغداد 1258، ثم القسطنطينية 1453، ثم رحلة كولومبوس 1492، ثم الثورة الفرنسية 1789."},
    # ================================================= Science wing =========
    # Sci_011-016 physics optics/electricity + Mat_011-016 applied math.
    "Sci_011": {"wing": "Science wing", "cat": "Sci", "diff": 1,
        "q": "Which part of the eye is the hole that lets light in?",
        "o": ["The pupil", "The eyebrow", "The eyelash", "The iris"], "ci": 0,
        "e": "Light enters the eye through the pupil; the iris muscles make it wider or narrower.",
        "qa": "أي جزء من العين هو الفتحة التي تدخل منها الأشعة؟",
        "oa": ["حدقة العين", "الحاجب", "الرموش", "قزحية العين"],
        "ea": "تدخل الأشعة إلى العين من الحدقة، والعضلات في القزحية تُوسّعها أو تُضيّقها."},
    "Sci_012": {"wing": "Science wing", "cat": "Sci", "diff": 1,
        "q": "What do we call a material that does not let electricity flow through it?",
        "o": ["A conductor", "An insulator", "A magnet", "A battery"], "ci": 1,
        "e": "Insulators like plastic and rubber stop the current; metals are conductors.",
        "qa": "ماذا نسمي المادة التي لا تمر بها الكهرباء؟",
        "oa": ["ناقل", "عازل", "مغناطيس", "بطارية"],
        "ea": "العوازل كالبلاستيك والمطاط تمنع مرور التيار، أما المعادن فتوصله."},
    "Sci_013": {"wing": "Science wing", "cat": "Sci", "diff": 2,
        "q": "A concave (inward-curved) mirror can make light rays...",
        "o": ["Bend around objects", "Gather at one point", "Disappear", "Change color"],
        "ci": 1,
        "e": "A concave mirror reflects parallel rays so they meet at a focus point - that is what makes a torch beam strong.",
        "qa": "المرآة المقعرة تستطيع أن تجعل أشعة الضوء...",
        "oa": ["تنثني حول الأجسام", "تتجمع في نقطة واحدة", "تختفي", "تتغير ألوانها"],
        "ea": "تعكس المرآة المقعرة الأشعة المتوازية بحيث تلتقي في نقطة البؤرة، ولهذا يكون شعاع الكشاف قويًا."},
    "Sci_014": {"wing": "Science wing", "cat": "Sci", "diff": 2,
        "q": "A straw in a glass of water looks bent at the surface. Why?",
        "o": ["The water pushes the straw", "Light bends as it passes from air into water",
              "The straw is broken", "Water dissolves plastic"], "ci": 1,
        "e": "That bending is refraction: light changes speed when it moves between air and water.",
        "qa": "تبدو الشفاطة في كوب الماء مقطوعة عند سطح الماء؛ لماذا؟",
        "oa": ["لأن الماء يدفع الشفاطة", "لأن الضوء ينكسر عند انتقاله من الهواء إلى الماء",
               "لأن الشفاطة مكسورة", "لأن الماء يذيب البلاستيك"],
        "ea": "الظاهرة تسمى الانكسار؛ يغير الضوء سرعته عندما ينتقل بين الهواء والماء."},
    "Sci_015": {"wing": "Science wing", "cat": "Sci", "diff": 2,
        "q": "In a simple circuit with a battery and a bulb, what happens if one wire is disconnected?",
        "o": ["The bulb glows brighter", "The current stops and the bulb goes out",
              "The current flows backwards", "Nothing changes"], "ci": 1,
        "e": "A circuit must be a complete loop; any break stops the current everywhere.",
        "qa": "في دارة بسيطة فيها بطارية ومصباح، ماذا يحدث لو فُصل سلك واحد؟",
        "oa": ["يضيء المصباح أقوى", "يتوقف التيار وينطفئ المصباح",
               "يعكس التيار اتجاهه", "لا يتغير شيء"],
        "ea": "يجب أن تكون الدارة حلقة مغلقة؛ وأي قطع في المسار يوقف التيار في كل مكان."},
    "Sci_016": {"wing": "Science wing", "cat": "Sci", "diff": 3,
        "q": "Two identical bulbs are connected in series to one battery. How bright are they compared with one bulb alone?",
        "o": ["Brighter", "The same", "Dimmer, because the battery's energy is shared",
              "They go out completely"], "ci": 2,
        "e": "In series the same current passes through both bulbs and the battery's push is shared, so each glows dimmer.",
        "qa": "يُوصل مصباحان متماثلان على التوالي ببطارية واحدة؛ كيف يظهران مقارنة بمصباح وحيد؟",
        "oa": ["أكثر إضاءة", "بالإضاءة نفسها", "أخفت، لأن طاقة البطارية تتوزع بينهما",
               "ينطفئان تمامًا"],
        "ea": "على التوالي يمر التيار نفسه في المصباحين وتتوزع طاقة البطارية بينهما، فيضيء كل واحد أخفت."},
    "Mat_011": {"wing": "Science wing", "cat": "Mat", "diff": 1,
        "q": "What is 15 + 27?",
        "o": ["32", "41", "42", "52"], "ci": 2,
        "e": "Five plus seven makes twelve and ten plus twenty makes thirty, so the total is 42.",
        "qa": "كم يساوي 15+27؟",
        "oa": ["32", "41", "42", "52"],
        "ea": "خمسة زائد سبعة تجعل اثني عشر، وعشرة زائد عشرين تجعل ثلاثين، فالمجموع 42."},
    "Mat_012": {"wing": "Science wing", "cat": "Mat", "diff": 1,
        "q": "Which of these numbers is even?",
        "o": ["13", "27", "34", "41"], "ci": 2,
        "e": "34 ends in 4, so it divides by 2; the others are odd.",
        "qa": "أي من هذه الأعداد عدد زوجي؟",
        "oa": ["13", "27", "34", "41"],
        "ea": "العدد 34 ينتهي بالرقم 4 فيقبل القسمة على 2، أما الباقية فأعداد فردية."},
    "Mat_013": {"wing": "Science wing", "cat": "Mat", "diff": 2,
        "q": "A rectangle is 8 cm long and 3 cm wide. What is its area?",
        "o": ["11 cm2", "16 cm2", "24 cm2", "22 cm2"], "ci": 2,
        "e": "Area is length times width: 8 x 3 = 24 cm2.",
        "qa": "مستطيل طوله 8cm وعرضه 3cm؛ ما مساحته؟",
        "oa": ["11 cm2", "16 cm2", "24 cm2", "22 cm2"],
        "ea": "الطول 8cm مضروبًا في العرض 3cm يجعل المساحة 24cm2."},
    "Mat_014": {"wing": "Science wing", "cat": "Mat", "diff": 2,
        "q": "The mean (average) of 4, 6 and 8 is...",
        "o": ["5", "6", "7", "18"], "ci": 1,
        "e": "Add them to get 18, then divide by the three numbers: 18 / 3 = 6.",
        "qa": "الوسط الحسابي للأعداد 4 و6 و8 يساوي...",
        "oa": ["5", "6", "7", "18"],
        "ea": "مجموعها 18، نقسمه على عدد الأعداد وهي 3، فيكون الوسط 6."},
    "Mat_015": {"wing": "Science wing", "cat": "Mat", "diff": 3,
        "q": "The sum of two consecutive whole numbers is 35. What is the larger number?",
        "o": ["16", "17", "18", "19"], "ci": 2,
        "e": "Half of 35 is 17.5, so the pair is 17 and 18; the larger is 18.",
        "qa": "مجموع عددين صحيحين متتاليين يساوي 35؛ ما العدد الأكبر؟",
        "oa": ["16", "17", "18", "19"],
        "ea": "نصف 35 هو 17.5، فيكون العددان 17 و18، والأكبر منهما 18."},
    "Mat_016": {"wing": "Science wing", "cat": "Mat", "diff": 3,
        "q": "A train travels 60 km in 45 minutes. What is its average speed in km/h?",
        "o": ["70", "75", "80", "90"], "ci": 2,
        "e": "45 minutes is three quarters of an hour, so 60 km in that time means 80 km per hour.",
        "qa": "قطار يقطع 60km في 45 دقيقة؛ ما متوسط سرعته بالكيلومتر في الساعة؟",
        "oa": ["70", "75", "80", "90"],
        "ea": "المدة 45 دقيقة هي ثلاثة أرباع الساعة، فيكون 60km في تلك المدة بسرعة 80km في الساعة."},
    # ================================================= Courtyard ============
    # Sci_017-020 biology + Hea_011-014 body/senses + Geo_011-014 earth/nature.
    "Sci_017": {"wing": "Courtyard", "cat": "Sci", "diff": 2,
        "q": "During photosynthesis, which gas do plants release into the air?",
        "o": ["Carbon dioxide", "Oxygen", "Nitrogen", "Hydrogen"], "ci": 1,
        "e": "Plants take in carbon dioxide and give out oxygen - one big reason we can breathe.",
        "qa": "أثناء التمثيل الضوئي، أي غاز تطلق النباتات في الهواء؟",
        "oa": ["ثاني أكسيد الكربون", "الأكسجين", "النيتروجين", "الهيدروجين"],
        "ea": "تمتص النباتات ثاني أكسيد الكربون وتخرج الأكسجين، وهذا من أسباب تنفسنا."},
    "Sci_018": {"wing": "Courtyard", "cat": "Sci", "diff": 1,
        "q": "Which sense organ do we use to hear sounds?",
        "o": ["The ears", "The nose", "The tongue", "The skin"], "ci": 0,
        "e": "The ears collect sound waves and the brain turns them into meaning.",
        "qa": "أي عضو من أعضاء الحواس نستعمله لنسمع الأصوات؟",
        "oa": ["الأذنان", "الأنف", "اللسان", "الجلد"],
        "ea": "تجمع الأذنان موجات الصوت، ثم يحوّلها الدماغ إلى معنى نفهمه."},
    "Sci_019": {"wing": "Courtyard", "cat": "Sci", "diff": 2,
        "q": "Which part of the brain keeps you balanced when you walk or stand?",
        "o": ["The cerebellum", "The lungs", "The stomach", "The skull"], "ci": 0,
        "e": "The cerebellum, at the back of the brain, fine-tunes your balance and movement.",
        "qa": "أي جزء من الدماغ يحفظ توازنك عندما تمشين أو تقفين؟",
        "oa": ["المخيخ", "الرئتان", "المعدة", "الجمجمة"],
        "ea": "المخيخ في مؤخرة الدماغ يضبط توازنك وحركتك بدقة."},
    "Sci_020": {"wing": "Courtyard", "cat": "Sci", "diff": 3,
        "q": "The process by which plants lose water vapour through their leaves is called...",
        "o": ["Germination", "Transpiration", "Pollination", "Digestion"], "ci": 1,
        "e": "Transpiration pulls water up from the roots and out through the leaves.",
        "qa": "العملية التي تفقد بها النباتات بخار الماء من أوراقها تسمى...",
        "oa": ["الإنبات", "النتح", "التلقيح", "الهضم"],
        "ea": "النتح يسحب الماء من الجذور إلى الأعلى ويخرجه من الأوراق."},
    "Hea_011": {"wing": "Courtyard", "cat": "Hea", "diff": 1,
        "q": "Which hard part of the body protects the brain?",
        "o": ["The ribs", "The skull", "The spine", "The pelvis"], "ci": 1,
        "e": "The skull is a bony helmet around the brain.",
        "qa": "أي جزء صلب من الجسم يحمي الدماغ؟",
        "oa": ["الأضلاع", "الجمجمة", "العمود الفقري", "الحوض"],
        "ea": "الجمجمة خوذة عظمية تحيط بالدماغ وتحميه."},
    "Hea_012": {"wing": "Courtyard", "cat": "Hea", "diff": 1,
        "q": "The nose is the sense organ for which sense?",
        "o": ["Sight", "Smell", "Hearing", "Taste"], "ci": 1,
        "e": "Receptors high inside the nose detect smells in the air.",
        "qa": "الأنف عضو خاص بأي حاسة؟",
        "oa": ["الإبصار", "الشم", "السمع", "التذوق"],
        "ea": "المستقبلات في أعلى الأنف تميز الروائح الموجودة في الهواء."},
    "Hea_013": {"wing": "Courtyard", "cat": "Hea", "diff": 3,
        "q": "Which vitamin helps the blood to clot after a cut?",
        "o": ["Vitamin A", "Vitamin C", "Vitamin K", "Vitamin D"], "ci": 2,
        "e": "Vitamin K lets the body make the proteins that form clots and stop bleeding.",
        "qa": "أي فيتامين يعين الدم على التجلط بعد الجرح؟",
        "oa": ["فيتامين أ", "فيتامين ج", "فيتامين ك", "فيتامين د"],
        "ea": "فيتامين ك يعين الجسم على صنع البروتينات التي تُكوّن الجلطة فتوقف النزيف."},
    "Hea_014": {"wing": "Courtyard", "cat": "Hea", "diff": 2,
        "q": "Which mineral from milk and cheese keeps bones and teeth strong?",
        "o": ["Iron", "Calcium", "Potassium", "Sodium"], "ci": 1,
        "e": "Calcium builds bone; dairy foods are its richest everyday source.",
        "qa": "أي معدن موجود في الحليب والجبن يبني العظام والأسنان ويقويها؟",
        "oa": ["الحديد", "الكالسيوم", "البوتاسيوم", "الصوديوم"],
        "ea": "الكالسيوم يبني العظام، ومنتجات الألبان أغنى مصادره اليومية."},
    "Geo_011": {"wing": "Courtyard", "cat": "Geo", "diff": 1,
        "q": "Which is the smallest ocean on Earth?",
        "o": ["The Atlantic Ocean", "The Indian Ocean", "The Southern Ocean", "The Arctic Ocean"],
        "ci": 3,
        "e": "The Arctic is the smallest and coldest ocean, around the North Pole.",
        "qa": "أي محيط هو الأصغر على وجه الأرض؟",
        "oa": ["المحيط الأطلسي", "المحيط الهندي", "المحيط المتجمد الجنوبي", "المحيط المتجمد الشمالي"],
        "ea": "المحيط المتجمد الشمالي هو الأصغر والأبرد، ويحيط بالقطب الشمالي."},
    "Geo_012": {"wing": "Courtyard", "cat": "Geo", "diff": 3,
        "q": "Which river carries more water than any other river on Earth?",
        "o": ["The Nile", "The Amazon", "The Yangtze", "The Mississippi"], "ci": 1,
        "e": "The Amazon pours about a fifth of all river water on Earth into the sea.",
        "qa": "أي نهر يحمل ماءً أكثر من أي نهر آخر على الأرض؟",
        "oa": ["النيل", "الأمازون", "اليانغتسي", "الميسيسيبي"],
        "ea": "يصب الأمازون نحو خُمس مياه أنهار الأرض كلها في البحر."},
    "Geo_013": {"wing": "Courtyard", "cat": "Geo", "diff": 2,
        "q": "The slow breaking-down of rocks in place by wind, water and temperature changes is called...",
        "o": ["Weathering", "Erosion", "Flooding", "Eruption"], "ci": 0,
        "e": "Weathering breaks the rock where it stands; erosion carries the pieces away.",
        "qa": "التفتت البطيء للصخور في مكانها بفعل الرياح والماء وتغيرات الحرارة يسمى...",
        "oa": ["التعويه", "النحر", "الفيضان", "الثوران"],
        "ea": "التعويه يكسر الصخر في مكانه، أما النحر فيحمل القطع بعيدًا بالماء والريح."},
    "Geo_014": {"wing": "Courtyard", "cat": "Geo", "diff": 2,
        "q": "What do we call the amount of water vapour present in the air?",
        "o": ["Humidity", "Altitude", "Gravity", "Friction"], "ci": 0,
        "e": "Air with high humidity feels heavy and sticky, especially before rain.",
        "qa": "ماذا نسمي كمية بخار الماء الموجودة في الهواء؟",
        "oa": ["الرطوبة", "الارتفاع", "الجاذبية", "الاحتكاك"],
        "ea": "الجو عالي الرطوبة يبدو ثقيلًا لزجًا، خصوصًا قبل المطر."},
    # ================================================= Assembly hall ========
    # His_017-020 history/civics + Lan_017-020 poetry + Geo_015-016 music
    # cultures + Mat_017-018 music math.
    "His_017": {"wing": "Assembly hall", "cat": "His", "diff": 2,
        "q": "Which scholar of the Islamic Golden Age wrote a famous medical book that Europe used for centuries?",
        "o": ["Ibn Sina", "Ibn Khaldun", "Al-Biruni", "Ibn Rushd"], "ci": 0,
        "e": "Ibn Sina's \"Canon of Medicine\" taught doctors in Europe for hundreds of years.",
        "qa": "أي عالم من العصر الذهبي الإسلامي كتب كتابًا طبيًا مشهورًا استعملته أوروبا قرونًا طويلة؟",
        "oa": ["ابن سينا", "ابن خلدون", "البيروني", "ابن رشد"],
        "ea": "كتاب ابن سينا «القانون في الطب» علّم أطباء أوروبا مئات السنين."},
    "His_018": {"wing": "Assembly hall", "cat": "His", "diff": 1,
        "q": "What do we call the basic law of a country that sets out citizens' rights and duties?",
        "o": ["A constitution", "A dictionary", "A map", "A newspaper"], "ci": 0,
        "e": "A constitution is the highest basic law of a country.",
        "qa": "ماذا نسمي القانون الأساسي للبلد الذي يحدد حقوق المواطنين وواجباتهم؟",
        "oa": ["الدستور", "القاموس", "الخريطة", "الصحيفة"],
        "ea": "الدستور هو القانون الأساسي الأعلى للبلد."},
    "His_019": {"wing": "Assembly hall", "cat": "His", "diff": 3,
        "q": "Which composer kept writing great music even after he lost his hearing?",
        "o": ["Wolfgang Amadeus Mozart", "Johann Sebastian Bach", "Ludwig van Beethoven",
              "Joseph Haydn"], "ci": 2,
        "e": "Beethoven composed some of his greatest works while almost completely deaf.",
        "qa": "أي ملحّن واصل تأليف موسيقى عظيمة حتى بعدما فقد سمعه؟",
        "oa": ["فولفغانغ أماديوس موتسارت", "يوهان سباستيان باخ", "لودفيج فان بيتهوفن",
               "جوزيف هايدن"],
        "ea": "ألّف بيتهوفن أشهر أعماله الكبرى وهو شبه أصم تمامًا."},
    "His_020": {"wing": "Assembly hall", "cat": "His", "diff": 1,
        "q": "The Olympic Games began in which ancient country?",
        "o": ["Greece", "Rome", "Egypt", "China"], "ci": 0,
        "e": "The first recorded Olympic Games were held at Olympia in Greece in 776 BC.",
        "qa": "بدأت الألعاب الأولمبية في أي بلد قديم؟",
        "oa": ["اليونان", "روما", "مصر", "الصين"],
        "ea": "أُقيمت أول ألعاب أولمبية مسجلة في أولمبيا باليونان عام 776 قبل الميلاد."},
    "Lan_017": {"wing": "Assembly hall", "cat": "Lan", "diff": 2,
        "q": "A long poem that tells a story of heroes and great events is called...",
        "o": ["an epic", "a recipe", "a letter", "a diary"], "ci": 0,
        "e": "An epic celebrates heroes and great events, like the Epic of Gilgamesh.",
        "qa": "القصيدة الطويلة التي تحكي قصص الأبطال والأحداث العظيمة تسمى...",
        "oa": ["ملحمة", "وصفة", "رسالة", "يومية"],
        "ea": "الملحمة تمجّد الأبطال والأحداث الكبيرة، مثل ملحمة جلجامش الشهيرة."},
    "Lan_018": {"wing": "Assembly hall", "cat": "Lan", "diff": 3,
        "q": "A sonnet is a poem of exactly how many lines?",
        "o": ["10", "12", "14", "16"], "ci": 2,
        "e": "A sonnet has fourteen lines and a fixed rhyme pattern.",
        "qa": "السونيتة قصيدة ثابتة عدد أبياتها؛ كم بيتًا فيها؟",
        "oa": ["10", "12", "14", "16"],
        "ea": "السونيتة أربعة عشر بيتًا بنظام قافية ثابت."},
    "Lan_019": {"wing": "Assembly hall", "cat": "Lan", "diff": 2,
        "q": "Which pair of words rhymes?",
        "o": ["moon / tree", "moon / spoon", "moon / band", "moon / sky"], "ci": 1,
        "e": "Rhyming words share their ending sound: moon and spoon both end in -oon.",
        "qa": "أي زوج من الكلمات متقافٍ؟",
        "oa": ["moon / tree", "moon / spoon", "moon / band", "moon / sky"],
        "ea": "المتقافي يشترك مع قرينه في صوت آخره؛ فـ«spoon» تنتهي مثل «moon»."},
    "Lan_020": {"wing": "Assembly hall", "cat": "Lan", "diff": 1,
        "q": "A haiku is a short Japanese poem of how many lines?",
        "o": ["Two", "Three", "Five", "Seven"], "ci": 1,
        "e": "A haiku has three short lines, often about nature.",
        "qa": "الهيكو قصيدة يابانية قصيرة، من كم سطر تتكون؟",
        "oa": ["سطرين", "ثلاثة أسطر", "خمسة أسطر", "سبعة أسطر"],
        "ea": "الهيكو ثلاثة أسطر قصيرة، غالبًا عن الطبيعة."},
    "Geo_015": {"wing": "Assembly hall", "cat": "Geo", "diff": 2,
        "q": "Flamenco, a song and dance style with guitar, comes from which country?",
        "o": ["Spain", "Italy", "Brazil", "Turkey"], "ci": 0,
        "e": "Flamenco grew in Andalusia in southern Spain.",
        "qa": "الفلامنكو، فن الغناء والرقص المصاحب للجيتار، جاء من أي بلد؟",
        "oa": ["إسبانيا", "إيطاليا", "البرازيل", "تركيا"],
        "ea": "نشأ الفلامنكو في أندلسيا بجنوب إسبانيا."},
    "Geo_016": {"wing": "Assembly hall", "cat": "Geo", "diff": 1,
        "q": "The didgeridoo, one of the oldest wind instruments in the world, comes from the Aboriginal peoples of which country?",
        "o": ["Australia", "Canada", "Japan", "India"], "ci": 0,
        "e": "Aboriginal Australians have played the didgeridoo for at least a thousand years.",
        "qa": "الديجيريدو، من أقدم آلات النفخ في العالم، ينتسب إلى السكان الأصليين في أي بلد؟",
        "oa": ["أستراليا", "كندا", "اليابان", "الهند"],
        "ea": "يعزف السكان الأصليون في أستراليا الديجيريدو منذ أكثر من ألف عام."},
    "Mat_017": {"wing": "Assembly hall", "cat": "Mat", "diff": 2,
        "q": "One quarter note lasts as long as how many eighth notes?",
        "o": ["Two", "Three", "Four", "Six"], "ci": 0,
        "e": "Each quarter note splits into two eighth notes.",
        "qa": "النوتة الربعية تساوي في طولها كم نوتة ثمنية؟",
        "oa": ["اثنتين", "ثلاث", "أربع", "ست"],
        "ea": "كل نوتة ربعية تنقسم إلى نوتتين ثمنيتين."},
    "Mat_018": {"wing": "Assembly hall", "cat": "Mat", "diff": 3,
        "q": "How many sixteenth notes fit into one quarter note?",
        "o": ["Two", "Three", "Four", "Eight"], "ci": 2,
        "e": "A quarter is two eighths, and each eighth is two sixteenths, so four in all.",
        "qa": "كم نوتة سادسية عشرية تتسع في نوتة ربعية واحدة؟",
        "oa": ["اثنتين", "ثلاث", "أربع", "ثماني"],
        "ea": "الربعية نوتتان ثمنيتان، وكل ثمنية نوتتان سادسيتان عشريتان، فالمجموع أربع."},
    # ================================================= Staff room ===========
    # Sci_021-022 + Mat_019-020 + His_021-022 + Geo_017-018 + Lan_021-022 +
    # Hea_015-016: mixed review across all six subjects.
    "Sci_021": {"wing": "Staff room", "cat": "Sci", "diff": 1,
        "q": "Which state of matter keeps both its shape and its volume?",
        "o": ["A solid", "A liquid", "A gas", "Steam"], "ci": 0,
        "e": "Solids keep their shape; liquids take the container's shape and gases spread out.",
        "qa": "أي حالة من حالات المادة تحافظ على شكلها وحجمها معًا؟",
        "oa": ["الصلبة", "السائلة", "الغازية", "البخار"],
        "ea": "الصلبة تحفظ شكلها، والسائلة تأخذ شكل الإناء، والغازية تنتشر في كل الاتجاهات."},
    "Sci_022": {"wing": "Staff room", "cat": "Sci", "diff": 2,
        "q": "Sound cannot travel at all through...",
        "o": ["Water", "Steel", "Air", "A vacuum"], "ci": 3,
        "e": "Sound needs particles to vibrate; space is almost empty, so no sound reaches us from it.",
        "qa": "لا ينتقل الصوت إطلاقًا عبر...",
        "oa": ["الماء", "الحديد", "الهواء", "الفراغ"],
        "ea": "يحتاج الصوت إلى وسط من الجزيئات يتذبذب؛ والفضاء لا يحتوي ما ينقله."},
    "Mat_019": {"wing": "Staff room", "cat": "Mat", "diff": 2,
        "q": "What is 20% of 150?",
        "o": ["25", "30", "35", "45"], "ci": 1,
        "e": "A tenth of 150 is 15, so a fifth (20%) is 30.",
        "qa": "ما قيمة 20% من 150؟",
        "oa": ["25", "30", "35", "45"],
        "ea": "عُشر 150 يساوي 15، فالخمس وهو 20% يساوي 30."},
    "Mat_020": {"wing": "Staff room", "cat": "Mat", "diff": 3,
        "q": "A jacket sells for 200 after a 20% discount. What was its original price?",
        "o": ["220", "240", "250", "260"], "ci": 2,
        "e": "The sale price is 80% of the original, and 200 is 80% of 250.",
        "qa": "جاكيت سعره بعد خصم 20% هو 200؛ ما سعره الأصلي؟",
        "oa": ["220", "240", "250", "260"],
        "ea": "سعر البيع هو 80% من الأصلي، و200 هي 80% من 250."},
    "His_021": {"wing": "Staff room", "cat": "His", "diff": 1,
        "q": "In which country was the Great Wall built?",
        "o": ["Japan", "China", "India", "Korea"], "ci": 1,
        "e": "The Great Wall of China stretches thousands of kilometres across the north of the country.",
        "qa": "في أي بلد بُني السور العظيم؟",
        "oa": ["اليابان", "الصين", "الهند", "كوريا"],
        "ea": "يمتد سور الصين العظيم آلاف الكيلومترات عبر شمال البلد."},
    "His_022": {"wing": "Staff room", "cat": "His", "diff": 2,
        "q": "Which medieval scholar travelled for almost 30 years across Africa and Asia and wrote down everything he saw?",
        "o": ["Ibn Battuta", "Marco Polo", "Ibn Khaldun", "Al-Biruni"], "ci": 0,
        "e": "Ibn Battuta journeyed for about 29 years and dictated his travels in the Rihla.",
        "qa": "أي عالم من العصور الوسطى رحل نحو 30 سنة في إفريقيا وآسيا وكتب كل ما رآه؟",
        "oa": ["ابن بطوطة", "ماركو بولو", "ابن خلدون", "البيروني"],
        "ea": "رحل ابن بطوطة نحو 29 سنة ثم أملى رحلته في كتاب «الرحلة»."},
    "Geo_017": {"wing": "Staff room", "cat": "Geo", "diff": 2,
        "q": "The needle of a compass points toward...",
        "o": ["The South Pole", "The North Pole", "The Equator", "The Sun"], "ci": 1,
        "e": "Earth acts like a magnet, and the compass needle lines up with it, pointing north.",
        "qa": "إبرة البوصلة تشير نحو...",
        "oa": ["القطب الجنوبي", "القطب الشمالي", "خط الاستواء", "الشمس"],
        "ea": "للأرض مجال مغناطيسي يجعل إبرة البوصلة تصطف معه وتشير إلى الشمال."},
    "Geo_018": {"wing": "Staff room", "cat": "Geo", "diff": 3,
        "q": "What is the deepest known place in the oceans?",
        "o": ["The Mariana Trench", "The Red Sea", "The Bermuda Triangle",
              "The Great Barrier Reef"], "ci": 0,
        "e": "The Mariana Trench dips almost 11 kilometres below the sea surface.",
        "qa": "ما أعمق بقعة معروفة في المحيطات؟",
        "oa": ["خندق ماريانا", "البحر الأحمر", "مثلث برمودا", "الحاجز المرجاني العظيم"],
        "ea": "ينخفض خندق ماريانا نحو 11 كيلومترًا تحت سطح البحر."},
    "Lan_021": {"wing": "Staff room", "cat": "Lan", "diff": 1,
        "q": "The saying \"a heart of gold\" describes a person who is...",
        "o": ["Very kind", "Very rich", "Very strict", "Quick to anger"], "ci": 0,
        "e": "A heart of gold means a warm, generous person; gold is precious, and so is their kindness.",
        "qa": "قول «قلبه من ذهب» يُستخدم لوصف...",
        "oa": ["شخص طيب جدًا", "شخص ثري جدًا", "شخص صارم", "شخص سريع الغضب"],
        "ea": "«قلبه من ذهب» تصف شخصًا طيبًا كريمًا؛ فالذهب ثمين، وطيبة قلبه مثل ذلك الثمن."},
    "Lan_022": {"wing": "Staff room", "cat": "Lan", "diff": 2,
        "q": "What is the plural of the noun \"leaf\"?",
        "o": ["leafs", "leafes", "leaves", "leafen"], "ci": 2,
        "e": "\"Leaf\" becomes \"leaves\" - words ending in -f often do this.",
        "qa": "ما جمع الاسم «leaf»؟",
        "oa": ["leafs", "leafes", "leaves", "leafen"],
        "ea": "الاسم «leaf» جمعه «leaves»؛ وكلمات كثيرة تنتهي بحرف «f» تحوّل إلى «ves»."},
    "Hea_015": {"wing": "Staff room", "cat": "Hea", "diff": 1,
        "q": "Which organs clean the blood and make urine?",
        "o": ["The kidneys", "The lungs", "The heart", "The stomach"], "ci": 0,
        "e": "The two kidneys filter the blood and send the waste out as urine.",
        "qa": "أي أعضاء تنقي الدم وتُخرج البول؟",
        "oa": ["الكليتان", "الرئتان", "القلب", "المعدة"],
        "ea": "الكليتان تنقّيان الدم وترسلان الفضلات خارج الجسم في البول."},
    "Hea_016": {"wing": "Staff room", "cat": "Hea", "diff": 3,
        "q": "Which organ makes insulin, the hormone that controls blood sugar?",
        "o": ["The pancreas", "The liver", "The spleen", "The bladder"], "ci": 0,
        "e": "The pancreas releases insulin so the body can use sugar for energy.",
        "qa": "أي عضو يفرز الأنسولين، الهرمون الذي يضبط سكر الدم؟",
        "oa": ["البنكرياس", "الكبد", "الطحال", "المثانة"],
        "ea": "يفرز البنكرياس الأنسولين لكي يستعمل الجسم السكر في الطاقة."},
}


# ------------------------------------------------------------------ checks --
def _latin_char(c):
    return bool(re.match(r"[0-9A-Za-z+\-/%=.,:;!?\[\]{}<>]", c))


def _script_tokens(s):
    """Port of ArabicSupport.Tokenize: maximal script runs, split at whitespace
    and at Arabic/Latin boundaries."""
    tokens = []
    run_type = None
    buf = []
    for c in s:
        if c.isspace():
            if buf:
                tokens.append((run_type, "".join(buf)))
                run_type, buf = None, []
            continue
        t = "lat" if _latin_char(c) else "ar"
        if run_type is None:
            run_type, buf = t, [c]
        elif run_type == t:
            buf.append(c)
        else:
            tokens.append((run_type, "".join(buf)))
            run_type, buf = t, [c]
    if buf:
        tokens.append((run_type, "".join(buf)))
    return tokens


def _latin_tokens_bad(s):
    """True if a mixed Arabic/Latin string carries two consecutive Latin runs
    (the runtime shim would word-reverse them). Pure-Latin strings never reach
    the shim, so they are safe."""
    if not re.search(r"[\u0600-\u06FF]", s):
        return False
    toks = _script_tokens(s)
    for i in range(len(toks) - 1):
        if toks[i][0] == "lat" and toks[i + 1][0] == "lat":
            return True
    if re.search(r"\(\s*[0-9][0-9.,%/-]*\s+[\u0600-\u06FF]", s):
        return True
    return False


def check_entry(qid, entry):
    problems = []
    if entry.get("source", "curriculum") != "curriculum":
        problems.append(qid + ": source is not \"curriculum\"")
    if entry["cat"] not in CATEGORIES:
        problems.append(qid + ": unknown category")
    if entry["diff"] not in (1, 2, 3):
        problems.append(qid + ": difficulty not 1/2/3")
    fields = [("q", entry["q"]), ("e", entry["e"]), ("qa", entry["qa"]),
              ("ea", entry["ea"])]
    fields += [("o%d" % i, o) for i, o in enumerate(entry["o"])]
    fields += [("oa%d" % i, o) for i, o in enumerate(entry["oa"])]
    for kind, value in fields:
        if value != value.strip() or not value:
            problems.append(qid + "/" + kind + ": empty or leading/trailing whitespace")
        if "\\" in value or "\n" in value or "\t" in value:
            problems.append(qid + "/" + kind + ": YAML-hostile character")
    # EN strings may carry "quotes" (y() emits them single-quoted, exactly like
    # the existing bank's Lan_001 question); Arabic fields use guillemets «».
    for kind, value in [("qa", entry["qa"]), ("ea", entry["ea"])] + [
            ("oa%d" % i, o) for i, o in enumerate(entry["oa"])]:
        if '"' in value:
            problems.append(qid + "/" + kind + ': use guillemets «», not "')
    for kind, value in [("qa", entry["qa"]), ("ea", entry["ea"])] + [
            ("oa%d" % i, o) for i, o in enumerate(entry["oa"])]:
        if _latin_tokens_bad(value):
            problems.append(qid + "/" + kind + ": two Latin tokens in a row "
                            "(would display word-reversed): " + value)
    for kind, value in [("qa", entry["qa"]), ("ea", entry["ea"])]:
        if not re.search(r"[\u0600-\u06FF]", value):
            problems.append(qid + "/" + kind + ": no Arabic characters")
    if len(entry["o"]) != 4 or len(entry["oa"]) != 4:
        problems.append(qid + ": options list is not 4 long")
    if entry["ci"] < 0 or entry["ci"] > 3:
        problems.append(qid + ": correctIndex out of range")
    if not entry["o"][entry["ci"]] or not entry["oa"][entry["ci"]]:
        problems.append(qid + ": correct option is empty")
    return problems


def check_wings():
    """Every wing exactly 12 questions, 4 easy / 5 standard / 3 hard."""
    problems = []
    wings = {}
    for qid, entry in E.items():
        wings.setdefault(entry["wing"], []).append((qid, entry["diff"]))
    expected_wings = ["Library", "Science wing", "Courtyard", "Assembly hall",
                      "Staff room"]
    for wing in expected_wings:
        if wing not in wings:
            problems.append("wing missing: " + wing)
            continue
        ids = wings[wing]
        if len(ids) != 12:
            problems.append(wing + ": " + str(len(ids)) + " questions (expected 12)")
        for diff, want in ((1, 4), (2, 5), (3, 3)):
            got = sum(1 for _, d in ids if d == diff)
            if got != want:
                problems.append(wing + ": difficulty " + str(diff) + " count "
                                + str(got) + " (expected " + str(want) + ")")
    for wing in wings:
        if wing not in expected_wings:
            problems.append("unknown wing: " + wing)
    return problems


def check_numbering(qdir):
    """New ids must continue each category's numbering with no gaps, and no
    target file may already exist (append-only work)."""
    problems = []
    existing = {}
    for f in sorted(os.listdir(qdir)):
        m = re.match(r"^Q_([A-Za-z]+)_(\d{3})\.asset$", f)
        if m:
            existing.setdefault(m.group(1), set()).add(int(m.group(2)))
    for cat in CATEGORIES:
        have = existing.get(cat, set())
        expected_start = EXISTING_PER_CATEGORY + 1
        new_nums = sorted(int(qid.split("_")[1]) for qid in E
                          if E[qid]["cat"] == cat)
        if not new_nums:
            problems.append(cat + ": no new questions")
            continue
        if new_nums[0] != expected_start:
            problems.append(cat + ": numbering does not continue at "
                            + str(expected_start))
        if new_nums != list(range(new_nums[0], new_nums[0] + len(new_nums))):
            problems.append(cat + ": numbering has gaps")
        overlap = [n for n in new_nums if n in have]
        if overlap:
            problems.append(cat + ": id already on disk: " + str(overlap))
    return problems


def check_duplicate_prompts(qdir):
    """No new EN prompt may duplicate any existing one (case-normalised)."""
    problems = []
    seen = {}
    for f in sorted(os.listdir(qdir)):
        if not (f.startswith("Q_") and f.endswith(".asset")):
            continue
        with open(os.path.join(qdir, f), "r", encoding="utf-8", newline="") as fh:
            text = fh.read()
        m = re.search(r"^  question: (.+)$", text, re.M)
        if m:
            seen.setdefault(m.group(1).strip().lower(), []).append(f)
    for qid, entry in E.items():
        key = entry["q"].strip().lower()
        if key in seen:
            problems.append(qid + ": EN prompt duplicates " + ", ".join(seen[key]))
        seen.setdefault(key, []).append(qid)
    return problems


# -------------------------------------------------------------------- YAML --
def y(s):
    """Emit a scalar the way Unity's YAML writer would: plain when safe,
    single-quoted (with '' doubling) otherwise."""
    if s == "":
        return '""'
    unsafe = (
        "\n" in s or "\t" in s
        or s != s.strip()
        or ": " in s or " #" in s
        or '"' in s or "\\" in s
        or s[0] in "-?:,[]{}#&*!|>'\"%@`"
    )
    if unsafe:
        return "'" + s.replace("'", "''") + "'"
    return s


def build_asset(name, entry):
    lines = [
        "%YAML 1.1",
        "%TAG !u! tag:unity3d.com,2011:",
        "--- !u!114 &11400000",
        "MonoBehaviour:",
        "  m_ObjectHideFlags: 0",
        "  m_CorrespondingSourceObject: {fileID: 0}",
        "  m_PrefabInstance: {fileID: 0}",
        "  m_PrefabAsset: {fileID: 0}",
        "  m_GameObject: {fileID: 0}",
        "  m_Enabled: 1",
        "  m_EditorHideFlags: 0",
        "  m_Script: {fileID: 11500000, guid: d2cb65e0913b0784b98ecec4a74e5c1f, type: 3}",
        "  m_Name: " + name,
        "  m_EditorClassIdentifier: Assembly-CSharp::LightShadow.UI.QuestionData",
        "  question: " + y(entry["q"]),
        "  options:",
    ]
    lines += ["  - " + y(o) for o in entry["o"]]
    lines += [
        "  correctIndex: " + str(entry["ci"]),
        "  category: " + str(CATEGORIES[entry["cat"]]),
        "  difficulty: " + str(entry["diff"]),
        "  explanation: " + y(entry["e"]),
        "  factSource: " + FACT_SOURCE,
        "  questionAr: " + y(entry["qa"]),
        "  optionsAr:",
    ]
    lines += ["  - " + y(o) for o in entry["oa"]]
    lines += ["  explanationAr: " + y(entry["ea"]), ""]
    return "\n".join(lines)


# --------------------------------------------------------------- verifying --
class UnityLoader(yaml.SafeLoader):
    pass


UnityLoader.add_multi_constructor(
    "tag:unity3d.com,2011:", lambda l, sfx, node: l.construct_mapping(node, deep=True))


def verify_written(path, asset_name, entry):
    with open(path, "r", encoding="utf-8", newline="") as fh:
        text = fh.read()
    doc = yaml.load(text, Loader=UnityLoader)
    doc = doc["MonoBehaviour"]
    if doc["m_Name"] != asset_name:
        raise RuntimeError(asset_name + ": m_Name mismatch")
    if doc["question"] != entry["q"]:
        raise RuntimeError(asset_name + ": question round-trip mismatch")
    if [str(x) for x in doc["options"]] != list(entry["o"]):
        raise RuntimeError(asset_name + ": options round-trip mismatch")
    if int(doc["correctIndex"]) != entry["ci"]:
        raise RuntimeError(asset_name + ": correctIndex mismatch")
    if int(doc["category"]) != CATEGORIES[entry["cat"]]:
        raise RuntimeError(asset_name + ": category mismatch")
    if int(doc["difficulty"]) != entry["diff"]:
        raise RuntimeError(asset_name + ": difficulty mismatch")
    if doc["explanation"] != entry["e"]:
        raise RuntimeError(asset_name + ": explanation round-trip mismatch")
    if doc["factSource"] != FACT_SOURCE:
        raise RuntimeError(asset_name + ": factSource wrong")
    if doc["questionAr"] != entry["qa"]:
        raise RuntimeError(asset_name + ": questionAr round-trip mismatch")
    if [str(x) for x in doc["optionsAr"]] != list(entry["oa"]):
        raise RuntimeError(asset_name + ": optionsAr round-trip mismatch")
    if doc["explanationAr"] != entry["ea"]:
        raise RuntimeError(asset_name + ": explanationAr round-trip mismatch")
    for field in ("question", "explanation", "questionAr", "explanationAr"):
        if not str(doc[field]).strip():
            raise RuntimeError(asset_name + ": " + field + " empty")
    for field in ("options", "optionsAr"):
        if len(doc[field]) != 4 or not all(str(x).strip() for x in doc[field]):
            raise RuntimeError(asset_name + ": " + field + " not 4 non-empty")


def main():
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")
    apply_mode = "--apply" in sys.argv
    root = os.path.dirname(os.path.abspath(__file__))
    while not os.path.isdir(os.path.join(root, QUESTIONS_REL)):
        parent = os.path.dirname(root)
        if parent == root:
            raise RuntimeError("Could not locate the LIGHT SHADOW repo root")
        root = parent
    qdir = os.path.join(root, QUESTIONS_REL)

    problems = []
    for qid in sorted(E):
        problems += check_entry(qid, E[qid])
    problems += check_wings()
    problems += check_numbering(qdir)
    problems += check_duplicate_prompts(qdir)

    if problems:
        print("CONTENT PROBLEMS (nothing written):")
        for p in problems:
            print("  - " + p)
        sys.exit(1)

    if not apply_mode:
        print("Checked " + str(len(E)) + " new questions: table, wings, "
              "numbering and display gates all clean. "
              "Re-run with --apply to write the files.")
        for qid in sorted(E):
            print("  would create: Q_" + qid + ".asset (wing="
                  + E[qid]["wing"] + ", diff=" + str(E[qid]["diff"]) + ")")
        return

    created = 0
    for qid in sorted(E):
        asset_name = "Q_" + qid
        path = os.path.join(qdir, asset_name + ".asset")
        if os.path.exists(path):
            raise RuntimeError(path + " already exists - refusing (append-only)")
        text = build_asset(asset_name, E[qid])
        with open(path, "w", encoding="utf-8", newline="") as fh:
            fh.write(text)
        verify_written(path, asset_name, E[qid])
        created += 1

    total = len([f for f in os.listdir(qdir)
                 if f.startswith("Q_") and f.endswith(".asset")])
    print("Created " + str(created) + " new question assets; every file "
          "re-parsed with YAML and round-tripped (EN+AR fields, 4 options, "
          "correctIndex, category, difficulty).")
    print("Question folder now holds " + str(total) + " Q_*.asset files.")
    print("No .meta files written (Unity generates them on import); "
          "QuestionDatabase.asset left for Unity's GenerateQuestionsSilent "
          "rebuild on the next editor run.")


if __name__ == "__main__":
    main()
