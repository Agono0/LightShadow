using LightShadow.Tools;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class SceneBuilderContentChecksTests
    {
        [Test]
        public void FullContent_HasNoProblems()
        {
            Assert.IsEmpty(SceneBuilder.ContentPresenceProblems(6, 7, 2, true, true));
        }

        [Test]
        public void StaleScene_FailsOnEveryMissingPiece()
        {
            var problems = SceneBuilder.ContentPresenceProblems(5, 0, 0, false, false);
            Assert.AreEqual(5, problems.Count);
        }

        [Test]
        public void OldFiveZoneExpectationNoLongerPasses()
        {
            Assert.IsNotEmpty(SceneBuilder.ContentPresenceProblems(5, 7, 2, true, true));
        }

        [Test]
        public void OneMissingSpareKey_IsReported()
        {
            var problems = SceneBuilder.ContentPresenceProblems(6, 7, 2, true, false);
            Assert.AreEqual(1, problems.Count);
            StringAssert.Contains("staff_key", problems[0]);
        }

        [Test]
        public void ExitCode_NonZeroOnFail()
        {
            Assert.AreEqual(1, SceneBuilder.ValidationExitCode("x\nRESULT: FAIL\n- y"));
            Assert.AreEqual(0, SceneBuilder.ValidationExitCode("RESULT: PASS"));
            Assert.AreEqual(0, SceneBuilder.ValidationExitCode(null));
        }
    }
}
