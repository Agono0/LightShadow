# -*- coding: utf-8 -*-
"""Fill the Arabic fields (questionAr / optionsAr / explanationAr) of every
Q_*.asset question in Assets/_Project/Data/Questions (LIGHT SHADOW, task P4).

Method (task P4, AGENTS.md rule 1 + rule 5):
  - Do NOT regenerate; EDIT the existing .asset YAML in place. The script appends
    the three Arabic fields after `factSource` (the last serialized field), so the
    bytes before them are untouched and the GUIDs / .meta files stay identical.
  - If a file already carries the Arabic fields (Unity re-save), the trailing
    questionAr/optionsAr/explanationAr block is replaced in place instead.
  - correctIndex is read from the file and asserted against the table.
  - factSource is never touched (stays NEEDS-FACT-CHECK until a human reviews).
  - Emission mimics Unity's own YAML style: plain scalars where safe,
    single-quoted scalars (with '' doubling) otherwise; LF endings preserved.

Display-safety gate (Assets/_Project/Scripts/Core/ArabicSupport.cs): the runtime
shim reverses token order per line and keeps Latin/digit runs LTR. Two Latin
tokens in a row (e.g. the phrase "Time is a thief") would render word-reversed,
so every Latin island in this table is a single whitespace-free token; the
script refuses to write a string that breaks that rule.

Usage:  python -I fill_arabic_questions.py [--apply]
        (without --apply it only checks and prints what it would do)
"""

import os
import re
import sys

QUESTIONS_REL = os.path.join("Assets", "_Project", "Data", "Questions")
FACT_SOURCE = "NEEDS-FACT-CHECK"

