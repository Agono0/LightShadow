using LightShadow.Core;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    // Contract H1: per-feature save blobs.
    public class SaveContribTests
    {
        [Test]
        public void SetGet_RoundTripsThroughJson()
        {
            var data = new SaveSystem.SaveData();
            SaveSystem.SetContrib(data, "journal", "{\"notes\":[\"n1\"]}");
            SaveSystem.SetContrib(data, "hints", "{\"used\":2}");
            SaveSystem.SetContrib(data, "journal", "{\"notes\":[\"n1\",\"n2\"]}");

            var copy = JsonUtility.FromJson<SaveSystem.SaveData>(JsonUtility.ToJson(data));

            Assert.AreEqual("{\"notes\":[\"n1\",\"n2\"]}", SaveSystem.GetContrib(copy, "journal"));
            Assert.AreEqual("{\"used\":2}", SaveSystem.GetContrib(copy, "hints"));
            Assert.AreEqual(2, copy.contrib.Count);
        }

        [Test]
        public void Get_ReturnsEmptyForMissingKeyOrOldSave()
        {
            var old = JsonUtility.FromJson<SaveSystem.SaveData>("{\"version\":1,\"sceneName\":\"02_School\"}");
            Assert.AreEqual("", SaveSystem.GetContrib(old, "journal"));
            Assert.AreEqual("", SaveSystem.GetContrib(null, "journal"));
            Assert.AreEqual("", SaveSystem.GetContrib(new SaveSystem.SaveData(), null));
        }
    }
}
