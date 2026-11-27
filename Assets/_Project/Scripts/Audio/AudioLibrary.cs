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
        public AudioClip[] footstepCarpet;
        public AudioClip[] footstepGrass;

        [Header("World interactions (finish-the-game audio map)")]
        public AudioClip[] doorOpen;
        public AudioClip[] doorClose;
        public AudioClip[] paperRustle;
        public AudioClip[] clothRustle;
        public AudioClip[] glassShuffle;
        public AudioClip doorSteelBang;
        public AudioClip gateSlam;
        public AudioClip lockerOpen;
        public AudioClip lockerClose;
        public AudioClip fuseClunk;
        public AudioClip lockClick;
        public AudioClip puzzleSuccess;
        public AudioClip puzzleFail;
        public AudioClip pageFlutter;
        public AudioClip flashlightClick;
        public AudioClip chargeChime;
        public AudioClip chairFold;
        public AudioClip glassCrack;
        public AudioClip schoolBell;

        [Header("The Shadow (CC0 moans, qubodup)")]
        public AudioClip[] shadowMoans;

        [Header("Ambience")]
        public AudioClip ambienceNight;

        [Header("Music bed (Phase 1, Kevin MacLeod CC-BY)")]
        [Tooltip("Explore/Stalk tension bed layered under the procedural AdaptiveMusic pads.")]
        public AudioClip musicBedLongNoteOne;

        [Header("Ending cues (Phase 1, Kevin MacLeod CC-BY)")]
        [Tooltip("FullRecovery ending: warm, resolved (Frozen Star).")]
        public AudioClip endingHopeGold;
        [Tooltip("Dawnless ending: cold hollow drone (Anguish).")]
        public AudioClip endingDawnlessGrey;
    }
}
