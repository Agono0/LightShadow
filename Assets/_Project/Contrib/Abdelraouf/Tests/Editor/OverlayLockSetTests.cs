using System.Collections.Generic;
using LightShadow.Contrib.Abdelraouf.Overlay;
using NUnit.Framework;

namespace LightShadow.Contrib.Abdelraouf.Tests.Editor
{
    // EditMode tests for the input-lock bookkeeping behind OverlayStack.
    // Pure logic only: no scene, no frame timing from the engine (frames are
    // passed in). NOT run when written; no Unity editor was available.
    public class OverlayLockSetTests
    {
        [Test]
        public void EmptySetIsUnlocked()
        {
            var set = new OverlayLockSet();
            Assert.AreEqual(0, set.Count);
            Assert.IsFalse(set.WantsFreeCursor);
            Assert.IsFalse(set.AnyDismissable);
        }

        [Test]
        public void TwoHoldersStayLockedUntilBothRelease()
        {
            var set = new OverlayLockSet();
            InputLockToken journal = set.Acquire("Journal", true, true, null);
            InputLockToken story = set.Acquire("Story", true, false, null);
            Assert.IsTrue(set.Release(story, 1));
            Assert.AreEqual(1, set.Count, "journal still holds the lock");
            Assert.IsTrue(set.Release(journal, 2));
            Assert.AreEqual(0, set.Count);
        }

        [Test]
        public void ReleaseTwiceIsANoOpAndNeverFreesAnotherHolder()
        {
            var set = new OverlayLockSet();
            InputLockToken a = set.Acquire("A", false, false, null);
            InputLockToken b = set.Acquire("B", false, false, null);
            Assert.IsTrue(set.Release(a, 1));
            Assert.IsFalse(set.Release(a, 2), "second release does nothing");
            Assert.AreEqual(1, set.Count, "B is still held");
            Assert.IsFalse(set.ClosedOnFrame(2), "a no-op release must not mark a close");
            Assert.IsTrue(set.Release(b, 3));
        }

        [Test]
        public void NullTokenReleaseIsSafe()
        {
            var set = new OverlayLockSet();
            Assert.IsFalse(set.Release(null, 1));
        }

        [Test]
        public void StaleTokenAfterClearCannotFreeANewHolder()
        {
            var set = new OverlayLockSet();
            InputLockToken old = set.Acquire("Old", false, false, null);
            set.Clear(false, 1);
            InputLockToken fresh = set.Acquire("Fresh", false, false, null);
            Assert.IsFalse(set.Release(old, 2));
            Assert.AreEqual(1, set.Count);
            Assert.IsTrue(fresh.IsActive);
        }

        [Test]
        public void ClosedOnFrameIsSetByReleaseOnly()
        {
            var set = new OverlayLockSet();
            InputLockToken t = set.Acquire("P", false, true, null);
            Assert.IsFalse(set.ClosedOnFrame(5));
            set.Release(t, 5);
            Assert.IsTrue(set.ClosedOnFrame(5));
            Assert.IsFalse(set.ClosedOnFrame(6));
        }

        [Test]
        public void CursorAndDismissableFlagsFollowTheHolders()
        {
            var set = new OverlayLockSet();
            InputLockToken hide = set.Acquire("Hiding", false, false, null);
            Assert.IsFalse(set.WantsFreeCursor);
            InputLockToken puzzle = set.Acquire("Puzzle", true, true, null);
            Assert.IsTrue(set.WantsFreeCursor);
            Assert.IsTrue(set.AnyDismissable);
            set.Release(puzzle, 1);
            Assert.IsFalse(set.WantsFreeCursor);
            Assert.IsFalse(set.AnyDismissable);
            set.Release(hide, 2);
        }

        [Test]
        public void StoryHolderIsNotDismissableSoItDoesNotBlockPause()
        {
            var set = new OverlayLockSet();
            set.Acquire("Story", true, false, null);
            Assert.IsFalse(set.AnyDismissable);
        }

        [Test]
        public void TryCloseTopClosesTheNewestDismissableThroughItsCallback()
        {
            var set = new OverlayLockSet();
            int closedA = 0, closedB = 0;
            set.Acquire("A", true, true, () => closedA++);
            set.Acquire("Story", true, false, () => closedA += 100);
            set.Acquire("B", true, true, () => closedB++);
            Assert.IsTrue(set.TryCloseTop());
            Assert.AreEqual(1, closedB);
            Assert.AreEqual(0, closedA);
        }

        [Test]
        public void TryCloseTopWithNoDismissableReturnsFalse()
        {
            var set = new OverlayLockSet();
            set.Acquire("Story", true, false, () => Assert.Fail("not dismissable"));
            Assert.IsFalse(set.TryCloseTop());
        }

