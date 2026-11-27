using UnityEngine;

namespace LightShadow.Player
{
    // Movable objects (user brief 2026-11-27, item 18): Nour shoves crates
    // and light furniture by walking into them. The CharacterController does
    // not push rigidbodies on its own; this applies the shove on contact, so
    // heavy props feel heavy and scrape loudly enough to be a risk.
    public class PlayerPush : MonoBehaviour
    {
        public float pushForce = 2.2f;

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.rigidbody;
            if (body == null || body.isKinematic || body.mass > 80f) return;

            Vector3 direction = hit.moveDirection;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f) return;

            // Push from slightly below centre so crates slide, not topple.
            body.AddForceAtPosition(direction.normalized * pushForce * (10f / Mathf.Max(4f, body.mass)),
                hit.point - Vector3.up * 0.1f, ForceMode.Force);
        }
    }
}
