using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;
using Scene = UnityEngine.SceneManagement.Scene;

namespace LightShadow.Tools
{
    // Editor tooling for the premium-pass engineering systems (quality tiers,
    // Cinemachine cinematic rigs). Everything here is executed by the Lead TD
    // through the menu item or batch methods - the game itself never depends
    // on this file running.
    public static partial class SceneBuilder
    {
        // Per-tier URP asset variants live next to the base assets. The runtime
        // tier system does NOT require them (it retunes the active asset), but
        // having real per-tier assets lets the Lead bind them into per-tier
        // quality levels for the Android build later.
        const string SettingsFolder = "Assets/Settings";
        const string LowAssetPath = SettingsFolder + "/RP_Low.asset";
        const string MediumAssetPath = SettingsFolder + "/RP_Medium.asset";
        const string HighAssetPath = SettingsFolder + "/RP_High.asset";
        const string UltraAssetPath = SettingsFolder + "/RP_Ultra.asset";
        const string MobileAssetPath = SettingsFolder + "/Mobile_RPAsset.asset";
        const string PcAssetPath = SettingsFolder + "/PC_RPAsset.asset";
        const string PcRendererPath = SettingsFolder + "/PC_Renderer.asset";
        const string MobileRendererPath = SettingsFolder + "/Mobile_Renderer.asset";

        [MenuItem("LightShadow/8. Quality Tiers + Cinematic Rigs", priority = 8)]
        public static void ConfigureSystemsMenu()
        {
            ConfigureQualityForBuild();
            if (EditorSceneManager.GetActiveScene().path != SchoolPath && !Application.isBatchMode
                && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return; // user declined to save their open scene; do not destroy it
            }
            BuildCinematicRigs();
        }

