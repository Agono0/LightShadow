using System;
using System.Collections.Generic;
using System.IO;
using LightShadow.Core;
using LightShadow.Enemy;
using LightShadow.Gameplay;
using LightShadow.Player;
using LightShadow.UI;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Scene = UnityEngine.SceneManagement.Scene;

namespace LightShadow.Tools
{
    // Builds every piece of content the scripts expect: tuning asset, materials,
    // prefabs, HUD, and the four scenes (Bootstrap, MainMenu, School, Ending).
    // This is the "LightShadow > 3. Build Scenes" step promised by the README.
    //
    // Everything here is editor-only and idempotent: running it again rebuilds
    // the scenes from code, which is exactly what a greybox level needs.
    public static partial class SceneBuilder
    {
        public const int PlayerLayer = 8;
        public const int EnemyLayer = 9;
        public const int InteractableLayer = 10;
        public const int LevelLayer = 11;

        const string DataFolder = "Assets/_Project/Data";
        const string QuestionsFolder = DataFolder + "/Questions";
        const string DatabasePath = QuestionsFolder + "/QuestionDatabase.asset";
        const string ConfigPath = DataFolder + "/GameConfig.asset";
        const string NavMeshPath = DataFolder + "/NavMesh_School.asset";
        const string MaterialsFolder = "Assets/_Project/Materials";
        const string PrefabsFolder = "Assets/_Project/Prefabs";
        const string ScenesFolder = "Assets/_Project/Scenes";
        const string BootstrapPath = ScenesFolder + "/00_Bootstrap.unity";
        const string MainMenuPath = ScenesFolder + "/01_MainMenu.unity";
        const string SchoolPath = ScenesFolder + "/02_School.unity";
        const string EndingPath = ScenesFolder + "/03_Ending.unity";

        // ---------------------------------------------------------------- entry

        [MenuItem("LightShadow/3. Build Scenes", priority = 3)]
        public static void BuildScenesMenu()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string summary = BuildAll();
            EditorUtility.DisplayDialog("LightShadow - Build Scenes", summary, "OK");
        }

