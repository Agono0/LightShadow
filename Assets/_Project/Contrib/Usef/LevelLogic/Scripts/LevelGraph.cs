using System;
using System.Collections.Generic;

namespace LightShadow.Contrib.Usef.LevelLogic
{
    // Plain data for a "what unlocks what" model of a level. Everything is a
    // string id. No Unity types, no references to the game assembly: this
    // package only describes the level, it never reads or changes the scene.
    public enum StepKind { Pickup, Door, Puzzle, FuseBox, Gate }

    // Verified: the requirement or grant is read straight from game code at
    // the model's source commit. Inferred: derived from coordinates or naming,
    // not from a code rule. Inferred steps must say so in Note.
    public enum Confidence { Verified, Inferred }

    public sealed class LevelStep
    {
        public string Id;
        public StepKind Kind;
        // Every token here must already be held for the step to run.
        public string[] RequiresAll = new string[0];
        // When not empty, at least one of these tokens must be held as well.
        public string[] RequiresAny = new string[0];
        // Tokens the player holds after the step ran.
        public string[] Grants = new string[0];
        public Confidence Confidence;
        // Where the rule comes from, e.g. "Scripts/Tools/Editor/SceneBuilderDoors.cs".
        public string Source = "";
        public string Note = "";
    }

    public sealed class LevelGraph
    {
        public readonly List<LevelStep> Steps = new List<LevelStep>();
        // Tokens held at the start of a run.
        public readonly List<string> InitialTokens = new List<string>();
        // The token that means "the level can be finished".
        public string GoalToken = "";
        // The repo commit the model was read from (for the drift note in the docs).
        public string SourceCommit = "";
    }
}
