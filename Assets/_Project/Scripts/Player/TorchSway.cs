using UnityEngine;

namespace LightShadow.Player
{
    // Torch sway (user brief 2026-11-27: "sway with walking, as realistic as
    // possible"). The beam lags the head: walking bobs it, turns drag it a
    // beat behind, strafing leans it, and everything settles on soft springs.
    // Mounted on the flashlight rig under the camera; the base pose the
    // builder set is the rest pose it returns to.
    public class TorchSway : MonoBehaviour
    {
        [Header("Tuning")]
        public float bobAmplitude = 0.022f;
        public float bobFrequency = 1.9f;
        public float strafeLeanDegrees = 2f;
        public float turnLagDegrees = 2.2f;

        Vector3 basePosition;
        Quaternion baseRotation;
        float bobPhase;
        Vector3 swayOffset;
        float yawLag;
        float pitchLag;
        float lastYaw;
        float lastPitch;
        CharacterController controller;

        void Start()
        {
            basePosition = transform.localPosition;
            baseRotation = transform.localRotation;
            // The flashlight sits under the camera; the CharacterController is
            // up the chain on the player root.
            controller = GetComponentInParent<CharacterController>();
            Vector3 angles = transform.parent != null ? transform.parent.eulerAngles : Vector3.zero;
            lastYaw = angles.y;
            lastPitch = angles.x;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return; // paused: hold the pose

            // Walk bob from actual ground speed, so standing still is still.
            Vector3 velocity = controller != null ? controller.velocity : Vector3.zero;
            Vector3 planar = new Vector3(velocity.x, 0f, velocity.z);
            float speed01 = Mathf.Clamp01(planar.magnitude / 3f);
            bobPhase += dt * bobFrequency * Mathf.PIf * 2f * Mathf.Clamp(planar.magnitude, 0f, 3f) / 3f;
            float bob = Mathf.Sin(bobPhase) * bobAmplitude * speed01;
            float sideSway = Mathf.Cos(bobPhase * 0.5f) * bobAmplitude * 0.6f * speed01;

            // Turn lag: the beam trails the look direction for a beat.
            Vector3 angles = transform.parent != null ? transform.parent.eulerAngles : Vector3.zero;
            float yawVelocity = Mathf.DeltaAngle(lastYaw, angles.y) / dt;
            float pitchVelocity = Mathf.DeltaAngle(lastPitch, angles.x) / dt;
            lastYaw = angles.y;
            lastPitch = angles.x;
            yawLag = Mathf.Lerp(yawLag, Mathf.Clamp(-yawVelocity * 0.02f, -turnLagDegrees, turnLagDegrees), dt * 6f);
            pitchLag = Mathf.Lerp(pitchLag, Mathf.Clamp(-pitchVelocity * 0.015f, -turnLagDegrees, turnLagDegrees), dt * 6f);

            // Strafe lean.
            Vector3 localVelocity = transform.parent != null ? transform.parent.InverseTransformDirection(planar) : planar;
            float lean = Mathf.Clamp(-localVelocity.x * strafeLeanDegrees * 0.2f, -strafeLeanDegrees, strafeLeanDegrees);

            Vector3 targetOffset = new Vector3(sideSway, bob, 0f);
            swayOffset = Vector3.Lerp(swayOffset, targetOffset, dt * 8f);
            transform.localPosition = basePosition + swayOffset;
            transform.localRotation = baseRotation * Quaternion.Euler(pitchLag, yawLag, lean);
        }
    }
}
