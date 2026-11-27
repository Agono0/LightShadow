using LightShadow.Core;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class WorldTextFaderTests
    {
        [Test]
        public void NearAndFacingIsFullyVisible()
        {
            Assert.AreEqual(1f, WorldTextFader.TargetFade(2f, 0f), 0.0001f);
            Assert.AreEqual(1f, WorldTextFader.TargetFade(WorldTextFader.ShowDistance, 30f), 0.0001f);
        }

        [Test]
        public void FarAwayIsInvisibleEvenWhenFacing()
        {
            Assert.AreEqual(0f, WorldTextFader.TargetFade(WorldTextFader.HideDistance, 0f), 0.0001f);
            Assert.AreEqual(0f, WorldTextFader.TargetFade(40f, 0f), 0.0001f);
        }

        [Test]
        public void BehindTheViewerIsInvisible()
        {
            Assert.AreEqual(0f, WorldTextFader.TargetFade(1f, 90f), 0.0001f);
            Assert.AreEqual(0f, WorldTextFader.TargetFade(1f, 170f), 0.0001f);
        }

        [Test]
        public void FadesSmoothlyBetweenShowAndHideDistance()
        {
            float mid = (WorldTextFader.ShowDistance + WorldTextFader.HideDistance) * 0.5f;
            float fade = WorldTextFader.TargetFade(mid, 0f);
            Assert.Greater(fade, 0.2f);
            Assert.Less(fade, 0.8f);
        }

        [Test]
        public void FadeNeverLeavesZeroToOne()
        {
            foreach (float distance in new[] { 0f, 1f, 5f, 5.5f, 6f, 100f })
            {
                foreach (float angle in new[] { 0f, 45f, 70f, 80f, 90f, 180f })
                {
                    float fade = WorldTextFader.TargetFade(distance, angle);
                    Assert.GreaterOrEqual(fade, 0f);
                    Assert.LessOrEqual(fade, 1f);
                }
            }
        }

        [Test]
        public void ApproachMovesAtItsRateAndStops()
        {
            Assert.AreEqual(0.4f, WorldTextFader.Approach(0f, 1f, 0.1f, 4f), 0.0001f);
            Assert.AreEqual(1f, WorldTextFader.Approach(0.9f, 1f, 1f, 4f), 0.0001f);
            Assert.AreEqual(0.6f, WorldTextFader.Approach(1f, 0f, 0.1f, 4f), 0.0001f);
            Assert.AreEqual(0.5f, WorldTextFader.Approach(0.5f, 0f, -1f, 4f), 0.0001f, "negative dt holds");
        }

        [Test]
        public void ShaderAndMaterialShipUnderResources()
        {
            Assert.IsNotNull(UnityEngine.Resources.Load<UnityEngine.Material>(WorldTextFader.MaterialPath),
                "Resources/" + WorldTextFader.MaterialPath + " must exist so builds include the shader");
        }
    }
}
