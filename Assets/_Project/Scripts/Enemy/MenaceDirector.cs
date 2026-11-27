using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Enemy
{
    // P3 menace director (plan 4.12): the hidden pacing gauge 0-100. It rises
    // while the Shadow sees Nour or lingers near her; at the cap it forces a
    // cool-down - the Shadow walks off to the farthest waypoint and stays gone
    // 45-60 s - and the gauge resets. Every chase end also arms a guaranteed
    // no-chase grace window, published as the static ChaseBlocked that
    // EnemyBehaviour consults before entering Chase (the LitZone static-access
    // pattern: no wiring needed for the FSM side).
    //
    // Lives on the school Systems object (SceneBuilder) and resolves its
    // references through GameManager, so direct Play of the school scene works
    // too. Without a director nothing enforces pacing - the game plays as it
    // did before P3.
    public class MenaceDirector : MonoBehaviour
    {
        public static MenaceDirector Instance { get; private set; }

        [Header("Wiring (resolved from GameManager when empty)")]
        public GameConfig config;
        public EnemyBehaviour enemy;
        public Transform player;

        public float Gauge { get; private set; }

        // True while the post-chase grace window is running: the FSM may not
        // enter Chase. Stalking stays allowed - the Shadow is calm, not blind.
        public static bool ChaseBlocked => Instance != null && Instance.chaseGraceTimer > 0f;

        float chaseGraceTimer;
        EnemyBehaviour.State previous = EnemyBehaviour.State.Patrol;

        // Config shortcuts with safe fallbacks (same pattern as EnemyBehaviour).
        float MenaceMax => config != null ? config.menaceMax : 100f;
        float MenaceReset => config != null ? config.menaceResetValue : 30f;
        float ChaseRise => config != null ? config.menaceChaseRisePerSecond : 10f;
        float SeenRise => config != null ? config.menaceSeenRisePerSecond : 5f;
        float NearRise => config != null ? config.menaceNearRisePerSecond : 2f;
        float Decay => config != null ? config.menaceDecayPerSecond : 1.5f;
        float NearDistance => config != null ? config.menaceNearDistance : 12f;
        float DawnMultiplier => config != null ? config.dawnMenaceMultiplier : 1.3f;
        float CooldownMin => config != null ? config.forcedCooldownMinSeconds : 45f;
        float CooldownMax => config != null ? config.forcedCooldownMaxSeconds : 60f;
        float GraceSeconds => config != null ? config.chaseGraceSeconds : 30f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void OnEnable()
        {
            GameEvents.EnemyStateChanged += OnEnemyStateChanged;
        }

        void OnDisable()
        {
            GameEvents.EnemyStateChanged -= OnEnemyStateChanged;
        }

        void ResolveReferences()
        {
            if (config == null && GameManager.Instance != null) config = GameManager.Instance.config;
            if (enemy == null && GameManager.Instance != null) enemy = GameManager.Instance.enemy;
            if (player == null && GameManager.Instance != null) player = GameManager.Instance.player;
        }

        // Chase ended -> arm the grace. Cool-down ended -> reset the gauge.
        void OnEnemyStateChanged(int value)
        {
            EnemyBehaviour.State next = (EnemyBehaviour.State)value;
            if (previous == EnemyBehaviour.State.Chase && next != EnemyBehaviour.State.Chase)
            {
                chaseGraceTimer = GraceSeconds;
            }
            if (previous == EnemyBehaviour.State.ForcedCooldown && next != EnemyBehaviour.State.ForcedCooldown)
            {
                Gauge = MenaceReset;
            }
            previous = next;
        }

        void Update()
        {
            ResolveReferences();

            if (chaseGraceTimer > 0f) chaseGraceTimer -= Time.deltaTime;
            if (enemy == null || player == null) return;
            if (enemy.CurrentState == EnemyBehaviour.State.ForcedCooldown) return; // parked until reset

            float rise = ComputeRise();
            Gauge = Mathf.Clamp(Gauge + rise * Time.deltaTime, 0f, MenaceMax);

            if (Gauge >= MenaceMax)
            {
                float seconds = Random.Range(CooldownMin, CooldownMax);
                enemy.ForceCooldown(seconds);
            }
        }

        // Rise = strongest applicable pressure: chase > seen > near, or a slow
        // decay when the Shadow is far and unseeing. Dawn hunts harder (the
        // enemy's aggressive flag mirrors Act III, plan 4.12). The sight read
        // goes through EnemySight's per-frame cache so the FSM poll and this
        // one share a single Linecast per frame.
        float ComputeRise()
        {
            float distance = Vector3.Distance(enemy.transform.position, player.position);
            bool seen = distance <= (enemy.sight != null ? enemy.sight.range : 15f)
                && enemy.sight != null && enemy.sight.HasLineOfSightCached();
            bool near = distance <= NearDistance;

            float rise;
            if (enemy.CurrentState == EnemyBehaviour.State.Chase) rise = ChaseRise;
            else if (seen) rise = SeenRise;
            else if (near) rise = NearRise;
            else rise = -Decay;

            return enemy.IsAggressive ? rise * DawnMultiplier : rise;
        }
    }
}
