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
            Canvas canvas = SceneBuilderUI.BuildBootstrapUI();

            var bootstrap = new GameObject("Bootstrap");
            bootstrap.AddComponent<BootstrapLoader>();

            SaveScene(scene, BootstrapPath);
        }

        // =============================================================== menu

        static void BuildMainMenuScene()
        {
            Scene scene = NewScene();
            ApplyAtmosphere(new Color(0.02f, 0.02f, 0.03f), Color.black, 0.02f);

            CreateCamera("MenuCamera", new Vector3(0f, 0f, -10f), Quaternion.identity,
                new Color(0.012f, 0.014f, 0.019f));
            EnsureEventSystem();
            SceneBuilderUI.BuildMainMenuUI();

            SaveScene(scene, MainMenuPath);
        }

        // ============================================================== ending

        static void BuildEndingScene()
        {
            Scene scene = NewScene();
            ApplyAtmosphere(new Color(0.05f, 0.045f, 0.04f), new Color(0.08f, 0.06f, 0.045f), 0.01f);

            CreateCamera("EndingCamera", new Vector3(0f, 0f, -10f), Quaternion.identity,
                new Color(0.07f, 0.055f, 0.045f));
            EnsureEventSystem();
            SceneBuilderUI.BuildEndingUI();

            SaveScene(scene, EndingPath);
        }

        // ============================================================== school

        static void BuildSchoolScene(GameConfig config, QuestionDatabase database, Palette palette, Prefabs prefabs)
        {
            Scene scene = NewScene();
            ApplyAtmosphere(new Color(0.030f, 0.033f, 0.045f), new Color(0.020f, 0.024f, 0.034f), 0.020f);

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
            BuildMainHall(level.transform, zones, prefabs, palette, database);
            BuildEntrance(level.transform, prefabs, palette, database);
            BuildGate(level.transform, zones, prefabs, palette, database);

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

            AudioSource music = CreateAudioSource(systems.transform, "MusicSource");
            AudioSource ambience = CreateAudioSource(systems.transform, "AmbienceSource");
            AudioSource sfx = CreateAudioSource(systems.transform, "SfxSource");
            audio.musicSource = music;
            audio.ambienceSource = ambience;
            audio.sfxSource = sfx;

            // The brightness option acts through URP post-exposure and ambient lift.
            systems.AddComponent<BrightnessController>();

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

            SaveScene(scene, SchoolPath);
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
            WallX(parent, -2.65f, -14f, 10f, 3.2f, palette.wall);
            WallX(parent, -2.65f, 12f, 22f, 3.2f, palette.wall);
            WallX(parent, -2.65f, 24f, 27f, 3.2f, palette.wall);

            WallX(parent, 2.65f, -27f, -16f, 3.2f, palette.wall);
            WallX(parent, 2.65f, -14f, 10f, 3.2f, palette.wall);
            WallX(parent, 2.65f, 12f, 27f, 3.2f, palette.wall);

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
            AddLamp(lights.transform, new Vector3(-3f, 2.85f, -30f), new Color(1f, 0.83f, 0.58f), 2.8f, 10f);
            AddLamp(lights.transform, new Vector3(3f, 2.85f, -30f), new Color(1f, 0.83f, 0.58f), 2.8f, 10f);
            CreatePointLight(lights.transform, "GateGlow", new Vector3(0f, 2.6f, -33.5f),
                new Color(1f, 0.72f, 0.42f), 1.2f, 7f);

            // A pair of weak blue emergency lights keep the corridor navigable.
            AddLamp(lights.transform, new Vector3(0f, 2.9f, -20f), new Color(0.38f, 0.48f, 0.70f), 0.42f, 7f);
            AddLamp(lights.transform, new Vector3(0f, 2.9f, 20f), new Color(0.38f, 0.48f, 0.70f), 0.42f, 7f);

            // Garden moonlight.
            CreatePointLight(lights.transform, "GardenMoon", new Vector3(12f, 5f, 11f),
                new Color(0.52f, 0.64f, 0.90f), 0.8f, 24f);

            // ---- fuse-controlled light groups ---------------------------------
            set.libraryLights = Group("LibraryLights", parent);
            AddLamp(set.libraryLights.transform, new Vector3(-8f, 2.8f, -19f), new Color(1f, 0.80f, 0.52f), 2.6f, 12f);
            AddLamp(set.libraryLights.transform, new Vector3(-16f, 2.8f, -14f), new Color(1f, 0.80f, 0.52f), 2.6f, 12f);
            AddLamp(set.libraryLights.transform, new Vector3(-8f, 2.8f, -10f), new Color(1f, 0.80f, 0.52f), 2.6f, 12f);

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

            // Dawn chase: corridor lights come on with the gate.
            set.dawnLights = Group("DawnChase", parent);
            for (int i = 0; i < 7; i++)
            {
                float z = -24f + i * 8f;
                AddLamp(set.dawnLights.transform, new Vector3(0f, 2.8f, z), new Color(1f, 0.64f, 0.34f), 1.6f, 11f);
            }

            Box(set.dawnLights.transform, "DawnPlane", new Vector3(0f, 3f, -36f), new Vector3(22f, 8f, 0.2f),
                palette.dawn, false).transform.localRotation = Quaternion.identity;

            set.libraryLights.SetActive(false);
            set.physicsLights.SetActive(false);
            set.mainHallLights.SetActive(false);
            set.dawnLights.SetActive(false);

            // ---- lit zones / checkpoints --------------------------------------
            set.entrance = MakeLitZone(parent, "LitZone_Entrance", new Vector3(0f, 0f, -29.5f),
                new Vector3(0f, 1.5f, -0.5f), new Vector3(13.4f, 3f, 5.4f), true,
                "The entrance light still works. It watched her come in.", true);

            set.library = MakeLitZone(parent, "LitZone_Library", new Vector3(-12f, 0f, -12f),
                new Vector3(-0.325f, 1.5f, -3f), new Vector3(19.35f, 3f, 14f), true,
                "The library holds its breath. \"Put me back together.\"", false);

            set.physics = MakeLitZone(parent, "LitZone_Physics", new Vector3(11f, 0f, -10f),
                new Vector3(1.325f, 1.5f, -5f), new Vector3(19.35f, 3f, 14f), true,
                "The lab smells of dust and copper. The light steadies her hands.", false);

            set.staff = MakeLitZone(parent, "LitZone_Staff", new Vector3(-12f, 0f, 21f),
                new Vector3(-0.325f, 1.5f, 2.5f), new Vector3(19.35f, 3f, 6.4f), true,
                "Her mug is still on the table. The room remembers her.", false);

            set.mainHall = MakeLitZone(parent, "LitZone_MainHall", new Vector3(0f, 0f, 28f),
                new Vector3(0f, 1.5f, 6.5f), new Vector3(27.35f, 3f, 13.4f), true,
                "The hall is full of empty seats. The last light is warm.", false);

            return set;
        }

        static LitZone MakeLitZone(Transform parent, string name, Vector3 position, Vector3 center, Vector3 size,
            bool checkpoint, string chalkLine, bool active)
        {
            var go = new GameObject(name);
            go.layer = LevelLayer;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;

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
                zones.libraryLights, new[] { zones.library }, "She always said a library card is a key that never rusts.");
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

            // The console gets a little glow so it is findable in the dark.
            Box(gate.transform, "ConsoleGlow", new Vector3(2.75f, 1.6f, 0.53f), new Vector3(0.4f, 0.06f, 0.04f),
                palette.lamp, false);
        }

        // ------------------------------------------------------------ furniture

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
    }
}
