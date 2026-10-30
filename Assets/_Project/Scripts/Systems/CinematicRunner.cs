using System;
using System.Collections;
using UnityEngine;

namespace LightShadow.Systems
{
    // Hidden helper that owns the cinematic coroutines. CinematicDirector is a
    // static class, and static classes cannot start coroutines themselves, so
    // PlayShot() parks this tiny runner on a DontDestroyOnLoad object.
    public class CinematicRunner : MonoBehaviour
    {
        public static CinematicRunner Instance { get; private set; }

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Run(IEnumerator routine)
        {
            StartCoroutine(routine);
        }

        // Reflection helpers shared by the director. They look the Cinemachine
        // 3.x types up at runtime so this file compiles (and the game runs)
        // even if the package were ever removed; PlayShot just warns and skips.
        public static class Cine
        {
            public const string CameraType = "Unity.Cinemachine.CinemachineCamera";
            public const string ComposerType = "Unity.Cinemachine.CinemachinePositionComposer";
            public const string BrainType = "Unity.Cinemachine.CinemachineBrain";

            public static Type Find(string fullName)
            {
                System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    Type[] types;
                    try
                    {
                        types = assemblies[i].GetTypes();
                    }
                    catch (Exception)
                    {
                        continue; // a dynamic/unloadable assembly; skip it
                    }
                    for (int j = 0; j < types.Length; j++)
                    {
                        if (types[j].FullName == fullName) return types[j];
                    }
                }
                return null;
            }

            // Sets the live priority on a CinemachineCamera: the property first
            // (CM3 exposes Priority), then the serialized m_Priority field.
            public static void SetPriority(Component camera, int priority)
            {
                if (camera == null) return;
                var property = camera.GetType().GetProperty("Priority");
                if (property != null && property.CanWrite)
                {
                    property.SetValue(camera, priority);
                    return;
                }
                var field = camera.GetType().GetField("m_Priority");
                if (field != null) field.SetValue(camera, priority);
            }
        }
    }
}
