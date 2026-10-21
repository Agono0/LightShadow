using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace LightShadow.Core
{
    // Safety net for the NavMesh. The scene builder bakes the Surface and saves
    // the data with the scene, but if that data is ever lost - a fresh clone, a
    // scene rebuilt by hand - the level still comes up with a walkable Shadow
    // instead of a frozen one. Runs before every other script in the scene.
    [DefaultExecutionOrder(-1000)]
    public class NavMeshBootstrap : MonoBehaviour
    {
        public NavMeshSurface surface;

        void Awake()
        {
            if (surface == null) surface = GetComponent<NavMeshSurface>();
        }

        void Start()
        {
            if (surface == null) surface = GetComponent<NavMeshSurface>();
            if (surface == null) return;

            // Start (not Awake) so NavMeshSurface.OnEnable has already registered
            // the baked data before we test for it.
            if (HasNavMesh()) return;

            Debug.LogWarning("LightShadow: no baked NavMesh found, building one at runtime.");
            surface.BuildNavMesh();
        }

        static bool HasNavMesh()
        {
            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            return triangulation.vertices != null && triangulation.vertices.Length > 0;
        }
    }
}