        /// <summary>Batch entry: -executeMethod LightShadow.Tools.SceneBuilder.QualityConfigBatch</summary>
        public static void QualityConfigBatch()
        {
            try
            {
                ConfigureQualityForBuild();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        // ====================================================== quality tiers --

        // Creates RP_Low / RP_Medium / RP_High / RP_Ultra by copying the existing
        // Mobile_/PC_ assets and adjusting the tier fields through SerializedObject.
        // Best effort by design: if anything here fails, the runtime tier system
        // still works (it retunes whichever asset QualitySettings serves).
        //
        // Tier table (mirrors PerformanceManager; see Docs/PERFORMANCE.md):
        //   renderScale     0.65 | 0.75 | 0.90 | 1.0
        //   shadowDistance  0    | 20   | 40   | 60
        //   MSAA            off  | off  | 2x   | 2x
        //   soft shadows    off  | off  | on   | on
        public static void ConfigureQualityForBuild()
        {
            // GPU Resident Drawer must be OFF: with the drawer resources absent
            // from the build the pipeline's CreatePipeline() throws every frame
            // (GPUResidentDrawer NRE -> Blitter already initialized), the
            // pipeline never caches, and frame rate collapses. Nothing in the
            // game needs it (small interior scene, few thousand renderers).
            string drawer = DisableGpuResidentDrawer(PcAssetPath);
            drawer += ", " + DisableGpuResidentDrawer(MobileAssetPath);
            string post = EnsurePostShadersIncluded();
            string ssao = DisableSsaoFeature(PcRendererPath);
            ssao += ", " + DisableSsaoFeature(MobileRendererPath);

            string summary = "quality assets: ";
            summary += TuneTierCopy(MobileAssetPath, LowAssetPath, 0.65f, 1, 0f, false);
            summary += ", " + TuneTierCopy(MobileAssetPath, MediumAssetPath, 0.75f, 1, 20f, false);
            summary += ", " + TuneTierCopy(PcAssetPath, HighAssetPath, 0.9f, 2, 40f, true);
            summary += ", " + TuneTierCopy(PcAssetPath, UltraAssetPath, 1.0f, 2, 60f, true);
            Debug.Log("[LightShadow] ConfigureQualityForBuild: " + drawer + "; " + post
                + "; SSAO: " + ssao + "; " + summary);
        }

        // URP's hidden post shaders get variant-stripped out of player builds
        // ("Shader ... is not supported -> PostProcessing render passes will
        // not execute"), which silently ships the game without bloom, vignette,
        // colour grading and film grain. Pinning them into Graphics settings'
        // Always Included Shaders keeps the post stack alive in builds.
        static readonly string[] PostShaderNames =
        {
            "Hidden/Universal Render Pipeline/UberPost",
            "Hidden/Universal Render Pipeline/BokehDepthOfField",
            "Hidden/Universal Render Pipeline/Bloom",
            "Hidden/Universal Render Pipeline/FilmGrain",
        };

        static string EnsurePostShadersIncluded()
        {
            try
            {
                Object settings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")
                    .FirstOrDefault();
                if (settings == null) return "post shaders: graphics settings missing";

                SerializedObject serialized = new SerializedObject(settings);
                SerializedProperty included = serialized.FindProperty("m_AlwaysIncludedShaders");
                if (included == null || !included.isArray) return "post shaders: array missing";

                var added = new List<string>();
                foreach (string shaderName in PostShaderNames)
                {
                    Shader shader = Shader.Find(shaderName);
                    if (shader == null)
                    {
                        added.Add(shaderName + " MISSING");
                        continue;
                    }

                    bool present = false;
                    for (int i = 0; i < included.arraySize; i++)
                    {
                        if (included.GetArrayElementAtIndex(i).objectReferenceValue == shader) present = true;
                    }
                    if (present) continue;
                    included.InsertArrayElementAtIndex(included.arraySize);
                    included.GetArrayElementAtIndex(included.arraySize - 1).objectReferenceValue = shader;
                    added.Add(Path.GetFileName(shaderName));
                }

                serialized.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
                return "post shaders pinned: " + (added.Count > 0 ? string.Join(", ", added) : "already present");
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Post shader pinning failed: " + exception.Message);
                return "post shaders ERROR";
            }
        }

        // SSAO's shader resources get stripped from player builds even with
        // the feature enabled ("Couldn't find the required resources for the
        // ScreenSpaceAmbientOcclusion render feature"), and on the integrated
        // GPUs this game actually ships on, SSAO at 120 FPS was never viable.
        // The feature is disabled on both renderers; depth comes from the
        // baked probes and the fog instead.
        static string DisableSsaoFeature(string rendererPath)
        {
            try
            {
                Object renderer = AssetDatabase.LoadAssetAtPath<Object>(rendererPath);
                if (renderer == null) return Path.GetFileName(rendererPath) + " MISSING";

                SerializedObject serialized = new SerializedObject(renderer);
                SerializedProperty features = serialized.FindProperty("m_RendererFeatures");
                if (features == null || !features.isArray) return Path.GetFileName(rendererPath) + " no-features";

                int disabled = 0;
                for (int i = 0; i < features.arraySize; i++)
                {
                    Object feature = features.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (feature == null) continue;
                    if (feature.GetType().Name.IndexOf("ScreenSpaceAmbientOcclusion", StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                    SerializedObject featureSerialized = new SerializedObject(feature);
                    SerializedProperty enabled = featureSerialized.FindProperty("m_Enabled");
                    if (enabled != null && enabled.boolValue)
                    {
                        enabled.boolValue = false;
                        featureSerialized.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(feature);
                        disabled++;
                    }
                }
                if (disabled > 0)
                {
                    EditorUtility.SetDirty(renderer);
                    AssetDatabase.SaveAssets();
                }
                return Path.GetFileName(rendererPath) + (disabled > 0 ? " SSAO off" : " SSAO already off");
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] SSAO disable failed for " + rendererPath + ": " + exception.Message);
                return "ERROR";
            }
        }

        // Sets m_GPUResidentDrawerMode = 0 (disabled) on a URP pipeline asset.
        static string DisableGpuResidentDrawer(string assetPath)
        {
            try
            {
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (asset == null) return Path.GetFileName(assetPath) + " MISSING";
                SerializedObject serialized = new SerializedObject(asset);
                SerializedProperty mode = serialized.FindProperty("m_GPUResidentDrawerMode");
                if (mode == null) return Path.GetFileName(assetPath) + " no-drawer-field";
                if (mode.intValue == 0) return Path.GetFileName(assetPath) + " already off";
                mode.intValue = 0;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
                return Path.GetFileName(assetPath) + " drawer OFF";
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] GPU drawer disable failed for " + assetPath + ": " + exception.Message);
                return "ERROR";
            }
        }

        // Copies base -> variant (skipping an identical existing copy), then sets
        // the four tier fields via SerializedObject. Returns a short status.
        static string TuneTierCopy(string basePath, string variantPath, float renderScale, int msaa,
            float shadowDistance, bool softShadows)
        {
            try
            {
                if (!File.Exists(basePath))
                {
                    Debug.LogWarning("[LightShadow] " + basePath + " missing; skipping " + variantPath);
                    return basePath + " MISSING";
                }

                if (File.Exists(variantPath))
                {
                    AssetDatabase.DeleteAsset(variantPath);
                }
                if (!AssetDatabase.CopyAsset(basePath, variantPath))
                {
                    Debug.LogWarning("[LightShadow] CopyAsset failed: " + basePath + " -> " + variantPath);
                    return "COPY FAILED";
                }

                Object variant = AssetDatabase.LoadAssetAtPath<Object>(variantPath);
                if (variant == null) return "LOAD FAILED";

                SerializedObject serialized = new SerializedObject(variant);
                SetFloat(serialized, "m_RenderScale", renderScale);
                SetInt(serialized, "m_MSAA", msaa);
                SetFloat(serialized, "m_ShadowDistance", shadowDistance);
                SetBool(serialized, "m_SoftShadowsSupported", softShadows);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(variant);
                return Path.GetFileName(variantPath) + " OK";
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Tier asset '" + variantPath + "' failed: " + exception.Message);
                return "ERROR";
            }
        }

        static void SetFloat(SerializedObject serialized, string property, float value)
        {
            SerializedProperty found = serialized.FindProperty(property);
            if (found != null) found.floatValue = value;
        }

        static void SetInt(SerializedObject serialized, string property, int value)
        {
            SerializedProperty found = serialized.FindProperty(property);
            if (found != null) found.intValue = value;
        }

        static void SetBool(SerializedObject serialized, string property, bool value)
        {
            SerializedProperty found = serialized.FindProperty(property);
            if (found != null) found.boolValue = value;
        }

        // ================================================== cinematic rigs ----

        // Creates the three dormant Cinemachine shots in 02_School (catch
        // close-up, dawn corridor, ending gate) plus a disabled CinemachineBrain
        // on the player camera. The rigs are DORMANT: priority 0, disabled game
        // objects; CinematicDirector wakes them for a few seconds at runtime.
        //
        // The gameplay camera (existing first-person rig) is untouched - the
        // brain only matters while a shot is live, and it is added DISABLED so
        // the validated first-person control never changes hands by accident.
        public static void BuildCinematicRigs()
        {
            try
            {
                Scene active = EditorSceneManager.GetActiveScene();
                Scene school = active.path == SchoolPath
                    ? active
                    : EditorSceneManager.OpenScene(SchoolPath, OpenSceneMode.Single);

                if (school.path != SchoolPath)
                {
                    Debug.LogWarning("[LightShadow] Cinematic rigs: 02_School could not be opened.");
                    return;
                }

                // Never duplicate: drop a previous pass's rigs first.
                GameObject previous = GameObject.Find("CM_Cinematics");
                if (previous != null) Undo.DestroyObjectImmediate(previous);

                GameObject root = new GameObject("CM_Cinematics");

                CreateShotRig(root.transform, "CM_Shot_catch", new Vector3(0f, 1.7f, -27.5f), Vector3.forward);
                CreateShotRig(root.transform, "CM_Shot_dawn", new Vector3(0f, 2.2f, -20f), Vector3.forward);
                CreateShotRig(root.transform, "CM_Shot_gate", new Vector3(0f, 1.8f, -24f), Vector3.forward);

                // One disabled brain on the player camera; the director flips it
                // on only for the length of a shot.
                GameObject player = GameObject.Find("Player");
                if (player != null)
                {
                    Camera playerCamera = player.GetComponentInChildren<Camera>();
                    if (playerCamera != null && playerCamera.GetComponent<CinemachineBrain>() == null)
                    {
                        CinemachineBrain brain = playerCamera.gameObject.AddComponent<CinemachineBrain>();
                        brain.enabled = false;
                    }
                }

                EditorSceneManager.MarkSceneDirty(school);
                EditorSceneManager.SaveScene(school, SchoolPath);
                Debug.Log("[LightShadow] Cinematic rigs built into 02_School (3 dormant shots + disabled brain).");
            }
            catch (Exception exception)
            {
                // A missing Cinemachine package must not take the build down.
                Debug.LogWarning("[LightShadow] Cinematic rigs skipped: " + exception.Message);
            }
        }

        static void CreateShotRig(Transform parent, string name, Vector3 position, Vector3 forward)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

            // Dormant by default: disabled object, priority 0 (below any live
            // shot priority of 100 that CinematicDirector raises at runtime).
            go.SetActive(false);

            var camera = go.AddComponent<CinemachineCamera>();
            camera.Priority = 0;

            var composer = go.AddComponent<CinemachinePositionComposer>();
            composer.Damping = new Vector3(0.6f, 0.6f, 0.4f);
        }
    }
}