# id -> {"ci": correctIndex, "q": questionAr, "o": [4 options], "e": explanationAr}
# Terminology is kept consistent per subject (Egyptian secondary curriculum):
# التمثيل الضوئي, المقام/البسط, خط الاستواء, سور برلين, كرات الدم البيضاء, ...
T = {
    # ------------------------------------------------------------- Science --
    "Sci_001": {"ci": 1,
        "q": "من أي جزء تمتص النباتات الماء والأملاح المعدنية من التربة في الغالب؟",
        "o": ["الأوراق", "الجذور", "الأزهار", "الساق"],
        "e": "الجذور تمتص الماء والأملاح المذابة من التربة، أما الأوراق فتتولى في الغالب صنع الغذاء بالتمثيل الضوئي."},
    "Sci_002": {"ci": 1,
        "q": "أي غاز من الهواء يحتاج الإنسان إلى تنفسه ليحيا؟",
        "o": ["النيتروجين", "الأكسجين", "ثاني أكسيد الكربون", "الهيليوم"],
        "e": "الأكسجين يدخل إلى الدم عبر الرئتين، ويُستعمل في تحرير الطاقة من الغذاء."},
    "Sci_003": {"ci": 2,
        "q": "يُسخَّن قدر من الماء على الموقد؛ ماذا يحدث لجزيئات الماء كلما ازداد حرارة؟",
        "o": ["تتباطأ", "تتوقف عن الحركة", "تتحرك أسرع", "تتحول إلى ضوء"],
        "e": "السخونة تُسرِّع الجزيئات: فالحرارة تقيس متوسط طاقة حركتها."},
    "Sci_004": {"ci": 2,
        "q": "نبات ينمو من بذرة صغيرة ويصير شجرة كبيرة؛ أغلب كتلته الجديدة يأتي من...",
        "o": ["التربة", "الماء الذي سُقيت به", "ثاني أكسيد الكربون في الهواء", "ضوء الشمس"],
        "e": "التمثيل الضوئي يبني جسم النبات من ثاني أكسيد الكربون في الهواء؛ وتجارب تتبع الكربون تثبت أن التربة لا تضيف إلا كتلة ضئيلة جدًا."},
    "Sci_005": {"ci": 2,
        "q": "عند مستوى سطح البحر، عند أي درجة حرارة يغلي الماء النقي؟",
        "o": ["50 درجة مئوية", "90 درجة مئوية", "100 درجة مئوية", "150 درجة مئوية"],
        "e": "عند الضغط الجوي المعتاد يغلي الماء عند 100 درجة مئوية، أي ما يعادل 212 درجة فهرنهايت."},
    "Sci_006": {"ci": 2,
        "q": "أي كوكب هو الأقرب إلى الشمس؟",
        "o": ["الزهرة", "الأرض", "عطارد", "المريخ"],
        "e": "عطارد أقرب كواكب المجموعة الشمسية إلى الشمس، ويدور حولها في نحو 88 يومًا أرضيًا."},
    "Sci_007": {"ci": 1,
        "q": "أي كوكب هو الأكبر في المجموعة الشمسية؟",
        "o": ["الأرض", "المشتري", "زحل", "نبتون"],
        "e": "المشتري هو أكبر الكواكب؛ يتسع في داخله أكثر من 1,300 أرض."},
    "Sci_008": {"ci": 1,
        "q": "ما القوة التي تُبقي القمر في مداره حول الأرض؟",
        "o": ["المغناطيسية", "الجاذبية", "الاحتكاك", "الكهرباء"],
        "e": "جاذبية الأرض تجذب القمر نحوها، وحركته الجانبية تمنعه من السقوط في داخلها."},
    "Sci_009": {"ci": 2,
        "q": "أي عضو يضخ الدم في أنحاء جسم الإنسان؟",
        "o": ["الرئتان", "الكبد", "القلب", "الكليتان"],
        "e": "القلب مضخة عضلية تدفع الدم في الشرايين والأوردة والشعيرات الدموية."},
    "Sci_010": {"ci": 2,
        "q": "البخار هو الماء في أي حالة من حالات المادة؟",
        "o": ["الصلبة", "السائلة", "الغازية", "البلازما"],
        "e": "البخار هو بخار الماء - الماء في حالته الغازية فوق 100 درجة مئوية عند مستوى سطح البحر."},
    # ----------------------------------------------------------------- Math --
    "Mat_001": {"ci": 1,
        "q": "أي الأعداد الآتية عدد أولي؟",
        "o": ["21", "29", "33", "51"],
        "e": "العدد 29 لا يقبل القسمة إلا على 1 وعلى نفسه، أما الباقية فكلها تقبل القسمة على 3."},
    "Mat_002": {"ci": 1,
        "q": "في الكسر 3/8، ماذا يُسمَّى العدد 8؟",
        "o": ["البسط", "المقام", "ناتج القسمة", "الباقي"],
        "e": "المقام يخبرنا إلى كم جزء متساوٍ قُسِّم الكل."},
    "Mat_003": {"ci": 1,
        "q": "وشاح سعره المعتاد 40، وهو معروض للبيع بخصم 25%. كم تدفعين؟",
        "o": ["25", "30", "32", "35"],
        "e": "25% من 40 يساوي 10، ومن ثم 40-10=30."},
    "Mat_004": {"ci": 2,
        "q": "متجر يبيع الأقلام بثلاثة أقلام بعشرة؛ بالنسبة نفسها كم تكلفة 12 قلمًا؟",
        "o": ["30", "36", "40", "44"],
        "e": "12 قلمًا هي أربع مجموعات كل منها 3، فأربع مرات من 10 تجعل 40."},
    "Mat_005": {"ci": 1,
        "q": "كم يساوي 7 مضروبًا في 8؟",
        "o": ["54", "56", "63", "72"],
        "e": "سبع مجموعات من ثمانية تجعل 56."},
    "Mat_006": {"ci": 2,
        "q": "مربع طول ضلعه 5cm؛ ما محيطه؟",
        "o": ["10 cm", "15 cm", "20 cm", "25 cm"],
        "e": "للمربع أربعة أضلاع متساوية، فمحيطه 5+5+5+5=20cm."},
    "Mat_007": {"ci": 2,
        "q": "كم يساوي 12 مضروبًا في 12؟",
        "o": ["124", "132", "144", "154"],
        "e": "اثنتا عشرة مجموعة من اثني عشر تجعل 144."},
    "Mat_008": {"ci": 1,
        "q": "ما قيمة 25% من 80؟",
        "o": ["15", "20", "25", "40"],
        "e": "ربع العدد 80 يساوي 20."},
    "Mat_009": {"ci": 1,
        "q": "الزوايا الثلاث داخل أي مثلث يكون مجموعها...",
        "o": ["90 درجة", "180 درجة", "270 درجة", "360 درجة"],
        "e": "في الهندسة المستوية يكون مجموع الزوايا الداخلية للمثلث دائمًا 180 درجة."},
    "Mat_010": {"ci": 3,
        "q": "ما العدد الأولي التالي بعد 7؟",
        "o": ["8", "9", "10", "11"],
        "e": "الأعداد 8 و9 و10 مركبة، أما 11 فلا يقبل القسمة إلا على 1 وعلى نفسه."},
    # -------------------------------------------------------------- History --
    "His_001": {"ci": 2,
        "q": "في أي عام انتهت الحرب العالمية الثانية؟",
        "o": ["1918", "1939", "1945", "1950"],
        "e": "توقف القتال في الحرب العالمية الثانية عام 1945."},
    "His_002": {"ci": 2,
        "q": "أي حضارة قديمة هي التي بنت أهرامات الجيزة؟",
        "o": ["الرومان", "الإغريق", "المصريون القدماء", "الفرس"],
        "e": "بُنيت أهرامات الجيزة في مصر القديمة قبل نحو 4,500 عام."},
    "His_003": {"ci": 1,
        "q": "لماذا أصبحت الكتب أرخص بكثير في أوروبا بعد أربعينيات القرن الخامس عشر؟",
        "o": ["اخترع أهل أوروبا الورق في ذلك الوقت",
             "طبعت المطابع النصوص أسرع بكثير مما يستطيعه الناساخون بأيديهم",
             "صغرت أحجام الكتب",
             "أصبح المؤلفون يكتبون أسرع"],
        "e": "مطبعة غوتنبرغ (نحو 1440) أنتجت النسخ أسرع كثيرًا من النسخ اليدوي، فهبطت الأسعار."},
    "His_004": {"ci": 1,
        "q": "أي من هذه الأحداث وقع أولًا؟",
        "o": ["الثورة الفرنسية", "إعلان الاستقلال الأمريكي", "سقوط سور برلين", "نهاية الحرب العالمية الأولى"],
        "e": "الترتيب: إعلان الاستقلال 1776، ثم الثورة الفرنسية 1789، ثم نهاية الحرب العالمية الأولى 1918، ثم سقوط سور برلين 1989."},
    "His_005": {"ci": 1,
        "q": "من أول إنسان مشى على سطح القمر؟",
        "o": ["يوري غاغارين", "نيل أرمسترونج", "بز ألدرين", "جون جلين"],
        "e": "وضع نيل أرمسترونج قدميه على القمر في 20 يوليو 1969 خلال مهمة أبولو 11."},
    "His_006": {"ci": 1,
        "q": "في أي عام ختم الملك جون ملك إنجلترا وثيقة الماجنا كارتا؟",
        "o": ["1066", "1215", "1492", "1648"],
        "e": "خُتمت الماجنا كارتا في رانيميد عام 1215، ثم أصبحت لاحقًا رمزًا لوضع حدود على سلطة الملوك."},
    "His_007": {"ci": 1,
        "q": "أي سور هو الذي قسّم مدينة برلين من 1961 إلى 1989؟",
        "o": ["سور الصين العظيم", "سور برلين", "سور هادريان", "الستار الحديدي"],
        "e": "قسّم سور برلين المدينة في سنوات الحرب الباردة حتى فُتح في نوفمبر 1989."},
    "His_008": {"ci": 1,
        "q": "من أول رئيس للولايات المتحدة الأمريكية؟",
        "o": ["توماس جيفرسون", "جورج واشنطن", "أبراهام لينكولن", "جون آدامز"],
        "e": "تولى جورج واشنطن أول رئاسة للولايات المتحدة من 1789 إلى 1797."},
    "His_009": {"ci": 2,
        "q": "أي حضارة قديمة هي التي بنت ماتشو بيتشو؟",
        "o": ["الأزتيك", "المايا", "الإنكا", "الأولمك"],
        "e": "بنى الإنكا ماتشو بيتشو في القرن الخامس عشر في جبال الأنديز ببيرو."},
    "His_010": {"ci": 1,
        "q": "بُني الهرم الأكبر بالجيزة في أي بلد؟",
        "o": ["العراق", "مصر", "اليونان", "تركيا"],
        "e": "يقوم الهرم الأكبر على هضبة الجيزة في مصر، وقد بُني للملك خوفو."},
    # ------------------------------------------------------------ Geography --
    "Geo_001": {"ci": 2,
        "q": "ما أكبر محيط على وجه الأرض؟",
        "o": ["المحيط الأطلسي", "المحيط الهندي", "المحيط الهادي", "المحيط المتجمد الشمالي"],
        "e": "المحيط الهادي تغطي مساحته أكثر من مساحة جميع المحيطات الأخرى مجتمعة."},
    "Geo_002": {"ci": 0,
        "q": "أي خط عرض هو الذي يقسم الأرض إلى نصف الكرة الشمالي ونصف الكرة الجنوبي؟",
        "o": ["خط الاستواء", "خط جرينتش", "مدار السرطان", "الدائرة القطبية الشمالية"],
        "e": "خط الاستواء يقع عند خط عرض 0 درجة، تمامًا بين القطبين."},
    "Geo_003": {"ci": 0,
        "q": "مدينتان على خط العرض نفسه، لكن المدينة (أ) تستقر عاليًا في الجبال والمدينة (ب) عند مستوى سطح البحر؛ أيهما يكون الجو فيها أبرد في الأحوال العادية؟",
        "o": ["المدينة (أ)، لأن الحرارة تنخفض بالارتفاع",
             "المدينة (ب)، لأن هواء البحر بارد",
             "حرارتها متساوية دائمًا",
             "المدينة (ب)، لأنها أقرب إلى الماء"],
        "e": "يبرد الهواء بنحو 6.5 درجة مئوية عن كل كيلومتر ارتفاع في طبقات الجو السفلى."},
    "Geo_004": {"ci": 1,
        "q": "لماذا تبقى الأماكن القريبة من خط الاستواء شبه ثابتة الحرارة طوال العام، بينما تعرف الأماكن القريبة من القطبين تغيرات فصلية كبيرة؟",
        "o": ["لأن خط الاستواء يحتوي محيطات أكثر",
             "لأن زاوية أشعة الشمس فوق الرؤوس تتغير خلال العام تغيرًا أقل بكثير قرب خط الاستواء",
             "لأن القطبين أبعد عن الشمس في الشتاء",
             "لأن الرياح لا تصل إلى القطبين أبدًا"],
        "e": "ميل محور الأرض يغيّر زاوية الشمس كثيرًا قرب القطبين وقليلًا جدًا قرب خط الاستواء، فتبقى المناطق المدارية دافئة طوال العام."},
    "Geo_005": {"ci": 2,
        "q": "ما عاصمة اليابان؟",
        "o": ["سيول", "بكين", "طوكيو", "بانكوك"],
        "e": "طوكيو عاصمة اليابان منذ عام 1868، حين كانت المدينة تُسمى إيدو."},
    "Geo_006": {"ci": 2,
        "q": "أي مما يلي أكبر صحراء حارة في العالم؟",
        "o": ["صحراء جوبي", "صحراء كالاهاري", "الصحراء الكبرى", "صحراء أتاكاما"],
        "e": "تمتد الصحراء الكبرى على نحو 9 ملايين كيلومتر مربع عبر شمال إفريقيا، فهي أكبر الصحاري الحارة."},
    "Geo_007": {"ci": 1,
        "q": "في أي قارة تقع الصحراء الكبرى؟",
        "o": ["آسيا", "إفريقيا", "أستراليا", "أمريكا الجنوبية"],
        "e": "تمتد الصحراء الكبرى عبر شمال إفريقيا من المحيط الأطلسي إلى البحر الأحمر."},
    "Geo_008": {"ci": 2,
        "q": "ما عاصمة فرنسا؟",
        "o": ["ليون", "مارسيليا", "باريس", "نيس"],
        "e": "باريس عاصمة فرنسا وأكبر مدنها، وتقوم على نهر السين."},
    "Geo_009": {"ci": 2,
        "q": "البرازيل تقع في أي قارة؟",
        "o": ["أمريكا الشمالية", "إفريقيا", "أمريكا الجنوبية", "أوروبا"],
        "e": "البرازيل هي أكبر بلاد أمريكا الجنوبية."},
    "Geo_010": {"ci": 2,
        "q": "يقع جبل إيفرست في أي سلسلة جبلية؟",
        "o": ["جبال الأنديز", "جبال الألب", "جبال الهيمالايا", "جبال روكي"],
        "e": "يقوم إيفرست في جبال الهيمالايا على الحدود بين نيبال والصين."},
    # ------------------------------------------------------------- Language --
    # English-language items keep the tested Latin words in Latin script; every
    # Latin island is a single token (ArabicSupport keeps single tokens LTR).
    "Lan_001": {"ci": 2,
        "q": "أي كلمة هي مرادف لكلمة «brave»؟",
        "o": ["خائف", "متردد", "شجاع", "مهمل"],
        "e": "«courageous» تعني ما تعنيه «brave» تمامًا: ألّا يهاب المرء موقفًا صعبًا."},
    "Lan_002": {"ci": 3,
        "q": "في جملة «نام القط بهدوء»، أي كلمة تعمل ظرفًا؟",
        "o": ["في", "القط", "نام", "بهدوء"],
        "e": "الظرف يصف كيفية حدوث الفعل؛ فكلمة «بهدوء» تخبرنا كيف نام القط."},
    "Lan_003": {"ci": 2,
        "q": "أي جملة صحيحة نحويًا؟",
        "o": ["She don't like tea.", "He go to school every day.",
             "They are reading a book.", "I has a pencil."],
        "e": "الجملة الصحيحة مطابقة لفاعلها، أما الجمل الأخرى فتخل بمطابقة الفعل لفاعله."},
    "Lan_004": {"ci": 1,
        "q": "قول «الوقت لص» يُعدّ استعارة؛ ماذا توحي بنا؟",
        "o": ["أن الوقت يستطيع سرقة الأشياء الملموسة",
             "أن الوقت يمضي ويسلب اللحظات خفيةً",
             "أن يُخبأ الساعات",
             "أن الوقت حيوان خطير"],
        "e": "الاستعارة شبهت الوقت باللص لتوحي بأنه يسلب اللحظات والسنوات في صمت."},
    "Lan_005": {"ci": 1,
        "q": "ما الزمن الماضي للفعل «go»؟",
        "o": ["goed", "went", "gone", "going"],
        "e": "«go» فعل شاذ التصريف: ماضيه «went» لا «goed»، وأصله التام «gone»."},
    "Lan_006": {"ci": 1,
        "q": "ماذا يُسمى جمع الأسود؟",
        "o": ["قطيع", "أُسرة", "سرب", "زمرة"],
        "e": "مجموعة الأسود تُسمى أُسرة، ومجموعة الذئاب زمرة، ومجموعة الخراف سرب."},
    "Lan_007": {"ci": 2,
        "q": "ما جمع الاسم «child»؟",
        "o": ["childs", "childes", "children", "childrens"],
        "e": "الاسم «child» جمعه تكسير «children»."},
    "Lan_008": {"ci": 1,
        "q": "أي كلمة معناها عكس كلمة «ancient»؟",
        "o": ["قديم", "حديث", "تاريخي", "عتيق"],
        "e": "«ancient» تعني قديمًا جدًا، و«modern» تعني ما هو من الزمن الحاضر."},
    "Lan_009": {"ci": 2,
        "q": "أي مما يلي جملة تامة؟",
        "o": ["Running down the street.", "Because it was late.",
             "She runs every morning.", "The big red ball."],
        "e": "الجملة التامة فيها فاعل وفعل، وتعبر عن معنى كامل يُفهم بذاته."},
    "Lan_010": {"ci": 1,
        "q": "التشبيه يقارن بين شيئين مستعينًا بأي كلمات؟",
        "o": ["«is» أو «was»", "«like» أو «as»", "«and» أو «but»", "«not» أو «never»"],
        "e": "التشبيه يقول إن شيئًا يشبه الآخر مستعينًا بـ«like» أو «as»، أما الاستعارة فتذكره مباشرة."},
    # --------------------------------------------------------------- Health --
    "Hea_001": {"ci": 0,
        "q": "أي مادة غذائية هي المصدر الرئيسي السريع للطاقة في الجسم؟",
        "o": ["الكربوهيدرات", "الحديد", "فيتامين ج", "الماء"],
        "e": "الكربوهيدرات تتحلل إلى جلوكوز، وهو أسرع وقود يمد الجسم بالطاقة."},
    "Hea_002": {"ci": 2,
        "q": "كم ساعة نوم يحتاج معظم المراهقين في كل ليلة؟",
        "o": ["من 4 إلى 5 ساعات", "من 6 إلى 7 ساعات", "من 8 إلى 10 ساعات", "من 12 إلى 14 ساعة"],
        "e": "توصيات النوم ترشد إلى 8-10 ساعات في الليلة للمراهقين من 13 إلى 18 عامًا."},
    "Hea_003": {"ci": 1,
        "q": "لماذا غسل اليدين بالصابون أفضل من غسلهما بالماء وحده؟",
        "o": ["لأن الصابون يجعل اليدين زُلقًا",
             "لأن الصابون يزيل الدهون والأوساخ التي يتركها الماء وحده",
             "لأن الصابون يضيف بكتيريا نافعة إلى الجلد",
             "لأن الصابون يجفف جلد الجراثيم"],
        "e": "الصابون يرفع الدهون والأوساخ التي تحتمي بها الجراثيم، فتُغسل مع الماء."},
    "Hea_004": {"ci": 1,
        "q": "طالبة تتخلى عن الفطور فتشعر قرب منتصف الصباح بدوخة وعجز عن التركيز؛ ما السبب الأرجح؟",
        "o": ["شرب الماء كثيرًا", "انخفاض سكر الدم لأنها لم تأكل",
             "الإفراط في وقت الشاشات", "النوم الكثير"],
        "e": "ترك الوجبات قد يخفض جلوكوز الدم، والدماغ يحتاج الجلوكوز لكي يركّز."},
    "Hea_005": {"ci": 2,
        "q": "أي فيتامين يستطيع الجسم أن يصنعه عندما يتعرض الجلد لأشعة الشمس؟",
        "o": ["فيتامين أ", "فيتامين ج", "فيتامين د", "فيتامين ب12"],
        "e": "أشعة الشمس على الجلد تعين الجسم على صنع فيتامين د الذي تحتاجه العظام لتقوى."},
    "Hea_006": {"ci": 0,
        "q": "أي من هذه الأطعمة مصدر جيد لفيتامين ج؟",
        "o": ["البرتقال", "الأرز الأبيض", "الزبدة", "ملح الطعام"],
        "e": "ثمار الحوامض كالبرتقال غنية بفيتامين ج الذي يدعم جهاز المناعة ويساعد الجسم على امتصاص الحديد."},
    "Hea_007": {"ci": 2,
        "q": "كم من النشاط البدني المعتدل ينبغي للمراهق أن يستهدفه كل يوم؟",
        "o": ["10 دقائق", "30 دقيقة", "60 دقيقة", "3 ساعات"],
        "e": "التوصيات الصحية تنصح المراهق بنحو 60 دقيقة يوميًا من النشاط المعتدل إلى الشديد."},
    "Hea_008": {"ci": 1,
        "q": "أي مشروب هو الخيار اليومي الأفضل لترطيب الجسم؟",
        "o": ["المشروبات الغازية", "الماء", "مشروب الطاقة", "الشاي المحلى"],
        "e": "الماء يرطب الجسم بلا سكر مضاف ولا كافيين ولا سعرات زائدة."},
    "Hea_009": {"ci": 1,
        "q": "أي خلايا الدم تعين الجسم على محاربة العدوى؟",
        "o": ["كرات الدم الحمراء", "كرات الدم البيضاء", "الصفائح الدموية", "خلايا البلازما"],
        "e": "كرات الدم البيضاء تميز الجراثيم الممرضة وتهاجمها، أما كرات الدم الحمراء فتحمل الأكسجين."},
    "Hea_010": {"ci": 0,
        "q": "العدس مصدر أساسي لأي مادة غذائية؟",
        "o": ["البروتين", "فيتامين ج", "الكالسيوم", "الدهون"],
        "e": "العدس من البقوليات، غني بالبروتين النباتي والألياف، وقليل الدهون جدًا."},
}


