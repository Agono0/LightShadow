using LightShadow.UI;
using NUnit.Framework;

namespace LightShadow.Contrib.Ziad.Tests.Editor
{
    // D5 (trimmed to the pure seam): the P2 discard policy shared by all six
    // puzzle overlays - a busy overlay may only close solved; an idle one may
    // be discarded. The Close(bool) guard and the close-button lock are wired
    // to this static; these tests pin the policy itself, not the wiring.
    //
    // UNRUN: never compiled or run here; static expected-pass is not a pass.
    public class PuzzleDiscardGuardTests
    {
        [Test]
        public void CardSort_CanDiscard()
        {
            Assert.IsTrue(CardSortUIController.CanDiscard(false));
            Assert.IsFalse(CardSortUIController.CanDiscard(true));
        }

        [Test]
        public void Optics_CanDiscard()
        {
            Assert.IsTrue(OpticsUIController.CanDiscard(false));
            Assert.IsFalse(OpticsUIController.CanDiscard(true));
        }

        [Test]
        public void Order_CanDiscard()
        {
            Assert.IsTrue(OrderUIController.CanDiscard(false));
            Assert.IsFalse(OrderUIController.CanDiscard(true));
        }

        [Test]
        public void CodeLock_CanDiscard()
        {
            Assert.IsTrue(CodeLockUIController.CanDiscard(false));
            Assert.IsFalse(CodeLockUIController.CanDiscard(true));
        }

        [Test]
        public void Circuit_CanDiscard()
        {
            Assert.IsTrue(CircuitUIController.CanDiscard(false));
            Assert.IsFalse(CircuitUIController.CanDiscard(true));
        }

        [Test]
        public void Sequence_CanDiscard()
        {
            Assert.IsTrue(SequenceUIController.CanDiscard(false));
            Assert.IsFalse(SequenceUIController.CanDiscard(true));
        }
    }
}
