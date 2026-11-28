using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightShadow.Tools
{
    // Contract H2: a contributor level module implements this interface in
    // an Editor folder. SceneBuilder.BuildAll discovers every implementation
    // by reflection (public or internal, parameterless constructor) and calls
    // Build once on the finished school scene, then saves the scene.
    public interface ILevelContribution
    {
        // Stable id for logs, e.g. "abdelraouf.journal-notes".
        string Id { get; }

        // Lower runs first. Ties break by Id.
        int Order { get; }

        // Add content under ctx.DynamicRoot / ctx.StaticRoot. Must be
        // idempotent in effect: BuildAll regenerates the scene from scratch,
        // so the scene never holds stale contributor objects.
        void Build(LevelContext ctx);
    }

    // What a contribution may rely on. Everything here is stable.
    public sealed class LevelContext
    {
        public Scene Scene;

        // The base level's player start (Spawn_Entrance), or null if absent.
        public Transform SpawnPoint;

        // Parent for walls/floors/static props. Layer is LevelLayer so the
        // NavMesh bake sees it. Created by the hook, name "Contrib_Static".
        // The NavMesh is rebaked after all contributions when this root holds
        // bake-visible colliders (see SceneBuilder.NeedsNavMeshRebake).
        public Transform StaticRoot;

        // Parent for anything that moves, opens or is picked up. Layer 0
        // (Default). Dynamic objects must stay off LevelLayer or they seal
        // the NavMesh bake. Created by the hook, name "Contrib_Dynamic".
        public Transform DynamicRoot;

        // The layer index the NavMesh bake collects.
        public int LevelLayer;
    }

    public static partial class SceneBuilder
    {
        static void RunLevelContributions()
        {
            var found = new List<ILevelContribution>();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<ILevelContribution>())
            {
                if (type.IsAbstract || type.IsInterface) continue;
                if (type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                        null, Type.EmptyTypes, null) == null) continue;
                try { found.Add((ILevelContribution)Activator.CreateInstance(type, true)); }
                catch (Exception exception)
                {
                    Debug.LogError("[LightShadow] Level contribution " + type.FullName + " could not be created: " + exception.Message);
                }
            }

            if (found.Count == 0) return;

            found.Sort((a, b) =>
            {
                int byOrder = a.Order.CompareTo(b.Order);
                return byOrder != 0 ? byOrder : string.CompareOrdinal(a.Id, b.Id);
            });

            EditorSceneManager.OpenScene(SchoolPath, OpenSceneMode.Single);
            Scene scene = EditorSceneManager.GetActiveScene();

            var staticRoot = new GameObject("Contrib_Static") { layer = LevelLayer };
            var dynamicRoot = new GameObject("Contrib_Dynamic") { layer = 0 };
            GameObject spawn = GameObject.Find("Spawn_Entrance");

            var ctx = new LevelContext
            {
                Scene = scene,
                SpawnPoint = spawn != null ? spawn.transform : null,
                StaticRoot = staticRoot.transform,
                DynamicRoot = dynamicRoot.transform,
                LevelLayer = LevelLayer,
            };

            foreach (ILevelContribution contribution in found)
            {
                try { contribution.Build(ctx); }
                catch (Exception exception)
                {
                    // One broken module must not take the whole build down,
                    // but it must never fail silently either.
                    Debug.LogError("[LightShadow] Level contribution '" + contribution.Id + "' failed: " + exception);
                }
            }

            // The school NavMesh was baked before the contributions ran, so
            // geometry added under Contrib_Static is not in it yet. Rebake only
            // when something there would actually be collected; with no such
            // content (today's modules) nothing changes and nothing is rebaked.
            if (NeedsNavMeshRebake(staticRoot.transform, 1 << LevelLayer))
            {
                NavMeshSurface surface = UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>();
                if (surface != null)
                {
                    Debug.Log("[LightShadow] Contrib_Static holds NavMesh-visible colliders; rebaking the school NavMesh.");
                    BakeNavMesh(surface);
                }
                else
                {
                    Debug.LogWarning("[LightShadow] Contrib_Static holds NavMesh-visible colliders but the scene has no NavMeshSurface; not rebaked.");
                }
            }

            SaveScene(scene, SchoolPath);
        }

        // True when any descendant of root is an enabled, non-trigger Collider
        // on an active object whose layer is in layerMask. Mirrors what the
        // school surface collects (PhysicsColliders, layerMask, All objects):
        // triggers are excluded by the NavMesh builder (Unity "As Designed"
        // UUM-54608); inactive objects and disabled colliders are assumed not
        // collected. Pure: reads only the transform tree, no bake, no assets.
        internal static bool NeedsNavMeshRebake(Transform root, int layerMask)
        {
            if (root == null) return false;
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider c = colliders[i];
                if (c == null || !c.enabled || c.isTrigger) continue;
                if (!c.gameObject.activeInHierarchy) continue;
                if ((layerMask & (1 << c.gameObject.layer)) == 0) continue;
                return true;
            }
            return false;
        }
    }
}
