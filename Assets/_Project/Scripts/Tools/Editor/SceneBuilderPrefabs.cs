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
            Light spot = flashlightGo.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.color = new Color(1f, 0.94f, 0.82f);
            spot.intensity = 2.2f;
            spot.range = 8f;
            spot.spotAngle = 58f;
            spot.innerSpotAngle = 34f;
            spot.shadows = LightShadows.Soft;
            spot.shadowStrength = 0.75f;
            spot.shadowBias = 0.04f;
            spot.shadowNormalBias = 0.2f;

            PlayerController player = root.AddComponent<PlayerController>();
            FlashlightBattery battery = root.AddComponent<FlashlightBattery>();
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

            // The Shadow's voice: growl drone following the state machine.
            ShadowAudio shadowAudio = root.AddComponent<ShadowAudio>();
            shadowAudio.behaviour = behaviour;

            return SavePrefab(root, PrefabsFolder + "/Enemy", "TheShadow");
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
                new Color(1f, 0.72f, 0.28f), 0.8f, 3f);

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

            Box(root.transform, "Board", Vector3.zero, new Vector3(2.3f, 1.2f, 0.08f), palette.chalkboard);
            Box(root.transform, "FrameTop", new Vector3(0f, 0.64f, 0f), new Vector3(2.42f, 0.08f, 0.14f), palette.wood);
            Box(root.transform, "FrameBottom", new Vector3(0f, -0.64f, 0f), new Vector3(2.42f, 0.08f, 0.14f), palette.wood);
            Box(root.transform, "FrameLeft", new Vector3(-1.19f, 0f, 0f), new Vector3(0.08f, 1.32f, 0.14f), palette.wood);
            Box(root.transform, "FrameRight", new Vector3(1.19f, 0f, 0f), new Vector3(0.08f, 1.32f, 0.14f), palette.wood);

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
                new Color(1f, 0.85f, 0.6f), 1.1f, 3.2f);

            PickupMotion motion = root.AddComponent<PickupMotion>();
            motion.visual = visual.transform;
            motion.bobHeight = 0.05f;
            motion.spinSpeed = 22f;

            root.AddComponent<KnowledgeFragment>();
            return SavePrefab(root, PrefabsFolder + "/Interactables", "KnowledgeFragment");
        }

        // ------------------------------------------------------------------- gate

        static GameObject BuildGatePrefab(Palette palette)
        {
            var root = new GameObject("ExitGate");
            root.layer = InteractableLayer;

            Box(root.transform, "PillarL", new Vector3(-2.1f, 1.6f, 0f), new Vector3(0.4f, 3.2f, 0.6f), palette.gate);
            Box(root.transform, "PillarR", new Vector3(2.1f, 1.6f, 0f), new Vector3(0.4f, 3.2f, 0.6f), palette.gate);
            Box(root.transform, "Crossbar", new Vector3(0f, 3.35f, 0f), new Vector3(4.6f, 0.4f, 0.6f), palette.gate);

            var pivot = new GameObject("GateLeafPivot");
            pivot.transform.SetParent(root.transform, false);
            pivot.transform.localPosition = new Vector3(-1.85f, 0f, 0f);

            var leaf = new GameObject("Leaf");
            leaf.transform.SetParent(pivot.transform, false);
            leaf.transform.localPosition = new Vector3(1.85f, 1.55f, 0f);
            Box(leaf.transform, "Panel", Vector3.zero, new Vector3(3.7f, 3.0f, 0.15f), palette.gate);
            for (int i = 0; i < 5; i++)
            {
                Box(leaf.transform, "Bar" + i, new Vector3(-1.45f + i * 0.72f, 0f, -0.1f),
                    new Vector3(0.07f, 2.9f, 0.07f), palette.metal, false);
            }

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
            gate.gateLeaf = pivot.transform;
            gate.escapeZone = escape;

            return SavePrefab(root, PrefabsFolder + "/Interactables", "ExitGate");
        }
    }
}
