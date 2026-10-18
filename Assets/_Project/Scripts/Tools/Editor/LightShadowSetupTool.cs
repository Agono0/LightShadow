using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LightShadow.Tools
{
    public static class LightShadowSetupTool
    {
        private const string CompanyName = "LightShadow Team";
        private const string ProductName = "LightShadow";
        private const string AndroidApplicationId = "com.lightshadowteam.lightshadow";
        private const string MobileRpAssetPath = "Assets/Settings/Mobile_RPAsset.asset";
        private const float MobileRenderScale = 0.8f;
        private const int ActiveInputHandlingBoth = 2;
        private const string PlayerSettingsAssetPath = "ProjectSettings/ProjectSettings.asset";
        private const string QualitySettingsAssetPath = "ProjectSettings/QualitySettings.asset";

        [MenuItem("LightShadow/1. Project Setup", priority = 1)]
        public static void RunProjectSetup()
        {
            var notes = new StringBuilder();

            ApplyVersionControlSettings(notes);
            ApplyPlayerIdentity(notes);
            ApplyAndroidPlayerSettings(notes);
            ApplyOrientation(notes);
            bool restartRequired = ApplyActiveInputHandling(notes);
            ApplyMobileUrpQuality(notes);
            ValidateUrpQualityWiring(notes);
            WarnIfAndroidModuleMissing(notes);

            AssetDatabase.SaveAssets();

            string header = notes.Length > 0
                ? "LightShadow setup finished. Changes applied:"
                : "LightShadow setup finished. Everything was already configured.";
            Debug.Log("[LightShadow] " + header + "\n" + notes);

            if (restartRequired)
            {
                EditorUtility.DisplayDialog(
                    "LightShadow - Restart Unity",
                    "Active Input Handling was changed to \"Both\" so keyboard/mouse input and the touch layer can work together.\n\nUnity must be restarted for this change to take effect.\nClose and reopen the editor now, then continue with \"LightShadow > 2. Generate Questions\".",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "LightShadow - Setup",
                    header + "\n\n" + notes,
                    "OK");
            }
        }

        private static void ApplyVersionControlSettings(StringBuilder notes)
        {
            if (EditorSettings.externalVersionControl != "Visible Meta Files")
            {
                EditorSettings.externalVersionControl = "Visible Meta Files";
                notes.AppendLine("- Version Control mode -> Visible Meta Files (doc 6.5)");
            }

            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                EditorSettings.serializationMode = SerializationMode.ForceText;
                notes.AppendLine("- Asset serialization -> Force Text (doc 6.5)");
            }
        }

        private static void ApplyPlayerIdentity(StringBuilder notes)
        {
            if (PlayerSettings.companyName != CompanyName)
            {
                PlayerSettings.companyName = CompanyName;
                notes.AppendLine("- Company Name -> " + CompanyName + " (change it in Player Settings if the team picks a different name)");
            }

            if (PlayerSettings.productName != ProductName)
            {
                PlayerSettings.productName = ProductName;
                notes.AppendLine("- Product Name -> " + ProductName);
            }
        }

        private static void ApplyAndroidPlayerSettings(StringBuilder notes)
        {
            try
            {
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, AndroidApplicationId);
            }
            catch (System.Exception exception)
            {
                notes.AppendLine("! Could not set the Android application ID: " + exception.Message);
            }

            if (PlayerSettings.Android.minSdkVersion != AndroidSdkVersions.AndroidApiLevel29)
            {
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
                notes.AppendLine("- Android Minimum API Level -> 29 / Android 10 (doc 6.8)");
            }

            try
            {
                if (PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) != ScriptingImplementation.IL2CPP)
                {
                    PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
                    notes.AppendLine("- Android Scripting Backend -> IL2CPP (doc 6.8)");
                }
            }
            catch (System.Exception exception)
            {
                notes.AppendLine("! Could not set the Android scripting backend: " + exception.Message);
            }

            if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
            {
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                notes.AppendLine("- Android Target Architecture -> ARM64 (doc 6.8)");
            }

            if (EditorUserBuildSettings.androidBuildSubtarget != MobileTextureSubtarget.ASTC)
            {
                EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
                notes.AppendLine("- Android Texture Compression -> ASTC (doc 6.8; per-machine setting, not stored in git)");
            }
        }

        private static void ApplyOrientation(StringBuilder notes)
        {
            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.AutoRotation)
            {
                PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
                notes.AppendLine("- Default Orientation -> Auto Rotation restricted to landscape (doc 6.8)");
            }

            if (PlayerSettings.allowedAutorotateToPortrait)
            {
                PlayerSettings.allowedAutorotateToPortrait = false;
                notes.AppendLine("- Portrait orientation -> disabled");
            }

            if (PlayerSettings.allowedAutorotateToPortraitUpsideDown)
            {
                PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
                notes.AppendLine("- Portrait upside-down orientation -> disabled");
            }

            if (!PlayerSettings.allowedAutorotateToLandscapeLeft)
            {
                PlayerSettings.allowedAutorotateToLandscapeLeft = true;
                notes.AppendLine("- Landscape Left -> allowed");
            }

            if (!PlayerSettings.allowedAutorotateToLandscapeRight)
            {
                PlayerSettings.allowedAutorotateToLandscapeRight = true;
                notes.AppendLine("- Landscape Right -> allowed");
            }
        }

        private static bool ApplyActiveInputHandling(StringBuilder notes)
        {
            Object[] settingsAssets = AssetDatabase.LoadAllAssetsAtPath(PlayerSettingsAssetPath);
            if (settingsAssets == null || settingsAssets.Length == 0)
            {
                notes.AppendLine("! Could not load PlayerSettings to change Active Input Handling - set it manually in Project Settings > Player (doc 6.8).");
                return false;
            }

            var serializedSettings = new SerializedObject(settingsAssets[0]);
            SerializedProperty handler = serializedSettings.FindProperty("activeInputHandler");
            if (handler == null)
            {
                notes.AppendLine("! activeInputHandler property not found - set Active Input Handling manually in Project Settings > Player (doc 6.8).");
                return false;
            }

            if (handler.intValue == ActiveInputHandlingBoth)
            {
                return false;
            }

            handler.intValue = ActiveInputHandlingBoth;
            serializedSettings.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            notes.AppendLine("- Active Input Handling -> Both (doc 6.8). UNITY RESTART REQUIRED.");
            return true;
        }

        private static void ApplyMobileUrpQuality(StringBuilder notes)
        {
            var mobileAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(MobileRpAssetPath);
            if (mobileAsset == null)
            {
                notes.AppendLine("! " + MobileRpAssetPath + " not found - verify the URP mobile quality asset manually (doc 6.8).");
                return;
            }

            var serializedAsset = new SerializedObject(mobileAsset);
            bool changed = false;

            SerializedProperty renderScale = serializedAsset.FindProperty("m_RenderScale");
            if (renderScale != null && !Mathf.Approximately(renderScale.floatValue, MobileRenderScale))
            {
                renderScale.floatValue = MobileRenderScale;
                changed = true;
                notes.AppendLine("- Mobile URP render scale -> " + MobileRenderScale + " (doc 6.8)");
            }

            changed |= DisableSerializedFlag(serializedAsset, "m_MainLightShadowsSupported",
                "- Mobile URP real-time main-light shadows -> off (doc 6.8)", notes);
            changed |= DisableSerializedFlag(serializedAsset, "m_AdditionalLightShadowsSupported",
                "- Mobile URP real-time additional-light shadows -> off (doc 6.8)", notes);
            changed |= DisableSerializedFlag(serializedAsset, "m_SoftShadowsSupported",
                "- Mobile URP soft shadows -> off (doc 6.8)", notes);

            if (changed)
            {
                serializedAsset.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.SaveAssets();
            }
        }

        private static bool DisableSerializedFlag(SerializedObject serializedObject, string propertyName, string note, StringBuilder notes)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.intValue == 0)
            {
                return false;
            }

            property.intValue = 0;
            notes.AppendLine(note);
            return true;
        }

        private static void ValidateUrpQualityWiring(StringBuilder notes)
        {
            Object[] settingsAssets = AssetDatabase.LoadAllAssetsAtPath(QualitySettingsAssetPath);
            if (settingsAssets == null || settingsAssets.Length == 0)
            {
                return;
            }

            var serializedSettings = new SerializedObject(settingsAssets[0]);
            SerializedProperty levels = serializedSettings.FindProperty("m_QualitySettings");
            if (levels == null || !levels.isArray)
            {
                return;
            }

            bool allWired = true;
            for (int i = 0; i < levels.arraySize; i++)
            {
                SerializedProperty level = levels.GetArrayElementAtIndex(i);
                SerializedProperty pipelineProperty = level.FindPropertyRelative("customRenderPipeline");
                if (pipelineProperty == null || pipelineProperty.objectReferenceValue == null)
                {
                    SerializedProperty nameProperty = level.FindPropertyRelative("name");
                    string levelName = nameProperty != null ? nameProperty.stringValue : "index " + i;
                    allWired = false;
                    notes.AppendLine("! Quality level \"" + levelName + "\" has no URP asset assigned (doc 6.8).");
                }
            }

            if (!allWired)
            {
                notes.AppendLine("  Assign the URP pipeline assets in Project Settings > Quality.");
            }
        }

        private static void WarnIfAndroidModuleMissing(StringBuilder notes)
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                notes.AppendLine("! Android build support module is not installed - add \"Android Build Support\" in Unity Hub before building APKs (doc 6.3, 6.8).");
            }
        }
    }
}
