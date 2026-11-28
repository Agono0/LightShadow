using NUnit.Framework;
using UnityEngine;
using LightShadow.UI;

namespace LightShadow.Tests.Editor
{
    // The fragment close-up must show the visible model, not a hidden greybox
    // that happens to be the first MeshFilter in the prefab.
    public class InspectPickMeshTests
    {
        static GameObject Part(Transform parent, string name, float size, bool rendererOn)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            Mesh mesh = new Mesh();
            mesh.vertices = new[] { Vector3.zero, Vector3.one * size, new Vector3(size, 0f, 0f) };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.RecalculateBounds();
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().enabled = rendererOn;
            return go;
        }

        [Test]
        public void SkipsHiddenGreyboxAndPicksVisibleModel()
        {
            GameObject root = new GameObject("Fragment");
            GameObject hidden = Part(root.transform, "Greybox", 2f, false);
            GameObject visible = Part(root.transform, "Model", 0.5f, true);
            try
            {
                Mesh mesh; Material material;
                InspectViewController.PickVisibleMesh(root, out mesh, out material);
                Assert.IsTrue(mesh == visible.GetComponent<MeshFilter>().sharedMesh, "visible model chosen");
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void FallsBackToAnyMeshWhenNothingIsVisible()
        {
            GameObject root = new GameObject("Fragment");
            Part(root.transform, "Only", 1f, false);
            try
            {
                Mesh mesh; Material material;
                InspectViewController.PickVisibleMesh(root, out mesh, out material);
                Assert.IsTrue(mesh != null);
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
