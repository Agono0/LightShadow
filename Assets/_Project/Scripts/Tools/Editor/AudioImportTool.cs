using System.IO;
using UnityEditor;
using UnityEngine;
using LightShadow.Audio;

namespace LightShadow.Tools
{
    /// <summary>
    /// Builds the runtime AudioLibrary from the imported CC0 packs under
    /// Assets/ThirdParty: copies the chosen files into _Project/Audio with the
    /// project's sound-name convention, configures their importers, and writes
    /// AudioLibrary.asset. Batch entry: AudioImportTool.ImportBatch — called
    /// from SceneBuilder.BuildAll so the library always exists for the builder
    /// to wire onto the Systems object.
    ///
    /// Sources (all logged in Docs/ASSET_LICENSES.md):
    ///   Kenney Interface/RPG/Impact Sounds (CC0), qubodup Ghost Moans (CC0),
    ///   tcarisland Abandoned Building Ambience (CC-BY 3.0).
    /// </summary>
    public static class AudioImportTool
    {
        const string ThirdParty = "Assets/ThirdParty";
        const string SfxFolder = "Assets/_Project/Audio/SFX";
        const string AmbFolder = "Assets/_Project/Audio/Ambience";
        const string MusicFolder = "Assets/_Project/Audio/Music";
        const string LibraryPath = "Assets/_Project/Audio/AudioLibrary.asset";

        [MenuItem("LightShadow/Art/5. Import Audio Library", priority = 25)]
        public static void ImportMenu()
        {
            Debug.Log("[LightShadow] " + ImportAll());
        }

        public static void ImportBatch()
        {
            Debug.Log("[LightShadow] " + ImportAll());
            EditorApplication.Exit(0);
        }

        public static string ImportAll()
        {
            EnsureFolder(SfxFolder);
            EnsureFolder(AmbFolder);
            EnsureFolder(MusicFolder);

            var library = AssetDatabase.LoadAssetAtPath<AudioLibrary>(LibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<AudioLibrary>();
                AssetDatabase.CreateAsset(library, LibraryPath);
            }

            // ---- interface / quiz ------------------------------------------------
            library.quizCorrect = ImportCopy("KenneyInterfaceSounds/Audio/confirmation_002.ogg", SfxFolder, "SFX_QuizCorrect");
            library.quizWrong = ImportCopy("KenneyInterfaceSounds/Audio/error_004.ogg", SfxFolder, "SFX_QuizWrong");
            library.click = ImportCopy("KenneyInterfaceSounds/Audio/click_002.ogg", SfxFolder, "SFX_Click");
            library.pickup = ImportCopy("KenneyInterfaceSounds/Audio/pluck_002.ogg", SfxFolder, "SFX_Pickup");
            library.power = ImportCopy("KenneyImpactSounds/Audio/impactMetal_heavy_002.ogg", SfxFolder, "SFX_Power");
            library.doorCreak = ImportCopy("KenneyRPAAudio/Audio/creak1.ogg", SfxFolder, "SFX_DoorCreak");

            // ---- footsteps (surface variants) ------------------------------------
            library.footstepConcrete = new AudioClip[4];
            for (int i = 0; i < 4; i++)
            {
                library.footstepConcrete[i] = ImportCopy(
                    "KenneyImpactSounds/Audio/footstep_concrete_00" + i + ".ogg",
                    SfxFolder, "SFX_FootstepConcrete_" + i);
            }
            library.footstepWood = new AudioClip[2];
            for (int i = 0; i < 2; i++)
            {
                library.footstepWood[i] = ImportCopy(
                    "KenneyImpactSounds/Audio/footstep_wood_00" + i + ".ogg",
                    SfxFolder, "SFX_FootstepWood_" + i);
            }

            // ---- the Shadow's voice ----------------------------------------------
            string[] moanSources =
            {
                "GhostMoansCC0/wav/qubodup-GhostMoan01.wav",
                "GhostMoansCC0/wav/qubodup-GhostMoan02.wav",
                "GhostMoansCC0/wav/qubodup-GhostMoan03.wav",
            };
            library.shadowMoans = new AudioClip[moanSources.Length];
            for (int i = 0; i < moanSources.Length; i++)
            {
                library.shadowMoans[i] = ImportCopy(moanSources[i], SfxFolder, "SFX_ShadowMoan_" + i);
            }

            // ---- ambience ---------------------------------------------------------
            library.ambienceNight = ImportCopy(
                "AbandonedBuildingAmbience/abandonedbuildingambience2.mp3", AmbFolder, "AMB_SchoolNight");

            // ---- music (Phase 1) ---------------------------------------------------
            // These files are downloaded directly into _Project (staged per
            // Docs/SOURCING_FONTS_MUSIC.md, logged in Docs/ASSET_LICENSES.md),
            // so unlike the pack copies above there is no ThirdParty source;
            // the tool only configures their importers and links the library.
            library.musicBedLongNoteOne = ImportPlaced(AmbFolder, "MUS_Bed_LongNoteOne.mp3");
            library.endingHopeGold = ImportPlaced(MusicFolder, "MUS_Ending_HopeGold.mp3");
            library.endingDawnlessGrey = ImportPlaced(MusicFolder, "MUS_Ending_DawnlessGrey.mp3");

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            return "Audio library built at " + LibraryPath;
        }

        /// <summary>Copies a pack file into _Project under the convention name
        /// (once; the copy is the runtime asset, the pack stays read-only) and
        /// applies sane importer settings. Returns the loaded clip or null.</summary>
        static AudioClip ImportCopy(string relativePackPath, string targetFolder, string targetName)
        {
            string sourcePath = ThirdParty + "/" + relativePackPath;
            if (!File.Exists(sourcePath))
            {
                Debug.LogWarning("[LightShadow] Audio source missing: " + sourcePath);
                return null;
            }

            string extension = Path.GetExtension(sourcePath);
            string targetPath = targetFolder + "/" + targetName + extension;
            if (!File.Exists(targetPath))
            {
                File.Copy(sourcePath, targetPath, false);
                AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            }

            ConfigureImporter(targetPath);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(targetPath);
        }

        /// <summary>Wires a music file that already lives in _Project (downloaded
        /// and license-logged, no ThirdParty pack copy) into the library:
        /// configures the importer and returns the loaded clip or null.</summary>
        static AudioClip ImportPlaced(string targetFolder, string fileName)
        {
            string targetPath = targetFolder + "/" + fileName;
            if (!File.Exists(targetPath))
            {
                Debug.LogWarning("[LightShadow] Music file missing: " + targetPath);
                return null;
            }

            ConfigureImporter(targetPath);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(targetPath);
        }

        static void ConfigureImporter(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null) return;
            bool changed = false;
            if (!importer.forceToMono) { /* keep original channels */ }
            if (importer.loadInBackground == false) { importer.loadInBackground = true; changed = true; }
            if (changed)
            {
                importer.SaveAndReimport();
            }
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