        [MenuItem("LightShadow/4. Build Windows Player", priority = 4)]
        public static void BuildWindowsMenu()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string result = BuildWindowsPlayer();
            EditorUtility.DisplayDialog("LightShadow - Windows Build", result, "OK");
        }

        [MenuItem("LightShadow/5. Validate Content", priority = 5)]
        public static void ValidateMenu()
        {
            string report = ValidateScenes();
            EditorUtility.DisplayDialog("LightShadow - Content Validation", report, "OK");
        }

        [MenuItem("LightShadow/6. Build Android APK", priority = 6)]
        public static void BuildAndroidMenu()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            string result = BuildAndroidPlayer();
            EditorUtility.DisplayDialog("LightShadow - Android Build", result, "OK");
        }

        // Called by Unity in batch mode: -executeMethod LightShadow.Tools.SceneBuilder.BuildAllBatch
        public static void BuildAllBatch()
        {
            try
            {
                string summary = BuildAll();
                Debug.Log("[LightShadow] " + summary);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void BuildWindowsBatch()
        {
            try
            {
                string result = BuildWindowsPlayer();
                Debug.Log("[LightShadow] " + result);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void BuildAndroidBatch()
        {
            try
            {
                string result = BuildAndroidPlayer();
                Debug.Log("[LightShadow] " + result);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void ValidateBatch()
        {
            try
            {
                string report = ValidateScenes();
                Debug.Log("[LightShadow] VALIDATION\n" + report);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        // --------------------------------------------------------------- build

        static string BuildAll()
        {
            ConfigureProjectSettings();
            EnsureFolder(DataFolder);
            EnsureFolder(QuestionsFolder);
            EnsureFolder(MaterialsFolder);
            EnsureFolder(PrefabsFolder);
            EnsureFolder(ScenesFolder);

            GameConfig config = EnsureConfig();

            // The school cannot be answered without questions.
            string questionSummary = QuestionGenerator.GenerateQuestionsSilent();
            QuestionDatabase database = AssetDatabase.LoadAssetAtPath<QuestionDatabase>(DatabasePath);
            if (database == null)
            {
                throw new InvalidOperationException("QuestionDatabase was not created at " + DatabasePath);
            }

            Palette palette = BuildPalette();
            Prefabs prefabs = BuildPrefabs(config, palette);

            BuildBootstrapScene();
            BuildMainMenuScene();
            BuildSchoolScene(config, database, palette, prefabs);
            BuildEndingScene();

            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return "Scenes built:\n- " + BootstrapPath + "\n- " + MainMenuPath + "\n- " + SchoolPath + "\n- " + EndingPath
                + "\n\n" + questionSummary
                + "\n\nPress Play from 00_Bootstrap or open any scene and press Play.";
        }

        // ------------------------------------------------------------- project

        static void ConfigureProjectSettings()
        {
            PlayerSettings.companyName = "LightShadow Team";
            PlayerSettings.productName = "LightShadow";
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = true;

            // The project uses the classic Input class everywhere (mouse, WASD,
            // touch). Active Input Handling must therefore be "Both".
            SetActiveInputHandlingBoth();

            // Android target from project document 6.8. Skip the whole block if
            // the Android module is not installed on this machine - the Windows
            // build and the scenes must not depend on a phone SDK being present.
            try
            {
                if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
                {
                    PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
                    PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.lightshadowteam.lightshadow");
                    PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                    PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
                    PlayerSettings.allowedAutorotateToPortrait = false;
                    PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
                    PlayerSettings.allowedAutorotateToLandscapeLeft = true;
                    PlayerSettings.allowedAutorotateToLandscapeRight = true;
                }
                else
                {
                    Debug.LogWarning("[LightShadow] Android Build Support is not installed; skipping Android player settings.");
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Could not apply Android player settings: " + exception.Message);
            }

            EnsureLayer(PlayerLayer, "Player");
            EnsureLayer(EnemyLayer, "Enemy");
            EnsureLayer(InteractableLayer, "Interactable");
            EnsureLayer(LevelLayer, "Level");
        }

        static void SetActiveInputHandlingBoth()
        {
            Object[] settingsAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (settingsAssets == null || settingsAssets.Length == 0) return;

            var serialized = new SerializedObject(settingsAssets[0]);
            SerializedProperty handler = serialized.FindProperty("activeInputHandler");
            if (handler == null || handler.intValue == 2) return;

            handler.intValue = 2;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log("[LightShadow] Active Input Handling set to Both. Restart Unity before playing.");
        }

        static void EnsureLayer(int index, string name)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0) return;

            var serialized = new SerializedObject(assets[0]);
            SerializedProperty layers = serialized.FindProperty("layers");
            if (layers == null || index >= layers.arraySize) return;

            SerializedProperty layer = layers.GetArrayElementAtIndex(index);
            if (layer.stringValue == name) return;
            if (!string.IsNullOrEmpty(layer.stringValue))
            {
                Debug.LogWarning("[LightShadow] Layer " + index + " was \"" + layer.stringValue + "\", replacing with \"" + name + "\".");
            }

            layer.stringValue = name;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameConfig EnsureConfig()
        {
            GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
            if (config != null) return config;

            config = ScriptableObject.CreateInstance<GameConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
            return config;
        }

        static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootstrapPath, true),
                new EditorBuildSettingsScene(MainMenuPath, true),
                new EditorBuildSettingsScene(SchoolPath, true),
                new EditorBuildSettingsScene(EndingPath, true)
            };
        }

        static string BuildWindowsPlayer()
        {
            EnsureFolder("Builds");
            EnsureFolder("Builds/Windows");

            var options = new BuildPlayerOptions
            {
                scenes = new[] { BootstrapPath, MainMenuPath, SchoolPath, EndingPath },
                locationPathName = "Builds/Windows/LightShadow.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                return "Windows build " + summary.result + " with " + summary.totalErrors + " error(s).\n"
                    + "See the Console for details.";
            }

            return "Windows build succeeded.\n"
                + "Location: Builds/Windows/LightShadow.exe\n"
                + "Size: " + (summary.totalSize / (1024f * 1024f)).ToString("0.0") + " MB\n"
                + "Time: " + summary.totalTime.TotalSeconds.ToString("0") + " s";
        }

        static string BuildAndroidPlayer()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                return "Android Build Support is not installed.\n"
                    + "Install the Android module in Unity Hub (Windows module is unaffected).";
            }

            EnsureFolder("Builds");
            EnsureFolder("Builds/Android");

            var options = new BuildPlayerOptions
            {
                scenes = new[] { BootstrapPath, MainMenuPath, SchoolPath, EndingPath },
                locationPathName = "Builds/Android/LightShadow.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                return "Android build " + summary.result + " with " + summary.totalErrors + " error(s).\n"
                    + "Check the SDK/NDK/JDK paths in Unity Hub > Installs > Add modules.";
            }

            return "Android build succeeded.\n"
                + "Location: Builds/Android/LightShadow.apk\n"
                + "Size: " + (summary.totalSize / (1024f * 1024f)).ToString("0.0") + " MB\n"
                + "Minimum API: Android 10 (API 29), ARM64, IL2CPP";
        }

        // ------------------------------------------------------------- palette

        public sealed class Palette
        {
            public Material floor;
            public Material wall;
            public Material ceiling;
            public Material wood;
            public Material metal;
            public Material chalkboard;
            public Material grass;
            public Material lamp;
            public Material paper;
            public Material battery;
            public Material fuse;
            public Material gate;
            public Material shadow;
            public Material dawn;
            public Material indicatorOff;
            public Material indicatorOn;
            public Material stone;
            public Material curtain;
            public Material books;
            public Material water;
        }

        static Palette BuildPalette()
        {
            return new Palette
            {
                floor = MakeLit("Mat_Floor", new Color(0.20f, 0.19f, 0.17f), 0.15f),
                wall = MakeLit("Mat_Wall", new Color(0.32f, 0.30f, 0.26f), 0.08f),
                ceiling = MakeLit("Mat_Ceiling", new Color(0.12f, 0.12f, 0.12f), 0.05f),
                wood = MakeLit("Mat_Wood", new Color(0.24f, 0.15f, 0.09f), 0.22f),
                metal = MakeLit("Mat_Metal", new Color(0.16f, 0.17f, 0.19f), 0.55f, 0.75f),
                chalkboard = MakeLit("Mat_Chalkboard", new Color(0.035f, 0.075f, 0.06f), 0.1f),
                grass = MakeLit("Mat_Grass", new Color(0.10f, 0.18f, 0.08f), 0.05f),
                lamp = MakeUnlit("Mat_Lamp", new Color(1f, 0.82f, 0.55f) * 1.7f),
                paper = MakeLit("Mat_Paper", new Color(0.82f, 0.79f, 0.68f), 0.05f),
                battery = MakeLit("Mat_Battery", new Color(0.75f, 0.55f, 0.12f), 0.45f, 0.3f),
                fuse = MakeLit("Mat_Fuse", new Color(0.20f, 0.27f, 0.22f), 0.4f, 0.4f),
                gate = MakeLit("Mat_Gate", new Color(0.11f, 0.12f, 0.13f), 0.6f, 0.85f),
                shadow = MakeLit("Mat_Shadow", new Color(0.008f, 0.007f, 0.012f), 0.05f, 0f,
                    new Color(0.018f, 0.005f, 0.030f)),
                dawn = MakeUnlit("Mat_Dawn", new Color(1f, 0.55f, 0.28f) * 2.2f),
                indicatorOff = MakeUnlit("Mat_IndicatorOff", new Color(0.9f, 0.08f, 0.05f)),
                indicatorOn = MakeUnlit("Mat_IndicatorOn", new Color(0.15f, 1f, 0.35f)),
                stone = MakeLit("Mat_Stone", new Color(0.28f, 0.27f, 0.25f), 0.1f),
                curtain = MakeLit("Mat_Curtain", new Color(0.28f, 0.05f, 0.05f), 0.1f),
                books = MakeLit("Mat_Books", new Color(0.35f, 0.18f, 0.10f), 0.1f),
                water = MakeLit("Mat_Water", new Color(0.08f, 0.18f, 0.25f), 0.85f, 0.1f)
            };
        }

        static Material MakeLit(string name, Color color, float smoothness, float metallic = 0f, Color? emission = null)
        {
            Material material = LoadOrCreateMaterial(name, "Universal Render Pipeline/Lit");
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", metallic);

            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        static Material MakeUnlit(string name, Color color)
        {
            Material material = LoadOrCreateMaterial(name, "Universal Render Pipeline/Unlit");
            material.SetColor("_BaseColor", color);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material LoadOrCreateMaterial(string name, string shaderName)
        {
            string path = MaterialsFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;

            Shader shader = Shader.Find(shaderName);
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            material = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        // ------------------------------------------------------------- helpers

        static void EnsureFolder(string folder)
        {
            EditorFolderUtility.EnsureFolder(folder);
        }

        static Scene NewScene()
        {
            return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        static void SaveScene(Scene scene, string path)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
        }

        static void ApplyAtmosphere(Color ambient, Color fogColor, float fogDensity)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = ambient;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.skybox = null;
        }

        static GameObject Group(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.layer = LevelLayer;
            return go;
        }

        static GameObject Box(Transform parent, string name, Vector3 localPosition, Vector3 size, Material material,
            bool collider = true, int layer = LevelLayer)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.layer = layer;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = size;

            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;

            if (!collider)
            {
                Collider box = go.GetComponent<Collider>();
                if (box != null) Object.DestroyImmediate(box);
            }

            return go;
        }

        static GameObject BoxLocal(Transform parent, string name, Vector3 localPosition, Vector3 size, Material material,
            Quaternion localRotation, bool collider = true, int layer = LevelLayer)
        {
            GameObject go = Box(parent, name, localPosition, size, material, collider, layer);
            go.transform.localRotation = localRotation;
            return go;
        }

        static void WallX(Transform parent, float x, float zMin, float zMax, float height, Material material, string name = "Wall")
        {
            Box(parent, name, new Vector3(x, height * 0.5f, (zMin + zMax) * 0.5f),
                new Vector3(0.3f, height, zMax - zMin), material);
        }

        static void WallZ(Transform parent, float z, float xMin, float xMax, float height, Material material, string name = "Wall")
        {
            Box(parent, name, new Vector3((xMin + xMax) * 0.5f, height * 0.5f, z),
                new Vector3(xMax - xMin, height, 0.3f), material);
        }

        static void Ceiling(Transform parent, float xMin, float xMax, float zMin, float zMax, float y, Material material)
        {
            Box(parent, "Ceiling", new Vector3((xMin + xMax) * 0.5f, y, (zMin + zMax) * 0.5f),
                new Vector3(xMax - xMin, 0.2f, zMax - zMin), material, false);
        }

        static GameObject Empty(Transform parent, string name, Vector3 localPosition, Quaternion localRotation)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = localRotation;
            go.layer = LevelLayer;
            return go;
        }

        static Camera CreateCamera(string name, Vector3 position, Quaternion rotation, Color background)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.transform.rotation = rotation;
            go.tag = "MainCamera";

            Camera camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = background;
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 120f;
            camera.allowHDR = true;
            camera.allowMSAA = true;

            go.AddComponent<AudioListener>();
            return camera;
        }

        static Light CreateDirectionalMoon(Transform parent)
        {
            var go = new GameObject("Moonlight");
            go.transform.SetParent(parent, false);
            go.transform.rotation = Quaternion.Euler(42f, -32f, 0f);

            Light light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.55f, 0.66f, 0.92f);
            light.intensity = 0.11f;
            light.shadows = LightShadows.None;
            light.renderMode = LightRenderMode.ForcePixel;
            return light;
        }

        static Light CreatePointLight(Transform parent, string name, Vector3 localPosition, Color color,
            float intensity, float range, bool shadows = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;

            Light light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
            light.renderMode = LightRenderMode.Auto;
            return light;
        }

        static GameObject AddLamp(Transform parent, Vector3 localPosition, Color color, float intensity, float range)
        {
            var go = new GameObject("Lamp");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;

            GameObject fixture = Box(go.transform, "Fixture", Vector3.zero, new Vector3(0.38f, 0.12f, 0.38f),
                AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/Mat_Lamp.mat"), false);
            fixture.transform.localPosition = new Vector3(0f, 0.08f, 0f);

            CreatePointLight(go.transform, "Light", new Vector3(0f, -0.15f, 0f), color, intensity, range);
            return go;
        }

        static TextMesh CreateWorldText(Transform parent, string name, string content, Vector3 localPosition,
            Quaternion localRotation, float characterSize, int fontSize, Color color, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            // TextMesh's readable face is +Z. Every caller supplies the rotation
            // it wants for a -Z-facing front, so flip the mesh once here instead
            // of mirroring text in every label.
            go.transform.localRotation = localRotation * Quaternion.Euler(0f, 180f, 0f);

            TextMesh text = go.AddComponent<TextMesh>();
            text.text = content;
            text.font = SceneBuilderUI.UIFont;
            text.fontSize = fontSize;
            text.characterSize = characterSize;
            text.anchor = anchor;
            text.alignment = TextAlignment.Center;
            text.color = color;
            text.richText = true;

            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null && SceneBuilderUI.UIFont != null)
            {
                renderer.sharedMaterial = SceneBuilderUI.UIFont.material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            return text;
        }

        static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }
}
