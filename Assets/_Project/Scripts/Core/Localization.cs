using System;
using System.Collections.Generic;
using LightShadow.UI;
using UnityEngine;

namespace LightShadow.Core
{
    public enum GameLanguage { English = 0, Arabic = 1 }

    // English is the source language; Arabic is a hand-written translation table.
    // Text is shaped/reordered only at display time, so narration and debugging
    // still see the logical Arabic sentence.
    public static class Localization
    {
        public static event Action LanguageChanged;

        static Font englishFont;
        static Font arabicFont;
        static GameLanguage current = GameLanguage.English;
        static bool loaded;

        public static GameLanguage Current
        {
            get
            {
                EnsureLoaded();
                return current;
            }
        }

        public static Font EnglishFont
        {
            get
            {
                if (englishFont == null)
                {
                    englishFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (englishFont == null) englishFont = Font.CreateDynamicFontFromOSFont("Arial", 18);
                }
                return englishFont;
            }
        }

        public static Font ArabicFont
        {
            get
            {
                if (arabicFont == null)
                {
                    string[] candidates =
                    {
                        "Tahoma", "Arial", "Segoe UI", "Noto Naskh Arabic",
                        "Noto Sans Arabic", "Droid Arabic Naskh", "Droid Sans Arabic"
                    };
                    arabicFont = Font.CreateDynamicFontFromOSFont(candidates, 28);
                    if (arabicFont == null) arabicFont = EnglishFont;
                }
                return arabicFont;
            }
        }

        public static Font CurrentFont => Current == GameLanguage.Arabic ? ArabicFont : EnglishFont;

        static void EnsureLoaded()
        {
            if (loaded) return;
            loaded = true;
            current = (GameLanguage)PlayerPrefs.GetInt("LS_Language", (int)GameLanguage.English);
        }

        public static void SetLanguage(GameLanguage language)
        {
            EnsureLoaded();
            if (current == language) return;
            current = language;
            PlayerPrefs.SetInt("LS_Language", (int)language);
            PlayerPrefs.Save();
            LanguageChanged?.Invoke();
        }

        public static string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return key;
            if (Current == GameLanguage.English) return key;
            return Arabic.TryGetValue(key, out string arabic) ? arabic : key;
        }

        // Translation by the English source sentence. Used by localized components
        // whose English text is already on screen.
        public static string Tr(string english)
        {
            return Get(english);
        }

        public static string Present(string text)
        {
            if (Current == GameLanguage.English || string.IsNullOrEmpty(text)) return text;
            return ArabicSupport.Fix(text);
        }

        public static string TrPresent(string english)
        {
            return Present(Tr(english));
        }

        public static string Format(string englishTemplate, params object[] args)
        {
            string template = Tr(englishTemplate);
            try
            {
                return Present(string.Format(template, args));
            }
            catch (FormatException)
            {
                return Present(template);
            }
        }

        public static string PresentBool(bool on) => Present(on ? Tr("On") : Tr("Off"));

        public static string Category(QuestionCategory category)
        {
            switch (category)
            {
                case QuestionCategory.Science: return Tr("Science");
                case QuestionCategory.Math: return Tr("Math");
                case QuestionCategory.History: return Tr("History");
                case QuestionCategory.Geography: return Tr("Geography");
                case QuestionCategory.Language: return Tr("Language");
                case QuestionCategory.Health: return Tr("Health");
                default: return category.ToString();
            }
        }

        public static string Difficulty(QuestionDifficulty difficulty)
        {
            switch (difficulty)
            {
                case QuestionDifficulty.ActI: return Tr("Act I");
                case QuestionDifficulty.ActII: return Tr("Act II");
                case QuestionDifficulty.ActIII: return Tr("Act III");
                default: return difficulty.ToString();
            }
        }

        public static string QualityName(int level)
        {
            if (level < 0 || level >= QualitySettings.names.Length) return "";
            string name = QualitySettings.names[level];
            return Tr(name);
        }

        public static string QuestionText(QuestionData question)
        {
            if (question == null) return "";
            if (Current == GameLanguage.Arabic && !string.IsNullOrEmpty(question.questionAr))
                return Present(question.questionAr);
            return question.question;
        }

        public static string QuestionOption(QuestionData question, int index)
        {
            if (question == null || question.options == null || index < 0 || index >= question.options.Length)
                return "";
            if (Current == GameLanguage.Arabic && question.optionsAr != null
                && index < question.optionsAr.Length && !string.IsNullOrEmpty(question.optionsAr[index]))
                return Present(question.optionsAr[index]);
            return question.options[index];
        }

        public static string QuestionExplanation(QuestionData question)
        {
            if (question == null) return "";
            if (Current == GameLanguage.Arabic && !string.IsNullOrEmpty(question.explanationAr))
                return Present(question.explanationAr);
            return question.explanation;
        }