        [Test]
        public void TryCloseTopSkipsDismissableTokensWithNullCallback()
        {
            var set = new OverlayLockSet();
            int closedOlder = 0;
            set.Acquire("Older", true, true, () => closedOlder++);
            InputLockToken top = set.Acquire("Newer", true, true, null);
            Assert.IsTrue(set.TryCloseTop(), "falls through to the older closable overlay");
            Assert.AreEqual(1, closedOlder);
            Assert.IsTrue(top.IsActive, "the null-callback token stays held");
            Assert.IsTrue(set.Holds("Newer"));
        }

        [Test]
        public void TryCloseTopDispatchesNewestToOldestThenRunsOut()
        {
            var set = new OverlayLockSet();
            var order = new List<string>();
            InputLockToken a = null, b = null;
            a = set.Acquire("A", true, true, () => { order.Add("A"); set.Release(a, 1); });
            b = set.Acquire("B", true, true, () => { order.Add("B"); set.Release(b, 1); });
            set.Acquire("Story", true, false, () => Assert.Fail("not dismissable"));
            Assert.IsTrue(set.TryCloseTop());
            Assert.IsTrue(set.TryCloseTop());
            Assert.AreEqual(new[] { "B", "A" }, order.ToArray());
            Assert.AreEqual(1, set.Count, "only the non-dismissable story token remains");
            Assert.IsFalse(set.TryCloseTop(), "nothing dismissable left to close");
        }

        [Test]
        public void ClearWithCallbacksLetsOverlaysReleaseThemselves()
        {
            var set = new OverlayLockSet();
            InputLockToken journal = null;
            journal = set.Acquire("Journal", true, true, () => set.Release(journal, 9));
            set.Acquire("Hiding", false, false, null);
            int dropped = set.Clear(true, 9);
            Assert.AreEqual(0, set.Count);
            Assert.AreEqual(1, dropped, "only the non-closing holder was dropped");
            Assert.IsFalse(journal.IsActive);
        }

        [Test]
        public void ClearWithoutCallbacksDropsEverythingSilently()
        {
            var set = new OverlayLockSet();
            set.Acquire("A", true, true, () => Assert.Fail("callbacks off"));
            Assert.AreEqual(1, set.Clear(false, 1));
            Assert.AreEqual(0, set.Count);
        }

        [Test]
        public void ClearIsSafeWhenACallbackReleasesDuringIteration()
        {
            var set = new OverlayLockSet();
            InputLockToken a = null, b = null;
            a = set.Acquire("A", true, true, () => set.Release(a, 1));
            b = set.Acquire("B", true, true, () => set.Release(b, 1));
            set.Clear(true, 1);
            Assert.AreEqual(0, set.Count);
        }

        [Test]
        public void HoldsReportsByOwner()
        {
            var set = new OverlayLockSet();
            InputLockToken t = set.Acquire("Journal", true, true, null);
            Assert.IsTrue(set.Holds("Journal"));
            Assert.IsFalse(set.Holds("Story"));
            set.Release(t, 1);
            Assert.IsFalse(set.Holds("Journal"));
        }

        [Test]
        public void ClearWithCallbacks_InterruptsNonDismissableStory()
        {
            var set = new OverlayLockSet();
            InputLockToken story = null;
            int interrupted = 0;
            story = set.Acquire("Story", true, false, null, () => { interrupted++; set.Release(story, 3); });
            int dropped = set.Clear(true, 3);
            Assert.AreEqual(1, interrupted);
            Assert.AreEqual(0, dropped, "the story ended itself, nothing was dropped silently");
            Assert.AreEqual(0, set.Count);
        }

        [Test]
        public void ClearWithoutCallbacks_DoesNotInterrupt()
        {
            var set = new OverlayLockSet();
            set.Acquire("Story", true, false, null, () => Assert.Fail("callbacks off"));
            set.Clear(false, 1);
            Assert.AreEqual(0, set.Count);
        }

        [Test]
        public void ThrowingCallbackDoesNotAbortClear()
        {
            var set = new OverlayLockSet();
            int closedB = 0;
            InputLockToken b = null;
            set.Acquire("A", true, true, () => throw new System.InvalidOperationException("boom"));
            b = set.Acquire("B", true, true, () => { closedB++; set.Release(b, 1); });
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex("Overlay callback failed"));
            set.Clear(true, 1);
            Assert.AreEqual(1, closedB);
            Assert.AreEqual(0, set.Count);
        }

        [Test]
        public void TryCloseTop_ThrowingCallbackFallsThroughToNext()
        {
            var set = new OverlayLockSet();
            int closedOlder = 0;
            set.Acquire("Older", true, true, () => closedOlder++);
            set.Acquire("Newer", true, true, () => throw new System.InvalidOperationException("boom"));
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex("Overlay callback failed"));
            Assert.IsTrue(set.TryCloseTop());
            Assert.AreEqual(1, closedOlder);
        }
    }
}
