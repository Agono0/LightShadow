using System.Reflection;
using LightShadow.Core;
using NUnit.Framework;

namespace LightShadow.Contrib.Ahmed.Tests.Editor
{
    // Arabic-table coverage for the story strings the school scene builder
    // injects in SceneBuilderScenes.cs: LitZone chalkLines, KnowledgeFragment
    // names/texts and FuseBox memory flashes. Every one of their display paths
    // ends in Localization.TrPresent (HUDController.DisplayMessage,
    // InspectViewController title/body), so a missing Arabic row means raw
    // English on screen in Arabic mode.
    //
    // EXPECTED RED on the base: BuilderStoryStrings_HaveArabicRows lists the 9
    // builder-injected keys (main bc7e6e1) that have no Arabic row yet and
    // fails until the local side lands those rows in Localization.cs (labeled
    // local-row dependency - the rows are the local side's K1 edit). The test
    // proves table presence only, not translation quality. The three
    // regression cases lock the 14 sibling keys that already translate.
    //
    // Isolation: reflective snapshot of Localization's private statics
    // (current, loaded). SetLanguage is never called, so no PlayerPrefs write
    // and no LanguageChanged event; the exact prior state is restored in
    // TearDown. No scene, no coroutine, no GameObjects.
    //
    // UNRUN: never compiled or run here; static review is not a run.
    public class BuilderStoryStringArabicCoverageTests
    {
        static readonly FieldInfo CurrentField =
            typeof(Localization).GetField("current", BindingFlags.NonPublic | BindingFlags.Static);
        static readonly FieldInfo LoadedField =
            typeof(Localization).GetField("loaded", BindingFlags.NonPublic | BindingFlags.Static);

        GameLanguage priorCurrent;
        bool priorLoaded;

        [SetUp]
        public void SetUp()
        {
            Assert.NotNull(CurrentField, "Localization.current not found; the field may have been renamed");
            Assert.NotNull(LoadedField, "Localization.loaded not found; the field may have been renamed");
            priorCurrent = (GameLanguage)CurrentField.GetValue(null);
            priorLoaded = (bool)LoadedField.GetValue(null);
            // loaded=true so EnsureLoaded cannot overwrite current from PlayerPrefs.
            CurrentField.SetValue(null, GameLanguage.Arabic);
            LoadedField.SetValue(null, true);
        }

        [TearDown]
        public void TearDown()
        {
            CurrentField.SetValue(null, priorCurrent);
            LoadedField.SetValue(null, priorLoaded);
        }

        static void AssertTranslates(string key)
        {
            Assert.AreNotEqual(key, Localization.Get(key),
                key + " is missing from the Arabic table");
        }

        // EXPECTED RED until the local side's 9 Arabic rows land in Localization.cs.
        [Test]
        public void BuilderStoryStrings_HaveArabicRows()
        {
            foreach (string key in new[]
            {
                "White sheets, cold instruments. She patched a hundred scraped knees here.",
                "the pressed-flower bookmark",
                "the note in the optics cabinet",
                "the marked exam paper",
                "the pressed jasmine and photograph",
                "her mug and the note for a colleague",
                "the hand-drawn health chart",
                "Her handwriting under a drawing of a bandaged arm: \"First the hands, always the hands.\"",
                "the poetry-evening programme"
            })
            {
                AssertTranslates(key);
            }
        }

        [Test]
        public void TranslatedChalkLines_KeepTheirRows()
        {
            foreach (string key in new[]
            {
                "The entrance light still works. It watched her come in.",
                "The library holds its breath. \"Put me back together.\"",
                "The lab smells of dust and copper. The light steadies her hands.",
                "Her mug is still on the table. The room remembers her.",
                "The hall is full of empty seats. The last light is warm."
            })
            {
                AssertTranslates(key);
            }
        }

        [Test]
        public void TranslatedMemoryFlashes_KeepTheirRows()
        {
            foreach (string key in new[]
            {
                "She always said a library card is a key that never rusts.",
                "In her classroom the chalk still smelled of jasmine.",
                "The whole hall hummed when her students read aloud."
            })
            {
                AssertTranslates(key);
            }
        }

        [Test]
        public void TranslatedFragmentTexts_KeepTheirRows()
        {
            foreach (string key in new[]
            {
                "Between the pages, a jasmine flower. Mother pressed it the week I started school.",
                "\"Light travels in straight lines - so do stories.\"",
                "An encouraging note in red pen: \"You see more than you think.\"",
                "She kept the photograph of this bench, and the jasmine she picked beside it.",
                "\"Keep the light on for the ones who come after.\"",
                "Her name is on the programme, third from the bottom, in small neat print."
            })
            {
                AssertTranslates(key);
            }
        }
    }
}
