using System.Collections;
using UnityEngine;

namespace LightShadow.Enemy
{
    // Body language for the Shadow: a slow hover, a breathing scale pulse and
    // swaying tendrils. The lunge is called by EnemyBehaviour right before the
    // catch, so the scare has motion instead of being a teleport into a fade.
    //
    // Everything is applied on top of the local transforms authored in the
    // prefab, so the build stays the source of truth and nothing drifts.
    public class ShadowMotion : MonoBehaviour
    {
        [Header("Wiring")]
        public Transform visual;
        public Transform leftArm;
        public Transform rightArm;

        [Header("Idle motion")]
        public float bobHeight = 0.045f;
        public float bobSpeed = 0.85f;
        public float swayDegrees = 5f;
        public float swaySpeed = 0.65f;
        public float pulseAmount = 0.022f;
        public float pulseSpeed = 1.25f;

        Vector3 visualBasePosition;
        Vector3 visualBaseScale;
        Quaternion leftBaseRotation;
        Quaternion rightBaseRotation;
        float phase;
        float lungeOffset;
        Coroutine lungeRoutine;

        void Awake()
        {
            Cache();
            phase = Random.value * 10f;
        }

        void OnDisable()
        {
            lungeOffset = 0f;
            lungeRoutine = null;
        }

        void Cache()
        {
            if (visual == null) visual = transform.Find("Visual");
            if (visual == null) return;

            visualBasePosition = visual.localPosition;
            visualBaseScale = visual.localScale;
            if (leftArm == null) leftArm = visual.Find("ArmL");
            if (rightArm == null) rightArm = visual.Find("ArmR");
            if (leftArm != null) leftBaseRotation = leftArm.localRotation;
            if (rightArm != null) rightBaseRotation = rightArm.localRotation;
        }

        void Update()
        {
            if (visual == null) return;

            float t = Time.time + phase;

            // Hovering: never sinks below the floor, so it reads as floating.
            float hover = (0.5f + 0.5f * Mathf.Sin(t * bobSpeed)) * bobHeight;
            visual.localPosition = visualBasePosition + Vector3.up * hover + Vector3.forward * lungeOffset;

            // Breathing: a very small scale pulse keeps the silhouette alive.
            float pulse = Mathf.Sin(t * pulseSpeed) * pulseAmount;
            visual.localScale = visualBaseScale * (1f + pulse);

            float sway = Mathf.Sin(t * swaySpeed) * swayDegrees;
            if (leftArm != null) leftArm.localRotation = leftBaseRotation * Quaternion.Euler(0f, 0f, sway);
            if (rightArm != null) rightArm.localRotation = rightBaseRotation * Quaternion.Euler(0f, 0f, -sway);
        }

        // Short forward thrust toward the player, used by the catch sequence.
        public void Lunge(float distance = 0.5f, float duration = 0.34f)
        {
            if (!isActiveAndEnabled) return;
            if (lungeRoutine != null) StopCoroutine(lungeRoutine);
            lungeRoutine = StartCoroutine(LungeRoutine(distance, Mathf.Max(0.05f, duration)));
        }

        IEnumerator LungeRoutine(float distance, float duration)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                lungeOffset = Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI) * distance;
                yield return null;
            }

            lungeOffset = 0f;
            lungeRoutine = null;
        }
    }
}
