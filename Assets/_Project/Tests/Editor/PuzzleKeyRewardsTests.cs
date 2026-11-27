using LightShadow.Gameplay;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    // Guards the puzzle-solve reward map (keys through puzzles): the two door
    // keys come from their puzzles, unknown puzzles pay nothing.
    public class PuzzleKeyRewardsTests
    {
        [Test]
        public void ChalkboardCipher_PaysClassroomKey()
        {
            string item;
            Assert.IsTrue(PuzzleKeyRewards.TryGetReward("Puzzle_ChalkboardCipher", out item));
            Assert.AreEqual("classroom_key", item);
        }

        [Test]
        public void MapLocker_PaysStaffKey()
        {
            string item;
            Assert.IsTrue(PuzzleKeyRewards.TryGetReward("Puzzle_MapLocker", out item));
            Assert.AreEqual("staff_key", item);
        }

        [Test]
        public void UnknownPuzzle_PaysNothing()
        {
            string item;
            Assert.IsFalse(PuzzleKeyRewards.TryGetReward("Puzzle_GateCircuit", out item));
            Assert.IsFalse(PuzzleKeyRewards.TryGetReward(null, out item));
            Assert.IsFalse(PuzzleKeyRewards.TryGetReward("", out item));
        }
    }
}
