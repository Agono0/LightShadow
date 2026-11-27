using System;
using System.Collections.Generic;
using LightShadow.Core;
using LightShadow.Enemy;
using LightShadow.Gameplay;
using LightShadow.Player;
using LightShadow.UI;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.AI;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Scene = UnityEngine.SceneManagement.Scene;

namespace LightShadow.Tools
{
    public static partial class SceneBuilder
    {
        sealed class ZoneSet
        {
            public LitZone entrance;
            public LitZone library;
            public LitZone physics;
            public LitZone mainHall;
            public LitZone staff;
            public LitZone infirmary;
            public GameObject libraryLights;
            public GameObject physicsLights;
            public GameObject mainHallLights;
            public GameObject staffLights;
            public GameObject dawnLights;
        }

        // ============================================================= bootstrap

        static void BuildBootstrapScene()
        {
            Scene scene = NewScene();
            ApplyAtmosphere(new Color(0.01f, 0.01f, 0.02f), Color.black, 0.02f);

            CreateCamera("BootstrapCamera", new Vector3(0f, 0f, -10f), Quaternion.identity, Color.black);
            CreatePostVolume(null, "GlobalVolume", PostNightPath);
            Canvas canvas = SceneBuilderUI.BuildBootstrapUI();

            var bootstrap = new GameObject("Bootstrap");
            bootstrap.AddComponent<BootstrapLoader>();

            // The mixer must be wired process-wide before the menu loads
            // (D15 follow-up §4.2); snapshot driving starts here and the school
            // Systems copy is the fallback for direct school-scene play.
            AttachAudioSnapshot(bootstrap.transform);

            SaveScene(scene, BootstrapPath);
        }

        // =============================================================== menu

        static void BuildMainMenuScene()
        {
            Scene scene = NewScene();
            // The menu is now a LIVE 3D vignette (player request): a school
            // corridor at night, one flickering lamp, drifting dust and the
            // Shadow waiting far down the hall. The camera breathes.
            ApplyAtmosphere(new Color(0.010f, 0.012f, 0.020f), new Color(0.012f, 0.015f, 0.024f), 0.032f);

            Material floor = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/Art/Mat_Art_WoodFloor.mat");
            Material wall = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/Art/Mat_Art_PlasterWall.mat");
            Material ceiling = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/Art/Mat_Art_Ceiling.mat");
            Material metal = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/Art/Mat_Art_MetalLocker.mat");
            Material wood = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/Art/Mat_Art_DoorWood.mat");

            var diorama = new GameObject("MenuDiorama");
            diorama.layer = LevelLayer;
            Transform t = diorama.transform;
            Box(t, "Floor", new Vector3(0f, -0.1f, 0f), new Vector3(7f, 0.2f, 34f), floor);
            Box(t, "Ceiling", new Vector3(0f, 4.3f, 0f), new Vector3(7f, 0.2f, 34f), ceiling);
            Box(t, "WallL", new Vector3(-3.6f, 2.1f, 0f), new Vector3(0.3f, 4.4f, 34f), wall);
            Box(t, "WallR", new Vector3(3.6f, 2.1f, 0f), new Vector3(0.3f, 4.4f, 34f), wall);
            Box(t, "EndWall", new Vector3(0f, 2.1f, 17.2f), new Vector3(7.5f, 4.4f, 0.3f), wall);
            Box(t, "DoorFrame", new Vector3(0f, 1.6f, 16.9f), new Vector3(1.6f, 3.2f, 0.12f), wood);
            // Wall dressing for parallax depth while the camera drifts.
            for (int i = 0; i < 4; i++)
            {
                float z = -8f + i * 6f;
                Box(t, "LockerL" + i, new Vector3(-3.15f, 1.1f, z), new Vector3(0.5f, 1.9f, 1.1f), metal);
                Box(t, "LockerR" + i, new Vector3(3.15f, 1.1f, z + 3f), new Vector3(0.5f, 1.9f, 1.1f), metal);
            }
            Box(t, "Bench", new Vector3(0.8f, 0.45f, -4f), new Vector3(1.9f, 0.5f, 0.55f), wood);

            // One lamp flickering near the camera, one dying far away.
            AddLamp(t, new Vector3(0f, 3.7f, -3f), new Color(1f, 0.72f, 0.40f), 1.1f, 9f);
            AddLamp(t, new Vector3(0f, 3.7f, 9f), new Color(1f, 0.70f, 0.38f), 0.45f, 7f);

            // The Shadow, far down the hall, barely inside the falloff.
            GameObject shadowArt = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Art/Shadow_Art.prefab");
            if (shadowArt != null)
            {
                GameObject silhouette = (GameObject)PrefabUtility.InstantiatePrefab(shadowArt, t);
                silhouette.transform.localPosition = new Vector3(0.4f, 0f, 13.5f);
                silhouette.transform.localRotation = Quaternion.Euler(0f, 195f, 0f);
            }

            // Drifting dust catches the lamp light.
            GameObject dust = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Art/DustMotes.prefab");
            if (dust != null)
            {
                GameObject dustInstance = (GameObject)PrefabUtility.InstantiatePrefab(dust, t);
                dustInstance.transform.localPosition = new Vector3(0f, 1.8f, -4f);
            }

            // Camera inside the corridor; MenuCameraDrift breathes slowly.
            Camera menuCamera = CreateCamera("MenuCamera", new Vector3(0f, 1.6f, -13f), Quaternion.identity,
                new Color(0.004f, 0.005f, 0.008f));
            menuCamera.transform.rotation = Quaternion.Euler(2f, 0f, 0f);
            menuCamera.gameObject.AddComponent<LightShadow.UI.MenuCameraDrift>();

            CreatePostVolume(null, "GlobalVolume", PostNightPath);
            EnsureEventSystem();
            SceneBuilderUI.UseLiveBackdrop = false; // the corridor IS the backdrop
            SceneBuilderUI.BuildMainMenuUI();

            SaveScene(scene, MainMenuPath);
        }

        // ============================================================== ending

        static void BuildEndingScene(Palette palette)
        {
            Scene scene = NewScene();
            ApplyAtmosphere(new Color(0.075f, 0.065f, 0.070f), new Color(0.16f, 0.12f, 0.10f), 0.010f);

            // A real set instead of a painted backdrop: the camera stands on the
            // pavement outside the gate and the dawn happens in three dimensions
            // while EndingController drives the sun and the light.
            var set = Group("DawnSet", null);
            Transform t = set.transform;

            Box(t, "Ground", new Vector3(0f, -0.1f, 10f), new Vector3(90f, 0.2f, 90f), palette.stone);
            Box(t, "Path", new Vector3(0f, 0.02f, 9f), new Vector3(6f, 0.04f, 16f), palette.floor, false);

            // ---- school silhouette ------------------------------------------
            Box(t, "School", new Vector3(0f, 3.8f, 20f), new Vector3(44f, 7.6f, 4f), palette.wall);
            Box(t, "SchoolRoof", new Vector3(0f, 7.75f, 20f), new Vector3(45f, 0.35f, 4.6f), palette.ceiling, false);
            Box(t, "SchoolSkirt", new Vector3(0f, 0.25f, 17.9f), new Vector3(44f, 0.5f, 0.3f), palette.ceiling, false);

            for (int i = 0; i < 9; i++)
            {
                for (int row = 0; row < 2; row++)
                {
                    float x = -18f + i * 4.5f;
                    float y = row == 0 ? 5.1f : 2.4f;
                    bool lit = (i * 3 + row) % 5 == 0;
                    Box(t, "Window_" + row + "_" + i, new Vector3(x, y, 17.92f), new Vector3(1.6f, 1.15f, 0.06f),
                        lit ? palette.lamp : palette.ceiling, false);
                }
            }

            CreateWorldText(t, "SchoolSign", "AL-NOOR SECONDARY SCHOOL", new Vector3(0f, 6.55f, 17.9f),
                Quaternion.identity, 0.045f, 64, new Color(0.86f, 0.78f, 0.60f));

            // ---- boundary wall and the open gate ----------------------------
            Box(t, "BoundaryL", new Vector3(-15f, 1.3f, 4f), new Vector3(18f, 2.6f, 0.4f), palette.wall);
            Box(t, "BoundaryR", new Vector3(15f, 1.3f, 4f), new Vector3(18f, 2.6f, 0.4f), palette.wall);
            Box(t, "PillarL", new Vector3(-6f, 1.9f, 4f), new Vector3(0.9f, 3.8f, 0.9f), palette.gate);
            Box(t, "PillarR", new Vector3(6f, 1.9f, 4f), new Vector3(0.9f, 3.8f, 0.9f), palette.gate);
            Box(t, "GateLintel", new Vector3(0f, 3.75f, 4f), new Vector3(12.6f, 0.35f, 0.7f), palette.gate, false);

            AddGateLeaf(t, "GateLeafL", -5.7f, 58f, 1f, palette);
            AddGateLeaf(t, "GateLeafR", 5.7f, -58f, -1f, palette);

            AddLamp(t, new Vector3(-6f, 4.05f, 4f), new Color(1f, 0.74f, 0.42f), 1.2f, 9f, false);
            AddLamp(t, new Vector3(6f, 4.05f, 4f), new Color(1f, 0.74f, 0.42f), 1.2f, 9f, false);

            // ---- the pavement outside the gate ------------------------------
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -5.2f : 5.2f;
                float z = -2.5f;
                Box(t, "LampPost_" + i, new Vector3(x, 1.7f, z), new Vector3(0.12f, 3.4f, 0.12f), palette.metal, false);
                AddLamp(t, new Vector3(x, 3.4f, z), new Color(1f, 0.70f, 0.38f), 0.85f, 8f, false);
            }

