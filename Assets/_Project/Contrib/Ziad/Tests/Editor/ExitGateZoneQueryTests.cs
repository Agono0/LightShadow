using LightShadow.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Contrib.Ziad.Tests.Editor
{
    // D3: pure cases for ExitGate.ContainsPlayer, the scan seam behind
    // PlayerInsideEscapeZone. Per the approved trim the PlayerController
    // POSITIVE case is deliberately OMITTED - no player fixture is built here,
    // so the true-branch of the scan is not verified by these tests, only the
    // safe false-branches and the defensive tolerances.
    //
    // Own GameObjects only, destroyed immediately. No physics scene needed.
    //
    // UNRUN: never compiled or run here; static expected-pass is not a pass.
    public class ExitGateZoneQueryTests
    {
        [Test]
        public void ContainsPlayer_NullArray_ReturnsFalse()
        {
            Assert.IsFalse(ExitGate.ContainsPlayer(null, 0));
            Assert.IsFalse(ExitGate.ContainsPlayer(null, 8));
        }

        [Test]
        public void ContainsPlayer_EmptyArray_ReturnsFalse()
        {
            Assert.IsFalse(ExitGate.ContainsPlayer(new Collider[0], 0));
        }

        [Test]
        public void ContainsPlayer_NullEntries_ReturnsFalse()
        {
            Assert.IsFalse(ExitGate.ContainsPlayer(new Collider[] { null, null }, 2));
        }

        [Test]
        public void ContainsPlayer_CountPastLength_IsClampedNotThrown()
        {
            var go = new GameObject("Plain");
            try
            {
                Collider plain = go.AddComponent<BoxCollider>();
                Assert.DoesNotThrow(() => ExitGate.ContainsPlayer(new[] { plain }, 16));
                Assert.IsFalse(ExitGate.ContainsPlayer(new[] { plain }, 16));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ContainsPlayer_PlainColliders_ReturnFalse()
        {
            var go = new GameObject("Plain");
            try
            {
                Collider plain = go.AddComponent<BoxCollider>();
                Assert.IsFalse(ExitGate.ContainsPlayer(new[] { plain }, 1),
                    "a collider with no PlayerController above it must not count");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
