using System.Collections.Generic;
using System.Text;
using LightShadow.UI;
using UnityEditor;
using UnityEngine;

namespace LightShadow.Tools
{
    // Phase 4 tool: seeds the starter question bank (doc 4.1: six subjects,
    // Act I recall -> Act II apply -> Act III mixed) and wires them into the
    // QuestionDatabase asset. Run "LightShadow > 2. Generate Questions" after
    // the Unity restart the setup tool asks for.
    // Every seeded question is marked NEEDS-FACT-CHECK until a teacher reviews it.
    public static class QuestionGenerator
    {
        private const string QuestionsFolder = "Assets/_Project/Data/Questions";
        private const string DatabasePath = QuestionsFolder + "/QuestionDatabase.asset";
        private const string FactSourcePending = "NEEDS-FACT-CHECK";

        private struct Seed
        {
            public string id;
            public QuestionCategory category;
            public QuestionDifficulty difficulty;
            public string question;
            public string[] options;
            public int correctIndex;
            public string explanation;
        }

        [MenuItem("LightShadow/2. Generate Questions", priority = 2)]
        public static void RunQuestionGeneration()
        {
            string summary = GenerateQuestionsSilent();

            EditorUtility.DisplayDialog(
                "LightShadow - Generate Questions",
                summary + "\n\nEvery seeded question carries Fact Source = \"NEEDS-FACT-CHECK\"."
                    + " Have a teacher open the Q_*.asset files and verify the content before release (doc 8.4).",
                "OK");
        }

        // The scene builder calls this before generating the school so the
        // QuestionDatabase always exists and is linked. No dialogs in batch mode.
        public static string GenerateQuestionsSilent()
        {
            EditorFolderUtility.EnsureFolder(QuestionsFolder);

            List<Seed> seeds = BuildSeeds();
            int created = 0;
            int kept = 0;

            foreach (Seed seed in seeds)
            {
                string path = QuestionsFolder + "/Q_" + seed.id + ".asset";
                if (AssetDatabase.LoadAssetAtPath<QuestionData>(path) != null)
                {
                    // Never overwrite: a teacher may already have reviewed this question.
                    kept++;
                    continue;
                }

                QuestionData asset = ScriptableObject.CreateInstance<QuestionData>();
                asset.question = seed.question;
                asset.options = seed.options;
                asset.correctIndex = seed.correctIndex;
                asset.category = seed.category;
                asset.difficulty = seed.difficulty;
                asset.explanation = seed.explanation;
                asset.factSource = FactSourcePending;
                AssetDatabase.CreateAsset(asset, path);
                created++;
            }

            int total = RebuildDatabase();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string summary = "Created " + created + " new question(s), kept " + kept + " existing one(s).\n"
                + "QuestionDatabase now links " + total + " question(s):\n" + DatabasePath;
            Debug.Log("[LightShadow] " + summary);
            return summary;
        }

        // Rebuilds the database list from the folder so manual additions survive
        // and every seeded asset is linked. Returns the number of linked questions.
        private static int RebuildDatabase()
        {
            QuestionDatabase database = AssetDatabase.LoadAssetAtPath<QuestionDatabase>(DatabasePath);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<QuestionDatabase>();
                AssetDatabase.CreateAsset(database, DatabasePath);
            }

            var questions = new List<QuestionData>();
            foreach (string guid in AssetDatabase.FindAssets("t:QuestionData", new[] { QuestionsFolder }))
            {
                QuestionData question = AssetDatabase.LoadAssetAtPath<QuestionData>(AssetDatabase.GUIDToAssetPath(guid));
                if (question != null) questions.Add(question);
            }

            questions.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            database.all = questions;
            EditorUtility.SetDirty(database);
            return questions.Count;
        }

        private static List<Seed> BuildSeeds()
        {
            var seeds = new List<Seed>();
            var notes = new StringBuilder();

            AddSci(seeds);
            AddMat(seeds);
            AddHis(seeds);
            AddGeo(seeds);
            AddLan(seeds);
            AddHea(seeds);

            // Cheap safety net: four options and a correct index inside range (doc 4.1).
            foreach (Seed seed in seeds)
            {
                if (seed.options == null || seed.options.Length != 4 || seed.correctIndex < 0 || seed.correctIndex > 3)
                {
                    notes.Append("Invalid seed: ").Append(seed.id).AppendLine();
                }
            }

            if (notes.Length > 0)
            {
                Debug.LogWarning("[LightShadow] Question seed table has problems:\n" + notes);
            }

            return seeds;
        }

