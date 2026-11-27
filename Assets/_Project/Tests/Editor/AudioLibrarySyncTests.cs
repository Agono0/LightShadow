using LightShadow.Tools;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    // Guards the audio naming convention map: registry id -> AudioLibrary
    // field. A typo here silently drops bro's clips from builds.
    public class AudioLibrarySyncTests
    {
        [Test]
        public void FootstepFamilies_MapToTheirArrays()
        {
            Assert.AreEqual("footstepConcrete", AudioLibrarySync.FieldForId("SFX_FootstepConcrete"));
            Assert.AreEqual("footstepWood", AudioLibrarySync.FieldForId("SFX_FootstepWood"));
            Assert.AreEqual("footstepCarpet", AudioLibrarySync.FieldForId("SFX_FootstepCarpet"));
            Assert.AreEqual("footstepGrass", AudioLibrarySync.FieldForId("SFX_FootstepGrass"));
            Assert.AreEqual("footstepGravel", AudioLibrarySync.FieldForId("SFX_FootstepGravel"));
            Assert.AreEqual("footstepLeaves", AudioLibrarySync.FieldForId("SFX_FootstepLeaves"));
            Assert.AreEqual("footstepMud", AudioLibrarySync.FieldForId("SFX_FootstepMud"));
        }

        [Test]
        public void BreathSingle_MapsToItsField()
        {
            Assert.AreEqual("breathTired", AudioLibrarySync.FieldForId("SFX_Breath_Tired"));
        }

        [Test]
        public void UnknownIds_MapToNull()
        {
            Assert.IsNull(AudioLibrarySync.FieldForId("SFX_FootstepCloud"));
            Assert.IsNull(AudioLibrarySync.FieldForId(""));
            Assert.IsNull(AudioLibrarySync.FieldForId(null));
        }
    }
}
