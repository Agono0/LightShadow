using System.Collections.Generic;
using System.Reflection;
using LightShadow.Audio;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Contrib.Ahmed.Tests.Editor
{
    // SfxRegistry runtime contract: family registration, variant indexing,
    // single-clip registration, safe misses and Clear. AudioLibrarySyncTests
    // covers the id->field name map only; nothing exercised the registry
    // itself. Footsteps, ShadowAudio, AmbientDrone and ProceduralSfx all read
    // through here, and a miscounted family is a silent sound.
    //
    // Known gap deliberately NOT locked in: RegisterMany counts non-null
    // entries but stores under the array's original indexes, so a sparse
    // array leaves holes ClipVariant can pick (reported separately; no
    // expected-red without a contract). Only the packed-path contract is
    // pinned here.
    //
    // Isolation: reflective snapshot/restore of the registry's two static
    // dictionaries; created clips are destroyed in TearDown. No scene, no
    // PlayerPrefs, no language or mixer state.
    //
    // UNRUN: never compiled or run here; static review is not a run.
    public class SfxRegistryContractTests
    {
        static readonly FieldInfo ClipsField =
            typeof(SfxRegistry).GetField("clips", BindingFlags.NonPublic | BindingFlags.Static);
        static readonly FieldInfo CountsField =
            typeof(SfxRegistry).GetField("familyCounts", BindingFlags.NonPublic | BindingFlags.Static);

        readonly List<AudioClip> created = new List<AudioClip>();
        Dictionary<string, AudioClip> priorClips;
        Dictionary<string, int> priorCounts;

        [SetUp]
        public void SetUp()
        {
            Assert.NotNull(ClipsField, "SfxRegistry.clips not found; the field may have been renamed");
            Assert.NotNull(CountsField, "SfxRegistry.familyCounts not found; the field may have been renamed");
            priorClips = new Dictionary<string, AudioClip>(Clips());
            priorCounts = new Dictionary<string, int>(Counts());
        }

        [TearDown]
        public void TearDown()
        {
            Clips().Clear();
            foreach (KeyValuePair<string, AudioClip> pair in priorClips) Clips()[pair.Key] = pair.Value;
            Counts().Clear();
            foreach (KeyValuePair<string, int> pair in priorCounts) Counts()[pair.Key] = pair.Value;
            foreach (AudioClip clip in created)
                if (clip != null) Object.DestroyImmediate(clip);
            created.Clear();
        }

        Dictionary<string, AudioClip> Clips() => (Dictionary<string, AudioClip>)ClipsField.GetValue(null);
        Dictionary<string, int> Counts() => (Dictionary<string, int>)CountsField.GetValue(null);

        AudioClip NewClip(string name)
        {
            AudioClip clip = AudioClip.Create(name, 64, 1, 44100, false);
            created.Add(clip);
            return clip;
        }

        [Test]
        public void RegisterMany_PackedArray_RegistersContiguousVariants()
        {
            AudioClip a = NewClip("t_a"), b = NewClip("t_b"), c = NewClip("t_c");
            SfxRegistry.RegisterMany("SFX_Test_Packed", new[] { a, b, c });
            Assert.AreEqual(3, SfxRegistry.VariantCount("SFX_Test_Packed"));
            Assert.AreSame(a, SfxRegistry.Clip("SFX_Test_Packed_0"));
            Assert.AreSame(b, SfxRegistry.Clip("SFX_Test_Packed_1"));
            Assert.AreSame(c, SfxRegistry.Clip("SFX_Test_Packed_2"));
        }

        [Test]
        public void RegisterMany_NullAndEmpty_AreNoOps()
        {
            SfxRegistry.RegisterMany("SFX_Test_Empty", new AudioClip[0]);
            SfxRegistry.RegisterMany("SFX_Test_Null", null);
            Assert.AreEqual(0, SfxRegistry.VariantCount("SFX_Test_Empty"));
            Assert.AreEqual(0, SfxRegistry.VariantCount("SFX_Test_Null"));
            Assert.IsNull(SfxRegistry.Clip("SFX_Test_Empty_0"));
        }

        [Test]
        public void Register_SingleClip_DoesNotStartAFamily()
        {
            AudioClip clip = NewClip("t_single");
            SfxRegistry.Register("SFX_Test_Single", clip);
            Assert.AreSame(clip, SfxRegistry.Clip("SFX_Test_Single"));
            Assert.AreEqual(0, SfxRegistry.VariantCount("SFX_Test_Single"),
                "a single Register must not create a variant family");
        }

        [Test]
        public void ClipVariant_SingleVariant_IsDeterministic()
        {
            AudioClip clip = NewClip("t_det");
            SfxRegistry.RegisterMany("SFX_Test_Det", new[] { clip });
            Assert.AreSame(clip, SfxRegistry.ClipVariant("SFX_Test_Det", 1));
        }

        [Test]
        public void MissingFamily_LookupsStaySafe()
        {
            Assert.IsNull(SfxRegistry.Clip("SFX_Test_Absent"));
            Assert.AreEqual(0, SfxRegistry.VariantCount("SFX_Test_Absent"));
            Assert.IsFalse(SfxRegistry.HasAny("SFX_Test_Absent", 3));
            Assert.IsNull(SfxRegistry.ClipVariant("SFX_Test_Absent", 0));
        }

        [Test]
        public void HasAny_SeesPartialFamilies()
        {
            SfxRegistry.Register("SFX_Test_Partial_0", NewClip("t_partial"));
            Assert.IsTrue(SfxRegistry.HasAny("SFX_Test_Partial", 3));
            Assert.IsFalse(SfxRegistry.HasAny("SFX_Test_Other", 3));
        }

        [Test]
        public void Clear_DropsClipsAndCounts()
        {
            SfxRegistry.RegisterMany("SFX_Test_Clear", new[] { NewClip("t_clear") });
            SfxRegistry.Clear();
            Assert.IsNull(SfxRegistry.Clip("SFX_Test_Clear_0"));
            Assert.AreEqual(0, SfxRegistry.VariantCount("SFX_Test_Clear"));
        }
    }
}
