using LightShadow.Core;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    // Audit 3.1/3.2/3.4: prompts and toasts must reach the HUD as LOGICAL text
    // (shaped exactly once, at the last layer), and every interactable prompt
    // must exist in the Arabic table.
    public class ArabicPromptTests
    {
        [TearDown]
        public void TearDown() { Localization.SetLanguage(GameLanguage.English); }

        [Test]
        public void PromptsThatWereMissingNowTranslate()
        {
            Localization.SetLanguage(GameLanguage.Arabic);
            foreach (string key in new[] { "Pick it up", "Put it down", "Take the medkit", "Locked - {0} needed" })
            {
                Assert.AreNotEqual(key, Localization.Get(key), key + " is missing from the Arabic table");
            }
        }

        [Test]
        public void LockedTemplateKeepsItsPlaceholder()
        {
            Localization.SetLanguage(GameLanguage.Arabic);
            StringAssert.Contains("{0}", Localization.Get("Locked - {0} needed"));
        }

        [Test]
        public void LogicalArabicIsShapedByPresentOnce()
        {
            Localization.SetLanguage(GameLanguage.Arabic);
            string logical = Localization.Get("Pick it up");
            string once = Localization.Present(logical);
            Assert.AreNotEqual(logical, once, "Present must reshape logical Arabic");
        }

        [Test]
        public void MixingShapedAndLogicalArabicIsTheGarbleThisAuditFixed()
        {
            // The old ItemPickup shape: Present("..") + " " + logical name.
            Localization.SetLanguage(GameLanguage.Arabic);
            string logicalOnly = Localization.Present("\u062E\u0630 \u0627\u0644\u0645\u0641\u062A\u0627\u062D");
            string mixed = Localization.Present(Localization.Present("\u062E\u0630") + " \u0627\u0644\u0645\u0641\u062A\u0627\u062D");
            Assert.AreNotEqual(logicalOnly, mixed, "shaping twice changes the result; callers must pass logical text");
        }

        [Test]
        public void EnglishModeNeverTouchesText()
        {
            Localization.SetLanguage(GameLanguage.English);
            Assert.AreEqual("Pick it up", Localization.TrPresent("Pick it up"));
        }
    }
}
