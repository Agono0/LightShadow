using LightShadow.Story;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class StoryOfferedMappingTests
    {
        [Test]
        public void SlotMapsToTheRealChoiceWhenEarlierChoicesAreHidden()
        {
            int[] offered = { 1, 3 };
            Assert.AreEqual(1, StoryDirector.MapOfferedToChoice(offered, 0));
            Assert.AreEqual(3, StoryDirector.MapOfferedToChoice(offered, 1));
        }

        [Test]
        public void OutOfRangeSlotsAreNoChoice()
        {
            int[] offered = { 0, 2 };
            Assert.AreEqual(-1, StoryDirector.MapOfferedToChoice(offered, -1));
            Assert.AreEqual(-1, StoryDirector.MapOfferedToChoice(offered, 2));
            Assert.AreEqual(-1, StoryDirector.MapOfferedToChoice(null, 0));
            Assert.AreEqual(-1, StoryDirector.MapOfferedToChoice(new int[0], 0));
        }
    }
}
