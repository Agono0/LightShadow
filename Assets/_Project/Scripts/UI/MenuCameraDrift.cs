using UnityEngine;

namespace LightShadow.UI
{
    /// <summary>
    /// Slow breathing motion for the main-menu camera standing inside the 3D
    /// corridor diorama: a gentle position sway plus a barely-there yaw drift,
    /// like someone holding still but not frozen. Hand-rolled (no tween
    /// library) and deterministic in feel — this replaces the old static
    /// camera bolted ten units in front of a painted backdrop.
    /// </summary>
    public class MenuCameraDrift : MonoBehaviour
    {
        public float swayAmplitude = 0.05f;
        public float swayPeriod = 9f;
        public float yawDriftDegrees = 1.2f;
        public float yawPeriod = 17f;

        Vector3 basePosition;
        Quaternion baseRotation;
        float clock;

        void Start()
        {
            basePosition = transform.localPosition;
            baseRotation = transform.localRotation;
        }

        void Update()
        {
            clock += Time.unscaledDeltaTime;
            float sway = Mathf.Sin(clock * Mathf.PI * 2f / swayPeriod);
            float bob = Mathf.Sin(clock * Mathf.PI * 2f / (swayPeriod * 1.6f) + 1.3f);
            float yaw = Mathf.Sin(clock * Mathf.PI * 2f / yawPeriod) * yawDriftDegrees;

            transform.localPosition = basePosition
                + transform.right * (sway * swayAmplitude)
                + transform.up * (bob * swayAmplitude * 0.6f);
            transform.localRotation = baseRotation * Quaternion.Euler(0f, yaw, 0f);
        }
    }
}
