using LightShadow.UI;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    public class MobileUiScaleTests
    {
        [Test]
        public void WidePhones_MatchHeight()
        {
            Assert.AreEqual(1f, MobileUiScale.MatchFor(2.2f));
            Assert.AreEqual(1f, MobileUiScale.MatchFor(16f / 9f));
        }

        [Test]
        public void SquarerScreens_MatchWidth_SoTheQuizCardFits()
        {
            Assert.AreEqual(0f, MobileUiScale.MatchFor(4f / 3f));
            Assert.AreEqual(0f, MobileUiScale.MatchFor(1.6f));
        }
    
        // Real card sizes from the builders against real canvases: 1600x900
        // (phones), 1600x1000 (16:10), 1600x1200 (4:3) and desktop 1080.
        static readonly Vector2[] Cards =
        {
            new Vector2(1120f, 1080f), // settings
            new Vector2(900f, 1020f),  // save slots
            new Vector2(1000f, 700f),  // how to play
        };

        static readonly Vector2[] Canvases =
        {
            new Vector2(1600f, 900f), new Vector2(1600f, 1000f),
            new Vector2(1600f, 1200f), new Vector2(1920f, 1080f),
            new Vector2(2000f, 900f),
        };

        [Test]
        public void EveryCard_FitsEveryCanvas_AfterCardFit()
        {
            foreach (Vector2 canvas in Canvases)
            foreach (Vector2 card in Cards)
            {
                float s = CardFit.FitScale(card, canvas, CardFit.Margin);
                Assert.LessOrEqual(card.y * s, canvas.y - 2f * CardFit.Margin + 0.01f, card + " in " + canvas);
                Assert.LessOrEqual(card.x * s, canvas.x - 2f * CardFit.Margin + 0.01f, card + " in " + canvas);
                Assert.LessOrEqual(s, 1f);
            }
        }

        [Test]
        public void CardFit_NeverScalesUp_AndKeepsTextReadable()
        {
            Assert.AreEqual(1f, CardFit.FitScale(new Vector2(1000f, 700f), new Vector2(1920f, 1080f), CardFit.Margin));
            // Settings on the 900 tall phone canvas stays at 0.79 or more
            // (still 0.95x the desktop size after the 1.2x mobile lift).
            Assert.GreaterOrEqual(CardFit.FitScale(new Vector2(1120f, 1080f), new Vector2(1600f, 900f), CardFit.Margin), 0.78f);
        }
}
}