            Box(t, "Pavement", new Vector3(0f, 0.03f, -6f), new Vector3(18f, 0.06f, 12f), palette.floor, false);
            Box(t, "KerbL", new Vector3(-9f, 0.09f, -6f), new Vector3(0.3f, 0.18f, 12f), palette.stone, false);
            Box(t, "KerbR", new Vector3(9f, 0.09f, -6f), new Vector3(0.3f, 0.18f, 12f), palette.stone, false);

            // ---- sky, hills, sun --------------------------------------------
            GameObject sky = Box(t, "Sky", new Vector3(0f, 14f, 40f), new Vector3(140f, 56f, 0.4f), palette.sky, false);

            Box(t, "HillFar", new Vector3(-16f, 2.2f, 34f), new Vector3(70f, 4.4f, 6f), palette.ceiling, false);
            Box(t, "HillNear", new Vector3(18f, 1.4f, 31f), new Vector3(54f, 2.8f, 5f), palette.ceiling, false);

            GameObject sun = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sun.name = "Sun";
            sun.transform.SetParent(t, false);
            sun.transform.localPosition = new Vector3(-21f, 1.6f, 28f);
            sun.transform.localScale = new Vector3(5.2f, 5.2f, 5.2f);
            Object.DestroyImmediate(sun.GetComponent<Collider>());
            sun.GetComponent<Renderer>().sharedMaterial = palette.sun;

            Light dawnLight = CreateDawnLight(t);

            // ---- camera and UI ----------------------------------------------
            Camera camera = CreateCamera("EndingCamera", new Vector3(0f, 1.65f, -7f), Quaternion.identity,
                new Color(0.05f, 0.05f, 0.08f));
            camera.fieldOfView = 55f;

            CreatePostVolume(null, "DawnVolume", PostDawnPath);
            EnsureEventSystem();
            EndingController ending = SceneBuilderUI.BuildEndingUI();

            ending.sun = sun.transform;
            ending.sunRenderer = sun.GetComponent<Renderer>();
            ending.skyRenderer = sky.GetComponent<Renderer>();
            ending.dawnLight = dawnLight;
            ending.endingCamera = camera;

            // Ending music cues (Phase 1): one-shot per ending id, wired from
            // the audio library (Kevin MacLeod CC-BY). PartialMemory /
            // HollowEscape have no cue yet and stay silent until the Pixabay
            // primaries are downloaded (Docs/SOURCING_FONTS_MUSIC.md).
            ending.musicSource = CreateAudioSource(ending.transform, "EndingMusicSource");
            LightShadow.Audio.AudioLibrary audioLibrary =
                AssetDatabase.LoadAssetAtPath<LightShadow.Audio.AudioLibrary>(
                    "Assets/_Project/Audio/AudioLibrary.asset");
            if (audioLibrary != null)
            {
                ending.endingMusic = new[]
                {
                    new EndingController.EndingMusic { endingId = "FullRecovery", clip = audioLibrary.endingHopeGold },
                    new EndingController.EndingMusic { endingId = "Dawnless", clip = audioLibrary.endingDawnlessGrey },
                };
            }

            SaveScene(scene, EndingPath);
        }

        // One leaf of the gate, hinged at the pillar and swung towards the road.
        // Built from bars instead of a slab so the school stays visible through it.
        static void AddGateLeaf(Transform parent, string name, float hingeX, float yaw, float direction, Palette palette)
        {
            const float length = 5.6f;

            var hinge = new GameObject(name);
            hinge.transform.SetParent(parent, false);
            hinge.transform.localPosition = new Vector3(hingeX, 0f, 4f);
            hinge.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            for (int i = 0; i < 7; i++)
            {
                float x = direction * (0.45f + i * (length - 0.9f) / 6f);
                Box(hinge.transform, "Bar_" + i, new Vector3(x, 1.2f, 0f), new Vector3(0.07f, 2.2f, 0.07f),
                    palette.gate, false);
            }

            Box(hinge.transform, "RailTop", new Vector3(direction * length * 0.5f, 2.16f, 0f),
                new Vector3(length, 0.1f, 0.12f), palette.gate, false);
            Box(hinge.transform, "RailBottom", new Vector3(direction * length * 0.5f, 0.34f, 0f),
                new Vector3(length, 0.1f, 0.12f), palette.gate, false);
        }

