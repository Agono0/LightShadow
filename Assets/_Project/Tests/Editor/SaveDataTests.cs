using System;
using LightShadow.Core;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    // EditMode tests for the SaveData blob and the slot guards in SaveSystem.
    // Nothing here touches the player's persistent data folder: only the JSON
    // shape and the out-of-range / null guards are exercised, so a dev machine
    // keeps its real saves.
    public class SaveDataTests
    {
        [Test]
        public void Json_RoundTrip_PreservesEveryField()
        {
            SaveSystem.SaveData original = new SaveSystem.SaveData
            {
                version = 1,
                sceneName = "02_School",
                position = new Vector3(1.5f, 0.25f, -7f),
                yaw = 90f,
                meterValue = 42f,
                batteryFraction = 0.5f,
                flashlightOn = true,
                fusesRestored = 2,
                fragmentsCollected = 3,
                gatePowered = true,
                storyJson = "{\"flags\":[\"a\"]}",
                puzzleJson = "{\"solved\":[\"p1\"]}",
                inventoryJson = "{\"itemIds\":[\"fuse\"]}",
                playSeconds = 123.5f,
                savedAtUtc = "2026-11-27T10:00:00Z",
            };

            SaveSystem.SaveData copy = JsonUtility.FromJson<SaveSystem.SaveData>(JsonUtility.ToJson(original, true));

            Assert.AreEqual(original.version, copy.version);
            Assert.AreEqual(original.sceneName, copy.sceneName);
            Assert.AreEqual(original.position, copy.position);
            Assert.AreEqual(original.yaw, copy.yaw, 0.0001f);
            Assert.AreEqual(original.meterValue, copy.meterValue, 0.0001f);
            Assert.AreEqual(original.batteryFraction, copy.batteryFraction, 0.0001f);
            Assert.AreEqual(original.flashlightOn, copy.flashlightOn);
            Assert.AreEqual(original.fusesRestored, copy.fusesRestored);
            Assert.AreEqual(original.fragmentsCollected, copy.fragmentsCollected);
            Assert.AreEqual(original.gatePowered, copy.gatePowered);
            Assert.AreEqual(original.storyJson, copy.storyJson);
            Assert.AreEqual(original.puzzleJson, copy.puzzleJson);
            Assert.AreEqual(original.inventoryJson, copy.inventoryJson);
            Assert.AreEqual(original.playSeconds, copy.playSeconds, 0.0001f);
            Assert.AreEqual(original.savedAtUtc, copy.savedAtUtc);
        }

        [Test]
        public void NewSaveData_HasSafeDefaults()
        {
            SaveSystem.SaveData data = new SaveSystem.SaveData();
            Assert.AreEqual(1, data.version);
            Assert.AreEqual("", data.storyJson);
            Assert.AreEqual("", data.puzzleJson);
            Assert.AreEqual("", data.inventoryJson);
            Assert.IsNull(data.sceneName);
        }

        [Test]
        public void OldSave_WithoutBlobFields_DeserializesToEmptyBlobs()
        {
            // A save written before puzzleJson / inventoryJson existed.
            string oldSave = "{\"version\":1,\"sceneName\":\"02_School\",\"meterValue\":60.0,\"fusesRestored\":1}";

            SaveSystem.SaveData data = JsonUtility.FromJson<SaveSystem.SaveData>(oldSave);

            Assert.NotNull(data);
            Assert.AreEqual("02_School", data.sceneName);
            Assert.AreEqual(1, data.fusesRestored);
            Assert.AreEqual("", data.puzzleJson);
            Assert.AreEqual("", data.inventoryJson);
            Assert.AreEqual("", data.storyJson);
        }

        [Test]
        public void UnknownFields_AreIgnored()
        {
            SaveSystem.SaveData data = JsonUtility.FromJson<SaveSystem.SaveData>(
                "{\"version\":2,\"sceneName\":\"02_School\",\"someFutureField\":99}");
            Assert.NotNull(data);
            Assert.AreEqual(2, data.version);
            Assert.AreEqual("02_School", data.sceneName);
        }

        [Test]
        public void CorruptJson_ThrowsFromJsonUtility_SoLoadFromSlotMustCatch()
        {
            // Pins the reason SaveSystem.LoadFromSlot wraps FromJson in try/catch.
            Assert.Catch<Exception>(() => JsonUtility.FromJson<SaveSystem.SaveData>("{not json"));
        }

        [Test]
        public void SaveToSlot_RejectsNullAndOutOfRangeSlots()
        {
            SaveSystem.SaveData data = new SaveSystem.SaveData();
            Assert.IsFalse(SaveSystem.SaveToSlot(0, null));
            Assert.IsFalse(SaveSystem.SaveToSlot(-1, data));
            Assert.IsFalse(SaveSystem.SaveToSlot(SaveSystem.SlotCount, data));
        }

        [Test]
        public void OutOfRangeSlots_HaveNoSave_AndLoadNull()
        {
            Assert.IsFalse(SaveSystem.HasSave(-1));
            Assert.IsFalse(SaveSystem.HasSave(SaveSystem.SlotCount));
            Assert.IsNull(SaveSystem.LoadFromSlot(-1));
            Assert.IsNull(SaveSystem.LoadFromSlot(SaveSystem.SlotCount));
        }

        [Test]
        public void DeleteSlot_OutOfRange_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => SaveSystem.DeleteSlot(-1));
            Assert.DoesNotThrow(() => SaveSystem.DeleteSlot(SaveSystem.SlotCount));
        }

        [Test]
        public void SlotCount_IsThree()
        {
            Assert.AreEqual(3, SaveSystem.SlotCount);
        }

        [Test]
        public void Capture_OutsideSchoolScene_ReturnsNull()
        {
            // In EditMode there is no GameManager / school scene.
            Assert.IsNull(SaveSystem.Capture());
        }
    }
}
