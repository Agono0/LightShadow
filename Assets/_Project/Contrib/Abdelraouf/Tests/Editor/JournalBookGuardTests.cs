using System.Reflection;
using LightShadow.Contrib.Abdelraouf.Journal;
using LightShadow.Contrib.Abdelraouf.Overlay;
using LightShadow.Core;
using LightShadow.UI;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.Tests.Editor
{
    // EditMode regression tests for the REAL guard order in
    // Contrib/Abdelraouf/Scripts/Journal/JournalBook.cs Open() (audit O3):
    //   (1) IsOpen
    //   (2) PauseMenu.IsOpen || PlayerController.ExternalInputLocked
    //   (3) GameManager.Instance == null
    //   (4) gm.quizUI != null && gm.quizUI.IsOpen
    //   (5) InspectViewController.IsOpen
    // and only then OverlayStack.Acquire("Journal", true, true, Close).
    //
    // EXECUTION STATUS: written by inspection against the sources at
    // feat/audit-fixes tip d5e73c96565d1a262f69647770404fd517de65b2; NOT
    // compiled or executed by the author (no Unity editor available where
    // these were written). Every reflection lookup is hard-asserted non-null
    // before use: a renamed or removed member fails the test loudly. No
    // Assert.Inconclusive anywhere.
    //
    // SINGLETON / STATIC FIXTURE CLEANUP:
    // - JournalBook.Instance is set in Awake and cleared in OnDestroy.
    //   GameManager.Instance likewise. Every created GameObject is
    //   DestroyImmediate'd in TearDown; because edit-mode OnDestroy dispatch
    //   is unverified from this suite's own runs, TearDown additionally
    //   clears each static through its private setter ONLY while it still
    //   points at the instance this fixture created, so another fixture's
    //   singleton is never overwritten.
    // - OverlayStack is a static token store. SetUp asserts it is empty
    //   (a refused Open() must leave it untouched) instead of blanket
    //   ResetAll: the only token these tests can ever hold is the "Journal"
    //   token the positive control acquires, and TearDown releases it through
    //   the real Close() if an assertion left the book open.
    // - PauseMenu.IsOpen (static, private setter) is never opened here and is
    //   asserted closed as a fixture guard, so guard (2) cannot make a
    //   refusal test pass for the wrong reason.
    //   PlayerController.ExternalInputLocked is a view of
    //   OverlayStack.AnyLocked, so the asserted-empty stack covers it too.
    // - The GameConfig the GameManager Awake creates when none is assigned
    //   belongs to this fixture and is DestroyImmediate'd in TearDown.
    public class JournalBookGuardTests
    {
        static readonly MethodInfo JournalBookAwakeMethod =
            typeof(JournalBook).GetMethod("Awake", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo GameManagerAwakeMethod =
            typeof(GameManager).GetMethod("Awake", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo JournalBookInstanceSetter =
            typeof(JournalBook).GetMethod("set_Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo GameManagerInstanceSetter =
            typeof(GameManager).GetMethod("set_Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        GameObject bookHost;
        JournalBook book;
        GameObject gmHost;
        GameManager gm;
        GameConfig createdConfig;

        [SetUp]
        public void SetUp()
        {
            Assert.NotNull(JournalBookAwakeMethod, "JournalBook.Awake missing or renamed");
            Assert.NotNull(GameManagerAwakeMethod, "GameManager.Awake missing or renamed");
            Assert.NotNull(JournalBookInstanceSetter, "JournalBook.Instance private setter missing or renamed");
            Assert.NotNull(GameManagerInstanceSetter, "GameManager.Instance private setter missing or renamed");
            Assert.AreEqual(0, OverlayStack.Count, "fixture leak: overlay locks held before the test");
            Assert.IsFalse(OverlayStack.Holds("Journal"), "fixture leak: Journal token held before the test");
            Assert.IsFalse(PauseMenu.IsOpen, "fixture leak: pause menu open before the test");
            Assert.IsNull(JournalBook.Instance, "fixture leak: a JournalBook singleton exists before the test");
            Assert.IsNull(GameManager.Instance, "fixture leak: a GameManager singleton exists before the test");
        }

        [TearDown]
        public void TearDown()
        {
            if (book != null && book.IsOpen) book.Close(); // releases only this fixture's Journal token
            if (bookHost != null) Object.DestroyImmediate(bookHost); // OnDestroy clears JournalBook.Instance
            if (gmHost != null) Object.DestroyImmediate(gmHost); // OnDestroy clears GameManager.Instance
            if (createdConfig != null) Object.DestroyImmediate(createdConfig);
            // Belt-and-suspenders: edit-mode OnDestroy dispatch is unverified,
            // so clear only the slots this fixture filled.
            if (JournalBook.Instance == book && JournalBookInstanceSetter != null)
                JournalBookInstanceSetter.Invoke(null, new object[] { null });
            if (GameManager.Instance == gm && GameManagerInstanceSetter != null)
                GameManagerInstanceSetter.Invoke(null, new object[] { null });
            book = null;
            bookHost = null;
            gm = null;
            gmHost = null;
            createdConfig = null;
        }

        // A fresh JournalBook from AddComponent. Awake is NOT invoked: the
        // editor loop does not dispatch it synchronously for AddComponent, so
        // panel stays null, IsOpen stays false and Instance stays unset.
        JournalBook CreateBook()
        {
            bookHost = new GameObject("JournalBookGuardTest", typeof(JournalBook));
            book = bookHost.GetComponent<JournalBook>();
            Assert.IsFalse(book.IsOpen, "a book whose Awake never ran must start closed");
            return book;
        }

        // Creates the game singleton the way EditMode tests in this repo do:
        // Awake is invoked explicitly (idempotent; builds a code-default
        // GameConfig when none is assigned) because the editor loop does not
        // dispatch it synchronously for AddComponent. quizUI stays null.
        GameManager CreateGameManager()
        {
            gmHost = new GameObject("GameManagerGuardTest", typeof(GameManager));
            gm = gmHost.GetComponent<GameManager>();
            GameManagerAwakeMethod.Invoke(gm, null);
            Assert.AreSame(gm, GameManager.Instance, "GameManager.Awake did not set Instance");
            createdConfig = gm.config;
            return gm;
        }

        [Test]
        public void Open_NoGameManager_Refuses_NoLockTaken()
        {
            CreateBook();

            book.Open();

            Assert.IsFalse(book.IsOpen, "Open() must refuse when GameManager.Instance is null");
            Assert.AreEqual(0, OverlayStack.Count, "a refused Open() must not take an overlay lock");
            Assert.IsFalse(OverlayStack.Holds("Journal"), "a refused Open() must not hold the Journal token");
        }

        // Positive control: proves the refusal above comes from the
        // null-GameManager guard and not from a broken fixture. The EditMode
        // UI build is grounded by inspection: BuildUi is pure uGUI
        // construction, Localization.CurrentFont falls back to the builtin
        // LegacyRuntime font (a null font assigns without throwing), and
        // Refresh() with zero found notes takes only null-safe paths.
        [Test]
        public void Open_NoBlockers_Opens_AndCloseReleases()
        {
            CreateGameManager();
            CreateBook();
            JournalBookAwakeMethod.Invoke(book, null); // BuildUi; panel starts inactive
            Assert.AreSame(book, JournalBook.Instance, "JournalBook.Awake did not set Instance");

            book.Open();

            Assert.IsTrue(book.IsOpen, "Open() must succeed when no guard blocks it");
            Assert.IsTrue(OverlayStack.Holds("Journal"), "an opened journal must hold the Journal token");

            book.Close();

            Assert.IsFalse(book.IsOpen, "Close() must shut the panel");
            Assert.IsFalse(OverlayStack.Holds("Journal"), "Close() must release the Journal token");
            Assert.AreEqual(0, OverlayStack.Count, "Close() must leave the overlay stack empty");
        }
    }
}
