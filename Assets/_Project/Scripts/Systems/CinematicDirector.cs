using System;
using System.Collections;
using LightShadow.Core;
using LightShadow.Gameplay;
using UnityEngine;

namespace LightShadow.Systems
{
    // Scripted camera shots for the three big beats - catch close-up, dawn
    // corridor, ending gate (premium pass, decision D6). The gameplay camera
    // stays the existing first-person rig; this director only takes it over for
    // a few seconds, then hands it back exactly as it found it.
    //
    // Safety contract: NOTHING here may break the game. Every Cinemachine
    // touch goes through reflection (see CinematicRunner.Cine); if the package
    // is missing, an API shape changed, or a rig is absent, the shot logs one
    // warning and is skipped. Priority blending is CM3's own: raising the shot
    // camera's Priority makes the CinemachineBrain blend to it, lowering it
    // hands the camera back.
    public static class CinematicDirector
    {
        public const string ShotCatch = "catch";
        public const string ShotDawn = "dawn";
        public const string ShotGate = "gate";

        // Hold times per shot, in seconds. Short by design: these are accents
        // inside fades, not cutscenes.
        const float CatchHoldSeconds = 1.1f;
        const float DawnHoldSeconds = 2.2f;
        const float GateHoldSeconds = 1.6f;

        // Editor-built dormant rigs live under this root (see SceneBuilderSystems).
        public const string RigRootName = "CM_Cinematics";
        const string RigPrefix = "CM_Shot_";

        const int GameplayPriority = 0;
        const int ShotPriority = 100;

        static GameObject rigRoot;
        static Camera shotCamera;
        static bool cinematicPlaying;
        static bool warnedNoPackage;

        // True while a scripted shot owns the camera (UI may want to know).
        public static bool IsPlaying => cinematicPlaying;

        // ------------------------------------------------------------ shots --

        public static void PlayShot(string id)
        {
            if (cinematicPlaying) return; // one shot at a time; beats never overlap

            if (CinematicRunner.Instance == null)
            {
                new GameObject("CinematicRunner").AddComponent<CinematicRunner>();
            }
            if (CinematicRunner.Instance == null) return;
            CinematicRunner.Instance.Run(PlayShotRoutine(id));
        }

        static IEnumerator PlayShotRoutine(string id)
        {
            Camera main = Camera.main;
            if (main == null)
            {
                Debug.LogWarning("[LightShadow] Cinematic skipped: no main camera for shot '" + id + "'.");
                yield break;
            }

            Component shotCam = ResolveShotCamera(id, main);
            if (shotCam == null)
            {
                WarnOnce("Cinemachine unavailable - shot '" + id + "' skipped.");
                yield break;
            }

            cinematicPlaying = true;

            // The main camera is a CHILD of the camera pivot; while the brain
            // drives it the world-space writes land as corrupted local pose and
            // survive the shot (the reported "controls reversed / camera stuck"
            // after being caught). Snapshot the exact local pose and restore it
            // no matter how the handback goes.
            Transform mainTransform = main.transform;
            Vector3 savedLocalPosition = mainTransform.localPosition;
            Quaternion savedLocalRotation = mainTransform.localRotation;

            try
            {
                // Take over: position the shot for this exact moment, raise its
                // priority, and wake the brain on the gameplay camera.
                PositionShot(id, shotCam, mainTransform);
                CinematicRunner.Cine.SetPriority(shotCam, ShotPriority);
                EnableBrain(main);

                yield return new WaitForSeconds(HoldSecondsFor(id));
            }
            finally
            {
                // Hand back: drop the priority, put the rig back to sleep, then
                // switch the brain off so the first-person rig owns the camera
                // again. The brain is re-fetched by component rather than
                // trusting EnableBrain's return, so a partially-failed enable
                // can never leave the brain permanently enabled.
                CinematicRunner.Cine.SetPriority(shotCam, GameplayPriority);
                shotCam.gameObject.SetActive(false);
                DisableBrain(main);

                mainTransform.localPosition = savedLocalPosition;
                mainTransform.localRotation = savedLocalRotation;

                cinematicPlaying = false;
            }
        }

        // ------------------------------------------------------ rig lookup --

