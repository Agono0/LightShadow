using LightShadow.Core;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    public class RestoreFollowupTests
    {
        [SetUp]
        public void SetUp() { SaveSystem.ClearPendingRestore(); }

        [TearDown]
        public void TearDown() { SaveSystem.ClearPendingRestore(); }

        [Test]
        public void EnemyBlob_RoundTrips()
        {
            string json = EnemySaveState.ToJson(new Vector3(1f, 2f, 3f), 42f, true);
            EnemySaveState.Blob blob;
            Assert.IsTrue(EnemySaveState.TryParse(json, out blob));
            Assert.AreEqual(new Vector3(1f, 2f, 3f), blob.Position);
            Assert.AreEqual(42f, blob.gauge);
            Assert.IsTrue(blob.aggressive);
        }

        [Test]
        public void EnemyBlob_RejectsEmptyAndGarbage()
        {
            EnemySaveState.Blob blob;
            Assert.IsFalse(EnemySaveState.TryParse("", out blob));
            Assert.IsFalse(EnemySaveState.TryParse(null, out blob));
            Assert.IsFalse(EnemySaveState.TryParse("not json", out blob));
        }

        [Test]
        public void EnemyBlob_RejectsNonFinite()
        {
            EnemySaveState.Blob blob;
            Assert.IsFalse(EnemySaveState.TryParse("{\"x\":NaN,\"y\":0,\"z\":0,\"gauge\":0}", out blob));
        }

        [Test]
        public void EnemyIsOnlyPlacedAwayFromThePlayer()
        {
            Vector3 player = Vector3.zero;
            Assert.IsFalse(EnemySaveState.ShouldPlace(new Vector3(3f, 0f, 0f), player));
            Assert.IsTrue(EnemySaveState.ShouldPlace(new Vector3(EnemySaveState.MinPlayerDistance, 0f, 0f), player));
            Assert.IsFalse(EnemySaveState.ShouldPlace(new Vector3(float.NaN, 0f, 0f), player));
        }

        [Test]
        public void RestoredFlag_IsConsumedOnce_AndClearedByNewRun()
        {
            Assert.IsFalse(SaveSystem.ConsumeRestoredFlag());
            SaveSystem.NotifySaveApplied(new SaveSystem.SaveData());
            Assert.IsTrue(SaveSystem.ConsumeRestoredFlag());
            Assert.IsFalse(SaveSystem.ConsumeRestoredFlag());

            SaveSystem.NotifySaveApplied(new SaveSystem.SaveData());
            SaveSystem.ClearPendingRestore(); // what a new game calls
            Assert.IsFalse(SaveSystem.ConsumeRestoredFlag());
        }

        [Test]
        public void NavMeshProjection_WithoutANavMesh_LeavesThePointAlone()
        {
            Vector3 projected;
            Vector3 point = new Vector3(5f, 1f, 5f);
            bool ok = SaveSystem.TryProjectToNavMesh(point, 1.5f, out projected);
            if (!ok) Assert.AreEqual(point, projected);
            else Assert.AreEqual(point.y, projected.y);
        }
    }
}
