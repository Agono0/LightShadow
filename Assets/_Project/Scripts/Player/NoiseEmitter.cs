using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Player
{
    // P3 noise bus (plan 4.12): publishes the player's noises on
    // GameEvents.NoiseEmitted with a hearing radius, and the Shadow's FSM
    // decides what to do about them. Two sources live here:
    //
    //   - Footsteps: distance-strided from the rig's real ground movement, so
    //     keyboard, touch and gamepad all feed the same cadence with no input
    //     plumbing. Sprinting is loud (10 m), walking carries 4 m, crouching
    //     whispers 1 m.
    //   - The wrong-answer buzzer: a wrong quiz answer (or a timeout) rings 6 m.
    //
    // Other systems (the crank minigame) call Emit() directly with their own
    // radius so their cadence stays theirs. Nothing here knows about the
    // Shadow; the event bus keeps the two sides decoupled.
    public class NoiseEmitter : MonoBehaviour
    {
        [Header("Wiring")]
        public GameConfig config;
        public PlayerController player;

        Vector3 lastPosition;
        float stepDistance;

        void Awake()
        {
            if (config == null && GameManager.Instance != null) config = GameManager.Instance.config;
            if (player == null) player = GetComponent<PlayerController>();
            lastPosition = transform.position;
        }

        void OnEnable()
        {
            GameEvents.QuestionAnswered += OnQuestionAnswered;
        }

        void OnDisable()
        {
            GameEvents.QuestionAnswered -= OnQuestionAnswered;
        }

        void Update()
        {
            // The input lock (dialogue/quiz/hiding) freezes movement, so the
            // feet stop with it. lastPosition keeps tracking while frozen so a
            // teleport during the lock (respawn, hide hop) never reads as a
            // giant step when the lock releases.
            if (player == null || PlayerController.ExternalInputLocked)
            {
                lastPosition = transform.position;
                stepDistance = Stride * 0.6f;
                return;
            }
            if (!player.HasMoveInput)
            {
                // Prime the clock so the first step after moving again lands
                // promptly instead of after a full silent stride.
                stepDistance = Stride * 0.6f;
                lastPosition = transform.position;
                return;
            }

            Vector3 delta = transform.position - lastPosition;
            delta.y = 0f;
            stepDistance += delta.magnitude;
            lastPosition = transform.position;
            if (stepDistance >= Stride)
            {
                stepDistance = 0f;
                Emit(StepRadius());
            }
        }

        float Stride => config != null ? Mathf.Max(0.2f, config.noiseStride) : 1.7f;

        float StepRadius()
        {
            if (config == null) return 4f;
            if (player != null && player.IsSprinting) return config.sprintNoiseRadius;
            if (player != null && player.IsCrouching) return config.crouchNoiseRange;
            return config.walkNoiseRange;
        }

        void OnQuestionAnswered(bool correct)
        {
            if (correct) return;
            Emit(config != null ? config.wrongAnswerNoiseRange : 6f);
        }

        // Public one-shot for other systems: pass the hearing radius in metres.
        // The Shadow hears it only if it is inside the radius when it fires.
        public void Emit(float radius)
        {
            GameEvents.RaiseNoiseEmitted(radius);
        }
    }
}
