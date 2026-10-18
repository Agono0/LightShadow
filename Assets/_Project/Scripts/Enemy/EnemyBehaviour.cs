using LightShadow.Core;
using LightShadow.Gameplay;
using LightShadow.Player;
using UnityEngine;
using UnityEngine.AI;

namespace LightShadow.Enemy
{
    // Doc 4.3: The Shadow hunts in darkness, slows near light and cannot enter
    // lit rooms. This is a small hand-written finite state machine with exactly
    // four states. Every state has its own -> enter / update / exit section below.
    //
    //        Patrol  --player near in the dark-->  Stalk  --line of sight-->  Chase
    //           ^                                     |   <-- light returns -->
    //           +-------------------------------------+
    //        Any state --beam at close range with meter>30-->  Repelled --> Stalk
    //
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyBehaviour : MonoBehaviour
    {
        public enum State { Patrol, Stalk, Chase, Repelled }

        [Header("Wiring")]
        public GameConfig config;
        public NavMeshAgent agent;
        public EnemySight sight;
        public PatrolRoute route;
        public Transform player;
        public PlayerController playerController;
        public FlashlightBattery flashlight;
        public BrightIdeaMeter meter;
        public Camera playerCamera;
        public AudioSource breathingSource;
        public AudioSource oneShotSource;
        public AudioClip whisperClip;

        State state = State.Patrol;
        int waypointIndex;
        float sightLostTimer;
        float repelCooldownTimer;
        float hearTimer;
        bool aggressive;

        // ---- Config shortcuts with safe fallbacks ----
        float PatrolSpeed => config != null ? config.enemyPatrolSpeed : 1.5f;
        float StalkSpeed => config != null ? config.enemyStalkSpeed : 2f;
        float ChaseSpeed => config != null ? config.enemyChaseSpeed : 4f;
        float StalkRadius => config != null ? config.stalkRadius : 12f;
        float EngageRadius => aggressive && config != null ? config.dawnStalkRadius : StalkRadius;
        float SightRange => config != null ? config.sightRange : 15f;
        float ChaseExitDistance => config != null ? config.chaseExitDistance : 18f;
        float SightLostGrace => config != null ? config.sightLostGrace : 5f;
        float CatchDistance => config != null ? config.catchDistance : 1.2f;
        float RepelRecoil => config != null ? config.repelRecoil : 8f;
        float RepelCooldown => config != null ? config.repelCooldown : 6f;
        float RepelThreshold => config != null ? config.repelMeterThreshold : 30f;
        float StalkLightThreshold => config != null ? config.stalkLightThreshold : 0.3f;
        float SprintNoiseRange => config != null ? config.sprintNoiseRange : 8f;

        void Awake()
        {
            if (agent == null) agent = GetComponent<NavMeshAgent>();
        }

        void Start()
        {
            // Resolve references through the GameManager when they were not wired
            // manually, so a prefab dropped into the scene still works.
            GameManager gm = GameManager.Instance;
            if (gm != null)
            {
                if (config == null) config = gm.config;
                if (player == null && gm.player != null) player = gm.player;
            }
            if (player != null)
            {
                if (playerController == null) playerController = player.GetComponent<PlayerController>();
                if (flashlight == null) flashlight = player.GetComponentInChildren<FlashlightBattery>();
                if (meter == null) meter = player.GetComponentInChildren<BrightIdeaMeter>();
                if (playerCamera == null) playerCamera = player.GetComponentInChildren<Camera>();
            }
            if (sight != null && sight.target == null && playerCamera != null) sight.target = playerCamera;

            if (breathingSource != null)
            {
                breathingSource.loop = true;
                breathingSource.playOnAwake = false;
            }

            EnterState(State.Patrol);
        }

        void Update()
        {
            if (player == null || agent == null || !agent.isOnNavMesh) return;

            float dt = Time.deltaTime;
            if (repelCooldownTimer > 0f) repelCooldownTimer -= dt;

            Vector3 playerPosition = player.position;
            float distance = Vector3.Distance(transform.position, playerPosition);

            // How visible Nour is right now: standing in a lit room counts as fully lit.
            float effectiveLight = LitZone.PlayerInLitZone ? 1f : FlashlightBattery.PlayerLightLevel;
            bool sighted = distance <= SightRange && (sight == null || sight.HasLineOfSight());

            // Sweaty-palm hearing rule: sprinting is loud. The Shadow pauses and
            // turns toward the noise but does not magically know where Nour hides.
            if (playerController != null && playerController.IsSprinting && distance <= SprintNoiseRange) hearTimer = 1.2f;
            if (hearTimer > 0f) hearTimer -= dt;

            // Caught: close enough while unlit. The GameManager handles the fade,
            // the meter penalty and the checkpoint return (Doc 4.3).
            if (state != State.Repelled && distance <= CatchDistance && !LitZone.PlayerInLitZone)
            {
                if (GameManager.Instance != null) GameManager.Instance.CaughtPlayer();
                ResetToPatrol();
                return;
            }

            // Repel check runs from every state: a strong beam to the face pushes
            // the Shadow back without ever hurting the player (Doc 4.3).
            if (state != State.Repelled && repelCooldownTimer <= 0f && CanRepel(sighted))
            {
                EnterRepelled(playerPosition);
                return;
            }

            switch (state)
            {
                case State.Patrol: UpdatePatrol(dt, playerPosition, distance, effectiveLight, sighted); break;
                case State.Stalk: UpdateStalk(dt, playerPosition, distance, effectiveLight, sighted); break;
                case State.Chase: UpdateChase(dt, playerPosition, distance, sighted); break;
                case State.Repelled: UpdateRepelled(); break;
            }
        }

        // ------------------------- Patrol -------------------------
        // Walks the waypoint loop at a slow speed. Purely a presence in the dark;
        // it only starts hunting when Nour gets close while unlit.
        void UpdatePatrol(float dt, Vector3 playerPosition, float distance, float light, bool sighted)
        {
            if (hearTimer > 0f)
            {
                agent.isStopped = true;
                FaceTowards(playerPosition);
            }
            else
            {
                agent.isStopped = false;
                if (!agent.hasPath || agent.remainingDistance <= 0.7f) AdvanceWaypoint();
            }

            bool playerIsDark = light < StalkLightThreshold;
            if (distance <= EngageRadius && (playerIsDark || aggressive)) EnterState(State.Stalk);
        }

        // ------------------------- Stalk -------------------------
        // Creeps toward Nour in the darkness with an audible breath. Stops
        // three or four meters away and watches; that is the horror beat.
        // Returns to patrol the moment the light comes up (unless dawn chase).
        void UpdateStalk(float dt, Vector3 playerPosition, float distance, float light, bool sighted)
        {
            if (distance > 4f)
            {
                agent.isStopped = false;
                SetDestination(playerPosition);
            }
            else
            {
                agent.isStopped = true;
                FaceTowards(playerPosition);
            }

            if (sighted)
            {
                EnterState(State.Chase);
                return;
            }

            bool lightReturned = light >= StalkLightThreshold;
            bool tooFar = distance > EngageRadius * 1.6f;
            if (!aggressive && (lightReturned || tooFar)) EnterState(State.Patrol);
        }

        // ------------------------- Chase -------------------------
        // Full speed straight at the player's last known position. Ends when the
        // player out-runs it, breaks line of sight long enough, or repels it.
        void UpdateChase(float dt, Vector3 playerPosition, float distance, bool sighted)
        {
            if (sighted)
            {
                sightLostTimer = 0f;
                SetDestination(playerPosition);
            }
            else
            {
                sightLostTimer += dt;
            }

            bool escaped = distance > ChaseExitDistance;
            bool lost = sightLostTimer > SightLostGrace;
            if (escaped || lost)
            {
                // Still close by? Lurk. Far away? Give up and patrol.
                EnterState(distance <= EngageRadius ? State.Stalk : State.Patrol);
            }
        }

        // ------------------------- Repelled -------------------------
        // The beam shoves it away (a short recoil warp, never through walls),
        // it whispers, then it circles back toward the edge of the light.
        void UpdateRepelled()
        {
            if (repelCooldownTimer <= 0f) EnterState(State.Stalk);
        }

        // ------------------------- State plumbing -------------------------

        void EnterState(State next)
        {
            if (state == State.Stalk && next != State.Stalk) SetBreathing(false);

            state = next;
            switch (next)
            {
                case State.Patrol:
                    agent.isStopped = false;
                    agent.speed = PatrolSpeed;
                    if (route != null && route.Count > 0)
                    {
                        waypointIndex = route.ClosestIndex(transform.position);
                        AdvanceWaypoint();
                    }
                    break;
                case State.Stalk:
                    agent.speed = StalkSpeed;
                    SetBreathing(true);
                    break;
                case State.Chase:
                    agent.speed = ChaseSpeed;
                    agent.isStopped = false;
                    sightLostTimer = 0f;
                    break;
                case State.Repelled:
                    agent.isStopped = true;
                    agent.ResetPath();
                    break;
            }
        }

        void EnterRepelled(Vector3 playerPosition)
        {
            EnterState(State.Repelled);
            repelCooldownTimer = RepelCooldown;

            // Recoil away and land on the NavMesh so it never clips into geometry.
            Vector3 away = (transform.position - playerPosition);
            away.y = 0f;
            away = away.sqrMagnitude > 0.01f ? away.normalized : -transform.forward;
            Vector3 recoilTarget = transform.position + away * RepelRecoil;

            if (NavMesh.SamplePosition(recoilTarget, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }

            AudioSource burstSource = oneShotSource != null ? oneShotSource : breathingSource;
            if (burstSource != null && whisperClip != null) burstSource.PlayOneShot(whisperClip);
        }

        // Called by the GameManager after a catch, and used internally on exit.
        public void ResetToPatrol()
        {
            hearTimer = 0f;
            sightLostTimer = 0f;
            EnterState(State.Patrol);
        }

        // Doc Act III: when the gate is powered, dawn lights the corridors and
        // the Shadow stops caring about the dark - it stalks across lit ground.
        public void SetAggressive(bool value)
        {
            aggressive = value;
        }

        void AdvanceWaypoint()
        {
            if (route == null || route.Count == 0) return;
            waypointIndex = (waypointIndex + 1) % route.Count;
            Transform waypoint = route.Get(waypointIndex);
            if (waypoint != null) SetDestination(waypoint.position);
        }

        // All destinations are clamped out of lit zones (Doc 4.3: the Shadow
        // genuinely cannot enter lit rooms, not merely "avoids" them).
        void SetDestination(Vector3 position)
        {
            agent.SetDestination(LitZone.ClampPoint(position, 0.8f));
        }

        void FaceTowards(Vector3 position)
        {
            Vector3 direction = position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f) return;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 6f);
        }

        void SetBreathing(bool on)
        {
            if (breathingSource == null) return;
            if (on && !breathingSource.isPlaying) breathingSource.Play();
            if (!on) breathingSource.Stop();
        }

        bool CanRepel(bool sighted)
        {
            if (!sighted) return false;
            if (flashlight == null || !flashlight.IsOn) return false;
            if (meter == null || meter.Meter <= RepelThreshold) return false;
            if (playerCamera == null) return false;

            Vector3 toEnemy = (transform.position + Vector3.up * 1f) - playerCamera.transform.position;
            return Vector3.Angle(playerCamera.transform.forward, toEnemy) < 30f;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, StalkRadius);
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, SightRange);
        }
    }
}
