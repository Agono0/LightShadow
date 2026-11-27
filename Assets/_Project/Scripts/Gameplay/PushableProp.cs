using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // A heavy movable prop (crate, desk): PlayerPush shoves it, and a hard
    // scrape or impact makes a positional noise the wraith can hear - moving
    // furniture to block a path is a gamble, not a free wall (user brief
    // 2026-11-27, item 18).
    public class PushableProp : MonoBehaviour
    {
        public float noiseRadius = 9f;
        public float minImpact = 1.2f;
        public float noiseCooldown = 0.8f;

        float lastNoiseTime = -99f;

        void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.magnitude < minImpact) return;
            if (Time.time - lastNoiseTime < noiseCooldown) return;
            lastNoiseTime = Time.time;

            Enemy.EnemyBehaviour enemy = GameManager.Instance != null ? GameManager.Instance.enemy : null;
            if (enemy != null) enemy.HearNoiseAt(transform.position, noiseRadius);

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(ProceduralSfx.Click); // placeholder scrape
        }
    }
}
