using UnityEngine;

namespace LightShadow.Audio
{
    /// <summary>
    /// Named library of the real audio clips imported from the CC0 packs
    /// (ThirdParty). Built by AudioImportTool; registered into the runtime
    /// SfxRegistry by AudioLibraryBootstrap, which the scene builder attaches
    /// to the school's Systems object. Every consumer keeps its synthesised
    /// fallback, so a missing clip can never silence the game.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioLibrary", menuName = "LightShadow/Audio Library")]
    public class AudioLibrary : ScriptableObject
    {
        [Header("Interface / quiz")]
        public AudioClip quizCorrect;
        public AudioClip quizWrong;
        public AudioClip click;
        public AudioClip pickup;
        public AudioClip power;
        public AudioClip doorCreak;

        [Header("Player")]
        public AudioClip[] footstepConcrete;
        public AudioClip[] footstepWood;

        [Header("The Shadow (CC0 moans, qubodup)")]
        public AudioClip[] shadowMoans;

        [Header("Ambience")]
        public AudioClip ambienceNight;
    }
}