        private static void AddSci(List<Seed> seeds)
        {
            seeds.Add(new Seed
            {
                id = "Sci_001", category = QuestionCategory.Science, difficulty = QuestionDifficulty.ActI,
                question = "Plants take up water and minerals from the soil mainly through which part?",
                options = new[] { "Leaves", "Roots", "Flowers", "Stem" },
                correctIndex = 1,
                explanation = "Roots absorb water and dissolved minerals from the soil; leaves mostly make food through photosynthesis."
            });
            seeds.Add(new Seed
            {
                id = "Sci_002", category = QuestionCategory.Science, difficulty = QuestionDifficulty.ActI,
                question = "Which gas from the air do humans need to breathe in to stay alive?",
                options = new[] { "Nitrogen", "Oxygen", "Carbon dioxide", "Helium" },
                correctIndex = 1,
                explanation = "Oxygen enters the blood through the lungs and is used to release energy from food."
            });
            seeds.Add(new Seed
            {
                id = "Sci_003", category = QuestionCategory.Science, difficulty = QuestionDifficulty.ActII,
                question = "A pot of water is heated on a stove. What happens to the water molecules as the water warms?",
                options = new[] { "They slow down", "They stop moving", "They move faster", "They turn into light" },
                correctIndex = 2,
                explanation = "Warming speeds molecules up: temperature measures the average energy of their motion."
            });
            seeds.Add(new Seed
            {
                id = "Sci_004", category = QuestionCategory.Science, difficulty = QuestionDifficulty.ActIII,
                question = "A plant grows from a small seed into a large tree. Most of its new mass comes from...",
                options = new[] { "The soil", "The water that was poured on it", "Carbon dioxide in the air", "Sunlight" },
                correctIndex = 2,
                explanation = "Photosynthesis builds the plant's body from carbon dioxide in the air. Experiments tracing carbon show the soil adds very little mass."
            });
            seeds.Add(new Seed
            {
                id = "Sci_005", category = QuestionCategory.Science, difficulty = QuestionDifficulty.ActI,
                question = "At sea level, at what temperature does pure water boil?",
                options = new[] { "50 degrees Celsius", "90 degrees Celsius", "100 degrees Celsius", "150 degrees Celsius" },
                correctIndex = 2,
                explanation = "At standard atmospheric pressure water boils at 100 degrees Celsius (212 degrees Fahrenheit)."
            });
            seeds.Add(new Seed
            {
                id = "Sci_006", category = QuestionCategory.Science, difficulty = QuestionDifficulty.ActI,
                question = "Which planet is closest to the Sun?",
                options = new[] { "Venus", "Earth", "Mercury", "Mars" },
                correctIndex = 2,
                explanation = "Mercury is the innermost planet of the Solar System, orbiting the Sun in about 88 Earth days."
            });
            seeds.Add(new Seed
            {
                id = "Sci_007", category = QuestionCategory.Science, difficulty = QuestionDifficulty.ActI,
                question = "Which planet is the largest in our Solar System?",
                options = new[] { "Earth", "Jupiter", "Saturn", "Neptune" },
                correctIndex = 1,
                explanation = "Jupiter is the largest planet; more than 1,300 Earths could fit inside it."
            });
            seeds.Add(new Seed
            {
                id = "Sci_008", category = QuestionCategory.Science, difficulty = QuestionDifficulty.ActII,
                question = "What force keeps the Moon in orbit around Earth?",
                options = new[] { "Magnetism", "Gravity", "Friction", "Electricity" },
                correctIndex = 1,
                explanation = "Earth's gravity pulls the Moon toward it; the Moon's sideways motion keeps it from falling in."
            });
            seeds.Add(new Seed
            {
                id = "Sci_009", category = QuestionCategory.Science, difficulty = QuestionDifficulty.ActI,
                question = "Which organ pumps blood around the human body?",
                options = new[] { "The lungs", "The liver", "The heart", "The kidneys" },
                correctIndex = 2,
                explanation = "The heart is a muscular pump that pushes blood through arteries, veins and capillaries."
            });
            seeds.Add(new Seed
            {
                id = "Sci_010", category = QuestionCategory.Science, difficulty = QuestionDifficulty.ActI,
                question = "Steam is water in which state of matter?",
                options = new[] { "Solid", "Liquid", "Gas", "Plasma" },
                correctIndex = 2,
                explanation = "Steam is water vapour - water in its gas state, above 100 degrees Celsius at sea level."
            });
        }

