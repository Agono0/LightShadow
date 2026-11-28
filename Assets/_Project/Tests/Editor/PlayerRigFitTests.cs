using LightShadow.Player;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    public class PlayerRigFitTests
    {
        [Test]
        public void GripLandsOnHandPoint()
        {
            Vector3 hand = new Vector3(0.01f, 0.035f, 0.02f);
            Vector3 grip = new Vector3(0f, 0.08f, 0f);
            Quaternion rot = Quaternion.Euler(90f, 0f, 0f);
            Vector3 root = PlayerRigFit.RootOffsetForGrip(hand, grip, rot, 1f);
            Vector3 gripInHand = root + rot * grip;
            Assert.Less((gripInHand - hand).magnitude, 1e-5f);
        }

        [Test]
        public void MuzzleFollowsRootOffset()
        {
            Vector3 root = new Vector3(0.1f, 0f, 0f);
            Vector3 p = PlayerRigFit.MuzzleInHandSpace(root, new Vector3(0f, 0.2f, 0f), Quaternion.identity, 2f);
            Assert.Less((p - new Vector3(0.1f, 0.4f, 0f)).magnitude, 1e-5f);
        }

        [Test]
        public void BrokenMarkersAreRejected()
        {
            Assert.IsTrue(PlayerRigFit.IsUsable(new Vector3(0.1f, 0.2f, 0.3f)));
            Assert.IsFalse(PlayerRigFit.IsUsable(new Vector3(float.NaN, 0f, 0f)));
            Assert.IsFalse(PlayerRigFit.IsUsable(new Vector3(float.PositiveInfinity, 0f, 0f)));
            Assert.IsFalse(PlayerRigFit.IsUsable(new Vector3(50f, 0f, 0f)));
        }

        [Test]
        public void ConfigureBodyMakesFrozenKinematicRigidbody()
        {
            GameObject go = new GameObject("RigTest");
            try
            {
                PlayerRigFit.ConfigureBody(go, 1.8f, 0.32f);
                PlayerRigFit.ConfigureBody(go, 1.8f, 0.32f); // idempotent
                Rigidbody body = go.GetComponent<Rigidbody>();
                Assert.IsTrue(body != null);
                Assert.IsTrue(body.isKinematic);
                Assert.IsFalse(body.useGravity);
                Assert.AreEqual(RigidbodyConstraints.FreezeRotation, body.constraints);
                Assert.AreEqual(1, go.GetComponents<Rigidbody>().Length);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
