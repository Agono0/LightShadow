using LightShadow.Core;
using LightShadow.Gameplay;
using LightShadow.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightShadow.Audio
{
    // Samples the world a few times a second, feeds DangerModel, and publishes
    // one smoothed danger number plus a gain per sound family. Everything that
    // makes noise by itself (ambience, scare events, footsteps, the wraith's
    // voice, breathing) multiplies its own volume by these gains, so the game
    // is calm and quiet until the wraith is close and aware.
    public class AudioDirector : MonoBehaviour
    {
        public static float Danger { get; private set; }
        public static DangerMix Mix { get; private set; } = DangerModel.Mix(0f);

        public static float SfxGain => Mix.Sfx;

        static AudioDirector instance;
        const float SampleSeconds = 0.1f;
        float sampleClock;
        float target;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Hook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            ResetState();
            EnsureFor(SceneManager.GetActiveScene());
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // A new scene always starts calm: no danger carries over from the
            // previous run (run-2 audio leak, audit 4.1).
            ResetState();
            EnsureFor(scene);
        }

        static void ResetState()
        {
            Danger = 0f;
            Mix = DangerModel.Mix(0f);
        }

        static void EnsureFor(Scene scene)
        {
            if (scene.name != SceneNames.School || instance != null) return;
            instance = new GameObject("AudioDirector").AddComponent<AudioDirector>();
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
            ResetState();
        }

        void Update()
        {
            sampleClock -= Time.unscaledDeltaTime;
            if (sampleClock <= 0f)
            {
                sampleClock = SampleSeconds;
                target = Sample();
            }

            Danger = DangerModel.Smooth(Danger, target, Time.unscaledDeltaTime);
            Mix = DangerModel.Mix(Danger);
        }

        static float Sample()
        {
            GameManager game = GameManager.Instance;
            if (game == null || game.player == null || game.enemy == null || !game.enemy.isActiveAndEnabled)
            {
                return 0f;
            }

            Enemy.EnemyBehaviour enemy = game.enemy;
            PlayerHealth health = PlayerHealth.Instance;
            float light = LitZone.PlayerInLitZone ? 1f : FlashlightBattery.PlayerLightLevel;

            return DangerModel.Raw(new DangerInputs
            {
                Distance = Vector3.Distance(enemy.transform.position, game.player.position),
                State = (DangerState)(int)enemy.CurrentState,
                InSight = enemy.sight != null && enemy.sight.HasLineOfSightCached(),
                PlayerLight = light,
                HitsRemaining = health != null ? health.HitsRemaining : PlayerHealth.MaxHits,
                Hidden = HidingSystem.IsHidden,
            });
        }
    }
}
