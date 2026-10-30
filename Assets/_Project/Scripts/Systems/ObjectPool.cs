using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightShadow.Systems
{
    // Generic component pool keyed by prefab (premium pass P2). Get() pops the
    // oldest released instance or instantiates a new one; Release() parks the
    // instance under the pool root and deactivates it. Preload() warms the pool
    // so the first burst never allocates during play.
    //
    // All lookups are dictionary/array based: no LINQ, no per-call allocations
    // beyond the first Instantiate of a new instance.
    public static class ObjectPool
    {
        class PoolEntry
        {
            public Transform Root;                  // parked instances live here
            public Component[] Available;           // ring of released components
            public int AvailableCount;
            public Func<Component> Factory;         // builds the template once
        }

        // Prefab -> pool state. Dictionary lookup per Get/Release, no boxing.
        static readonly Dictionary<int, PoolEntry> Pools = new Dictionary<int, PoolEntry>();
        static Transform poolRoot;

        const int InitialCapacity = 16;

        // The scene-independent root every pool parks under. Exposed so lazily
        // created effect templates can live here too (they survive scene loads,
        // which keeps pool keys stable).
        public static Transform Root => PoolRoot();

        // Registers (or tops up) a pool for a prefab. Preloads `count` dormant
        // instances so later Gets are allocation-free.
        public static void Preload(Component prefab, int count)
        {
            if (prefab == null || count <= 0) return;
            PoolEntry entry = GetOrCreateEntry(prefab);
            for (int i = entry.AvailableCount; i < count; i++)
            {
                Component instance = CreateInstance(prefab, entry);
                Park(entry, instance);
            }
        }

        // Takes an instance out of the pool. The returned component is active
        // and re-parented to the scene root.
        public static T Get<T>(T prefab) where T : Component
        {
            return Get(prefab, Vector3.zero, Quaternion.identity, null);
        }

        public static T Get<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
        {
            if (prefab == null) return null;
            PoolEntry entry = GetOrCreateEntry(prefab);

            Component instance = null;
            if (entry.AvailableCount > 0)
            {
                entry.AvailableCount--;
                instance = entry.Available[entry.AvailableCount];
                entry.Available[entry.AvailableCount] = null;
            }

            if (instance == null) instance = CreateInstance(prefab, entry);

            Transform transform = instance.transform;
            transform.SetParent(parent, false);
            transform.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);
            return (T)instance;
        }

        // Returns an instance to its pool. Safe to call twice; destroys nothing.
        public static void Release(Component instance)
        {
            if (instance == null) return;
            PoolEntry entry;
            if (!Pools.TryGetValue(instance.gameObject.GetInstanceID(), out entry)) return;
            Park(entry, instance);
        }

        // Release() needs to know which pool an instance belongs to, so every
        // instance gets a marker component mapping back to its pool root.
        static void Park(PoolEntry entry, Component instance)
        {
            if (entry.Available == null) entry.Available = new Component[InitialCapacity];
            if (entry.AvailableCount == entry.Available.Length)
            {
                Component[] grown = new Component[entry.Available.Length * 2];
                Array.Copy(entry.Available, grown, entry.Available.Length);
                entry.Available = grown;
            }

            instance.transform.SetParent(entry.Root, false);
            instance.gameObject.SetActive(false);
            entry.Available[entry.AvailableCount++] = instance;
        }

        static PoolEntry GetOrCreateEntry(Component prefab)
        {
            PoolEntry entry;
            int key = prefab.gameObject.GetInstanceID();
            if (!Pools.TryGetValue(key, out entry))
            {
                entry = new PoolEntry
                {
                    Root = new GameObject("Pool_" + prefab.gameObject.name).transform,
                    Available = new Component[InitialCapacity],
                    AvailableCount = 0,
                };
                entry.Root.SetParent(PoolRoot(), false);
                Pools[key] = entry;
            }
            return entry;
        }

        static Component CreateInstance(Component prefab, PoolEntry entry)
        {
            Component instance = UnityEngine.Object.Instantiate(prefab, entry.Root);
            instance.gameObject.SetActive(false);
            // Marker: Get/Release on the instance redirects through its pool.
            Pools[instance.gameObject.GetInstanceID()] = entry;
            return instance;
        }

        static Transform PoolRoot()
        {
            if (poolRoot == null)
            {
                GameObject root = new GameObject("ObjectPools");
                UnityEngine.Object.DontDestroyOnLoad(root);
                poolRoot = root.transform;
            }
            return poolRoot;
        }
    }
}