        private static void AddMat(List<Seed> seeds)
        {
            seeds.Add(new Seed
            {
                id = "Mat_001", category = QuestionCategory.Math, difficulty = QuestionDifficulty.ActI,
                question = "Which of these numbers is a prime number?",
                options = new[] { "21", "29", "33", "51" },
                correctIndex = 1,
                explanation = "29 has no divisors other than 1 and itself. The others are all divisible by 3."
            });
            seeds.Add(new Seed
            {
                id = "Mat_002", category = QuestionCategory.Math, difficulty = QuestionDifficulty.ActI,
                question = "In the fraction 3/8, what is the number 8 called?",
                options = new[] { "Numerator", "Denominator", "Quotient", "Remainder" },
                correctIndex = 1,
                explanation = "The denominator tells how many equal parts the whole is divided into."
            });
            seeds.Add(new Seed
            {
                id = "Mat_003", category = QuestionCategory.Math, difficulty = QuestionDifficulty.ActII,
                question = "A scarf normally costs 40. It is on sale at 25% off. How much do you pay?",
                options = new[] { "25", "30", "32", "35" },
                correctIndex = 1,
                explanation = "25% of 40 is 10, and 40 - 10 = 30."
            });
            seeds.Add(new Seed
            {
                id = "Mat_004", category = QuestionCategory.Math, difficulty = QuestionDifficulty.ActIII,
                question = "A shop sells pens at 3 pens for 10. At that same rate, how much do 12 pens cost?",
                options = new[] { "30", "36", "40", "44" },
                correctIndex = 2,
                explanation = "12 pens are four groups of 3, so four times 10 makes 40."
            });
            seeds.Add(new Seed
            {
                id = "Mat_005", category = QuestionCategory.Math, difficulty = QuestionDifficulty.ActI,
                question = "What is 7 multiplied by 8?",
                options = new[] { "54", "56", "63", "72" },
                correctIndex = 1,
                explanation = "Seven groups of eight make 56."
            });
            seeds.Add(new Seed
            {
                id = "Mat_006", category = QuestionCategory.Math, difficulty = QuestionDifficulty.ActII,
                question = "A square has sides of 5 cm. What is its perimeter?",
                options = new[] { "10 cm", "15 cm", "20 cm", "25 cm" },
                correctIndex = 2,
                explanation = "A square has four equal sides, so 5 + 5 + 5 + 5 = 20 cm."
            });
            seeds.Add(new Seed
            {
                id = "Mat_007", category = QuestionCategory.Math, difficulty = QuestionDifficulty.ActI,
                question = "What is 12 multiplied by 12?",
                options = new[] { "124", "132", "144", "154" },
                correctIndex = 2,
                explanation = "Twelve groups of twelve make 144."
            });
            seeds.Add(new Seed
            {
                id = "Mat_008", category = QuestionCategory.Math, difficulty = QuestionDifficulty.ActII,
                question = "What is 25% of 80?",
                options = new[] { "15", "20", "25", "40" },
                correctIndex = 1,
                explanation = "A quarter of 80 is 20."
            });
            seeds.Add(new Seed
            {
                id = "Mat_009", category = QuestionCategory.Math, difficulty = QuestionDifficulty.ActI,
                question = "The three angles inside any triangle add up to...",
                options = new[] { "90 degrees", "180 degrees", "270 degrees", "360 degrees" },
                correctIndex = 1,
                explanation = "In flat geometry the interior angles of a triangle always total 180 degrees."
            });
            seeds.Add(new Seed
            {
                id = "Mat_010", category = QuestionCategory.Math, difficulty = QuestionDifficulty.ActII,
                question = "Which number is the next prime after 7?",
                options = new[] { "8", "9", "10", "11" },
                correctIndex = 3,
                explanation = "8, 9 and 10 are composite; 11 has no divisors other than 1 and itself."
            });
        }

