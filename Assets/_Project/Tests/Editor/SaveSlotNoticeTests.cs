using LightShadow.UI;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class SaveSlotNoticeTests
    {
        [Test]
        public void Notice_ReadsCloseAndHidesCancel()
        {
            Assert.AreEqual("CLOSE", SaveSlotUI.YesLabelKey(true));
            Assert.IsFalse(SaveSlotUI.ShowNoButton(true));
        }

        [Test]
        public void Confirm_KeepsConfirmCancelPair()
        {
            Assert.AreEqual("CONFIRM", SaveSlotUI.YesLabelKey(false));
            Assert.IsTrue(SaveSlotUI.ShowNoButton(false));
        }
    }
}
