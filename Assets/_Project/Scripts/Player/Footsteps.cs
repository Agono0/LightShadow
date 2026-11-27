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
            if (player == null || SfxRegistry.VariantCount("SFX_FootstepConcrete") == 0)
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
            // Surface-dependent (user brief 2026-11-27): raycast the floor
            // material. Grass (the garden) reads soft - the wood set at low
            // volume approximates it until a grass pack ships; concrete is the
            // base with an occasional wood plank step indoors. Variant counts
            // come from the registry, so growing a pack needs no script edit.
            string materialName = "";
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 0.5f))
            {
                Renderer floor = hit.collider.GetComponent<Renderer>();
                materialName = floor != null && floor.sharedMaterial != null ? floor.sharedMaterial.name : "";
            }

            // Surface selects the clip family by floor material name. The
            // palette grows Mat_Gravel / Mat_Leaves / Mat_Mud / Mat_Carpet as
            // those floors land; a family with no clips yet falls through to
            // the indoor concrete/wood mix, so a missing pack never silences.
            string family = null;
            float volumeScale = 1f;
            if (materialName.Contains("Grass")) { family = "SFX_FootstepGrass"; volumeScale = 0.85f; }
            else if (materialName.Contains("Gravel")) { family = "SFX_FootstepGravel"; volumeScale = 0.95f; }
            else if (materialName.Contains("Leaves")) { family = "SFX_FootstepLeaves"; volumeScale = 0.9f; }
            else if (materialName.Contains("Mud")) { family = "SFX_FootstepMud"; volumeScale = 0.8f; }
            else if (materialName.Contains("Carpet")) { family = "SFX_FootstepCarpet"; volumeScale = 0.7f; }

            AudioClip clip = null;
            if (family != null)
            {
                int familyCount = SfxRegistry.VariantCount(family);
                if (familyCount > 0) clip = SfxRegistry.ClipVariant(family, familyCount);
            }
            if (clip == null)
            {
                int concreteCount = SfxRegistry.VariantCount("SFX_FootstepConcrete");
                int woodCount = SfxRegistry.VariantCount("SFX_FootstepWood");
                bool softGround = materialName.Contains("Grass");
                if (softGround && woodCount > 0)
                {
                    clip = SfxRegistry.ClipVariant("SFX_FootstepWood", woodCount);
                    volumeScale = 0.55f; // soft ground swallows the step
                }
                else
                {
                    clip = Random.value < 0.75f || woodCount == 0
                        ? SfxRegistry.ClipVariant("SFX_FootstepConcrete", concreteCount)
                        : SfxRegistry.ClipVariant("SFX_FootstepWood", woodCount);
                    volumeScale = 1f;
                }
            }
            if (clip == null) return;

            source.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            source.PlayOneShot(clip, CurrentVolume() * volumeScale);
        }
    }
}
