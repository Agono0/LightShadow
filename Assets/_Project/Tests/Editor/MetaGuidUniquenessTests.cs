using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    // Audit D3: 157 VO assets once shared GUIDs with a stale copy. Unity
    // keeps one owner per GUID and logs errors for the rest, so references by
    // GUID become nondeterministic. Any duplicate .meta GUID under Assets now
    // fails the suite.
    public class MetaGuidUniquenessTests
    {
        public static List<string> FindDuplicates(IEnumerable<KeyValuePair<string, string>> pathAndGuid)
        {
            var first = new Dictionary<string, string>();
            var dupes = new List<string>();
            foreach (var pair in pathAndGuid)
            {
                if (string.IsNullOrEmpty(pair.Value)) continue;
                string other;
                if (first.TryGetValue(pair.Value, out other)) dupes.Add(pair.Value + ": " + other + " | " + pair.Key);
                else first[pair.Value] = pair.Key;
            }
            return dupes;
        }

        static string GuidOf(string metaPath)
        {
            foreach (string line in File.ReadLines(metaPath))
            {
                if (line.StartsWith("guid: ")) return line.Substring(6).Trim();
            }
            return null;
        }

        [Test]
        public void FindDuplicates_ReportsCollisions()
        {
            var dupes = FindDuplicates(new[]
            {
                new KeyValuePair<string, string>("a.meta", "g1"),
                new KeyValuePair<string, string>("b.meta", "g2"),
                new KeyValuePair<string, string>("c.meta", "g1"),
                new KeyValuePair<string, string>("d.meta", null),
            });
            Assert.AreEqual(1, dupes.Count);
            StringAssert.Contains("a.meta", dupes[0]);
        }

        [Test]
        public void AssetsFolder_HasNoDuplicateGuids()
        {
            var entries = new List<KeyValuePair<string, string>>();
            foreach (string path in Directory.GetFiles(Application.dataPath, "*.meta", SearchOption.AllDirectories))
                entries.Add(new KeyValuePair<string, string>(path, GuidOf(path)));
            List<string> dupes = FindDuplicates(entries);
            Assert.IsEmpty(dupes, "duplicate GUIDs:\n" + string.Join("\n", dupes.ToArray()));
        }
    }
}
