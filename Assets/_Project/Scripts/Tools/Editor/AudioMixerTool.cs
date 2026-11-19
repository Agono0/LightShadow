using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using LightShadow.Audio;

namespace LightShadow.Tools
{
    /// <summary>
    /// Builds the real AudioMixer asset (D15 follow-up, AGENT_REPORT_AUDIO_MIXER
    /// §2): Assets/_Project/Audio/Mixers/M_Main.mixer with the category bus tree
    /// (Master > Music/Ambience/SFX/Voice/Shadow/UI, each with a snapshot-owned
    /// duck child), six snapshots (Explore/Stalk/Chase/Hide/MemoryFlash/Dawn) and
    /// the 14 exposed parameters the runtime bridge drives.
    ///
    /// The mixer is created through Unity's own editor serialization pipeline
    /// (AssetDatabase + the internal UnityEditor.Audio controller classes) — never
    /// by writing YAML text. Every internal API here was verified against Unity's
    /// published reference source for editor 6000.4 (spec §2.2); each is reached
    /// via reflection because the types are internal. ValidateMixer() asserts the
    /// finished asset and fails the batch loudly on any mismatch.
    ///
    /// Batch entry: LightShadow.Tools.AudioMixerTool.EnsureBatch — called from
    /// SceneBuilder.BuildAll right after AudioImportTool.ImportAll() so the mixer
    /// exists before any scene is built.
    /// </summary>
    public static class AudioMixerTool
    {
        const string MixersFolder = "Assets/_Project/Audio/Mixers";
        public const string MixerPath = MixersFolder + "/M_Main.mixer";

        // ---- names shared with the runtime bridge (AudioSnapshotController) ----

        // Category groups carry the user sliders; duck children carry snapshot
        // ducking. Sources route to the category group only (spec §2.1).
        public static readonly string[] CategoryGroupNames =
        {
            "Master", "Music", "Ambience", "SFX", "Voice", "Shadow", "UI",
        };

        // Aligned pairs: duck group -> parent category group.
        static readonly string[] DuckGroupNames =
        {
            "MusicDuck", "AmbienceDuck", "SfxDuck", "VoiceDuck", "ShadowDuck", "UiDuck",
        };
        static readonly string[] DuckParentNames =
        {
            "Music", "Ambience", "SFX", "Voice", "Shadow", "UI",
        };

        // Group name -> exposed user-volume parameter (the only params runtime
        // code may SetFloat; snapshots never control them, spec §4.1).
        static readonly Dictionary<string, string> GroupToUserParam = new Dictionary<string, string>
        {
            { "Master", "MasterVol" },
            { "Music", "MusicVol" },
            { "Ambience", "AmbienceVol" },
            { "SFX", "SfxVol" },
            { "Voice", "VoiceVol" },
            { "Shadow", "ShadowVol" },
            { "UI", "UiVol" },
        };

        // Duck group name -> exposed duck-volume parameter (snapshot-owned).
        static readonly Dictionary<string, string> DuckToParam = new Dictionary<string, string>
        {
            { "MusicDuck", "MusicDuckVol" },
            { "AmbienceDuck", "AmbienceDuckVol" },
            { "SfxDuck", "SfxDuckVol" },
            { "VoiceDuck", "VoiceDuckVol" },
            { "ShadowDuck", "ShadowDuckVol" },
            { "UiDuck", "UiDuckVol" },
        };

        public const string LowpassParam = "MasterLowpass";

        // ---- authored snapshot values (spec §3; data lives in the mixer asset) --

        // Duck gains in dB per snapshot, aligned with DuckGroupNames.
        static readonly float[][] DuckDb =
        {
            new[] {  0f,   0f,   0f,   0f,   0f,   0f  },  // Snapshot_Explore
            new[] { -2f,  -3f,  -1f,   0f,   1.5f, 0f  },  // Snapshot_Stalk
            new[] {  0f,  -6f,  -2f,  -6f,   2f,  -6f  },  // Snapshot_Chase
            new[] { -4f,  -4f,  -3f,   0f,  -3f,   0f  },  // Snapshot_Hide
            new[] { -4f, -15f, -12f,   0f, -15f, -12f  },  // Snapshot_MemoryFlash
            new[] {  1.5f, 0f,  1f,    0f,   0f,   0f  },  // Snapshot_Dawn
        };

