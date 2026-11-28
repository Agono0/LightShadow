using NUnit.Framework;
using LightShadow.UI;

namespace LightShadow.Tests
{
    // Seam only: the cancel result is distinct from every real answer.
    public class QuizCancelTests
    {
        [Test]
        public void CancelIndex_isNotATimeoutOrAnOption()
        {
            Assert.IsTrue(QuizUIController.IsCancel(QuizUIController.CancelIndex));
            Assert.IsFalse(QuizUIController.IsCancel(-1), "-1 is the timer running out, a real wrong answer");
            for (int i = 0; i < 4; i++) Assert.IsFalse(QuizUIController.IsCancel(i));
        }
    }
}
