using System.Collections.Generic;
using System.Text;

namespace LightShadow.Gameplay
{
    // Pure-C# reachability check for a level's lock-and-key graph. It answers
    // one question the scene builder cannot: starting with nothing, can the
    // player reach every room and collect every key? A key that only exists
    // behind the door it opens (the staff-room soft-lock, audit F0) shows up as
    // an unreachable room.
    //
    // Interface for level authors (stable):
    //   var g = new LevelGraphValidator();
    //   g.AddRoom("hall");
    //   g.AddDoor("hall", "library", requiredItem: null);          // open door
    //   g.AddDoor("hall", "staff", requiredItem: "staff_key");     // locked door, both ways
    //   g.AddSource("library", "staff_key");                        // loose pickup
    //   g.AddSource("classroom", "master_key", requiresItem: "x");  // reward behind an item
    //   LevelGraphResult r = g.Validate("hall");
    //   r.IsSolvable, r.UnreachableRooms, r.MissingItems, r.Describe()
    public sealed class LevelGraphValidator
    {
        sealed class Door
        {
            public string a, b, item;
        }

        sealed class Source
        {
            public string room, item, needs;
        }

        readonly HashSet<string> rooms = new HashSet<string>();
        readonly List<Door> doors = new List<Door>();
        readonly List<Source> sources = new List<Source>();
        readonly HashSet<string> demanded = new HashSet<string>();

        public LevelGraphValidator AddRoom(string room)
        {
            rooms.Add(room);
            return this;
        }

        // Two-way connection; requiredItem null or empty means unlocked.
        public LevelGraphValidator AddDoor(string from, string to, string requiredItem = null)
        {
            rooms.Add(from);
            rooms.Add(to);
            doors.Add(new Door { a = from, b = to, item = string.IsNullOrEmpty(requiredItem) ? null : requiredItem });
            if (!string.IsNullOrEmpty(requiredItem)) demanded.Add(requiredItem);
            return this;
        }

        // An item the player can get once they stand in `room`, optionally
        // only after already holding `requiresItem` (a puzzle fed by a key).
        public LevelGraphValidator AddSource(string room, string item, string requiresItem = null)
        {
            rooms.Add(room);
            sources.Add(new Source { room = room, item = item, needs = string.IsNullOrEmpty(requiresItem) ? null : requiresItem });
            if (!string.IsNullOrEmpty(requiresItem)) demanded.Add(requiresItem);
            return this;
        }

        public LevelGraphResult Validate(string startRoom)
        {
            var reached = new HashSet<string>();
            var items = new HashSet<string>();
            var result = new LevelGraphResult();
            if (!rooms.Contains(startRoom))
            {
                result.Errors.Add("Start room '" + startRoom + "' is not in the graph.");
                return result;
            }

            reached.Add(startRoom);
            bool grew = true;
            while (grew)
            {
                grew = false;
                for (int i = 0; i < doors.Count; i++)
                {
                    Door d = doors[i];
                    bool openable = d.item == null || items.Contains(d.item);
                    if (!openable) continue;
                    if (reached.Contains(d.a) && reached.Add(d.b)) grew = true;
                    if (reached.Contains(d.b) && reached.Add(d.a)) grew = true;
                }
                for (int i = 0; i < sources.Count; i++)
                {
                    Source s = sources[i];
                    if (!reached.Contains(s.room)) continue;
                    if (s.needs != null && !items.Contains(s.needs)) continue;
                    if (items.Add(s.item)) grew = true;
                }
            }

            foreach (string room in rooms)
            {
                if (!reached.Contains(room)) result.UnreachableRooms.Add(room);
            }
            foreach (string item in demanded)
            {
                if (!items.Contains(item)) result.MissingItems.Add(item);
            }
            result.UnreachableRooms.Sort();
            result.MissingItems.Sort();
            result.ReachableRooms.AddRange(reached);
            result.ReachableRooms.Sort();
            result.CollectedItems.AddRange(items);
            result.CollectedItems.Sort();
            return result;
        }
    }

    public sealed class LevelGraphResult
    {
        public readonly List<string> ReachableRooms = new List<string>();
        public readonly List<string> UnreachableRooms = new List<string>();
        public readonly List<string> MissingItems = new List<string>();
        public readonly List<string> CollectedItems = new List<string>();
        public readonly List<string> Errors = new List<string>();

        public bool IsSolvable => Errors.Count == 0 && UnreachableRooms.Count == 0 && MissingItems.Count == 0;

        public string Describe()
        {
            if (IsSolvable) return "Level graph is solvable (" + ReachableRooms.Count + " rooms, " + CollectedItems.Count + " items).";
            var sb = new StringBuilder("Level graph is NOT solvable.");
            foreach (string e in Errors) sb.Append(" ").Append(e);
            if (UnreachableRooms.Count > 0) sb.Append(" Unreachable rooms: ").Append(string.Join(", ", UnreachableRooms)).Append(".");
            if (MissingItems.Count > 0) sb.Append(" Items the level demands but the player can never get: ").Append(string.Join(", ", MissingItems)).Append(".");
            return sb.ToString();
        }
    }
}
