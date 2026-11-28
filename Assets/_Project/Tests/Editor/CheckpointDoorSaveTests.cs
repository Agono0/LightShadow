using NUnit.Framework;
using UnityEngine;
using LightShadow.Core;
using LightShadow.Gameplay;

namespace LightShadow.Tests
{
    public class CheckpointDoorSaveTests
    {
        [Test]
        public void PickZone_PrefersNearestSameName()
        {
            string[] names = { "Lit", "Lit", "Other" };
            Vector3[] pos = { new Vector3(0, 0, 0), new Vector3(10, 0, 0), new Vector3(9, 0, 0) };
            Assert.AreEqual(1, CheckpointManager.PickZone(names, pos, "Lit", new Vector3(9, 0, 0)));
        }

        [Test]
        public void PickZone_MissingNameIsMinusOne()
        {
            Assert.AreEqual(-1, CheckpointManager.PickZone(new[] { "A" }, new[] { Vector3.zero }, "B", Vector3.zero));
            Assert.AreEqual(-1, CheckpointManager.PickZone(null, null, "A", Vector3.zero));
        }

        [Test]
        public void Checkpoint_SaveRoundTripsAndNoCheckpointIsDefault()
        {
            GameObject root = new GameObject("cm");
            GameObject spawn = new GameObject("spawn");
            GameObject zone = new GameObject("LitZone_Hall");
            try
            {
                CheckpointManager manager = root.AddComponent<CheckpointManager>();
                manager.defaultSpawn = spawn.transform;
                // Awake ran on AddComponent in the editor; defaultSpawn was null then.
                manager.RestoreFromJson("");
                Assert.AreSame(spawn.transform, manager.SpawnPoint);
                Assert.IsFalse(manager.Capture().lit);

                manager.RestoreFromJson(JsonUtility.ToJson(new CheckpointManager.CheckpointSave { lit = false }));
                Assert.AreSame(spawn.transform, manager.SpawnPoint);
                manager.RestoreFromJson("not json");
                Assert.AreSame(spawn.transform, manager.SpawnPoint);
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(spawn);
                Object.DestroyImmediate(zone);
            }
        }

        [Test]
        public void DoorId_StableAndDistinguishesPositions()
        {
            Assert.AreEqual(DoorController.IdFor("Door", new Vector3(1, 0, 2)), DoorController.IdFor("Door", new Vector3(1.01f, 0, 2)));
            Assert.AreNotEqual(DoorController.IdFor("Door", new Vector3(1, 0, 2)), DoorController.IdFor("Door", new Vector3(5, 0, 2)));
        }

        [Test]
        public void Door_RestoreAppliesLockOpenFreed()
        {
            GameObject go = new GameObject("Door");
            try
            {
                DoorController door = go.AddComponent<DoorController>();
                door.keyId = "key";
                door.RestoreFromSave(false, true, false);
                Assert.IsTrue(door.IsLocked);
                Assert.IsFalse(door.IsOpen);
                door.RestoreFromSave(true, false, true);
                Assert.IsFalse(door.IsLocked);
                Assert.IsTrue(door.IsOpen);
                Assert.IsTrue(door.PlayerFreed);
                door.keyId = "";
                door.RestoreFromSave(false, true, false);
                Assert.IsFalse(door.IsLocked, "a door with no key never locks");
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
