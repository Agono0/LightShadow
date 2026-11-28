using LightShadow.UI;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Contrib.Ziad.Tests.Editor
{
    // N2/H1: the inspect overlay passed (position, size) pairs into helpers that take
    // (offsetMin, offsetMax) corner offsets, so with point anchors the title, body and
    // close button sat half a screen to the right. These cases cover the pure
    // CenteredOffsets math only. LIMIT: no Show fixture, no camera, no overlay stack;
    // the real overlay layout was not run in play mode.
    public class InspectViewLayoutTests
    {
        static readonly Vector2 Reference = new Vector2(1920f, 1080f);

        static void Corners(Vector2 shift, Vector2 size, out Vector2 min, out Vector2 max)
        {
            min = InspectViewController.CenteredOffsets(shift, size, out max);
        }

        [Test]
        public void ZeroShift_IsSymmetricAroundTheAnchor()
        {
            Corners(Vector2.zero, new Vector2(1100f, 260f), out Vector2 min, out Vector2 max);
            Assert.AreEqual(new Vector2(-550f, -130f), min);
            Assert.AreEqual(new Vector2(550f, 130f), max);
        }

        [Test]
        public void Size_IsMaxMinusMin_AndCentreIsTheShift()
        {
            Vector2 shift = new Vector2(0f, -320f);
            Vector2 size = new Vector2(260f, 64f);
            Corners(shift, size, out Vector2 min, out Vector2 max);
            Assert.AreEqual(size, max - min);
            Assert.AreEqual(shift, (min + max) * 0.5f);
        }

        [Test]
        public void TitleRect_StaysInsideReferenceWidth()
        {
            // Top-centre anchor: x extent is symmetric around x = 960.
            Corners(new Vector2(0f, -115f), new Vector2(1400f, 90f), out Vector2 min, out Vector2 max);
            Assert.GreaterOrEqual(Reference.x * 0.5f + min.x, 0f);
            Assert.LessOrEqual(Reference.x * 0.5f + max.x, Reference.x);
        }

        [Test]
        public void CloseButtonRect_IsHorizontallyCentred()
        {
            Corners(new Vector2(0f, -320f), new Vector2(260f, 64f), out Vector2 min, out Vector2 max);
            Assert.AreEqual(-max.x, min.x);
        }
    }
}