        // Master low-pass cutoff in Hz per snapshot (900 Hz = the Hide muffle).
        static readonly float[] LowpassHz =
        {
            22000f, 12000f, 20000f, 900f, 12000f, 22000f,
        };

        static string[] SnapshotNames => AudioSnapshotController.SnapshotNames;
        const string StartSnapshot = AudioSnapshotController.SnapshotExplore;

        // ------------------------------------------------------------ entry ----

        [MenuItem("LightShadow/Audio/7. Build Audio Mixer", priority = 26)]
        public static void EnsureMenu()
        {
            Debug.Log("[LightShadow] " + BuildMixer());
            Debug.Log("[LightShadow] " + ValidateMixer());
        }

        // Called by Unity in batch mode:
        //   -executeMethod LightShadow.Tools.AudioMixerTool.EnsureBatch
        public static void EnsureBatch()
        {
            try
            {
                BuildMixer();
                string report = ValidateMixer();
                Debug.Log("[LightShadow] " + report);
                EditorApplication.Exit(report.Contains("RESULT: FAIL") ? 1 : 0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        // ----------------------------------------------------------- build ----

        /// <summary>Idempotent: keeps a structurally valid mixer, otherwise
        /// rebuilds it from scratch (groups/snapshots carry no other data).</summary>
        public static string BuildMixer()
        {
            EnsureFolder(MixersFolder);

            AudioMixer existing = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (existing != null && Validate(existing).Count == 0)
            {
                return "Audio mixer already valid at " + MixerPath;
            }

            if (existing != null) AssetDatabase.DeleteAsset(MixerPath);

            object controller = Activator.CreateInstance(ControllerType, true);
            object created = Call(controller, "CreateDefaultAsset", MixerPath);
            if (created is bool ok && !ok)
            {
                throw new InvalidOperationException("AudioMixerController.CreateDefaultAsset failed for " + MixerPath);
            }

            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (mixer == null)
            {
                throw new InvalidOperationException("Mixer asset did not import at " + MixerPath);
            }

            // ---- bus tree: category groups under Master, duck children under
            //      their category. Sources route to the category only. ----
            object masterGroup = GetMember(controller, "masterGroup");

            var groups = new Dictionary<string, object> { { "Master", masterGroup } };
            for (int i = 1; i < CategoryGroupNames.Length; i++)
            {
                string name = CategoryGroupNames[i];
                object group = Call(controller, "CreateNewGroup", name, false);
                Call(controller, "AddChildToParent", group, masterGroup);
                groups[name] = group;
            }

            for (int i = 0; i < DuckGroupNames.Length; i++)
            {
                object duck = Call(controller, "CreateNewGroup", DuckGroupNames[i], false);
                Call(controller, "AddChildToParent", duck, groups[DuckParentNames[i]]);
                groups[DuckGroupNames[i]] = duck;
            }

            // ---- Master low-pass (snapshot-owned cutoff, spec §2.1/§3) ----
            object lowpass = Activator.CreateInstance(EffectControllerType, "Lowpass");
            AssetDatabase.AddObjectToAsset((UnityEngine.Object)lowpass, mixer);
            int insertAt = 0; // head of the Master chain; the cutoff is unaffected
            try
            {
                if (GetMember(masterGroup, "effects") is Array effects) insertAt = effects.Length;
            }
            catch (MissingMemberException)
            {
                // effects member not found: insert at the head (before Attenuation).
            }
            Call(masterGroup, "InsertEffect", lowpass, insertAt);

            // ---- snapshots: rename the default "Snapshot", add the other five.
            //      Pattern = CloneNewSnapshotFromTarget minus the cloning (§2.2). ----
            Array snapshots = (Array)GetMember(controller, "snapshots");
            if (snapshots.Length != 1)
            {
                throw new InvalidOperationException(
                    "Expected the default mixer to ship exactly one snapshot, found " + snapshots.Length);
            }

            var snapshotObjects = new List<object>(SnapshotNames.Length);
            object explore = snapshots.GetValue(0);
            ((UnityEngine.Object)explore).name = AudioSnapshotController.SnapshotExplore;
            snapshotObjects.Add(explore);

            for (int i = 1; i < SnapshotNames.Length; i++)
            {
                object snapshot = Activator.CreateInstance(SnapshotControllerType, controller);
                AssetDatabase.AddObjectToAsset((UnityEngine.Object)snapshot, mixer);
                ((UnityEngine.Object)snapshot).name = SnapshotNames[i];
                snapshotObjects.Add(snapshot);

                Array grown = Array.CreateInstance(snapshots.GetType().GetElementType(), i + 1);
                Array.Copy(snapshots, grown, i);
                grown.SetValue(snapshot, i);
                snapshots = grown;
                SetMember(controller, "snapshots", grown);
            }

            SetMember(controller, "startSnapshot", explore);

            // ---- snapshot values: user params inert at 0 dB, ducks per table,
            //      low-pass per table (spec §3). ----
            string cutoffParameter = FindCutoffParameterName();
            for (int s = 0; s < snapshotObjects.Count; s++)
            {
                object snapshot = snapshotObjects[s];

                foreach (string categoryName in CategoryGroupNames)
                {
                    Call(groups[categoryName], "SetValueForVolume", controller, snapshot, 0f);
                }

                for (int d = 0; d < DuckGroupNames.Length; d++)
                {
                    Call(groups[DuckGroupNames[d]], "SetValueForVolume", controller, snapshot, DuckDb[s][d]);
                }

                Call(lowpass, "SetValueForParameter", controller, snapshot, cutoffParameter, LowpassHz[s]);
            }

            // ---- expose the 14 parameters (7 user + 6 duck + low-pass) and
            //      rewrite the "MyExposedParam" defaults in place (§2.2). ----
            var desiredByGroup = new Dictionary<object, string>();
            foreach (KeyValuePair<string, string> pair in GroupToUserParam)
            {
                desiredByGroup[groups[pair.Key]] = pair.Value;
            }
            foreach (KeyValuePair<string, string> pair in DuckToParam)
            {
                desiredByGroup[groups[pair.Key]] = pair.Value;
            }

            object volumeGuid = Call(masterGroup, "GetGUIDForVolume");
            object volumePath = Activator.CreateInstance(GroupParameterPathType, masterGroup, volumeGuid);
            Call(controller, "AddExposedParameter", volumePath);

            object cutoffGuid = Call(lowpass, "GetGUIDForParameter", cutoffParameter);
            object cutoffPath = Activator.CreateInstance(EffectParameterPathType, masterGroup, lowpass, cutoffGuid);
            Call(controller, "AddExposedParameter", cutoffPath);

            for (int i = 0; i < DuckGroupNames.Length; i++)
            {
                object duckGroup = groups[DuckGroupNames[i]];
                object duckGuid = Call(duckGroup, "GetGUIDForVolume");
                object duckPath = Activator.CreateInstance(GroupParameterPathType, duckGroup, duckGuid);
                Call(controller, "AddExposedParameter", duckPath);
            }

            for (int i = 0; i < CategoryGroupNames.Length - 1; i++) // Master already exposed
            {
                object group = groups[CategoryGroupNames[i + 1]];
                object guid = Call(group, "GetGUIDForVolume");
                object path = Activator.CreateInstance(GroupParameterPathType, group, guid);
                Call(controller, "AddExposedParameter", path);
            }

            RenameExposedParameters(controller, desiredByGroup);

            EditorUtility.SetDirty(mixer);
            AssetDatabase.SaveAssets();

            var problems = Validate(mixer);
            if (problems.Count > 0)
            {
                throw new InvalidOperationException("Mixer built but invalid: " + string.Join(" | ", problems));
            }

            return "Audio mixer built at " + MixerPath + " ("
                + CategoryGroupNames.Length + " category groups, "
                + DuckGroupNames.Length + " duck children, "
                + SnapshotNames.Length + " snapshots, 14 exposed parameters)";
        }

        static void RenameExposedParameters(object controller, Dictionary<object, string> desiredByGroup)
        {
            Array exposed = (Array)GetMember(controller, "exposedParameters");
            int renamed = 0;

            for (int i = 0; i < exposed.Length; i++)
            {
                object entry = exposed.GetValue(i);
                object path = GetMember(entry, "parameterPath");

                string desired = null;
                if (path.GetType() == EffectParameterPathType) desired = LowpassParam;
                else if (GetMember(path, "group") is object group && desiredByGroup.TryGetValue(group, out string name))
                {
                    desired = name;
                }

                if (desired == null) continue;
                SetMember(entry, "name", desired);
                exposed.SetValue(entry, i);
                renamed++;
            }

            SetMember(controller, "exposedParameters", exposed);

            if (renamed != desiredByGroup.Count + 1) // + the low-pass
            {
                throw new InvalidOperationException(
                    "Renamed " + renamed + " exposed parameters, expected " + (desiredByGroup.Count + 1));
            }
        }

        // The built-in Lowpass parameter name is discovered, never hardcoded
        // (spec §2.2 — enumerate MixerEffectDefinitions at build time).
        static string FindCutoffParameterName()
        {
            object[] definitions = (object[])CallStatic(MixerEffectDefinitionsType, "GetEffectParameters", "Lowpass");
            foreach (object definition in definitions)
            {
                if (GetMember(definition, "name") is string name
                    && name.IndexOf("cutoff", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return name;
                }
            }

            throw new InvalidOperationException("Lowpass effect has no Cutoff parameter (Unity internals drifted).");
        }

        // ------------------------------------------------------- validation ----

        /// <summary>Asserts the finished asset: exact groups + duck children,
        /// six snapshots, start snapshot, 14 exposed params. Returns problems;
        /// an empty list means valid.</summary>
        static List<string> Validate(AudioMixer mixer)
        {
            var problems = new List<string>();
            if (mixer == null)
            {
                problems.Add("Mixer asset missing at " + MixerPath);
                return problems;
            }

            foreach (string name in CategoryGroupNames)
            {
                if (FindGroupExact(mixer, name) == null) problems.Add("Missing group: " + name);
            }
            foreach (string name in DuckGroupNames)
            {
                if (FindGroupExact(mixer, name) == null) problems.Add("Missing duck child: " + name);
            }

            foreach (string name in SnapshotNames)
            {
                if (mixer.FindSnapshot(name) == null) problems.Add("Missing snapshot: " + name);
            }

            AudioMixerSnapshot start = GetMember(mixer, "startSnapshot") as AudioMixerSnapshot;
            if (start == null || start.name != StartSnapshot)
            {
                problems.Add("startSnapshot should be " + StartSnapshot
                    + ", found " + (start != null ? start.name : "null"));
            }

            foreach (string parameter in ExposedParameterNames())
            {
                if (!mixer.GetFloat(parameter, out _)) problems.Add("Missing exposed parameter: " + parameter);
            }

            return problems;
        }

        public static string[] ExposedParameterNames()
        {
            var names = new List<string>(15);
            foreach (string name in CategoryGroupNames) names.Add(GroupToUserParam[name]);
            foreach (string name in DuckGroupNames) names.Add(DuckToParam[name]);
            names.Add(LowpassParam);
            return names.ToArray();
        }

        /// <summary>Reflection read of the mixer controller's start snapshot
        /// (startSnapshot lives on the internal AudioMixerController, not the
        /// public AudioMixer surface).</summary>
        public static AudioMixerSnapshot ReadStartSnapshot(AudioMixer mixer)
        {
            return GetMember(mixer, "startSnapshot") as AudioMixerSnapshot;
        }

        /// <summary>Full validation report ending in RESULT: PASS / RESULT: FAIL
        /// (batch-greppable, same convention as the scene validator).</summary>
        public static string ValidateMixer()
        {
            var report = new System.Text.StringBuilder();
            List<string> problems = Validate(AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath));
            report.AppendLine("Audio mixer (" + MixerPath + "):");
            report.AppendLine("- Groups: " + CategoryGroupNames.Length + " category + "
                + DuckGroupNames.Length + " duck children");
            report.AppendLine("- Snapshots: " + string.Join(", ", SnapshotNames));
            report.AppendLine("- Exposed parameters: " + ExposedParameterNames().Length);
            report.AppendLine(problems.Count == 0 ? "RESULT: PASS" : "RESULT: FAIL");
            foreach (string problem in problems) report.AppendLine("- " + problem);
            return report.ToString();
        }

        static AudioMixerGroup FindGroupExact(AudioMixer mixer, string name)
        {
            AudioMixerGroup[] matches = mixer.FindMatchingGroups(name);
            for (int i = 0; i < matches.Length; i++)
            {
                if (matches[i] != null && matches[i].name == name) return matches[i];
            }
            return null;
        }

        // -------------------------------------------------------- reflection ----

        static Type ControllerType => EditorType("UnityEditor.Audio.AudioMixerController");
        static Type SnapshotControllerType => EditorType("UnityEditor.Audio.AudioMixerSnapshotController");
        static Type EffectControllerType => EditorType("UnityEditor.Audio.AudioMixerEffectController");
        static Type GroupParameterPathType => EditorType("UnityEditor.Audio.AudioGroupParameterPath");
        static Type EffectParameterPathType => EditorType("UnityEditor.Audio.AudioEffectParameterPath");
        static Type MixerEffectDefinitionsType => EditorType("UnityEditor.Audio.MixerEffectDefinitions");

        static readonly BindingFlags Any =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        static Type EditorType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName);
                if (type != null) return type;
            }

            throw new TypeLoadException("UnityEditor internal type not found for 6000.4: " + fullName);
        }