        public static string OptionLabel(int index, string option)
        {
            string raw = (index + 1) + ". " + option;
            return Present(raw);
        }

        public static string TimerLabel(float seconds)
        {
            int whole = Mathf.CeilToInt(Mathf.Max(0f, seconds));
            return Present(Current == GameLanguage.Arabic ? whole + " ث" : whole + "s");
        }

        public static string LanguageName(GameLanguage language)
        {
            return language == GameLanguage.Arabic ? "العربية" : "English";
        }

        // ------------------------------------------------------------------ data

        static readonly Dictionary<string, string> Arabic = new Dictionary<string, string>
        {
            // ---- menu ------------------------------------------------------
            { "LIGHT SHADOW", "الضوء والظل" },
            { "AL-NOOR SECONDARY SCHOOL  ·  THE LAST NIGHT", "مدرسة النور الثانوية  ·  الليلة الأخيرة" },
            { "ENTER THE SCHOOL", "ادخل المدرسة" },
            { "HOW TO PLAY", "كيف تلعب" },
            { "SETTINGS", "الإعدادات" },
            { "QUIT", "خروج" },
            { "BACK", "رجوع" },
            { "CLOSE", "إغلاق" },
            { "RESUME", "متابعة" },
            { "QUIT TO DESKTOP", "الخروج إلى سطح المكتب" },
            { "PAUSED", "إيقاف مؤقت" },
            { "The school waits.", "المدرسة تنتظر." },
            { "Esc or the pause button closes this card.", "اضغط Esc أو زر الإيقاف لإغلاق هذه البطاقة." },
            { "A game for good  ·  no combat, no gore  ·  one night, one flashlight",
              "لعبة من أجل الخير  ·  بلا قتال أو دماء  ·  ليلة واحدة ومصباح واحد" },
            { "Her mother's old school, the night before it falls -\na determined student recharges a knowledge-powered flashlight by answering real questions.",
              "مدرسة أمها القديمة، في الليلة قبل هدمها —\nعلى طالبة عازمة أن تشحن مصباحًا يعمل بالمعرفة عبر الإجابة عن أسئلة حقيقية." },

            // ---- how to play ----------------------------------------------
            { "You are Nour. The school is dark, the demolition crew comes at dawn, and the Shadow hunts in the dark.",
              "أنت نور. المدرسة مظلمة، وفريق الهدم يأتي عند الفجر، والظل يطارد في العتمة." },
            { "Answer chalkboard questions to charge the Bright Idea Meter. A brighter beam reaches further, repels the Shadow above 30, and keeps the dark away. Wrong answers and timeouts dim it.",
              "أجب عن أسئلة السبورة لشحن مؤشر الفكرة المضيئة. الشعاع الأقوى يصل أبعد، ويطرد الظل فوق 30، ويبقي العتمة بعيدة. الإجابات الخاطئة وانتهاء الوقت تخفت الشعاع." },
            { "Lit rooms are safe: the Shadow cannot enter them, and every lit room is a checkpoint. The battery is fuel and runs out; batteries are scattered through the school.",
              "الغرف المضاءة آمنة: لا يستطيع الظل دخولها، وكل غرفة مضاءة نقطة حفظ. البطارية وقود وتنفد؛ البطاريات موزعة في المدرسة." },
            { "Restore three fuse boxes, collect six Knowledge Fragments, power the gate, and walk out. You cannot die - being caught only costs light and sends you back to the last checkpoint.",
              "أعد تشغيل ثلاثة صناديق مصاهر، واجمع ست شذرات معرفة، وشغّل البوابة، واخرج. لا يمكنك الموت — الإمساك بك يكلفك ضوءًا ويعيدك إلى آخر نقطة حفظ." },

            // ---- HUD ------------------------------------------------------
            { "BRIGHT IDEA", "الفكرة المضيئة" },
            { "BATTERY", "البطارية" },
            { "OBJECTIVES", "المهام" },
            { "Restore fuse boxes: {0}/{1}", "أعد تشغيل صناديق المصاهر: {0}/{1}" },
            { "Restore fuse boxes: {0}/{1}  (done)", "أعد تشغيل صناديق المصاهر: {0}/{1}  (تم)" },
            { "Collect Knowledge Fragments: {0}/{1}", "اجمع شذرات المعرفة: {0}/{1}" },
            { "Collect Knowledge Fragments: {0}/{1}  (done)", "اجمع شذرات المعرفة: {0}/{1}  (تم)" },
            { "Power the main gate", "شغّل البوابة الرئيسية" },
            { "Power the main gate  (the crank is live)", "شغّل البوابة الرئيسية  (المفتاح جاهز)" },
            { "Power the main gate  (three fuse boxes first)", "شغّل البوابة الرئيسية  (ثلاثة صناديق مصاهر أولاً)" },
            { "Escape at dawn", "اهرب عند الفجر" },
            { "Escape at dawn  (go now)", "اهرب عند الفجر  (اذهب الآن)" },
            { "  (done)", "  (تم)" },
            { "  (the crank is live)", "  (المفتاح جاهز)" },
            { "  (three fuse boxes first)", "  (ثلاثة صناديق مصاهر أولاً)" },
            { "  (go now)", "  (اذهب الآن)" },
            { "The crank light died. Find a battery.", "مات ضوء المصباح. ابحث عن بطارية." },
            { "Checkpoint reached.", "تم الوصول إلى نقطة الحفظ." },
            { "The gate is powered. Run.", "تم تشغيل البوابة. اهرب." },
            { "The gate crank is dead. Three fuse boxes must be restored first.",
              "مفتاح البوابة ميت. يجب إعادة تشغيل ثلاثة صناديق مصاهر أولاً." },

            // ---- prompts --------------------------------------------------
            { "Read the chalkboard question", "اقرأ سؤال السبورة" },
            { "Restore the fuse box", "أعد تشغيل صندوق المصاهر" },
            { "The fuse hums quietly.", "الصندوق يطن بهدوء." },
            { "Collect {0}", "اجمع {0}" },
            { "Take the battery", "خذ البطارية" },
            { "Examine the gate", "افحص البوابة" },
            { "The gate is open.", "البوابة مفتوحة." },

            // ---- quiz -----------------------------------------------------
            { "Correct.", "صحيح." },
            { "Not quite.", "ليس تمامًا." },
            { "Out of time.", "انتهى الوقت." },
            { "The answer is: ", "الإجابة هي: " },
            { "1-4 or click to answer   ·   the world does not pause", "1-4 أو انقر للإجابة   ·   العالم لا يتوقف" },
            { "Science", "العلوم" },
            { "Math", "الرياضيات" },
            { "History", "التاريخ" },
            { "Geography", "الجغرافيا" },
            { "Language", "اللغة" },
            { "Health", "الصحة" },
            { "Act I", "الفصل الأول" },
            { "Act II", "الفصل الثاني" },
            { "Act III", "الفصل الثالث" },

            // ---- settings -------------------------------------------------
            { "Resolution", "الدقة" },
            { "Fullscreen", "ملء الشاشة" },
            { "Windowed", "نافذة" },
            { "Quality", "الجودة" },
            { "VSync", "تزامن الإطارات" },
            { "Shadows", "الظلال" },
            { "Anti-aliasing", "تنعيم الحواف" },
            { "Render scale", "مقياس الرسم" },
            { "Native", "أصلي" },
            { "Brightness", "السطوع" },
            { "Mouse sensitivity", "حساسية الفأرة" },
            { "Narrator voice", "صوت الراوي" },
            { "On", "مفعّل" },
            { "Off", "متوقف" },
            { "Reset defaults", "إعادة الإعدادات الافتراضية" },
            { "Low", "منخفضة" },
            { "Medium", "متوسطة" },
            { "High", "عالية" },
            { "Very High", "عالية جدًا" },
            { "Ultra", "فائقة" },

            // ---- recap ----------------------------------------------------
            { "Study Recap", "خلاصة المراجعة" },
            { "No questions were answered tonight.", "لم تُجب عن أي سؤال هذه الليلة." },
            { "Nothing was asked tonight.", "لم يُطرح شيء هذه الليلة." },
            { "Answered: {0}    Correct: {1} ({2}%)", "أجبت: {0}    صحيح: {1} ({2}%)" },
            { "Your answer: {0}", "إجابتك: {0}" },
            { "Your answer: none (out of time)", "إجابتك: لا شيء (انتهى الوقت)" },
            { "Correct answer: {0}", "الإجابة الصحيحة: {0}" },
            { "RETURN TO MAIN MENU", "العودة إلى القائمة الرئيسية" },
            { "\"Knowledge is the only light that follows a person into the dark.\"",
              "«المعرفة هي النور الوحيد الذي يتبع المرء في العتمة.»" },

            // ---- ending ---------------------------------------------------
            { "DAWN", "الفجر" },
            { "The gate closes behind her. The sky above the school turns from black to grey to a thin, clean gold.\n\nShe opens the memory box on the pavement and reads her mother's lesson plans one last time.",
              "تُغلق البوابة خلفها. يتحول السماء فوق المدرسة من الأسود إلى الرمادي إلى ذهب رقيق صافٍ.\n\nتفتح صندوق الذكريات على الرصيف وتقرأ خطط دروس أمها للمرة الأخيرة." },

            // ---- world labels --------------------------------------------
            { "LIBRARY", "المكتبة" },
            { "LITERATURE", "الأدب" },
            { "STAFF ROOM", "غرفة المعلمين" },
            { "PHYSICS LAB", "مختبر الفيزياء" },
            { "GARDEN", "الحديقة" },
            { "MAIN HALL", "القاعة الرئيسية" },
            { "ENTRANCE HALL", "بهو الدخول" },
            { "AL-NOOR SECONDARY SCHOOL", "مدرسة النور الثانوية" },
            { "QUIZ\n<size=26>press E</size>", "سؤال\n<size=26>اضغط E</size>" },
            { "FUSE", "مصهر" },
            { "MAIN GATE", "البوابة الرئيسية" },
            { "KNOWLEDGE IS LIGHT\n<size=26>answer the boards to charge your flashlight</size>",
              "المعرفة نور\n<size=26>أجب عن الأسئلة لشحن مصباحك</size>" },
            { "Put me back together.", "أعدني كما كنت." },

            // ---- story ----------------------------------------------------
            { "The entrance light still works. It watched her come in.",
              "ضوء المدخل ما زال يعمل. لقد رآها تدخل." },
            { "The library holds its breath. \"Put me back together.\"",
              "المكتبة تحبس أنفاسها. «أعدني كما كنت.»" },
            { "The lab smells of dust and copper. The light steadies her hands.",
              "رائحة الغبار والنحاس في المختبر. النور يثبّت يديها." },
            { "Her mug is still on the table. The room remembers her.",
              "كوبها ما زال على الطاولة. الغرفة تتذكرها." },
            { "The hall is full of empty seats. The last light is warm.",
              "القاعة مليئة بالمقاعد الفارغة. الضوء الأخير دافئ." },
            { "She always said a library card is a key that never rusts.",
              "كانت تقول دائمًا إن بطاقة المكتبة مفتاح لا يصدأ." },
            { "In her classroom the chalk still smelled of jasmine.",
              "في صفها، ما زال الطباشير يفوح منه رائحة الياسمين." },
            { "The whole hall hummed when her students read aloud.",
              "كانت القاعة كلها تطن حين يقرأ طلابها بصوت عالٍ." },
            { "Between the pages, a jasmine flower. Mother pressed it the week I started school.",
              "بين الصفحات زهرة ياسمين. أمي ضغطتها الأسبوع الذي بدأت فيه المدرسة." },
            { "An encouraging note in red pen: \"You see more than you think.\"",
              "ملاحظة مشجعة بقلم أحمر: «أنت ترى أكثر مما تظن.»" },
            { "She kept the photograph of this bench, and the jasmine she picked beside it.",
              "احتفظت بصورة هذا المقعد، وبالياسمين الذي قطفتْه بجانبه." },
            { "\"Light travels in straight lines - so do stories.\"",
              "«الضوء يسير في خطوط مستقيمة — وكذلك الحكايات.»" },
            { "Her name is on the programme, third from the bottom, in small neat print.",
              "اسمها على البرنامج، الثالث من الأسفل، بخط صغير أنيق." },
            { "\"Keep the light on for the ones who come after.\"",
              "«أبقِ النور مشتعلًا لمن يأتون بعدك.»" },

            // ---- narration -----------------------------------------------
            { "The demolition crew comes at dawn. Restore the light. Carry her memories home.",
              "فريق الهدم يأتي عند الفجر. أعد النور. واحمل ذكرياتها معك إلى الوطن." },
            { "The library hums awake. Two fuse boxes are still dead.",
              "تطن المكتبة وقد استيقظت. ما زال هناك صندوقا مصاهر ميتان." },
            { "The staff room remembers her red pen. One fuse box left.",
              "غرفة المعلمين تتذكر قلمها الأحمر. بقي صندوق مصاهر واحد." },
            { "The last fuse is alive. The gate crank is waiting.",
              "المصهر الأخير حي الآن. مفتاح البوابة ينتظر." },
            { "Every fragment is found. The memory box is full of her voice.",
              "وُجدت كل الشذرات. صندوق الذكريات صار مليئًا بصوتها." },
            { "The gate is powered. The Shadow is right behind you. Run for the dawn.",
              "شُغّلت البوابة. الظل خلفك مباشرة. اهرب نحو الفجر." },
            { "The school remembers her mother. Nour must carry what is precious out before dawn.",
              "المدرسة تتذكر أمها. على نور أن تحمل ما هو عزيز قبل الفجر." },
            { "The gate closes behind Nour. She opens the memory box on the pavement and reads the lesson plans one last time.",
              "تُغلق البوابة خلف نور. تفتح صندوق الذكريات على الرصيف وتقرأ خطط الدروس للمرة الأخيرة." }
        };
    }
}
