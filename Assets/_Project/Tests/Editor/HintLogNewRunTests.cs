using LightShadow.Contrib.Abdelraouf.HintLog;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    // New-game hole: a load that quit to menu before any Adopt left the
    // restore flag set, so the next run adopted stale hints. The new-run
    // entry points now call HintLogRecord.Clear().
    public class HintLogNewRunTests
    {
        [SetUp]
        public void SetUp() { HintLogRecord.Clear(); }

        [TearDown]
        public void TearDown() { HintLogRecord.Clear(); }

        [Test]
        public void ClearAfterARestoreDropsStaleHintsOnTheNextAdopt()
        {
            HintLogRecord.Record("Puzzle_A", 2);
            string json = HintLogRecord.ToJson();
            HintLogRecord.Clear();
            HintLogRecord.FromJson(json);       // a restore put the hints back
            HintLogRecord.Clear();              // new game starts
            HintLogRecord.Adopt(new object());  // first scene of the new run
            Assert.AreEqual(0, HintLogRecord.Count);
            Assert.AreEqual(0, HintLogRecord.Level("Puzzle_A"));
        }

        [Test]
        public void ClearIsSafeToCallTwice()
        {
            HintLogRecord.Record("Puzzle_B", 1);
            HintLogRecord.Clear();
            HintLogRecord.Clear();
            Assert.AreEqual(0, HintLogRecord.Count);
        }

        [Test]
        public void ANewOwnerWithoutARestoreStartsEmpty()
        {
            HintLogRecord.Record("Puzzle_C", 3);
            HintLogRecord.Adopt(new object());
            Assert.AreEqual(0, HintLogRecord.Count);
        }
    }
}
