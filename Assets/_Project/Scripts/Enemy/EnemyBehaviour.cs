using System.Collections.Generic;
using LightShadow.Core;
using LightShadow.Gameplay;
using LightShadow.Player;
using UnityEngine;
using UnityEngine.AI;

namespace LightShadow.Enemy
{
    // Doc 4.3: The Shadow hunts in darkness, slows near light and cannot enter
    // lit rooms. This is a small hand-written finite state machine. Every state
    // has its own -> enter / update / exit section below.
    //
    //        Patrol  --player near in the dark-->  Stalk  --line of sight-->  Chase
    //           ^                                     |   <-- light returns -->
    //           +-------------------------------------+
    //  Patrol/Stalk --fresh noise in range-->  Investigate  --found nothing-->  Search
    //        Chase lost ---------------------->  Search (checks 1-2 hide spots)
    //        Any state --focused beam held 2 s, meter>=25-->  Repelled (20 s) --> Stalk
    //        Gauge 100 (MenaceDirector) -->  ForcedCooldown  --timer done-->  Patrol
    //
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyBehaviour : MonoBehaviour
    {
        public enum State { Patrol, Stalk, Chase, Repelled, Investigate, ForcedCooldown, Search, Ambush }

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
        public ShadowMotion shadowMotion;
        public CameraShake cameraShake;

        State state = State.Patrol;
        int waypointIndex;
        // Dynamic roaming (user brief 2026-11-27): the wraith wanders the
        // whole school at random instead of looping a scripted route. The
        // recent list stops it ping-ponging between two corners.
        readonly System.Collections.Generic.List<int> recentWaypoints = new System.Collections.Generic.List<int>();
        // Ambush (same brief: "can hide to scare you"): from Patrol it slips
        // into a dark corner on Nour's path and waits, quiet, until she is on
        // top of it, then bursts out.
        float ambushCooldown = 20f;
        float ambushTimer;
        float sightLostTimer;
        float repelCooldownTimer;
        float catchCooldownTimer;
        bool aggressive;
        bool catching;
        float catchingSeconds;

        // Hearing (P3 noise bus): the last noise the player made and how loud
        // it was. OnNoiseEmitted fills these; the FSM reacts in Update.
        Vector3 lastNoisePosition;
        float lastNoiseTime = -99f;
        float lastNoiseRadius;
        float investigateTimer;

        // Forced cool-down (P3 MenaceDirector): the Shadow leaves the wing.
        Vector3 forcedCooldownTarget;
        float forcedCooldownTimer;
        float forcedCooldownSeconds;

        // Search (P3 hiding): after a lost chase or a fruitless investigate the
        // Shadow checks a few random hide spots around the last known point.
        readonly List<Vector3> searchPath = new List<Vector3>();
        int searchStageIndex;
        float searchTimer;
        float searchDwell;

        // Banish (P3, plan 4.6): the focused beam must STAY on the Shadow for
        // banishHoldSeconds before it recoils into the 20 s retreat. No instant
        // repel any more - the timer decays fast when the beam wanders off.
        float banishHoldTimer;

        public State CurrentState => state;
        public bool IsAggressive => aggressive;

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
        float BanishHoldSeconds => config != null ? config.banishHoldSeconds : 2f;
        float BanishThreshold => config != null ? config.banishMeterThreshold : 25f;
        float BanishRetreatSeconds => config != null ? config.banishRetreatSeconds : 20f;
        float BanishRecoilDistance => config != null ? config.banishRecoilDistance : 8f;
        float BanishBeamAngle => config != null ? config.banishBeamAngle : 30f;
        float BanishReleaseDecay => config != null ? config.banishReleaseDecay : 2f;
        float StalkLightThreshold => config != null ? config.stalkLightThreshold : 0.3f;
        float InvestigateNoiseMaxAge => config != null ? config.investigateNoiseMaxAge : 3f;
        float InvestigateSearchSeconds => config != null ? config.investigateSearchSeconds : 8f;
        float InvestigateArriveDistance => config != null ? config.investigateArriveDistance : 0.9f;
        float InvestigateSweepDegrees => config != null ? config.investigateSweepDegrees : 60f;
        float InvestigateSweepSpeed => config != null ? config.investigateSweepSpeed : 1.5f;
        float SearchSeconds => config != null ? config.searchSeconds : 60f;
        int SearchHideSpotsChecked => config != null ? config.searchHideSpotsChecked : 2;
        float SearchExcludeRadius => config != null ? config.searchHideSpotExcludeRadius : 2f;
        float SearchSpotDwellSeconds => config != null ? config.searchSpotDwellSeconds : 2.5f;

        void Awake()
        {
            if (agent == null) agent = GetComponent<NavMeshAgent>();
        }

        void OnEnable()
        {
            GameEvents.NoiseEmitted += OnNoiseEmitted;
        }

        void OnDisable()
        {
            GameEvents.NoiseEmitted -= OnNoiseEmitted;
        }

        // P3 noise bus: remember where the noise happened and how far it
        // carries. Whether the Shadow reacts at all is decided in Update (the
        // noise must still be fresh AND inside the radius when checked).
        // Positional noise (user brief 2026-11-27: distraction throws). The
        // player-driven noise bus reports at the player's feet; a thrown prop
        // lands somewhere else entirely, and the wraith goes THERE.
        public void HearNoiseAt(Vector3 position, float radius)
        {
            lastNoisePosition = position;
            lastNoiseTime = Time.time;
            lastNoiseRadius = radius;
            if (state == State.Investigate) investigateTimer = 0f;
        }

        void OnNoiseEmitted(float radius)
        {
            lastNoisePosition = player != null ? player.position : transform.position;
            lastNoiseTime = Time.time;
            lastNoiseRadius = radius;
            // Already on the way to a noise? A newer one re-targets the search.
            if (state == State.Investigate) investigateTimer = 0f;
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
            if (shadowMotion == null) shadowMotion = GetComponent<ShadowMotion>();
            if (cameraShake == null && playerCamera != null) cameraShake = playerCamera.GetComponent<CameraShake>();
            if (sight != null && sight.target == null && playerCamera != null) sight.target = playerCamera.transform;
            // E3: the config is the one source of truth for sight range;
            // HidingSystem and MenaceDirector read sight.range.
            if (sight != null) sight.range = ResolveSightRange(config != null, config != null ? config.sightRange : 0f, sight.range);

            if (breathingSource != null)
            {
                breathingSource.loop = true;
                breathingSource.playOnAwake = false;
            }

            // The prefab keeps the agent disabled so it cannot complain before the
            // NavMesh is registered. The bootstrap runs first (execution order
            // -1000), so by now the baked data is available.
            if (agent != null && !agent.enabled) agent.enabled = true;

            // If the bake is missing or the prefab was dropped off the mesh,
            // snap onto the nearest walkable point before the state machine runs.
            if (agent != null && !agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 8f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
                else
                {
                    Debug.LogWarning("LightShadow: the Shadow is not on a NavMesh; enemy behaviour disabled.");
                    enabled = false;
                    return;
                }
            }

            EnterState(State.Patrol);
        }

        void Update()
        {
            if (player == null || agent == null || !agent.isOnNavMesh) return;
            if (catching)
            {
                // Watchdog: the catch must always hand back. If the sequence or
                // the checkpoint wake never clears it, force the wraith to
                // withdraw so it can never freeze in place for good.
                catchingSeconds += Time.deltaTime;
                if (CatchRecovery.WatchdogExpired(catchingSeconds))
                {
                    Debug.LogWarning("[LightShadow] Catch watchdog fired: forcing the wraith to withdraw.");
                    StopAllCoroutines();
                    ApplyCatchPlan(CatchRecovery.AfterStrike(true));
                }
                return;
            }

            float dt = Time.deltaTime;
            if (repelCooldownTimer > 0f) repelCooldownTimer -= dt;
            if (catchCooldownTimer > 0f) catchCooldownTimer -= dt;
            if (ambushCooldown > 0f) ambushCooldown -= dt;

            Vector3 playerPosition = player.position;
            float distance = Vector3.Distance(transform.position, playerPosition);

            // How visible Nour is right now: standing in a lit room counts as fully lit.
            float effectiveLight = LitZone.PlayerInLitZone ? 1f : FlashlightBattery.PlayerLightLevel;
            bool sighted = distance <= SightRange && (sight == null || sight.HasLineOfSightCached());

            // Sweaty-palm hearing (P3): a fresh noise inside its radius pulls
            // the Shadow off patrol/stalk (or interrupts a search) to
            // Investigate. It walks to where the sound came from and searches -
            // it does not magically know where Nour is standing now.
            if (!sighted && (state == State.Patrol || state == State.Stalk || state == State.Search)
                && FreshNoiseHeard())
            {
                EnterState(State.Investigate);
                return;
            }

            // Caught: close enough while unlit. The GameManager handles the fade,
            // the meter penalty and the checkpoint return (Doc 4.3). Exempt:
            // the forced cool-down (the Shadow is leaving the wing on purpose)
            // and a hidden Nour - the Shadow cannot grab through the spot wall;
            // Search still FINDS a witnessed hider through sight (UpdateSearch).
            if (state != State.Repelled && state != State.ForcedCooldown && catchCooldownTimer <= 0f
                && distance <= CatchDistance && !LitZoneProtects(aggressive, LitZone.PlayerInLitZone)
                && !HidingSystem.IsHidden)
            {
                // No cinematic close-up here any more: it swapped the camera
                // while input stayed live and was part of the first-hit
                // freeze report. The shake in CatchSequence carries the hit.
                StartCoroutine(CatchSequence());
                return;
            }

            // Banish check runs from every state (P3, plan 4.6): holding the
            // focused beam on the Shadow builds toward the recoil+retreat. The
            // timer decays fast when the beam wanders off; no instant repel.
            if (state != State.Repelled && state != State.ForcedCooldown)
            {
                bool focusing = FocusedBeamOnShadow(sighted) && meter != null && meter.Meter >= BanishThreshold;
                if (focusing)
                {
                    banishHoldTimer += dt;
                    if (banishHoldTimer >= BanishHoldSeconds)
                    {
                        EnterBanished(playerPosition);
                        return;
                    }
                }
                else
                {
                    banishHoldTimer = Mathf.Max(0f, banishHoldTimer - dt * BanishReleaseDecay);
                }
            }

            switch (state)
            {
                case State.Patrol: UpdatePatrol(dt, playerPosition, distance, effectiveLight, sighted); break;
                case State.Stalk: UpdateStalk(dt, playerPosition, distance, effectiveLight, sighted); break;
                case State.Chase: UpdateChase(dt, playerPosition, distance, sighted); break;
                case State.Repelled: UpdateRepelled(); break;
                case State.Investigate: UpdateInvestigate(dt, playerPosition, distance, effectiveLight, sighted); break;
                case State.ForcedCooldown: UpdateForcedCooldown(dt); break;
                case State.Ambush: UpdateAmbush(dt, distance, effectiveLight); break;
                case State.Search: UpdateSearch(dt, playerPosition, sighted); break;
            }
        }

        // P3 MenaceDirector pacing: a public one-way valve. At gauge 100 the
        // director parks the Shadow on the farthest waypoint for 45-60 s; no
        // catches, no chases, no stalk while it is leaving. The gauge resets
        // when the director sees ForcedCooldown exit.
        public void ForceCooldown(float seconds)
        {
            if (catching || !CanForceCooldown(aggressive)) return;
            forcedCooldownSeconds = Mathf.Max(1f, seconds);
            EnterState(State.ForcedCooldown);
        }

        // A noise is actionable when it is young enough and close enough: the
        // Shadow must be standing inside the noise's hearing radius to have
        // "heard" it.
        bool FreshNoiseHeard()
        {
            if (Time.time - lastNoiseTime > InvestigateNoiseMaxAge) return false;
            return Vector3.Distance(transform.position, lastNoisePosition) <= lastNoiseRadius;
        }

        // ------------------------- Patrol -------------------------
        // Walks the waypoint loop at a slow speed. Purely a presence in the dark;
        // it only starts hunting when Nour gets close while unlit. Noises are
        // handled by the Investigate state, not by stopping mid-step any more.
        void UpdatePatrol(float dt, Vector3 playerPosition, float distance, float light, bool sighted)
        {
            agent.isStopped = false;
            if (!agent.hasPath || agent.remainingDistance <= 0.7f) AdvanceWaypoint();

            bool playerIsDark = light < StalkLightThreshold;
            if (distance <= EngageRadius && (playerIsDark || aggressive)) EnterState(State.Stalk);

            // Ambush: Nour is near but has not engaged yet, and the wraith
            // slips into cover on her path. Only in the dark, never right
            // after the last scare.
            if (playerIsDark && ambushCooldown <= 0f && distance <= 14f && distance > EngageRadius)
            {
                EnterState(State.Ambush);
            }
        }

        // ------------------------- Investigate -------------------------
        // The noise bus pulled it here (P3, replaces the old stop-and-face
        // hack): walk to where the sound came from, sweep a look-around for a
        // few seconds, then give up and resume the patrol. A direct sighting
        // still escalates straight to the chase; a hidden Nour in the dark
        // nearby reads exactly like any other patrol stalk engagement.
        void UpdateInvestigate(float dt, Vector3 playerPosition, float distance, float light, bool sighted)
        {
            if (!agent.hasPath || agent.remainingDistance <= InvestigateArriveDistance)
            {
                // Arrived: stand still and sweep the gaze across the noise spot.
                agent.isStopped = true;
                investigateTimer += dt;

                Vector3 noiseDirection = lastNoisePosition - transform.position;
                noiseDirection.y = 0f;
                float baseAngle = noiseDirection.sqrMagnitude > 0.01f
                    ? Quaternion.LookRotation(noiseDirection).eulerAngles.y
                    : transform.eulerAngles.y;

                float sway = Mathf.Sin(investigateTimer * InvestigateSweepSpeed) * InvestigateSweepDegrees;
                Quaternion target = Quaternion.Euler(0f, baseAngle + sway, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 4f);
            }
            else
            {
                agent.isStopped = false;
                SetDestination(lastNoisePosition);
            }

            if (sighted && !MenaceDirector.ChaseBlocked)
            {
                EnterState(State.Chase);
                return;
            }

            // The player close by in the dark is still worth stalking, exactly
            // as from Patrol.
            if (distance <= EngageRadius && light < StalkLightThreshold)
            {
                EnterState(State.Stalk);
                return;
            }

            if (investigateTimer >= InvestigateSearchSeconds) BeginSearch(lastNoisePosition);
        }

        // ------------------------- Search -------------------------
        // Plan 4.12: after a lost chase (or a fruitless investigate) the Shadow
        // checks up to searchHideSpotsChecked random hide spots around the last
        // known point - never the one Nour currently occupies on the first
        // pass. Sighting still escalates; running out of time or spots ends
        // back at Patrol.
        void BeginSearch(Vector3 lastKnown)
        {
            searchTimer = 0f;
            searchDwell = 0f;
            searchPath.Clear();
            searchPath.Add(lastKnown);

            // Candidate spots: exclude the one Nour is occupying right now
            // (first pass never checks the occupied spot - plan 4.12).
            List<Vector3> candidates = new List<Vector3>();
            for (int i = 0; i < HideSpot.All.Count; i++)
            {
                HideSpot spot = HideSpot.All[i];
                if (spot == null) continue;
                if (HidingSystem.IsHidden && player != null
                    && Vector3.Distance(spot.transform.position, player.position) <= SearchExcludeRadius)
                {
                    continue;
                }
                candidates.Add(spot.transform.position);
            }

            // Sample distinct random candidates in random order.
            int wanted = Mathf.Max(0, SearchHideSpotsChecked);
            while (searchPath.Count < wanted + 1 && candidates.Count > 0)
            {
                int index = Random.Range(0, candidates.Count);
                searchPath.Add(candidates[index]);
                candidates.RemoveAt(index);
            }

            EnterState(State.Search);
        }

        // Dawn chase (Doc 4.4, story: "the Shadow is right behind you"): once the
        // gate is powered the lit rooms stop protecting Nour, the entrance
        // included, so the run to the gate is a real finale. Before dawn the lit
        // rooms stay safe (destinations clamped, no catch inside).
        public static bool ClampsToLitZones(bool dawnChase) { return !dawnChase; }
        public static bool LitZoneProtects(bool dawnChase, bool playerInLitZone) { return playerInLitZone && !dawnChase; }

        // N1: in the dawn chase the menace gauge climbs fast, and parking the
        // Shadow for 45-60s right after the gate powers would kill the finale.
        public static bool CanForceCooldown(bool dawnChase) { return !dawnChase; }

        // E3 seam: config wins over the EnemySight field when there is one.
        public static float ResolveSightRange(bool hasConfig, float configRange, float fieldRange)
        {
            return hasConfig && configRange > 0f ? configRange : fieldRange;
        }

        // E1 seam: a hidden target counts as sight-lost for Chase, even when
        // the Shadow watched her duck in (witnessed hiders do not blind it).
        public static bool ChaseSightLost(bool sighted, bool hidden)
        {
            return !sighted || hidden;
        }

        public static bool ShouldExitChase(bool escaped, float sightLostTimer, float grace)
        {
            return escaped || sightLostTimer > grace;
        }

        // E1 seam: a search stop on the occupied spot forces the extraction
        // only for a hider the Shadow watched enter. Unwitnessed stays safe.
        public static bool ShouldExtractHider(int searchStage, bool atStop, bool hidden, bool enteredUnderWatch)
        {
            return searchStage == 0 && atStop && hidden && enteredUnderWatch;
        }

        void UpdateSearch(float dt, Vector3 playerPosition, bool sighted)
        {
            searchTimer += dt;

            // A hider the Shadow can still "see" must not bounce Search back to
            // Chase: the stop at her spot decides it (E1).
            if (sighted && !HidingSystem.IsHidden && !MenaceDirector.ChaseBlocked)
            {
                EnterState(State.Chase);
                return;
            }

            if (searchStageIndex >= searchPath.Count)
            {
                // Every stop checked: give up.
                EnterState(State.Patrol);
                return;
            }

            Vector3 target = searchPath[searchStageIndex];
            if (!agent.hasPath || agent.remainingDistance <= InvestigateArriveDistance)
            {
                // Dwell at the stop and look it over, then move to the next.
                agent.isStopped = true;
                FaceTowards(searchStageIndex == 0 ? playerPosition : target);
                if (ShouldExtractHider(searchStageIndex, true, HidingSystem.IsHidden, HidingSystem.EnteredUnderWatch)
                    && !catching && catchCooldownTimer <= 0f && !LitZone.PlayerInLitZone)
                {
                    HidingSystem.ExitCurrent();
                    StartCoroutine(CatchSequence());
                    return;
                }
                searchDwell += dt;
                if (searchDwell >= SearchSpotDwellSeconds)
                {
                    searchDwell = 0f;
                    searchStageIndex++;
                }
            }
            else
            {
                agent.isStopped = false;
                SetDestination(target);
            }

            if (searchTimer >= SearchSeconds) EnterState(State.Patrol);
        }

        // ------------------------- ForcedCooldown -------------------------
        // The MenaceDirector pulled the gauge to 100: walk to the waypoint
        // farthest from Nour, then stand there until the cool-down ends. The
        // Update gates above already exempt this state from catching, chasing,
        // stalking and the beam repel - the Shadow is leaving, full stop.
        void UpdateForcedCooldown(float dt)
        {
            if (!agent.hasPath || agent.remainingDistance > InvestigateArriveDistance)
            {
                agent.isStopped = false;
                SetDestination(forcedCooldownTarget);
                return;
            }

            agent.isStopped = true;
            forcedCooldownTimer += dt;
            if (forcedCooldownTimer >= forcedCooldownSeconds) EnterState(State.Patrol);
        }

        void PickFarWaypoint()
        {
            forcedCooldownTarget = transform.position;
            if (route == null || route.Count == 0 || player == null) return;

            float best = -1f;
            for (int i = 0; i < route.Count; i++)
            {
                Transform waypoint = route.Get(i);
                if (waypoint == null) continue;
                float distance = Vector3.Distance(waypoint.position, player.position);
                if (distance > best)
                {
                    best = distance;
                    forcedCooldownTarget = waypoint.position;
                }
            }
        }

        // ------------------------- Stalk -------------------------
        // Creeps toward Nour in the darkness with an audible breath. Stops
        // three or four meters away and watches; that is the horror beat.
        // Returns to patrol the moment the light comes up (unless dawn chase).
        // Chase is gated by the MenaceDirector's post-chase grace window.
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

            if (sighted && !MenaceDirector.ChaseBlocked)
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
        // player out-runs it, breaks line of sight long enough, or repels it -
        // and a lost chase hands over to the hide-spot Search instead of
        // straight patrol (plan 4.12).
        void UpdateChase(float dt, Vector3 playerPosition, float distance, bool sighted)
        {
            if (!ChaseSightLost(sighted, HidingSystem.IsHidden))
            {
                sightLostTimer = 0f;
                SetDestination(playerPosition);
            }
            else
            {
                sightLostTimer += dt;
            }

            bool escaped = distance > ChaseExitDistance;
            if (ShouldExitChase(escaped, sightLostTimer, SightLostGrace))
            {
                BeginSearch(playerPosition);
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

        // Pure policy, no NavMesh or AudioSource needed, so tests can pin it.
        public static bool BreathingFor(State target)
        {
            return target == State.Stalk || target == State.Investigate || target == State.Search;
        }

        void EnterState(State next)
        {
            state = next;
            // One central rule for the breathing loop (policy: on while the
            // Shadow stalks, investigates or searches; off otherwise).
            SetBreathing(BreathingFor(next));
            // Audio snapshot driving (AudioSnapshotController) follows the FSM
            // through this single raise: every transition enters here.
            GameEvents.RaiseEnemyStateChanged((int)next);
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
                case State.Investigate:
                    // Creep toward the noise with a stalker's pace, not a jog.
                    agent.speed = StalkSpeed;
                    agent.isStopped = false;
                    investigateTimer = 0f;
                    break;
                case State.Ambush:
                    // Slip into cover: stop, go quiet, wait for her.
                    if (agent != null && agent.isOnNavMesh)
                    {
                        agent.isStopped = true;
                        agent.ResetPath();
                    }
                    ambushTimer = 18f;
                    ambushCooldown = 45f;
                    break;
                case State.ForcedCooldown:
                    // Leaving the wing: patrol pace, quiet, no target fixes
                    // while the timer runs.
                    agent.speed = PatrolSpeed;
                    agent.isStopped = false;
                    forcedCooldownTimer = 0f;
                    PickFarWaypoint();
                    break;
                case State.Search:
                    // Check the picked spots with a stalker's pace.
                    agent.speed = StalkSpeed;
                    agent.isStopped = false;
                    searchStageIndex = 0;
                    searchDwell = 0f;
                    break;
            }
        }

        // P3 banish (plan 4.6): the held focused beam wins - recoil warp onto
        // the NavMesh, 20 s retreat, and it costs meter. Never hurts anyone.
        void EnterBanished(Vector3 playerPosition)
        {
            EnterState(State.Repelled);
            repelCooldownTimer = BanishRetreatSeconds;
            banishHoldTimer = 0f;

            if (meter != null && config != null) meter.RemoveMeter(config.banishMeterCost);

            // Recoil away and land on the NavMesh so it never clips into geometry.
            Vector3 away = (transform.position - playerPosition);
            away.y = 0f;
            away = away.sqrMagnitude > 0.01f ? away.normalized : -transform.forward;
            Vector3 recoilTarget = transform.position + away * BanishRecoilDistance;

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
            catching = false;
            catchingSeconds = 0f;
            catchCooldownTimer = 1.5f;
            sightLostTimer = 0f;
            investigateTimer = 0f;
            forcedCooldownTimer = 0f;
            banishHoldTimer = 0f;
            searchStageIndex = 0;
            searchPath.Clear();
            EnterState(State.Patrol);
        }

        // The catch is a short piece of choreography: stop, face Nour, lunge, a
        // camera shake and a whisper, and only then hand over to the fade. The
        // state machine stays frozen (catching) until the checkpoint wakes her.
        System.Collections.IEnumerator CatchSequence()
        {
            catching = true;
            catchingSeconds = 0f;
            sightLostTimer = 0f;
            investigateTimer = 0f;
            forcedCooldownTimer = 0f;
            searchStageIndex = 0;
            searchPath.Clear();

            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            Vector3 direction = player.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(direction);

            if (shadowMotion != null) shadowMotion.Lunge(0.55f, 0.32f);
            if (cameraShake != null) cameraShake.Shake(0.4f, 0.085f);

            AudioSource burstSource = oneShotSource != null ? oneShotSource : breathingSource;
            if (burstSource != null && whisperClip != null) burstSource.PlayOneShot(whisperClip);

            SetBreathing(true);
            yield return new WaitForSeconds(0.38f);

            if (GameManager.Instance != null)
            {
                bool survived = GameManager.Instance.EnemyHitPlayer();
                ApplyCatchPlan(CatchRecovery.AfterStrike(survived));
            }
            else ResetToPatrol();
        }

        // Applies the pure CatchRecovery plan. A survived strike clears the
        // catching flag (the old code never did, which froze the wraith after
        // the first hit) and sends it away for a while; a lethal one stays
        // frozen until the checkpoint wake calls ResetToPatrol.
        void ApplyCatchPlan(CatchRecovery.Plan plan)
        {
            catchingSeconds = 0f;
            if (plan.ClearCatching) catching = false;
            catchCooldownTimer = Mathf.Max(catchCooldownTimer, plan.CatchCooldownSeconds);
            if (plan.Retreat)
            {
                ForceCooldown(plan.RetreatSeconds);
            }
            else if (plan.ResetToPatrol && !catching)
            {
                ResetToPatrol();
            }
        }

        // Doc Act III: when the gate is powered, dawn lights the corridors and
        // the Shadow stops caring about the dark - it stalks across lit ground.
        public void SetAggressive(bool value)
        {
            aggressive = value;
        }

        // ------------------------- Ambush -------------------------
        // Waiting in cover, quiet and still. When Nour walks into it the
        // wraith bursts out (lunge + whisper) and the normal stalk takes over;
        // if she never comes, it gives up and roams again.
        void UpdateAmbush(float dt, float distance, float light)
        {
            ambushTimer -= dt;
            if (ambushTimer <= 0f || light >= StalkLightThreshold)
            {
                EnterState(State.Patrol);
                return;
            }

            if (distance <= 4.5f)
            {
                if (shadowMotion != null) shadowMotion.Lunge(0.55f, 0.32f);
                if (cameraShake != null) cameraShake.Shake(0.3f, 0.06f);
                AudioSource burstSource = oneShotSource != null ? oneShotSource : breathingSource;
                if (burstSource != null && whisperClip != null) burstSource.PlayOneShot(whisperClip);
                EnterState(State.Stalk);
            }
        }

        // Dynamic roam (user brief 2026-11-27): pick a random waypoint the
        // wraith has not visited lately, so its wandering reads as intent,
        // not a patrol loop. Falls back to the next waypoint when the route
        // is tiny.
        void AdvanceWaypoint()
        {
            if (route == null || route.Count == 0) return;
            int next;
            if (route.Count <= 2)
            {
                next = (waypointIndex + 1) % route.Count;
            }
            else
            {
                next = waypointIndex;
                for (int tries = 0; tries < 12; tries++)
                {
                    int candidate = UnityEngine.Random.Range(0, route.Count);
                    if (candidate != waypointIndex && !recentWaypoints.Contains(candidate))
                    {
                        next = candidate;
                        break;
                    }
                }
                if (next == waypointIndex) next = (waypointIndex + 1) % route.Count;
            }
            waypointIndex = next;
            recentWaypoints.Add(next);
            if (recentWaypoints.Count > 4) recentWaypoints.RemoveAt(0);
            Transform waypoint = route.Get(waypointIndex);
            if (waypoint != null) SetDestination(waypoint.position);
        }

        // Before dawn all destinations are clamped out of lit zones (Doc 4.3: the Shadow
        // genuinely cannot enter lit rooms, not merely "avoids" them).
        void SetDestination(Vector3 position)
        {
            agent.SetDestination(ClampsToLitZones(aggressive) ? LitZone.ClampPoint(position, 0.8f) : position);
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

        // Is the FOCUSED beam actually pointing at the Shadow right now?
        // Replaces the old CanRepel instant check (P3 banish rebalance).
        bool FocusedBeamOnShadow(bool sighted)
        {
            if (!sighted) return false;
            if (flashlight == null || !flashlight.IsFocused) return false;
            if (playerCamera == null) return false;

            Vector3 toEnemy = (transform.position + Vector3.up * 1f) - playerCamera.transform.position;
            return Vector3.Angle(playerCamera.transform.forward, toEnemy) < BanishBeamAngle;
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