        // Prefers a dormant rig built by the editor tool; builds one at runtime
        // (through reflection) when that tool has not run yet.
        static Component ResolveShotCamera(string id, Camera main)
        {
            Type cameraType = CinematicRunner.Cine.Find(CinematicRunner.Cine.CameraType);
            if (cameraType == null) return null;

            EnsureRigRoot();

            string objectName = RigPrefix + id;
            Transform existing = rigRoot != null ? rigRoot.transform.Find(objectName) : null;

            GameObject go;
            if (existing != null)
            {
                go = existing.gameObject;
                go.SetActive(true);
            }
            else
            {
                go = new GameObject(objectName);
                go.transform.SetParent(rigRoot.transform, false);
                try
                {
                    go.AddComponent(cameraType);
                    Type composerType = CinematicRunner.Cine.Find(CinematicRunner.Cine.ComposerType);
                    if (composerType != null)
                    {
                        Component composer = go.AddComponent(composerType);
                        ApplyDamping(composer);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("[LightShadow] Cinematic rig '" + id + "' could not be created: " + exception.Message);
                    UnityEngine.Object.Destroy(go);
                    return null;
                }
            }

            // One brain per camera, created lazily; stays disabled between shots.
            Type brainType = CinematicRunner.Cine.Find(CinematicRunner.Cine.BrainType);
            Component brain = brainType != null ? main.GetComponent(brainType) : null;
            if (brain == null && brainType != null)
            {
                try
                {
                    brain = main.gameObject.AddComponent(brainType);
                    brain.GetType().GetProperty("enabled")?.SetValue(brain, false);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("[LightShadow] CinemachineBrain could not be added: " + exception.Message);
                }
            }

            CinematicRunner.Cine.SetPriority(go.GetComponent(cameraType), GameplayPriority);
            shotCamera = main;
            return go.GetComponent(cameraType);
        }

        static void ApplyDamping(Component composer)
        {
            try
            {
                var damping = composer.GetType().GetProperty("Damping");
                if (damping != null && damping.CanWrite)
                {
                    damping.SetValue(composer, new Vector3(0.6f, 0.6f, 0.4f));
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Composer damping not set: " + exception.Message);
            }
        }

        static void EnableBrain(Camera main)
        {
            Component brain = FindBrain(main);
            if (brain == null) return;
            try
            {
                brain.GetType().GetProperty("enabled")?.SetValue(brain, true);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] CinemachineBrain could not wake: " + exception.Message);
            }
        }

        // Looked up fresh for every handback: trusting EnableBrain's return
        // left the brain enabled forever when the wake half-way failed, which
        // pinned the camera to a stale shot pose (mouse-look dead).
        static void DisableBrain(Camera main)
        {
            Component brain = FindBrain(main);
            if (brain == null) return;
            try
            {
                brain.GetType().GetProperty("enabled")?.SetValue(brain, false);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Cinemachine brain re-disable failed: " + exception.Message);
            }
        }

        static Component FindBrain(Camera main)
        {
            Type brainType = CinematicRunner.Cine.Find(CinematicRunner.Cine.BrainType);
            if (brainType == null) return null;
            return main.GetComponent(brainType);
        }

        static void EnsureRigRoot()
        {
            if (rigRoot != null) return;
            GameObject existing = GameObject.Find(RigRootName);
            rigRoot = existing != null ? existing : new GameObject(RigRootName);
        }

        // ------------------------------------------------------- placement --

        static float HoldSecondsFor(string id)
        {
            switch (id)
            {
                case ShotCatch: return CatchHoldSeconds;
                case ShotDawn: return DawnHoldSeconds;
                case ShotGate: return GateHoldSeconds;
                default: return 1.5f;
            }
        }

        // Positions the shot camera for this exact beat. The editor rigs carry
        // the composer settings; the runtime moment decides where the camera
        // actually stands, since the catch/gate happen wherever the player is.
        static void PositionShot(string id, Component shotCam, Transform gameplayCamera)
        {
            Transform camTransform = shotCam.transform;
            Transform player = GameManager.Instance != null ? GameManager.Instance.player : null;
            Vector3 head = player != null ? player.position + Vector3.up * 1.55f : gameplayCamera.position;

            switch (id)
            {
                case ShotCatch:
                {
                    // Close-up from just past Nour's shoulder toward the Shadow.
                    Transform enemy = GameManager.Instance != null && GameManager.Instance.enemy != null
                        ? GameManager.Instance.enemy.transform
                        : null;
                    camTransform.position = head + gameplayCamera.forward * 1.6f + gameplayCamera.right * 0.9f;
                    camTransform.LookAt(enemy != null
                        ? Vector3.Lerp(head, enemy.position + Vector3.up * 1.2f, 0.55f)
                        : head + gameplayCamera.forward * 2f);
                    SetTargets(shotCam, player, enemy);
                    break;
                }
                case ShotDawn:
                {
                    // Low angle from behind, looking down the corridor the dawn
                    // lights are waking up in.
                    camTransform.position = head - gameplayCamera.forward * 2.6f + Vector3.up * 0.8f;
                    camTransform.LookAt(head + gameplayCamera.forward * 9f - Vector3.up * 0.4f);
                    SetTargets(shotCam, player, null);
                    break;
                }
                case ShotGate:
                {
                    // The gate framed from outside while the fade runs.
                    ExitGate gate = UnityEngine.Object.FindFirstObjectByType<ExitGate>();
                    Vector3 gateCenter = gate != null ? gate.transform.position : head + gameplayCamera.forward * 6f;
                    camTransform.position = gateCenter + Vector3.back * 4.2f + Vector3.up * 1.8f;
                    camTransform.LookAt(gateCenter + Vector3.up * 0.9f);
                    SetTargets(shotCam, null, gate != null ? gate.transform : null);
                    break;
                }
                default:
                    camTransform.position = head + gameplayCamera.forward * 2f;
                    camTransform.LookAt(head);
                    SetTargets(shotCam, player, null);
                    break;
            }
        }

        // Wires Follow/LookAt so the position composer's damping has real
        // targets during the shot (reflection; silently skipped if absent).
        static void SetTargets(Component shotCam, Transform follow, Transform lookAt)
        {
            try
            {
                var followProperty = shotCam.GetType().GetProperty("Follow");
                if (followProperty != null && followProperty.CanWrite) followProperty.SetValue(shotCam, follow);
                var lookAtProperty = shotCam.GetType().GetProperty("LookAt");
                if (lookAtProperty != null && lookAtProperty.CanWrite) lookAtProperty.SetValue(shotCam, lookAt);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Cinematic targets not set: " + exception.Message);
            }
        }

        static void WarnOnce(string message)
        {
            if (warnedNoPackage) return;
            warnedNoPackage = true;
            Debug.LogWarning("[LightShadow] " + message);
        }
    }
}
