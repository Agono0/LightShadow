using NUnit.Framework;
using UnityEngine;
using LightShadow.Core;

namespace LightShadow.Contrib.Abdelraouf.Tests.Editor
{
    // Regression coverage for the S7 save layoutVersion guard in
    // Scripts/Core/SaveSystem.cs (PR129, commit d1248d3): an old save from
    // before layout versioning (layoutVersion 0) stays compatible, while a
    // save from a DIFFERENT positive layout must not restore its position
    // inside new geometry.
    //
    // Distinct from Tests/Editor/SaveLayoutVersionTests.cs, which exercises
    // the HasUsablePosition predicate only with the DEFAULT position
    // (0, 0, 0). These cases combine the layout gate with positions the
    // S7 finite/limit guard would accept or reject on its own, so each gate
    // is proven not to bypass the other:
    //  * legacy + real position        -> kept   (old saves stay compatible)
    //  * mismatched + real position    -> dropped (the layout gate, not the
    //    finite guard, is what rejects it)
    //  * current layout + NaN position -> dropped (a matching layout never
    //    bypasses the S7 NaN guard)
    //  * legacy + NaN position         -> dropped (legacy compatibility
    //    never bypasses the S7 NaN guard either)
    //
    // NOT covered here (editor limits, stated honestly): the
    // "progress still loads" half of the policy lives inline in
    // GameManager.ApplySave and needs scene objects, and Capture() stamping
    // layoutVersion needs a live GameManager and scene. Neither is faked.
    //
    // EXECUTION STATUS: written by inspection against the sources at PR129
    // tip 7400f4d27dc8321f68f669c3542c66bf5695a749; NOT compiled or executed
    // by the author (no Unity editor available where these were written).
    // Static expected-pass is not a pass. No Assert.Inconclusive anywhere.
    //
    // FIXTURE: HasUsablePosition is a pure static predicate over the data
    // object it is handed. No GameObjects, no disk, no statics; there is
    // nothing to clean up.
    public class SaveLayoutVersionGuardTests
    {
        static readonly Vector3 RealPosition = new Vector3(12.5f, 1.2f, -30.75f);

        [Test]
        public void LegacyLayout_WithRealPosition_KeepsPosition()
        {
            var data = new SaveSystem.SaveData { layoutVersion = 0, position = RealPosition };

            Assert.IsTrue(SaveSystem.HasUsablePosition(data),
                "a pre-versioning save (layoutVersion 0) with a valid position must stay usable");
        }

        [Test]
        public void MismatchedLayout_WithRealPosition_DropsPosition()
        {
            var data = new SaveSystem.SaveData { layoutVersion = SaveSystem.LayoutVersion + 1, position = RealPosition };

            Assert.IsFalse(SaveSystem.HasUsablePosition(data),
                "a different positive layout must drop a position the finite guard would accept");
        }

        [Test]
        public void CurrentLayout_WithNaNPosition_StillDrops()
        {
            var data = new SaveSystem.SaveData { layoutVersion = SaveSystem.LayoutVersion, position = new Vector3(float.NaN, 0f, 0f) };

            Assert.IsFalse(SaveSystem.HasUsablePosition(data),
                "a matching layout must never bypass the S7 NaN guard");
        }

        [Test]
        public void LegacyLayout_WithNaNPosition_StillDrops()
        {
            var data = new SaveSystem.SaveData { layoutVersion = 0, position = new Vector3(0f, float.NaN, 0f) };

            Assert.IsFalse(SaveSystem.HasUsablePosition(data),
                "legacy compatibility must never bypass the S7 NaN guard");
        }
    }
}
