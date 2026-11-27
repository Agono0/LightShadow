using LightShadow.Contrib.Abdelraouf.Support;
using UnityEditor;
using UnityEngine;

namespace LightShadow.Tools
{
    // Contract H2 implementation for the player-support systems (packages C2
    // and C3). SceneBuilder.BuildAll finds this class by reflection and calls
    // Build on the finished school scene. It adds one object that carries the
    // hint service and the caption service and hands them their data files.
    // It places nothing else and does not touch any existing scene object.
    public class SupportLevelContribution : ILevelContribution
    {
        const string HintsPath = "Assets/_Project/Contrib/Abdelraouf/Data/Hints/hints.json";
        const string CaptionsPath = "Assets/_Project/Contrib/Abdelraouf/Data/Captions/captions.json";
        const string RootName = "Contrib_SupportSystems";

        public string Id => "abdelraouf.support-systems";

        // After the journal (100); nothing depends on the order.
        public int Order => 110;

        public void Build(LevelContext ctx)
        {
            Transform old = ctx.DynamicRoot.Find(RootName);
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = new GameObject(RootName) { layer = 0 };
            root.transform.SetParent(ctx.DynamicRoot, false);

            var bootstrap = root.AddComponent<SupportBootstrap>();
            bootstrap.hintsJson = AssetDatabase.LoadAssetAtPath<TextAsset>(HintsPath);
            bootstrap.captionsJson = AssetDatabase.LoadAssetAtPath<TextAsset>(CaptionsPath);
            if (bootstrap.hintsJson == null)
            {
                Debug.LogError("[LightShadow] Hint data missing at " + HintsPath + ", hints will be empty.");
            }
            if (bootstrap.captionsJson == null)
            {
                Debug.LogError("[LightShadow] Caption data missing at " + CaptionsPath + ", captions will show raw keys.");
            }
        }
    }
}
