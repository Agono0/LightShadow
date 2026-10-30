using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using LightShadow.Gameplay;

namespace LightShadow.Tools
{
    // Lighting pass for the premium art (ROADMAP P1, DECISIONS.md D11):
    //   - MarkStaticArchitecture: static editor flags on the architecture and
    //     furniture that never moves (contribute GI + occluder/occludee +
    //     static batching), while lamps, dawn lights, the gate, pickups and
    //     everything the validator counts stay dynamic;
    //   - ApplyLightingProbes: light probe groups per wing plus three baked
    //     reflection probes (entrance / library / corridor);
    //   - BakeLightingBatch: configures a LightingSettings asset and bakes
    //     asynchronously, then bakes the reflection probes and exits 0/1.
    //     Without graphics (batchmode -nographics) the lightmapper cannot run:
    //     the batch then exits NON-ZERO with one clear log line, and the Lead
    //     re-runs this single command WITHOUT -nographics.
    public static partial class SceneBuilder
    {
        const string LightingSettingsPath = "Assets/_Project/Data/LightingSettings.asset";
        const string ReflectionProbesFolder = "Assets/_Project/Data/ReflectionProbes";
        const string ProbesRootName = "ArtProbes";

        // Room groups in the school scene whose contents are fully static.
        // The Lighting/DawnChase roots (toggleable lamps) are deliberately NOT
        // in this list, and prefab instances (interactables, gate) are skipped
        // by component checks below.
        static readonly string[] StaticRoomRoots =
        {
            "Environment", "Library", "PhysicsLab", "Classroom",
            "Garden", "StaffRoom", "MainHall", "Entrance"
        };

        // Light probe groups: (name, center, size, grid steps x/y/z).
        static readonly Vector3[] ProbeRoomCenters =
        {
            new Vector3(0f, 1.5f, -30f),   // entrance
            new Vector3(0f, 1.5f, 0f),     // corridor (expanded below)
            new Vector3(-12f, 1.5f, -15f), // library
            new Vector3(11f, 1.5f, -15f),  // physics lab
            new Vector3(-12f, 1.5f, 11f),  // classroom
            new Vector3(12f, 1.5f, 11f),   // garden
            new Vector3(-12f, 1.5f, 23f),  // staff room
            new Vector3(0f, 2.2f, 34f)     // main hall
        };
        static readonly Vector3[] ProbeRoomSizes =
        {
            new Vector3(13f, 3f, 6f),
            new Vector3(5f, 3f, 54f),
            new Vector3(19f, 3f, 14f),
            new Vector3(19f, 3f, 14f),
            new Vector3(19f, 3f, 14f),
            new Vector3(19f, 3f, 14f),
            new Vector3(19f, 3f, 6f),
            new Vector3(27f, 4f, 13f)
        };
        static readonly int[] ProbeRoomStepsX = { 3, 2, 4, 4, 4, 4, 4, 4 };
        static readonly int[] ProbeRoomStepsZ = { 2, 8, 3, 3, 3, 3, 2, 3 };
        const int ProbeStepsY = 2;

        // Reflection probes: (name, position, box size).
        static readonly string[] ReflectionProbeNames = { "Entrance", "Library", "Corridor" };
        static readonly Vector3[] ReflectionProbePositions =
        {
            new Vector3(0f, 1.6f, -29.5f),
            new Vector3(-12f, 1.6f, -15f),
            new Vector3(0f, 2.0f, 0f)
        };
        static readonly Vector3[] ReflectionProbeSizes =
        {
            new Vector3(14f, 4f, 8f),
            new Vector3(20f, 4f, 15f),
            new Vector3(6f, 4f, 54f)
        };
        static readonly Color ReflectionClearColor = new Color(0.02f, 0.024f, 0.034f);

        // Lightmap budget: low resolution and 1024 maps keep the mobile build
        // small; the school is mostly lit by realtime points anyway (D11).
        const int LightmapResolution = 24;
        const int LightmapSize = 1024;
        const int GiBounceIntensity = 2;

        // ------------------------------------------------------------- batch
        // -executeMethod LightShadow.Tools.SceneBuilder.ApplyLightingBatch
        public static void ApplyLightingBatch()
        {
            try
            {
                string summary = ApplyLighting();
                Debug.Log("[LightShadow] LIGHTING SETUP\n" + summary);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError("[LightShadow] LIGHTING SETUP FAILED: " + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("LightShadow/Art/3. Apply Probes + Static Flags", priority = 43)]
        public static void ApplyLightingMenu()
        {
            string summary = ApplyLighting();
            EditorUtility.DisplayDialog("LightShadow - Lighting Setup", summary, "OK");
        }

        public static string ApplyLighting()
        {
            AssetDatabase.Refresh();
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SchoolPath) == null)
            {
                throw new InvalidOperationException("School scene missing - run LightShadow.Tools.SceneBuilder.BuildAllBatch first.");
            }

            EditorSceneManager.OpenScene(SchoolPath, OpenSceneMode.Single);
            Scene scene = EditorSceneManager.GetActiveScene();

            int staticCount = MarkStaticArchitecture();
            int probeCount = ApplyLightingProbes();

            SaveScene(scene, SchoolPath);
            return "Static architecture objects: " + staticCount
                + "\nProbe groups: " + probeCount
                + "\nReflection probes: " + ReflectionProbeNames.Length
                + "\nNext: bake with BakeLightingBatch (needs graphics, NOT -nographics).";
        }

