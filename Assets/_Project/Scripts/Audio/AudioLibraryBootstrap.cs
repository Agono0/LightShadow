using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Audio
{
    /// <summary>
    /// Registers the imported AudioLibrary into the runtime SfxRegistry.
    /// Attached to the school's Systems object by the scene builder (the
    /// library asset is wired at build time). Lives for the scene lifetime —
    /// the registry is static, so the clips stay available across the
    /// bootstrap → menu → school flow once this runs.
    /// </summary>
    public class AudioLibraryBootstrap : MonoBehaviour
    {
        public AudioLibrary library;

        void Awake()
        {
            Register(library);
        }

        public static void Register(AudioLibrary library)
        {
            if (library == null) return;
            SfxRegistry.Register("SFX_QuizCorrect", library.quizCorrect);
            SfxRegistry.Register("SFX_QuizWrong", library.quizWrong);
            SfxRegistry.Register("SFX_Click", library.click);
            SfxRegistry.Register("SFX_Pickup", library.pickup);
            SfxRegistry.Register("SFX_Power", library.power);
            SfxRegistry.Register("SFX_DoorCreak", library.doorCreak);
            SfxRegistry.RegisterMany("SFX_FootstepConcrete", library.footstepConcrete);
            SfxRegistry.RegisterMany("SFX_FootstepWood", library.footstepWood);
            SfxRegistry.RegisterMany("SFX_FootstepCarpet", library.footstepCarpet);
            SfxRegistry.RegisterMany("SFX_FootstepGrass", library.footstepGrass);
            SfxRegistry.RegisterMany("SFX_FootstepGravel", library.footstepGravel);
            SfxRegistry.RegisterMany("SFX_FootstepLeaves", library.footstepLeaves);
            SfxRegistry.RegisterMany("SFX_FootstepMud", library.footstepMud);
            SfxRegistry.Register("SFX_Breath_Tired", library.breathTired);
            SfxRegistry.RegisterMany("SFX_DoorOpen", library.doorOpen);
            SfxRegistry.RegisterMany("SFX_KeyPickup", library.keyPickup);
            SfxRegistry.RegisterMany("SFX_DoorUnlock", library.doorUnlock);
            SfxRegistry.RegisterMany("SFX_DoorLockedRattle", library.doorLockedRattle);
            SfxRegistry.RegisterMany("SFX_MedkitUse", library.medkitUse);
            SfxRegistry.RegisterMany("SFX_HurtHit", library.hurtHit);
            SfxRegistry.RegisterMany("SFX_HurtHeavy", library.hurtHeavy);
            SfxRegistry.RegisterMany("SFX_Respawn", library.respawn);
            SfxRegistry.RegisterMany("SFX_ThrowWhoosh", library.throwWhoosh);
            SfxRegistry.RegisterMany("SFX_ThrowImpactSoft", library.throwImpactSoft);
            SfxRegistry.RegisterMany("SFX_ThrowImpactHard", library.throwImpactHard);
            SfxRegistry.RegisterMany("SFX_CrateScrape", library.crateScrape);
            SfxRegistry.RegisterMany("SFX_WraithRoam", library.wraithRoam);
            SfxRegistry.RegisterMany("SFX_WraithAmbushStinger", library.wraithAmbushStinger);
            SfxRegistry.RegisterMany("SFX_WraithBreathNear", library.wraithBreathNear);
            SfxRegistry.Register("AMB_SchoolyardNight", library.ambienceYard);
            SfxRegistry.RegisterMany("SFX_DoorClose", library.doorClose);
            SfxRegistry.RegisterMany("SFX_PaperRustle", library.paperRustle);
            SfxRegistry.RegisterMany("SFX_ClothRustle", library.clothRustle);
            SfxRegistry.RegisterMany("SFX_GlassShuffle", library.glassShuffle);
            SfxRegistry.Register("SFX_DoorSteelBang", library.doorSteelBang);
            SfxRegistry.Register("SFX_GateSlam", library.gateSlam);
            SfxRegistry.Register("SFX_LockerOpen", library.lockerOpen);
            SfxRegistry.Register("SFX_LockerClose", library.lockerClose);
            SfxRegistry.Register("SFX_FuseClunk", library.fuseClunk);
            SfxRegistry.Register("SFX_LockClick", library.lockClick);
            SfxRegistry.Register("SFX_PuzzleSuccess", library.puzzleSuccess);
            SfxRegistry.Register("SFX_PuzzleFail", library.puzzleFail);
            SfxRegistry.Register("SFX_PageFlutter", library.pageFlutter);
            SfxRegistry.Register("SFX_FlashlightClick", library.flashlightClick);
            SfxRegistry.Register("SFX_ChargeChime", library.chargeChime);
            SfxRegistry.Register("SFX_ChairFold", library.chairFold);
            SfxRegistry.Register("SFX_GlassCrack", library.glassCrack);
            SfxRegistry.Register("SFX_SchoolBell", library.schoolBell);
            SfxRegistry.RegisterMany("SFX_ShadowMoan", library.shadowMoans);
            SfxRegistry.Register("AMB_SchoolNight", library.ambienceNight);
            SfxRegistry.Register("MUS_MemoryTheme", library.memoryTheme);
            // Alias: the menu plays the memory theme (one asset, one register
            // row - Docs/AUDIO_CUES.md MUS_Menu). Register() skips null, so a
            // missing import leaves the menu silent instead of crashing.
            SfxRegistry.Register("MUS_Menu", library.memoryTheme);
            SfxRegistry.Register("MUS_Bed_LongNoteOne", library.musicBedLongNoteOne);
            SfxRegistry.Register("MUS_ChasePulse", library.chasePulse);
            SfxRegistry.Register("MUS_Ending_HopeAlt", library.endingHopeAlt);
            SfxRegistry.Register("AMB_DawnBirds", library.dawnBirds);
            SfxRegistry.Register("AMB_SchoolDreadA", library.schoolDreadA);
            SfxRegistry.Register("AMB_SchoolDreadB", library.schoolDreadB);
            SfxRegistry.Register("MUS_Ending_HopeGold", library.endingHopeGold);
            SfxRegistry.Register("MUS_Ending_DawnlessGrey", library.endingDawnlessGrey);
            Debug.Log("[LightShadow] Audio library registered (" + library.name + ").");
        }
    }
}