        private static void AddHis(List<Seed> seeds)
        {
            seeds.Add(new Seed
            {
                id = "His_001", category = QuestionCategory.History, difficulty = QuestionDifficulty.ActI,
                question = "In which year did the Second World War end?",
                options = new[] { "1918", "1939", "1945", "1950" },
                correctIndex = 2,
                explanation = "Fighting in World War II ended in 1945."
            });
            seeds.Add(new Seed
            {
                id = "His_002", category = QuestionCategory.History, difficulty = QuestionDifficulty.ActI,
                question = "Which ancient civilization built the pyramids at Giza?",
                options = new[] { "The Romans", "The Greeks", "The Ancient Egyptians", "The Persians" },
                correctIndex = 2,
                explanation = "The Giza pyramids were built in ancient Egypt, roughly 4,500 years ago."
            });
            seeds.Add(new Seed
            {
                id = "His_003", category = QuestionCategory.History, difficulty = QuestionDifficulty.ActII,
                question = "Why did books become much cheaper in Europe after the 1440s?",
                options = new[] { "Paper was invented in Europe then", "Printing presses copied texts far faster than scribes could", "Books became smaller", "Authors started writing faster" },
                correctIndex = 1,
                explanation = "Gutenberg's printing press (around 1440) could produce copies far more quickly than hand-copying, so prices fell."
            });
            seeds.Add(new Seed
            {
                id = "His_004", category = QuestionCategory.History, difficulty = QuestionDifficulty.ActIII,
                question = "Which of these events happened first?",
                options = new[] { "The French Revolution", "The American Declaration of Independence", "The fall of the Berlin Wall", "The end of the First World War" },
                correctIndex = 1,
                explanation = "Order: Declaration of Independence 1776, French Revolution 1789, First World War ends 1918, Berlin Wall falls 1989."
            });
            seeds.Add(new Seed
            {
                id = "His_005", category = QuestionCategory.History, difficulty = QuestionDifficulty.ActI,
                question = "Who was the first person to walk on the Moon?",
                options = new[] { "Yuri Gagarin", "Neil Armstrong", "Buzz Aldrin", "John Glenn" },
                correctIndex = 1,
                explanation = "Neil Armstrong stepped onto the Moon on 20 July 1969 during the Apollo 11 mission."
            });
            seeds.Add(new Seed
            {
                id = "His_006", category = QuestionCategory.History, difficulty = QuestionDifficulty.ActII,
                question = "In which year was the Magna Carta sealed by King John of England?",
                options = new[] { "1066", "1215", "1492", "1648" },
                correctIndex = 1,
                explanation = "The Magna Carta was sealed at Runnymede in 1215 and later became a symbol of limits on royal power."
            });
            seeds.Add(new Seed
            {
                id = "His_007", category = QuestionCategory.History, difficulty = QuestionDifficulty.ActI,
                question = "Which wall divided the city of Berlin from 1961 to 1989?",
                options = new[] { "The Great Wall", "The Berlin Wall", "Hadrian's Wall", "The Iron Curtain" },
                correctIndex = 1,
                explanation = "The Berlin Wall split the city during the Cold War until it was opened in November 1989."
            });
            seeds.Add(new Seed
            {
                id = "His_008", category = QuestionCategory.History, difficulty = QuestionDifficulty.ActI,
                question = "Who was the first President of the United States?",
                options = new[] { "Thomas Jefferson", "George Washington", "Abraham Lincoln", "John Adams" },
                correctIndex = 1,
                explanation = "George Washington served as the first U.S. President from 1789 to 1797."
            });
            seeds.Add(new Seed
            {
                id = "His_009", category = QuestionCategory.History, difficulty = QuestionDifficulty.ActII,
                question = "Which ancient civilization built Machu Picchu?",
                options = new[] { "The Aztecs", "The Maya", "The Inca", "The Olmec" },
                correctIndex = 2,
                explanation = "Machu Picchu was built by the Inca in the fifteenth century in the Andes of Peru."
            });
            seeds.Add(new Seed
            {
                id = "His_010", category = QuestionCategory.History, difficulty = QuestionDifficulty.ActI,
                question = "The Great Pyramid of Giza was built in which country?",
                options = new[] { "Iraq", "Egypt", "Greece", "Turkey" },
                correctIndex = 1,
                explanation = "The Great Pyramid stands on the Giza plateau in Egypt and was built for Pharaoh Khufu."
            });
        }

