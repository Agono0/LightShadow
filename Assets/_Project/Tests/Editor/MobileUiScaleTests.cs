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
            new Vector2(1200f, 840f),  // how to play
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
            Assert.AreEqual(1f, CardFit.FitScale(new Vector2(1200f, 840f), new Vector2(1920f, 1080f), CardFit.Margin));
            // Settings on the 900 tall phone canvas stays at 0.79 or more
            // stays at full scale now that the card is compact.
            Assert.AreEqual(1f, CardFit.FitScale(new Vector2(1120f, SettingsPanelController.CardHeight), new Vector2(1600f, 900f), CardFit.Margin));
        }

        [Test]
        public void SettingsCard_FitsPhoneCanvasWithoutShrinking_AndArrowsAreTouchSized()
        {
            Assert.GreaterOrEqual(SettingsPanelController.ArrowWidth, 48f);
            Assert.GreaterOrEqual(SettingsPanelController.ArrowHeight, 48f);
            // The arrow of the tallest page (12 rows) must end above the
            // bottom buttons (top edge at CardHeight - 98).
            float lastArrowBottom = SettingsPanelController.RowTop + 11 * SettingsPanelController.RowPitch
                - SettingsPanelController.ArrowDrop + SettingsPanelController.ArrowHeight;
            Assert.Less(lastArrowBottom, SettingsPanelController.CardHeight - 98f);
            Assert.LessOrEqual(SettingsPanelController.CardHeight, 900f - 2f * CardFit.Margin);
        }

        [Test]
        public void HowToBody_EstimatedHeightFitsItsBox()
        {
            // Builder texts without tags: English 1198 chars, Arabic 1016, 11
            // hard breaks each. Body box 1104 x 640 at 21 px, line spacing 1.08
            // (MakeText); best fit may shrink to 17.
            Assert.LessOrEqual(CardFit.EstimateTextHeight(1198, 11, 1104f, 21f, 1.08f), 640f);
            Assert.LessOrEqual(CardFit.EstimateTextHeight(1016, 11, 1104f, 21f, 1.08f), 640f);
            Assert.LessOrEqual(CardFit.EstimateTextHeight(1198 * 12 / 10, 11, 1104f, 17f, 1.08f), 640f);
        }
}
}
