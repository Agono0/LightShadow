using System.Reflection;
using LightShadow.Gameplay;
using LightShadow.Player;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Contrib.Ziad.Tests.Editor
{
    // D2: F3 prompt hunks - an interactable that cannot be used right now must
    // show NO verb instead of a verb that silently does nothing.
    //
    //   ThrowableProp: while another prop is carried, CanInteract is false and
    //     Prompt must be "" (it was "Pick it up").
    //   HideSpot: while Nour is hidden in a DIFFERENT spot, CanInteract is
    //     false and Prompt must be "" (it was "Hide in ...").
    //
    // Fixture policy: own GameObjects only, destroyed in TearDown; the
    // ThrowableProp.Carried and HidingSystem statics are snapshotted in SetUp
    // and restored in TearDown. No coroutines, no external fixtures.
    //
    // UNRUN: never compiled or run here; static expected-pass is not a pass.
    public class InteractionPromptTests
    {
        static readonly PropertyInfo CarriedProp = typeof(ThrowableProp).GetProperty("Carried");
        static readonly PropertyInfo IsHiddenProp = typeof(HidingSystem).GetProperty("IsHidden");
        static readonly PropertyInfo CurrentSpotProp = typeof(HidingSystem).GetProperty("CurrentSpot");

        readonly System.Collections.Generic.List<GameObject> own = new System.Collections.Generic.List<GameObject>();
        ThrowableProp wasCarried;
        bool wasHidden;
        HideSpot wasSpot;

        GameObject Own(string name)
        {
            var go = new GameObject(name);
            own.Add(go);
            return go;
        }

        [SetUp]
        public void Snapshot()
        {
            wasCarried = ThrowableProp.Carried;
            wasHidden = HidingSystem.IsHidden;
            wasSpot = HidingSystem.CurrentSpot;
        }

        [TearDown]
        public void Restore()
        {
            CarriedProp.GetSetMethod(true).Invoke(null, new object[] { wasCarried });
            IsHiddenProp.GetSetMethod(true).Invoke(null, new object[] { wasHidden });
            CurrentSpotProp.GetSetMethod(true).Invoke(null, new object[] { wasSpot });
            foreach (GameObject go in own) Object.DestroyImmediate(go);
            own.Clear();
        }

        [Test]
        public void ThrowableProp_PromptEmpty_WhileAnotherPropCarried()
        {
            ThrowableProp carried = Own("Carried").AddComponent<ThrowableProp>();
            ThrowableProp other = Own("Other").AddComponent<ThrowableProp>();
            CarriedProp.GetSetMethod(true).Invoke(null, new object[] { carried });

            Assert.IsFalse(other.CanInteract, "precondition: hands are full");
            Assert.AreEqual("", other.Prompt, "no verb while another prop is carried");
            Assert.AreEqual("Put it down", carried.Prompt, "the carried prop keeps its own verb");
        }

        [Test]
        public void HideSpot_PromptEmpty_WhileHiddenInAnotherSpot()
        {
            HideSpot occupied = Own("Occupied").AddComponent<HideSpot>();
            HideSpot other = Own("Other").AddComponent<HideSpot>();
            IsHiddenProp.GetSetMethod(true).Invoke(null, new object[] { true });
            CurrentSpotProp.GetSetMethod(true).Invoke(null, new object[] { occupied });

            Assert.IsFalse(other.CanInteract, "precondition: this spot cannot be entered now");
            Assert.AreEqual("", other.Prompt, "no verb for a spot Nour cannot enter");
        }
    }
}
