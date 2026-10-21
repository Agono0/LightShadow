using UnityEngine;

namespace LightShadow.Player
{
    // Subtle first-person hand motion. The hand itself is built from primitives
    // under the camera by the scene builder; this only adds breathing, walking
    // bob and a small crouch dip so the model never feels pasted on.
    public class FirstPersonHands : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform handRoot;
        public PlayerController player;

        [Header("Motion")]
        public float walkBob = 0.012f;
        public float idleBob = 0.003f;
        public float walkFrequency = 8f;
        public float idleFrequency = 1.5f;

        Vector3 basePosition;
        Quaternion baseRotation;

        void Awake()
        {
            if (handRoot == null) handRoot = transform;
            if (player == null) player = GetComponentInParent<PlayerController>();
            basePosition = handRoot.localPosition;
            baseRotation = handRoot.localRotation;
        }

        void Update()
        {
            if (handRoot == null) return;

            bool moving = player != null && player.HasMoveInput;
            bool sprinting = player != null && player.IsSprinting;
            bool crouching = player != null && player.IsCrouching;

            float speed = sprinting ? 1.65f : 1f;
            float frequency = moving ? walkFrequency * speed : idleFrequency;
            float amount = moving ? walkBob : idleBob;

            float phase = Time.time * frequency;
            float vertical = Mathf.Sin(phase * 2f) * amount;
            float horizontal = Mathf.Cos(phase) * amount * 0.7f;

            Vector3 target = basePosition + new Vector3(horizontal, vertical, 0f);
            if (crouching) target.y -= 0.035f;

            handRoot.localPosition = Vector3.Lerp(handRoot.localPosition, target, Time.deltaTime * 10f);
            Quaternion sway = Quaternion.Euler(vertical * 220f, horizontal * 280f, horizontal * 140f);
            handRoot.localRotation = Quaternion.Slerp(handRoot.localRotation, baseRotation * sway,
                Time.deltaTime * 8f);
        }
    }
}
