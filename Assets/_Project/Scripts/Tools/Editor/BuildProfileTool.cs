using System.IO;
using UnityEditor;
using UnityEngine;

namespace LightShadow.Tools
{
    /// <summary>
    /// Repairs the Unity 6 build-profile assets the editor expects at
    /// Assets/Settings/Build Profiles/. A missing active profile made batch runs
    /// abort at startup ("Failed to read the specified build profile"); these two
    /// assets plus a valid active-profile selection keep the editor state consistent.
    /// Creation is best-effort: builds and validation use explicit BuildPlayerOptions
    /// and do not depend on profiles. If the editor ever fails to boot again with a
    /// profile error, delete Library/EditorUserBuildSettings.asset (machine-local,
    /// regenerated) and rerun this tool.
    /// Batch entry point: LightShadow.Tools.BuildProfileTool.EnsureBatch
    /// </summary>
    public static class BuildProfileTool
    {
        private const string WindowsProfilePath = "Assets/Settings/Build Profiles/Windows.asset";
        private const string AndroidProfilePath = "Assets/Settings/Build Profiles/Android.asset";

        // Platform identifiers as serialized by Unity's own internal profiles (m_PlatformId).
        private const string WindowsPlatformId = "4e3c793746204150860bf175a9a41a05";
        private const string AndroidPlatformId = "84a3bb9e7420477f885e98145999eb20";

        [MenuItem("LightShadow/7. Repair Build Profiles", priority = 7)]
        public static void EnsureMenu()
        {
            EnsureAll();
            Debug.Log("[LightShadow] Build profiles ensured.");
        }

        /// <summary>Batch entry: create any missing build profile, then exit 0.</summary>
        public static void EnsureBatch()
        {
            EnsureAll();
            EditorApplication.Exit(0);
        }

        // Asks the editor for the real platform id of a build target (internal API,
        // reached by reflection) so no GUID has to be guessed. The hard-coded Android
        // id turned out to be the Web platform in the editor UI (Unity pass 2026-11-28).
        // Returns the fallback when the editor does not expose it.
        internal static string ResolvePlatformId(BuildTarget target, int subtarget, string fallback)
        {
            try
            {
                System.Type util = typeof(UnityEditor.Build.Profile.BuildProfile).Assembly
                    .GetType("UnityEditor.Build.Profile.BuildProfileModuleUtil");
                if (util == null) return fallback;
                foreach (var method in util.GetMethods(System.Reflection.BindingFlags.Static
                    | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
                {
                    if (method.Name != "GetPlatformId") continue;
                    var parameters = method.GetParameters();
                    if (parameters.Length != 2 || parameters[0].ParameterType != typeof(BuildTarget)) continue;
                    object sub = System.Enum.ToObject(parameters[1].ParameterType, subtarget);
                    object result = method.Invoke(null, new object[] { target, sub });
                    string text = result != null ? result.ToString() : null;
                    if (!string.IsNullOrEmpty(text) && text.Length == 32) return text;
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("[LightShadow] Could not resolve the platform id for " + target + ": " + e.Message);
            }
            return fallback;
        }

        // Rewrites m_PlatformId on an existing profile when it does not match the editor's id.
        private static void RepairPlatformId(UnityEditor.Build.Profile.BuildProfile profile, string platformId)
        {
            if (profile == null) return;
            var so = new SerializedObject(profile);
            var id = so.FindProperty("m_PlatformId");
            if (id == null || id.stringValue == platformId) return;
            Debug.Log("[LightShadow] Repairing build profile platform id " + id.stringValue + " -> " + platformId);
            id.stringValue = platformId;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void EnsureAll()
        {
            string windowsId = ResolvePlatformId(BuildTarget.StandaloneWindows64, 2, WindowsPlatformId);
            string androidId = ResolvePlatformId(BuildTarget.Android, 0, AndroidPlatformId);
            var windows = Ensure(WindowsProfilePath, BuildTarget.StandaloneWindows64, windowsId, 2);
            var android = Ensure(AndroidProfilePath, BuildTarget.Android, androidId, 0);
            RepairPlatformId(windows, windowsId);
            RepairPlatformId(android, androidId);
            if (windows != null)
                TryMakeActive(windows);
            AssetDatabase.SaveAssets();
        }

        /// <summary>Creates the profile asset if missing. Returns the profile or null.</summary>
        private static UnityEditor.Build.Profile.BuildProfile Ensure(
            string path, BuildTarget target, string platformId, int subtarget)
        {
            try
            {
                if (File.Exists(path))
                {
                    Debug.Log("[LightShadow] Build profile already present: " + path);
                    return AssetDatabase.LoadAssetAtPath<UnityEditor.Build.Profile.BuildProfile>(path);
                }

                var folder = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                AssetDatabase.Refresh();

                var profile = ScriptableObject.CreateInstance<UnityEditor.Build.Profile.BuildProfile>();
                AssetDatabase.CreateAsset(profile, path);

                // Core serialized fields; platform-settings sub-assets are created by the
                // editor UI on first open if needed — batch builds do not read them.
                var so = new SerializedObject(profile);
                var t = so.FindProperty("m_BuildTarget");
                if (t != null) t.intValue = (int)target;
                var s = so.FindProperty("m_Subtarget");
                if (s != null) s.intValue = subtarget;
                var id = so.FindProperty("m_PlatformId");
                if (id != null) id.stringValue = platformId;
                so.ApplyModifiedPropertiesWithoutUndo();

                AssetDatabase.ImportAsset(path);
                Debug.Log("[LightShadow] Created build profile '" + path + "' for " + target);
                return profile;
            }
            catch (System.Exception e)
            {
                // Non-fatal: batch builds use explicit BuildPlayerOptions, not profiles.
                Debug.LogWarning("[LightShadow] Could not create build profile '" + path + "': " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Re-selects the Windows profile as the editor's active build profile so the next
        /// boot resolves it. Uses the public static API from Unity 6000.4.
        /// </summary>
        private static void TryMakeActive(UnityEditor.Build.Profile.BuildProfile profile)
        {
            try
            {
                UnityEditor.Build.Profile.BuildProfile.SetActiveBuildProfile(profile);
                Debug.Log("[LightShadow] Active build profile set to the Windows profile.");
            }
            catch (System.Exception e)
            {
                Debug.Log("[LightShadow] Could not set active build profile (non-fatal): " + e.Message);
            }
        }
    }
}
