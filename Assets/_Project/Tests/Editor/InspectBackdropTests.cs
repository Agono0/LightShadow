using LightShadow.UI;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class InspectBackdropTests
    {
        [Test]
        public void Backdrop_LetsTheItemShowThrough()
        {
            Assert.GreaterOrEqual(InspectViewController.ItemVisibleShare(InspectViewController.BackdropAlpha),
                InspectViewController.MinVisibleItemShare);
        }

        [Test]
        public void OldBackdrop_HidItem()
        {
            Assert.Less(InspectViewController.ItemVisibleShare(0.94f), InspectViewController.MinVisibleItemShare);
        }

        [Test]
        public void VisibleShare_IsClamped()
        {
            Assert.AreEqual(0f, InspectViewController.ItemVisibleShare(2f), 0.0001f);
            Assert.AreEqual(1f, InspectViewController.ItemVisibleShare(-1f), 0.0001f);
        }
    }
}
