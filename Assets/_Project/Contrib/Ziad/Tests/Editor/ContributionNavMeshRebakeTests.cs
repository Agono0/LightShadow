using LightShadow.Tools;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Contrib.Ziad.Tests.Editor
{
    // H3 (hardening): SceneBuilder.NeedsNavMeshRebake decides whether geometry
    // under Contrib_Static would be collected by the school NavMeshSurface
    // (PhysicsColliders, layerMask = LevelLayer, all objects), and so whether
    // RunLevelContributions must rebake. Pure cases on the helper only.
    //
    // Fixture policy: bare GameObjects this fixture creates, destroyed
    // immediately in TearDown. No scene open or saved, no NavMeshSurface, no
    // bake, no assets, no singletons, cameras, overlay stack or RenderTextures.
    //
    // Assumptions mirrored from the NavMesh builder: trigger colliders are not
    // collected (Unity "As Designed", UUM-54608); inactive objects and disabled
    // colliders are assumed not collected (not verified from docs here).
    //
    // UNRUN: never compiled or run here; no Unity editor was available. The
    // real rebake (BuildNavMesh, asset recreate, scene save) is not covered.
    public class ContributionNavMeshRebakeTests
    {
        int mask;
        GameObject root;

        [SetUp]
        public void SetUp()
        {
            mask = 1 << SceneBuilder.LevelLayer;
            root = new GameObject("Contrib_Static_Test");
            root.layer = SceneBuilder.LevelLayer;
        }

        [TearDown]
        public void TearDown()
        {
            if (root != null) Object.DestroyImmediate(root);
        }

        GameObject Child(string name, Transform parent, int layer)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.layer = layer;
            return go;
        }

        [Test]
        public void NullRoot_False()
        {
            Assert.IsFalse(SceneBuilder.NeedsNavMeshRebake(null, mask));
        }

        [Test]
        public void EmptyRoot_False()
        {
            Assert.IsFalse(SceneBuilder.NeedsNavMeshRebake(root.transform, mask));
        }

        [Test]
        public void LayerOnly_NoCollider_False()
        {
            Child("Wall", root.transform, SceneBuilder.LevelLayer);
            Assert.IsFalse(SceneBuilder.NeedsNavMeshRebake(root.transform, mask));
        }

        [Test]
        public void DirectChild_LevelLayerCollider_True()
        {
            Child("Wall", root.transform, SceneBuilder.LevelLayer).AddComponent<BoxCollider>();
            Assert.IsTrue(SceneBuilder.NeedsNavMeshRebake(root.transform, mask));
        }

        [Test]
        public void NestedDescendant_LevelLayerCollider_True()
        {
            GameObject a = Child("A", root.transform, 0);
            GameObject b = Child("B", a.transform, 0);
            Child("Wall", b.transform, SceneBuilder.LevelLayer).AddComponent<BoxCollider>();
            Assert.IsTrue(SceneBuilder.NeedsNavMeshRebake(root.transform, mask));
        }

        [Test]
        public void Collider_WrongLayer_False()
        {
            Child("Prop", root.transform, 0).AddComponent<BoxCollider>();
            Assert.IsFalse(SceneBuilder.NeedsNavMeshRebake(root.transform, mask));
        }

        [Test]
        public void DisabledCollider_False()
        {
            BoxCollider c = Child("Wall", root.transform, SceneBuilder.LevelLayer).AddComponent<BoxCollider>();
            c.enabled = false;
            Assert.IsFalse(SceneBuilder.NeedsNavMeshRebake(root.transform, mask));
        }

        [Test]
        public void TriggerCollider_False()
        {
            BoxCollider c = Child("Zone", root.transform, SceneBuilder.LevelLayer).AddComponent<BoxCollider>();
            c.isTrigger = true;
            Assert.IsFalse(SceneBuilder.NeedsNavMeshRebake(root.transform, mask));
        }

        [Test]
        public void InactiveObject_False()
        {
            GameObject wall = Child("Wall", root.transform, SceneBuilder.LevelLayer);
            wall.AddComponent<BoxCollider>();
            wall.SetActive(false);
            Assert.IsFalse(SceneBuilder.NeedsNavMeshRebake(root.transform, mask));
        }

        [Test]
        public void InactiveParent_ActiveChildCollider_False()
        {
            GameObject parent = Child("Parent", root.transform, 0);
            Child("Wall", parent.transform, SceneBuilder.LevelLayer).AddComponent<BoxCollider>();
            parent.SetActive(false);
            Assert.IsFalse(SceneBuilder.NeedsNavMeshRebake(root.transform, mask));
        }

        [Test]
        public void InvalidFirst_ThenValidSibling_True()
        {
            BoxCollider off = Child("Off", root.transform, SceneBuilder.LevelLayer).AddComponent<BoxCollider>();
            off.enabled = false;
            Child("Wrong", root.transform, 0).AddComponent<BoxCollider>();
            Child("Good", root.transform, SceneBuilder.LevelLayer).AddComponent<BoxCollider>();
            Assert.IsTrue(SceneBuilder.NeedsNavMeshRebake(root.transform, mask));
        }
    }
}
