using UnityEngine;

namespace LightShadow.Core
{
    // Single tuning asset for the whole game. Every system reads its numbers
    // from here so designers can re-balance without touching code.
    // SceneBuilder creates one at Assets/_Project/Data/GameConfig.asset.
    [CreateAssetMenu(menuName = "LightShadow/Game Config", fileName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Bright Idea Meter (Doc 4.1)")]
        public float meterMax = 100f;
        public float meterStart = 60f;
        public float meterGainCorrect = 25f;
        public float meterPenaltyWrong = 10f;
        public float meterDrainPerSecond = 0.5f;
        public float lightRadiusMin = 4f;
        public float lightRadiusMax = 12f;
        public float lightIntensityMin = 1.5f;
        public float lightIntensityMax = 3.5f;

        [Header("Questions (Doc 4.1)")]
        public float questionTimer = 20f;
        public float feedbackSeconds = 3f;

        [Header("Flashlight And Battery (Doc 4.2, 4.5)")]
        public float batterySeconds = 300f;
        public float batteryPickupFraction = 0.5f;

        [Header("Player (Doc 4.5)")]
        public float walkSpeed = 3f;
        public float sprintSpeed = 5f;
        public float crouchSpeed = 1.5f;
        public float mouseSensitivity = 2f;
        public float interactRange = 2.5f;

        [Header("Enemy Behaviour (Doc 4.3)")]
        public float enemyPatrolSpeed = 1.5f;
        public float enemyStalkSpeed = 2f;
        public float enemyChaseSpeed = 4f;
        public float stalkRadius = 12f;
        public float sightRange = 15f;
        public float chaseExitDistance = 18f;
        public float sightLostGrace = 5f;
        public float catchDistance = 1.2f;
        public float repelRecoil = 8f;
        public float repelCooldown = 6f;
        public float repelMeterThreshold = 30f;
        public float stalkLightThreshold = 0.3f;
        public float sprintNoiseRange = 8f;

        [Header("Penalties (Doc 4.3)")]
        public float caughtMeterPenalty = 15f;

        [Header("Dawn Chase (Doc 3.7)")]
        public float dawnStalkRadius = 20f;

        [Header("Audio Mixer (D15 follow-up)")]
        // Snapshot transition times in seconds (AGENT_REPORT_AUDIO_MIXER §4.3):
        // the mixer asset owns the duck/low-pass VALUES, these own the SPEEDS.
        public float snapshotTransitionExplore = 0.75f;
        public float snapshotTransitionStalk = 0.5f;
        public float snapshotTransitionChase = 0.15f;
        public float snapshotTransitionRelease = 0.6f;
        public float snapshotTransitionHide = 0.8f;
        public float snapshotTransitionHideRelease = 0.5f;
        public float snapshotTransitionMemoryFlash = 0.35f;
        public float snapshotTransitionMemoryFlashRelease = 1.2f;
        public float snapshotTransitionDawn = 2.5f;
        public float memoryFlashSeconds = 6f;
    }
}
