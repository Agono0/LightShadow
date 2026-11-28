using System;
using System.IO;
using System.Reflection;
using LightShadow.Core;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    public class SaveHardeningTests
    {
        string tempDir;
        string priorRoot;

        [SetUp]
        public void SetUp()
        {
            priorRoot = SaveSystem.RootDirectoryOverride;
            tempDir = Path.Combine(Path.GetTempPath(), "ls_savehard_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            SaveSystem.RootDirectoryOverride = tempDir;
        }

        [TearDown]
        public void TearDown()
        {
            SaveSystem.RootDirectoryOverride = priorRoot;
            if (tempDir != null && tempDir.Contains("ls_savehard_") && Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }

        static SaveSystem.SaveData Save(string stamp)
        {
            return new SaveSystem.SaveData { sceneName = "02_School", savedAtUtc = stamp };
        }

        [Test]
        public void NewestSlot_StamplessSlotNeverBeatsStampedOne()
        {
            Assert.IsTrue(SaveSystem.SaveToSlot(0, Save("2026-01-01T00:00:00Z")));
            Assert.IsTrue(SaveSystem.SaveToSlot(1, Save("")));
            Assert.IsTrue(SaveSystem.SaveToSlot(2, Save("2025-01-01T00:00:00Z")));
            Assert.AreEqual(0, SaveSystem.NewestSlot());
        }

        [Test]
        public void NewestSlot_OnlyStampless_StillOffersASlot()
        {
            Assert.IsTrue(SaveSystem.SaveToSlot(1, Save("")));
            Assert.AreEqual(1, SaveSystem.NewestSlot());
        }

        [Test]
        public void NewestSlot_PicksLatestStamp()
        {
            Assert.IsTrue(SaveSystem.SaveToSlot(0, Save("2026-01-01T00:00:00Z")));
            Assert.IsTrue(SaveSystem.SaveToSlot(2, Save("2026-03-01T00:00:00Z")));
            Assert.AreEqual(2, SaveSystem.NewestSlot());
        }

        [Test]
        public void Sanitize_ReplacesNonFiniteValues()
        {
            var data = Save("x");
            data.yaw = float.NaN;
            data.meterValue = float.PositiveInfinity;
            data.batteryFraction = 7f;
            SaveSystem.Sanitize(data);
            Assert.AreEqual(0f, data.yaw);
            Assert.AreEqual(60f, data.meterValue);
            Assert.AreEqual(1f, data.batteryFraction);
        }

        [Test]
        public void HasUsablePosition_RejectsNaNAndHugeValues()
        {
            var data = Save("x");
            data.position = new Vector3(1f, 0f, 2f);
            Assert.IsTrue(SaveSystem.HasUsablePosition(data));
            data.position = new Vector3(float.NaN, 0f, 0f);
            Assert.IsFalse(SaveSystem.HasUsablePosition(data));
            data.position = new Vector3(0f, 1e9f, 0f);
            Assert.IsFalse(SaveSystem.HasUsablePosition(data));
            Assert.IsFalse(SaveSystem.HasUsablePosition(null));
        }

        [Test]
        public void InvokeEach_ThrowingSubscriberDoesNotSkipOthers()
        {
            MethodInfo invoke = typeof(SaveSystem).GetMethod("InvokeEach", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(invoke, "SaveSystem.InvokeEach missing or renamed");
            int ran = 0;
            Action<SaveSystem.SaveData> handlers = d => { throw new InvalidOperationException("boom"); };
            handlers += d => ran++;
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("handler failed"));
            invoke.Invoke(null, new object[] { handlers, Save("x"), "Test" });
            Assert.AreEqual(1, ran);
        }
    }
}
