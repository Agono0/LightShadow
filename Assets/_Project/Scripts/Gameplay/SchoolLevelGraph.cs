namespace LightShadow.Gameplay
{
    // The school's locks and keys as data, mirroring SceneBuilderDoors and
    // PuzzleKeyRewards. Keep it in step when a door or key moves; the
    // EditMode test fails if the school can no longer be finished.
    public static class SchoolLevelGraph
    {
        public const string Start = "corridor";

        public static LevelGraphValidator Build()
        {
            var g = new LevelGraphValidator();
            g.AddRoom("entrance");
            g.AddDoor("entrance", "corridor");

            g.AddDoor("corridor", "library");
            g.AddDoor("corridor", "infirmary");
            g.AddDoor("corridor", "physics");
            g.AddDoor("corridor", "garden");
            g.AddDoor("corridor", "classroom", "classroom_key");
            g.AddDoor("corridor", "staff", "staff_key");
            g.AddDoor("corridor", "main_hall", "master_key");

            // Chalkboard cipher (classroom) pays the classroom key; it is
            // solved outside the classroom, on the cipher board in the hall.
            g.AddSource("corridor", "classroom_key");
            // Spare staff key on the infirmary shelf (audit F0 fix) and the
            // map locker inside the staff room, which pays the master key.
            g.AddSource("infirmary", "staff_key");
            g.AddSource("staff", "master_key");
            g.AddSource("infirmary", "medkit");
            return g;
        }
    }
}
