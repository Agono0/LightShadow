using System.Collections.Generic;
using System.Reflection;
using LightShadow.Story;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    // Per-conversation linked-node loop guard (audit 2.2): a linked
    // (trigger None) node already visited in the CURRENT conversation cannot
    // replay (Resolve returns null, ending the chain), while a triggered node
    // is unaffected, and EndConversation clears the guard so a shared linked
    // node can play again in a later conversation.
    //
    // Seams: config is public; IndexNodes/Resolve/EndConversation and the
    // conversationVisited set are private and reached by reflection. PlayNode
    // is deliberately NOT invoked (it touches OverlayStack.Acquire, voiceover
    // and UI), so the guard set is primed directly. EndConversation is
    // headless-safe here: OverlayStack.Release tolerates a null token,
    // VoiceoverPlayer.Stop null-guards AudioManager.Instance, and autosave
    // only fires for FuseRestored nodes. The PlayAfterGap coroutine chain is
    // runtime-only and not covered.
    public class StoryLoopGuardTests
    {
        static readonly MethodInfo IndexNodesMethod =
            typeof(StoryDirector).GetMethod("IndexNodes", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo ResolveMethod =
            typeof(StoryDirector).GetMethod("Resolve", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo EndConversationMethod =
            typeof(StoryDirector).GetMethod("EndConversation", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo ConversationVisitedField =
            typeof(StoryDirector).GetField("conversationVisited", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        GameObject host;
        StoryDirector director;
        StoryConfig config;
        StoryNode linkedA;
        StoryNode linkedB;
        StoryNode triggeredC;

        [SetUp]
        public void SetUp()
        {
            linkedA = ScriptableObject.CreateInstance<StoryNode>();
            linkedA.nodeId = "SN_GuardTest_A";
            linkedA.trigger = StoryTrigger.None;
            linkedB = ScriptableObject.CreateInstance<StoryNode>();
            linkedB.nodeId = "SN_GuardTest_B";
            linkedB.trigger = StoryTrigger.None;
            triggeredC = ScriptableObject.CreateInstance<StoryNode>();
            triggeredC.nodeId = "SN_GuardTest_C";
            triggeredC.trigger = StoryTrigger.ActStart;

            config = ScriptableObject.CreateInstance<StoryConfig>();
            config.nodes = new[] { linkedA, linkedB, triggeredC };

            // Inactive host: Awake (Instance management, AccessibilityStore)
            // never runs; IndexNodes is invoked directly after wiring config.
            host = new GameObject("StoryLoopGuardTest");
            host.SetActive(false);
            director = host.AddComponent<StoryDirector>();
            director.config = config;
            IndexNodesMethod.Invoke(director, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (host != null) Object.DestroyImmediate(host);
            if (config != null) Object.DestroyImmediate(config);
            if (linkedA != null) Object.DestroyImmediate(linkedA);
            if (linkedB != null) Object.DestroyImmediate(linkedB);
            if (triggeredC != null) Object.DestroyImmediate(triggeredC);
        }

        HashSet<string> Visited => (HashSet<string>)ConversationVisitedField.GetValue(director);

        StoryNode Resolve(string nodeId)
        {
            return (StoryNode)ResolveMethod.Invoke(director, new object[] { nodeId });
        }

        [Test]
        public void Resolve_LinkedNodeVisitedInSameConversation_ReturnsNull()
        {
            Visited.Add("SN_GuardTest_A");
            Assert.IsNull(Resolve("SN_GuardTest_A"), "a linked node already visited this conversation must not replay");
            Assert.AreSame(linkedB, Resolve("SN_GuardTest_B"), "an unvisited linked node still resolves");
        }

        [Test]
        public void Resolve_TriggeredNodeVisitedInSameConversation_StillResolves()
        {
            Visited.Add("SN_GuardTest_C");
            Assert.AreSame(triggeredC, Resolve("SN_GuardTest_C"),
                "the guard is scoped to linked (trigger None) nodes; triggered nodes are gated by the played set instead");
        }

        [Test]
        public void EndConversation_ClearsConversationVisited_LinkedNodeResolvesAgain()
        {
            Visited.Add("SN_GuardTest_A");
            Assert.IsNull(Resolve("SN_GuardTest_A"), "precondition: guarded inside the conversation");

            EndConversationMethod.Invoke(director, new object[] { linkedA });

            Assert.AreEqual(0, Visited.Count, "EndConversation must clear the per-conversation guard");
            Assert.AreSame(linkedA, Resolve("SN_GuardTest_A"), "a shared linked node plays again in a later conversation");
        }

        [Test]
        public void Resolve_UnknownOrEmptyId_ReturnsNull()
        {
            Assert.IsNull(Resolve(""));
            Assert.IsNull(Resolve(null));
            Assert.IsNull(Resolve("SN_DoesNotExist"));
        }
    }
}
