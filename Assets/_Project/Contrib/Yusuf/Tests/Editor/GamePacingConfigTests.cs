using LightShadow.Core;
using NUnit.Framework;
using UnityEditor;

namespace LightShadow.Contrib.Yusuf.Tests.Editor
{
    // Relationships between enemy and player tuning values that the FSM relies on.
    // Reads the shipped asset. Fields missing from the serialized asset keep their code
    // defaults, so these checks cover the effective values.
    //
    // Distinct from Tests/Editor/GameConfigSanityTests.cs, which checks only
    // puzzleWrongMeterPenalty. The intent behind each ratio is read from comments in
    // EnemyBehaviour.cs and MenaceDirector.cs, not from a design sign-off; if the owners
    // change an intent on purpose, update the matching test.
    //
    // UNRUN: written by source inspection at PR130 head 2d3632f; not compiled or run in a
    // Unity editor by the author. No fixtures, no statics touched; the asset is only read.
    public class GamePacingConfigTests
    {
        const string Path = "Assets/_Project/Data/GameConfig.asset";

        // EnemyBehaviour.UpdateStalk stops at this distance (hard-coded 4f there).
        const float StalkStandoffMetres = 4f;

        GameConfig Load()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(Path);
            Assert.NotNull(config, Path + " must load as GameConfig");
            return config;
        }

        [Test]
        public void ChaseExitDistance_IsBeyondSightRange()
        {
            GameConfig c = Load();
            Assert.Greater(c.chaseExitDistance, c.sightRange,
                "otherwise a chase that starts at the edge of sight would end on its first frame");
        }

        [Test]
        public void CatchDistance_IsInsideTheStalkStandoff()
        {
            GameConfig c = Load();
            Assert.Greater(c.catchDistance, 0f);
            Assert.Less(c.catchDistance, StalkStandoffMetres,
                "a stalking wraith holds at 4 m and must not be able to catch from there");
        }

        [Test]
        public void ChaseSpeed_BeatsWalking_ButNotSprinting()
        {
            GameConfig c = Load();
            Assert.Greater(c.enemyChaseSpeed, c.walkSpeed, "walking away must not escape a chase");
            Assert.Less(c.enemyChaseSpeed, c.sprintSpeed, "sprinting must be able to open a gap");
        }

        [Test]
        public void MenacePacing_RangesAreOrdered()
        {
            GameConfig c = Load();
            Assert.LessOrEqual(c.forcedCooldownMinSeconds, c.forcedCooldownMaxSeconds,
                "the cool-down length is drawn from this range, so min must not exceed max");
            Assert.Less(c.menaceResetValue, c.menaceMax,
                "a reset value at or above the cap would re-trigger the cool-down at once");
            Assert.GreaterOrEqual(c.menaceResetValue, 0f);
            Assert.Greater(c.chaseGraceSeconds, 0f);
        }
    }
}
