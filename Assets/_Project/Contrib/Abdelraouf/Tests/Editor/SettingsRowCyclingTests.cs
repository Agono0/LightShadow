using System.Reflection;
using LightShadow.UI;
using NUnit.Framework;

namespace LightShadow.Contrib.Abdelraouf.Tests.Editor
{
    // SettingsPanelController's private static row-cycling helpers
    // (PreviousIndex / NextIndex, SettingsPanelController.cs:344/:349 at the
    // base commit): pure index math for the fine-override cycle
    // -1 (follow tier) -> 0 -> ... -> count-1 -> -1. Covered counts are the
    // call-site row counts Shadows 4, MSAA 3 and RenderScale 6.
    // The helpers are private static and reached by reflection, like
    // SettingsRowLabelTests; both reflection lookups are hard-asserted in
    // SetUp. The helpers are pure (no PlayerPrefs, no statics touched), so
    // there is nothing to save or restore.
    // NOT compiled and NOT run when written (no Unity in this environment);
    // expected values are argued from the source, no pass is claimed.
    public class SettingsRowCyclingTests
    {
        static readonly MethodInfo PreviousIndexMethod = typeof(SettingsPanelController)
            .GetMethod("PreviousIndex", BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo NextIndexMethod = typeof(SettingsPanelController)
            .GetMethod("NextIndex", BindingFlags.NonPublic | BindingFlags.Static);

        static readonly int[] RowCounts = { 4, 3, 6 }; // Shadows, MSAA, RenderScale

        [SetUp]
        public void ReflectionTargetsExist()
        {
            Assert.NotNull(PreviousIndexMethod,
                "SettingsPanelController.PreviousIndex(int, int) not found by reflection");
            Assert.NotNull(NextIndexMethod,
                "SettingsPanelController.NextIndex(int, int) not found by reflection");
        }

        static int Prev(int current, int count)
        {
            return (int)PreviousIndexMethod.Invoke(null, new object[] { current, count });
        }

        static int Next(int current, int count)
        {
            return (int)NextIndexMethod.Invoke(null, new object[] { current, count });
        }

        [Test]
        public void PreviousIndex_FromMinusOneWrapsToLastSlot()
        {
            foreach (int count in RowCounts)
                Assert.AreEqual(count - 1, Prev(-1, count), "count " + count);
        }

        [Test]
        public void PreviousIndex_FromZeroWrapsToFollowTier()
        {
            foreach (int count in RowCounts)
                Assert.AreEqual(-1, Prev(0, count), "count " + count);
        }

        [Test]
        public void PreviousIndex_MidStepsDownByOne()
        {
            Assert.AreEqual(1, Prev(2, 4)); // Shadows
            Assert.AreEqual(1, Prev(2, 3)); // MSAA
            Assert.AreEqual(4, Prev(5, 6)); // RenderScale
        }

        [Test]
        public void NextIndex_FromFollowTierGoesToFirstSlot()
        {
            foreach (int count in RowCounts)
                Assert.AreEqual(0, Next(-1, count), "count " + count);
        }

        [Test]
        public void NextIndex_FromLastSlotWrapsToFollowTier()
        {
            foreach (int count in RowCounts)
                Assert.AreEqual(-1, Next(count - 1, count), "count " + count);
        }

        [Test]
        public void NextIndex_MidStepsUpByOne()
        {
            Assert.AreEqual(2, Next(1, 4)); // Shadows
            Assert.AreEqual(1, Next(0, 3)); // MSAA
            Assert.AreEqual(3, Next(2, 6)); // RenderScale
        }

        [Test]
        public void Cycling_RoundTripsThroughEveryIndex()
        {
            foreach (int count in RowCounts)
            {
                for (int i = -1; i < count; i++)
                {
                    Assert.AreEqual(i, Prev(Next(i, count), count),
                        "next-then-prev, count " + count + ", index " + i);
                    Assert.AreEqual(i, Next(Prev(i, count), count),
                        "prev-then-next, count " + count + ", index " + i);
                }
            }
        }
    }
}
