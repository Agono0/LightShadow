using LightShadow.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    public class DoubleDoorTests
    {
        [Test]
        public void SecondLeafSwingsTheMirroredWay()
        {
            GameObject go = new GameObject("DoorDouble");
            GameObject a = new GameObject("A");
            GameObject b = new GameObject("B");
            try
            {
                a.transform.SetParent(go.transform, false);
                b.transform.SetParent(go.transform, false);
                DoorController door = go.AddComponent<DoorController>();
                door.hinge = a.transform;
                door.hingeB = b.transform;
                door.openAngle = -105f;

                door.RestoreFromSave(true, false, true);
                Assert.AreEqual(-105f, Mathf.DeltaAngle(0f, a.transform.localEulerAngles.y), 0.01f);
                Assert.AreEqual(105f, Mathf.DeltaAngle(0f, b.transform.localEulerAngles.y), 0.01f);

                door.RestoreFromSave(false, false, true);
                Assert.AreEqual(0f, Mathf.DeltaAngle(0f, a.transform.localEulerAngles.y), 0.01f);
                Assert.AreEqual(0f, Mathf.DeltaAngle(0f, b.transform.localEulerAngles.y), 0.01f);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
