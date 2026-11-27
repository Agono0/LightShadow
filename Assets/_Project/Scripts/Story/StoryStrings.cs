using System.Collections.Generic;
using LightShadow.Core;

namespace LightShadow.Story
{
    // EN/AR string table for everything the story pass adds (dialogue, endings,
    // save slots, loading screen, accessibility rows, volume rows). Keys are the
    // English sentences themselves - the same convention Core/Localization uses -
    // so English display needs no lookup and Arabic is shaped through the shared
    // path (Localization.Present -> ArabicSupport) with Localization.CurrentFont.
    //
    // Deviation, documented: Localization has no runtime registration API and
    // Core is outside this agent's file scope, so the story keeps its own table
    // here and falls back to Localization.Get for anything it does not carry.
    public static class StoryStrings
    {
        // Display names per speaker enum (also localized).
        public static string SpeakerName(StorySpeaker speaker)
        {
            switch (speaker)
            {
                case StorySpeaker.Nour: return Get("NOUR");
                case StorySpeaker.Mother: return Get("MOTHER");
                case StorySpeaker.Shadow: return Get("THE SHADOW");
                default: return Get("MEMORY");
            }
        }

        // English in, localized text out. Arabic passes through the same shaping
        // path every other string uses (call Localization.Present at display time).
        public static string Get(string english)
        {
            if (string.IsNullOrEmpty(english)) return english ?? "";
            if (Localization.Current != GameLanguage.Arabic) return english;
            return Story.TryGetValue(english, out string arabic) ? arabic : Localization.Get(english);
        }

        // Convenience for UI code: translate, shape, and hand back display text.
        public static string Tr(string english)
        {
            return Localization.Present(Get(english));
        }

