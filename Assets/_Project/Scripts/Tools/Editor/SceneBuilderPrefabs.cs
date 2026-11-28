using System.Collections.Generic;
using LightShadow.Core;
using LightShadow.Enemy;
using LightShadow.Gameplay;
using LightShadow.Player;
using LightShadow.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace LightShadow.Tools
{
    public static partial class SceneBuilder
    {
        public sealed class Prefabs
        {
            public GameObject player;
            public GameObject enemy;
            public GameObject battery;
            public GameObject quizBoard;
            public GameObject fuseBox;
            public GameObject fragment;
            public GameObject gate;
        }

        static Prefabs BuildPrefabs(GameConfig config, Palette palette)
        {
            // Prefabs are authored in a throw-away scene so the user's open scene
            // is never dirtied by the build.
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var prefabs = new Prefabs
            {
                player = BuildPlayerPrefab(config, palette),
                enemy = BuildEnemyPrefab(config, palette),
                battery = BuildBatteryPrefab(palette),
                quizBoard = BuildQuizBoardPrefab(palette),
                fuseBox = BuildFuseBoxPrefab(palette),
                fragment = BuildFragmentPrefab(palette),
                gate = BuildGatePrefab(palette)
            };

            AssetDatabase.SaveAssets();
            return prefabs;
        }

        static GameObject SavePrefab(GameObject root, string folder, string fileName)
        {
            EnsureFolder(folder);
            string path = folder + "/" + fileName + ".prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        // ----------------------------------------------------------------- player

        static GameObject BuildPlayerPrefab(GameConfig config, Palette palette)
        {
            var root = new GameObject("Player");
            root.layer = PlayerLayer;
            root.tag = "Player";

            CharacterController controller = root.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.32f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.slopeLimit = 45f;
            controller.stepOffset = 0.35f;
            controller.skinWidth = 0.02f;
            controller.minMoveDistance = 0f;
            PlayerRigFit.ConfigureBody(root, controller.height, controller.radius);

            var pivot = new GameObject("CameraPivot");
            pivot.transform.SetParent(root.transform, false);
            pivot.transform.localPosition = new Vector3(0f, 1.6f, 0f);

            var cameraGo = new GameObject("Main Camera");
            cameraGo.transform.SetParent(pivot.transform, false);
            cameraGo.tag = "MainCamera";
            Camera camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.fieldOfView = 68f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 120f;
            camera.allowHDR = true;
            camera.allowMSAA = true;
            EnableCameraPostProcessing(camera);
            cameraGo.AddComponent<AudioListener>();

            // Catch feedback: a short decaying shake of the camera itself. The
            // pivot still owns pitch, so the two never fight over rotation.
            CameraShake shake = cameraGo.AddComponent<CameraShake>();
            shake.shakeRoot = cameraGo.transform;

            var flashlightGo = new GameObject("Flashlight");
            flashlightGo.transform.SetParent(cameraGo.transform, false);
            flashlightGo.transform.localPosition = new Vector3(0.12f, -0.08f, 0.18f);
            // Realistic sway with walking (user brief 2026-11-27).
            flashlightGo.AddComponent<TorchSway>();
            Light spot = flashlightGo.AddComponent<Light>();
            spot.type = LightType.Spot;
            // Owner's brief (2026-11-28): a circular, wide, warm beam with a large
            // radius. Amber tungsten colour, a wide cone with a broad soft rim;
            // range and intensity follow the meter (GameConfig 8..22 m, 8..20).
            spot.color = new Color(1f, 0.80f, 0.55f);
            spot.intensity = 8f;
            spot.range = 15f;
            spot.spotAngle = 100f;
            spot.innerSpotAngle = 55f;
            spot.shadows = LightShadows.Soft;
            spot.shadowStrength = 0.8f;
            spot.shadowBias = 0.04f;
            spot.shadowNormalBias = 0.2f;
            // The beam must not light the hand and torch it starts inside of:
            // at a few millimetres the inverse-square falloff burns the torch
            // head white (seen in the editor 2026-11-28).
            spot.cullingMask = ~(1 << PlayerLayer);

            // Round cookie: bright core, soft circular falloff and a faint outer
            // ring, so the pool reads as a torch circle (owner's brief). No haze
            // cone: the reference shows only the lit pool, and the old additive
            // cone drew a solid white wedge in front of the camera.
            spot.cookie = EnsureRoundTorchCookie();

            Material beamMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/_Project/Materials/Art/M_Art_Beam.mat");
            // Dust motes in the beam (user brief 2026-11-27): subtle, tiny,
            // drifting slowly up the cone, on mobile and PC alike. BeamDust
            // gates emission on the light so a dead torch shows no dust.
            BuildBeamDust(flashlightGo.transform, spot, beamMaterial);

            PlayerController player = root.AddComponent<PlayerController>();
            FlashlightBattery battery = root.AddComponent<FlashlightBattery>();
            // Two-hit health (user brief 2026-11-27): strikes drain it, the
            // checkpoint wake refills it, medkits are the only other heal.
            root.AddComponent<PlayerHealth>();
            BrightIdeaMeter meter = root.AddComponent<BrightIdeaMeter>();
            InteractionRaycaster raycaster = root.AddComponent<InteractionRaycaster>();

            player.config = config;
            player.cameraPivot = pivot.transform;
            player.playerCamera = camera;

            battery.config = config;
            battery.meter = meter;
            battery.spotLight = spot;
            battery.enabled = true;

            meter.config = config;
            meter.flashlight = battery;
            meter.spotLight = spot;

            raycaster.config = config;
            raycaster.rayCamera = camera;
            raycaster.interactMask = ~((1 << PlayerLayer) | (1 << EnemyLayer));

            // Real-audio pass: footsteps timed from the controller's ground
            // speed; silent when the CC0 footstep library has not shipped.
            Footsteps footsteps = root.AddComponent<Footsteps>();
            footsteps.player = player;

            // Labored breathing when winded or scared (user brief 2026-11-27).
            root.AddComponent<PlayerBreathing>();

            // Movable objects (same brief): walking shoves crates and light
            // furniture.
            root.AddComponent<PlayerPush>();

            // P3 horror systems: the noise bus (hearing radii), the crank
            // minigame and hiding v1 all ride the player rig.
            NoiseEmitter noise = root.AddComponent<NoiseEmitter>();
            noise.config = config;
            noise.player = player;

            FlashlightCrank crank = root.AddComponent<FlashlightCrank>();
            crank.config = config;
            crank.battery = battery;
            crank.noise = noise;

            HidingSystem hiding = root.AddComponent<HidingSystem>();
            hiding.config = config;
            hiding.player = player;
            hiding.noise = noise;

            // P3 calm FX: the vignette/heartbeat/tremor driver lives on the
            // camera itself (its own high-priority post volume override).
            cameraGo.AddComponent<CalmEffectsController>();

            // Premium pass §8.3: listener-side occlusion probe on the camera —
            // this is the object carrying the AudioListener, so its transform is
            // the ray origin. Idempotent attach (AttachAudioSnapshot pattern).
            LightShadow.Audio.AudioOcclusionProbe occlusion =
                cameraGo.GetComponent<LightShadow.Audio.AudioOcclusionProbe>();
            if (occlusion == null)
            {
                occlusion = cameraGo.AddComponent<LightShadow.Audio.AudioOcclusionProbe>();
            }
            occlusion.config = config;

            // Low-poly hand holding the flashlight under the camera. FirstPersonHands
            // adds breathing, walk bob and a crouch dip; the visuals carry no
            // colliders, and the interaction ray already excludes the player layer.
            GameObject handRootGo = new GameObject("Hand");
            handRootGo.transform.SetParent(cameraGo.transform, false);
            handRootGo.transform.localPosition = new Vector3(0.26f, -0.2f, 0.38f);
            handRootGo.transform.localRotation = Quaternion.Euler(6f, -14f, 4f);

            Box(handRootGo.transform, "Palm", new Vector3(0f, 0f, 0.05f),
                new Vector3(0.09f, 0.05f, 0.11f), palette.paper, false, PlayerLayer);
            Box(handRootGo.transform, "Thumb", new Vector3(-0.05f, 0.015f, 0.02f),
                new Vector3(0.035f, 0.04f, 0.05f), palette.paper, false, PlayerLayer);
            Box(handRootGo.transform, "Forearm", new Vector3(0f, -0.025f, 0.21f),
                new Vector3(0.075f, 0.07f, 0.18f), palette.paper, false, PlayerLayer);

            GameObject torch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            torch.name = "TorchBody";
            torch.layer = PlayerLayer;
            torch.transform.SetParent(handRootGo.transform, false);
            torch.transform.localPosition = new Vector3(0.01f, 0.035f, -0.04f);
            torch.transform.localRotation = Quaternion.Euler(90f, 0f, 6f);
            torch.transform.localScale = new Vector3(0.055f, 0.17f, 0.055f);
            MeshRenderer torchRenderer = torch.GetComponent<MeshRenderer>();
            if (torchRenderer != null) torchRenderer.sharedMaterial = palette.metal;
            Collider torchCollider = torch.GetComponent<Collider>();
            if (torchCollider != null) Object.DestroyImmediate(torchCollider);

            // P2 hero prop: the Blender crank flashlight replaces the primitive
            // tube at the same hand anchor (see FlashlightHandAnchor for the
            // tail-origin offset). The beam cone and cookie stay on the
            // Flashlight light object above and are untouched; without the
            // FBX the primitive keeps its renderer as the fallback.
            if (AddHeroModelChild(handRootGo, HeroModelFlashlight, 0f,
                    FlashlightHandRotation, FlashlightHandAnchor) != null
                && torchRenderer != null)
            {
                torchRenderer.enabled = false;
            }

            // Real torch rig (audit v2 S2-E): the beam now starts at the torch
            // LENS in the hand instead of floating off the camera. The hand
            // root is tilted for the pose, so the light is counter-rotated to
            // keep the beam aimed down the camera's view axis (aim and the
            // interaction ray stay honest); only its ORIGIN moves with the
            // hand, so walking bob and crouch dip carry the light too.
            flashlightGo.transform.SetParent(handRootGo.transform, false);
            flashlightGo.transform.localPosition = new Vector3(0.01f, 0.035f, 0.13f);
            flashlightGo.transform.localRotation = Quaternion.Inverse(handRootGo.transform.localRotation);

            ApplyPlayerRigModels(root, handRootGo, flashlightGo, torch, palette);

            FirstPersonHands hands = handRootGo.AddComponent<FirstPersonHands>();
            hands.handRoot = handRootGo.transform;
            hands.player = player;

            AudioSource ambience = root.AddComponent<AudioSource>();
            ambience.playOnAwake = false;
            ambience.loop = true;
            ambience.spatialBlend = 0f;
            ambience.volume = 0f;
            AmbientDrone drone = root.AddComponent<AmbientDrone>();
            drone.targetVolume = 0.14f;

            return SavePrefab(root, PrefabsFolder + "/Player", "Player");
        }

        // ------------------------------------------------------------------ enemy

        static GameObject BuildEnemyPrefab(GameConfig config, Palette palette)
        {
            var root = new GameObject("TheShadow");
            root.layer = EnemyLayer;

            NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
            agent.radius = 0.36f;
            agent.height = 1.8f;
            agent.baseOffset = 0f;
            agent.speed = 1.5f;
            agent.angularSpeed = 260f;
            agent.acceleration = 10f;
            agent.stoppingDistance = 0.2f;
            agent.autoBraking = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            agent.avoidancePriority = 50;
            // Kept disabled until EnemyBehaviour.Start, after the NavMesh is ready.
            agent.enabled = false;

            CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
            collider.height = 1.8f;
            collider.radius = 0.36f;
            collider.center = new Vector3(0f, 0.9f, 0f);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);

            AddShadowShape(visual.transform, "Body", PrimitiveType.Capsule, new Vector3(0f, 0.92f, 0f),
                new Vector3(0.42f, 0.9f, 0.34f), Quaternion.identity, palette.shadow);
            AddShadowShape(visual.transform, "Head", PrimitiveType.Sphere, new Vector3(0f, 1.78f, 0f),
                new Vector3(0.42f, 0.48f, 0.38f), Quaternion.identity, palette.shadow);
            AddShadowShape(visual.transform, "ArmL", PrimitiveType.Capsule, new Vector3(-0.3f, 1.12f, 0f),
                new Vector3(0.1f, 0.42f, 0.1f), Quaternion.Euler(0f, 0f, 16f), palette.shadow);
            AddShadowShape(visual.transform, "ArmR", PrimitiveType.Capsule, new Vector3(0.3f, 1.12f, 0f),
                new Vector3(0.1f, 0.42f, 0.1f), Quaternion.Euler(0f, 0f, -16f), palette.shadow);

            // Two embers where the face should be. Unlit on purpose: the eyes stay
            // visible when the body is a silhouette in the dark.
            AddShadowShape(visual.transform, "EyeL", PrimitiveType.Sphere, new Vector3(-0.095f, 1.86f, 0.168f),
                new Vector3(0.075f, 0.055f, 0.05f), Quaternion.identity, palette.eye);
            AddShadowShape(visual.transform, "EyeR", PrimitiveType.Sphere, new Vector3(0.095f, 1.86f, 0.168f),
                new Vector3(0.075f, 0.055f, 0.05f), Quaternion.identity, palette.eye);

            // Hover, breathe, sway and lunge. Purely additive on top of the
            // authored local transforms, so the prefab stays the source of truth.
            ShadowMotion motion = root.AddComponent<ShadowMotion>();
            motion.visual = visual.transform;
            motion.leftArm = visual.transform.Find("ArmL");
            motion.rightArm = visual.transform.Find("ArmR");

            // Premium pass: the art silhouette ships with the prefab itself so
            // a fresh Build Scenes never regresses to the greybox shapes (the
            // smoke shader mesh, not the enemy behaviour - the ember eyes stay
            // visible). ArtImporter's restyle does the same for older prefabs;
            // both are marker-idempotent on the "Shadow_Art" child name.
            GameObject shadowArtPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(ArtPrefabsFolder + "/Shadow_Art.prefab");
            if (shadowArtPrefab != null)
            {
                GameObject silhouette = (GameObject)PrefabUtility.InstantiatePrefab(shadowArtPrefab, visual.transform);
                silhouette.name = "Shadow_Art";
                foreach (string shapeName in new[] { "Body", "Head", "ArmL", "ArmR" })
                {
                    Transform shape = visual.transform.Find(shapeName);
                    if (shape == null) continue;
                    Renderer shapeRenderer = shape.GetComponent<Renderer>();
                    if (shapeRenderer != null) shapeRenderer.enabled = false;
                }
            }

            AddGhostSkullVisual(root, visual.transform);

            var head = new GameObject("Head");
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.72f, 0f);

            EnemySight sight = root.AddComponent<EnemySight>();
            sight.head = head.transform;
            sight.obstacleMask = ~(1 << EnemyLayer);
            sight.range = 15f;

            var breathingGo = new GameObject("Breathing");
            breathingGo.transform.SetParent(root.transform, false);
            AudioSource breathing = breathingGo.AddComponent<AudioSource>();
            breathing.playOnAwake = false;
            breathing.loop = true;
            breathing.spatialBlend = 1f;
            breathing.minDistance = 2f;
            breathing.maxDistance = 20f;
            breathing.rolloffMode = AudioRolloffMode.Linear;

            AudioSource oneShot = root.AddComponent<AudioSource>();
            oneShot.playOnAwake = false;
            oneShot.loop = false;
            oneShot.spatialBlend = 1f;
            oneShot.minDistance = 2f;
            oneShot.maxDistance = 25f;

            EnemyBehaviour behaviour = root.AddComponent<EnemyBehaviour>();
            behaviour.config = config;
            behaviour.agent = agent;
            behaviour.sight = sight;
            behaviour.breathingSource = breathing;
            behaviour.oneShotSource = oneShot;

            WraithAnimator wraithAnimator = root.GetComponent<WraithAnimator>();
            if (wraithAnimator != null) wraithAnimator.behaviour = behaviour;

            // The Shadow's voice: growl drone following the state machine.
            ShadowAudio shadowAudio = root.AddComponent<ShadowAudio>();
            shadowAudio.behaviour = behaviour;

            return SavePrefab(root, PrefabsFolder + "/Enemy", "TheShadow");
        }

        // GhostSkull (Quaternius CC0) wraith model with embedded clips. Only used
        // when the editor can import the .glb (needs the glTFast package); if it
        // does not load, the primitives above stay exactly as before. NOT SEEN IN
        // THE EDITOR: scale and pivot are auto-fitted from renderer bounds and
        // unverified.
        const string GhostSkullPath = "Assets/_Project/Models/SM_Wraith_GhostSkull.glb";

        static void AddGhostSkullVisual(GameObject root, Transform visual)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(GhostSkullPath);
            if (model == null)
            {
                Debug.LogWarning("[LightShadow] GhostSkull model did not import; keeping the primitive wraith.");
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(model, visual);
            instance.name = "GhostSkull";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                Object.DestroyImmediate(instance);
                Debug.LogWarning("[LightShadow] GhostSkull has no renderers; keeping the primitive wraith.");
                return;
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            float height = Mathf.Max(0.01f, bounds.size.y);
            float scale = Mathf.Clamp(1.5f / height, 0.01f, 100f);
            instance.transform.localScale = Vector3.one * scale;
            // Hover: lowest point 0.35 m above the floor.
            instance.transform.localPosition = new Vector3(0f, 0.35f - (bounds.min.y - instance.transform.position.y) * scale, 0f);

            foreach (string shapeName in new[] { "Body", "Head", "ArmL", "ArmR", "EyeL", "EyeR", "Shadow_Art" })
            {
                Transform shape = visual.Find(shapeName);
                if (shape == null) continue;
                foreach (Renderer r in shape.GetComponentsInChildren<Renderer>()) r.enabled = false;
            }

            Animation player = instance.GetComponent<Animation>();
            if (player == null) player = instance.AddComponent<Animation>();
            player.playAutomatically = false;

            var clipList = new System.Collections.Generic.List<AnimationClip>();
            foreach (Object sub in AssetDatabase.LoadAllAssetsAtPath(GhostSkullPath))
            {
                AnimationClip clip = sub as AnimationClip;
                if (clip != null && !clip.name.StartsWith("__preview__")) clipList.Add(clip);
            }

            WraithAnimator animator = root.AddComponent<WraithAnimator>();
            animator.animationPlayer = player;
            animator.clips = clipList.ToArray();
            animator.behaviour = null; // wired below, after EnemyBehaviour exists
        }

        static void AddShadowShape(Transform parent, string name, PrimitiveType type, Vector3 localPosition,
            Vector3 scale, Quaternion rotation, Material material)
        {
            GameObject shape = GameObject.CreatePrimitive(type);
            shape.name = name;
            shape.transform.SetParent(parent, false);
            shape.transform.localPosition = localPosition;
            shape.transform.localRotation = rotation;
            shape.transform.localScale = scale;

            Collider collider = shape.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);

            Renderer renderer = shape.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
        }

        // ----------------------------------------------------------------- battery

        static GameObject BuildBatteryPrefab(Palette palette)
        {
            var root = new GameObject("BatteryPickup");
            root.layer = InteractableLayer;

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.42f, 0.62f, 0.42f);
            collider.center = new Vector3(0f, 0.31f, 0f);

            // All visible parts live under "Visual" so PickupMotion can bob and
            // spin them without moving the collider the interaction ray targets.
            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = "Cell";
            body.transform.SetParent(visual.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            body.transform.localScale = new Vector3(0.13f, 0.16f, 0.13f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().sharedMaterial = palette.battery;

            Box(visual.transform, "Band", new Vector3(0f, 0.4f, 0f), new Vector3(0.16f, 0.05f, 0.16f),
                palette.metal, false, InteractableLayer);

            CreatePointLight(visual.transform, "Glow", new Vector3(0f, 0.55f, 0f),
                new Color(1f, 0.72f, 0.28f), 0.45f, 2.2f);

            PickupMotion motion = root.AddComponent<PickupMotion>();
            motion.visual = visual.transform;
            motion.bobHeight = 0.055f;
            motion.spinSpeed = 28f;

            root.AddComponent<BatteryPickup>();
            return SavePrefab(root, PrefabsFolder + "/Interactables", "BatteryPickup");
        }

        // -------------------------------------------------------------- quiz board

        static GameObject BuildQuizBoardPrefab(Palette palette)
        {
            var root = new GameObject("QuizBoard");
            root.layer = InteractableLayer;

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(2.4f, 1.42f, 0.22f);
            collider.center = new Vector3(0f, 0f, 0.04f);

            GameObject board = Box(root.transform, "Board", Vector3.zero, new Vector3(2.3f, 1.2f, 0.08f), palette.chalkboard);
            GameObject frameTop = Box(root.transform, "FrameTop", new Vector3(0f, 0.64f, 0f), new Vector3(2.42f, 0.08f, 0.14f), palette.wood);
            GameObject frameBottom = Box(root.transform, "FrameBottom", new Vector3(0f, -0.64f, 0f), new Vector3(2.42f, 0.08f, 0.14f), palette.wood);
            GameObject frameLeft = Box(root.transform, "FrameLeft", new Vector3(-1.19f, 0f, 0f), new Vector3(0.08f, 1.32f, 0.14f), palette.wood);
            GameObject frameRight = Box(root.transform, "FrameRight", new Vector3(1.19f, 0f, 0f), new Vector3(0.08f, 1.32f, 0.14f), palette.wood);

            // P2 hero prop: the Blender chalkboard (frame, tray, chalk sticks
            // and eraser included) replaces the greybox boxes when the FBX
            // has shipped. The prefab is rebuilt from code every run, so the
            // swap needs no marker; the QUIZ label and the collider stay.
            if (AddHeroModelChild(root, HeroModelChalkboard, QuizBoardChalkboardTargetHeight,
                    Quaternion.Euler(0f, 180f, 0f), new Vector3(0f, -0.6f, 0f)) != null)
            {
                foreach (GameObject part in new[] { board, frameTop, frameBottom, frameLeft, frameRight })
                {
                    HideRenderer(part);
                }
            }

            CreateWorldText(root.transform, "Label", "QUIZ\n<size=26>press E</size>", new Vector3(0f, 0.18f, -0.08f),
                Quaternion.identity, 0.08f, 64, new Color(0.92f, 0.94f, 0.86f));

            root.AddComponent<QuizBoard>();
            return SavePrefab(root, PrefabsFolder + "/Interactables", "QuizBoard");
        }

        // ---------------------------------------------------------------- fuse box

        static GameObject BuildFuseBoxPrefab(Palette palette)
        {
            var root = new GameObject("FuseBox");
            root.layer = InteractableLayer;

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.95f, 1.25f, 0.32f);
            collider.center = new Vector3(0f, 0f, 0.05f);

            Box(root.transform, "Case", Vector3.zero, new Vector3(0.82f, 1.02f, 0.24f), palette.fuse);
            Box(root.transform, "Door", new Vector3(0f, 0f, -0.14f), new Vector3(0.66f, 0.8f, 0.06f), palette.metal);
            Box(root.transform, "Handle", new Vector3(0.19f, -0.1f, -0.19f), new Vector3(0.16f, 0.05f, 0.05f), palette.metal);

            GameObject indicator = Box(root.transform, "Indicator", new Vector3(-0.24f, 0.32f, -0.19f),
                new Vector3(0.12f, 0.12f, 0.05f), palette.indicatorOff);

            CreateWorldText(root.transform, "Label", "FUSE", new Vector3(0f, 0.72f, -0.13f),
                Quaternion.identity, 0.06f, 48, new Color(0.9f, 0.78f, 0.55f));

            FuseBox fuse = root.AddComponent<FuseBox>();
            fuse.indicator = indicator.GetComponent<Renderer>();
            fuse.restoredMaterial = palette.indicatorOn;

            return SavePrefab(root, PrefabsFolder + "/Interactables", "FuseBox");
        }

        // ---------------------------------------------------------------- fragment

        static GameObject BuildFragmentPrefab(Palette palette)
        {
            var root = new GameObject("KnowledgeFragment");
            root.layer = InteractableLayer;

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.55f, 0.6f, 0.55f);
            collider.center = new Vector3(0f, 0.3f, 0f);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);

            Box(visual.transform, "Paper", new Vector3(0f, 0.4f, 0f), new Vector3(0.24f, 0.02f, 0.42f),
                palette.paper, false).transform.localRotation = Quaternion.Euler(8f, 24f, 10f);

            GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glow.name = "Glow";
            glow.transform.SetParent(visual.transform, false);
            glow.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            glow.transform.localScale = Vector3.one * 0.1f;
            Object.DestroyImmediate(glow.GetComponent<Collider>());
            glow.GetComponent<Renderer>().sharedMaterial = palette.lamp;

            CreatePointLight(visual.transform, "GlowLight", new Vector3(0f, 0.55f, 0f),
                new Color(1f, 0.85f, 0.6f), 0.6f, 2.4f);

            PickupMotion motion = root.AddComponent<PickupMotion>();
            motion.visual = visual.transform;
            motion.bobHeight = 0.05f;
            motion.spinSpeed = 22f;

            root.AddComponent<KnowledgeFragment>();
            return SavePrefab(root, PrefabsFolder + "/Interactables", "KnowledgeFragment");
        }

        // ------------------------------------------------------------------- gate

        // One leaf of the double gate. side = +1 hinges on the left pillar
        // (panel extends +x), -1 on the right pillar (panel extends -x).
        static Transform BuildGateLeaf(Transform parent, string name, float hingeX, float side, Palette palette)
        {
            var pivot = new GameObject(name);
            pivot.transform.SetParent(parent, false);
            pivot.transform.localPosition = new Vector3(hingeX, 0f, 0f);

            var leaf = new GameObject("Leaf");
            leaf.transform.SetParent(pivot.transform, false);
            leaf.transform.localPosition = new Vector3(side * 0.95f, 1.55f, 0f);
            Box(leaf.transform, "Panel", Vector3.zero, new Vector3(1.9f, 3.0f, 0.15f), palette.gate);
            for (int i = 0; i < 3; i++)
            {
                Box(leaf.transform, "Bar" + i, new Vector3(-0.6f + i * 0.6f, 0f, -0.1f),
                    new Vector3(0.07f, 2.9f, 0.07f), palette.metal, false);
            }
            return pivot.transform;
        }

        static GameObject BuildGatePrefab(Palette palette)
        {
            var root = new GameObject("ExitGate");
            root.layer = InteractableLayer;

            Box(root.transform, "PillarL", new Vector3(-2.1f, 1.6f, 0f), new Vector3(0.4f, 3.2f, 0.6f), palette.gate);
            Box(root.transform, "PillarR", new Vector3(2.1f, 1.6f, 0f), new Vector3(0.4f, 3.2f, 0.6f), palette.gate);
            Box(root.transform, "Crossbar", new Vector3(0f, 3.35f, 0f), new Vector3(4.6f, 0.4f, 0.6f), palette.gate);

            // Double gate: two leaves, one hinged on each pillar, together
            // covering the whole 3.8 m opening between the pillars (the old
            // single 3.7 m leaf read as a half-closed door in the middle).
            // Left swings +90, right -90, both outward (-z) when powered.
            Transform pivotLeft = BuildGateLeaf(root.transform, "GateLeafPivotL", -1.9f, 1f, palette);
            Transform pivotRight = BuildGateLeaf(root.transform, "GateLeafPivotR", 1.9f, -1f, palette);

            Box(root.transform, "Console", new Vector3(2.75f, 1.2f, 0.3f), new Vector3(0.8f, 1.0f, 0.4f), palette.metal);
            Box(root.transform, "Screen", new Vector3(2.75f, 1.45f, 0.52f), new Vector3(0.45f, 0.22f, 0.03f),
                palette.indicatorOff, false);
            CreateWorldText(root.transform, "Label", "MAIN GATE", new Vector3(0f, 3.85f, -0.28f),
                Quaternion.Euler(0f, 180f, 0f), 0.10f, 64, new Color(0.85f, 0.78f, 0.62f));

            // The console is the interaction collider; the escape trigger sits
            // outside the gate and only switches on once the crank is turned.
            BoxCollider consoleCollider = root.AddComponent<BoxCollider>();
            consoleCollider.center = new Vector3(2.75f, 1.2f, 0.3f);
            consoleCollider.size = new Vector3(1.0f, 1.35f, 0.65f);

            BoxCollider escape = root.AddComponent<BoxCollider>();
            escape.center = new Vector3(0f, 1.5f, -1.35f);
            escape.size = new Vector3(4.2f, 3.2f, 2.7f);
            escape.isTrigger = true;
            escape.enabled = false;

            ExitGate gate = root.AddComponent<ExitGate>();
            gate.gateLeaf = pivotLeft;
            gate.gateLeafRight = pivotRight;
            gate.escapeZone = escape;

            return SavePrefab(root, PrefabsFolder + "/Interactables", "ExitGate");
        }
        // Tapered beam cone in front of the flashlight: narrow at the lens,
        // wide at the far end, vertex alpha fading to zero so the haze
        // dissolves instead of ending. Forward is -Z (camera forward).
        // Subtle dust inside the beam cone. Local-space cone emitter, alpha
        // low on purpose - atmosphere, not snow.
        const string RoundCookiePath = "Assets/_Project/Textures/Art/Generated/T_TorchCookieRound.png";

        // 256 px greyscale spot cookie, regenerated by the builder so CI and every
        // machine get the same file: full brightness inside 30% of the radius, a
        // smooth cosine fall to black at the rim, and a soft ring at 82% the way a
        // reflector throws one.
        static Texture2D EnsureRoundTorchCookie()
        {
            const int size = 256;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f;
                    float dy = (y + 0.5f) / size * 2f - 1f;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    float core = r <= 0.3f ? 1f : 0.5f + 0.5f * Mathf.Cos(Mathf.PI * Mathf.Clamp01((r - 0.3f) / 0.68f));
                    float ring = 0.12f * Mathf.Exp(-Mathf.Pow((r - 0.82f) / 0.05f, 2f));
                    float v = r >= 0.98f ? 0f : Mathf.Clamp01(core * 0.92f + ring);
                    byte b = (byte)Mathf.RoundToInt(v * 255f);
                    pixels[y * size + x] = new Color32(b, b, b, b);
                }
            }
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            texture.SetPixels32(pixels);
            texture.Apply();
            byte[] png = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);
            byte[] existing = System.IO.File.Exists(RoundCookiePath) ? System.IO.File.ReadAllBytes(RoundCookiePath) : null;
            if (existing == null || !System.Linq.Enumerable.SequenceEqual(existing, png))
            {
                System.IO.File.WriteAllBytes(RoundCookiePath, png);
                AssetDatabase.ImportAsset(RoundCookiePath, ImportAssetOptions.ForceSynchronousImport);
            }
            var importer = AssetImporter.GetAtPath(RoundCookiePath) as TextureImporter;
            if (importer != null && (importer.textureType != TextureImporterType.Cookie || importer.wrapMode != TextureWrapMode.Clamp))
            {
                importer.textureType = TextureImporterType.Cookie;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.alphaSource = TextureImporterAlphaSource.FromGrayScale;
                importer.mipmapEnabled = true;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(RoundCookiePath);
        }

        static void BuildBeamDust(Transform parent, Light spot, Material beamMaterial)
        {
            var dust = new GameObject("BeamDust");
            dust.transform.SetParent(parent, false);
            dust.transform.localPosition = new Vector3(0f, 0f, 0.4f);

            ParticleSystem ps = dust.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = 2.4f;
            main.startSpeed = 0.02f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.008f, 0.025f);
            main.startColor = new Color(1f, 0.93f, 0.78f, 0.10f);
            main.maxParticles = 40;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 10f;

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 12f;
            shape.radius = 0.05f;
            shape.length = 4.5f;

            // A slow drift forward through the cone.
            ParticleSystem.VelocityOverLifetimeModule velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.z = new ParticleSystem.MinMaxCurve(1.2f);

            ParticleSystemRenderer renderer = dust.GetComponent<ParticleSystemRenderer>();
            // URP particle material with the soft dust sprite. The builtin
            // Default-ParticleSystem material is a Built-in pipeline asset and
            // drew every mote as a hard square under URP (seen in the editor
            // 2026-11-28); it is only the fallback now.
            Material dustMaterial = beamMaterial != null ? beamMaterial
                : AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
            renderer.sharedMaterial = dustMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            BeamDust motes = dust.AddComponent<BeamDust>();
            motes.beam = spot;
            motes.motes = ps;
        }


    }
}