        // ---------------------------------------------------- static markers
        static int MarkStaticArchitecture()
        {
            StaticEditorFlags flags = StaticEditorFlags.ContributeGI
                | StaticEditorFlags.OccluderStatic
                | StaticEditorFlags.OccludeeStatic
                | StaticEditorFlags.BatchingStatic;

            Transform level = GetSceneRoot("Level");
            if (level == null) throw new InvalidOperationException("School scene has no Level root - rebuild scenes first.");

            int count = 0;
            foreach (string roomName in StaticRoomRoots)
            {
                Transform room = level.Find(roomName);
                if (room == null)
                {
                    Debug.LogWarning("[LightShadow] Lighting: room root not found: " + roomName);
                    continue;
                }

                foreach (Transform node in room.GetComponentsInChildren<Transform>(true))
                {
                    if (!IsStaticCandidate(node)) continue;
                    GameObjectUtility.SetStaticEditorFlags(node.gameObject, flags);
                    count++;
                }
            }
            return count;
        }

        // A node contributes to GI/batching only when it is a plain visual:
        // no lights, particles, probes, labels, interactables, dawn props, and
        // nothing inside a prefab instance (gate leaf must keep moving).
        static bool IsStaticCandidate(Transform node)
        {
            GameObject go = node.gameObject;

            if (PrefabUtility.IsPartOfPrefabInstance(go)) return false;
            if (go.GetComponent<Light>() != null) return false;
            if (go.GetComponent<ParticleSystem>() != null) return false;
            if (go.GetComponent<ReflectionProbe>() != null) return false;
            if (go.GetComponent<LightProbeGroup>() != null) return false;
            if (go.GetComponent<TextMesh>() != null) return false;
            if (go.GetComponentInParent<ExitGate>() != null) return false;
            if (go.name == "DawnPlane") return false;
            return true;
        }

        // ------------------------------------------------------------- probes
        static int ApplyLightingProbes()
        {
            Transform level = GetSceneRoot("Level");
            if (level == null) throw new InvalidOperationException("School scene has no Level root - rebuild scenes first.");

            // Idempotent: the probe root is rebuilt from scratch every run.
            Transform existing = level.Find(ProbesRootName);
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);

            GameObject probesRoot = new GameObject(ProbesRootName);
            probesRoot.transform.SetParent(level, false);
            probesRoot.layer = LevelLayer;

            for (int i = 0; i < ProbeRoomCenters.Length; i++)
            {
                var groupGo = new GameObject("Probes_" + i);
                groupGo.transform.SetParent(probesRoot.transform, false);
                groupGo.transform.position = ProbeRoomCenters[i];
                LightProbeGroup group = groupGo.AddComponent<LightProbeGroup>();
                group.probePositions = BuildProbeGrid(ProbeRoomSizes[i], ProbeRoomStepsX[i], ProbeRoomStepsZ[i]);
            }

            if (!AssetDatabase.IsValidFolder(ReflectionProbesFolder))
            {
                EnsureDataFolder(ReflectionProbesFolder);
            }

            for (int i = 0; i < ReflectionProbeNames.Length; i++)
            {
                var probeGo = new GameObject("Reflection_" + ReflectionProbeNames[i]);
                probeGo.transform.SetParent(probesRoot.transform, false);
                probeGo.transform.position = ReflectionProbePositions[i];

                ReflectionProbe probe = probeGo.AddComponent<ReflectionProbe>();
                probe.mode = ReflectionProbeMode.Baked;
                probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
                probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
                probe.boxProjection = true;
                probe.intensity = 1f;
                probe.size = ReflectionProbeSizes[i];
                probe.center = Vector3.zero;
                probe.cullingMask = ~(1 << PlayerLayer) & ~(1 << EnemyLayer);
                probe.clearFlags = ReflectionProbeClearFlags.SolidColor;
                probe.backgroundColor = ReflectionClearColor;
                probe.resolution = 128;
            }

            return ProbeRoomCenters.Length;
        }

