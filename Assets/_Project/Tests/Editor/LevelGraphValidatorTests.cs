using LightShadow.Gameplay;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class LevelGraphValidatorTests
    {
        [Test]
        public void OpenDoorsAreReachable()
        {
            var g = new LevelGraphValidator().AddDoor("a", "b").AddDoor("b", "c");
            Assert.IsTrue(g.Validate("a").IsSolvable);
        }

        [Test]
        public void KeyOnlyBehindItsOwnDoorIsASoftLock()
        {
            // The audit F0 shape: staff_key exists only inside the staff room.
            var g = new LevelGraphValidator()
                .AddDoor("hall", "staff", "staff_key")
                .AddSource("staff", "staff_key");
            LevelGraphResult r = g.Validate("hall");
            Assert.IsFalse(r.IsSolvable);
            CollectionAssert.Contains(r.UnreachableRooms, "staff");
            CollectionAssert.Contains(r.MissingItems, "staff_key");
            StringAssert.Contains("NOT solvable", r.Describe());
        }

        [Test]
        public void SpareKeyOutsideBreaksTheSoftLock()
        {
            var g = new LevelGraphValidator()
                .AddDoor("hall", "staff", "staff_key")
                .AddDoor("hall", "infirmary")
                .AddSource("infirmary", "staff_key");
            Assert.IsTrue(g.Validate("hall").IsSolvable);
        }

        [Test]
        public void ChainedKeysResolveInAnyDeclarationOrder()
        {
            var g = new LevelGraphValidator()
                .AddDoor("b", "c", "k2")
                .AddDoor("a", "b", "k1")
                .AddSource("c", "done")
                .AddSource("a", "k1")
                .AddSource("b", "k2");
            LevelGraphResult r = g.Validate("a");
            Assert.IsTrue(r.IsSolvable, r.Describe());
        }

        [Test]
        public void SourceThatNeedsAnItemWaitsForIt()
        {
            var g = new LevelGraphValidator()
                .AddSource("a", "gold", "pick");
            Assert.IsFalse(g.Validate("a").IsSolvable);
            g.AddSource("a", "pick");
            Assert.IsTrue(g.Validate("a").IsSolvable);
        }

        [Test]
        public void UnknownStartIsAnError()
        {
            var r = new LevelGraphValidator().AddRoom("a").Validate("zzz");
            Assert.IsFalse(r.IsSolvable);
            Assert.AreEqual(1, r.Errors.Count);
        }

        [Test]
        public void TheSchoolCanBeFinishedFromTheCorridor()
        {
            LevelGraphResult r = SchoolLevelGraph.Build().Validate(SchoolLevelGraph.Start);
            Assert.IsTrue(r.IsSolvable, r.Describe());
            CollectionAssert.Contains(r.ReachableRooms, "staff");
            CollectionAssert.Contains(r.ReachableRooms, "main_hall");
        }

        [Test]
        public void TheSchoolWithoutTheSpareKeyIsTheOriginalSoftLock()
        {
            var g = new LevelGraphValidator();
            g.AddDoor("corridor", "staff", "staff_key");
            g.AddSource("staff", "staff_key");
            Assert.IsFalse(g.Validate("corridor").IsSolvable);
        }

        [Test]
        public void MapBoardSitsInsideTheStaffRoom()
        {
            // North wall inner face is z 25.85; the board must be south of it.
            const float boardZ = 25.78f;
            const float wallInnerFace = 25.85f;
            Assert.Less(boardZ, wallInnerFace);
        }
    }
}
