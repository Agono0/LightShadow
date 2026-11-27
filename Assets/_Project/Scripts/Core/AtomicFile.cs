using System.IO;

namespace LightShadow.Core
{
    // Crash-safe file writes for saves (audit: SaveToSlot wrote in place, so a
    // kill mid-write left a truncated slot and lost the player's progress).
    // Write to <path>.tmp, then swap it in; the previous good file is kept as
    // <path>.bak so a read can fall back if the main file is damaged.
    public static class AtomicFile
    {
        public const string TempSuffix = ".tmp";
        public const string BackupSuffix = ".bak";

        public static void WriteAllText(string path, string text)
        {
            string temp = path + TempSuffix;
            string backup = path + BackupSuffix;
            File.WriteAllText(temp, text);
            if (File.Exists(path))
            {
                File.Copy(path, backup, true);
                File.Delete(path);
            }
            File.Move(temp, path);
        }

        // Reads the main file; if it is missing, empty or rejected by `isValid`,
        // falls back to the backup. Returns null when neither is usable.
        public static string ReadAllText(string path, System.Func<string, bool> isValid = null)
        {
            string text = TryRead(path, isValid);
            if (text != null) return text;
            return TryRead(path + BackupSuffix, isValid);
        }

        static string TryRead(string path, System.Func<string, bool> isValid)
        {
            try
            {
                if (!File.Exists(path)) return null;
                string text = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(text)) return null;
                if (isValid != null && !isValid(text)) return null;
                return text;
            }
            catch (IOException)
            {
                return null;
            }
        }

        public static void Delete(string path)
        {
            foreach (string p in new[] { path, path + TempSuffix, path + BackupSuffix })
            {
                if (File.Exists(p)) File.Delete(p);
            }
        }
    }
}
