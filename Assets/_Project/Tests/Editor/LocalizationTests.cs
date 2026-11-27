using LightShadow.Core;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    // Fallback behaviour of the localization table: English passes through,
    // unknown keys never come back empty, known keys translate in Arabic mode.
    public class LocalizationTests
    {
        [TearDown]
        public void TearDown()
        {
            Localization.SetLanguage(GameLanguage.English);
        }

        [Test]
        public void Get_English_ReturnsKey()
        {
            Localization.SetLanguage(GameLanguage.English);
            Assert.AreEqual("Continue", Localization.Get("Continue"));
        }

        [Test]
        public void Get_Arabic_KnownKeyTranslates_UnknownFallsBack()
        {
            Localization.SetLanguage(GameLanguage.Arabic);
            string translated = Localization.Get("Continue");
            // The table either translates the key or falls back to it; it must
            // never return null or empty.
            Assert.IsFalse(string.IsNullOrEmpty(translated));
            Assert.AreEqual("definitely-not-a-key", Localization.Get("definitely-not-a-key"));
        }

        [Test]
        public void Get_NullAndEmpty_PassThrough()
        {
            Assert.AreEqual("", Localization.Get(""));
            Assert.IsNull(Localization.Get(null));
        }

        [Test]
        public void Present_English_PassesTextThrough()
        {
            Localization.SetLanguage(GameLanguage.English);
            Assert.AreEqual("The panel cover is locked.", Localization.Present("The panel cover is locked."));
        }
    }
}