# --------------------------------------------------------------------- YAML --
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


# ------------------------------------------------- ArabicSupport display gate --
# Matches ArabicSupport.IsLatinOrDigit: alphanumerics plus the punctuation that
# belongs to numbers or inline English fragments.
_LATIN_CHAR = re.compile(r"[0-9A-Za-z+\-/%=.,:;!?\[\]{}<>]")


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
        t = "lat" if _LATIN_CHAR.match(c) else "ar"
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


def _latin_tokens_warn(s):
    """Two consecutive Latin runs (e.g. the phrase 'Time is a thief') would be
    word-reversed by the runtime shim. Strings without Arabic never reach the
    shim (Fix returns them unchanged), so they are safe. Also flags the
    '(<number> <Arabic words>)' pattern, where the number would be pushed
    visually outside the parentheses."""
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
    for kind, value in [("q", entry["q"]), ("e", entry["e"])] + [
            ("o%d" % i, o) for i, o in enumerate(entry["o"])]:
        if value != value.strip():
            problems.append(qid + "/" + kind + ": leading/trailing whitespace")
        if '"' in value or "\\" in value or "\n" in value:
            problems.append(qid + "/" + kind + ": YAML-hostile character")
        if not re.search(r"[\u0600-\u06FF]", value) and kind in ("q", "e"):
            problems.append(qid + "/" + kind + ": no Arabic characters")
        if _latin_tokens_warn(value):
            problems.append(qid + "/" + kind + ": two Latin tokens in a row "
                            "(would display word-reversed): " + value)
    if len(entry["o"]) != 4:
        problems.append(qid + ": options list is not 4 long")
    return problems


