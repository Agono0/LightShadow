using LightShadow.Contrib.Abdelraouf.HintLog;
using UnityEngine;

namespace LightShadow.Tools
{
    // Optional H2 hook for the hint log. SceneBuilder.BuildAll finds this class
    // by reflection and calls Build on the finished school scene; it adds one
    // object that carries the log, its input poller and the mobile hint
    // button. The log also self-creates at scene load (HintLogBootstrap), so
    // this is a convenience, not a requirement; the singletons prevent
    // duplicates either way.
    public class HintLogLevelContribution : ILevelContribution
    {
        const string RootName = "Contrib_HintLog";

        public string Id => "abdelraouf.hint-log";

        // After the inventory screen (120); nothing depends on the order.
        public int Order => 130;

        public void Build(LevelContext ctx)
        {
            Transform old = ctx.DynamicRoot.Find(RootName);
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = new GameObject(RootName) { layer = 0 };
            root.transform.SetParent(ctx.DynamicRoot, false);
            root.AddComponent<HintLogUI>();
            root.AddComponent<HintLogInput>();
            root.AddComponent<HintTouchButton>();
        }
    }
}