        static readonly Dictionary<string, string> Story = new Dictionary<string, string>
        {
            // ---- speaker tags ------------------------------------------------
            { "NOUR", "نور" },
            { "MOTHER", "الأم" },
            { "THE SHADOW", "الظل" },
            { "MEMORY", "ذاكرة" },

            // ---- dialogue UI chrome ------------------------------------------
            { "HOLD TO SKIP", "اضغط مطولًا للتخطي" },
            { "AUTO-ADVANCE", "تقدم تلقائي" },
            { "Choose", "اختر" },

            // ---- save slots --------------------------------------------------
            { "SAVE SLOTS", "خانات الحفظ" },
            { "CONTINUE", "متابعة" },
            { "NEW GAME", "لعبة جديدة" },
            { "LOAD", "تحميل" },
            { "DELETE", "حذف" },
            { "CONFIRM", "تأكيد" },
            { "CANCEL", "إلغاء" },
            { "EMPTY", "فارغة" },
            { "SLOT {0}", "خانة {0}" },
            { "CHAPTER {0}", "الفصل {0}" },
            { "Fragments: {0}/6", "الشذرات: {0}/6" },
            { "Saved {0}", "محفوظ في {0}" },
            { "Overwrite this slot?", "الكتابة فوق هذه الخانة؟" },
            { "Delete this save?", "حذف هذا الحفظ؟" },
            { "No saves yet. The night has not been written.", "لا حفظيات بعد. الليلة لم تُكتب بعد." },
            { "SAVES", "الحفظ" },

            // ---- loading screen ----------------------------------------------
            { "DEMOLITION AT DAWN", "الهدم عند الفجر" },
            { "Loading...", "جارٍ التحميل..." },

            // ---- settings page toggle -----------------------------------------
            { "MORE OPTIONS", "خيارات إضافية" },
            { "BASIC OPTIONS", "خيارات أساسية" },

            // ---- settings: accessibility rows --------------------------------
            { "Text speed", "سرعة النص" },
            { "Slow", "بطيئة" },
            { "Normal", "عادية" },
            { "Fast", "سريعة" },
            { "Instant", "فورية" },
            { "Subtitle size", "حجم الترجمة" },
            { "Small", "صغير" },
            { "Large", "كبير" },
            { "Colourblind palette", "لوحة آمنة لعمى الألوان" },
            { "Standard", "قياسية" },
            { "Blue-Orange", "أزرق-برتقالي" },
            { "High contrast", "تباين عالٍ" },
            { "Auto-advance", "تقدم تلقائي" },
            { "Skip hold time", "مدة الضغط للتخطي" },
            { "Quality tier", "مستوى الجودة" },
            { "Auto", "تلقائي" },
            { "Low", "منخفضة" },
            { "Medium", "متوسطة" },
            { "High", "عالية" },
            { "Ultra", "فائقة" },

            // ---- settings: volume rows ---------------------------------------
            { "Master volume", "الصوت العام" },
            { "Music volume", "الموسيقى" },
            { "Ambience volume", "الصوت المحيط" },
            { "SFX volume", "المؤثرات" },
            { "UI volume", "أصوات الواجهة" },

            // ---- ending + recap ----------------------------------------------
            { "ENDING", "النهاية" },
            { "REPLAY THE NIGHT", "أعد الليلة" },
            { "FULL RECOVERY", "استعادة كاملة" },
            { "PARTIAL MEMORY", "ذاكرة ناقصة" },
            { "HOLLOW ESCAPE", "نجاة جوفاء" },
            { "DAWNLESS", "بلا فجر" },
            { "Full Recovery", "استعادة كاملة" },
            { "Partial Memory", "ذاكرة ناقصة" },
            { "Hollow Escape", "نجاة جوفاء" },
            { "Dawnless", "بلا فجر" },

            { "The gate closes behind her. The sky over the school turns from black to grey to a thin, clean gold.\n\nOn the pavement she opens the box: bookmark, paper, jasmine, note, programme, mug. Six small true things. She reads the lesson plans to the last page - the one line she has been climbing toward all night.\n\nAt a first-floor window the Shadow stands like a student kept after class, and watches the light walk away without her.\n\nKnowledge is the only light that follows a person into the dark. Tonight it followed Nour all the way out.",
              "تُغلق البوابة خلفها. تتحول السماء فوق المدرسة من الأسود إلى الرمادي إلى ذهب رقيق صافٍ.\n\nعلى الرصيف تفتح الصندوق: فاصل الكتاب، ورقة الامتحان، الياسمين، الورقة المثبّتة، البرنامج، الكوب. ستة أشياء صغيرة صادقة. تقرأ خطط الدروس إلى الصفحة الأخيرة - السطر الذي صعدت نحوه الليلة كلها.\n\nفي نافذة الطابق الأول يقف الظل كتلميذ حُبس بعد الحصة، يشاهد النور يغادر دون أن يحمله.\n\nالمعرفة هي النور الوحيد الذي يتبع المرء في العتمة. الليلة رافق نورًا كل الطريق إلى الخارج." },
            { "Six pieces, every answer held. The box is full, and the morning lets her carry it.",
              "ست قطع، وكل إجابة في مكانها. الصندوق ممتلئ، والصباح يسمح لها بحمله." },

            { "The gate closes. The dawn is ordinary and enormous.\n\nThe box is not full. She can name the missing pieces - which is its own kind of remembering - and the pieces she has, she has read aloud, checked, and corrected, the way her mother taught her to study.\n\nSome of the voice is gone. Not all of it. Never all of it.\n\nShe carries the box home the way you carry a lit lamp through wind: close, and careful.",
              "تُغلق البوابة. الفجر عادي وهائل.\n\nالصندوق ليس ممتلئًا. تستطيع أن تسمّي القطع الناقصة - وهذا نوع آخر من التذكر - وما لديها قرأته بصوت عالٍ، وراجعته، وصوّبته، كما علّمتها أمها أن تراجع.\n\nبعض الصوت ذهب. ليس كلّه. ليس كلّه أبدًا.\n\nتحمل الصندوق إلى البيت كما يحمل المرء مصباحًا مضيئًا عبر الريح: قريبًا من الصدر، وبحذر." },
            { "She saved what she could hold. Some of the voice is gone; not all of it.",
              "أنقذت ما استطاعت حملَه. بعض الصوت ذهب؛ ليس كلّه." },

            { "The gate closes behind her with a sound like a full stop.\n\nShe came for six pieces of a person and left with a handful. The box is light. Light boxes are easy to run with; that is the trouble.\n\nBehind her the building opens its arms to the crews and gives up what it kept, and she will spend years half-remembering that there was something she meant to bring.\n\nThe morning is very bright and there is almost nothing in her hands.",
              "تُغلق البوابة خلفها بصوت يشبه نقطة النهاية.\n\nجاءت لأجل ست قطع من إنسانة وخرجت بقبضة محدودة. الصندوق خفيف. الصناديق الخفيفة سهلة الجري بها؛ هذه هي المشكلة.\n\nخلفها يفتح المبنى ذراعيه لفرق الهدم ويسلّم ما احتفظ به، وستقضي سنوات تتذكر نصف تذكر أن هناك شيئًا قصدت إحضاره.\n\nالصباح ساطع جدًا، ولا يكاد شيء في يديها." },
            { "The gate, the morning, the road. The box barely weighs anything at all.",
              "البوابة، والصباح، والطريق. الصندوق لا يكاد يزن شيئًا." },

            { "The corridor ends at the gate and the gate does not open fast enough. The crews' vans turn the corner. The sky does its morning work.\n\nThe Shadow reaches her not as a hand but as a hush: names gone quiet, a night folded up like a finished exam.\n\nShe wakes on the pavement with the box still shut, and the school - patient, gentle, terrible - keeps her mother's memory the way a library keeps a book nobody borrowed.\n\nDawn comes. It simply comes without her.",
              "ينتهي الممر عند البوابة، والبوابة لا تنفتح بالسرعة الكافية. سيارات الفرق تدور على الزاوية. السماء تقوم بعمل الصباح.\n\nيصل الظل إليها لا كيدٍ بل كخفوت: أسماء صارت هادئة، ليلة مطوية كورقة امتحان انتهى وقتها.\n\nتفيق على الرصيف والصندوق ما زال مغلقًا، والمدرسة - الصبورة، الرقيقة، المرعبة - تحفظ ذاكرة أمها كما تحفظ المكتبة كتابًا لم يستعره أحد.\n\nيأتي الفجر. يأتي ببساطة دونها." },
            { "The night ran out. The school keeps what she could not carry.",
              "نفدت الليلة. المدرسة تحفظ ما لم تستطع حملَه." },

            // ---- Act I --------------------------------------------------------
            { "The gate slams behind me. Of course it does. Schools only open all the way for the last time.",
              "تُصفق البوابة خلفي بعنف. طبعًا تفعل. المدارس لا تفتح على مصراعيها إلا في آخر مرة." },
            { "Steady. Count the windows that still hold light: three.",
              "اهدئي. عُدّي النوافذ التي ما زالت تحتفظ بنورها: ثلاث." },
            { "Walk like I'm only late for her class.",
              "امشي كأنني تأخرت عن حصتها فقط." },
            { "Say sorry to the dark. Quietly, so it knows my hands are empty.",
              "اعتذري للعتمة. بهمس، لتعرف أن يديّ خاويستان." },
            { "Three wings, three fuse boxes, six pieces of her. The crew comes at dawn. All right. One room at a time.",
              "ثلاثة أجنحة، ثلاثة صناديق مصاهر، وست قطع منها. الفريق يأتي عند الفجر. حسنًا. غرفة تلو الأخرى." },
            { "First the wiring, then the reading. One room at a time - that is how you eat a school.",
              "أولًا الأسلاك ثم القراءة. غرفة تلو الأخرى - هكذا تُؤكَل مدرسة." },
            { "Stay in the light while it hums.", "أبقي في النور وهو يطن." },
            { "Step back into the corridor. Look at the dark properly.",
              "أرجع خطوة إلى الممر. أنظر إلى العتمة كما ينبغي." },
            { "Turn the flashlight off. Just to hear what the dark says.",
              "أطفئ المصباح. فقط لأسمع ما تقوله العتمة." },
            { "The crank wins. The light stays on.", "اللافاة تنتصر. يبقى النور مشتعلًا." },
            { "Warm light on white sheets. Her classroom smelled like this - chalk, jasmine, the inside of a book.",
              "نور دافئ على أغطية بيضاء. هكذا كانت تشمّ صفها - طباشير وياسمين ودفء الكتب." },
            { "It stands at the end of the corridor the way a sentence stands when one word has been rubbed out. My beam touches it and it slides away. Politely.",
              "يقف في نهاية الممر كما تقف الجملة حين يُمحى منها كلمة. يمسّه شعاعي فينسحب. بأدب." },
            { "On her desk: the empty memory box, and one card in her handwriting. \"Put me back together.\"",
              "على مكتبها: صندوق الذكريات فارغًا، وبطاقة واحدة بخط يدها. «أعدني كما كنت.»" },
            { "Slide the card into my pocket. Evidence first.", "أدسّ البطاقة في جيبي. الأدلة أولًا." },
            { "Read it aloud. Like she's listening.", "أقرأها بصوت عالٍ. كأنها تسمع." },
            { "The words stay behind my teeth. Maybe next time.", "تبقى الكلمات خلف أسناني. ربما في المرة القادمة." },
            { "Leave it on the desk. I'll take it when the box is full.", "أتركها على المكتب. آخذها حين يمتلئ الصندوق." },
            { "Six fragments, three fuse boxes, one gate. I know the shape of this night. I have been studying for it my whole life.",
              "ست شذرات، ثلاثة صناديق مصاهر، بوابة واحدة. أعرف هيئة هذه الليلة. درستُ لها طول عمري." },
            { "A pressed-flower bookmark, left in a book she never re-shelved. Her hand underneath: \"Return by Thursday.\" I am four years late.",
              "فاصل كتاب بزهرة مضغوطة، تركته في كتاب لم تعدِه إلى رفّه. بخطها أسفله: «يُعاد يوم الخميس». أنا متأخرة أربع سنوات." },
            { "Press it between the lesson plans. Where it belongs.", "أضغطه بين خطط الدروس. حيث ينتمي." },
            { "Keep it in my hand a while.", "أبقيه في يدي قليلًا." },
            { "A library card is a key that never rusts.", "بطاقة المكتبة مفتاح لا يصدأ." },
            { "My exam paper. Red pen across the top: \"You see more than you think.\" She marked sixty papers that week. She remembered mine.",
              "ورقتي. قلم أحمر عبر الأعلى: «أنتِ ترين أكثر مما تظنين». صنّفت ستين ورقة ذلك الأسبوع. وتذكرت ورقتي." },

            // ---- Act II -------------------------------------------------------
            { "Fuse two hums awake. Through the lab window, on the lit floor: two shadows. Mine - and one more, edges smudged like an eraser was passed over it.",
              "الصندوق الثاني يطن وقد استيقظ. عبر نافذة المختبر، على الأرضية المضيئة: ظلّان. ظلي - وآخر، حوافّه ممسوحة كأن ممحاةً مرّت عليها." },
            { "Call her name into the wing.", "أنادي اسمها في الجناح." },
            { "The name stays where names are safe.", "يبقى الاسم حيث تظل الأسماء آمنة." },
            { "Count the fuse wires. Order is a kind of courage.", "أعدّ أسلاك المصهر. الترتيب نوع من الشجاعة." },
            { "Keep walking. Windows lie at this hour.", "أواصل المشي. النوافذ تكذب في مثل هذه الساعة." },
            { "It answers with a name. Almost mine. The vowels are worn like stairs. \"...N-oor. Nour.\" It has been practicing.",
              "يجيب باسم. شبه اسمي. حروفه الممدودة بالية كالدرج. «...ن-ور. نور». لقد كان يتدرب." },
            { "The whisper folds in on itself, chalk under a door, static rising. Somewhere down the wing a poster goes blank.",
              "يلتف الهمس على نفسه، صرير طباشير تحت باب، وتذبذب يعلو. في مكان ما من الجناح تفقد ملصقةٌ كلَّ ما عليها." },
            { "The science wing is awake. One fuse box left, and the staff room that remembers her red pen.",
              "جناح العلوم صحي. بقي صندوق مصاهر واحد، وغرفة المعلمين التي تتذكر قلمها الأحمر." },
            { "The garden bench. A jasmine pressed flat in her book of poems, and a photograph of this exact bench, her hand caught in the corner of the frame.",
              "مقعد الحديقة. ياسمين مضغوط في كتاب قصائدها، وصورة لهذا المقعد بعينه، ويدها عالقة في زاوية الكادر." },
            { "Sit for one minute. The Shadow slows near light; grief slows near benches.",
              "أجلس دقيقة واحدة. الظل يتباطأ قرب النور؛ والحزن يتباطأ قرب المقاعد." },
            { "I take the photograph and keep standing. Standing is easier.", "آخذ الصورة وأبقى واقفة. الوقوف أسهل." },
            { "Take the photograph. Leave the jasmine where it grew.", "آخذ الصورة. وأترك الياسمين حيث نما." },
            { "Both in the box. This bench will not exist by Friday.", "كلاهما في الصندوق. لن يبقى هذا المقعد بعد الجمعة." },
            { "Water the jasmine on Thursdays. Talk to it. Plants are students that never interrupt.",
              "اسقي الياسمين أيام الخميس. حدّثيه. النباتات تلاميذ لا يقاطعون أبدًا." },
            { "Pinned inside the optics cabinet, in her lesson-plan voice: \"Light travels in straight lines - so do stories.\"",
              "مثبّتة داخل خزانة البصريات، بصوت خطط دروسها: «الضوء يسير في خطوط مستقيمة - وكذلك الحكايات.»" },
            { "Read it aloud, exactly as written. Precision is a kind of prayer.", "أقرؤها بصوت عالٍ كما كُتبت. الدقة نوع من الصلاة." },
            { "Copy it onto my hand. Ink is portable.", "أنسخها على يدي. الحبر قابل للحمل." },
            { "...and so do stories. Straight lines, even through walls. Especially through walls.",
              "...وكذلك الحكايات. خطوط مستقيمة، حتى عبر الجدران. خصوصًا عبر الجدران." },
            { "It stands in the assembly hall aisle with a roll call in its mouth. Names, most of them worn past reading. One it has kept too well: mine.",
              "يقف في ممر القاعة ودفّة أسماء في فمه. أسماء، أغلبها بالي حتى تعذّر قراءتها. اسمًا واحدًا حفظه أكثر من اللازم: اسمي." },
            { "Answer it. \"That one's mine. You can't have it.\"", "أجبه. «هذا اسمي. لن تأخذه.»" },
            { "Too slow. The roll call moves on.", "بطيئة أكثر من اللازم. تمرّ الدفّة." },
            { "Recite her lesson line: \"Light travels in straight lines.\"", "أرتل سطر درسها: «الضوء يسير في خطوط مستقيمة.»" },
            { "Hide behind the folded chairs. Let it pass.", "أختبئ خلف الكراسي المطوية. وأدعه يمر." },
            { "\"...kept. Kept it. Kept-\" It folds my name carefully, like paper, and sets it down between us. Down the aisle: a door I never once used. The back stairs. I know the way now.",
              "«...حفظت. حفظتُه. حفظ-» يطوي اسمي بعناية كورقة، ويضعه بيننا. في آخر الممر: باب لم أستخدمه قط. الدرج الخلفي. أعرف الطريق الآن." },
            { "My voice hits it and comes back wrong, missing its middle. The whisper sharpens. A lamp dims somewhere without being touched.",
              "يصطدم صوتي به ويعود مشوّهًا، ناقصًا وسطه. يزداد الهمس حدّة. مصباح ما يخفت دون أن يلمسه أحد." },
            { "I say it like a student who did the reading. Straight lines, even through walls. The Shadow thins to a smudge and is gone down the dark aisle.",
              "أقولها كطالبة أعدت الدرس. خطوط مستقيمة، حتى عبر الجدران. يرقّ الظل حتى يصير بسمة ثم يزول في الممر المظلم." },
            { "Chalk static. Folded chairs. It passes close enough to read the absent look on its absent face. I hold still until the school forgets me again.",
              "تذبذب كالطباشير. كراسٍ مطوية. يمر قريبًا بما يكفي لأقرأ النظرة الغائبة في وجهه الغائب. أثبت حتى تنساني المدرسة من جديد." },

            // ---- Act III ------------------------------------------------------
            { "Fuse three. The staff room lights up row by row, and there is her mug beside the sink, and a note she left a colleague: \"Back Thursday - the kettle forgives you.\"",
              "المصهر الثالث. تضيء غرفة المعلمين صفًا صفًا، وهناك كوبها بجانب المغسلة، وورقة تركتها لزميلة: «أعود يوم الخميس - الغلاية تسامحك»." },
            { "Pour the cold tea out first. Then take the mug.", "أصبّ الشاي البارد أولًا. ثم آخذ الكوب." },
            { "Take the mug. The tea can be tomorrow's problem.", "آخذ الكوب. الشاي مشكلة الغد." },
            { "Leave everything. The room remembers her better with it here.", "أترك كل شيء. الغرفة تتذكرها أكثر بوجوده هنا." },
            { "Keep the light on for the ones who come after.", "أبقي النور مشتعلًا لمن يأتون من بعد." },
            { "The gate crank is live. The corridor to the front doors is a lit runway. The Shadow knows it too.",
              "مفتاح البوابة صار حيًا. الممر إلى الأبواب الأمامية مدرج مضيء. الظل يعرف ذلك أيضًا." },
            { "Between me and the gate it has stopped pretending to be smoke. It is the school's whole forgetting, standing up: blank posters, erased names, a poem with its middle rubbed out.",
              "بيني وبين البوابة توقف عن تظاهره بأنه دخان. إنه نسيان المدرسة كله، واقفًا: ملصقات فارغة، أسماء ممحوة، قصيدة مُحيت وسطها." },
            { "Say her name back to it. \"Her name was-\"", "أعيد عليه اسمها. «اسمها كان-»" },
            { "The name won't come. Feet, then. Use feet.", "الاسم لا يأتي. إذن القدمان. استعملي القدمان." },
            { "Erase it first. Swing the beam like an answer.", "أمحيه أنا أولًا. ألوّح بالشعاع كأنه إجابة." },
            { "Walk past. It cannot enter the lit corridor.", "أمرّ من جانبه. لا يمكنه دخول الممر المضيء." },
            { "\"...we kept everything. We kept-\" It holds its shape one moment longer, the way a class holds its breath before the bell. Then it lets the morning go.",
              "«...حفظنا كل شيء. حفظن-» يبقى على هيئته لحظة إضافية، كما تحبس فصلٌ أنفاسه قبل الجرس. ثم يترك الصباح يذهب." },
            { "The name comes out with holes in it. The Shadow leans close - courteous, terrible - and takes the holes as its fee.",
              "يخرج الاسم وفيه ثقوب. ينحني الظل قريبًا - مؤدبًا، مرعبًا - ويتخذ الثقوب أجرته." },
            { "Light is a straight line and I am the line. It comes apart like chalk dust, and the dust is gone before it lands.",
              "النور خط مستقيم وأنا الخط. يتفكك كغبار الطباشير، ويزول الغبار قبل أن يستقر." },
            { "It stands aside the way furniture stands aside in a school you no longer attend. I keep to the light. I do not run. Not yet.",
              "ينزاح جانبًا كما تنزاح الأثاث في مدرسة لم تعودي إليها. ألتزم بالنور. لا أركض. ليس بعد." },
            { "The windows are turning grey. The gate is one lit runway away, and my legs are a student's legs: they remember every corridor of this school.",
              "تتحول النوافذ إلى الرمادي. البوابة على بعد مدرج مضيء واحد، وساقاي ساقا طالبة: تتذكران كل ممر في هذه المدرسة." },
            { "Run the lit corridor. Straight lines only.", "أركض في الممر المضيء. خطوط مستقيمة فقط." },
            { "The back stairs I never used. Faster, darker. Mine.", "الدرج الخلفي الذي لم أستخدمه قط. أسرع، أظلم. ملكي." },
            { "Decide with your feet.", "قرّري برجليك." },
            { "The corridor lights come on ahead of me, one by one, like roll call.",
              "أضواء الممر تشتعل أمامي، واحدة تلو الأخرى، كنداء الأسماء." },
            { "Faster, habibti. The bell does not wait for the slow - and neither does the morning.",
              "أسرع يا حبيبتي. الجرس لا ينتظر البطيء - ولا الصباح." },
            { "The back stairs take the dark in jumps: three flights, a door with a push-bar, and the front hall roaring up to meet me.",
              "يأخذني الدرج الخلفي عبر العتمة نطّات: ثلاثة طوابق، باب بمزلاج ضغط، والبهو الأمامي يزأر ليلاقيني." },
            { "The crank is cold and eager. Before I turn it: one promise, out loud, because the school is listening, and schools keep records.",
              "المفتاح بارد ومتشوق. قبل أن ألويه: وعد واحد بصوت عالٍ، لأن المدرسة تسمع، والمدارس تحفظ السجلات." },
            { "I'll keep the light on. For the ones who come after.", "سأبقي النور مشتعلًا. لمن يأتون من بعد." },
            { "I'll come back with a key, not a fence to climb.", "سأعود بمفتاح، لا بسياج أتسلقه." },
            { "Say nothing. Some things should stay in the building.", "أصمت. بعض الأشياء ينبغي أن تبقى في المبنى." },
            { "Go on, then. Knowledge is the only light that follows a person into the dark.",
              "انطلقي إذاً. المعرفة هي النور الوحيد الذي يتبع المرء في العتمة." },
        };
    }
}
