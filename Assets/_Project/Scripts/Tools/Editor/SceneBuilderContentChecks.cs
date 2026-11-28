using System.Collections.Generic;

namespace LightShadow.Tools
{
    // Pure content-presence rules for the validation gate (audit B1). A scene
    // that predates the puzzle, spare-key and medkit builders must FAIL, not
    // pass on counts that never changed.
    public static partial class SceneBuilder
    {
        public const int ExpectedLitZones = 6;   // entrance, library, physics, main hall, staff, infirmary
        public const int ExpectedPuzzles = 7;    // Puzzle_* roots from SceneBuilderPuzzles
        public const int ExpectedMedkits = 2;

        public static List<string> ContentPresenceProblems(
            int litZones, int puzzles, int medkits, bool classroomSpareKey, bool staffSpareKey)
        {
            var problems = new List<string>();
            if (litZones != ExpectedLitZones)
                problems.Add("School: expected " + ExpectedLitZones + " lit zones, found " + litZones + ".");
            if (puzzles < ExpectedPuzzles)
                problems.Add("School: expected " + ExpectedPuzzles + " puzzles, found " + puzzles + " (stale scene? rebuild with LightShadow/3. Build Scenes).");
            if (medkits < ExpectedMedkits)
                problems.Add("School: expected " + ExpectedMedkits + " medkits, found " + medkits + ".");
            if (!classroomSpareKey) problems.Add("School: no classroom_key source outside the classroom (spare key missing).");
            if (!staffSpareKey) problems.Add("School: no staff_key source outside the staff room (spare key missing).");
            return problems;
        }

        // Batch exit code for a validation report: non-zero on FAIL.
        public static int ValidationExitCode(string report)
        {
            return report != null && report.Contains("RESULT: FAIL") ? 1 : 0;
        }
    }
}