        private static void AddGeo(List<Seed> seeds)
        {
            seeds.Add(new Seed
            {
                id = "Geo_001", category = QuestionCategory.Geography, difficulty = QuestionDifficulty.ActI,
                question = "What is the largest ocean on Earth?",
                options = new[] { "The Atlantic Ocean", "The Indian Ocean", "The Pacific Ocean", "The Arctic Ocean" },
                correctIndex = 2,
                explanation = "The Pacific covers more area than all the other oceans combined."
            });
            seeds.Add(new Seed
            {
                id = "Geo_002", category = QuestionCategory.Geography, difficulty = QuestionDifficulty.ActI,
                question = "Which line of latitude divides the Earth into the Northern and Southern Hemispheres?",
                options = new[] { "The Equator", "The Prime Meridian", "The Tropic of Cancer", "The Arctic Circle" },
                correctIndex = 0,
                explanation = "The Equator is at 0 degrees latitude, exactly between the two poles."
            });
            seeds.Add(new Seed
            {
                id = "Geo_003", category = QuestionCategory.Geography, difficulty = QuestionDifficulty.ActII,
                question = "Two cities share the same latitude, but city A sits high in the mountains and city B is at sea level. Which one is normally cooler?",
                options = new[] { "City A, because temperature drops with altitude", "City B, because sea air is cold", "They are always the same temperature", "City B, because it is closer to the water" },
                correctIndex = 0,
                explanation = "Air cools by about 6.5 degrees Celsius for every kilometer you climb in the lower atmosphere."
            });
            seeds.Add(new Seed
            {
                id = "Geo_004", category = QuestionCategory.Geography, difficulty = QuestionDifficulty.ActIII,
                question = "Why do places near the Equator stay almost the same temperature all year, while places near the poles have big seasonal changes?",
                options = new[] { "The Equator has more oceans", "The Sun's angle overhead changes much less near the Equator through the year", "The poles are farther from the Sun in winter", "Wind never reaches the poles" },
                correctIndex = 1,
                explanation = "Earth's tilt changes the Sun's angle a lot near the poles but only a little near the Equator, so the tropics stay warm all year."
            });
            seeds.Add(new Seed
            {
                id = "Geo_005", category = QuestionCategory.Geography, difficulty = QuestionDifficulty.ActI,
                question = "What is the capital city of Japan?",
                options = new[] { "Seoul", "Beijing", "Tokyo", "Bangkok" },
                correctIndex = 2,
                explanation = "Tokyo has been Japan's capital since 1868, when the city was still called Edo."
            });
            seeds.Add(new Seed
            {
                id = "Geo_006", category = QuestionCategory.Geography, difficulty = QuestionDifficulty.ActII,
                question = "Which of these is the largest hot desert in the world?",
                options = new[] { "The Gobi", "The Kalahari", "The Sahara", "The Atacama" },
                correctIndex = 2,
                explanation = "The Sahara covers roughly 9 million square kilometres across North Africa, making it the largest hot desert."
            });
            seeds.Add(new Seed
            {
                id = "Geo_007", category = QuestionCategory.Geography, difficulty = QuestionDifficulty.ActI,
                question = "On which continent is the Sahara Desert?",
                options = new[] { "Asia", "Africa", "Australia", "South America" },
                correctIndex = 1,
                explanation = "The Sahara stretches across North Africa, from the Atlantic to the Red Sea."
            });
            seeds.Add(new Seed
            {
                id = "Geo_008", category = QuestionCategory.Geography, difficulty = QuestionDifficulty.ActI,
                question = "What is the capital of France?",
                options = new[] { "Lyon", "Marseille", "Paris", "Nice" },
                correctIndex = 2,
                explanation = "Paris is the capital and largest city of France, on the River Seine."
            });
            seeds.Add(new Seed
            {
                id = "Geo_009", category = QuestionCategory.Geography, difficulty = QuestionDifficulty.ActI,
                question = "Which continent is Brazil part of?",
                options = new[] { "North America", "Africa", "South America", "Europe" },
                correctIndex = 2,
                explanation = "Brazil is the largest country in South America."
            });
            seeds.Add(new Seed
            {
                id = "Geo_010", category = QuestionCategory.Geography, difficulty = QuestionDifficulty.ActII,
                question = "Mount Everest lies in which mountain range?",
                options = new[] { "The Andes", "The Alps", "The Himalayas", "The Rockies" },
                correctIndex = 2,
                explanation = "Everest sits in the Himalayas on the border between Nepal and China."
            });
        }

