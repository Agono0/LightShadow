using LightShadow.Core;
using LightShadow.Gameplay;
using LightShadow.Story;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LightShadow.Player
{
    // P3 hiding v1 (plan 4.12): the enter/exit choreography and the hold-breath
    // mechanic for HideSpots. One component rides on the player rig (auto-added
    // on first use, so direct Play without the scene builder still works).
    //
    //   - Enter: Nour hops to the spot's hidePoint (CharacterController off for
    //     the move, same pattern as the checkpoint wake), movement input is
    //     frozen via ExternalInputLocked and the view becomes the peek.
    //   - Hidden and NOT holding breath: her breathing carries
    //     breathNoiseRange every hiddenBreathIntervalSeconds - the Shadow can
    //     Investigate it. Holding breath (Space / gamepad B / the touch HOLD
    //     BREATH button) silences it but drains the breath meter and Calm.
    //   - Releasing the hold early, or empty lungs, rings breathReleaseNoiseRange.
    //   - Exit: E (desktop), gamepad A, or the touch Interact button.
    //
    // Sight: EnemySight treats a hidden Nour as invisible unless the Shadow
    // watched her enter (EnteredUnderWatch).
    public class HidingSystem : MonoBehaviour
    {
        public static bool IsHidden { get; private set; }
        public static HideSpot CurrentSpot { get; private set; }

        // True when the Shadow had line of sight at the moment Nour ducked in:
        // hiding does not blind a witness.
        public static bool EnteredUnderWatch { get; private set; }

        [Header("Wiring (resolved when empty)")]
        public GameConfig config;
        public PlayerController player;
        public NoiseEmitter noise;

        static HidingSystem instance;

        CharacterController controller;
        Vector3 entryPosition;
        Quaternion entryRotation;

        // Breath meter: 1 = full lungs, 0 = empty (forced gasp).
        float breath = 1f;
        bool holdingBreath;
        bool gasping;
        bool touchBreathHeld;
        float breathNoiseClock;

        // Config shortcuts with safe fallbacks (same pattern as EnemyBehaviour).
        float BreathHoldSeconds => config != null ? config.breathHoldSeconds : 6f;
        float BreathRecoverSeconds => config != null ? config.breathRecoverSeconds : 4f;
        float BreathReholdFraction => config != null ? config.breathReholdFraction : 0.5f;
        float BreathNoiseRange => config != null ? config.breathNoiseRange : 3f;
        float BreathReleaseNoiseRange => config != null ? config.breathReleaseNoiseRange : 6f;
        float HiddenBreathInterval => config != null ? config.hiddenBreathIntervalSeconds : 2f;
        float BreathCalmPerSecond => config != null ? config.breathCalmPerSecond : 0.05f;

        void Awake()
        {
            if (instance != null && instance != this) return;
            instance = this;
            if (config == null && GameManager.Instance != null) config = GameManager.Instance.config;
            if (player == null) player = GetComponent<PlayerController>();
            if (noise == null) noise = GetComponent<NoiseEmitter>();
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        static HidingSystem Resolve()
        {
            if (instance != null) return instance;
            GameManager game = GameManager.Instance;
            if (game == null || game.player == null) return null;
            instance = game.player.GetComponent<HidingSystem>();
            if (instance == null) instance = game.player.gameObject.AddComponent<HidingSystem>();
            return instance;
        }

        // ------------------------------------------------- enter / exit ----

        public static void Enter(HideSpot spot)
        {
            HidingSystem system = Resolve();
            if (system == null || spot == null || IsHidden) return;
            system.EnterInternal(spot);
        }

        public static void ExitCurrent()
        {
            HidingSystem system = Resolve();
            if (system == null || !IsHidden) return;
            system.ExitInternal();
        }

        // Touch layer API: the HOLD BREATH button (TouchButton.Kind.BreathHold).
        public void SetBreathHold(bool held)
        {
            touchBreathHeld = held;
        }

        // Recovery-path reset (catch / checkpoint respawn): clears the hidden
        // state and releases the input lock WITHOUT the exit choreography -
        // no release noise and no hop back to the entry pose, because the
        // caller (RespawnReset / WakeAtCheckpoint) teleports Nour to the
        // checkpoint itself. Without this a catch interrupted mid-hide leaves
        // IsHidden stuck on: the Shadow goes permanently blind (EnemySight)
        // and the next E teleport hops her back into the old spot.
        public static void ResetState()
        {
            IsHidden = false;
            CurrentSpot = null;
            EnteredUnderWatch = false;
            if (instance != null)
            {
                instance.breath = 1f;
                instance.holdingBreath = false;
                instance.gasping = false;
                instance.touchBreathHeld = false;
                instance.breathNoiseClock = 0f;
            }
            PlayerController.ExternalInputLocked = false;
        }

        void EnterInternal(HideSpot spot)
        {
            if (controller == null && player != null) controller = player.GetComponent<CharacterController>();
            if (player == null || controller == null) return;

            // Witnessed entry: was the Shadow looking when Nour ducked in?
            Enemy.EnemyBehaviour enemy = GameManager.Instance != null ? GameManager.Instance.enemy : null;
            EnteredUnderWatch = false;
            if (enemy != null && enemy.sight != null)
            {
                float distance = Vector3.Distance(enemy.transform.position, player.transform.position);
                EnteredUnderWatch = distance <= enemy.sight.range && enemy.sight.HasLineOfSight();
            }

            entryPosition = player.transform.position;
            entryRotation = player.transform.rotation;

            Vector3 hidePosition = spot.hidePoint != null ? spot.hidePoint.position : spot.transform.position;
            Quaternion hideRotation = spot.hidePoint != null ? spot.hidePoint.rotation : spot.transform.rotation;

            // Same hop as the checkpoint wake: switch the capsule off for the
            // teleport so it does not fight the direct move.
            controller.enabled = false;
            player.transform.SetPositionAndRotation(hidePosition, hideRotation);
            controller.enabled = true;

            CurrentSpot = spot;
            IsHidden = true;
            PlayerController.ExternalInputLocked = true;

            breath = 1f;
            holdingBreath = false;
            gasping = false;
            touchBreathHeld = false;
            breathNoiseClock = 0f;
        }

        void ExitInternal()
        {
            if (!IsHidden || CurrentSpot == null || player == null) return;
            if (controller == null) controller = player.GetComponent<CharacterController>();

            // Leaving is not free: the door/rustle rings the release noise.
            GameEvents.RaiseNoiseEmitted(BreathReleaseNoiseRange);

            if (controller != null) controller.enabled = false;
            player.transform.SetPositionAndRotation(entryPosition, entryRotation);
            if (controller != null) controller.enabled = true;

            IsHidden = false;
            CurrentSpot = null;
            EnteredUnderWatch = false;
            holdingBreath = false;
            PlayerController.ExternalInputLocked = false;
        }

        // ------------------------------------------------------- update ----

        void Update()
        {
            if (!IsHidden) return;

            // Exit input: E on desktop, gamepad A, and the touch Interact
            // button (TouchButton routes hidden taps to ExitCurrent). The
            // pause menu stays available through its own button.
            if (!Application.isMobilePlatform && Input.GetKeyDown(KeyCode.E))
            {
                ExitInternal();
                return;
            }
            Gamepad pad = Gamepad.current;
            if (pad != null && pad.buttonSouth.wasPressedThisFrame)
            {
                ExitInternal();
                return;
            }

            UpdateBreath(pad);
        }

        void UpdateBreath(Gamepad pad)
        {
            bool wantHold = touchBreathHeld
                || (!Application.isMobilePlatform && Input.GetKey(KeyCode.Space))
                || (pad != null && pad.buttonEast.isPressed);

            bool holding = wantHold && !gasping && breath > 0f;

            if (holding)
            {
                breath -= Time.deltaTime / Mathf.Max(0.5f, BreathHoldSeconds);
                if (breath <= 0f)
                {
                    // Empty lungs: the gasp gives her away.
                    breath = 0f;
                    gasping = true;
                    holding = false;
                    GameEvents.RaiseNoiseEmitted(BreathReleaseNoiseRange);
                }
                else if (StoryDirector.Instance != null)
                {
                    // Holding your breath is itself tense: calm drains (story
                    // stat lives on the -1..1 scale, hence the small unit).
                    StoryDirector.Instance.AddCalm(-BreathCalmPerSecond * Time.deltaTime);
                }
            }
            else
            {
                breath = Mathf.Min(1f, breath + Time.deltaTime / Mathf.Max(0.5f, BreathRecoverSeconds));
                if (gasping && breath >= BreathReholdFraction) gasping = false;
            }

            // Unheld breathing carries while hidden - the reason to hold when
            // the Shadow is close (plan 4.12: within 3 m).
            if (holding)
            {
                breathNoiseClock = 0f;
            }
            else
            {
                breathNoiseClock += Time.deltaTime;
                if (breathNoiseClock >= HiddenBreathInterval)
                {
                    breathNoiseClock = 0f;
                    GameEvents.RaiseNoiseEmitted(BreathNoiseRange);
                }
            }
        }
    }
}
