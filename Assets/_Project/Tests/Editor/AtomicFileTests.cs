using System.IO;
using LightShadow.Core;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class AtomicFileTests
    {
        string dir;
        string path;

        [SetUp]
        public void SetUp()
        {
            dir = Path.Combine(Path.GetTempPath(), "ls_atomic_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            path = Path.Combine(dir, "slot.json");
        }

        [TearDown]
        public void TearDown() { if (Directory.Exists(dir)) Directory.Delete(dir, true); }

        [Test]
        public void WritesAndReadsBack()
        {
            AtomicFile.WriteAllText(path, "{\"a\":1}");
            Assert.AreEqual("{\"a\":1}", AtomicFile.ReadAllText(path));
            Assert.IsFalse(File.Exists(path + AtomicFile.TempSuffix));
        }

        [Test]
        public void SecondWriteKeepsThePreviousAsBackup()
        {
            AtomicFile.WriteAllText(path, "one");
            AtomicFile.WriteAllText(path, "two");
            Assert.AreEqual("two", File.ReadAllText(path));
            Assert.AreEqual("one", File.ReadAllText(path + AtomicFile.BackupSuffix));
        }

        [Test]
        public void TruncatedMainFileFallsBackToBackup()
        {
            AtomicFile.WriteAllText(path, "{\"ok\":true}");
            AtomicFile.WriteAllText(path, "{\"ok\":true,\"n\":2}");
            File.WriteAllText(path, "{\"ok\":tr"); // a crash mid-write
            string read = AtomicFile.ReadAllText(path, t => t.TrimEnd().EndsWith("}"));
            Assert.AreEqual("{\"ok\":true}", read);
        }

        [Test]
        public void EmptyMainFileFallsBack()
        {
            AtomicFile.WriteAllText(path, "good");
            AtomicFile.WriteAllText(path, "newer");
            File.WriteAllText(path, "");
            Assert.AreEqual("good", AtomicFile.ReadAllText(path));
        }

        [Test]
        public void MissingEverythingReturnsNull()
        {
            Assert.IsNull(AtomicFile.ReadAllText(path));
        }

        [Test]
        public void DeleteRemovesAllThreeFiles()
        {
            AtomicFile.WriteAllText(path, "a");
            AtomicFile.WriteAllText(path, "b");
            File.WriteAllText(path + AtomicFile.TempSuffix, "stale");
            AtomicFile.Delete(path);
            Assert.IsFalse(File.Exists(path));
            Assert.IsFalse(File.Exists(path + AtomicFile.BackupSuffix));
            Assert.IsFalse(File.Exists(path + AtomicFile.TempSuffix));
        }
    }
}
