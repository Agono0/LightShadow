using System.Collections.Generic;
using LightShadow.Contrib.Abdelraouf.InventoryScreen;
using NUnit.Framework;

namespace LightShadow.Contrib.Abdelraouf.Tests.Editor
{
    // EditMode tests for the inventory screen view model (pure logic, no
    // scene). The model is the provisional stable-snapshot layer documented
    // in InventoryScreenModel.cs.
    public class InventoryScreenModelTests
    {
        static readonly string[] Unsorted = { "staff_key", "fuse", "master_key", "classroom_key", "medkit" };

        [Test]
        public void Refresh_SortsOrdinal_AndIsStableAcrossRefreshes()
        {
            var model = new InventoryScreenModel();
            model.Refresh(Unsorted);
            var first = new List<string>(model.Items);

            Assert.AreEqual(5, first.Count);
            Assert.AreEqual("classroom_key", first[0]);
            Assert.AreEqual("master_key", first[2]);

            // A HashSet-style enumeration order differs between runs; the
            // model's sorted snapshot must not.
            var shuffled = new List<string>(Unsorted);
            shuffled.Reverse();
            model.Refresh(shuffled);
            CollectionAssert.AreEqual(first, new List<string>(model.Items));
        }

        [Test]
        public void Refresh_ClampsSelection_WhenItemsDisappear()
        {
            var model = new InventoryScreenModel { Columns = 2 };
            model.Refresh(Unsorted);
            model.Select(4);
            model.Refresh(new[] { "fuse", "medkit" });
            Assert.AreEqual(1, model.SelectedIndex);
            Assert.AreEqual("medkit", model.SelectedId);
        }

        [Test]
        public void Refresh_Empty_YieldsNullSelection()
        {
            var model = new InventoryScreenModel();
            model.Refresh(Unsorted);
            model.Refresh((IEnumerable<string>)null);
            Assert.AreEqual(0, model.Count);
            Assert.IsNull(model.SelectedId);
            Assert.IsFalse(model.MoveLeft());
        }

        [Test]
        public void MoveLeftRight_WrapsInsideTheRow()
        {
            var model = new InventoryScreenModel { Columns = 2 };
            model.Refresh(Unsorted); // sorted: classroom_key, fuse, master_key, medkit, staff_key
            model.Select(0);
            Assert.IsTrue(model.MoveLeft());
            Assert.AreEqual(1, model.SelectedIndex, "left from column 0 wraps to the row's end");
            Assert.IsTrue(model.MoveRight());
            Assert.AreEqual(0, model.SelectedIndex);
        }

        [Test]
        public void MoveUpDown_WrapsBetweenRows_AndKeepsColumnWhenPossible()
        {
            var model = new InventoryScreenModel { Columns = 2 };
            model.Refresh(Unsorted); // rows: [0,1] [2,3] [4]
            model.Select(0);
            Assert.IsTrue(model.MoveUp());
            Assert.AreEqual(4, model.SelectedIndex, "up from the top row wraps to the bottom row");
            Assert.IsTrue(model.MoveDown());
            Assert.AreEqual(0, model.SelectedIndex);

            model.Select(1);
            model.MoveDown(); // (0,1) -> (1,1) = index 3
            Assert.AreEqual(3, model.SelectedIndex);
            model.MoveDown(); // (1,1) -> (2,1) does not exist; clamps to last item
            Assert.AreEqual(4, model.SelectedIndex);
        }

        [Test]
        public void Refresh_SkipsNullAndEmptyIds()
        {
            var model = new InventoryScreenModel();
            model.Refresh(new[] { "fuse", null, "", "medkit" });
            Assert.AreEqual(2, model.Count);
        }
    }
}