        private static void AddLan(List<Seed> seeds)
        {
            seeds.Add(new Seed
            {
                id = "Lan_001", category = QuestionCategory.Language, difficulty = QuestionDifficulty.ActI,
                question = "Which word is a synonym of \"brave\"?",
                options = new[] { "Fearful", "Timid", "Courageous", "Careless" },
                correctIndex = 2,
                explanation = "Courageous means the same as brave: showing no fear in a hard situation."
            });
            seeds.Add(new Seed
            {
                id = "Lan_002", category = QuestionCategory.Language, difficulty = QuestionDifficulty.ActI,
                question = "In the sentence \"The cat slept quietly,\" which word is an adverb?",
                options = new[] { "The", "cat", "slept", "quietly" },
                correctIndex = 3,
                explanation = "Adverbs describe how an action happens: quietly tells how the cat slept."
            });
            seeds.Add(new Seed
            {
                id = "Lan_003", category = QuestionCategory.Language, difficulty = QuestionDifficulty.ActII,
                question = "Which sentence is grammatically correct?",
                options = new[] { "She don't like tea.", "He go to school every day.", "They are reading a book.", "I has a pencil." },
                correctIndex = 2,
                explanation = "\"They are reading\" agrees with its subject. The others break subject-verb agreement."
            });
            seeds.Add(new Seed
            {
                id = "Lan_004", category = QuestionCategory.Language, difficulty = QuestionDifficulty.ActIII,
                question = "The sentence \"Time is a thief\" is a metaphor. What does it suggest?",
                options = new[] { "Time can steal objects", "Time passes and quietly takes moments away", "Clocks should be hidden", "Time is a dangerous animal" },
                correctIndex = 1,
                explanation = "The metaphor compares time to a thief to suggest it quietly takes away moments and years."
            });
            seeds.Add(new Seed
            {
                id = "Lan_005", category = QuestionCategory.Language, difficulty = QuestionDifficulty.ActI,
                question = "What is the past tense of the verb \"go\"?",
                options = new[] { "goed", "went", "gone", "going" },
                correctIndex = 1,
                explanation = "\"Go\" is irregular: today I go, yesterday I went, I have gone."
            });
            seeds.Add(new Seed
            {
                id = "Lan_006", category = QuestionCategory.Language, difficulty = QuestionDifficulty.ActII,
                question = "What is a group of lions called?",
                options = new[] { "A herd", "A pride", "A flock", "A pack" },
                correctIndex = 1,
                explanation = "A group of lions is a pride; a group of wolves is a pack and a group of sheep is a flock."
            });
            seeds.Add(new Seed
            {
                id = "Lan_007", category = QuestionCategory.Language, difficulty = QuestionDifficulty.ActI,
                question = "What is the plural of the noun \"child\"?",
                options = new[] { "childs", "childes", "children", "childrens" },
                correctIndex = 2,
                explanation = "\"Child\" has the irregular plural \"children\"."
            });
            seeds.Add(new Seed
            {
                id = "Lan_008", category = QuestionCategory.Language, difficulty = QuestionDifficulty.ActII,
                question = "Which word is the opposite of \"ancient\"?",
                options = new[] { "old", "modern", "historic", "antique" },
                correctIndex = 1,
                explanation = "Ancient means very old; modern means belonging to the present time."
            });
            seeds.Add(new Seed
            {
                id = "Lan_009", category = QuestionCategory.Language, difficulty = QuestionDifficulty.ActII,
                question = "Which of these is a complete sentence?",
                options = new[] { "Running down the street.", "Because it was late.", "She runs every morning.", "The big red ball." },
                correctIndex = 2,
                explanation = "\"She runs every morning\" has a subject and a verb and expresses a complete thought."
            });
            seeds.Add(new Seed
            {
                id = "Lan_010", category = QuestionCategory.Language, difficulty = QuestionDifficulty.ActII,
                question = "A simile compares two things using which words?",
                options = new[] { "\"is\" or \"was\"", "\"like\" or \"as\"", "\"and\" or \"but\"", "\"not\" or \"never\"" },
                correctIndex = 1,
                explanation = "A simile says one thing is like another (\"as brave as a lion\"); a metaphor states it directly."
            });
        }

