namespace LightShadow.Contrib.Usef.LevelLogic
{
    // The key/puzzle/gate chain of 02_School, written down by hand from game
    // code. It is a MODEL: it does not read the scene and nothing in the game
    // reads it. If the game changes, this file is stale until someone updates it.
    //
    // Read from Agono0/LightShadow main at SourceCommit (the code, not open PRs):
    //   Scripts/Tools/Editor/SceneBuilderDoors.cs   doors, key pickups, medkits
    //   Scripts/Tools/Editor/SceneBuilderPuzzles.cs puzzle names and reward pickups
    //   Scripts/Gameplay/Puzzles/GateCircuitPuzzle.cs, Gameplay/ExitGate.cs, Gameplay/FuseBox.cs
    //   Scripts/Core/GameManager.cs                  FuseTarget = 3, CanPowerGate
    // Token naming: item:<id> for Inventory ids, solved:<puzzle> for a solved puzzle,
    // access:<room> for a door that is open, fuse_box:<name> for a restored fuse box.
    public static class SchoolLevelModel
    {
        public const string SourceCommit = "c3b4604bb83c038d5e0e410f526584d633ec1dd0";
        public const string GoalToken = "goal:gate_powered";

        const string Doors = "Scripts/Tools/Editor/SceneBuilderDoors.cs";
        const string Puzzles = "Scripts/Tools/Editor/SceneBuilderPuzzles.cs";

        static string[] A(params string[] v) { return v; }

        static LevelStep Step(string id, StepKind kind, string[] all, string[] any, string[] grants,
            Confidence confidence, string source, string note)
        {
            return new LevelStep
            {
                Id = id, Kind = kind, RequiresAll = all, RequiresAny = any, Grants = grants,
                Confidence = confidence, Source = source, Note = note,
            };
        }

        // A puzzle step also always grants solved:<id>, matching PuzzleManager's solved set.
        static LevelStep Puzzle(string id, string[] all, string[] any, string[] extraGrants,
            Confidence confidence, string source, string note)
        {
            string[] grants = new string[extraGrants.Length + 1];
            grants[0] = "solved:" + id;
            for (int i = 0; i < extraGrants.Length; i++) grants[i + 1] = extraGrants[i];
            return Step(id, StepKind.Puzzle, all, any, grants, confidence, source, note);
        }

        public static LevelGraph Build()
        {
            var g = new LevelGraph { GoalToken = GoalToken, SourceCommit = SourceCommit };
            string[] none = new string[0];

            // Key chain: plain pickups today (SceneBuilderDoors.cs says moving them
            // behind puzzle solves is a follow-up).
            g.Steps.Add(Step("Key_Classroom", StepKind.Pickup, none, none, A("item:classroom_key"),
                Confidence.Verified, Doors, "Plain ItemPickup, no requirement in the builder."));
            g.Steps.Add(Step("Key_Staff", StepKind.Pickup, none, none, A("item:staff_key"),
                Confidence.Verified, Doors, "Plain ItemPickup, no requirement in the builder."));

            // The only two locked doors (startsLocked with a keyId). The other four
            // wing doors have an empty keyId and never lock.
            g.Steps.Add(Step("Door_Classroom", StepKind.Door, A("item:classroom_key"), none, A("access:classroom"),
                Confidence.Verified, Doors, "DoorController.keyId = classroom_key."));
            g.Steps.Add(Step("Door_Staff", StepKind.Door, A("item:staff_key"), none, A("access:staff_room"),
                Confidence.Verified, Doors, "DoorController.keyId = staff_key."));

            // Wing puzzles. Which room a puzzle sits in is read from its world
            // coordinates against the door positions, so the room gate is Inferred.
            g.Steps.Add(Puzzle("Puzzle_ChalkboardCipher", A("access:classroom"), none, A("item:battery"),
                Confidence.Inferred, Puzzles,
                "Reward: battery. Room gate inferred: puzzle at z=16..18, Door_Classroom at z=10."));
            g.Steps.Add(Puzzle("Puzzle_CatalogueSort", none, none, A("item:battery"),
                Confidence.Verified, Puzzles, "Reward: battery. Library door never locks."));
            g.Steps.Add(Puzzle("Puzzle_OpticsBench", none, none, A("item:battery"),
                Confidence.Verified, Puzzles, "Reward: battery. Physics door never locks."));
            g.Steps.Add(Puzzle("Puzzle_StageLights", none, none, A("item:battery"),
                Confidence.Verified, Puzzles, "Reward: battery. No door on the route in the builder."));
            g.Steps.Add(Puzzle("Puzzle_FirstAidOrder", none, none, A("item:fuse"),
                Confidence.Verified, Puzzles, "Reward: fuse item. Infirmary door never locks."));
            g.Steps.Add(Puzzle("Puzzle_MapLocker", A("access:staff_room"), none, A("item:master_key"),
                Confidence.Inferred, Puzzles,
                "Reward: master_key. Room gate inferred: puzzle at z=26, Door_Staff at z=22."));

            // Three fuse boxes. FuseBox.Interact has no item or puzzle requirement.
            g.Steps.Add(Step("Fuse_Library", StepKind.FuseBox, none, none, A("fuse_box:Fuse_Library"),
                Confidence.Verified, "Scripts/Gameplay/FuseBox.cs", "No requirement in FuseBox.Interact."));
            g.Steps.Add(Step("Fuse_Physics", StepKind.FuseBox, none, none, A("fuse_box:Fuse_Physics"),
                Confidence.Verified, "Scripts/Gameplay/FuseBox.cs", "No requirement in FuseBox.Interact."));
            g.Steps.Add(Step("Fuse_MainHall", StepKind.FuseBox, none, none, A("fuse_box:Fuse_MainHall"),
                Confidence.Verified, "Scripts/Gameplay/FuseBox.cs", "No requirement in FuseBox.Interact."));

            // Finale console: needs CanPowerGate (3 fuse boxes) and either the master
            // key item or the MapLocker puzzle being solved (older saves).
            g.Steps.Add(Puzzle("Puzzle_GateCircuit",
                A("fuse_box:Fuse_Library", "fuse_box:Fuse_Physics", "fuse_box:Fuse_MainHall"),
                A("item:master_key", "solved:Puzzle_MapLocker"), none,
                Confidence.Verified, "Scripts/Gameplay/Puzzles/GateCircuitPuzzle.cs",
                "GameManager.CanPowerGate and (Inventory master_key or MapLocker solved)."));

            // The gate itself: routes through the circuit puzzle when one is wired
            // (SceneBuilderPuzzles.cs sets exit.circuitPuzzle).
            g.Steps.Add(Step("ExitGate", StepKind.Gate, A("solved:Puzzle_GateCircuit"), none, A(GoalToken),
                Confidence.Verified, "Scripts/Gameplay/ExitGate.cs",
                "Powers after the circuit puzzle is solved. Fragments are not required by the gate."));
            return g;
        }
    }
}
