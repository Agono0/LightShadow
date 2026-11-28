using LightShadow.UI;
using NUnit.Framework;

namespace LightShadow.Contrib.Ziad.Tests.Editor
{
    // D4 (trimmed): pure validation functions only. Per the approved trim
    // there is NO valid-Show assertion through a UI fixture - these tests pin
    // the constraint models the six Show() methods refuse on, nothing else.
    // "Refuses before UI" is guaranteed structurally: Show() calls the
    // validator and returns before touching panels or the overlay stack.
    //
    // UNRUN: never compiled or run here; static expected-pass is not a pass.
    public class PuzzleInputValidationTests
    {
        [Test]
        public void CardSort_ValidDrawers()
        {
            Assert.IsTrue(CardSortUIController.ValidDrawers(new[] { 0, 1, 2, 3 }, 4), "valid default must pass");
            Assert.IsFalse(CardSortUIController.ValidDrawers(new[] { 0, 4 }, 4), "drawer past the buttons");
            Assert.IsFalse(CardSortUIController.ValidDrawers(new[] { -1 }, 4), "negative drawer");
            Assert.IsFalse(CardSortUIController.ValidDrawers(new int[0], 4), "empty");
            Assert.IsFalse(CardSortUIController.ValidDrawers(null, 4), "null");
        }

        [Test]
        public void Optics_ValidSolution()
        {
            Assert.IsTrue(OpticsUIController.ValidSolution(new[] { 0, 1, 3 }, 3), "valid default must pass");
            Assert.IsFalse(OpticsUIController.ValidSolution(new[] { 0, 4, 1 }, 3), "rotation step past 3");
            Assert.IsFalse(OpticsUIController.ValidSolution(new[] { 0, -1, 1 }, 3), "negative step");
            Assert.IsFalse(OpticsUIController.ValidSolution(new[] { 0, 1 }, 3), "wrong length");
            Assert.IsFalse(OpticsUIController.ValidSolution(null, 3), "null");
        }

        [Test]
        public void Order_ValidDisplayOrder()
        {
            Assert.IsTrue(OrderUIController.ValidDisplayOrder(new[] { 2, 0, 1 }, 3), "a permutation must pass");
            Assert.IsFalse(OrderUIController.ValidDisplayOrder(new[] { 1, 1, 0 }, 3), "duplicate step");
            Assert.IsFalse(OrderUIController.ValidDisplayOrder(new[] { 0, 3, 1 }, 3), "step past the range");
            Assert.IsFalse(OrderUIController.ValidDisplayOrder(new[] { 0, 1 }, 3), "wrong length");
            Assert.IsFalse(OrderUIController.ValidDisplayOrder(null, 3), "null");
        }

        [Test]
        public void CodeLock_ValidCode()
        {
            Assert.IsTrue(CodeLockUIController.ValidCode("1234", 4), "valid default must pass");
            Assert.IsTrue(CodeLockUIController.ValidCode("0000", 4), "zeros are digits");
            Assert.IsFalse(CodeLockUIController.ValidCode("12a4", 4), "non-digit can never be entered");
            Assert.IsFalse(CodeLockUIController.ValidCode("123", 4), "wrong length");
            Assert.IsFalse(CodeLockUIController.ValidCode("", 4), "empty");
            Assert.IsFalse(CodeLockUIController.ValidCode(null, 4), "null");
        }

        [Test]
        public void Circuit_ValidMapping()
        {
            Assert.IsTrue(CircuitUIController.ValidMapping(new[] { 0, 1, 2 }, 3, 3), "identity mapping must pass");
            Assert.IsTrue(CircuitUIController.ValidMapping(new[] { 2, 0, 1 }, 3, 3), "a permutation must pass");
            Assert.IsFalse(CircuitUIController.ValidMapping(new[] { 0, 0, 1 }, 3, 3), "shared terminal locks once");
            Assert.IsFalse(CircuitUIController.ValidMapping(new[] { 0, 1, 3 }, 3, 3), "terminal past the range");
            Assert.IsFalse(CircuitUIController.ValidMapping(new[] { 0, 1 }, 3, 3), "wrong length");
            Assert.IsFalse(CircuitUIController.ValidMapping(null, 3, 3), "null");
        }

        [Test]
        public void Sequence_ValidSequence()
        {
            Assert.IsTrue(SequenceUIController.ValidSequence(new[] { 0, 3, 2, 0 }, 4), "valid default must pass");
            Assert.IsFalse(SequenceUIController.ValidSequence(new[] { 4 }, 4), "step past the switches");
            Assert.IsFalse(SequenceUIController.ValidSequence(new[] { -1 }, 4), "negative step");
            Assert.IsFalse(SequenceUIController.ValidSequence(new int[0], 4), "empty");
            Assert.IsFalse(SequenceUIController.ValidSequence(null, 4), "null");
        }
    }
}
