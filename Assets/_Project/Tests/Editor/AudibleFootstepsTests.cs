using LightShadow.Player;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    // The footsteps used the first AudioSource on the player root, the same
    // one AmbientDrone drives (volume 0 to 0.14), which silenced PlayOneShot.
    public class AudibleFootstepsTests
    {
        [Test]
        public void DedicatedSource_IsNotTheFirstSourceOnTheHost()
        {
            GameObject host = new GameObject("FootstepHost");
            try
            {
                AudioSource first = host.AddComponent<AudioSource>(); // what the drone grabs
                first.volume = 0f;
                AudioSource steps = Footsteps.CreateDedicatedSource(host);

                Assert.AreNotSame(first, steps);
                Assert.AreSame(first, host.GetComponent<AudioSource>(), "the drone's lookup must still find its own source");
                Assert.AreEqual(1f, steps.volume, 0.0001f);
                Assert.IsFalse(steps.loop);
                Assert.IsFalse(steps.playOnAwake);
                Assert.AreEqual(0f, steps.spatialBlend, 0.0001f);
            }
            finally { Object.DestroyImmediate(host); }
        }
    }
}
