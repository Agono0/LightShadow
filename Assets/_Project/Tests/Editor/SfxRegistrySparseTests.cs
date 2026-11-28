using LightShadow.Audio;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    public class SfxRegistrySparseTests
    {
        [Test]
        public void RegisterMany_SparseArray_CompactsIndices()
        {
            AudioClip a = AudioClip.Create("sparse_a", 100, 1, 8000, false);
            AudioClip b = AudioClip.Create("sparse_b", 100, 1, 8000, false);
            try
            {
                SfxRegistry.RegisterMany("SFX_SparseTest", new AudioClip[] { a, null, b });
                Assert.AreEqual(2, SfxRegistry.VariantCount("SFX_SparseTest"));
                Assert.AreSame(a, SfxRegistry.Clip("SFX_SparseTest_0"));
                Assert.AreSame(b, SfxRegistry.Clip("SFX_SparseTest_1"));
                for (int i = 0; i < 20; i++)
                    Assert.IsTrue(SfxRegistry.ClipVariant("SFX_SparseTest", 2) != null);
            }
            finally
            {
                Object.DestroyImmediate(a);
                Object.DestroyImmediate(b);
            }
        }
    }
}
