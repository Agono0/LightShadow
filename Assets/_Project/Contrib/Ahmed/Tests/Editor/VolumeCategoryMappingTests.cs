using System;
using System.Reflection;
using LightShadow.Audio;
using NUnit.Framework;

namespace LightShadow.Contrib.Ahmed.Tests.Editor
{
    // Bus and category mapping contracts for VolumeCategories: the persisted
    // Category ordinals, the source-name -> category convention, and the two
    // private tables that map categories onto the real mixer's groups and
    // exposed parameters (GroupNames / GroupToParameter).
    //
    // Pure and reflection-only: no AudioSource, no mixer asset, no scene, no
    // PlayerPrefs (VolumeCategories type init is literal-only; PlayerPrefs is
    // read solely inside Bootstrap, which nothing here invokes). No static is
    // read that these tests mutate; nothing is mutated at all.
    //
    // UNRUN: never compiled or run here; static expected-pass is not a pass.
    public class VolumeCategoryMappingTests
    {
        static readonly MethodInfo CategoryFromNameMethod =
            typeof(VolumeCategories).GetMethod("CategoryFromName", BindingFlags.NonPublic | BindingFlags.Static);

        static T PrivateStatic<T>(string fieldName)
        {
            FieldInfo field = typeof(VolumeCategories).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(field, "VolumeCategories." + fieldName + " not found; the table may have been renamed");
            return (T)field.GetValue(null);
        }

        static VolumeCategories.Category CategoryFromName(string name)
        {
            return (VolumeCategories.Category)CategoryFromNameMethod.Invoke(null, new object[] { name });
        }

        // PrefKeys is indexed by (int)category, so moving an ordinal silently
        // remaps every saved slider setting (the enum is documented
        // append-only for exactly this reason).
        [Test]
        public void CategoryEnum_KeepsPersistedOrdinals()
        {
            Assert.AreEqual(0, (int)VolumeCategories.Category.Master);
            Assert.AreEqual(1, (int)VolumeCategories.Category.Music);
            Assert.AreEqual(2, (int)VolumeCategories.Category.Ambience);
            Assert.AreEqual(3, (int)VolumeCategories.Category.Sfx);
            Assert.AreEqual(4, (int)VolumeCategories.Category.Ui);
            Assert.AreEqual(5, (int)VolumeCategories.Category.Voice);
            Assert.AreEqual(6, (int)VolumeCategories.Category.Shadow);
            Assert.AreEqual(7, Enum.GetValues(typeof(VolumeCategories.Category)).Length,
                "a new category must be appended, not inserted");
        }

        [TestCase("MUS_MenuTheme", VolumeCategories.Category.Music)]
        [TestCase("MusicPlayer", VolumeCategories.Category.Music)]
        [TestCase("SomeMusicBox", VolumeCategories.Category.Music)]
        [TestCase("AMB_SchoolNight", VolumeCategories.Category.Ambience)]
        [TestCase("AmbienceBed", VolumeCategories.Category.Ambience)]
        [TestCase("SFX_DoorSlam", VolumeCategories.Category.Sfx)]
        [TestCase("SfxButton", VolumeCategories.Category.Sfx)]
        [TestCase("UIHud", VolumeCategories.Category.Ui)]
        [TestCase("Panel_UI", VolumeCategories.Category.Ui)]
        [TestCase("HUDRoot", VolumeCategories.Category.Sfx)]
        [TestCase("RandomProp", VolumeCategories.Category.Sfx)]
        [TestCase("", VolumeCategories.Category.Sfx)]
        public void CategoryFromName_FollowsNamingConvention(string sourceName, VolumeCategories.Category expected)
        {
            Assert.AreEqual(expected, CategoryFromName(sourceName), "'" + sourceName + "'");
        }

        // The mixer bridge contract: exactly seven user params, all distinct,
        // none of them snapshot-owned (SetFloat on a snapshot-owned name would
        // freeze the snapshot choreography on that value forever).
        [Test]
        public void GroupToParameter_SevenUniqueUserParams_NoneSnapshotOwned()
        {
            string[] parameters = PrivateStatic<string[]>("GroupToParameter");
            Assert.AreEqual(7, parameters.Length);
            CollectionAssert.AllItemsAreUnique(parameters);
            foreach (string parameter in parameters)
            {
                CollectionAssert.DoesNotContain(AudioSnapshotController.SnapshotOwnedParams, parameter,
                    parameter + " is snapshot-owned and must never be SetFloat'ed");
            }
            CollectionAssert.Contains(parameters, "VoiceVol");
            CollectionAssert.Contains(parameters, "ShadowVol");
            Assert.AreNotEqual(parameters[5], parameters[6], "Voice and Shadow must stay separate buses");
        }

        // GroupNames feeds FindGroupExact; the exact-name pick is what stops
        // "Music" resolving onto the "MusicDuck" snapshot child. Voice and
        // Shadow must stay distinct groups at their persisted indices.
        [Test]
        public void GroupNames_AlignWithParameters_AndKeepVoiceShadowDistinct()
        {
            string[] groupNames = PrivateStatic<string[]>("GroupNames");
            string[] parameters = PrivateStatic<string[]>("GroupToParameter");
            Assert.AreEqual(parameters.Length, groupNames.Length, "the two tables must stay index-aligned");
            CollectionAssert.AllItemsAreUnique(groupNames);
            Assert.AreEqual("Voice", groupNames[5]);
            Assert.AreEqual("Shadow", groupNames[6]);
            Assert.AreEqual("VoiceVol", parameters[5]);
            Assert.AreEqual("ShadowVol", parameters[6]);
            for (int i = 0; i < groupNames.Length; i++)
            {
                Assert.IsTrue(parameters[i].StartsWith(groupNames[i], StringComparison.OrdinalIgnoreCase),
                    "parameter " + parameters[i] + " should be named after group " + groupNames[i]);
            }
        }
    }
}
