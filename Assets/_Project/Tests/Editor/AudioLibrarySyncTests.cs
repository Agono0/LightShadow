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
        public void RedesignFamilies_MapToTheirFields()
        {
            Assert.AreEqual("keyPickup", AudioLibrarySync.FieldForId("SFX_KeyPickup"));
            Assert.AreEqual("doorUnlock", AudioLibrarySync.FieldForId("SFX_DoorUnlock"));
            Assert.AreEqual("doorLockedRattle", AudioLibrarySync.FieldForId("SFX_DoorLockedRattle"));
            Assert.AreEqual("medkitUse", AudioLibrarySync.FieldForId("SFX_MedkitUse"));
            Assert.AreEqual("hurtHit", AudioLibrarySync.FieldForId("SFX_HurtHit"));
            Assert.AreEqual("hurtHeavy", AudioLibrarySync.FieldForId("SFX_HurtHeavy"));
            Assert.AreEqual("respawn", AudioLibrarySync.FieldForId("SFX_Respawn"));
            Assert.AreEqual("throwWhoosh", AudioLibrarySync.FieldForId("SFX_ThrowWhoosh"));
            Assert.AreEqual("gateCrank", AudioLibrarySync.FieldForId("SFX_GateCrank"));
            Assert.AreEqual("uiPaperTap", AudioLibrarySync.FieldForId("SFX_UIPaperTap"));
            Assert.AreEqual("throwImpactSoft", AudioLibrarySync.FieldForId("SFX_ThrowImpactSoft"));
            Assert.AreEqual("throwImpactHard", AudioLibrarySync.FieldForId("SFX_ThrowImpactHard"));
            Assert.AreEqual("crateScrape", AudioLibrarySync.FieldForId("SFX_CrateScrape"));
            Assert.AreEqual("wraithRoam", AudioLibrarySync.FieldForId("SFX_WraithRoam"));
            Assert.AreEqual("wraithAmbushStinger", AudioLibrarySync.FieldForId("SFX_WraithAmbushStinger"));
            Assert.AreEqual("wraithBreathNear", AudioLibrarySync.FieldForId("SFX_WraithBreathNear"));
            Assert.AreEqual("ambienceYard", AudioLibrarySync.FieldForId("AMB_SchoolyardNight"));
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
