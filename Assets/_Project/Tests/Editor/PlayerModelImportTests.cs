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

        // Marker centre in the root's space from the mesh bounds, independent of
        // where the importer puts the child pivot.
        static bool MarkerPoint(GameObject root, string name, out Vector3 point)
        {
            point = Vector3.zero;
            Transform t = Find(root, name);
            if (t == null) return false;
            Renderer r = t.GetComponent<Renderer>();
            point = root.transform.InverseTransformPoint(r != null ? r.bounds.center : t.position);
            return true;
        }

        static Transform Find(GameObject root, string name)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
        }

        // Seen in the editor (Unity 6000.4.0f1, 2026-11-28): the OBJ importer
        // makes one child per "g" group and ignores "o" names, every child
        // pivot is identity at the model origin, and X is mirrored (file +X
        // becomes Unity -X). The delivered files carry "g" lines next to each
        // "o" line so the markers become separate, hideable children.
        [Test]
        public void ModelsImportWithMarkers()
        {
            GameObject torch = Load("SM_Torch");
            GameObject handL = Load("SM_Player_Hand_L");
            GameObject handR = Load("SM_Player_Hand_R");
            GameObject body = Load("SM_Player_Body");
            if (torch == null || handL == null || handR == null || body == null) return; // not delivered
            Assert.IsTrue(Find(torch, "Grip") != null, "torch Grip (the OBJ needs a 'g Grip' line)");
            Assert.IsTrue(Find(torch, "Muzzle") != null, "torch Muzzle (the OBJ needs a 'g Muzzle' line)");
            Assert.IsTrue(Find(handL, "Grip") != null, "left hand Grip");
            Assert.IsTrue(Find(handR, "Grip") != null, "right hand Grip");
            Assert.IsTrue(Find(body, "Torso") != null, "body parts split into groups");
        }

        [Test]
        public void TorchMarkersMatchTheSpec()
        {
            GameObject torch = Load("SM_Torch");
            if (torch == null) return;
            Vector3 grip, muzzle;
            Assert.IsTrue(MarkerPoint(torch, "Grip", out grip), "torch Grip");
            Assert.IsTrue(MarkerPoint(torch, "Muzzle", out muzzle), "torch Muzzle");
            Assert.AreEqual(0f, grip.magnitude, 0.002f, "grip is the torch origin");
            Assert.AreEqual(0.2f, muzzle.z, 0.002f, "muzzle at z 0.200");
            Assert.Greater(muzzle.z, grip.z + 0.1f, "torch points along +Z");
        }

        [Test]
        public void HandGripsSitAtTheSpecAfterTheXMirror()
        {
            GameObject handL = Load("SM_Player_Hand_L");
            GameObject handR = Load("SM_Player_Hand_R");
            if (handL == null || handR == null) return;
            Vector3 gl, gr;
            Assert.IsTrue(MarkerPoint(handL, "Grip", out gl), "left hand Grip");
            Assert.IsTrue(MarkerPoint(handR, "Grip", out gr), "right hand Grip");
            // File space puts the right grip at +0.034 (+X = character left);
            // Unity mirrors X, so in Unity the right hand's grip is at -0.034.
            Assert.AreEqual(-0.034f, gr.x, 0.004f, "right grip x");
            Assert.AreEqual(0.034f, gl.x, 0.004f, "left grip x");
            Assert.AreEqual(0f, gr.y, 0.004f);
            Assert.AreEqual(0.05f, gr.z, 0.01f);
            Assert.AreEqual(0.05f, gl.z, 0.01f);
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