        static Light CreateDawnLight(Transform parent)
        {
            var go = new GameObject("DawnLight");
            go.transform.SetParent(parent, false);
            go.transform.rotation = Quaternion.Euler(14f, 148f, 0f);

            Light light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.72f, 0.44f);
            light.intensity = 0f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.55f;
            light.renderMode = LightRenderMode.ForcePixel;
            return light;
        }

        // ============================================================== school

        static void BuildSchoolScene(GameConfig config, QuestionDatabase database, Palette palette, Prefabs prefabs)
        {
            Scene scene = NewScene();
            // Darkness pass: near-black ambient — the flashlight and the lit
            // islands carry the scene now (player request: top-horror look).
            ApplyAtmosphere(new Color(0.012f, 0.014f, 0.022f), new Color(0.014f, 0.017f, 0.026f), 0.026f);

            var level = new GameObject("Level");
            level.layer = LevelLayer;
            NavMeshSurface surface = level.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = 1 << LevelLayer;
            surface.minRegionArea = 0.5f;

            NavMeshBootstrap navBootstrap = level.AddComponent<NavMeshBootstrap>();
            navBootstrap.surface = surface;

            CreateDirectionalMoon(level.transform);

            var environment = Group("Environment", level.transform);
            BuildSchoolShell(environment.transform, palette);

            ZoneSet zones = BuildLightingAndZones(level.transform, palette);

            BuildLibrary(level.transform, zones, prefabs, palette, database);
            BuildPhysicsLab(level.transform, zones, prefabs, palette, database);
            BuildClassroom(level.transform, prefabs, palette, database);
            BuildGarden(level.transform, prefabs, palette, database);
            BuildStaffRoom(level.transform, zones, prefabs, palette, database);
            BuildInfirmary(level.transform, zones, prefabs, palette, database);
            BuildMainHall(level.transform, zones, prefabs, palette, database);
            BuildEntrance(level.transform, prefabs, palette, database);
            BuildGate(level.transform, zones, prefabs, palette, database);
            BuildStoryPlaques(level.transform);
            BuildHideSpots(level.transform, palette);

            // Premium pass §8.3: one reverb zone per interior room (structural —
            // the distances are computed from the room bounds, no GameConfig
            // fields). Added before the bake: the zone colliders are triggers on
            // the Default layer, so the Level-layer bake mask never collects them.
            AttachReverbZones(level.transform);

            BakeNavMesh(surface);

            // Player and enemy are spawned after the bake so their colliders can
            // never become part of the walkable mesh.
            var spawn = Empty(level.transform, "Spawn_Entrance", new Vector3(0f, 0.05f, -29.5f), Quaternion.identity);
            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.player);
            player.name = "Player";
            player.transform.SetParent(level.transform, false);
            player.transform.SetPositionAndRotation(spawn.transform.position, Quaternion.identity);

            PlayerController playerController = player.GetComponent<PlayerController>();
            FlashlightBattery flashlight = player.GetComponent<FlashlightBattery>();
            BrightIdeaMeter meter = player.GetComponent<BrightIdeaMeter>();
            InteractionRaycaster raycaster = player.GetComponent<InteractionRaycaster>();
            Camera playerCamera = player.GetComponentInChildren<Camera>();
            Light spot = player.GetComponentInChildren<Light>();

            playerController.config = config;
            playerController.cameraPivot = player.transform.Find("CameraPivot");
            playerController.playerCamera = playerCamera;

            flashlight.config = config;
            flashlight.meter = meter;
            flashlight.spotLight = spot;

            meter.config = config;
            meter.flashlight = flashlight;
            meter.spotLight = spot;

            raycaster.config = config;
            raycaster.rayCamera = playerCamera;
            raycaster.interactMask = ~((1 << PlayerLayer) | (1 << EnemyLayer));

            // The Shadow starts far from Nour, in the main hall.
            GameObject enemy = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.enemy);
            enemy.name = "TheShadow";
            enemy.transform.SetParent(level.transform, false);
            enemy.transform.position = new Vector3(0f, 0f, 33f);

            PatrolRoute route = BuildPatrolRoute(level.transform);

            EnemyBehaviour behaviour = enemy.GetComponent<EnemyBehaviour>();
            behaviour.config = config;
            behaviour.player = player.transform;
            behaviour.playerController = playerController;
            behaviour.flashlight = flashlight;
            behaviour.meter = meter;
            behaviour.playerCamera = playerCamera;
            behaviour.route = route;

            var systems = new GameObject("Systems");
            GameManager manager = systems.AddComponent<GameManager>();
            CheckpointManager checkpoints = systems.AddComponent<CheckpointManager>();
            AudioManager audio = systems.AddComponent<AudioManager>();
            systems.AddComponent<PuzzleManager>();

            AudioSource music = CreateAudioSource(systems.transform, "MusicSource");
            AudioSource ambience = CreateAudioSource(systems.transform, "AmbienceSource");
            AudioSource sfx = CreateAudioSource(systems.transform, "SfxSource");
            audio.musicSource = music;
            audio.ambienceSource = ambience;
            audio.sfxSource = sfx;

            // The brightness option acts through URP post-exposure and ambient lift.
            systems.AddComponent<BrightnessController>();

            // P3 menace director: the Shadow pacing gauge (plan 4.12).
            MenaceDirector menace = systems.AddComponent<MenaceDirector>();
            menace.config = config;
            menace.enemy = behaviour;
            menace.player = player.transform;

            // Real-audio pass: register the imported CC0 clip library (idempotent;
            // consumers keep synthesised fallbacks when a clip is missing).
            LightShadow.Audio.AudioLibrary audioLibrary =
                AssetDatabase.LoadAssetAtPath<LightShadow.Audio.AudioLibrary>(
                    "Assets/_Project/Audio/AudioLibrary.asset");
            if (audioLibrary != null)
            {
                var bootstrap = systems.AddComponent<LightShadow.Audio.AudioLibraryBootstrap>();
                bootstrap.library = audioLibrary;
            }

            // Snapshot driving on the school Systems object (idempotent; direct
            // play of the school scene works without the bootstrap).
            AttachAudioSnapshot(systems.transform);

            CreatePostVolume(systems.transform, "GlobalVolume", PostNightPath);

            checkpoints.defaultSpawn = spawn.transform;

            SceneBuilderUI.SchoolUI ui = SceneBuilderUI.BuildSchoolUI(player.transform);
            EnsureEventSystem();

            manager.config = config;
            manager.player = player.transform;
            manager.enemy = behaviour;
            manager.database = database;
            manager.hud = ui.hud;
            manager.quizUI = ui.quiz;
            manager.recapScreen = null;

            raycaster.hud = ui.hud;
            ui.touch.player = playerController;

            // Give the starting checkpoint a spawn even before a LitZone fires.
            Empty(environment.transform, "PlayerStartMarker", new Vector3(0f, 0f, -29.5f), Quaternion.identity);

            // Save the freshly built scene BEFORE the cinematic rigs run:
            // BuildCinematicRigs resolves the school scene by path, and for an
            // UNSAVED active scene it would OpenScene(Single) the stale file
            // from disk — silently discarding the entire build (this exact bug
            // ate every school-scene builder change between round 1 and now).
            SaveScene(scene, SchoolPath);

            // Premium pass: dormant Cinemachine rigs for the scripted catch/dawn/gate shots.
            BuildCinematicRigs();
        }

        // ------------------------------------------------------------- shell

        static void BuildSchoolShell(Transform parent, Palette palette)
        {
            // One continuous floor under the whole building.
            Box(parent, "Floor", new Vector3(0f, -0.1f, 3.5f), new Vector3(46f, 0.2f, 77f), palette.floor);

            // Garden grass, slightly above the concrete.
            Box(parent, "GardenGrass", new Vector3(12.3f, 0.02f, 11f), new Vector3(19f, 0.04f, 14f),
                palette.grass, false);

            // Ceilings.
            Ceiling(parent, -2.8f, 2.8f, -27f, 27f, 3.3f, palette.ceiling);
            Ceiling(parent, -7f, 7f, -33f, -27f, 3.3f, palette.ceiling);
            Ceiling(parent, -22f, -2.65f, -22f, -8f, 3.3f, palette.ceiling);
            Ceiling(parent, 2.65f, 22f, -22f, -8f, 3.3f, palette.ceiling);
            Ceiling(parent, -22f, -2.65f, 4f, 18f, 3.3f, palette.ceiling);
            Ceiling(parent, -22f, -2.65f, 20f, 26f, 3.3f, palette.ceiling);
            Ceiling(parent, -14f, 14f, 27f, 41f, 4.8f, palette.ceiling);

            // ---- central corridor, with a door gap for every wing -------------
            WallX(parent, -2.65f, -27f, -16f, 3.2f, palette.wall);
            WallX(parent, -2.65f, -14f, -3f, 3.2f, palette.wall);
            WallX(parent, -2.65f, -1f, 10f, 3.2f, palette.wall);
            WallX(parent, -2.65f, 12f, 22f, 3.2f, palette.wall);
            WallX(parent, -2.65f, 24f, 27f, 3.2f, palette.wall);

            WallX(parent, 2.65f, -27f, -16f, 3.2f, palette.wall);
            WallX(parent, 2.65f, -14f, 10f, 3.2f, palette.wall);
            WallX(parent, 2.65f, 12f, 27f, 3.2f, palette.wall);

            // ---- locked wing doors (decor) -----------------------------------
            // Grille-door hosts in the classroom and staff-room doorways. No
            // colliders, so the walk gaps and the NavMesh stay exactly as
            // built; the art pass swaps in the SM_Arch_DoorGrille hero mesh,
            // and the wood-material box is its fallback while the FBX has
            // not shipped.
            Box(parent, "Door_Classroom", new Vector3(-2.65f, 1.025f, 10.5f),
                new Vector3(0.06f, 2.05f, 0.95f), palette.wood, false);
            Box(parent, "Door_Staff", new Vector3(-2.65f, 1.025f, 22.5f),
                new Vector3(0.06f, 2.05f, 0.95f), palette.wood, false);

            // ---- entrance hall ------------------------------------------------
            WallX(parent, -7f, -33f, -27f, 3.2f, palette.wall);
            WallX(parent, 7f, -33f, -27f, 3.2f, palette.wall);
            WallZ(parent, -27f, -7f, -1.5f, 3.2f, palette.wall);
            WallZ(parent, -27f, 1.5f, 7f, 3.2f, palette.wall);
            WallZ(parent, -33f, -7f, -1.75f, 3.2f, palette.wall);
            WallZ(parent, -33f, 1.75f, 7f, 3.2f, palette.wall);
            Box(parent, "EntranceLintel", new Vector3(0f, 3.28f, -27f), new Vector3(3f, 0.16f, 0.3f), palette.wall, false);

            // ---- library wing -------------------------------------------------
            WallX(parent, -22f, -22f, -8f, 3.2f, palette.wall);
            WallZ(parent, -22f, -22f, -2.65f, 3.2f, palette.wall);
            WallZ(parent, -8f, -22f, -2.65f, 3.2f, palette.wall);

            // ---- physics lab --------------------------------------------------
            WallX(parent, 22f, -22f, -8f, 3.2f, palette.wall);
            WallZ(parent, -22f, 2.65f, 22f, 3.2f, palette.wall);
            WallZ(parent, -8f, 2.65f, 22f, 3.2f, palette.wall);

            // ---- infirmary (west strip between library and classroom) ------
            WallX(parent, -22f, -8f, 4f, 3.2f, palette.wall);
            Ceiling(parent, -22f, -2.65f, -8f, 4f, 3.3f, palette.ceiling);

            // ---- classroom ----------------------------------------------------
            WallX(parent, -22f, 4f, 18f, 3.2f, palette.wall);
            WallZ(parent, 4f, -22f, -2.65f, 3.2f, palette.wall);
            WallZ(parent, 18f, -22f, -2.65f, 3.2f, palette.wall);

            // ---- garden (open to the sky) ------------------------------------
            WallX(parent, 22f, 4f, 18f, 3.2f, palette.wall);
            WallZ(parent, 4f, 2.65f, 22f, 3.2f, palette.wall);
            WallZ(parent, 18f, 2.65f, 22f, 3.2f, palette.wall);

            // ---- staff room ---------------------------------------------------
            WallX(parent, -22f, 20f, 26f, 3.2f, palette.wall);
            WallZ(parent, 20f, -22f, -2.65f, 3.2f, palette.wall);
            WallZ(parent, 26f, -22f, -2.65f, 3.2f, palette.wall);

            // ---- main hall ----------------------------------------------------
            WallX(parent, -14f, 27f, 41f, 4.5f, palette.wall);
            WallX(parent, 14f, 27f, 41f, 4.5f, palette.wall);
            WallZ(parent, 41f, -14f, 14f, 4.5f, palette.wall);
            WallZ(parent, 27f, -14f, -1.5f, 4.5f, palette.wall);
            WallZ(parent, 27f, 1.5f, 14f, 4.5f, palette.wall);
            Box(parent, "MainHallLintel", new Vector3(0f, 3.85f, 27f), new Vector3(3f, 1.3f, 0.3f), palette.wall, false);

            // ---- wing labels at the doorways ---------------------------------
            CreateWorldText(parent, "Label_Library", "LIBRARY", new Vector3(-2.46f, 2.65f, -15f),
                Quaternion.Euler(0f, -90f, 0f), 0.05f, 64, new Color(0.82f, 0.76f, 0.62f));
            CreateWorldText(parent, "Label_Classroom", "LITERATURE", new Vector3(-2.46f, 2.65f, 11f),
                Quaternion.Euler(0f, -90f, 0f), 0.045f, 64, new Color(0.82f, 0.76f, 0.62f));
            CreateWorldText(parent, "Label_Staff", "STAFF ROOM", new Vector3(-2.46f, 2.65f, 23f),
                Quaternion.Euler(0f, -90f, 0f), 0.042f, 64, new Color(0.82f, 0.76f, 0.62f));
            CreateWorldText(parent, "Label_Infirmary", "INFIRMARY", new Vector3(-2.46f, 2.65f, -2f),
                Quaternion.Euler(0f, -90f, 0f), 0.045f, 64, new Color(0.82f, 0.76f, 0.62f));
            CreateWorldText(parent, "Label_Physics", "PHYSICS LAB", new Vector3(2.46f, 2.65f, -15f),
                Quaternion.Euler(0f, 90f, 0f), 0.045f, 64, new Color(0.82f, 0.76f, 0.62f));
            CreateWorldText(parent, "Label_Garden", "GARDEN", new Vector3(2.46f, 2.65f, 11f),
                Quaternion.Euler(0f, 90f, 0f), 0.05f, 64, new Color(0.82f, 0.76f, 0.62f));
            CreateWorldText(parent, "Label_MainHall", "MAIN HALL", new Vector3(0f, 3.85f, 26.78f),
                Quaternion.identity, 0.055f, 64, new Color(0.82f, 0.76f, 0.62f));
            CreateWorldText(parent, "Label_Entrance", "ENTRANCE HALL", new Vector3(-4f, 2.65f, -27.18f),
                Quaternion.identity, 0.04f, 64, new Color(0.82f, 0.76f, 0.62f));

            CreateWorldText(parent, "Label_School", "AL-NOOR SECONDARY SCHOOL", new Vector3(0f, 2.75f, -32.72f),
                Quaternion.Euler(0f, 180f, 0f), 0.06f, 64, new Color(0.75f, 0.68f, 0.53f));
        }

        // ------------------------------------------------------------ lighting

        static ZoneSet BuildLightingAndZones(Transform parent, Palette palette)
        {
            var set = new ZoneSet();

            var lights = Group("Lighting", parent);

            // Entrance is always lit and is the first checkpoint.
            AddLamp(lights.transform, new Vector3(-3f, 2.85f, -30f), new Color(1f, 0.83f, 0.58f), 2.4f, 10f);
            AddLamp(lights.transform, new Vector3(3f, 2.85f, -30f), new Color(1f, 0.83f, 0.58f), 2.4f, 10f);
            CreatePointLight(lights.transform, "GateGlow", new Vector3(0f, 2.6f, -33.5f),
                new Color(1f, 0.72f, 0.42f), 1.6f, 8f);

            // A pair of weak blue emergency lights keep the corridor navigable.
            AddLamp(lights.transform, new Vector3(0f, 2.9f, -20f), new Color(0.38f, 0.48f, 0.70f), 0.65f, 8f);
            AddLamp(lights.transform, new Vector3(0f, 2.9f, 20f), new Color(0.38f, 0.48f, 0.70f), 0.65f, 8f);

            // Garden moonlight.
            CreatePointLight(lights.transform, "GardenMoon", new Vector3(12f, 5f, 11f),
                new Color(0.52f, 0.64f, 0.90f), 1.0f, 26f);

            // A soft fill on every wing doorway so the signs read from the hall.
            CreatePointLight(lights.transform, "DoorwayFill", new Vector3(0f, 2.9f, -15f),
                new Color(0.42f, 0.5f, 0.68f), 0.55f, 7f);

            // ---- fuse-controlled light groups ---------------------------------
            set.libraryLights = Group("LibraryLights", parent);
            AddLamp(set.libraryLights.transform, new Vector3(-8f, 2.8f, -19f), new Color(1f, 0.80f, 0.52f), 2.6f, 12f);
            AddLamp(set.libraryLights.transform, new Vector3(-16f, 2.8f, -14f), new Color(1f, 0.80f, 0.52f), 2.6f, 12f);
            AddLamp(set.libraryLights.transform, new Vector3(-8f, 2.8f, -10f), new Color(1f, 0.80f, 0.52f), 2.6f, 12f);

            // The infirmary belongs to the library wing: its lamp lives under
            // the same fuse-controlled root (the staff room / hall precedent).
            AddLamp(set.libraryLights.transform, new Vector3(-12f, 2.8f, -2f), new Color(0.86f, 0.92f, 1f), 2.2f, 10f);

            set.physicsLights = Group("PhysicsLights", parent);
            AddLamp(set.physicsLights.transform, new Vector3(8f, 2.8f, -19f), new Color(0.85f, 0.90f, 1f), 2.5f, 12f);
            AddLamp(set.physicsLights.transform, new Vector3(16f, 2.8f, -14f), new Color(0.85f, 0.90f, 1f), 2.5f, 12f);
            AddLamp(set.physicsLights.transform, new Vector3(8f, 2.8f, -10f), new Color(0.85f, 0.90f, 1f), 2.5f, 12f);

            set.mainHallLights = Group("MainHallLights", parent);
            AddLamp(set.mainHallLights.transform, new Vector3(-8f, 4f, 31f), new Color(1f, 0.82f, 0.55f), 3.0f, 15f);
            AddLamp(set.mainHallLights.transform, new Vector3(8f, 4f, 31f), new Color(1f, 0.82f, 0.55f), 3.0f, 15f);
            AddLamp(set.mainHallLights.transform, new Vector3(-8f, 4f, 38f), new Color(1f, 0.82f, 0.55f), 3.0f, 15f);
            AddLamp(set.mainHallLights.transform, new Vector3(8f, 4f, 38f), new Color(1f, 0.82f, 0.55f), 3.0f, 15f);

            // The staff room belongs to the Main Hall wing: its lamps live under
            // the same fuse-controlled root as the hall.
            AddLamp(set.mainHallLights.transform, new Vector3(-8f, 2.8f, 23f), new Color(1f, 0.84f, 0.58f), 2.2f, 10f);
            AddLamp(set.mainHallLights.transform, new Vector3(-16f, 2.8f, 23f), new Color(1f, 0.84f, 0.58f), 2.2f, 10f);
            set.staffLights = set.mainHallLights;

            // Per-wing LIT grade (plan section 10): a local warm volume inside
            // each fuse-controlled group, bounded to the wing's LitZone extents.
            AddWingLitGrade(set.libraryLights, "LitGrade", new Vector3(-12.325f, 1.5f, -15f),
                new Vector3(19.35f, 3.5f, 14f));
            AddWingLitGrade(set.physicsLights, "LitGrade", new Vector3(12.325f, 1.5f, -15f),
                new Vector3(19.35f, 3.5f, 14f));
            AddWingLitGrade(set.mainHallLights, "LitGrade_Hall", new Vector3(0f, 1.5f, 34.5f),
                new Vector3(27.35f, 3.5f, 13.4f));
            AddWingLitGrade(set.mainHallLights, "LitGrade_Staff", new Vector3(-12.325f, 1.5f, 23.5f),
                new Vector3(19.35f, 3.5f, 6.4f));

            // Dawn chase: corridor lights come on with the gate. They must not
            // flicker - DawnRiser owns their intensity while they fade up.
            set.dawnLights = Group("DawnChase", parent);
            for (int i = 0; i < 7; i++)
            {
                float z = -24f + i * 8f;
                AddLamp(set.dawnLights.transform, new Vector3(0f, 2.8f, z), new Color(1f, 0.64f, 0.34f), 1.6f, 11f, false);
            }

            GameObject dawnPlane = Box(set.dawnLights.transform, "DawnPlane", new Vector3(0f, 3f, -36f),
                new Vector3(22f, 8f, 0.2f), palette.dawn, false);
            dawnPlane.transform.localRotation = Quaternion.identity;

            DawnRiser dawnRiser = set.dawnLights.AddComponent<DawnRiser>();
            dawnRiser.glow = dawnPlane.GetComponent<Renderer>();
            dawnRiser.duration = 2.6f;

            set.libraryLights.SetActive(false);
            set.physicsLights.SetActive(false);
            set.mainHallLights.SetActive(false);
            set.dawnLights.SetActive(false);

            // ---- lit zones / checkpoints --------------------------------------
            // The zone's yaw is the respawn facing (CheckpointManager stores the
            // zone transform): each spawn wakes looking into its room, not at
            // world +Z.
            set.entrance = MakeLitZone(parent, "LitZone_Entrance", new Vector3(0f, 0f, -29.5f),
                new Vector3(0f, 1.5f, -0.5f), new Vector3(13.4f, 3f, 5.4f), 0f, true,
                "The entrance light still works. It watched her come in.", true);

            set.library = MakeLitZone(parent, "LitZone_Library", new Vector3(-12f, 0f, -12f),
                new Vector3(-0.325f, 1.5f, -3f), new Vector3(19.35f, 3f, 14f), 90f, true,
                "The library holds its breath. \"Put me back together.\"", false);

            set.physics = MakeLitZone(parent, "LitZone_Physics", new Vector3(11f, 0f, -10f),
                new Vector3(1.325f, 1.5f, -5f), new Vector3(19.35f, 3f, 14f), 270f, true,
                "The lab smells of dust and copper. The light steadies her hands.", false);

            set.staff = MakeLitZone(parent, "LitZone_Staff", new Vector3(-12f, 0f, 21f),
                new Vector3(-0.325f, 1.5f, 2.5f), new Vector3(19.35f, 3f, 6.4f), 90f, true,
                "Her mug is still on the table. The room remembers her.", false);

            set.infirmary = MakeLitZone(parent, "LitZone_Infirmary", new Vector3(-12f, 0f, -2f),
                new Vector3(-0.325f, 1.5f, 0f), new Vector3(19.35f, 3f, 12f), 90f, true,
                "White sheets, cold instruments. She patched a hundred scraped knees here.", false);

            set.mainHall = MakeLitZone(parent, "LitZone_MainHall", new Vector3(0f, 0f, 28f),
                new Vector3(0f, 1.5f, 6.5f), new Vector3(27.35f, 3f, 13.4f), 180f, true,
                "The hall is full of empty seats. The last light is warm.", false);

            return set;
        }

        static LitZone MakeLitZone(Transform parent, string name, Vector3 position, Vector3 center, Vector3 size,
            float spawnYaw, bool checkpoint, string chalkLine, bool active)
        {
            var go = new GameObject(name);
            go.layer = LevelLayer;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(0f, spawnYaw, 0f);

            BoxCollider box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = center;
            box.size = size;

            LitZone zone = go.AddComponent<LitZone>();
            zone.isCheckpoint = checkpoint;
            zone.chalkLine = chalkLine;

            if (!active) go.SetActive(false);
            return zone;
        }

        // --------------------------------------------------------------- props

        static void BuildLibrary(Transform parent, ZoneSet zones, Prefabs prefabs, Palette palette, QuestionDatabase database)
        {
            var room = Group("Library", parent);
            Transform t = room.transform;

            // Shelves.
            Box(t, "Shelf_W1", new Vector3(-21.4f, 1.2f, -20f), new Vector3(0.6f, 2.4f, 3f), palette.wood);
            Box(t, "Shelf_W2", new Vector3(-21.4f, 1.2f, -16.5f), new Vector3(0.6f, 2.4f, 3f), palette.wood);
            Box(t, "Shelf_W3", new Vector3(-21.4f, 1.2f, -13f), new Vector3(0.6f, 2.4f, 3f), palette.wood);
            Box(t, "Shelf_S1", new Vector3(-18f, 1.2f, -21.4f), new Vector3(3f, 2.4f, 0.6f), palette.wood);
            Box(t, "Shelf_S2", new Vector3(-13f, 1.2f, -21.4f), new Vector3(3f, 2.4f, 0.6f), palette.wood);

            AddBookRow(t, new Vector3(-21.05f, 1.9f, -20f), 2.4f, palette);
            AddBookRow(t, new Vector3(-21.05f, 1.2f, -16.5f), 2.4f, palette);

            Box(t, "Rug", new Vector3(-12f, 0.03f, -17f), new Vector3(5.5f, 0.02f, 4f), palette.curtain, false);

            Table(t, "ReadingTable", new Vector3(-12f, 0f, -17f), new Vector3(3.6f, 1.8f), palette.wood);
            Chair(t, "Chair_1", new Vector3(-12f, 0f, -18.3f), 180f, palette.wood);
            Chair(t, "Chair_2", new Vector3(-14.2f, 0f, -17f), 90f, palette.wood);
            Chair(t, "Chair_3", new Vector3(-9.8f, 0f, -17f), -90f, palette.wood);

            Table(t, "MotherDesk", new Vector3(-18f, 0f, -12f), new Vector3(2.6f, 1.2f), palette.wood);
            Box(t, "MemoryBox", new Vector3(-18f, 1.05f, -12f), new Vector3(0.55f, 0.32f, 0.4f), palette.books);

            CreateWorldText(t, "MemoryCard", "\"Put me back together.\"", new Vector3(-18f, 1.3f, -12.3f),
                Quaternion.identity, 0.03f, 48, new Color(0.95f, 0.90f, 0.72f));

            CreateFragment(prefabs, t, "Fragment_Bookmark", new Vector3(-12f, 0.86f, -17f), Quaternion.Euler(0f, 20f, 0f),
                "the pressed-flower bookmark", "Between the pages, a jasmine flower. Mother pressed it the week I started school.",
                QuestionCategory.Language);

            CreateBattery(prefabs, t, "Battery_Library", new Vector3(-19.3f, 0.9f, -12f));
            CreateBoard(prefabs, t, database, "Board_Library", new Vector3(-9f, 1.7f, -8.18f), Quaternion.identity);
            CreateFuse(prefabs, t, "Fuse_Library", new Vector3(-2.88f, 1.55f, -20f), Quaternion.Euler(0f, 90f, 0f),
                zones.libraryLights, new[] { zones.library, zones.infirmary }, "She always said a library card is a key that never rusts.");
            CreateCatalogueSort(prefabs, t, palette);
        }

        static void BuildPhysicsLab(Transform parent, ZoneSet zones, Prefabs prefabs, Palette palette, QuestionDatabase database)
        {
            var room = Group("PhysicsLab", parent);
            Transform t = room.transform;

            Box(t, "Bench_1", new Vector3(8f, 0.45f, -18f), new Vector3(4f, 0.9f, 1.4f), palette.metal);
            Box(t, "Bench_2", new Vector3(14f, 0.45f, -18f), new Vector3(4f, 0.9f, 1.4f), palette.metal);
            Box(t, "Bench_3", new Vector3(8f, 0.45f, -12f), new Vector3(4f, 0.9f, 1.4f), palette.metal);
            Box(t, "Bench_4", new Vector3(14f, 0.45f, -12f), new Vector3(4f, 0.9f, 1.4f), palette.metal);

            Box(t, "Cabinet", new Vector3(20.5f, 1.05f, -9f), new Vector3(1.6f, 2.1f, 2.4f), palette.wood);
            Box(t, "CabinetTop", new Vector3(20.5f, 2.13f, -9f), new Vector3(1.7f, 0.06f, 2.5f), palette.metal, false);

            Box(t, "Shelf_E1", new Vector3(21.4f, 1.2f, -20f), new Vector3(0.6f, 2.4f, 3f), palette.wood);
            Box(t, "Shelf_E2", new Vector3(21.4f, 1.2f, -16.5f), new Vector3(0.6f, 2.4f, 3f), palette.wood);

            // Glassware, purely decorative.
            for (int i = 0; i < 3; i++)
            {
                GameObject beaker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                beaker.name = "Beaker_" + i;
                beaker.transform.SetParent(t, false);
                beaker.transform.localPosition = new Vector3(7f + i * 1.1f, 1.0f, -18f);
                beaker.transform.localScale = new Vector3(0.14f, 0.12f, 0.14f);
                Object.DestroyImmediate(beaker.GetComponent<Collider>());
                beaker.GetComponent<Renderer>().sharedMaterial = palette.water;
            }

            CreateFragment(prefabs, t, "Fragment_Optics", new Vector3(20.5f, 2.18f, -9f), Quaternion.Euler(0f, -90f, 0f),
                "the note in the optics cabinet", "\"Light travels in straight lines - so do stories.\"",
                QuestionCategory.Science);

            CreateBattery(prefabs, t, "Battery_Physics", new Vector3(10f, 0.92f, -18f));
            CreateBoard(prefabs, t, database, "Board_Physics", new Vector3(12f, 1.7f, -21.82f), Quaternion.Euler(0f, 180f, 0f));
            CreateFuse(prefabs, t, "Fuse_Physics", new Vector3(6f, 1.55f, -8.18f), Quaternion.identity,
                zones.physicsLights, new[] { zones.physics }, "In her classroom the chalk still smelled of jasmine.");
            CreateOpticsBench(prefabs, t, palette);
        }

        static void BuildClassroom(Transform parent, Prefabs prefabs, Palette palette, QuestionDatabase database)
        {
            var room = Group("Classroom", parent);
            Transform t = room.transform;

            Box(t, "Chalkboard", new Vector3(-12f, 1.75f, 18.12f), new Vector3(4.4f, 1.9f, 0.08f), palette.chalkboard);
            Box(t, "ChalkFrameTop", new Vector3(-12f, 2.72f, 18.12f), new Vector3(4.6f, 0.08f, 0.14f), palette.wood, false);
            Box(t, "ChalkFrameBottom", new Vector3(-12f, 0.78f, 18.12f), new Vector3(4.6f, 0.08f, 0.14f), palette.wood, false);

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    float x = -18f + col * 4f;
                    float z = 7f + row * 4.5f;
                    Table(t, "Desk_" + row + "_" + col, new Vector3(x, 0f, z), new Vector3(2f, 1f), palette.wood);
                    Chair(t, "Chair_" + row + "_" + col, new Vector3(x, 0f, z - 0.95f), 0f, palette.wood);
                }
            }

            Table(t, "TeacherDesk", new Vector3(-12f, 0f, 16.4f), new Vector3(2.6f, 1.2f), palette.wood);

            CreateFragment(prefabs, t, "Fragment_ExamPaper", new Vector3(-12f, 0.9f, 16.4f), Quaternion.Euler(0f, 12f, 0f),
                "the marked exam paper", "An encouraging note in red pen: \"You see more than you think.\"",
                QuestionCategory.Language);

            CreateBattery(prefabs, t, "Battery_Classroom", new Vector3(-20.2f, 0.05f, 5.2f));
            CreateBoard(prefabs, t, database, "Board_Classroom", new Vector3(-12f, 1.7f, 4.18f), Quaternion.Euler(0f, 180f, 0f));
            CreateChalkboardCipher(prefabs, t, palette);
        }

        static void BuildGarden(Transform parent, Prefabs prefabs, Palette palette, QuestionDatabase database)
        {
            var room = Group("Garden", parent);
            Transform t = room.transform;

            for (int i = 0; i < 3; i++)
            {
                Box(t, "Planter_" + i, new Vector3(20.8f, 0.3f, 6f + i * 4f), new Vector3(1.4f, 0.6f, 2.8f), palette.stone);
                AddBush(t, new Vector3(20.8f, 0.8f, 6f + i * 4f), 0.6f, palette);
            }

            Box(t, "BenchSeat", new Vector3(12f, 0.48f, 10f), new Vector3(3f, 0.14f, 0.7f), palette.wood);
            Box(t, "BenchBack", new Vector3(12f, 0.95f, 10.32f), new Vector3(3f, 0.7f, 0.12f), palette.wood);
            Box(t, "BenchLegL", new Vector3(10.7f, 0.24f, 10f), new Vector3(0.14f, 0.48f, 0.62f), palette.metal, false);
            Box(t, "BenchLegR", new Vector3(13.3f, 0.24f, 10f), new Vector3(0.14f, 0.48f, 0.62f), palette.metal, false);

            AddBush(t, new Vector3(14.8f, 0.55f, 10f), 0.55f, palette);
            AddBush(t, new Vector3(9.2f, 0.45f, 12f), 0.45f, palette);

            CreateFragment(prefabs, t, "Fragment_Jasmine", new Vector3(12f, 0.56f, 10f), Quaternion.Euler(0f, -20f, 0f),
                "the pressed jasmine and photograph", "She kept the photograph of this bench, and the jasmine she picked beside it.",
                QuestionCategory.Science);

            CreateBattery(prefabs, t, "Battery_Garden", new Vector3(19.8f, 0.62f, 6f));
            CreateBoard(prefabs, t, database, "Board_Garden", new Vector3(12f, 1.7f, 18.18f), Quaternion.identity);
        }

        static void BuildStaffRoom(Transform parent, ZoneSet zones, Prefabs prefabs, Palette palette, QuestionDatabase database)
        {
            var room = Group("StaffRoom", parent);
            Transform t = room.transform;

            Table(t, "StaffTable", new Vector3(-12f, 0f, 23f), new Vector3(4f, 1.4f), palette.wood);
            Chair(t, "StaffChair_1", new Vector3(-13.4f, 0f, 21.8f), 0f, palette.wood);
            Chair(t, "StaffChair_2", new Vector3(-10.6f, 0f, 21.8f), 0f, palette.wood);

            Box(t, "Lockers_1", new Vector3(-21.4f, 1.1f, 21f), new Vector3(0.7f, 2.2f, 0.8f), palette.metal);
            Box(t, "Lockers_2", new Vector3(-21.4f, 1.1f, 23f), new Vector3(0.7f, 2.2f, 0.8f), palette.metal);
            Box(t, "Lockers_3", new Vector3(-21.4f, 1.1f, 25f), new Vector3(0.7f, 2.2f, 0.8f), palette.metal);

            CreateFragment(prefabs, t, "Fragment_Mug", new Vector3(-12f, 0.86f, 23f), Quaternion.Euler(0f, 35f, 0f),
                "her mug and the note for a colleague", "\"Keep the light on for the ones who come after.\"",
                QuestionCategory.Health);

            CreateBattery(prefabs, t, "Battery_Staff", new Vector3(-16f, 0.9f, 23f));
            CreateBoard(prefabs, t, database, "Board_Staff", new Vector3(-8f, 1.7f, 20.18f), Quaternion.Euler(0f, 180f, 0f));
            CreateMapLocker(prefabs, t, palette);
        }

        // The infirmary (plan workstream 5, wing puzzle 5): the west strip
        // between the library and the classroom, on the library wing's fuse.
        static void BuildInfirmary(Transform parent, ZoneSet zones, Prefabs prefabs, Palette palette, QuestionDatabase database)
        {
            var room = Group("Infirmary", parent);
            Transform t = room.transform;

            // A cot, a screen, a sink counter.
            Box(t, "Cot", new Vector3(-19f, 0.45f, 1.5f), new Vector3(2f, 0.5f, 0.9f), palette.paper);
            Box(t, "CotLeg_1", new Vector3(-19.8f, 0.2f, 1.5f), new Vector3(0.08f, 0.4f, 0.7f), palette.metal, false);
            Box(t, "CotLeg_2", new Vector3(-18.2f, 0.2f, 1.5f), new Vector3(0.08f, 0.4f, 0.7f), palette.metal, false);
            Box(t, "Screen", new Vector3(-16f, 1.1f, 3.2f), new Vector3(1.6f, 2.2f, 0.08f), palette.curtain);
            Box(t, "SinkCounter", new Vector3(-6f, 0.5f, 3.3f), new Vector3(1.8f, 1f, 0.6f), palette.metal);
            Box(t, "SinkBasin", new Vector3(-6f, 1.02f, 3.3f), new Vector3(0.7f, 0.08f, 0.45f), palette.water, false);

            CreateFragment(prefabs, t, "Fragment_HealthChart", new Vector3(-6f, 1.1f, 3.3f), Quaternion.Euler(0f, -15f, 0f),
                "the hand-drawn health chart", "Her handwriting under a drawing of a bandaged arm: \"First the hands, always the hands.\"",
                QuestionCategory.Health);

            CreateBoard(prefabs, t, database, "Board_Infirmary", new Vector3(-9f, 1.7f, 3.82f), Quaternion.Euler(0f, 180f, 0f));
            CreateFirstAidBoard(prefabs, t, palette);
        }

        static void BuildMainHall(Transform parent, ZoneSet zones, Prefabs prefabs, Palette palette, QuestionDatabase database)
        {
            var room = Group("MainHall", parent);
            Transform t = room.transform;

            Box(t, "Stage", new Vector3(0f, 0.15f, 38.5f), new Vector3(20f, 0.3f, 5f), palette.wood);
            Box(t, "StageBackdrop", new Vector3(0f, 2.3f, 40.7f), new Vector3(20f, 4.2f, 0.25f), palette.curtain);

            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    float x = -8f + col * 4f;
                    float z = 30f + row * 2.4f;
                    Box(t, "Bench_" + row + "_" + col, new Vector3(x, 0.3f, z), new Vector3(2.4f, 0.2f, 0.5f), palette.wood);
                    Box(t, "BenchBack_" + row + "_" + col, new Vector3(x, 0.62f, z + 0.22f),
                        new Vector3(2.4f, 0.5f, 0.08f), palette.wood, false);
                }
            }

            CreateFragment(prefabs, t, "Fragment_Programme", new Vector3(0f, 0.42f, 39f), Quaternion.Euler(0f, -10f, 0f),
                "the poetry-evening programme", "Her name is on the programme, third from the bottom, in small neat print.",
                QuestionCategory.History);

            CreateBattery(prefabs, t, "Battery_MainHall", new Vector3(10f, 0.05f, 29f));
            CreateBoard(prefabs, t, database, "Board_MainHall", new Vector3(-13.82f, 1.7f, 34f), Quaternion.Euler(0f, -90f, 0f));
            CreateFuse(prefabs, t, "Fuse_MainHall", new Vector3(13.82f, 1.55f, 38f), Quaternion.Euler(0f, 90f, 0f),
                zones.mainHallLights, new[] { zones.mainHall, zones.staff }, "The whole hall hummed when her students read aloud.");
            CreateStageRig(prefabs, t, palette);
        }

        static void BuildEntrance(Transform parent, Prefabs prefabs, Palette palette, QuestionDatabase database)
        {
            var room = Group("Entrance", parent);
            Transform t = room.transform;

            Table(t, "ReceptionDesk", new Vector3(-4f, 0f, -28.5f), new Vector3(3f, 1.2f), palette.wood);
            Box(t, "ReceptionInset", new Vector3(-4f, 0.62f, -28.5f), new Vector3(2.6f, 0.5f, 0.06f), palette.metal, false);

            Chair(t, "EntranceChair_1", new Vector3(4f, 0f, -29f), 0f, palette.wood);
            Chair(t, "EntranceChair_2", new Vector3(5.5f, 0f, -29f), 0f, palette.wood);

            CreateBattery(prefabs, t, "Battery_Entrance", new Vector3(5f, 0.05f, -31f));
            CreateBoard(prefabs, t, database, "Board_Entrance", new Vector3(-4.5f, 1.7f, -27.18f), Quaternion.identity);

            CreateWorldText(t, "Notice", "KNOWLEDGE IS LIGHT\n<size=26>answer the boards to charge your flashlight</size>",
                new Vector3(4.6f, 2.2f, -32.72f), Quaternion.Euler(0f, 180f, 0f), 0.04f, 64,
                new Color(0.78f, 0.70f, 0.55f));
        }

        static void BuildGate(Transform parent, ZoneSet zones, Prefabs prefabs, Palette palette, QuestionDatabase database)
        {
            GameObject gate = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.gate);
            gate.name = "ExitGate";
            gate.transform.SetParent(parent, false);
            gate.transform.position = new Vector3(0f, 0f, -33f);

            ExitGate exit = gate.GetComponent<ExitGate>();
            exit.dawnChaseRoot = zones.dawnLights;
            CreateGateCircuit(gate.transform, exit, palette);

            // The console gets a little glow so it is findable in the dark.
            Box(gate.transform, "ConsoleGlow", new Vector3(2.75f, 1.6f, 0.53f), new Vector3(0.4f, 0.06f, 0.04f),
                palette.lamp, false);
        }

        // ------------------------------------------------------ story plaques

        // The rest of the story is written on the school itself: small quotes in
        // her mother's voice, one per room, in the same text style as the wing
        // labels. TextMesh glyphs read from the side the forward (+Z) axis
        // points AWAY from, so each text faces into the room it decorates.
        static void BuildStoryPlaques(Transform parent)
        {
            CreateWorldText(parent, "Story_Classroom",
                "In her classroom the chalk still smelled of jasmine.",
                new Vector3(-15.4f, 1.55f, 4.12f), Quaternion.Euler(0f, 180f, 0f), 0.035f, 48,
                new Color(0.80f, 0.74f, 0.60f));

            CreateWorldText(parent, "Story_Physics",
                "\"Light travels in straight lines - so do stories.\"",
                new Vector3(15.4f, 1.55f, -21.76f), Quaternion.Euler(0f, 180f, 0f), 0.035f, 48,
                new Color(0.80f, 0.74f, 0.60f));

            CreateWorldText(parent, "Story_Garden",
                "She kept the photograph of this bench, and the jasmine she picked beside it.",
                new Vector3(15.4f, 1.55f, 18.12f), Quaternion.identity, 0.035f, 48,
                new Color(0.80f, 0.74f, 0.60f));

            CreateWorldText(parent, "Story_Staff",
                "An encouraging note in red pen: \"You see more than you think.\"",
                new Vector3(-11.4f, 1.55f, 20.12f), Quaternion.Euler(0f, 180f, 0f), 0.035f, 48,
                new Color(0.80f, 0.74f, 0.60f));

            CreateWorldText(parent, "Story_Library",
                "She always said a library card is a key that never rusts.",
                new Vector3(-21.8f, 1.9f, -12.3f), Quaternion.Euler(0f, -90f, 0f), 0.035f, 48,
                new Color(0.80f, 0.74f, 0.60f));

            CreateWorldText(parent, "Story_Programme",
                "Her name is on the programme, third from the bottom, in small neat print.",
                new Vector3(0f, 1.15f, 40.54f), Quaternion.identity, 0.035f, 48,
                new Color(0.80f, 0.74f, 0.60f));

            CreateWorldText(parent, "Story_Keep",
                "\"Keep the light on for the ones who come after.\"",
                new Vector3(0f, 2.85f, 40.54f), Quaternion.identity, 0.04f, 48,
                new Color(0.83f, 0.77f, 0.62f));

            CreateWorldText(parent, "Story_HallHum",
                "The whole hall hummed when her students read aloud.",
                new Vector3(13.8f, 2.2f, 34f), Quaternion.Euler(0f, 90f, 0f), 0.035f, 48,
                new Color(0.80f, 0.74f, 0.60f));
        }

        // ------------------------------------------------- hiding (P3, plan 4.12)

        // P3 hiding v1 (plan 4.12): six hide spots. Two lockers lean on the
        // east corridor wall (new greybox props, baked into the NavMesh), two
        // hide under the existing teacher desks (classroom + staff room), two
        // peek from the main-hall stage curtains. Each spot is a thin solid
        // plate the interact ray can hit plus a hidePoint with capsule
        // clearance; EnemySight treats a hidden player as unseen.
        static void BuildHideSpots(Transform parent, Palette palette)
        {
            var spots = Group("HideSpots", parent);
            Transform t = spots.transform;

            // The two corridor lockers themselves (Level layer: the NavMesh
            // bake routes the Shadow around them).
            Box(t, "Locker_Corridor_1", new Vector3(2.1f, 1.1f, -5f), new Vector3(0.7f, 2.2f, 1.0f), palette.metal);
            Box(t, "Locker_Corridor_2", new Vector3(2.1f, 1.1f, 8f), new Vector3(0.7f, 2.2f, 1.0f), palette.metal);

            // (position, yaw, interact plate centre, plate size, hidePoint local, display name)
            CreateHideSpot(t, "Hide_Locker_1", new Vector3(1.73f, 0f, -5f), 0f,
                new Vector3(0f, 1.05f, 0f), new Vector3(0.1f, 2.1f, 1.0f),
                new Vector3(-0.45f, 0f, 0f), "the corridor locker");
            CreateHideSpot(t, "Hide_Locker_2", new Vector3(1.73f, 0f, 8f), 0f,
                new Vector3(0f, 1.05f, 0f), new Vector3(0.1f, 2.1f, 1.0f),
                new Vector3(-0.45f, 0f, 0f), "the corridor locker");
            CreateHideSpot(t, "Hide_Desk_Classroom", new Vector3(-12f, 0f, 15.76f), 0f,
                new Vector3(0f, 1.0f, 0.05f), new Vector3(2.4f, 2.0f, 0.12f),
                new Vector3(0f, 0f, -0.42f), "the teacher's desk");
            CreateHideSpot(t, "Hide_Desk_Staff", new Vector3(-12f, 0f, 22.26f), 0f,
                new Vector3(0f, 1.0f, 0.05f), new Vector3(3.6f, 2.0f, 0.12f),
                new Vector3(0f, 0f, -0.45f), "the staff-room desk");
            CreateHideSpot(t, "Hide_Curtain_Left", new Vector3(-6.5f, 0.3f, 40.5f), 180f,
                new Vector3(0f, 1.8f, 0.12f), new Vector3(2.6f, 3.6f, 0.14f),
                new Vector3(0f, 0f, -0.62f), "the stage curtain");
            CreateHideSpot(t, "Hide_Curtain_Right", new Vector3(6.5f, 0.3f, 40.5f), 180f,
                new Vector3(0f, 1.8f, 0.12f), new Vector3(2.6f, 3.6f, 0.14f),
                new Vector3(0f, 0f, -0.62f), "the stage curtain");
        }

        static void CreateHideSpot(Transform parent, string name, Vector3 position, float yaw,
            Vector3 plateCenter, Vector3 plateSize, Vector3 hidePointLocal, string displayName)
        {
            GameObject spot = new GameObject(name);
            spot.transform.SetParent(parent, false);
            spot.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));

            // Solid thin plate: the interact ray (triggers ignored) needs it,
            // and it doubles as sight cover hugging the prop's face.
            BoxCollider plate = spot.AddComponent<BoxCollider>();
            plate.center = plateCenter;
            plate.size = plateSize;

            HideSpot hide = spot.AddComponent<HideSpot>();
            hide.displayName = displayName;

            GameObject point = new GameObject("HidePoint");
            point.transform.SetParent(spot.transform, false);
            point.transform.localPosition = hidePointLocal;
            hide.hidePoint = point.transform;
        }

        static GameObject Table(Transform parent, string name, Vector3 position, Vector3 size, Material material)
        {
            var group = new GameObject(name);
            group.transform.SetParent(parent, false);
            group.transform.localPosition = position;

            Box(group.transform, "Top", new Vector3(0f, 0.78f, 0f), new Vector3(size.x, 0.1f, size.y), material);
            Box(group.transform, "Leg1", new Vector3(-size.x * 0.45f, 0.39f, -size.y * 0.42f),
                new Vector3(0.08f, 0.78f, 0.08f), material, false);
            Box(group.transform, "Leg2", new Vector3(size.x * 0.45f, 0.39f, -size.y * 0.42f),
                new Vector3(0.08f, 0.78f, 0.08f), material, false);
            Box(group.transform, "Leg3", new Vector3(-size.x * 0.45f, 0.39f, size.y * 0.42f),
                new Vector3(0.08f, 0.78f, 0.08f), material, false);
            Box(group.transform, "Leg4", new Vector3(size.x * 0.45f, 0.39f, size.y * 0.42f),
                new Vector3(0.08f, 0.78f, 0.08f), material, false);

            return group;
        }

        static void Chair(Transform parent, string name, Vector3 position, float yaw, Material material)
        {
            var group = new GameObject(name);
            group.transform.SetParent(parent, false);
            group.transform.localPosition = position;
            group.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Box(group.transform, "Seat", new Vector3(0f, 0.46f, 0f), new Vector3(0.62f, 0.08f, 0.62f), material);
            Box(group.transform, "Back", new Vector3(0f, 0.82f, -0.28f), new Vector3(0.62f, 0.66f, 0.08f), material, false);
            Box(group.transform, "Leg1", new Vector3(-0.24f, 0.23f, -0.24f), new Vector3(0.07f, 0.46f, 0.07f), material, false);
            Box(group.transform, "Leg2", new Vector3(0.24f, 0.23f, -0.24f), new Vector3(0.07f, 0.46f, 0.07f), material, false);
            Box(group.transform, "Leg3", new Vector3(-0.24f, 0.23f, 0.24f), new Vector3(0.07f, 0.46f, 0.07f), material, false);
            Box(group.transform, "Leg4", new Vector3(0.24f, 0.23f, 0.24f), new Vector3(0.07f, 0.46f, 0.07f), material, false);
        }

        static void AddBookRow(Transform parent, Vector3 position, float width, Palette palette)
        {
            int count = Mathf.Max(3, Mathf.FloorToInt(width / 0.22f));
            for (int i = 0; i < count; i++)
            {
                float offset = -width * 0.5f + (i + 0.5f) * (width / count);
                Box(parent, "Book", position + new Vector3(offset, 0f, 0f),
                    new Vector3(0.16f, 0.34f, 0.22f), palette.books, false, LevelLayer);
            }
        }

        static void AddBush(Transform parent, Vector3 position, float radius, Palette palette)
        {
            GameObject bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bush.name = "Bush";
            bush.transform.SetParent(parent, false);
            bush.transform.localPosition = position;
            bush.transform.localScale = new Vector3(radius * 2f, radius * 1.6f, radius * 2f);
            Object.DestroyImmediate(bush.GetComponent<Collider>());
            bush.GetComponent<Renderer>().sharedMaterial = palette.grass;
        }

        // ---------------------------------------------------------- interactables

        static void CreateBoard(Prefabs prefabs, Transform parent, QuestionDatabase database, string name,
            Vector3 position, Quaternion rotation)
        {
            GameObject board = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.quizBoard);
            board.name = name;
            board.transform.SetParent(parent, false);
            board.transform.SetPositionAndRotation(position, rotation);
            board.GetComponent<QuizBoard>().database = database;
        }

        static void CreateFuse(Prefabs prefabs, Transform parent, string name, Vector3 position, Quaternion rotation,
            GameObject lightsRoot, LitZone[] zones, string memoryFlash)
        {
            if (lightsRoot == null && zones == null && memoryFlash == null)
            {
                // The staff-room fuse is the main-hall fuse; this call is a no-op
                // so the room builder can keep its symmetrical shape.
                return;
            }

            GameObject fuse = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.fuseBox);
            fuse.name = name;
            fuse.transform.SetParent(parent, false);
            fuse.transform.SetPositionAndRotation(position, rotation);

            FuseBox box = fuse.GetComponent<FuseBox>();
            box.lightsRoot = lightsRoot;
            box.wingZones = zones;
            box.memoryFlash = memoryFlash;
        }

        static void CreateFragment(Prefabs prefabs, Transform parent, string name, Vector3 position, Quaternion rotation,
            string fragmentName, string fragmentText, QuestionCategory category)
        {
            GameObject fragment = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.fragment);
            fragment.name = name;
            fragment.transform.SetParent(parent, false);
            fragment.transform.SetPositionAndRotation(position, rotation);

            KnowledgeFragment knowledge = fragment.GetComponent<KnowledgeFragment>();
            knowledge.fragmentName = fragmentName;
            knowledge.fragmentText = fragmentText;
            knowledge.category = category;
            knowledge.memorySeconds = 4f;
        }

        static void CreateBattery(Prefabs prefabs, Transform parent, string name, Vector3 position)
        {
            GameObject battery = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.battery);
            battery.name = name;
            battery.transform.SetParent(parent, false);
            battery.transform.position = position;
        }

        // ------------------------------------------------------------- navigation

        static PatrolRoute BuildPatrolRoute(Transform parent)
        {
            var routeGo = new GameObject("PatrolRoute");
            routeGo.transform.SetParent(parent, false);
            PatrolRoute route = routeGo.AddComponent<PatrolRoute>();

            float[] zs = { 33f, 26f, 18f, 10f, 2f, -6f, -14f, -22f };
            for (int i = 0; i < zs.Length; i++)
            {
                GameObject marker = Empty(routeGo.transform, "WP_" + i, new Vector3(0f, 0f, zs[i]), Quaternion.identity);
                route.waypoints.Add(marker.transform);
            }

            return route;
        }

        static void BakeNavMesh(NavMeshSurface surface)
        {
            try
            {
                surface.BuildNavMesh();

                if (surface.navMeshData == null)
                {
                    Debug.LogWarning("[LightShadow] NavMesh bake returned no data; the runtime bootstrap will build it.");
                    return;
                }

                if (AssetDatabase.Contains(surface.navMeshData)) return;

                AssetDatabase.DeleteAsset(NavMeshPath);
                AssetDatabase.CreateAsset(surface.navMeshData, NavMeshPath);
                surface.navMeshData = AssetDatabase.LoadAssetAtPath<NavMeshData>(NavMeshPath);
                Debug.Log("[LightShadow] NavMesh baked to " + NavMeshPath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] NavMesh bake failed, runtime fallback will run: " + exception.Message);
            }
        }

        static AudioSource CreateAudioSource(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            return source;
        }

        // ------------------------------------------------------- audio mixer --

        // Idempotent attach of the runtime snapshot driver with the mixer asset
        // wired (AGENT_REPORT_AUDIO_MIXER §4.2/§4.5). AudioSnapshotController
        // guards duplicates itself (first instance wins), so attaching to both
        // the bootstrap and the school Systems is safe.
        static void AttachAudioSnapshot(Transform parent)
        {
            if (parent == null) return;
            LightShadow.Audio.AudioSnapshotController controller =
                parent.GetComponent<LightShadow.Audio.AudioSnapshotController>();
            if (controller == null)
            {
                controller = parent.gameObject.AddComponent<LightShadow.Audio.AudioSnapshotController>();
            }
            controller.mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(AudioMixerTool.MixerPath);
        }

        // ------------------------------------------------------- reverb zones --

        // Interior rooms and their floor bounds, mirroring the shell walls laid
        // out in BuildSchoolShell (same literals: the wall spans define the x/z
        // extents, the height comes from the matching Ceiling call). Presets are
        // the premium-pass brief §8.3 acoustic tiers: small dry rooms, medium
        // circulation spaces, one long assembly hall.
        struct ReverbRoom
        {
            public string name;
            public float minX, maxX, minZ, maxZ;
            public float height;
            public AudioReverbPreset preset;
        }

        static readonly ReverbRoom[] ReverbRooms =
        {
            // Small/dry: shelved, furnished or outdoor rooms barely ring.
            new ReverbRoom { name = "Library",    minX = -22f,  maxX = -2.65f, minZ = -22f, maxZ = -8f,  height = 3.3f, preset = AudioReverbPreset.Room },
            new ReverbRoom { name = "PhysicsLab", minX = 2.65f, maxX = 22f,    minZ = -22f, maxZ = -8f,  height = 3.3f, preset = AudioReverbPreset.Room },
            new ReverbRoom { name = "Classroom",  minX = -22f,  maxX = -2.65f, minZ = 4f,   maxZ = 18f,  height = 3.3f, preset = AudioReverbPreset.Room },
            new ReverbRoom { name = "Garden",     minX = 2.65f, maxX = 22f,    minZ = 4f,   maxZ = 18f,  height = 3.3f, preset = AudioReverbPreset.Room },
            new ReverbRoom { name = "StaffRoom",  minX = -22f,  maxX = -2.65f, minZ = 20f,  maxZ = 26f,  height = 3.3f, preset = AudioReverbPreset.Room },
            // Medium: circulation spaces (StoneCorridor is the report §5 medium
            // preset; Unity's enum has no "MediumRoom" member).
            new ReverbRoom { name = "Corridor",   minX = -2.65f, maxX = 2.65f, minZ = -27f, maxZ = 27f,  height = 3.3f, preset = AudioReverbPreset.StoneCorridor },
            new ReverbRoom { name = "Entrance",   minX = -7f,    maxX = 7f,    minZ = -33f, maxZ = -27f, height = 3.3f, preset = AudioReverbPreset.StoneCorridor },
            // Long: the assembly hall rings like an empty concert space
            // (Unity spells the preset "Concerthall").
            new ReverbRoom { name = "MainHall",   minX = -14f,   maxX = 14f,   minZ = 27f,  maxZ = 41f,  height = 4.8f, preset = AudioReverbPreset.Concerthall },
        };

        // Attaches one AudioReverbZone per interior room. Idempotent: an existing
        // ReverbZone child is left alone (same guard style as AttachAudioSnapshot).
        // Falloff comes from the room bounds: minDistance = half of the smaller
        // floor extent (the fully-wet radius fits inside the room in both
        // directions), maxDistance = the full floor diagonal. The zone collider
        // is a trigger on the Default layer — NOT LevelLayer — so the NavMesh
        // bake (layerMask 1 << LevelLayer, physics colliders) never collects it,
        // and every project query (interaction ray, EnemySight, the occlusion
        // probe) passes QueryTriggerInteraction.Ignore, so it blocks no gameplay
        // ray. Overlapping zones blend, which smooths doorway transitions.
        static void AttachReverbZones(Transform levelRoot)
        {
            for (int i = 0; i < ReverbRooms.Length; i++)
            {
                ReverbRoom room = ReverbRooms[i];
                Transform roomRoot = levelRoot.Find(room.name);
                if (roomRoot == null)
                {
                    // The corridor is shell geometry with no room builder; the
                    // zone gets its own group so the graph stays named like the
                    // other rooms.
                    roomRoot = Group(room.name, levelRoot).transform;
                }

                if (roomRoot.Find("ReverbZone") != null) continue;

                var zoneGo = new GameObject("ReverbZone");
                zoneGo.transform.SetParent(roomRoot, false);
                zoneGo.layer = 0; // Default: see the method comment (bake safety)

                float extentX = room.maxX - room.minX;
                float extentZ = room.maxZ - room.minZ;
                zoneGo.transform.localPosition =
                    new Vector3((room.minX + room.maxX) * 0.5f, 0f, (room.minZ + room.maxZ) * 0.5f);

                BoxCollider bounds = zoneGo.AddComponent<BoxCollider>();
                bounds.isTrigger = true;
                bounds.center = new Vector3(0f, room.height * 0.5f, 0f);
                bounds.size = new Vector3(extentX, room.height, extentZ);

                AudioReverbZone reverb = zoneGo.AddComponent<AudioReverbZone>();
                reverb.reverbPreset = room.preset;
                reverb.minDistance = Mathf.Min(extentX, extentZ) * 0.5f;
                reverb.maxDistance = Mathf.Sqrt(extentX * extentX + extentZ * extentZ);
            }
        }
    }
}
