using System;

namespace LightShadow.Story
{
    // Story-side event bus, mirroring the GameEvents pattern the project
    // already uses: the director raises here, and UI / audio subscribe without
    // either of them knowing about the other. Raised with null-conditional
    // guards so an event with no listeners costs nothing.
    public static class StoryEvents
    {
        // A node went on screen (director already set ExternalInputLocked).
        public static event Action<StoryNode> NodeEntered;

        // A choice was taken (index, and the effects it applied).
        public static event Action<StoryNode, int> ChoiceMade;

        // A non-choice node finished its typewriter and moved on.
        public static event Action<StoryNode> NodeLeft;

        // The whole conversation ended - input unlocked again.
        public static event Action ConversationEnded;

        // The director picked one of the four endings (D13).
        public static event Action<string> EndingDecided;

        // Timed choice: the bar emptied and the timeout route was taken.
        public static event Action<StoryNode, int> ChoiceTimedOut;

        public static void RaiseNodeEntered(StoryNode node)
        {
            NodeEntered?.Invoke(node);
        }

        public static void RaiseChoiceMade(StoryNode node, int choiceIndex)
        {
            ChoiceMade?.Invoke(node, choiceIndex);
        }

        public static void RaiseNodeLeft(StoryNode node)
        {
            NodeLeft?.Invoke(node);
        }

        public static void RaiseConversationEnded()
        {
            ConversationEnded?.Invoke();
        }

        public static void RaiseEndingDecided(string endingId)
        {
            EndingDecided?.Invoke(endingId);
        }

        public static void RaiseChoiceTimedOut(StoryNode node, int choiceIndex)
        {
            ChoiceTimedOut?.Invoke(node, choiceIndex);
        }
    }
}
