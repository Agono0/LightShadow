using LightShadow.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightShadow.Audio
{
    // Sparse random scare beats in the school: a building creak or distant
    // breaking glass somewhere around Nour every 25-70 s, plus a frightened
    // breath when the wraith starts a chase. Created at runtime when the
    // School scene loads (no scene edit needed). Every clip comes from the
    // registry; with no library loaded it does nothing.
    public class AmbientHorrorEvents : MonoBehaviour
    {
        const int ChaseState = (int)Enemy.EnemyBehaviour.State.Chase;

        float nextEventTime;
        float nextBreathTime;
        static AmbientHorrorEvents instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Hook()
        {
            SceneManager.sceneLoaded += (scene, mode) => EnsureFor(scene);
            EnsureFor(SceneManager.GetActiveScene());
        }

        static void EnsureFor(Scene scene)
        {
            if (scene.name != SceneNames.School || instance != null) return;
            var go = new GameObject("AmbientHorrorEvents");
            instance = go.AddComponent<AmbientHorrorEvents>();
        }

        void OnEnable()
        {
            GameEvents.EnemyStateChanged += OnEnemyState;
            ScheduleNext(60f);
        }

        void OnDisable()
        {
            GameEvents.EnemyStateChanged -= OnEnemyState;
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        void ScheduleNext(float minDelay)
        {
            // Calm rooms get long silences; danger shortens the gaps.
            nextEventTime = Time.time + Mathf.Max(minDelay, DangerModel.ScareDelaySeconds(AudioDirector.Danger, Random.value));
        }

        void OnEnemyState(int state)
        {
            if (state != ChaseState || Time.time < nextBreathTime) return;
            nextBreathTime = Time.time + 20f;
            Sfx.Play("SFX_Breath_Scared", null, 0.8f * AudioDirector.Mix.Breathing);
        }

        void Update()
        {
            if (Time.time < nextEventTime) return;
            Transform player = GameManager.Instance != null ? GameManager.Instance.player : null;
            if (player == null) { nextEventTime = Time.time + 5f; return; }

            // 70% creak, 30% distant glass; 6-12 m away, behind her half the time.
            Vector3 dir = Random.insideUnitSphere;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) dir = -player.forward;
            dir.Normalize();
            if (Random.value < 0.5f && Vector3.Dot(dir, player.forward) > 0f) dir = -dir;
            Vector3 pos = player.position + dir * Random.Range(6f, 12f);

            Sfx.PlayAt(Random.value < 0.7f ? "SFX_BuildingCreak" : "SFX_WindBrokenGlass", pos, null, AudioDirector.Mix.ScareEvents);
            ScheduleNext(40f);
        }
    }
}