        static Vector3[] BuildProbeGrid(Vector3 size, int stepsX, int stepsZ)
        {
            var positions = new List<Vector3>();
            int clampedX = Mathf.Max(1, stepsX);
            int clampedZ = Mathf.Max(1, stepsZ);
            for (int x = 0; x < clampedX; x++)
            {
                float fx = clampedX == 1 ? 0.5f : (float)x / (clampedX - 1);
                for (int z = 0; z < clampedZ; z++)
                {
                    float fz = clampedZ == 1 ? 0.5f : (float)z / (clampedZ - 1);
                    for (int y = 0; y < ProbeStepsY; y++)
                    {
                        float fy = ProbeStepsY == 1 ? 0.5f : (float)y / (ProbeStepsY - 1);
                        // Sample the box interior, inset by 10 percent.
                        Vector3 local = new Vector3(
                            Mathf.Lerp(-size.x, size.x, fx) * 0.45f,
                            Mathf.Lerp(-size.y, size.y, fy) * 0.45f,
                            Mathf.Lerp(-size.z, size.z, fz) * 0.45f);
                        positions.Add(local);
                    }
                }
            }
            return positions.ToArray();
        }

        // -------------------------------------------------------------- bake
        // -executeMethod LightShadow.Tools.SceneBuilder.BakeLightingBatch
        // NOTE: requires graphics - run WITHOUT -nographics. Exits non-zero
        // with one clear log line when baking is impossible in this session.
        public static void BakeLightingBatch()
        {
            try
            {
                if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                {
                    Debug.LogError("[LightShadow] LIGHTING BAKE IMPOSSIBLE: this Unity session has no graphics device "
                        + "(-nographics). Re-run BakeLightingBatch WITHOUT -nographics; probes and static flags "
                        + "from ApplyLightingBatch remain valid meanwhile.");
                    EditorApplication.Exit(1);
                    return;
                }

                LightingSettings settings = EnsureLightingSettings();
                Lightmapping.lightingSettings = settings;

                Lightmapping.bakeCompleted += OnBakeCompleted;
                Lightmapping.BakeAsync();
                Debug.Log("[LightShadow] Lighting bake started (async)...");
            }
            catch (Exception exception)
            {
                Debug.LogError("[LightShadow] LIGHTING BAKE IMPOSSIBLE: " + exception.Message
                    + " - re-run BakeLightingBatch WITHOUT -nographics.");
                EditorApplication.Exit(1);
            }
        }

        static void OnBakeCompleted()
        {
            try
            {
                Lightmapping.bakeCompleted -= OnBakeCompleted;

                // Baked reflection probes one by one; a probe failure is a
                // warning (gameplay degrades to no reflections), while a GI
                // failure above is fatal.
                int bakedProbes = 0;
                ReflectionProbe[] probes = UnityEngine.Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None);
                foreach (ReflectionProbe probe in probes)
                {
                    if (probe.mode != ReflectionProbeMode.Baked) continue;
                    string probePath = ReflectionProbesFolder + "/Reflection_" + probe.name + ".exr";
                    try
                    {
                        if (Lightmapping.BakeReflectionProbe(probe, probePath))
                        {
                            bakedProbes++;
                        }
                        else
                        {
                            Debug.LogWarning("[LightShadow] Reflection probe bake returned false: " + probe.name);
                        }
                    }
                    catch (Exception probeException)
                    {
                        Debug.LogWarning("[LightShadow] Reflection probe bake failed for " + probe.name
                            + ": " + probeException.Message);
                    }
                }

                EditorSceneManager.SaveOpenScenes();
                Debug.Log("[LightShadow] LIGHTING BAKE COMPLETE - GI data saved, reflection probes baked: "
                    + bakedProbes + "/" + probes.Length);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogError("[LightShadow] LIGHTING BAKE IMPOSSIBLE (completion stage): " + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        static LightingSettings EnsureLightingSettings()
        {
            LightingSettings settings = AssetDatabase.LoadAssetAtPath<LightingSettings>(LightingSettingsPath);
            if (settings != null) return settings;

            // Unity 6: LightingSettings is no longer a ScriptableObject, so it is
            // constructed directly and saved with AssetDatabase.CreateAsset.
            // bounceIntensity was removed from the API in Unity 6 and is skipped.
            settings = new LightingSettings();
            settings.bakedGI = true;
            settings.realtimeGI = false;
            settings.lightmapper = LightingSettings.Lightmapper.ProgressiveCPU;
            settings.lightmapResolution = LightmapResolution;
            settings.compressLightmaps = true;
            settings.ao = false;

            EnsureDataFolder("Assets/_Project/Data");
            AssetDatabase.CreateAsset(settings, LightingSettingsPath);
            Debug.Log("[LightShadow] Lighting settings created at " + LightingSettingsPath);
            return settings;
        }

        // ------------------------------------------------------------ helpers
        static Transform GetSceneRoot(string rootName)
        {
            Scene active = EditorSceneManager.GetActiveScene();
            foreach (GameObject root in active.GetRootGameObjects())
            {
                if (root.name == rootName) return root.transform;
            }
            return null;
        }

        static void EnsureDataFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;
            string parentPath = folderPath.Substring(0, folderPath.LastIndexOf('/'));
            string leaf = folderPath.Substring(folderPath.LastIndexOf('/') + 1);
            EnsureDataFolder(parentPath);
            AssetDatabase.CreateFolder(parentPath, leaf);
        }
    }
}
