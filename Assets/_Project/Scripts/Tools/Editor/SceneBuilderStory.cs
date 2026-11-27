using System;
using System.Collections.Generic;
using LightShadow.Audio;
using LightShadow.Core;
using LightShadow.Story;
using LightShadow.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Tools
{
    // Story-pass builder (premium pass, decisions D4/D12/D13/D15). Two jobs:
    //
    //   1. BuildStoryAssets() - compiles the content table below (the authored
    //      graph from Docs/Story/SCRIPT.md) into StoryNode + SO_StoryConfig
    //      assets under Assets/_Project/Data/Story/. Idempotent: existing
    //      assets are updated in place, never duplicated.
    //   2. DecorateStoryScenes() - opens 01_MainMenu / 02_School / 03_Ending,
    //      adds the StoryDirector, DialogueUIController, AdaptiveMusic, the
    //      save-slot panel, the settings page B rows and the recap ending row,
    //      then saves. All UI is generated in the SceneBuilderUI style - no
    //      hand-written YAML anywhere.
    //
    // Run via "LightShadow > 9. Story, Dialogue, Saves & Audio", or
    // SceneBuilder.BuildStoryBatch() (see Docs/AGENT_REPORT_NARRATIVE.md for
    // the Lead's exact ordering).
    public static partial class SceneBuilder
    {
        const string StoryFolder = "Assets/_Project/Data/Story";
        const string StoryConfigPath = StoryFolder + "/SO_StoryConfig.asset";

        [MenuItem("LightShadow/9. Story, Dialogue, Saves & Audio", priority = 9)]
        public static void BuildStoryMenu()
        {
            string summary = BuildStoryAll();
            Debug.Log("[SceneBuilderStory] " + summary);
        }

        // Batch entry point for the Lead TD (matches the other *.Batch commands):
        //   Unity.exe -batchmode -quit -nographics -projectPath .
        //     -executeMethod LightShadow.Tools.SceneBuilder.BuildStoryBatch
        public static void BuildStoryBatch()
        {
            Debug.Log("[SceneBuilderStory] " + BuildStoryAll());
        }

        public static string BuildStoryAll()
        {
            EnsureFolder(StoryFolder);
            StoryConfig config = BuildStoryAssets();

            string summary = "story assets: " + config.nodes.Length + " nodes at " + StoryFolder;

            summary += "\n" + DecorateMainMenuScene(config);
            summary += "\n" + DecorateSchoolScene(config);
            summary += "\n" + DecorateEndingScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return summary;
        }

        // ================================================================ assets

        static StoryConfig BuildStoryAssets()
        {
            StoryConfig config = AssetDatabase.LoadAssetAtPath<StoryConfig>(StoryConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<StoryConfig>();
                AssetDatabase.CreateAsset(config, StoryConfigPath);
            }

            var nodes = new List<StoryNode>(StoryContent.Count);
            foreach (NodeDef def in StoryContent)
            {
                nodes.Add(UpsertNode(def));
            }

            config.nodes = nodes.ToArray();
            EditorUtility.SetDirty(config);
            return config;
        }

        static StoryNode UpsertNode(NodeDef def)
        {
            string path = StoryFolder + "/" + def.id + ".asset";
            StoryNode node = AssetDatabase.LoadAssetAtPath<StoryNode>(path);
            if (node == null)
            {
                node = ScriptableObject.CreateInstance<StoryNode>();
                AssetDatabase.CreateAsset(node, path);
            }

            node.nodeId = def.id;
            node.speaker = def.speaker;
            node.trigger = def.trigger;
            node.triggerOrdinal = def.ordinal;
            node.text = def.text;
            node.nextNodeId = def.next ?? "";
            node.onEnterEventHint = def.hint ?? "";
            node.isEndingNode = def.isEnding;
            node.endingId = def.endingId ?? "";
            node.localizationKeys = def.extraKeys ?? Array.Empty<string>();
            node.enterSetFlags = SplitFlags(def.enterSet);
            node.enterBoldness = def.enterB;
            node.enterTrust = def.enterT;
            node.enterCalm = def.enterC;

            if (def.choices == null || def.choices.Length == 0)
            {
                node.choices = Array.Empty<StoryNode.StoryChoice>();
            }
            else
            {
                var choices = new StoryNode.StoryChoice[def.choices.Length];
                for (int i = 0; i < def.choices.Length; i++)
                {
                    ChoiceDef c = def.choices[i];
                    choices[i] = new StoryNode.StoryChoice
                    {
                        text = c.text,
                        nextNodeId = c.next ?? "",
                        nextIfCalmNegative = c.nextIfCalmNegative ?? "",
                        setFlags = SplitFlags(c.set),
                        requiredFlags = SplitFlags(c.req),
                        blockedFlags = SplitFlags(c.block),
                        boldness = c.b,
                        trust = c.t,
                        calm = c.c,
                        isTimed = c.timed,
                        timeSeconds = c.seconds,
                        timeoutNextId = c.timeout ?? "",
                    };
                }
                node.choices = choices;
            }

            EditorUtility.SetDirty(node);
            return node;
        }

        static string[] SplitFlags(string commaSeparated)
        {
            if (string.IsNullOrEmpty(commaSeparated)) return Array.Empty<string>();
            string[] parts = commaSeparated.Split(',');
            var flags = new List<string>(parts.Length);
            foreach (string part in parts)
            {
                string trimmed = part.Trim();
                if (!string.IsNullOrEmpty(trimmed)) flags.Add(trimmed);
            }
            return flags.ToArray();
        }

        // ------------------------------------------------------- content DSL --

        sealed class ChoiceDef
        {
            public string text;
            public string next;
            public string nextIfCalmNegative;
            public string set;
            public string req;
            public string block;
            public float b, t, c;
            public bool timed;
            public float seconds = 8f;
            public string timeout;
        }

        sealed class NodeDef
        {
            public string id;
            public StorySpeaker speaker;
            public StoryTrigger trigger;
            public int ordinal;
            public string text;
            public string next;
            public string hint;
            public string enterSet;
            public float enterB, enterT, enterC;
            public bool isEnding;
            public string endingId;
            public string[] extraKeys;
            public ChoiceDef[] choices;
        }

        static NodeDef N(string id, StorySpeaker speaker, StoryTrigger trigger, int ordinal,
            string text, string next = null, ChoiceDef[] choices = null,
            string hint = null, string enterSet = null,
            float enterB = 0f, float enterT = 0f, float enterC = 0f,
            bool isEnding = false, string endingId = null, string[] extraKeys = null)
        {
            return new NodeDef
            {
                id = id, speaker = speaker, trigger = trigger, ordinal = ordinal,
                text = text, next = next, choices = choices, hint = hint,
                enterSet = enterSet, enterB = enterB, enterT = enterT, enterC = enterC,
                isEnding = isEnding, endingId = endingId, extraKeys = extraKeys,
            };
        }

        static ChoiceDef C(string text, string next, string set = null,
            float b = 0f, float t = 0f, float c = 0f,
            bool timed = false, float seconds = 8f, string timeout = null,
            string req = null, string block = null, string nextIfCalmNegative = null)
        {
            return new ChoiceDef
            {
                text = text, next = next, set = set, b = b, t = t, c = c,
                timed = timed, seconds = seconds, timeout = timeout,
                req = req, block = block, nextIfCalmNegative = nextIfCalmNegative,
            };
        }

        // ------------------------------------------------------ content table --
        // The authored graph (Docs/Story/SCRIPT.md). The English sentence IS the
        // localization key; Arabic pairs live in StoryStrings.cs. Keep the two
        // in sync - the report lists this file as the compile target.

        static readonly List<NodeDef> StoryContent = new List<NodeDef>
        {
            // ---------------- ACT I - LOCKED IN ----------------
            N("SN_Act1_01", StorySpeaker.Nour, StoryTrigger.ActStart, 0,
                "The gate slams behind me. Of course it does. Schools only open all the way for the last time.",
                choices: new[]
                {
                    C("Steady. Count the windows that still hold light: three.", "SN_Act1_01b", set: "mood_steady", c: 0.2f),
                    C("Walk like I'm only late for her class.", "SN_Act1_01b", set: "mood_role", b: 0.1f),
                    C("Say sorry to the dark. Quietly, so it knows my hands are empty.", "SN_Act1_01b", set: "mood_apology", t: 0.2f),
                }),

            N("SN_Act1_01b", StorySpeaker.Nour, StoryTrigger.None, 0,
                "Three wings, three fuse boxes, six pieces of her. The crew comes at dawn. All right. One room at a time."),

            N("SN_Act1_03", StorySpeaker.Mother, StoryTrigger.FuseRestored, 1,
                "First the wiring, then the reading. One room at a time - that is how you eat a school.",
                choices: new[]
                {
                    C("Stay in the light while it hums.", "SN_Act1_03a", c: 0.1f),
                    C("Step back into the corridor. Look at the dark properly.", "SN_Act1_03b", set: "looked_back", b: 0.2f),
                    C("Turn the flashlight off. Just to hear what the dark says.", "SN_Act1_03b",
                        b: 0.3f, c: -0.1f, timed: true, seconds: 8f, timeout: "SN_Act1_03a"),
                }),

            N("SN_Act1_03a", StorySpeaker.Nour, StoryTrigger.None, 0,
                "Warm light on white sheets. Her classroom smelled like this - chalk, jasmine, the inside of a book.",
                next: "SN_Act1_02"),

            N("SN_Act1_03b", StorySpeaker.Shadow, StoryTrigger.None, 0,
                "It stands at the end of the corridor the way a sentence stands when one word has been rubbed out. My beam touches it and it slides away. Politely.",
                next: "SN_Act1_02"),

            N("SN_Act1_02", StorySpeaker.Nour, StoryTrigger.None, 0,
                "On her desk: the empty memory box, and one card in her handwriting. \"Put me back together.\"",
                choices: new[]
                {
                    C("Slide the card into my pocket. Evidence first.", "SN_Act1_02b", set: "card_pocket", b: 0.1f),
                    C("Read it aloud. Like she's listening.", "SN_Act1_02b",
                        set: "card_read_aloud", t: 0.3f, c: 0.1f, timed: true, seconds: 7f, timeout: "SN_Act1_02t"),
                    C("Leave it on the desk. I'll take it when the box is full.", "SN_Act1_02b", set: "card_left"),
                }),

            N("SN_Act1_02t", StorySpeaker.Nour, StoryTrigger.None, 0,
                "The words stay behind my teeth. Maybe next time.",
                next: "SN_Act1_02b", enterSet: "card_pocket", enterT: -0.1f),

            N("SN_Act1_02b", StorySpeaker.Nour, StoryTrigger.None, 0,
                "Six fragments, three fuse boxes, one gate. I know the shape of this night. I have been studying for it my whole life."),

            N("SN_Act1_04", StorySpeaker.Nour, StoryTrigger.FragmentCollected, 1,
                "A pressed-flower bookmark, left in a book she never re-shelved. Her hand underneath: \"Return by Thursday.\" I am four years late.",
                choices: new[]
                {
                    C("Press it between the lesson plans. Where it belongs.", "SN_Act1_04b", set: "bookmark_boxed", c: 0.2f),
                    C("Keep it in my hand a while.", "SN_Act1_04b", set: "bookmark_kept", t: 0.1f),
                }),

            N("SN_Act1_04b", StorySpeaker.Mother, StoryTrigger.None, 0,
                "A library card is a key that never rusts."),

            N("SN_Act1_05", StorySpeaker.Nour, StoryTrigger.FragmentCollected, 2,
                "My exam paper. Red pen across the top: \"You see more than you think.\" She marked sixty papers that week. She remembered mine."),

            // ---------------- ACT II - THE DARK WINGS ----------------
            N("SN_Act2_01", StorySpeaker.Nour, StoryTrigger.FuseRestored, 2,
                "Fuse two hums awake. Through the lab window, on the lit floor: two shadows. Mine - and one more, edges smudged like an eraser was passed over it.",
                choices: new[]
                {
                    C("Call her name into the wing.", "SN_Act2_01_calm",
                        b: 0.4f, timed: true, seconds: 8f, timeout: "SN_Act2_01_pass",
                        nextIfCalmNegative: "SN_Act2_01_anger"),
                    C("Count the fuse wires. Order is a kind of courage.", "SN_Act2_01_pass", c: 0.3f),
                    C("Keep walking. Windows lie at this hour.", "SN_Act2_01_pass"),
                }),

            N("SN_Act2_01_calm", StorySpeaker.Shadow, StoryTrigger.None, 0,
                "It answers with a name. Almost mine. The vowels are worn like stairs. \"...N-oor. Nour.\" It has been practicing."),

            N("SN_Act2_01_anger", StorySpeaker.Shadow, StoryTrigger.None, 0,
                "The whisper folds in on itself, chalk under a door, static rising. Somewhere down the wing a poster goes blank.",
                enterSet: "angered_shadow"),

            N("SN_Act2_01_pass", StorySpeaker.Nour, StoryTrigger.None, 0,
                "The science wing is awake. One fuse box left, and the staff room that remembers her red pen."),

            N("SN_Act2_02", StorySpeaker.Nour, StoryTrigger.FragmentCollected, 3,
                "The garden bench. A jasmine pressed flat in her book of poems, and a photograph of this exact bench, her hand caught in the corner of the frame.",
                choices: new[]
                {
                    C("Sit for one minute. The Shadow slows near light; grief slows near benches.", "SN_Act2_02b",
                        set: "sat_bench", c: 0.4f, timed: true, seconds: 9f, timeout: "SN_Act2_02t"),
                    C("Take the photograph. Leave the jasmine where it grew.", "SN_Act2_02b", set: "left_jasmine", t: 0.3f),
                    C("Both in the box. This bench will not exist by Friday.", "SN_Act2_02b", set: "took_all", b: 0.1f),
                }),

            N("SN_Act2_02t", StorySpeaker.Nour, StoryTrigger.None, 0,
                "I take the photograph and keep standing. Standing is easier.",
                next: "SN_Act2_02b", enterC: 0.1f),

            N("SN_Act2_02b", StorySpeaker.Mother, StoryTrigger.None, 0,
                "Water the jasmine on Thursdays. Talk to it. Plants are students that never interrupt."),

            N("SN_Act2_03", StorySpeaker.Nour, StoryTrigger.FragmentCollected, 4,
                "Pinned inside the optics cabinet, in her lesson-plan voice: \"Light travels in straight lines - so do stories.\"",
                choices: new[]
                {
                    C("Read it aloud, exactly as written. Precision is a kind of prayer.", "SN_Act2_03b",
                        set: "optics_aloud", c: 0.3f, t: 0.1f, timed: true, seconds: 8f, timeout: "SN_Act2_03b"),
                    C("Copy it onto my hand. Ink is portable.", "SN_Act2_03b", set: "optics_copied", b: 0.1f),
                }),

            N("SN_Act2_03b", StorySpeaker.Mother, StoryTrigger.None, 0,
                "...and so do stories. Straight lines, even through walls. Especially through walls."),

            N("SN_Act2_04", StorySpeaker.Shadow, StoryTrigger.FragmentCollected, 5,
                "It stands in the assembly hall aisle with a roll call in its mouth. Names, most of them worn past reading. One it has kept too well: mine.",
                choices: new[]
                {
                    C("Answer it. \"That one's mine. You can't have it.\"", "SN_Act2_04_stand",
                        b: 0.4f, timed: true, seconds: 10f, timeout: "SN_Act2_04_pass",
                        nextIfCalmNegative: "SN_Act2_04_shaken"),
                    C("Recite her lesson line: \"Light travels in straight lines.\"", "SN_Act2_04_line",
                        set: "recited_lesson", c: 0.3f, t: 0.2f),
                    C("Hide behind the folded chairs. Let it pass.", "SN_Act2_04_pass",
                        set: "hid_passed", c: 0.1f, b: -0.2f),
                }),

            N("SN_Act2_04_stand", StorySpeaker.Shadow, StoryTrigger.None, 0,
                "\"...kept. Kept it. Kept-\" It folds my name carefully, like paper, and sets it down between us. Down the aisle: a door I never once used. The back stairs. I know the way now.",
                enterSet: "confronted_shadow,knows_backstairs", enterT: 0.3f),

            N("SN_Act2_04_shaken", StorySpeaker.Shadow, StoryTrigger.None, 0,
                "My voice hits it and comes back wrong, missing its middle. The whisper sharpens. A lamp dims somewhere without being touched.",
                enterSet: "angered_shadow", enterC: -0.3f),

            N("SN_Act2_04_line", StorySpeaker.Nour, StoryTrigger.None, 0,
                "I say it like a student who did the reading. Straight lines, even through walls. The Shadow thins to a smudge and is gone down the dark aisle."),

            N("SN_Act2_04_pass", StorySpeaker.Nour, StoryTrigger.None, 0,
                "Chalk static. Folded chairs. It passes close enough to read the absent look on its absent face. I hold still until the school forgets me again."),

            // ---------------- ACT III - DAWN ----------------
            N("SN_Act3_01", StorySpeaker.Nour, StoryTrigger.FuseRestored, 3,
                "Fuse three. The staff room lights up row by row, and there is her mug beside the sink, and a note she left a colleague: \"Back Thursday - the kettle forgives you.\"",
                choices: new[]
                {
                    C("Pour the cold tea out first. Then take the mug.", "SN_Act3_01b", set: "poured_tea", c: 0.2f, t: 0.2f),
                    C("Take the mug. The tea can be tomorrow's problem.", "SN_Act3_01b", set: "took_mug", b: 0.1f),
                    C("Leave everything. The room remembers her better with it here.", "SN_Act3_01b", set: "left_all", t: 0.2f),
                }),

            N("SN_Act3_01b", StorySpeaker.Mother, StoryTrigger.None, 0,
                "Keep the light on for the ones who come after."),

            N("SN_Act3_02", StorySpeaker.System, StoryTrigger.GatePowered, 0,
                "The gate crank is live. The corridor to the front doors is a lit runway. The Shadow knows it too."),

            N("SN_Act3_03", StorySpeaker.Shadow, StoryTrigger.FragmentCollected, 6,
                "Between me and the gate it has stopped pretending to be smoke. It is the school's whole forgetting, standing up: blank posters, erased names, a poem with its middle rubbed out.",
                choices: new[]
                {
                    C("Say her name back to it. \"Her name was-\"", "SN_Act3_03_forgave",
                        t: 0.3f, timed: true, seconds: 10f, timeout: "SN_Act3_03_pass",
                        nextIfCalmNegative: "SN_Act3_03_shaken"),
                    C("Erase it first. Swing the beam like an answer.", "SN_Act3_03_bold",
                        set: "erased_first", b: 0.4f, t: -0.2f),
                    C("Walk past. It cannot enter the lit corridor.", "SN_Act3_03_pass", set: "walked_past", c: 0.1f),
                }),

            N("SN_Act3_03_forgave", StorySpeaker.Shadow, StoryTrigger.None, 0,
                "\"...we kept everything. We kept-\" It holds its shape one moment longer, the way a class holds its breath before the bell. Then it lets the morning go.",
                enterSet: "confronted_shadow,forgave_shadow"),

            N("SN_Act3_03_shaken", StorySpeaker.Shadow, StoryTrigger.None, 0,
                "The name comes out with holes in it. The Shadow leans close - courteous, terrible - and takes the holes as its fee.",
                enterSet: "confronted_shadow", enterC: -0.2f),

            N("SN_Act3_03_bold", StorySpeaker.Nour, StoryTrigger.None, 0,
                "Light is a straight line and I am the line. It comes apart like chalk dust, and the dust is gone before it lands."),

            N("SN_Act3_03_pass", StorySpeaker.Nour, StoryTrigger.None, 0,
                "It stands aside the way furniture stands aside in a school you no longer attend. I keep to the light. I do not run. Not yet."),

            N("SN_Act3_04", StorySpeaker.Nour, StoryTrigger.DawnStarted, 0,
                "The windows are turning grey. The gate is one lit runway away, and my legs are a student's legs: they remember every corridor of this school.",
                choices: new[]
                {
                    C("Run the lit corridor. Straight lines only.", "SN_Act3_05",
                        set: "dawn_direct", c: 0.2f, timed: true, seconds: 9f, timeout: "SN_Act3_04t"),
                    C("The back stairs I never used. Faster, darker. Mine.", "SN_Act3_07",
                        set: "dawn_bold", b: 0.3f, req: "knows_backstairs"),
                }),

            N("SN_Act3_04t", StorySpeaker.Nour, StoryTrigger.None, 0,
                "Decide with your feet.",
                next: "SN_Act3_05", enterSet: "dawn_direct", enterC: -0.1f),

            N("SN_Act3_05", StorySpeaker.System, StoryTrigger.None, 0,
                "The corridor lights come on ahead of me, one by one, like roll call.",
                next: "SN_Act3_06"),

            N("SN_Act3_06", StorySpeaker.Mother, StoryTrigger.None, 0,
                "Faster, habibti. The bell does not wait for the slow - and neither does the morning.",
                next: "SN_Act3_08"),

            N("SN_Act3_07", StorySpeaker.Nour, StoryTrigger.None, 0,
                "The back stairs take the dark in jumps: three flights, a door with a push-bar, and the front hall roaring up to meet me.",
                next: "SN_Act3_08"),

            N("SN_Act3_08", StorySpeaker.Nour, StoryTrigger.None, 0,
                "The crank is cold and eager. Before I turn it: one promise, out loud, because the school is listening, and schools keep records.",
                choices: new[]
                {
                    C("I'll keep the light on. For the ones who come after.", "SN_Act3_09", set: "promised_light", t: 0.2f),
                    C("I'll come back with a key, not a fence to climb.", "SN_Act3_09", set: "promised_return", b: 0.1f),
                    C("Say nothing. Some things should stay in the building.", "SN_Act3_09", set: "promised_silence", req: "left_all"),
                }),

            N("SN_Act3_09", StorySpeaker.Mother, StoryTrigger.None, 0,
                "Go on, then. Knowledge is the only light that follows a person into the dark."),

            // ---------------- ENDINGS (D13) ----------------
            N("SN_End_FullRecovery", StorySpeaker.System, StoryTrigger.None, 0,
                "The gate closes behind her. The sky over the school turns from black to grey to a thin, clean gold.\n\nOn the pavement she opens the box: bookmark, paper, jasmine, note, programme, mug. Six small true things. She reads the lesson plans to the last page - the one line she has been climbing toward all night.\n\nAt a first-floor window the Shadow stands like a student kept after class, and watches the light walk away without her.\n\nKnowledge is the only light that follows a person into the dark. Tonight it followed Nour all the way out.",
                isEnding: true, endingId: "FullRecovery",
                extraKeys: new[]
                {
                    "FULL RECOVERY",
                    "Six pieces, every answer held. The box is full, and the morning lets her carry it.",
                }),

            N("SN_End_PartialMemory", StorySpeaker.System, StoryTrigger.None, 0,
                "The gate closes. The dawn is ordinary and enormous.\n\nThe box is not full. She can name the missing pieces - which is its own kind of remembering - and the pieces she has, she has read aloud, checked, and corrected, the way her mother taught her to study.\n\nSome of the voice is gone. Not all of it. Never all of it.\n\nShe carries the box home the way you carry a lit lamp through wind: close, and careful.",
                isEnding: true, endingId: "PartialMemory",
                extraKeys: new[]
                {
                    "PARTIAL MEMORY",
                    "She saved what she could hold. Some of the voice is gone; not all of it.",
                }),

            N("SN_End_HollowEscape", StorySpeaker.System, StoryTrigger.None, 0,
                "The gate closes behind her with a sound like a full stop.\n\nShe came for six pieces of a person and left with a handful. The box is light. Light boxes are easy to run with; that is the trouble.\n\nBehind her the building opens its arms to the crews and gives up what it kept, and she will spend years half-remembering that there was something she meant to bring.\n\nThe morning is very bright and there is almost nothing in her hands.",
                isEnding: true, endingId: "HollowEscape",
                extraKeys: new[]
                {
                    "HOLLOW ESCAPE",
                    "The gate, the morning, the road. The box barely weighs anything at all.",
                }),

            N("SN_End_Dawnless", StorySpeaker.System, StoryTrigger.None, 0,
                "The corridor ends at the gate and the gate does not open fast enough. The crews' vans turn the corner. The sky does its morning work.\n\nThe Shadow reaches her not as a hand but as a hush: names gone quiet, a night folded up like a finished exam.\n\nShe wakes on the pavement with the box still shut, and the school - patient, gentle, terrible - keeps her mother's memory the way a library keeps a book nobody borrowed.\n\nDawn comes. It simply comes without her.",
                isEnding: true, endingId: "Dawnless",
                extraKeys: new[]
                {
                    "DAWNLESS",
                    "The night ran out. The school keeps what she could not carry.",
                }),
        };

        // =========================================================== scene pass

        static string DecorateMainMenuScene(StoryConfig config)
        {
            OpenScene(MainMenuPath);

            GameObject canvasGo = GameObject.Find("UI_MainMenu");
            if (canvasGo == null) return "main menu: UI_MainMenu canvas missing (run Build Scenes first) - skipped";

            MainMenuController controller = canvasGo.GetComponent<MainMenuController>();
            if (controller == null) return "main menu: MainMenuController missing - skipped";

            // ---- save slot panel (D12) ------------------------------------
            SaveSlotUI slots = canvasGo.GetComponent<SaveSlotUI>();
            if (slots == null) slots = BuildSaveSlotsPanel(canvasGo.transform);

            // ---- SAVES button under QUIT ------------------------------------
            if (controller.savesButton == null)
            {
                Text savesLabel;
                Button saves = MakeStoryButton(canvasGo.transform, "SavesButton", "SAVES",
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -284f), new Vector2(430f, 46f), StoryButtonDark, 18, out savesLabel);
                LocalizeStory(savesLabel);
                controller.savesButton = saves;
            }
            controller.saveSlots = slots;

            // ---- settings page B (accessibility + volume rows) --------------
            ExtendSettingsPanel(controller.settings);

            SaveOpenScene();
            return "main menu: save slots + SAVES button + settings page B";
        }

        static string DecorateSchoolScene(StoryConfig config)
        {
            OpenScene(SchoolPath);

            // Story director + adaptive music live on the Systems object next to
            // GameManager / CheckpointManager / AudioManager.
            GameObject systems = GameObject.Find("Systems");
            if (systems == null) systems = new GameObject("Systems");

            StoryDirector director = systems.GetComponent<StoryDirector>();
            if (director == null) director = systems.AddComponent<StoryDirector>();
            director.config = config;

            // User decision (2026-11-27): no background music. AdaptiveMusic is
            // deliberately NOT added; the horror mix is ambience + SFX + the
            // memory-flash one-shot only. The class stays for a possible return.

            // Snapshot driving survives story-scene rebuilds too (idempotent;
            // D15 follow-up §4.5).
            AttachAudioSnapshot(systems.transform);

            // Dialogue box on the school canvas.
            GameObject canvasGo = GameObject.Find("UI_School");
            if (canvasGo == null) return "school: UI_School canvas missing (run Build Scenes first) - director added, UI skipped";

            DialogueUIController dialogue = canvasGo.GetComponent<DialogueUIController>();
            if (dialogue == null) dialogue = BuildDialogueUI(canvasGo.transform);
            director.dialogueUI = dialogue;

            // The pause-menu settings card gets page B too.
            ExtendSettingsPanel(canvasGo.GetComponent<SettingsPanelController>());

            SaveOpenScene();
            return "school: StoryDirector + AdaptiveMusic + dialogue UI + settings page B";
        }

        static string DecorateEndingScene()
        {
            OpenScene(EndingPath);

            GameObject canvasGo = GameObject.Find("UI_Ending");
            if (canvasGo == null) return "ending: UI_Ending canvas missing (run Build Scenes first) - skipped";

            RecapScreen recap = canvasGo.GetComponent<RecapScreen>();
            if (recap == null) return "ending: RecapScreen missing - skipped";

            Transform recapPanel = recap.panel != null ? recap.panel.transform : canvasGo.transform;

            // Ending name row, top-right of the recap title.
            if (recap.endingText == null)
            {
                Text endingText = MakeStoryText(recapPanel, "EndingName", "", 26, StoryWarm,
                    TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                    new Vector2(-72f, -46f), new Vector2(860f, 44f), FontStyle.Bold);
                endingText.gameObject.SetActive(false);
                recap.endingText = endingText;
            }

            // REPLAY button (back to main menu through the loading screen),
            // left of the existing RETURN button.
            if (recap.replayButton == null)
            {
                Text replayLabel;
                Button replay = MakeStoryButton(recapPanel, "ReplayButton", "REPLAY THE NIGHT",
                    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(-680f, 48f), new Vector2(340f, 62f), StoryButtonDark, 18, out replayLabel);
                LocalizeStory(replayLabel);
                recap.replayButton = replay;
            }

            SaveOpenScene();
            return "ending: recap ending row + REPLAY button";
        }

        static void OpenScene(string scenePath)
        {
            if (EditorSceneManager.GetActiveScene().path == scenePath) return;
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        static void SaveOpenScene()
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        // ================================================== save slots panel --

        static SaveSlotUI BuildSaveSlotsPanel(Transform canvas)
        {
            RectTransform dim = MakeStoryFullScreen(canvas, "SaveSlotsPanel",
                new Color(0f, 0f, 0f, 0.88f), true);

            SaveSlotUI slots = canvas.gameObject.AddComponent<SaveSlotUI>();
            slots.panel = dim.gameObject;

            RectTransform card = MakeStoryPanel(dim, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 1020f), StoryPanelDark, false)
                .rectTransform;

            slots.titleText = MakeStoryText(card, "Title", "SAVE SLOTS", 40, StoryWarm,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -46f), new Vector2(700f, 56f), FontStyle.Bold, true);

            slots.rows = new SaveSlotUI.SlotRow[SaveSystem.SlotCount];
            for (int i = 0; i < SaveSystem.SlotCount; i++)
            {
                slots.rows[i] = BuildSlotRow(card, i);
            }

            Text continueLabel;
            slots.continueButton = MakeStoryButton(card, "ContinueButton", "CONTINUE",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-190f, 48f), new Vector2(330f, 62f), StoryButtonWarm, 20, out continueLabel);
            LocalizeStory(continueLabel);

            Text backLabel;
            slots.backButton = MakeStoryButton(card, "BackButton", "BACK",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(210f, 48f), new Vector2(280f, 62f), StoryButtonDark, 20, out backLabel);
            LocalizeStory(backLabel);

            dim.gameObject.SetActive(false);
            return slots;
        }

        static SaveSlotUI.SlotRow BuildSlotRow(Transform card, int index)
        {
            var row = new SaveSlotUI.SlotRow { root = new GameObject("Slot" + index, typeof(RectTransform)) };
            RectTransform rect = row.root.GetComponent<RectTransform>();
            rect.SetParent(card, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -120f - index * 130f);
            rect.sizeDelta = new Vector2(820f, 120f);

            Image backing = row.root.AddComponent<Image>();
            backing.sprite = StoryBackground;
            backing.type = Image.Type.Sliced;
            backing.color = new Color(0.09f, 0.085f, 0.08f, 0.92f);
            backing.raycastTarget = false;

            row.slotText = MakeStoryText(rect, "SlotName", "SLOT " + (index + 1), 22, StoryPaper,
                TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -12f), new Vector2(220f, 34f), FontStyle.Bold);

            row.infoText = MakeStoryText(rect, "Info", "", 17, StoryDim,
                TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(250f, -10f), new Vector2(430f, 100f));

            Text loadLabel;
            row.loadButton = MakeStoryButton(rect, "Load", "LOAD",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-24f, -14f), new Vector2(130f, 44f), StoryButtonWarm, 19, out loadLabel);
            LocalizeStory(loadLabel);

            Text newLabel;
            row.newButton = MakeStoryButton(rect, "New", "NEW",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-24f, -66f), new Vector2(130f, 44f), StoryButtonDark, 19, out newLabel);
            LocalizeStory(newLabel);

            Text deleteLabel;
            row.deleteButton = MakeStoryButton(rect, "Delete", "DELETE",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-170f, -66f), new Vector2(130f, 44f), StoryButtonDark, 17, out deleteLabel);
            LocalizeStory(deleteLabel);

            // Confirm overlay (overwrite / delete), hidden until asked.
            GameObject confirm = new GameObject("Confirm", typeof(RectTransform));
            RectTransform confirmRect = confirm.GetComponent<RectTransform>();
            confirmRect.SetParent(rect, false);
            confirmRect.anchorMin = Vector2.zero;
            confirmRect.anchorMax = Vector2.one;
            confirmRect.offsetMin = Vector2.zero;
            confirmRect.offsetMax = Vector2.zero;
            Image confirmBacking = confirm.AddComponent<Image>();
            confirmBacking.sprite = StoryBackground;
            confirmBacking.type = Image.Type.Sliced;
            confirmBacking.color = new Color(0.02f, 0.02f, 0.03f, 0.97f);
            confirmBacking.raycastTarget = true;
            confirm.SetActive(false);

            row.confirmRoot = confirm;
            row.confirmText = MakeStoryText(confirmRect, "Prompt", "", 20, StoryPaper,
                TextAnchor.MiddleLeft, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(24f, 0f), new Vector2(500f, 70f));

            Text yesLabel;
            row.confirmYes = MakeStoryButton(confirmRect, "Yes", "CONFIRM",
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-24f, 0f), new Vector2(130f, 50f), StoryButtonWarm, 18, out yesLabel);
            LocalizeStory(yesLabel);

            Text noLabel;
            row.confirmNo = MakeStoryButton(confirmRect, "No", "CANCEL",
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-168f, 0f), new Vector2(130f, 50f), StoryButtonDark, 18, out noLabel);
            LocalizeStory(noLabel);

            return row;
        }

        // ================================================== dialogue box UI --

        static DialogueUIController BuildDialogueUI(Transform canvas)
        {
            // Bottom-anchored box; the world stays visible behind it (the story
            // does not black the screen out, it holds the player still).
            GameObject rootGo = new GameObject("StoryDialogue", typeof(RectTransform));
            RectTransform root = rootGo.GetComponent<RectTransform>();
            root.SetParent(canvas, false);
            root.anchorMin = new Vector2(0.5f, 0f);
            root.anchorMax = new Vector2(0.5f, 0f);
            root.pivot = new Vector2(0.5f, 0f);
            root.anchoredPosition = new Vector2(0f, 16f);
            root.sizeDelta = new Vector2(1240f, 540f);

            DialogueUIController dialogue = canvas.gameObject.AddComponent<DialogueUIController>();
            dialogue.root = rootGo;

            // Hit panel: catches click-to-advance over the box only.
            GameObject hitGo = new GameObject("HitPanel", typeof(RectTransform));
            RectTransform hitRect = hitGo.GetComponent<RectTransform>();
            hitRect.SetParent(root, false);
            hitRect.anchorMin = Vector2.zero;
            hitRect.anchorMax = Vector2.one;
            hitRect.offsetMin = Vector2.zero;
            hitRect.offsetMax = Vector2.zero;
            Image hit = hitGo.AddComponent<Image>();
            hit.sprite = StoryBackground;
            hit.type = Image.Type.Sliced;
            hit.color = new Color(0.045f, 0.045f, 0.06f, 0.94f);
            hit.raycastTarget = true;
            dialogue.hitPanel = hitGo;

            Outline hitOutline = hitGo.AddComponent<Outline>();
            hitOutline.effectColor = new Color(1f, 0.86f, 0.62f, 0.28f);
            hitOutline.effectDistance = new Vector2(2f, -2f);

            dialogue.speakerText = MakeStoryText(root, "Speaker", "", 24, StoryWarm,
                TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(28f, -14f), new Vector2(700f, 42f), FontStyle.Bold, true);

            // Timed-choice shrinking bar (hidden unless the node is timed).
            RectTransform timerTrack = MakeStoryRect(root, "TimerTrack",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(28f, -62f), new Vector2(1184f, 8f));
            Image timerBack = timerTrack.gameObject.AddComponent<Image>();
            timerBack.sprite = StoryUi;
            timerBack.color = new Color(0.06f, 0.06f, 0.07f, 0.9f);
            timerBack.raycastTarget = false;

            Image timerFill = MakeStoryImage(timerTrack, "TimerFill", Vector2.zero, Vector2.one,
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, StoryUi,
                new Color(0.95f, 0.75f, 0.35f, 1f), false);
            timerFill.type = Image.Type.Filled;
            timerFill.fillMethod = Image.FillMethod.Horizontal;
            timerFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            timerFill.fillAmount = 1f;
            dialogue.timerFill = timerFill;
            timerTrack.gameObject.SetActive(false);

            dialogue.bodyText = MakeStoryText(root, "Body", "", 24, StoryPaper,
                TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(28f, -78f), new Vector2(1184f, 180f));

            // 2-4 choice rows; 58 px high on a 64 px pitch (touch friendly),
            // keyboard/controller navigable top-down.
            dialogue.choiceButtons = new Button[4];
            dialogue.choiceTexts = new Text[4];
            for (int i = 0; i < 4; i++)
            {
                Text label;
                Button button = MakeStoryButton(root, "Choice" + (i + 1), "",
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, -276f - i * 64f), new Vector2(1184f, 58f), StoryButtonDark, 21, out label);
                label.alignment = TextAnchor.MiddleLeft;
                label.rectTransform.offsetMin = new Vector2(20f, 0f);
                label.rectTransform.offsetMax = new Vector2(-20f, 0f);
                // No LocalizedText here: the controller fills these labels at
                // runtime (per node, per language) and owns font/alignment.

                Navigation nav = button.navigation;
                nav.mode = Navigation.Mode.Automatic;
                button.navigation = nav;

                button.gameObject.SetActive(false);
                dialogue.choiceButtons[i] = button;
                dialogue.choiceTexts[i] = label;
            }

            // Skip (hold) + auto-advance toggles, top-right of the box.
            Text skipLabel;
            dialogue.skipButton = MakeStoryButton(root, "SkipButton", "HOLD TO SKIP",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-16f, -12f), new Vector2(250f, 42f), StoryButtonDark, 15, out skipLabel);
            LocalizeStory(skipLabel);

            Text autoLabel;
            dialogue.autoButton = MakeStoryButton(root, "AutoButton", "AUTO-ADVANCE  OFF",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-16f, -62f), new Vector2(250f, 38f), StoryButtonDark, 14, out autoLabel);
            dialogue.autoLabel = autoLabel;

            rootGo.SetActive(false);
            return dialogue;
        }

        // ============================================== settings page B rows --

        // Rows 11-21: accessibility (left column, 6 rows) and the five volume
        // sliders (right column, 5 rows), driven by the same prev/next row
        // pattern as page A. Runs on both the main-menu and pause-menu cards.
        static void ExtendSettingsPanel(SettingsPanelController controller)
        {
            if (controller == null) return;

            // Grow the wiring arrays (idempotent).
            if (controller.rowLabels == null || controller.rowLabels.Length < SettingsPanelController.TotalRows)
            {
                controller.rowLabels = Resize(controller.rowLabels, SettingsPanelController.TotalRows);
                controller.rowValues = Resize(controller.rowValues, SettingsPanelController.TotalRows);
                controller.prevButtons = Resize(controller.prevButtons, SettingsPanelController.TotalRows);
                controller.nextButtons = Resize(controller.nextButtons, SettingsPanelController.TotalRows);
            }

            if (controller.rowLabels[SettingsPanelController.TextSpeedRow] != null)
            {
                controller.BindStoryRows();
                controller.SetPage(0);
                return; // already built
            }

            Transform card = controller.title != null ? controller.title.transform.parent : null;
            if (card == null) return;

            string[] pageBKeys =
            {
                "Text speed", "Subtitle size", "Colourblind palette", "Auto-advance",
                "Skip hold time",
                "Master volume", "Music volume", "Ambience volume", "SFX volume", "UI volume",
            };

            for (int i = 0; i < pageBKeys.Length; i++)
            {
                int rowIndex = SettingsPanelController.TextSpeedRow + i;
                // The quality-tier row moved to page A (graphics refactor), so
                // page B is a balanced 5 + 5 grid of accessibility and volume.
                bool leftColumn = i < 5;
                int columnRow = leftColumn ? i : i - 5;
                float x = leftColumn ? 48f : 580f;
                float valueX = leftColumn ? 285f : 817f;
                float prevX = leftColumn ? 412f : 944f;
                float nextX = leftColumn ? 460f : 992f;
                float y = -120f - columnRow * 64f;

                controller.rowLabels[rowIndex] = MakeStoryText(card, "RowLabel" + rowIndex, pageBKeys[i], 22,
                    StoryPaper, TextAnchor.MiddleLeft,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(x, y), new Vector2(230f, 36f));
                LocalizeStory(controller.rowLabels[rowIndex]);

                controller.rowValues[rowIndex] = MakeStoryText(card, "RowValue" + rowIndex, "", 22,
                    StoryWarm, TextAnchor.MiddleCenter,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(valueX, y), new Vector2(120f, 36f));

                Text prevLabel;
                controller.prevButtons[rowIndex] = MakeStoryButton(card, "Prev" + rowIndex, "<",
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(prevX, y + 3f), new Vector2(44f, 40f), StoryButtonDark, 22, out prevLabel);

                Text nextLabel;
                controller.nextButtons[rowIndex] = MakeStoryButton(card, "Next" + rowIndex, ">",
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(nextX, y + 3f), new Vector2(44f, 40f), StoryButtonDark, 22, out nextLabel);
            }

            // Page toggle at the bottom centre of the card.
            if (controller.moreButton == null)
            {
                Text moreLabel;
                controller.moreButton = MakeStoryButton(card, "MoreButton", "MORE OPTIONS",
                    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 48f), new Vector2(330f, 58f), StoryButtonDark, 19, out moreLabel);
                controller.moreLabel = moreLabel;
            }

            controller.BindStoryRows();
            controller.SetPage(0);
        }

        static T[] Resize<T>(T[] source, int length)
        {
            var result = new T[length];
            if (source != null && source.Length > 0)
            {
                Array.Copy(source, result, Mathf.Min(source.Length, length));
            }
            return result;
        }

        // ====================================================== UI helpers --
        // Local copies of the SceneBuilderUI helper style (that class is not a
        // partial of SceneBuilder and its helpers are private), with the same
        // dark-card palette and builtin uGUI sprites.

        static readonly Color StoryWarm = new Color(1f, 0.86f, 0.62f);
        static readonly Color StoryPaper = new Color(0.90f, 0.88f, 0.80f);
        static readonly Color StoryDim = new Color(0.62f, 0.60f, 0.56f);
        static readonly Color StoryPanelDark = new Color(0.055f, 0.055f, 0.065f, 0.97f);
        static readonly Color StoryButtonDark = new Color(0.16f, 0.15f, 0.14f, 0.95f);
        static readonly Color StoryButtonWarm = new Color(0.36f, 0.25f, 0.13f, 0.98f);

        static Sprite _storyBackground;
        static Sprite StoryBackground
        {
            get
            {
                if (_storyBackground == null)
                {
                    _storyBackground = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
                }
                return _storyBackground;
            }
        }

        static Sprite _storyUi;
        static Sprite StoryUi
        {
            get
            {
                if (_storyUi == null)
                {
                    _storyUi = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                }
                return _storyUi;
            }
        }

        static Font StoryFont
        {
            // Bundled OFL set (Inter with LegacyRuntime fallback) so story UI
            // matches the rest of the interface; fonts wire identically here.
            get { return SceneBuilderUI.UIFont; }
        }

        static RectTransform MakeStoryRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        static RectTransform MakeStoryFullScreen(Transform parent, string name, Color color, bool raycast)
        {
            RectTransform rect = MakeStoryRect(parent, name, Vector2.zero, Vector2.one,
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return rect;
        }

        static Image MakeStoryImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 position, Vector2 size, Sprite sprite, Color color, bool raycast)
        {
            RectTransform rect = MakeStoryRect(parent, name, anchorMin, anchorMax, pivot, position, size);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = raycast;
            if (sprite == StoryBackground) image.type = Image.Type.Sliced;
            return image;
        }

        static Image MakeStoryPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 position, Vector2 size, Color color, bool raycast)
        {
            return MakeStoryImage(parent, name, anchorMin, anchorMax, pivot, position, size,
                StoryBackground, color, raycast);
        }

        static Text MakeStoryText(Transform parent, string name, string content, int fontSize, Color color,
            TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position,
            Vector2 size, FontStyle style = FontStyle.Normal, bool shadow = false)
        {
            RectTransform rect = MakeStoryRect(parent, name, anchorMin, anchorMax, pivot, position, size);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = StoryFont;
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.lineSpacing = 1.08f;
            text.supportRichText = true;
            text.raycastTarget = false;

            if (shadow)
            {
                Shadow effect = rect.gameObject.AddComponent<Shadow>();
                effect.effectColor = new Color(0f, 0f, 0f, 0.85f);
                effect.effectDistance = new Vector2(2f, -2f);
            }

            return text;
        }

        static Button MakeStoryButton(Transform parent, string name, string label, Vector2 anchorMin,
            Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size, Color background,
            int fontSize, out Text labelText)
        {
            RectTransform rect = MakeStoryRect(parent, name, anchorMin, anchorMax, pivot, position, size);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = StoryBackground;
            image.type = Image.Type.Sliced;
            image.color = background;
            image.raycastTarget = true;

            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.12f, 1.05f, 1f);
            colors.pressedColor = new Color(0.78f, 0.76f, 0.72f, 1f);
            colors.selectedColor = new Color(1.05f, 1.02f, 0.98f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            labelText = MakeStoryText(rect, "Label", label, fontSize, StoryPaper, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            labelText.raycastTarget = false;

            return button;
        }

        // Story strings live in their own EN/AR table (StoryStrings), so these
        // rows attach through the same LocalizedText display path used across
        // the game's chrome.
        static void LocalizeStory(Text text)
        {
            if (text == null) return;
            LocalizedText.Attach(text);
        }
    }
}
