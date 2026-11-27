using LightShadow.Core;
using NUnit.Framework;
using UnityEditor;

namespace LightShadow.Tests.Editor
{
    // GAP COVERED: nothing guards the shipped GameConfig.asset against a bad
    // tuning value. Only fields that exist in Scripts/Core/GameConfig.cs and have
    // an obvious valid range are checked here. UNVERIFIED LIMITS: ranges are
    // the author's reading of the field tooltips, not a design sign-off; more
    // fields can be added once the owners confirm intended ranges.
    // Not run in an editor by the author; CI test-editmode is the first run.
    public class GameConfigSanityTests
    {
        const string Path = "Assets/_Project/Data/GameConfig.asset";

        [Test]
        public void ShippedConfig_Loads()
        {
            Assert.NotNull(AssetDatabase.LoadAssetAtPath<GameConfig>(Path), Path + " must load as GameConfig");
        }

        [Test]
        public void ShippedConfig_PuzzleWrongMeterPenalty_IsNotNegative()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(Path);
            Assert.NotNull(config);
            Assert.GreaterOrEqual(config.puzzleWrongMeterPenalty, 0f);
        }
    }
}
