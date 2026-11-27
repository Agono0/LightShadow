using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using LightShadow.UI;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    // Cheap guards against silent audio failures: a mistyped SFX id or a VO
    // clip that exists in one language only plays nothing, with no error.
    public class AssetCoverageTests
    {
        const string Root = "Assets/_Project";

        static HashSet<string> Names(string dir)
        {
            var set = new HashSet<string>();
            foreach (string path in Directory.GetFiles(dir))
            {
                if (path.EndsWith(".meta")) continue;
                set.Add(Path.GetFileNameWithoutExtension(path));
            }
            return set;
        }

        [Test]
        public void VoiceClips_EnglishAndArabicSetsMatch()
        {
            HashSet<string> en = Names(Root + "/Resources/VO/EN");
            HashSet<string> ar = Names(Root + "/Resources/VO/AR");
            var onlyEn = new HashSet<string>(en);
            onlyEn.ExceptWith(ar);
            var onlyAr = new HashSet<string>(ar);
            onlyAr.ExceptWith(en);
            Assert.IsEmpty(onlyEn, "EN-only VO clips: " + string.Join(", ", onlyEn));
            Assert.IsEmpty(onlyAr, "AR-only VO clips: " + string.Join(", ", onlyAr));
        }

        [Test]
        public void EndingLines_HaveClipsInBothLanguages()
        {
            HashSet<string> en = Names(Root + "/Resources/VO/EN");
            HashSet<string> ar = Names(Root + "/Resources/VO/AR");
            foreach (EndingCinematicController.EndingLine line in EndingCinematicController.Lines)
            {
                Assert.IsTrue(en.Contains(line.clipId), "no EN clip for " + line.clipId);
                Assert.IsTrue(ar.Contains(line.clipId), "no AR clip for " + line.clipId);
            }
        }

        // Every literal id passed to Sfx.Play / Sfx.PlayAt / Sfx.Pick must
        // match a shipped file "<id>.<ext>" or a numbered family "<id>_0.<ext>".
        [Test]
        public void SfxCalls_ReferenceShippedClips()
        {
            HashSet<string> shipped = Names(Root + "/Audio/SFX");
            var call = new Regex("Sfx\\.(?:Play|PlayAt|Pick)\\(\\s*\"(SFX_[A-Za-z0-9_]+)\"");
            var missing = new List<string>();
            foreach (string file in Directory.GetFiles(Root + "/Scripts", "*.cs", SearchOption.AllDirectories))
            {
                foreach (Match m in call.Matches(File.ReadAllText(file)))
                {
                    string id = m.Groups[1].Value;
                    if (!shipped.Contains(id) && !shipped.Contains(id + "_0"))
                        missing.Add(Path.GetFileName(file) + ": " + id);
                }
            }
            Assert.IsEmpty(missing, "SFX ids with no clip: " + string.Join("; ", missing));
        }
    }
}
