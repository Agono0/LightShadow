using LightShadow.Contrib.Abdelraouf.HintLog;
using NUnit.Framework;

namespace LightShadow.Contrib.Abdelraouf.Tests.Editor
{
    // EditMode tests for the shown-level record behind the Hint Recall screen.
    public class HintLogRecordTests
    {
        [SetUp]
        public void SetUp() { HintLogRecord.Clear(); }

        [TearDown]
        public void TearDown() { HintLogRecord.Clear(); }

        [Test]
        public void Record_KeepsHighestLevel_AndClamps()
        {
            HintLogRecord.Record("Puzzle_A", 2);
            HintLogRecord.Record("Puzzle_A", 1);
            Assert.AreEqual(2, HintLogRecord.Level("Puzzle_A"));
            HintLogRecord.Record("Puzzle_A", 50);
            Assert.AreEqual(3, HintLogRecord.Level("Puzzle_A"));
            HintLogRecord.Record("Puzzle_B", 0);
            HintLogRecord.Record("", 2);
            HintLogRecord.Record(null, 2);
            Assert.AreEqual(1, HintLogRecord.Count);
            Assert.AreEqual(0, HintLogRecord.Level("Puzzle_B"));
        }

        [Test]
        public void Json_RoundTrips()
        {
            HintLogRecord.Record("Puzzle_A", 2);
            HintLogRecord.Record("Puzzle_B", 3);
            string json = HintLogRecord.ToJson();
            HintLogRecord.Clear();
            HintLogRecord.FromJson(json);
            Assert.AreEqual(2, HintLogRecord.Level("Puzzle_A"));
            Assert.AreEqual(3, HintLogRecord.Level("Puzzle_B"));
        }

        [Test]
        public void FromJson_ClampsCorruptLevels_AndIgnoresEmpty()
        {
            HintLogRecord.FromJson("{\"ids\":[\"Puzzle_A\",\"Puzzle_B\",\"\"],\"levels\":[99,-4,2]}");
            Assert.AreEqual(3, HintLogRecord.Level("Puzzle_A"));
            Assert.AreEqual(0, HintLogRecord.Level("Puzzle_B"));
            Assert.AreEqual(1, HintLogRecord.Count);

            HintLogRecord.FromJson("");
            Assert.AreEqual(0, HintLogRecord.Count);
        }

        [Test]
        public void Adopt_ClearsOnNewOwner_UnlessRestorePending()
        {
            var first = new object();
            var second = new object();
            HintLogRecord.Adopt(first);
            HintLogRecord.Record("Puzzle_A", 2);
            HintLogRecord.Adopt(first);
            Assert.AreEqual(2, HintLogRecord.Level("Puzzle_A")); // same owner: kept

            HintLogRecord.Adopt(second);
            Assert.AreEqual(0, HintLogRecord.Level("Puzzle_A")); // fresh scene: cleared
        }
    }
}