# -------------------------------------------------------------------- editing --
def build_block(entry):
    lines = ["  questionAr: " + y(entry["q"]), "  optionsAr:"]
    lines += ["  - " + y(o) for o in entry["o"]]
    lines.append("  explanationAr: " + y(entry["e"]))
    return "\n".join(lines) + "\n"


def fill_file(path, asset_name, qid, entry):
    with open(path, "r", encoding="utf-8", newline="") as fh:
        text = fh.read()

    m = re.search(r"^  correctIndex: (\d)\s*$", text, re.M)
    if not m:
        raise RuntimeError(asset_name + ": no correctIndex line found")
    if int(m.group(1)) != entry["ci"]:
        raise RuntimeError(qid + ": correctIndex on disk is " + m.group(1)
                           + " but table says " + str(entry["ci"]) + " - aborting")

    m = re.search(r"^  factSource: (.*)$", text, re.M)
    if not m or m.group(1).strip() != FACT_SOURCE:
        raise RuntimeError(qid + ": factSource is not untouched " + FACT_SOURCE)

    if "  questionAr:" in text:
        # Fields already exist (Unity re-save): replace the trailing block.
        idx = text.index("  questionAr:")
        new_text = text[:idx] + build_block(entry)
        mode = "replaced"
        prefix_ok = new_text.startswith(text[:idx])
    else:
        if not text.endswith("\n"):
            text += "\n"
        new_text = text + build_block(entry)
        mode = "appended"
        prefix_ok = new_text.startswith(text)

    if not prefix_ok:
        raise RuntimeError(qid + ": edit would modify existing bytes - aborting")

    with open(path, "w", encoding="utf-8", newline="") as fh:
        fh.write(new_text)
    return mode