        private static void AddHea(List<Seed> seeds)
        {
            seeds.Add(new Seed
            {
                id = "Hea_001", category = QuestionCategory.Health, difficulty = QuestionDifficulty.ActI,
                question = "Which nutrient is the body's main quick source of energy?",
                options = new[] { "Carbohydrates", "Iron", "Vitamin C", "Water" },
                correctIndex = 0,
                explanation = "Carbohydrates break down into glucose, the body's fastest source of fuel."
            });
            seeds.Add(new Seed
            {
                id = "Hea_002", category = QuestionCategory.Health, difficulty = QuestionDifficulty.ActI,
                question = "How many hours of sleep do most teenagers need each night?",
                options = new[] { "4-5 hours", "6-7 hours", "8-10 hours", "12-14 hours" },
                correctIndex = 2,
                explanation = "Sleep guidelines recommend 8-10 hours a night for teenagers aged 13 to 18."
            });
            seeds.Add(new Seed
            {
                id = "Hea_003", category = QuestionCategory.Health, difficulty = QuestionDifficulty.ActII,
                question = "Why is washing hands with soap better than washing with water alone?",
                options = new[] { "Soap makes hands slippery", "Soap removes oils and dirt that water alone leaves behind", "Soap adds healthy bacteria to the skin", "Soap dries out the germs' skin" },
                correctIndex = 1,
                explanation = "Soap lifts the oils and dirt that trap germs so they rinse away."
            });
            seeds.Add(new Seed
            {
                id = "Hea_004", category = QuestionCategory.Health, difficulty = QuestionDifficulty.ActIII,
                question = "A student skips breakfast and by mid-morning feels dizzy and cannot concentrate. What is the most likely cause?",
                options = new[] { "Drinking too much water", "Low blood sugar because she has not eaten", "Too much screen time", "Sleeping too much" },
                correctIndex = 1,
                explanation = "Skipping meals can drop blood glucose, and the brain needs glucose to focus."
            });
            seeds.Add(new Seed
            {
                id = "Hea_005", category = QuestionCategory.Health, difficulty = QuestionDifficulty.ActI,
                question = "Which vitamin can the body make when skin is exposed to sunlight?",
                options = new[] { "Vitamin A", "Vitamin C", "Vitamin D", "Vitamin B12" },
                correctIndex = 2,
                explanation = "Sunlight on the skin helps the body produce vitamin D, which is needed for strong bones."
            });
            seeds.Add(new Seed
            {
                id = "Hea_006", category = QuestionCategory.Health, difficulty = QuestionDifficulty.ActI,
                question = "Which of these foods is a good source of vitamin C?",
                options = new[] { "Oranges", "White rice", "Butter", "Table salt" },
                correctIndex = 0,
                explanation = "Citrus fruits such as oranges are rich in vitamin C, which supports the immune system and helps the body absorb iron."
            });
            seeds.Add(new Seed
            {
                id = "Hea_007", category = QuestionCategory.Health, difficulty = QuestionDifficulty.ActII,
                question = "How much moderate physical activity should teenagers aim for each day?",
                options = new[] { "10 minutes", "30 minutes", "60 minutes", "3 hours" },
                correctIndex = 2,
                explanation = "Health guidelines recommend about 60 minutes of moderate-to-vigorous activity daily for teenagers."
            });
            seeds.Add(new Seed
            {
                id = "Hea_008", category = QuestionCategory.Health, difficulty = QuestionDifficulty.ActI,
                question = "Which drink is the best everyday choice for hydration?",
                options = new[] { "Soda", "Water", "Energy drink", "Sweet tea" },
                correctIndex = 1,
                explanation = "Water hydrates without added sugar, caffeine or extra calories."
            });
            seeds.Add(new Seed
            {
                id = "Hea_009", category = QuestionCategory.Health, difficulty = QuestionDifficulty.ActII,
                question = "Which blood cells help the body fight infection?",
                options = new[] { "Red blood cells", "White blood cells", "Platelets", "Plasma cells" },
                correctIndex = 1,
                explanation = "White blood cells recognise and attack pathogens; red blood cells carry oxygen."
            });
            seeds.Add(new Seed
            {
                id = "Hea_010", category = QuestionCategory.Health, difficulty = QuestionDifficulty.ActI,
                question = "Lentils are mainly a source of which nutrient?",
                options = new[] { "Protein", "Vitamin C", "Calcium", "Fat" },
                correctIndex = 0,
                explanation = "Lentils are legumes rich in plant protein and fibre, with very little fat."
            });
        }
    }
}
