using LightShadow.Audio;
using UnityEngine;

namespace LightShadow.Player
{
    /// <summary>
    /// Footstep audio for Nour (player request: the game was silent underfoot).
    /// Steps are timed from actual ground speed — walking, sprinting and
    /// crouching each get their own cadence — with a randomised variant and
    /// pitch so the loop never machines. Clips come from the imported CC0
    /// footstep sets (concrete classroom, wood corridor); when the library is
    /// absent the component stays silent rather than synthesising noise.
    /// Registered in the SFX volume category by the source's name.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class Footsteps : MonoBehaviour
    {
        [Header("Wiring")]
        public PlayerController player;

        [Header("Cadence (seconds per step, tuned in code — GameConfig holds locomotion)")]
        public float walkInterval = 0.52f;
        public float sprintInterval = 0.34f;
        public float crouchInterval = 0.78f;

        [Header("Mix")]
        [Range(0f, 1f)] public float walkVolume = 0.5f;
        [Range(0f, 1f)] public float sprintVolume = 0.75f;
        [Range(0f, 1f)] public float crouchVolume = 0.25f;
        public float pitchJitter = 0.08f;

        AudioSource source;
        float stepClock;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f; // the player IS the listener
            source.loop = false;
        }

        void Update()
        {
            int concreteCount = SfxRegistry.VariantCount("SFX_FootstepConcrete");
            int woodCount = SfxRegistry.VariantCount("SFX_FootstepWood");
            if (player == null || concreteCount == 0)
            {
                return;
            }

            // Only steps while actually moving on the ground; the input lock
            // (dialogue) freezes movement, so it also freezes the feet.
            if (PlayerController.ExternalInputLocked || !player.HasMoveInput)
            {
                stepClock = 0.6f * CurrentInterval(); // first step lands promptly after moving again
                return;
            }

            stepClock += Time.deltaTime;
            if (stepClock < CurrentInterval()) return;
            stepClock = 0f;
            PlayStep();
        }

        float CurrentInterval()
        {
            if (player.IsCrouching) return crouchInterval;
            return player.IsSprinting ? sprintInterval : walkInterval;
        }

        float CurrentVolume()
        {
            if (player.IsCrouching) return crouchVolume;
            return player.IsSprinting ? sprintVolume : walkVolume;
        }

        void PlayStep()
        {
            // Corridors and classrooms mix concrete and wood; the concrete set
            // is the base with an occasional wood plank step.
            AudioClip clip = Random.value < 0.75f || woodCount == 0
                ? SfxRegistry.ClipVariant("SFX_FootstepConcrete", concreteCount)
                : SfxRegistry.ClipVariant("SFX_FootstepWood", woodCount);
            if (clip == null) return;

            source.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            source.PlayOneShot(clip, CurrentVolume());
        }
    }
}