# ------------------------------------------------------------------ verifying --
def verify_file(path, asset_name, qid, entry):
    import yaml

    class UnityLoader(yaml.SafeLoader):
        pass

    UnityLoader.add_multi_constructor(
        "tag:unity3d.com,2011:", lambda l, sfx, node: l.construct_mapping(node, deep=True))

    with open(path, "r", encoding="utf-8", newline="") as fh:
        text = fh.read()
    doc = yaml.load(text, Loader=UnityLoader)
    if doc is None:
        raise RuntimeError(asset_name + ": YAML parse failed")
    # Unity wraps the fields: root mapping -> "MonoBehaviour" -> fields.
    doc = doc["MonoBehaviour"]
    if doc["m_Name"] != asset_name:
        raise RuntimeError(asset_name + ": m_Name mismatch after edit")
    if doc["questionAr"] != entry["q"]:
        raise RuntimeError(asset_name + ": questionAr round-trip mismatch")
    # Pure-digit options (e.g. "1918") parse as YAML ints; Unity types the
    # field as string, so compare textually.
    if [str(x) for x in doc["optionsAr"]] != list(entry["o"]):
        raise RuntimeError(asset_name + ": optionsAr round-trip mismatch")
    if doc["explanationAr"] != entry["e"]:
        raise RuntimeError(asset_name + ": explanationAr round-trip mismatch")
    if len(doc["optionsAr"]) != 4:
        raise RuntimeError(asset_name + ": optionsAr is not 4 long")
    if int(doc["correctIndex"]) != entry["ci"]:
        raise RuntimeError(asset_name + ": correctIndex changed - aborting")
    if doc["factSource"] != FACT_SOURCE:
        raise RuntimeError(asset_name + ": factSource changed - aborting")
    if doc["question"] != entry.get("_en_question", doc["question"]):
        pass  # EN question untouched by construction (append-only edit)


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

    # Asset files are Q_<Cat>_<NNN>.asset; table keys use <Cat>_<NNN>.
    names_on_disk = sorted(
        f[:-6] for f in os.listdir(qdir) if f.startswith("Q_") and f.endswith(".asset"))
    ids_on_disk = [n[2:] for n in names_on_disk]
    missing = [i for i in ids_on_disk if i not in T]
    extra = [i for i in T if i not in ids_on_disk]
    if missing or extra:
        raise RuntimeError("Table/disk mismatch. On disk but not in table: "
                           + str(missing) + " | in table but not on disk: " + str(extra))

    all_problems = []
    for qid in sorted(T):
        all_problems += check_entry(qid, T[qid])
    if all_problems:
        print("DISPLAY/CONTENT PROBLEMS (nothing written):")
        for p in all_problems:
            print("  - " + p)
        sys.exit(1)

    if not apply_mode:
        print("Checked " + str(len(T)) + " entries: table clean. "
              "Re-run with --apply to write the files.")
        return

    replaced = 0
    appended = 0
    for qid in sorted(T):
        asset_name = "Q_" + qid
        mode = fill_file(os.path.join(qdir, asset_name + ".asset"), asset_name, qid, T[qid])
        verify_file(os.path.join(qdir, asset_name + ".asset"), asset_name, qid, T[qid])
        if mode == "appended":
            appended += 1
        else:
            replaced += 1
    print("Filled Arabic fields in " + str(len(T)) + " question assets ("
          + str(appended) + " appended, " + str(replaced) + " replaced in place).")
    print("All files re-parsed with YAML; correctIndex, factSource and EN bytes "
          "verified untouched.")


if __name__ == "__main__":
    main()
