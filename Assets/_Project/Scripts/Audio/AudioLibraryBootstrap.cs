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
            SfxRegistry.RegisterMany("SFX_ShadowMoan", library.shadowMoans);
            SfxRegistry.Register("AMB_SchoolNight", library.ambienceNight);
            Debug.Log("[LightShadow] Audio library registered (" + library.name + ").");
        }
    }
}
