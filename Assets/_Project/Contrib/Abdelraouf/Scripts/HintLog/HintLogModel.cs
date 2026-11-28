using System;
using System.Collections.Generic;
using System.Text;
using LightShadow.Contrib.Abdelraouf.Hints;

namespace LightShadow.Contrib.Abdelraouf.HintLog
{
    // Pure view model for the Hint Recall screen. No scene access.
    //
    // SPOILER RULE (the one thing this class exists to guarantee): a hint
    // level is only ever returned when it was actually unlocked. A puzzle with
    // nothing unlocked has no row at all, and TextFor/UnlockedTexts refuse any
    // level above the unlocked count. The unlocked count is clamped to
    // 0..HintService.MaxLevel, so corrupt save data cannot reveal more.
    public class HintLogModel
    {
        public class Row
        {
            public string PuzzleId;
            public string DisplayName;
            public int Unlocked;
            public HintEntry Entry;
        }

        readonly List<Row> rows = new List<Row>();
        string selectedId;

        public IReadOnlyList<Row> Rows => rows;
        public int Count => rows.Count;
        public int SelectedIndex { get; private set; } = -1;
        public Row Selected => SelectedIndex >= 0 && SelectedIndex < rows.Count ? rows[SelectedIndex] : null;

        public static int Clamp(int level)
        {
            return Math.Max(0, Math.Min(HintService.MaxLevel, level));
        }

        // catalog: every puzzle that has hints. unlockedLevel(id): how many
        // levels of that puzzle were really shown. Puzzles at 0 are left out.
        public void Rebuild(IReadOnlyList<HintEntry> catalog, Func<string, int> unlockedLevel)
        {
            string keep = selectedId;
            rows.Clear();
            if (catalog != null && unlockedLevel != null)
            {
                for (int i = 0; i < catalog.Count; i++)
                {
                    HintEntry entry = catalog[i];
                    if (entry == null || string.IsNullOrEmpty(entry.puzzleId)) continue;
                    int level = Clamp(unlockedLevel(entry.puzzleId));
                    if (level < 1) continue;
                    if (IndexOf(entry.puzzleId) >= 0) continue; // duplicate id in data: first wins
                    rows.Add(new Row
                    {
                        PuzzleId = entry.puzzleId,
                        DisplayName = DisplayName(entry.puzzleId),
                        Unlocked = level,
                        Entry = entry,
                    });
                }
                rows.Sort((a, b) => string.CompareOrdinal(a.PuzzleId, b.PuzzleId));
            }

            int index = keep != null ? IndexOf(keep) : -1;
            if (index < 0 && rows.Count > 0) index = 0;
            SelectedIndex = index;
            selectedId = Selected != null ? Selected.PuzzleId : null;
        }

        public bool Select(int index)
        {
            if (index < 0 || index >= rows.Count || index == SelectedIndex) return false;
            SelectedIndex = index;
            selectedId = rows[index].PuzzleId;
            return true;
        }

        // Up/down through the list, wrapping at both ends.
        public bool Move(int delta)
        {
            if (rows.Count == 0 || delta == 0) return false;
            int next = ((SelectedIndex + delta) % rows.Count + rows.Count) % rows.Count;
            return Select(next);
        }

        // level: 1 nudge, 2 clue, 3 answer. Null when that level is not unlocked.
        public static string TextFor(Row row, int level, bool arabic)
        {
            if (row == null || row.Entry == null) return null;
            if (level < 1 || level > HintService.MaxLevel || level > row.Unlocked) return null;
            return row.Entry.Get(level, arabic);
        }

        // Unlocked levels only, in order 1..Unlocked. Never longer than Unlocked.
        public static List<KeyValuePair<int, string>> UnlockedTexts(Row row, bool arabic)
        {
            var result = new List<KeyValuePair<int, string>>();
            if (row == null) return result;
            for (int level = 1; level <= row.Unlocked; level++)
            {
                string text = TextFor(row, level, arabic);
                if (!string.IsNullOrEmpty(text)) result.Add(new KeyValuePair<int, string>(level, text));
            }
            return result;
        }

        public static int LockedCount(Row row)
        {
            return row == null ? 0 : HintService.MaxLevel - Clamp(row.Unlocked);
        }

        int IndexOf(string puzzleId)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].PuzzleId == puzzleId) return i;
            }
            return -1;
        }

        // "Puzzle_ChalkboardCipher" -> "Chalkboard Cipher". The puzzle id is
        // the only name the hint data carries (see HintEntry), so this is a
        // readable guess, not an authored title.
        public static string DisplayName(string puzzleId)
        {
            if (string.IsNullOrEmpty(puzzleId)) return "";
            string name = puzzleId.StartsWith("Puzzle_", StringComparison.Ordinal) ? puzzleId.Substring(7) : puzzleId;
            name = name.Replace('_', ' ');
            var builder = new StringBuilder(name.Length + 4);
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (i > 0 && char.IsUpper(c) && char.IsLower(name[i - 1])) builder.Append(' ');
                builder.Append(c);
            }
            return builder.ToString().Trim();
        }
    }
}
