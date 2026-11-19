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
        public float stalkLightThreshold = 0.3f;

        // Deprecated (P3 banish rebalance): the old instant-repel tuning kept so
        // existing serialized GameConfig assets still load. The live banish
        // fields are in the Banish header below; nothing reads these any more.
        public float repelRecoil = 8f;
        public float repelCooldown = 6f;
        public float repelMeterThreshold = 30f;

        // Deprecated (P3 noise rebalance): the old sprint-hearing radius kept so
        // existing serialized GameConfig assets still load. The live hearing
        // radii are the noise fields below; nothing reads this any more.
        public float sprintNoiseRange = 8f;

        [Header("Banish (Plan 4.6, P3)")]
        // Hold-focused banish: keep the focused beam on the Shadow for
        // banishHoldSeconds with meter >= banishMeterThreshold and it recoils
        // and retreats for banishRetreatSeconds, costing banishMeterCost meter.
        // The focused beam also drains the battery focusBatteryDrainMultiplier
        // times faster (plan 4.6: wide 1/s, focused 4/s).
        public float banishHoldSeconds = 2f;
        public float banishMeterThreshold = 25f;
        public float banishRetreatSeconds = 20f;
        public float banishRecoilDistance = 8f;
        public float banishBeamAngle = 30f;
        public float banishReleaseDecay = 2f;
        public float banishMeterCost = 25f;
        public float focusBatteryDrainMultiplier = 4f;
        // Hold-vs-tap split for the on-screen LIGHT button: holds longer than
        // this focus the beam, shorter taps toggle it.
        public float flashlightTapSeconds = 0.25f;

        [Header("Calm FX (Plan 4.7, P3)")]
        // Vignette/heartbeat/tremor driven by the calm stat. Payloads arrive
        // normalized 0-1 (0 = worst calm); FX ramp up as calm drops below
        // calmFxFloor, peaking at 0. vignetteBaseline mirrors the night post
        // profile's vignette intensity (the override volume takes over the
        // parameter when it is present, so it repaints the baseline too).
        public float calmFxFloor = 0.45f;
        public float calmFxVignetteBaseline = 0.34f;
        public float calmFxVignetteMax = 0.45f;
        public float calmFxHeartbeatMinBpm = 55f;
        public float calmFxHeartbeatMaxBpm = 150f;
        public float calmFxHeartbeatGain = 0.55f;
        public float calmFxTremorMax = 0.004f;
        public float calmFxSmoothSpeed = 2f;

        [Header("Inspect View (P3)")]
        // 360-degree close-up for story items: the item spawns this far in
        // front of the camera, scaled, idling at inspectSpinSpeed; drag/swipe
        // orbits it at inspectDragDegreesPerPixel; the overlay auto-closes
        // after inspectAutoSeconds if the player never does.
        public float inspectItemDistance = 0.55f;
        public float inspectItemScale = 1.2f;
        public float inspectSpinSpeed = 20f;
        public float inspectDragDegreesPerPixel = 0.4f;
        public float inspectAutoSeconds = 12f;

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

        [Header("Menace Director (Plan 4.12, P3)")]
        // Pacing gauge 0-100: rises while the Shadow sees Nour or stays near;
        // at menaceMax it forces a 45-60 s cool-down (the Shadow leaves the
        // wing) and the gauge resets to menaceResetValue. chaseGraceSeconds is
        // the guaranteed no-chase window after every chase ends. The dawn
        // multiplier is plan 4.12's Act III aggression (+30%).
        public float menaceMax = 100f;
        public float menaceResetValue = 30f;
        public float menaceChaseRisePerSecond = 10f;
        public float menaceSeenRisePerSecond = 5f;
        public float menaceNearRisePerSecond = 2f;
        public float menaceDecayPerSecond = 1.5f;
        public float menaceNearDistance = 12f;
        public float dawnMenaceMultiplier = 1.3f;
        public float forcedCooldownMinSeconds = 45f;
        public float forcedCooldownMaxSeconds = 60f;
        public float chaseGraceSeconds = 30f;

        [Header("Hiding And Search (Plan 4.12, P3)")]
        // Hiding v1: the breath meter lasts breathHoldSeconds, recovers over
        // breathRecoverSeconds, and re-holding is allowed past
        // breathReholdFraction. Unheld breathing carries breathNoiseRange every
        // hiddenBreathIntervalSeconds while hidden; releasing the hold early
        // (or empty lungs) rings breathReleaseNoiseRange. Holding costs calm in
        // story-calm units (-1..1 scale; the plan's "drains Calm 2/s" assumes
        // the 0-100 scale, which the story stat does not use).
        public float breathHoldSeconds = 6f;
        public float breathRecoverSeconds = 4f;
        public float breathReholdFraction = 0.5f;
        public float breathNoiseRange = 3f;
        public float breathReleaseNoiseRange = 6f;
        public float hiddenBreathIntervalSeconds = 2f;
        public float breathCalmPerSecond = 0.05f;
        // Search: after a chase is lost or an investigate finds nothing, the
        // Shadow checks up to searchHideSpotsChecked random hide spots around
        // the last known point for searchSeconds; the spot Nour currently
        // occupies is excluded on the first pass within this radius.
        public float searchSeconds = 60f;
        public int searchHideSpotsChecked = 2;
        public float searchHideSpotExcludeRadius = 2f;
        public float searchSpotDwellSeconds = 2.5f;

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