        static object GetMember(object target, string name)
        {
            PropertyInfo property = target.GetType().GetProperty(name, Any);
            if (property != null) return property.GetValue(target, null);
            FieldInfo field = target.GetType().GetField(name, Any);
            if (field != null) return field.GetValue(target);
            throw new MissingMemberException(target.GetType().FullName + "." + name + " not found.");
        }

        static void SetMember(object target, string name, object value)
        {
            PropertyInfo property = target.GetType().GetProperty(name, Any);
            if (property != null)
            {
                property.SetValue(target, value, null);
                return;
            }

            FieldInfo field = target.GetType().GetField(name, Any);
            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }

            throw new MissingMemberException(target.GetType().FullName + "." + name + " not found.");
        }

        static object Call(object target, string method, params object[] args)
        {
            MethodInfo info = FindMethod(target.GetType(), method, args);
            if (info == null)
            {
                throw new MissingMethodException(target.GetType().FullName, method);
            }

            return info.Invoke(target, args);
        }

        static object CallStatic(Type type, string method, params object[] args)
        {
            MethodInfo info = FindMethod(type, method, args);
            if (info == null)
            {
                throw new MissingMethodException(type.FullName, method);
            }

            return info.Invoke(null, args);
        }

        // CoreCLR's GetMethod(name, flags, binder, types, modifiers) throws on a null
        // Type[] — resolve by name + arity, then by argument compatibility for overloads.
        static MethodInfo FindMethod(Type type, string method, object[] args)
        {
            MethodInfo[] candidates = type.GetMethods(Any)
                .Where(m => m.Name == method && m.GetParameters().Length == args.Length)
                .ToArray();
            if (candidates.Length == 1) return candidates[0];

            foreach (MethodInfo candidate in candidates)
            {
                ParameterInfo[] parameters = candidate.GetParameters();
                bool matches = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (args[i] == null) continue;
                    if (!parameters[i].ParameterType.IsInstanceOfType(args[i]))
                    {
                        matches = false;
                        break;
                    }
                }
                if (matches) return candidate;
            }

            return candidates.Length > 0 ? candidates[0] : null;
        }

        static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            string leaf = Path.GetFileName(folder);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
