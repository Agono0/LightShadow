using System;
using UnityEngine;

namespace LightShadow.Story
{
    public enum StorySpeaker { Nour = 0, Mother = 1, Shadow = 2, System = 3 }

    public enum StoryAct { Act1 = 0, Act2 = 1, Act3 = 2, Any = 3 }

    // What gameplay occurrence can put this node on screen. The ordinal picks
    // which fuse box / fragment / act the trigger belongs to (1-based, matching
    // GameEvents payloads). TriggerKind.None nodes are only reached by links.
    public enum StoryTrigger
    {
        None = 0,
        ActStart = 1,
        FuseRestored = 2,
        FragmentCollected = 3,
        GatePowered = 4,
        PlayerCaught = 5,
        CheckpointReached = 6,
        DawnStarted = 7,
        EscapeStarted = 8,
    }

    // One node of the branching story graph (decision D4): a single dialogue
    // beat shown by the DialogueUIController. Every asset is built by
    // Tools/Editor/SceneBuilderStory.cs from Docs/Story/SCRIPT.md - no story
    // text is ever written inside runtime code.
    //
    // The English text IS the localization key (project convention), so the
    // display path is: StoryStrings.Get(node.text) -> Localization.Present.
    [CreateAssetMenu(fileName = "SN_New", menuName = "LightShadow/Story Node")]
    public class StoryNode : ScriptableObject
    {
        [Serializable]
        public class StoryChoice
        {
            [Tooltip("English sentence = localization key (StoryStrings carries the Arabic).")]
            public string text;

            [Tooltip("Flags switched on when this choice is taken.")]
            public string[] setFlags = Array.Empty<string>();

            [Tooltip("The choice is only offered while every flag here is set.")]
            public string[] requiredFlags = Array.Empty<string>();

            [Tooltip("The choice is hidden while any flag here is set.")]
            public string[] blockedFlags = Array.Empty<string>();

            [Tooltip("Stat deltas, each clamped to -1..+1.")]
            public float boldness;
            public float trust;
            public float calm;

            [Tooltip("Node shown next, or empty when the conversation ends here.")]
            public string nextNodeId;

            [Tooltip("Calm gate: route here instead when the calm stat is negative (the risky line lands differently on a shaken Nour).")]
            public string nextIfCalmNegative;

            [Tooltip("Timed choice: a shrinking bar counts down and takes timeoutNextId.")]
            public bool isTimed;
            public float timeSeconds = 8f;

            [Tooltip("Where a timed choice goes when the bar empties (with timeout calm dent).")]
            public string timeoutNextId;
        }

        [Header("Identity")]
        public string nodeId = "SN_Act1_01";
        public StoryAct act = StoryAct.Act1;

        [Header("Trigger")]
        public StoryTrigger trigger = StoryTrigger.None;
        [Tooltip("1-based ordinal: which fuse box (1-3), which fragment (1-6), which act (0 = first).")]
        public int triggerOrdinal;

        [Tooltip("Node only plays while every flag here is set.")]
        public string[] requiredFlags = Array.Empty<string>();

        [Tooltip("Node never plays while any flag here is set.")]
        public string[] blockedFlags = Array.Empty<string>();

        [Header("Content")]
        public StorySpeaker speaker = StorySpeaker.Nour;
        [TextArea(3, 8)]
        public string text = "";
        [Tooltip("Optional override/extra keys (e.g. an Arabic-only variant line). English text stays the primary key.")]
        public string[] localizationKeys = Array.Empty<string>();

        [Header("Flow")]
        public StoryChoice[] choices = Array.Empty<StoryChoice>();
        [Tooltip("Continuation when the node has no choices (memory flashes, resolves).")]
        public string nextNodeId = "";

        [Header("Effects applied on enter (used by timeout/route nodes)")]
        public string[] enterSetFlags = Array.Empty<string>();
        public float enterBoldness;
        public float enterTrust;
        public float enterCalm;

        [Header("Ending node")]
        public bool isEndingNode;
        [Tooltip("Ending id: FullRecovery / PartialMemory / HollowEscape / Dawnless (D13).")]
        public string endingId = "";

        [Tooltip("Free-form hint consumed by the director: e.g. \"Autosave\" after a fuse beat. Never gameplay logic.")]
        public string onEnterEventHint = "";
    }
}
