# LIGHT SHADOW - Voice Line Sheet (A-workstream)

Generated 2026-11-27 from the repo's actual story-node inventory,
`Assets/_Project/Data/Story/SN_*.asset` (44 nodes - NOT from Docs/Story/SCRIPT.md,
which is the human-readable twin and has drifted in places). Arabic resolved from
`StoryStrings.cs` then `Localization.cs` (runtime lookup order; whitespace-normalized
matching). Speakers per `StoryNode.StorySpeaker` enum.

Inventory reconciliation: `Assets/_Project/Data/Story/` holds 45 `.asset` files =
44 story nodes (SN_*) + `SO_StoryConfig.asset`. The 44 vs 45 count difference is the
config asset, not a missing node.

Method: line IDs `VO_<nodeId>` for node lines, `VO_<nodeId>_C<n>` for choice lines,
in asset order. Emotion is a SUGGESTED read for casting/direction only. Max seconds =
EN words / 2.6 wps, min 2s, rounded to 0.5s - a recording budget, not a runtime rule.
Timed choices also carry their in-game cap. Documentation only: nothing is wired and
VO_RECORDING_SPECS.md (PR #7 scope) is untouched.

Coverage: 77 lines from 44 nodes; Arabic present for 73, MISSING for 4.
The 4 missing are exactly the ending nodes (SN_End_Dawnless, SN_End_FullRecovery,
SN_End_HollowEscape, SN_End_PartialMemory): their asset text has drifted from the
StoryStrings.cs keys (e.g. "The sky over the school..." vs the key's "The sky
above the school..."), so no matching Arabic entry exists in the codebase for them.
Either the assets or the StoryStrings keys need reconciling before those lines can
be localized.

## Act I

| line ID | node | speaker | emotion (suggested) | EN text | AR text | max sec | cap |
| --- | --- | --- | --- | --- | --- | --- | --- |
| VO_SN_Act1_01 | SN_Act1_01 | Nour | rising panic | The gate slams behind me. Of course it does. Schools only open all the way for the last time. | تُصفق البوابة خلفي بعنف. طبعًا تفعل. المدارس لا تفتح على مصراعيها إلا في آخر مرة. | 7.5 | - |
| VO_SN_Act1_01_C1 | SN_Act1_01 | Nour | player choice (Nour inner voice) | Steady. Count the windows that still hold light: three. | اهدئي. عُدّي النوافذ التي ما زالت تحتفظ بنورها: ثلاث. | 3.5 | - |
| VO_SN_Act1_01_C2 | SN_Act1_01 | Nour | player choice (Nour inner voice) | Walk like I'm only late for her class. | امشي كأنني تأخرت عن حصتها فقط. | 3 | - |
| VO_SN_Act1_01_C3 | SN_Act1_01 | Nour | player choice (Nour inner voice) | Say sorry to the dark. Quietly, so it knows my hands are empty. | اعتذري للعتمة. بهمس، لتعرف أن يديّ خاويستان. | 5 | - |
| VO_SN_Act1_01b | SN_Act1_01b | Nour | quiet hope | Three wings, three fuse boxes, six pieces of her. The crew comes at dawn. All right. One room at a time. | ثلاثة أجنحة، ثلاثة صناديق مصاهر، وست قطع منها. الفريق يأتي عند الفجر. حسنًا. غرفة تلو الأخرى. | 8 | - |
| VO_SN_Act1_02 | SN_Act1_02 | Nour | tender grief | On her desk: the empty memory box, and one card in her handwriting. "Put me back together." | على مكتبها: صندوق الذكريات فارغًا، وبطاقة واحدة بخط يدها. «أعدني كما كنت.» | 6.5 | - |
| VO_SN_Act1_02_C1 | SN_Act1_02 | Nour | player choice (Nour inner voice) | Slide the card into my pocket. Evidence first. | أدسّ البطاقة في جيبي. الأدلة أولًا. | 3 | - |
| VO_SN_Act1_02_C2 | SN_Act1_02 | Nour | player choice (Nour inner voice) | Read it aloud. Like she's listening. | أقرأها بصوت عالٍ. كأنها تسمع. | 2.5 | timed 7s |
| VO_SN_Act1_02_C3 | SN_Act1_02 | Nour | player choice (Nour inner voice) | Leave it on the desk. I'll take it when the box is full. | أتركها على المكتب. آخذها حين يمتلئ الصندوق. | 5 | - |
| VO_SN_Act1_02b | SN_Act1_02b | Nour | quiet resolve | Six fragments, three fuse boxes, one gate. I know the shape of this night. I have been studying for it my whole life. | ست شذرات، ثلاثة صناديق مصاهر، بوابة واحدة. أعرف هيئة هذه الليلة. درستُ لها طول عمري. | 9 | - |
| VO_SN_Act1_02t | SN_Act1_02t | Nour | rising panic | The words stay behind my teeth. Maybe next time. | تبقى الكلمات خلف أسناني. ربما في المرة القادمة. | 3.5 | - |
| VO_SN_Act1_03 | SN_Act1_03 | Mother | warm, instructive (memory) | First the wiring, then the reading. One room at a time - that is how you eat a school. | أولًا الأسلاك ثم القراءة. غرفة تلو الأخرى - هكذا تُؤكَل مدرسة. | 7.5 | - |
| VO_SN_Act1_03_C1 | SN_Act1_03 | Mother | player choice (Nour inner voice) | Stay in the light while it hums. | أبقي في النور وهو يطن. | 2.5 | - |
| VO_SN_Act1_03_C2 | SN_Act1_03 | Mother | player choice (Nour inner voice) | Step back into the corridor. Look at the dark properly. | أرجع خطوة إلى الممر. أنظر إلى العتمة كما ينبغي. | 4 | - |
| VO_SN_Act1_03_C3 | SN_Act1_03 | Mother | player choice (Nour inner voice) | Turn the flashlight off. Just to hear what the dark says. | أطفئ المصباح. فقط لأسمع ما تقوله العتمة. | 4 | timed 8s |
| VO_SN_Act1_03a | SN_Act1_03a | Nour | tender grief | Warm light on white sheets. Her classroom smelled like this - chalk, jasmine, the inside of a book. | نور دافئ على أغطية بيضاء. هكذا كانت تشمّ صفها - طباشير وياسمين ودفء الكتب. | 7 | - |
| VO_SN_Act1_03b | SN_Act1_03b | Shadow | menacing, sibilant whisper | It stands at the end of the corridor the way a sentence stands when one word has been rubbed out. My beam touches it and it slides away. Politely. | يقف في نهاية الممر كما تقف الجملة حين يُمحى منها كلمة. يمسّه شعاعي فينسحب. بأدب. | 11 | - |
| VO_SN_Act1_04 | SN_Act1_04 | Nour | tender grief | A pressed-flower bookmark, left in a book she never re-shelved. Her hand underneath: "Return by Thursday." I am four years late. | فاصل كتاب بزهرة مضغوطة، تركته في كتاب لم تعدِه إلى رفّه. بخطها أسفله: «يُعاد يوم الخميس». أنا متأخرة أربع سنوات. | 8 | - |
| VO_SN_Act1_04_C1 | SN_Act1_04 | Nour | player choice (Nour inner voice) | Press it between the lesson plans. Where it belongs. | أضغطه بين خطط الدروس. حيث ينتمي. | 3.5 | - |
| VO_SN_Act1_04_C2 | SN_Act1_04 | Nour | player choice (Nour inner voice) | Keep it in my hand a while. | أبقيه في يدي قليلًا. | 2.5 | - |
| VO_SN_Act1_04b | SN_Act1_04b | Mother | warm, instructive (memory) | A library card is a key that never rusts. | بطاقة المكتبة مفتاح لا يصدأ. | 3.5 | - |
| VO_SN_Act1_05 | SN_Act1_05 | Nour | quiet resolve | My exam paper. Red pen across the top: "You see more than you think." She marked sixty papers that week. She remembered mine. | ورقتي. قلم أحمر عبر الأعلى: «أنتِ ترين أكثر مما تظنين». صنّفت ستين ورقة ذلك الأسبوع. وتذكرت ورقتي. | 9 | - |
| VO_SN_Act2_01 | SN_Act2_01 | Nour | quiet resolve | Fuse two hums awake. Through the lab window, on the lit floor: two shadows. Mine - and one more, edges smudged like an eraser was passed over it. | الصندوق الثاني يطن وقد استيقظ. عبر نافذة المختبر، على الأرضية المضيئة: ظلّان. ظلي - وآخر، حوافّه ممسوحة كأن ممحاةً مرّت عليها. | 11 | - |
| VO_SN_Act2_01_C1 | SN_Act2_01 | Nour | player choice (Nour inner voice) | Call her name into the wing. | أنادي اسمها في الجناح. | 2.5 | timed 8s |
| VO_SN_Act2_01_C2 | SN_Act2_01 | Nour | player choice (Nour inner voice) | Count the fuse wires. Order is a kind of courage. | أعدّ أسلاك المصهر. الترتيب نوع من الشجاعة. | 4 | - |
| VO_SN_Act2_01_C3 | SN_Act2_01 | Nour | player choice (Nour inner voice) | Keep walking. Windows lie at this hour. | أواصل المشي. النوافذ تكذب في مثل هذه الساعة. | 2.5 | - |
| VO_SN_Act2_01_anger | SN_Act2_01_anger | Shadow | menacing, sibilant whisper | The whisper folds in on itself, chalk under a door, static rising. Somewhere down the wing a poster goes blank. | يلتف الهمس على نفسه، صرير طباشير تحت باب، وتذبذب يعلو. في مكان ما من الجناح تفقد ملصقةٌ كلَّ ما عليها. | 7.5 | - |
| VO_SN_Act2_01_calm | SN_Act2_01_calm | Shadow | menacing, sibilant whisper | It answers with a name. Almost mine. The vowels are worn like stairs. "...N-oor. Nour." It has been practicing. | يجيب باسم. شبه اسمي. حروفه الممدودة بالية كالدرج. «...ن-ور. نور». لقد كان يتدرب. | 7.5 | - |
| VO_SN_Act2_01_pass | SN_Act2_01_pass | Nour | tender grief | The science wing is awake. One fuse box left, and the staff room that remembers her red pen. | جناح العلوم صحي. بقي صندوق مصاهر واحد، وغرفة المعلمين التي تتذكر قلمها الأحمر. | 7 | - |
| VO_SN_Act2_02 | SN_Act2_02 | Nour | tender grief | The garden bench. A jasmine pressed flat in her book of poems, and a photograph of this exact bench, her hand caught in the corner of the frame. | مقعد الحديقة. ياسمين مضغوط في كتاب قصائدها، وصورة لهذا المقعد بعينه، ويدها عالقة في زاوية الكادر. | 11 | - |
| VO_SN_Act2_02_C1 | SN_Act2_02 | Nour | player choice (Nour inner voice) | Sit for one minute. The Shadow slows near light; grief slows near benches. | أجلس دقيقة واحدة. الظل يتباطأ قرب النور؛ والحزن يتباطأ قرب المقاعد. | 5 | timed 9s |
| VO_SN_Act2_02_C2 | SN_Act2_02 | Nour | player choice (Nour inner voice) | Take the photograph. Leave the jasmine where it grew. | آخذ الصورة. وأترك الياسمين حيث نما. | 3.5 | - |
| VO_SN_Act2_02_C3 | SN_Act2_02 | Nour | player choice (Nour inner voice) | Both in the box. This bench will not exist by Friday. | كلاهما في الصندوق. لن يبقى هذا المقعد بعد الجمعة. | 4 | - |
| VO_SN_Act2_02b | SN_Act2_02b | Mother | warm, instructive (memory) | Water the jasmine on Thursdays. Talk to it. Plants are students that never interrupt. | اسقي الياسمين أيام الخميس. حدّثيه. النباتات تلاميذ لا يقاطعون أبدًا. | 5.5 | - |
| VO_SN_Act2_02t | SN_Act2_02t | Nour | quiet resolve | I take the photograph and keep standing. Standing is easier. | آخذ الصورة وأبقى واقفة. الوقوف أسهل. | 4 | - |
| VO_SN_Act2_03 | SN_Act2_03 | Nour | tender grief | Pinned inside the optics cabinet, in her lesson-plan voice: "Light travels in straight lines - so do stories." | مثبّتة داخل خزانة البصريات، بصوت خطط دروسها: «الضوء يسير في خطوط مستقيمة - وكذلك الحكايات.» | 7 | - |
| VO_SN_Act2_03_C1 | SN_Act2_03 | Nour | player choice (Nour inner voice) | Read it aloud, exactly as written. Precision is a kind of prayer. | أقرؤها بصوت عالٍ كما كُتبت. الدقة نوع من الصلاة. | 4.5 | timed 8s |
| VO_SN_Act2_03_C2 | SN_Act2_03 | Nour | player choice (Nour inner voice) | Copy it onto my hand. Ink is portable. | أنسخها على يدي. الحبر قابل للحمل. | 3 | - |
| VO_SN_Act2_03b | SN_Act2_03b | Mother | warm, instructive (memory) | ...and so do stories. Straight lines, even through walls. Especially through walls. | ...وكذلك الحكايات. خطوط مستقيمة، حتى عبر الجدران. خصوصًا عبر الجدران. | 4.5 | - |
| VO_SN_Act2_04 | SN_Act2_04 | Shadow | menacing, sibilant whisper | It stands in the assembly hall aisle with a roll call in its mouth. Names, most of them worn past reading. One it has kept too well: mine. | يقف في ممر القاعة ودفّة أسماء في فمه. أسماء، أغلبها بالي حتى تعذّر قراءتها. اسمًا واحدًا حفظه أكثر من اللازم: اسمي. | 11 | - |
| VO_SN_Act2_04_C1 | SN_Act2_04 | Shadow | player choice (Nour inner voice) | Answer it. "That one's mine. You can't have it." | أجبه. «هذا اسمي. لن تأخذه.» | 3.5 | timed 10s |
| VO_SN_Act2_04_C2 | SN_Act2_04 | Shadow | player choice (Nour inner voice) | Recite her lesson line: "Light travels in straight lines." | أرتل سطر درسها: «الضوء يسير في خطوط مستقيمة.» | 3.5 | - |
| VO_SN_Act2_04_C3 | SN_Act2_04 | Shadow | player choice (Nour inner voice) | Hide behind the folded chairs. Let it pass. | أختبئ خلف الكراسي المطوية. وأدعه يمر. | 3 | - |
| VO_SN_Act2_04_line | SN_Act2_04_line | Nour | quiet resolve | I say it like a student who did the reading. Straight lines, even through walls. The Shadow thins to a smudge and is gone down the dark aisle. | أقولها كطالبة أعدت الدرس. خطوط مستقيمة، حتى عبر الجدران. يرقّ الظل حتى يصير بسمة ثم يزول في الممر المظلم. | 11 | - |
| VO_SN_Act2_04_pass | SN_Act2_04_pass | Nour | quiet resolve | Chalk static. Folded chairs. It passes close enough to read the absent look on its absent face. I hold still until the school forgets me again. | تذبذب كالطباشير. كراسٍ مطوية. يمر قريبًا بما يكفي لأقرأ النظرة الغائبة في وجهه الغائب. أثبت حتى تنساني المدرسة من جديد. | 10 | - |
| VO_SN_Act2_04_shaken | SN_Act2_04_shaken | Shadow | menacing, sibilant whisper | My voice hits it and comes back wrong, missing its middle. The whisper sharpens. A lamp dims somewhere without being touched. | يصطدم صوتي به ويعود مشوّهًا، ناقصًا وسطه. يزداد الهمس حدّة. مصباح ما يخفت دون أن يلمسه أحد. | 8 | - |
| VO_SN_Act2_04_stand | SN_Act2_04_stand | Shadow | menacing, sibilant whisper | "...kept. Kept it. Kept-" It folds my name carefully, like paper, and sets it down between us. Down the aisle: a door I never once used. The back stairs. I know the way now. | «...حفظت. حفظتُه. حفظ-» يطوي اسمي بعناية كورقة، ويضعه بيننا. في آخر الممر: باب لم أستخدمه قط. الدرج الخلفي. أعرف الطريق الآن. | 13 | - |
| VO_SN_Act3_01 | SN_Act3_01 | Nour | tender grief | Fuse three. The staff room lights up row by row, and there is her mug beside the sink, and a note she left a colleague: "Back Thursday - the kettle forgives you." | المصهر الثالث. تضيء غرفة المعلمين صفًا صفًا، وهناك كوبها بجانب المغسلة، وورقة تركتها لزميلة: «أعود يوم الخميس - الغلاية تسامحك». | 12.5 | - |
| VO_SN_Act3_01_C1 | SN_Act3_01 | Nour | player choice (Nour inner voice) | Pour the cold tea out first. Then take the mug. | أصبّ الشاي البارد أولًا. ثم آخذ الكوب. | 4 | - |
| VO_SN_Act3_01_C2 | SN_Act3_01 | Nour | player choice (Nour inner voice) | Take the mug. The tea can be tomorrow's problem. | آخذ الكوب. الشاي مشكلة الغد. | 3.5 | - |
| VO_SN_Act3_01_C3 | SN_Act3_01 | Nour | player choice (Nour inner voice) | Leave everything. The room remembers her better with it here. | أترك كل شيء. الغرفة تتذكرها أكثر بوجوده هنا. | 4 | - |
| VO_SN_Act3_01b | SN_Act3_01b | Mother | warm, instructive (memory) | Keep the light on for the ones who come after. | أبقي النور مشتعلًا لمن يأتون من بعد. | 4 | - |
| VO_SN_Act3_02 | SN_Act3_02 | System | flat, systemic | The gate crank is live. The corridor to the front doors is a lit runway. The Shadow knows it too. | مفتاح البوابة صار حيًا. الممر إلى الأبواب الأمامية مدرج مضيء. الظل يعرف ذلك أيضًا. | 7.5 | - |
| VO_SN_Act3_03 | SN_Act3_03 | Shadow | menacing, sibilant whisper | Between me and the gate it has stopped pretending to be smoke. It is the school's whole forgetting, standing up: blank posters, erased names, a poem with its middle rubbed out. | بيني وبين البوابة توقف عن تظاهره بأنه دخان. إنه نسيان المدرسة كله، واقفًا: ملصقات فارغة، أسماء ممحوة، قصيدة مُحيت وسطها. | 12 | - |
| VO_SN_Act3_03_C1 | SN_Act3_03 | Shadow | player choice (Nour inner voice) | Say her name back to it. "Her name was-" | أعيد عليه اسمها. «اسمها كان-» | 3.5 | timed 10s |
| VO_SN_Act3_03_C2 | SN_Act3_03 | Shadow | player choice (Nour inner voice) | Erase it first. Swing the beam like an answer. | أمحيه أنا أولًا. ألوّح بالشعاع كأنه إجابة. | 3.5 | - |
| VO_SN_Act3_03_C3 | SN_Act3_03 | Shadow | player choice (Nour inner voice) | Walk past. It cannot enter the lit corridor. | أمرّ من جانبه. لا يمكنه دخول الممر المضيء. | 3 | - |
| VO_SN_Act3_03_bold | SN_Act3_03_bold | Nour | quiet hope | Light is a straight line and I am the line. It comes apart like chalk dust, and the dust is gone before it lands. | النور خط مستقيم وأنا الخط. يتفكك كغبار الطباشير، ويزول الغبار قبل أن يستقر. | 9 | - |
| VO_SN_Act3_03_forgave | SN_Act3_03_forgave | Shadow | menacing, sibilant whisper | "...we kept everything. We kept-" It holds its shape one moment longer, the way a class holds its breath before the bell. Then it lets the morning go. | «...حفظنا كل شيء. حفظن-» يبقى على هيئته لحظة إضافية، كما تحبس فصلٌ أنفاسه قبل الجرس. ثم يترك الصباح يذهب. | 11 | - |
| VO_SN_Act3_03_pass | SN_Act3_03_pass | Nour | rising panic | It stands aside the way furniture stands aside in a school you no longer attend. I keep to the light. I do not run. Not yet. | ينزاح جانبًا كما تنزاح الأثاث في مدرسة لم تعودي إليها. ألتزم بالنور. لا أركض. ليس بعد. | 10 | - |
| VO_SN_Act3_03_shaken | SN_Act3_03_shaken | Shadow | menacing, sibilant whisper | The name comes out with holes in it. The Shadow leans close - courteous, terrible - and takes the holes as its fee. | يخرج الاسم وفيه ثقوب. ينحني الظل قريبًا - مؤدبًا، مرعبًا - ويتخذ الثقوب أجرته. | 9 | - |
| VO_SN_Act3_04 | SN_Act3_04 | Nour | rising panic | The windows are turning grey. The gate is one lit runway away, and my legs are a student's legs: they remember every corridor of this school. | تصفرّ النوافذ رمادية. البوابة على بعد مدرج مضيء واحد، وساقاي ساقا طالبة: تتذكران كل ممر في هذه المدرسة. | 10 | - |
| VO_SN_Act3_04_C1 | SN_Act3_04 | Nour | player choice (Nour inner voice) | Run the lit corridor. Straight lines only. | أركض في الممر المضيء. خطوط مستقيمة فقط. | 2.5 | timed 9s |
| VO_SN_Act3_04_C2 | SN_Act3_04 | Nour | player choice (Nour inner voice) | The back stairs I never used. Faster, darker. Mine. | الدرج الخلفي الذي لم أستخدمه قط. أسرع، أظلم. ملكي. | 3.5 | - |
| VO_SN_Act3_04t | SN_Act3_04t | Nour | quiet resolve | Decide with your feet. | قرّري برجليك. | 2 | - |
| VO_SN_Act3_05 | SN_Act3_05 | System | flat, systemic | The corridor lights come on ahead of me, one by one, like roll call. | أضواء الممر تشتعل أمامي، واحدة تلو الأخرى، كنداء الأسماء. | 5.5 | - |
| VO_SN_Act3_06 | SN_Act3_06 | Mother | warm, instructive (memory) | Faster, habibti. The bell does not wait for the slow - and neither does the morning. | أسرع يا حبيبتي. الجرس لا ينتظر البطيء - ولا الصباح. | 6 | - |
| VO_SN_Act3_07 | SN_Act3_07 | Nour | quiet hope | The back stairs take the dark in jumps: three flights, a door with a push-bar, and the front hall roaring up to meet me. | يأخذني الدرج الخلفي عبر العتمة نطّات: ثلاثة طوابق، باب بمزلاج ضغط، والبهو الأمامي يزأر ليلاقيني. | 9 | - |
| VO_SN_Act3_08 | SN_Act3_08 | Nour | quiet resolve | The crank is cold and eager. Before I turn it: one promise, out loud, because the school is listening, and schools keep records. | المفتاح بارد ومتشوق. قبل أن ألويه: وعد واحد بصوت عالٍ، لأن المدرسة تسمع، والمدارس تحفظ السجلات. | 9 | - |
| VO_SN_Act3_08_C1 | SN_Act3_08 | Nour | player choice (Nour inner voice) | I'll keep the light on. For the ones who come after. | سأبقي النور مشتعلًا. لمن يأتون من بعد. | 4 | - |
| VO_SN_Act3_08_C2 | SN_Act3_08 | Nour | player choice (Nour inner voice) | I'll come back with a key, not a fence to climb. | سأعود بمفتاح، لا بسياج أتسلقه. | 4 | - |
| VO_SN_Act3_08_C3 | SN_Act3_08 | Nour | player choice (Nour inner voice) | Say nothing. Some things should stay in the building. | أصمت. بعض الأشياء ينبغي أن تبقى في المبنى. | 3.5 | - |
| VO_SN_Act3_09 | SN_Act3_09 | Mother | warm, instructive (memory) | Go on, then. Knowledge is the only light that follows a person into the dark. | انطلقي إذاً. المعرفة هي النور الوحيد الذي يتبع المرء في العتمة. | 6 | - |
| VO_SN_End_Dawnless | SN_End_Dawnless | System | flat, systemic | 'The corridor ends at the gate and the gate does not open fast enough. The crews'' vans turn the corner. The sky does its morning work. | **MISSING AR (no pair in StoryStrings/Localization)** | 10 | - |
| VO_SN_End_FullRecovery | SN_End_FullRecovery | System | flat, systemic | 'The gate closes behind her. The sky over the school turns from black to grey to a thin, clean gold. | **MISSING AR (no pair in StoryStrings/Localization)** | 7.5 | - |
| VO_SN_End_HollowEscape | SN_End_HollowEscape | System | flat, systemic | 'The gate closes behind her with a sound like a full stop. | **MISSING AR (no pair in StoryStrings/Localization)** | 4.5 | - |
| VO_SN_End_PartialMemory | SN_End_PartialMemory | System | flat, systemic | 'The gate closes. The dawn is ordinary and enormous. | **MISSING AR (no pair in StoryStrings/Localization)** | 3.5 | - |
