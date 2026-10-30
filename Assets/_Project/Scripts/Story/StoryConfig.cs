using UnityEngine;

namespace LightShadow.Story
{
    // The one tuning asset for the story system. Decision D13 says the ending
    // formula lives in a story asset, not in code: every threshold below is a
    // serialized field the Lead can re-tune without touching C#.
    [CreateAssetMenu(fileName = "SO_StoryConfig", menuName = "LightShadow/Story Config")]
    public class StoryConfig : ScriptableObject
    {
        [Header("Node sources")]
        [Tooltip("Every story node asset built by SceneBuilderStory (wired by the builder).")]
        public StoryNode[] nodes = new StoryNode[0];

        [Tooltip("Node played when the school scene starts (trigger ActStart).")]
        public string actStartNodeId = "SN_Act1_01";

        [Header("Endings (D13) — checked in this order")]
        public string endingDawnlessId = "Dawnless";
        public string endingFullRecoveryId = "FullRecovery";
        public string endingPartialMemoryId = "PartialMemory";
        public string endingHollowEscapeId = "HollowEscape";

        [Tooltip("Flag that proves Nour faced the Shadow at the roll call / main hall.")]
        public string confrontedFlag = "confronted_shadow";

        [Header("Ending thresholds")]
        [Tooltip("Accuracy needed for Full Recovery at 6/6 fragments.")]
        public float fullRecoveryAccuracy = 0.75f;
        [Tooltip("Minimum fragments for Partial Memory.")]
        public int partialMemoryFragments = 4;
        [Tooltip("Accuracy needed for Partial Memory.")]
        public float partialMemoryAccuracy = 0.5f;

        [Header("Dawnless rule")]
        [Tooltip("Catches during the dawn chase (after DawnStarted) that end the night.")]
        public int dawnlessCatchLimit = 2;

        [Header("Pacing")]
        [Tooltip("Seconds to wait after a gameplay event before the node appears (lets the memory flash / checkpoint toast read first).")]
        public float triggerDelaySeconds = 1.2f;

        [Tooltip("Pause taken between chained nodes.")]
        public float chainGapSeconds = 0.35f;

        [Tooltip("Calm dented when a timed choice runs out.")]
        public float timeoutCalmPenalty = 0.1f;
    }
}
