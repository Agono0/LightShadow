using System.Collections.Generic;
using LightShadow.UI;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    // Guards the unified ending script table (spec: Docs/ENDING_VO_SPEC.md):
    // 14 lines, unique node/clip ids in the VO_<nodeId> convention, both
    // languages present, valid speaker channels.
    public class EndingCinematicTests
    {
        [Test]
        public void LineTable_HasFourteenLinesWithUniqueIds()
        {
            Assert.AreEqual(14, EndingCinematicController.Lines.Length);

            var nodeIds = new HashSet<string>();
            var clipIds = new HashSet<string>();
            foreach (EndingCinematicController.EndingLine line in EndingCinematicController.Lines)
            {
                Assert.IsFalse(string.IsNullOrEmpty(line.nodeId));
                Assert.IsTrue(nodeIds.Add(line.nodeId), "duplicate nodeId " + line.nodeId);
                Assert.AreEqual("VO_" + line.nodeId, line.clipId);
                Assert.IsTrue(clipIds.Add(line.clipId), "duplicate clipId " + line.clipId);
            }
        }

        [Test]
        public void LineTable_BothLanguagesPresentAndSpeakersValid()
        {
            foreach (EndingCinematicController.EndingLine line in EndingCinematicController.Lines)
            {
                Assert.IsFalse(string.IsNullOrEmpty(line.en), line.nodeId + " missing EN");
                Assert.IsFalse(string.IsNullOrEmpty(line.ar), line.nodeId + " missing AR");
                Assert.IsTrue(line.speaker >= 0 && line.speaker <= 3,
                    line.nodeId + " speaker out of range: " + line.speaker);
            }
        }

        [Test]
        public void FindLine_ResolvesEveryNodeId()
        {
            foreach (EndingCinematicController.EndingLine line in EndingCinematicController.Lines)
                Assert.AreSame(line, EndingCinematicController.FindLine(line.nodeId));
            Assert.IsNull(EndingCinematicController.FindLine("SN_Does_Not_Exist"));
        }
    }
}
