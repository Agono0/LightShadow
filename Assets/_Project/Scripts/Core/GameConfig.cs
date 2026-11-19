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

        // Deprecated (P3 noise rebalance): the old sprint-hearing radius kept so
        // existing serialized GameConfig assets still load. The live hearing
        // radii are the noise fields below; nothing reads this any more.
        public float sprintNoiseRange = 8f;

        [Header("Noise And Hearing (Plan 4.12, P3)")]
        // Hearing radii in metres: how far each player noise carries to the
        // Shadow. Sprint is 10 m per plan 4.12 (the old sprintNoiseRange was 8).
        public float walkNoiseRange = 4f;
        public float sprintNoiseRadius = 10f;
        public float crouchNoiseRange = 1f;
        public float crankNoiseRange = 12f;
        public float wrongAnswerNoiseRange = 6f;
        // Ground distance per audible footstep (stride length).
        public float noiseStride = 1.7f;
        // Investigate: only react to noise younger than this; how long the
        // Shadow looks around at the noise before giving up; how close it must
        // land to consider the noise "reached"; and the sweep look-around shape.
        public float investigateNoiseMaxAge = 3f;
        public float investigateSearchSeconds = 8f;
        public float investigateArriveDistance = 0.9f;
        public float investigateSweepDegrees = 60f;
        public float investigateSweepSpeed = 1.5f;

        [Header("Crank Minigame (Plan 4.6, P3)")]
        // Emergency hand-crank: hold (F / gamepad RT / the touch CRANK button)
        // and turn in circles. Charge is a battery fraction per second of
        // actual turning; cranking never charges past crankMaxCharge percent.
        // Loud, never calming: each ratchet tick also emits crankNoiseRange.
        public float crankChargePerSecond = 0.02f;
        public float crankMaxCharge = 15f;
        public float crankRatchetSeconds = 0.45f;
        public float crankGestureResetSeconds = 0.35f;
        public float crankGestureDeadZone = 4f;

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
