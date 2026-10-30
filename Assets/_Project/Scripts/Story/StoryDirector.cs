using System;
using System.Collections;
using System.Collections.Generic;
using LightShadow.Core;
using LightShadow.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightShadow.Story
{
    // Runs the branching story graph (decision D4). Gameplay talks to this
    // class only through GameEvents; the director talks back through
    // StoryEvents and the DialogueUIController. All story content lives in
    // StoryNode assets (built by Tools/Editor/SceneBuilderStory.cs) plus the
    // SO_StoryConfig thresholds - no story logic is written here beyond
    // scheduling, flag/stats bookkeeping and the D13 ending formula.
    //
    // Persistence: the whole story state serializes to one JSON blob which is
    // copied verbatim into SaveSystem.SaveData.storyJson (D12). The engine
    // never interprets it; this class fills it on SaveCaptured and restores it
    // on SaveApplied.
    public class StoryDirector : MonoBehaviour
    {
        public static StoryDirector Instance { get; private set; }

        [Header("Wiring (set by SceneBuilderStory)")]
        public StoryConfig config;
        public DialogueUIController dialogueUI;

        [Serializable]
        class StoryState
        {
            public int version = 1;
            public string[] played = Array.Empty<string>();
            public string[] flags = Array.Empty<string>();
            public float boldness;
            public float trust;
            public float calm;
            public int asked;
            public int correct;
            public int dawnCatches;
            public bool dawnActive;
            public string endingId = "";
        }

        readonly HashSet<string> played = new HashSet<string>();
        readonly HashSet<string> flags = new HashSet<string>();
        readonly Queue<(StoryTrigger trigger, int ordinal)> pending = new Queue<(StoryTrigger, int)>();
        readonly Dictionary<string, StoryNode> nodesById = new Dictionary<string, StoryNode>();

        StoryState state = new StoryState();
        StoryNode current;
        float lastConversationEndedAt = -99f;
        bool chainScheduled;   // a PlayAfterGap continuation is in flight
        bool endingDecided;
        string decidedEndingId = "";

        // ------------------------------------------------------------- access --

        public string CurrentEndingId => decidedEndingId;
        public bool HasFlag(string flag) => flags.Contains(flag);
        public float Boldness => state.boldness;
        public float Trust => state.trust;
        public float Calm => state.calm;
        public float Accuracy => state.asked > 0 ? (float)state.correct / state.asked : 0f;

        // Fallback spawner so pressing Play in any scene never hard-crashes.
        // Real wiring (nodes + UI) is added by SceneBuilderStory; without it the
        // director stays a quiet no-op and the game plays exactly as before.
        public static void EnsureCreated()
        {
            if (Instance != null) return;
            new GameObject("StoryDirector").AddComponent<StoryDirector>();
        }

        // Fresh run: clears every flag/stat/played node (New Game on a slot).
        public static void ResetForNewRun()
        {
            if (Instance == null) return;
            Instance.StopAllCoroutines();      // a stale PlayAfterGap must not fire
            Instance.state = new StoryState();
            Instance.played.Clear();
            Instance.flags.Clear();
            Instance.pending.Clear();
            Instance.chainScheduled = false;
            Instance.endingDecided = false;
            Instance.decidedEndingId = "";
            Instance.current = null;
            PlayerController.ExternalInputLocked = false;
            if (Instance.dialogueUI != null) Instance.dialogueUI.Hide();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            AccessibilityStore.Load();
            IndexNodes();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void IndexNodes()
        {
            nodesById.Clear();
            if (config == null) return;
            for (int i = 0; i < config.nodes.Length; i++)
            {
                StoryNode node = config.nodes[i];
                if (node == null || string.IsNullOrEmpty(node.nodeId)) continue;
                nodesById[node.nodeId] = node;
            }
        }

        // ------------------------------------------------- GameEvents plumbing --

        void OnEnable()
        {
            GameEvents.FuseRestored += OnFuseRestored;
            GameEvents.FragmentCollected += OnFragmentCollected;
            GameEvents.GatePowered += OnGatePowered;
            GameEvents.PlayerCaught += OnPlayerCaught;
            GameEvents.DawnStarted += OnDawnStarted;
            GameEvents.EscapeStarted += OnEscapeStarted;
            GameEvents.QuestionAnswered += OnQuestionAnswered;
            SaveSystem.SaveCaptured += OnSaveCaptured;
            SaveSystem.SaveApplied += OnSaveApplied;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            // Never leak handlers on the static buses.
            GameEvents.FuseRestored -= OnFuseRestored;
            GameEvents.FragmentCollected -= OnFragmentCollected;
            GameEvents.GatePowered -= OnGatePowered;
            GameEvents.PlayerCaught -= OnPlayerCaught;
            GameEvents.DawnStarted -= OnDawnStarted;
            GameEvents.EscapeStarted -= OnEscapeStarted;
            GameEvents.QuestionAnswered -= OnQuestionAnswered;
            SaveSystem.SaveCaptured -= OnSaveCaptured;
            SaveSystem.SaveApplied -= OnSaveApplied;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // The act-start beat plays once per school visit on a fresh run.
            if (scene.name == SceneNames.School && config != null
                && !string.IsNullOrEmpty(config.actStartNodeId)
                && !played.Contains(config.actStartNodeId))
            {
                QueueTrigger(StoryTrigger.ActStart, 0);
            }

            if (scene.name != SceneNames.School)
            {
                // Leaving the school (ending/menu) always releases the player.
                PlayerController.ExternalInputLocked = false;
            }
        }

        void OnFuseRestored(int restored, int total) => QueueTrigger(StoryTrigger.FuseRestored, restored);
        void OnFragmentCollected(int collected, int total) => QueueTrigger(StoryTrigger.FragmentCollected, collected);
        void OnGatePowered() => QueueTrigger(StoryTrigger.GatePowered, 0);

        void OnPlayerCaught()
        {
            // Dawnless rule (D13): two catches after dawn ends the night. Not a
            // game-over - GameManager.Escape() (public API) runs the standard
            // bloodless fade into the ending scene, which shows the Dawnless text.
            if (state.dawnActive && !endingDecided)
            {
                state.dawnCatches++;
                if (config != null && state.dawnCatches >= config.dawnlessCatchLimit)
                {
                    DecideEnding(config.endingDawnlessId);
                    GameManager game = GameManager.Instance;
                    if (game != null) game.Escape();
                    return;
                }
            }

            QueueTrigger(StoryTrigger.PlayerCaught, 0);
        }

        void OnDawnStarted()
        {
            state.dawnActive = true;
            QueueTrigger(StoryTrigger.DawnStarted, 0);
        }

        void OnEscapeStarted()
        {
            if (!endingDecided) DecideEnding(PickEndingId());
            // No dialogue on escape: the fade and the ending scene own the moment.
        }

        void OnQuestionAnswered(bool correct)
        {
            state.asked++;
            if (correct) state.correct++;
        }

        void QueueTrigger(StoryTrigger trigger, int ordinal)
        {
            if (trigger == StoryTrigger.None) return;
            pending.Enqueue((trigger, ordinal));
        }

        // ------------------------------------------------------------ update --

        void Update()
        {
            if (pending.Count == 0) return;
            if (chainScheduled) return;                        // a chain continuation is already queued
            if (current != null) return;                       // a conversation is live
            if (QuizOpen()) return;                            // quiz keeps priority (brief #3)
            if (Time.unscaledTime - lastConversationEndedAt < (config != null ? config.triggerDelaySeconds : 1.2f)) return;

            // Drain until a playable node is found (stale ordinals just drop).
            while (pending.Count > 0)
            {
                (StoryTrigger trigger, int ordinal) = pending.Dequeue();
                StoryNode node = FindTriggeredNode(trigger, ordinal);
                if (node != null)
                {
                    PlayNode(node);
                    return;
                }
            }
        }

        bool QuizOpen()
        {
            GameManager game = GameManager.Instance;
            return game != null && game.quizUI != null && game.quizUI.IsOpen;
        }

        StoryNode FindTriggeredNode(StoryTrigger trigger, int ordinal)
        {
            if (config == null) return null;
            for (int i = 0; i < config.nodes.Length; i++)
            {
                StoryNode node = config.nodes[i];
                if (node == null || node.trigger != trigger) continue;
                if (node.triggerOrdinal != ordinal) continue;
                if (played.Contains(node.nodeId)) continue;
                if (!FlagsAllow(node.requiredFlags, node.blockedFlags)) continue;
                return node;
            }
            return null;
        }

        bool FlagsAllow(string[] required, string[] blocked)
        {
            if (required != null)
            {
                for (int i = 0; i < required.Length; i++)
                {
                    if (!flags.Contains(required[i])) return false;
                }
            }
            if (blocked != null)
            {
                for (int i = 0; i < blocked.Length; i++)
                {
                    if (flags.Contains(blocked[i])) return false;
                }
            }
            return true;
        }

        // ---------------------------------------------------------- playback --

        void PlayNode(StoryNode node)
        {
            current = node;
            played.Add(node.nodeId);

            if (!string.IsNullOrEmpty(node.onEnterEventHint)
                && node.onEnterEventHint.IndexOf("Autosave", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                SaveSystem.Autosave();
            }

            PlayerController.ExternalInputLocked = true;
            StoryEvents.RaiseNodeEntered(node);

            // Node-enter effects (timeout/route nodes carry their own small
            // consequences so a timed miss still sets its flag and stat dent).
            if (node.enterSetFlags != null)
            {
                for (int i = 0; i < node.enterSetFlags.Length; i++)
                {
                    if (!string.IsNullOrEmpty(node.enterSetFlags[i])) flags.Add(node.enterSetFlags[i]);
                }
            }
            if (node.enterBoldness != 0f) state.boldness = ClampStat(state.boldness + node.enterBoldness);
            if (node.enterTrust != 0f) state.trust = ClampStat(state.trust + node.enterTrust);
            if (node.enterCalm != 0f) state.calm = ClampStat(state.calm + node.enterCalm);

            // Narration uses the logical (unshaped) string; StoryTeller picks the
            // right voice for Arabic by itself and respects the narrator toggle.
            StoryTeller.Speak(StoryStrings.Get(node.text), true);

            if (dialogueUI == null) dialogueUI = GetComponentInChildren<DialogueUIController>();
            if (dialogueUI == null)
            {
                // Headless fail-safe: walk the graph silently so nothing locks.
                AdvanceFrom(node, -1, false);
                return;
            }

            // Offered choices only (required/blocked flags evaluated here; the
            // UI stays dumb). A timed node highlights its timed choice index.
            List<int> offered = new List<int>(node.choices.Length);
            int timedIndex = -1;
            for (int i = 0; i < node.choices.Length; i++)
            {
                StoryNode.StoryChoice choice = node.choices[i];
                if (!FlagsAllow(choice.requiredFlags, choice.blockedFlags)) continue;
                offered.Add(i);
                if (choice.isTimed) timedIndex = offered.Count - 1;
            }

            dialogueUI.ShowNode(node, offered.ToArray(), timedIndex, OnUiChoice, OnUiAdvance);
        }

        // UI callbacks: index refers to the offered list; timedOut routes the
        // choice through its timeout branch (with the calm penalty).
        void OnUiChoice(int offeredIndex, bool timedOut)
        {
            if (current == null) return;
            AdvanceFrom(current, offeredIndex, timedOut);
        }

        void OnUiAdvance()
        {
            if (current == null) return;
            AdvanceFrom(current, -1, false);
        }

        // The one place choices resolve: effects, routing, chain scheduling.
        void AdvanceFrom(StoryNode node, int offeredIndex, bool timedOut)
        {
            StoryNode next = null;

            if (offeredIndex >= 0 && offeredIndex < node.choices.Length)
            {
                StoryNode.StoryChoice choice = node.choices[offeredIndex];

                if (timedOut)
                {
                    StoryEvents.RaiseChoiceTimedOut(node, offeredIndex);
                    if (config != null) state.calm = ClampStat(state.calm - config.timeoutCalmPenalty);
                    next = Resolve(choice.timeoutNextId);
                }
                else
                {
                    // Effects first (flag-gated routing can read the new flags),
                    // then the events, then routing.
                    if (choice.setFlags != null)
                    {
                        for (int i = 0; i < choice.setFlags.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(choice.setFlags[i])) flags.Add(choice.setFlags[i]);
                        }
                    }
                    state.boldness = ClampStat(state.boldness + choice.boldness);
                    state.trust = ClampStat(state.trust + choice.trust);
                    state.calm = ClampStat(state.calm + choice.calm);

                    StoryEvents.RaiseChoiceMade(node, offeredIndex);

                    // Calm gate: the same risky line lands differently on a
                    // shaken Nour (negative calm routes to the harsh branch).
                    next = !string.IsNullOrEmpty(choice.nextIfCalmNegative) && state.calm < 0f
                        ? Resolve(choice.nextIfCalmNegative)
                        : Resolve(choice.nextNodeId);
                }
            }
            else
            {
                StoryEvents.RaiseNodeLeft(node);
                next = Resolve(node.nextNodeId);
            }

            if (next == null)
            {
                EndConversation(node);
                return;
            }

            StartCoroutine(PlayAfterGap(next));
        }

        StoryNode Resolve(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return null;
            nodesById.TryGetValue(nodeId, out StoryNode node);
            if (node != null && played.Contains(node.nodeId) && node.trigger == StoryTrigger.None)
            {
                // Loop guard: never replay a linked node in the same conversation.
                return null;
            }
            return node;
        }

        IEnumerator PlayAfterGap(StoryNode next)
        {
            float gap = config != null ? config.chainGapSeconds : 0.35f;
            current = null;
            chainScheduled = true;   // triggers must not interleave mid-chain
            yield return new WaitForSecondsRealtime(gap);
            chainScheduled = false;
            PlayNode(next);
        }

        void EndConversation(StoryNode node)
        {
            current = null;
            lastConversationEndedAt = Time.unscaledTime;
            PlayerController.ExternalInputLocked = false;
            StoryEvents.RaiseConversationEnded();

            if (dialogueUI != null) dialogueUI.Hide();

            // Fuse/fragment beats close with an autosave so a conversation never
            // half-survives a quit (D12).
            if (node != null && node.trigger == StoryTrigger.FuseRestored)
            {
                SaveSystem.Autosave();
            }
        }

        static float ClampStat(float value) => Mathf.Clamp(value, -1f, 1f);

        // ------------------------------------------------------------ endings --

        // D13 formula. Constants live in SO_StoryConfig; the order of checks is
        // Dawnless-first only when explicitly triggered (see OnPlayerCaught).
        string PickEndingId()
        {
            if (config == null) return "PartialMemory";

            int fragments = GameManager.Instance != null ? GameManager.Instance.FragmentsCollected : 0;
            float accuracy = Accuracy;
            bool confronted = flags.Contains(config.confrontedFlag);

            if (fragments >= GameManager.FragmentTarget
                && accuracy >= config.fullRecoveryAccuracy
                && confronted)
            {
                return config.endingFullRecoveryId;
            }

            if (fragments >= config.partialMemoryFragments
                && accuracy >= config.partialMemoryAccuracy)
            {
                return config.endingPartialMemoryId;
            }

            return config.endingHollowEscapeId;
        }

        void DecideEnding(string endingId)
        {
            if (endingDecided) return;
            endingDecided = true;
            decidedEndingId = endingId;
            state.endingId = endingId;
            StoryEvents.RaiseEndingDecided(endingId);
        }

        // Ending scene API. Decides on demand (editor: Play straight into
        // 03_Ending) and returns the id; also answers presentation lookups from
        // the ending nodes. Ending node text conventions:
        //   text              = the ending body
        //   localizationKeys[0] = title key (e.g. "FULL RECOVERY")
        //   localizationKeys[1] = narrator line
        public string EnsureEndingDecided()
        {
            if (!endingDecided) DecideEnding(PickEndingId());
            return decidedEndingId;
        }

        public StoryNode GetEndingNode(string endingId)
        {
            if (config == null || string.IsNullOrEmpty(endingId)) return null;
            for (int i = 0; i < config.nodes.Length; i++)
            {
                StoryNode node = config.nodes[i];
                if (node != null && node.isEndingNode && node.endingId == endingId) return node;
            }
            return null;
        }

        // ------------------------------------------------- save/load (D12) ----

        // The blob written into SaveSystem.SaveData.storyJson. Shape:
        // {"version":1,"played":["SN_.."],"flags":[".."],"boldness":0.0,
        //  "trust":0.0,"calm":0.0,"asked":0,"correct":0,"dawnCatches":0,
        //  "dawnActive":false,"endingId":""}
        public string SerializeState()
        {
            state.played = new string[played.Count];
            played.CopyTo(state.played);
            state.flags = new string[flags.Count];
            flags.CopyTo(state.flags);
            return JsonUtility.ToJson(state);
        }

        public void DeserializeState(string json)
        {
            state = string.IsNullOrEmpty(json) ? new StoryState() : JsonUtility.FromJson<StoryState>(json);
            if (state == null) state = new StoryState();

            played.Clear();
            if (state.played != null)
            {
                for (int i = 0; i < state.played.Length; i++) played.Add(state.played[i]);
            }
            flags.Clear();
            if (state.flags != null)
            {
                for (int i = 0; i < state.flags.Length; i++) flags.Add(state.flags[i]);
            }

            // A restored run may already be past its ending (edge case: save taken
            // during the escape fade) - keep the id but allow re-decision only if
            // empty.
            endingDecided = !string.IsNullOrEmpty(state.endingId);
            decidedEndingId = state.endingId ?? "";
        }

        void OnSaveCaptured(SaveSystem.SaveData data)
        {
            if (data != null) data.storyJson = SerializeState();
        }

        void OnSaveApplied(SaveSystem.SaveData data)
        {
            if (data == null) return;
            DeserializeState(data.storyJson);
            // Nothing on screen while a save is applied - the next trigger will
            // resume the graph from wherever the flags/played set stands.
        }
    }
}
