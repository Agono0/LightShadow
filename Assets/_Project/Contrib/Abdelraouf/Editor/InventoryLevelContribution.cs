using LightShadow.Contrib.Abdelraouf.InventoryScreen;
using UnityEngine;

namespace LightShadow.Tools
{
    // Optional H2 hook for the inventory screen. SceneBuilder.BuildAll finds
    // this class by reflection and calls Build on the finished school scene;
    // it adds one object that carries the screen and its input poller. The
    // screen also self-creates at scene load (InventoryScreenBootstrap), so
    // this contribution is a convenience, not a requirement; the Instance
    // singleton prevents duplicates either way.
    public class InventoryLevelContribution : ILevelContribution
    {
        const string RootName = "Contrib_InventoryScreen";

        public string Id => "abdelraouf.inventory-screen";

        // After the support systems (110); nothing depends on the order.
        public int Order => 120;

        public void Build(LevelContext ctx)
        {
            Transform old = ctx.DynamicRoot.Find(RootName);
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var root = new GameObject(RootName) { layer = 0 };
            root.transform.SetParent(ctx.DynamicRoot, false);
            root.AddComponent<InventoryScreenUI>();
            root.AddComponent<InventoryScreenInput>();
        }
    }
}
