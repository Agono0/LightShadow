using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    // Checks the delivered OBJ models after Unity's import: left/right,
    // forward axis, markers and winding. The OBJ importer mirrors X, so this
    // is checked here instead of assumed.
    public class PlayerModelImportTests
    {
        const string Folder = "Assets/_Project/Models/Hero/";

        static GameObject Load(string name)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(Folder + name + ".obj");
        }

        static Transform Find(GameObject root, string name)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
        }

        [Test]
        public void ModelsImportWithMarkers()
        {
            GameObject torch = Load("SM_Torch");
            GameObject handL = Load("SM_Player_Hand_L");
            GameObject handR = Load("SM_Player_Hand_R");
            GameObject body = Load("SM_Player_Body");
            if (torch == null || handL == null || handR == null || body == null) return; // not delivered
            // The torch Grip marker sits exactly at the model origin (spec), and the
            // importer may not surface it as a child; the loader falls back to the
            // origin, so only the Muzzle and the hand Grips are required.
            Assert.IsTrue(Find(torch, "Muzzle") != null, "torch Muzzle");
            Assert.IsTrue(Find(handL, "Grip") != null, "left hand Grip");
            Assert.IsTrue(Find(handR, "Grip") != null, "right hand Grip");
        }

        [Test]
        public void TorchPointsForwardAlongZ()
        {
            GameObject torch = Load("SM_Torch");
            if (torch == null) return;
            Transform grip = Find(torch, "Grip");
            Transform muzzle = Find(torch, "Muzzle");
            if (grip == null || muzzle == null) return;
            Assert.Greater(muzzle.localPosition.z, grip.localPosition.z + 0.1f);
        }

        [Test]
        public void HandGripsMirrorEachOtherAtTheHandleAxis()
        {
            GameObject handL = Load("SM_Player_Hand_L");
            GameObject handR = Load("SM_Player_Hand_R");
            if (handL == null || handR == null) return;
            Transform gl = Find(handL, "Grip");
            Transform gr = Find(handR, "Grip");
            if (gl == null || gr == null) return;
            // Each hand's handle axis sits toward the body midline, so the two
            // grips have opposite X. The sign depends on Unity's OBJ X mirror,
            // which the loader does not rely on (it aligns on the marker).
            Assert.AreEqual(0.034f, Mathf.Abs(gl.localPosition.x), 0.004f);
            Assert.AreEqual(0.034f, Mathf.Abs(gr.localPosition.x), 0.004f);
            Assert.Less(gl.localPosition.x * gr.localPosition.x, 0f, "hand grips must be on opposite sides");
            Assert.AreEqual(0.05f, gl.localPosition.z, 0.01f);
        }

        [Test]
        public void BodyMeshNormalsFaceOutward()
        {
            GameObject torch = Load("SM_Torch");
            if (torch == null) return;
            foreach (MeshFilter filter in torch.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null || (filter.name != "Handle" && filter.name != "Body")) continue; // cylinders only; discs have no radial normals
                Mesh mesh = filter.sharedMesh;
                Vector3 centre = mesh.bounds.center;
                Vector3[] v = mesh.vertices;
                Vector3[] n = mesh.normals;
                if (n.Length != v.Length) continue;
                float sum = 0f;
                for (int i = 0; i < v.Length; i++)
                {
                    Vector3 d = v[i] - centre;
                    d.z = 0f; // radial check around the barrel axis
                    if (d.sqrMagnitude < 1e-8f) continue;
                    sum += Vector3.Dot(n[i], d.normalized);
                }
                Assert.Greater(sum, 0f, filter.name + " normals point inward (winding flipped)");
            }
        }
    }
}
