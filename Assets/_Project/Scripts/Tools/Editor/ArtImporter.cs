using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LightShadow.Tools
{
    // Programmatic import pipeline for the premium-pass art assets.
    // Everything in this file runs headlessly from the Lead's batch command
    // (LightShadow.Tools.ArtImporter.ImportAllBatch) - no hand-edited YAML:
    //   1. textures under _Project/Textures/Art get correct sRGB/normal/linear
    //      settings, 2048 on Windows, a 1024 Android override, mipmaps and
    //      streaming; roughness maps get an inverted-alpha "smoothness" PNG
    //      that URP/Lit can actually read;
    //   2. Kenney FBX models get unit-safe import with no blend shapes,
    //      animations or colliders, medium mesh compression and generated
    //      lightmap UVs; the materials Unity auto-generates next to the FBX
    //      are converted in place to URP/Lit so nothing renders pink;
    //   3. hero FBX models from Assets/_Project/Models/Hero (built by the
    //      Art/pipeline Blender scripts) get the same unit-safe static-prop
    //      import with their own HeroModels settings table;
    //   4. surface materials (Mat_Art_*) are built from the texture sets;
    //   5. special prefabs (Shadow_Art, Hands_Art, DustMotes) are assembled;
    //   6. the existing TheShadow and Player prefabs get their art wiring;
    //   7. the Post_Night / Post_Dawn volume profiles gain fog, bloom and
    //      film grain through the VolumeProfile API only.
    // Every step is idempotent: running it twice changes nothing.
    public static class ArtImporter
    {
        // ------------------------------------------------------------- paths
        const string ThirdPartyFolder = "Assets/ThirdParty";
        const string TexturesFolder = "Assets/_Project/Textures/Art";
        const string ProcessedTexturesFolder = TexturesFolder + "/Generated";
        const string MaterialsFolder = "Assets/_Project/Materials/Art";
        const string ArtPrefabsFolder = "Assets/_Project/Prefabs/Art";
        const string PostNightPath = "Assets/_Project/Data/Post/Post_Night.asset";
        const string PostDawnPath = "Assets/_Project/Data/Post/Post_Dawn.asset";
        const string ShadowArtPrefabPath = ArtPrefabsFolder + "/Shadow_Art.prefab";
        const string HandsArtPrefabPath = ArtPrefabsFolder + "/Hands_Art.prefab";
        const string DustMotesPrefabPath = ArtPrefabsFolder + "/DustMotes.prefab";
        const string EnemyShadowPrefabPath = "Assets/_Project/Prefabs/Enemy/TheShadow.prefab";
        const string PlayerPrefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";

        // ------------------------------------------------------- pack config
        // The packs extracted under Assets/ThirdParty (see Docs/ASSET_LICENSES.md).
        static readonly string[] PackFolders =
        {
            "KenneyFurnitureKit", "KenneyNatureKit", "KenneyGraveyardKit", "KenneyBlockyCharacters"
        };

        // ----------------------------------------------------- hero models
        // FBX files built by the Blender pipeline in Art/pipeline (P2). The
        // scripts export real-world metres with Forward -Z / Up Y / Apply
        // Transform, so everything imports at scale 1. UV1 (lightmap UVs)
        // is authored in Blender because Unity 6 removed the importer's
        // generateLightmapUVs flag - the HeroModelImport entry keeps the
        // flag to document the intent and to assert the FBX carries UV1.
        public const string HeroModelsFolder = "Assets/_Project/Models/Hero";

        // Basenames of the two hero meshes embedded by the special prefabs
        // (Shadow_Art's silhouette and the Player's hand torch).
        const string WraithModelName = "SM_Wraith_HD";
        const string PlayerFlashlightModelName = "SM_Player_Flashlight";

        sealed class HeroModelImport
        {
            public readonly string Name;        // FBX basename without .fbx
            public readonly float FbxScale;     // ModelImporter.globalScale
            public readonly bool GenerateLightmapUVs; // true: UV1 ships in the FBX

            public HeroModelImport(string name, float fbxScale = 1f, bool generateLightmapUVs = true)
            {
                Name = name;
                FbxScale = fbxScale;
                GenerateLightmapUVs = generateLightmapUVs;
            }
        }

        static readonly HeroModelImport[] HeroModels =
        {
            new HeroModelImport("SM_Story_MemoryBox"),
            new HeroModelImport("SM_Story_Card"),
            new HeroModelImport("SM_Story_Bookmark"),
            new HeroModelImport("SM_Story_ExamPaper"),
            new HeroModelImport("SM_Story_Prism"),
            new HeroModelImport("SM_Story_Photograph"),
            new HeroModelImport("SM_Story_Programme"),
            new HeroModelImport("SM_Story_Mug"),
            new HeroModelImport("SM_Player_Flashlight"),
            new HeroModelImport("SM_Wraith_HD"),
            new HeroModelImport("SM_Arch_DoorGrille"),
            new HeroModelImport("SM_Arch_Chalkboard"),
            new HeroModelImport("SM_Arch_GateCrank"),
            new HeroModelImport("SM_Arch_IronGate"),
        };

        /// <summary>Basenames (without .fbx) of every hero FBX the pipeline
        /// exports; the content validator asserts all of them are imported.</summary>
        public static string[] HeroModelNames()
        {
            var names = new string[HeroModels.Length];
            for (int i = 0; i < HeroModels.Length; i++) names[i] = HeroModels[i].Name;
            return names;
        }

        // Surface folders under Textures/Art and the material each becomes.
        // Every set ships albedo + OpenGL normal + roughness 2k JPGs.
        static readonly string[] SurfaceNames =
        {
            "PlasterWall", "WoodFloor", "CorridorTile", "Concrete",
            "MetalLocker", "DoorWood", "Grass", "Fabric"
        };

        // ------------------------------------------------------- look tuning
        // Hand-derived surface tints (kept as consts: GameConfig is not ours).
        static readonly Color CeilingTint = new Color(0.42f, 0.44f, 0.50f);
        static readonly Color GateMetalTint = new Color(0.42f, 0.45f, 0.52f);
        static readonly Color StoneTint = new Color(0.55f, 0.54f, 0.50f);
        static readonly Color ChalkboardColor = new Color(0.045f, 0.085f, 0.065f);
        static readonly Color HandSkinColor = new Color(0.80f, 0.64f, 0.52f);
        static readonly Color TorchBodyColor = new Color(0.10f, 0.11f, 0.13f);
        static readonly Color SmokeBodyColor = new Color(0.012f, 0.010f, 0.022f, 0.88f);
        static readonly Color SmokeEdgeColor = new Color(0.16f, 0.18f, 0.24f, 0.95f);
        static readonly Color PuffColor = new Color(0.09f, 0.10f, 0.14f, 0.08f);
        static readonly Color DustColor = new Color(0.85f, 0.82f, 0.75f);

        // P2 story materials (named 1:1 after the FBX-embedded slots, see the
        // export manifest Art/pipeline/manifest.json): red marking pen on the
        // exam paper, the flashlight lens glass, and its amber LED strip.
        static readonly Color StoryRedInkColor = new Color(0.55f, 0.06f, 0.05f);
        static readonly Color StoryGlassColor = new Color(0.78f, 0.88f, 0.95f, 0.32f);
        static readonly Color StoryLedColor = new Color(0.90f, 0.45f, 0.10f);
        static readonly Color StoryLedEmissionColor = new Color(1f, 0.55f, 0.16f);

        // Shadow silhouette target height in metres (matches the enemy's 1.8 m
        // capsule collider built by SceneBuilderPrefabs).
        const float ShadowTargetHeight = 1.8f;
        const float ShadowCharacterHeight = 1.85f;

        // Fog / bloom / grain tuning for the night profile.
        static readonly Color NightFogColor = new Color(0.024f, 0.031f, 0.052f);
        const float NightFogDensity = 0.035f;
        const float NightBloomThreshold = 1.15f;
        const float NightBloomIntensity = 3.5f;
        const float NightBloomScatter = 0.55f;
        const float NightFilmGrainIntensity = 0.18f;

        // Dawn profile keeps the same look but a warmer, thinner haze.
        static readonly Color DawnFogColor = new Color(0.10f, 0.075f, 0.06f);
        const float DawnFogDensity = 0.022f;
        const float DawnBloomThreshold = 1.05f;
        const float DawnBloomIntensity = 4.5f;

        // URP shader names resolved once.
        const string UrpLitShaderName = "Universal Render Pipeline/Lit";
        const string UrpParticlesUnlitShaderName = "Universal Render Pipeline/Particles/Unlit";
        const string ShadowSmokeShaderName = "LightShadow/ShadowSmoke";

        // ------------------------------------------------------------ entry
        // Called by Unity in batch mode:
        // -executeMethod LightShadow.Tools.ArtImporter.ImportAllBatch
        public static void ImportAllBatch()
        {
            try
            {
                string summary = ImportAll();
                Debug.Log("[LightShadow] ART IMPORT\n" + summary);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError("[LightShadow] ART IMPORT FAILED: " + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("LightShadow/Art/1. Import Art Assets", priority = 41)]
        public static void ImportAllMenu()
        {
            string summary = ImportAll();
            EditorUtility.DisplayDialog("LightShadow - Art Import", summary, "OK");
        }

        static string ImportAll()
        {
            AssetDatabase.Refresh();

            EnsureFolder(MaterialsFolder);
            EnsureFolder(ArtPrefabsFolder);

            int textures = ImportTextures();
            int models = ImportModels();
            int heroModels = ImportHeroModels();
            int converted = ConvertImportedMaterialsToUrp();
            int surfaces = BuildSurfaceMaterials();
            int specials = BuildSpecialMaterials();
            int story = BuildStoryMaterials();
            string prefabs = BuildSpecialPrefabs();
            string dedupe = DedupeThirdPartyMaterials();
            string post = UpdatePostVolumes();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return "Textures configured: " + textures
                + "\nModels configured: " + models
                + "\nHero models configured: " + heroModels
                + "\nPack materials converted to URP: " + converted
                + "\nSurface materials ensured: " + surfaces
                + "\nSpecial materials ensured: " + specials
                + "\nStory materials ensured: " + story
                + "\n" + prefabs
                + "\n" + dedupe
                + "\n" + post;
        }

        // -------------------------------------------------- material dedupe --
        // Kenney packs carry one flat material per model file, so a furnished
        // school ended up with several hundred near-identical materials — each
        // one a separate SRP-Batcher SetPass and a real frame-time cost (the
        // ART pass flagged this as a follow-up). This builds a small set of
        // shared materials keyed by (shader, colour, metallic, smoothness,
        // texture); the scene-side sweep in SceneBuilderArt then swaps every
        // renderer still using a ThirdParty material onto its shared twin, so
        // the shipped scenes render with a handful of materials instead of
        // hundreds. Idempotent: shared materials never live under ThirdParty.
        static string DedupeThirdPartyMaterials()
        {
            var packMaterials = new List<Material>();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets/ThirdParty" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null) continue;
                packMaterials.Add(material);
            }

            if (packMaterials.Count == 0) return "Material dedupe: no pack materials found";

            string sharedFolder = MaterialsFolder + "/Shared";
            EnsureFolder(sharedFolder);

            var sharedByKey = new Dictionary<string, Material>();
            int duplicates = 0;

            foreach (Material material in packMaterials)
            {
                string key = MaterialKey(material);
                Material shared;
                if (!sharedByKey.TryGetValue(key, out shared))
                {
                    string hash = Mathf.Abs(StableHash(key)).ToString("X8");
                    string sharedPath = sharedFolder + "/Mat_Shared_" + hash + ".mat";
                    shared = AssetDatabase.LoadAssetAtPath<Material>(sharedPath);
                    if (shared == null)
                    {
                        shared = new Material(material.shader);
                        shared.color = material.color;
                        shared.SetFloat("_Metallic", material.GetFloat("_Metallic"));
                        shared.SetFloat("_Smoothness", material.GetFloat("_Smoothness"));
                        if (material.mainTexture != null) shared.mainTexture = material.mainTexture;
                        AssetDatabase.CreateAsset(shared, sharedPath);
                    }
                    sharedByKey[key] = shared;
                }

                if (shared != material) duplicates++;
            }

            sharedLookup = sharedByKey;
            return "Material dedupe: " + packMaterials.Count + " pack materials → "
                + sharedByKey.Count + " shared (" + duplicates + " duplicates)";
        }

        // Key that decides "these two materials render identically". Rounded to
        // 1/255 per channel — flat Kenney colours land on the same key.
        static string MaterialKey(Material material)
        {
            Color color = material.color;
            string textureId = material.mainTexture != null
                ? AssetDatabase.GetAssetPath(material.mainTexture)
                : "none";
            return material.shader.name
                + "|" + Mathf.RoundToInt(Mathf.Clamp01(color.r) * 255f)
                + "," + Mathf.RoundToInt(Mathf.Clamp01(color.g) * 255f)
                + "," + Mathf.RoundToInt(Mathf.Clamp01(color.b) * 255f)
                + "," + Mathf.RoundToInt(color.a * 255f)
                + "|" + material.GetFloat("_Metallic").ToString("0.00")
                + "|" + material.GetFloat("_Smoothness").ToString("0.00")
                + "|" + textureId;
        }

        // Deterministic hash (string.GetHashCode differs across .NET targets).
        static int StableHash(string text)
        {
            unchecked
            {
                int hash = 17;
                foreach (char character in text) hash = hash * 31 + character;
                return hash;
            }
        }

        /// <summary>Scene-side dedupe: returns the shared twin for a pack
        /// material, or null when the material is not from ThirdParty (or the
        /// dedupe pass has not run). Callers must not mutate the result.</summary>
        public static Material SharedReplacementFor(Material material)
        {
            if (material == null || sharedLookup == null) return null;
            Material shared;
            if (!sharedLookup.TryGetValue(MaterialKey(material), out shared)) return null;
            return shared != material ? shared : null;
        }

        static Dictionary<string, Material> sharedLookup;

        // ---------------------------------------------------------- textures
        static int ImportTextures()
        {
            int count = 0;
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { TexturesFolder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                bool isNormal = path.EndsWith("_Normal.jpg", StringComparison.OrdinalIgnoreCase);
                bool isRoughness = path.EndsWith("_Roughness.jpg", StringComparison.OrdinalIgnoreCase);

                // Normals must be flagged as normal maps; roughness maps are
                // data, not colour, so they import linear (sRGB off).
                importer.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.sRGBTexture = !isNormal && !isRoughness;
                importer.alphaIsTransparency = false;
                importer.mipmapEnabled = true;
                importer.streamingMipmaps = true;
                importer.anisoLevel = 2;
                importer.maxTextureSize = 2048;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Repeat;

                // Android override: half resolution, automatic compression
                // (Unity 6 picks ASTC on mobile by default).
                TextureImporterPlatformSettings android = new TextureImporterPlatformSettings();
                android.name = "Android";
                android.overridden = true;
                android.maxTextureSize = 1024;
                android.format = TextureImporterFormat.Automatic;
                android.textureCompression = TextureImporterCompression.Compressed;
                importer.SetPlatformTextureSettings(android);

                // Reimport unconditionally: the settings below are the source
                // of truth and the pass must converge on them every run.
                importer.SaveAndReimport();
                count++;
            }

            BuildSmoothnessMaps();
            return count;
        }

        // URP/Lit cannot invert a roughness map at runtime, so each roughness
        // JPG gets an inverted-alpha PNG twin that lands in the metallic/
        // smoothness slot (smoothness = 1 - roughness, exactly what the artist
        // intent is). One PNG per surface, generated once.
        static void BuildSmoothnessMaps()
        {
            EnsureFolder(ProcessedTexturesFolder);
            foreach (string surface in SurfaceNames)
            {
                string roughPath = TexturesFolder + "/" + surface + "/T_" + surface + "_Roughness.jpg";
                string smoothPath = ProcessedTexturesFolder + "/T_" + surface + "_Smoothness.png";
                if (!File.Exists(roughPath)) continue;
                if (File.Exists(smoothPath)) continue;

                TextureImporter roughImporter = AssetImporter.GetAtPath(roughPath) as TextureImporter;
                if (roughImporter == null) continue;

                // Make the source readable only for this pass, then restore it.
                bool wasReadable = roughImporter.isReadable;
                roughImporter.isReadable = true;
                roughImporter.SaveAndReimport();

                Texture2D rough = AssetDatabase.LoadAssetAtPath<Texture2D>(roughPath);
                if (rough == null)
                {
                    Debug.LogWarning("[ArtImporter] Could not load roughness map: " + roughPath);
                    continue;
                }

                Color32[] pixels = rough.GetPixels32();
                Color32[] inverted = new Color32[pixels.Length];
                for (int i = 0; i < pixels.Length; i++)
                {
                    byte roughByte = pixels[i].r;
                    inverted[i] = new Color32(roughByte, roughByte, roughByte, (byte)(255 - roughByte));
                }

                Texture2D output = new Texture2D(rough.width, rough.height, TextureFormat.RGBA32, false, true);
                output.SetPixels32(inverted);
                output.Apply();
                File.WriteAllBytes(smoothPath, output.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(output);

                roughImporter.isReadable = wasReadable;
                roughImporter.SaveAndReimport();

                // Import the new PNG as linear data with the same mip policy.
                AssetDatabase.ImportAsset(smoothPath);
                TextureImporter smoothImporter = AssetImporter.GetAtPath(smoothPath) as TextureImporter;
                if (smoothImporter != null)
                {
                    smoothImporter.sRGBTexture = false;
                    smoothImporter.mipmapEnabled = true;
                    smoothImporter.streamingMipmaps = true;
                    smoothImporter.maxTextureSize = 2048;
                    smoothImporter.SaveAndReimport();
                }
            }
        }

        // ------------------------------------------------------------ models
        static int ImportModels()
        {
            int count = 0;
            foreach (string pack in PackFolders)
            {
                string packPath = ThirdPartyFolder + "/" + pack;
                if (!AssetDatabase.IsValidFolder(packPath)) continue;

                string[] guids = AssetDatabase.FindAssets("t:Model", new[] { packPath });
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;

                    ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                    if (importer == null) continue;

                    // Kenney FBX carry no units: treat them as metres at scale 1
                    // (the art pass normalizes per-instance scale anyway).
                    importer.useFileUnits = true;
                    importer.useFileScale = true;
                    importer.globalScale = 1f;

                    // Static props only: no blend shapes, no animation, and
                    // never importer-generated colliders (physics is owned by
                    // the SceneBuilder boxes and must stay untouched). Unity 6
                    // removed generateColliders/generateLightmapUVs from the
                    // importer, so neither is set here.
                    importer.importBlendShapes = false;
                    importer.importAnimation = false;
                    importer.meshCompression = ModelImporterMeshCompression.Medium;

                    // Per-colour materials keep the Kenney palette.
                    importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                    importer.materialLocation = ModelImporterMaterialLocation.External;

                    importer.SaveAndReimport();
                    count++;
                }
            }
            return count;
        }

        // Hero FBX models from Art/pipeline (see HeroModels above). Same
        // unit-safe static-prop policy as the Kenney packs: metres at scale
        // 1, no blend shapes, no animations, no importer colliders, medium
        // mesh compression, external materials so the generated .mat files
        // (named exactly Mat_Art_* / M_ShadowSmoke / Mat_Story_*) can be
        // remapped onto the existing art materials by name. UV1 ships in
        // the FBX; Unity 6 has no generateLightmapUVs importer flag anymore.
        static int ImportHeroModels()
        {
            if (!AssetDatabase.IsValidFolder(HeroModelsFolder)) return 0;

            var known = new Dictionary<string, HeroModelImport>();
            foreach (HeroModelImport entry in HeroModels) known[entry.Name] = entry;

            int count = 0;
            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { HeroModelsFolder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;

                string fileName = Path.GetFileNameWithoutExtension(path);
                if (!known.TryGetValue(fileName, out HeroModelImport entry))
                {
                    Debug.LogWarning("[ArtImporter] Hero FBX without a HeroModels entry: " + path);
                    continue;
                }

                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) continue;

                importer.useFileUnits = true;
                importer.useFileScale = true;
                importer.globalScale = entry.FbxScale;

                importer.importBlendShapes = false;
                importer.importAnimation = false;
                importer.meshCompression = ModelImporterMeshCompression.Medium;

                importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                importer.materialLocation = ModelImporterMaterialLocation.External;

                importer.SaveAndReimport();

                // The pipeline authors non-overlapping lightmap UVs as the
                // second UV channel. Unity 6 cannot generate them anymore,
                // so a missing UV1 must be loud now, not at lightmap bake.
                if (entry.GenerateLightmapUVs && !HeroMeshHasUv1(path))
                {
                    Debug.LogWarning("[ArtImporter] Hero FBX is missing UV1 lightmap UVs: " + path);
                }
                count++;
            }
            return count;
        }

        // True when at least one mesh in the FBX carries a second UV set.
        static bool HeroMeshHasUv1(string path)
        {
            var uvBuffer = new List<Vector2>();
            foreach (UnityEngine.Object candidate in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (candidate is Mesh mesh && mesh.GetUVs(1, uvBuffer) && uvBuffer.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        // Unity's importer writes one .mat per FBX material into
        // <pack>/Materials/ (and Hero/Materials/ for the P2 hero FBX). They
        // come in on the built-in Standard shader, which is pink under URP -
        // convert them in place, copying the diffuse colour across. These
        // files are Unity-generated import output (not pack content); see the
        // report for the reasoning.
        static int ConvertImportedMaterialsToUrp()
        {
            int count = 0;
            Shader lit = Shader.Find(UrpLitShaderName);
            if (lit == null)
            {
                Debug.LogError("[ArtImporter] URP/Lit shader not found - cannot convert pack materials.");
                return 0;
            }

            foreach (string pack in PackFolders)
            {
                count += ConvertMaterialsInFolderToUrp(ThirdPartyFolder + "/" + pack + "/Materials", lit);
            }

            // Hero FBX models generate their external material twins next to
            // the FBX (Hero/Materials). Convert them too so a renderer that
            // keeps an unregistered material still renders instead of pink.
            count += ConvertMaterialsInFolderToUrp(HeroModelsFolder + "/Materials", lit);

            return count;
        }

        // Converts every non-URP material in a folder to URP/Lit in place,
        // carrying the diffuse colour across. Returns the converted count.
        static int ConvertMaterialsInFolderToUrp(string folderPath, Shader lit)
        {
            if (!AssetDatabase.IsValidFolder(folderPath)) return 0;
            int count = 0;
            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { folderPath });
            foreach (string guid in guids)
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (material == null || material.shader == lit) continue;

                Color color = material.HasProperty("_Color")
                    ? material.GetColor("_Color")
                    : Color.white;

                material.shader = lit;
                material.color = color;
                material.SetFloat("_Metallic", 0f);
                material.SetFloat("_Smoothness", 0.4f);
                EditorUtility.SetDirty(material);
                count++;
            }
            return count;
        }

        // -------------------------------------------------- surface materials
        static int BuildSurfaceMaterials()
        {
            Shader lit = Shader.Find(UrpLitShaderName);
            if (lit == null) return 0;
            int count = 0;

            foreach (string surface in SurfaceNames)
            {
                string albedo = TexturesFolder + "/" + surface + "/T_" + surface + "_Albedo.jpg";
                string normal = TexturesFolder + "/" + surface + "/T_" + surface + "_Normal.jpg";
                string smoothness = ProcessedTexturesFolder + "/T_" + surface + "_Smoothness.png";

                Material material = GetOrCreateLitMaterial("Mat_Art_" + surface, lit);
                if (File.Exists(albedo)) material.SetTexture("_BaseMap", LoadAssetGuaranteed<Texture2D>(albedo));
                if (File.Exists(normal))
                {
                    material.SetTexture("_BumpMap", LoadAssetGuaranteed<Texture2D>(normal));
                    material.EnableKeyword("_NORMALMAP");
                    material.SetFloat("_NormalScale", 1f);
                }
                if (File.Exists(smoothness))
                {
                    // Roughness flows through the metallic/smoothness slot with
                    // smoothness read from that map's alpha (already inverted).
                    material.SetTexture("_MetallicGlossMap", LoadAssetGuaranteed<Texture2D>(smoothness));
                    material.EnableKeyword("_METALLICSPECGLOSSMAP");
                    material.SetFloat("_SmoothnessTextureChannel", 1f); // 1 = MetallicAlpha
                    material.SetFloat("_Metallic", 0f);
                }
                else
                {
                    material.SetFloat("_Smoothness", 0.35f);
                }
                EditorUtility.SetDirty(material);
                count++;
            }

            // Tinted/constant variants used by the art pass.
            Material ceiling = GetOrCreateLitMaterial("Mat_Art_Ceiling", lit);
            CopyTextureSetup("Mat_Art_PlasterWall", ceiling);
            ceiling.SetColor("_BaseColor", CeilingTint);
            EditorUtility.SetDirty(ceiling);

            Material gate = GetOrCreateLitMaterial("Mat_Art_GateMetal", lit);
            CopyTextureSetup("Mat_Art_MetalLocker", gate);
            gate.SetColor("_BaseColor", GateMetalTint);
            EditorUtility.SetDirty(gate);

            Material stone = GetOrCreateLitMaterial("Mat_Art_Stone", lit);
            CopyTextureSetup("Mat_Art_Concrete", stone);
            stone.SetColor("_BaseColor", StoneTint);
            EditorUtility.SetDirty(stone);

            Material chalkboard = GetOrCreateLitMaterial("Mat_Art_Chalkboard", lit);
            chalkboard.SetColor("_BaseColor", ChalkboardColor);
            chalkboard.SetFloat("_Smoothness", 0.3f);
            EditorUtility.SetDirty(chalkboard);

            Material skin = GetOrCreateLitMaterial("Mat_Art_HandSkin", lit);
            skin.SetColor("_BaseColor", HandSkinColor);
            skin.SetFloat("_Smoothness", 0.12f);
            EditorUtility.SetDirty(skin);

            Material torch = GetOrCreateLitMaterial("Mat_Art_TorchMetal", lit);
            torch.SetColor("_BaseColor", TorchBodyColor);
            torch.SetFloat("_Smoothness", 0.55f);
            torch.SetFloat("_Metallic", 0.6f);
            EditorUtility.SetDirty(torch);

            return count;
        }

        // -------------------------------------------------- hero material map
        // The Blender exporter names the FBX-embedded materials exactly after
        // the factory assets in Materials/Art (verified 1:1 against the export
        // manifest Art/pipeline/manifest.json: DoorWood, MetalLocker,
        // GateMetal, TorchMetal, PlasterWall, Fabric, Chalkboard, Stone and
        // M_ShadowSmoke) plus the three P2 story materials below. The swap
        // layer rebinds every renderer of an instantiated hero FBX through
        // this map, so the importer's generated twins (written next to the
        // FBX) never ship inside a scene or prefab. Register any material the
        // exporter adds later here 1:1, or it will keep its generated twin.
        static readonly Dictionary<string, string> HeroMaterialMap = new Dictionary<string, string>
        {
            { "Mat_Art_DoorWood", "Mat_Art_DoorWood" },
            { "Mat_Art_MetalLocker", "Mat_Art_MetalLocker" },
            { "Mat_Art_GateMetal", "Mat_Art_GateMetal" },
            { "Mat_Art_TorchMetal", "Mat_Art_TorchMetal" },
            { "Mat_Art_PlasterWall", "Mat_Art_PlasterWall" },
            { "Mat_Art_Fabric", "Mat_Art_Fabric" },
            { "Mat_Art_Chalkboard", "Mat_Art_Chalkboard" },
            { "Mat_Art_Stone", "Mat_Art_Stone" },
            { "M_ShadowSmoke", "M_ShadowSmoke" },
            { "Mat_Story_RedInk", "Mat_Story_RedInk" },
            { "Mat_Story_Glass", "Mat_Story_Glass" },
            { "Mat_Story_LED", "Mat_Story_LED" },
        };

        /// <summary>Canonical art material for an FBX-embedded material name,
        /// or null when the name is not registered (the generated twin stays).
        /// Shared by the SceneBuilder hero swaps and the content validator.</summary>
        public static Material HeroRemapMaterial(string fbxMaterialName)
        {
            if (string.IsNullOrEmpty(fbxMaterialName)) return null;
            if (!HeroMaterialMap.TryGetValue(fbxMaterialName, out string canonicalName)) return null;
            return AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/" + canonicalName + ".mat");
        }

        /// <summary>Every material name the exporter embeds in the hero FBX
        /// files; the validator checks each resolves to a canonical asset.</summary>
        public static string[] HeroMaterialNames()
        {
            var names = new string[HeroMaterialMap.Count];
            int index = 0;
            foreach (string name in HeroMaterialMap.Keys) names[index++] = name;
            return names;
        }

        // -------------------------------------------------- story materials
        // The three FBX-embedded slots the existing factory does not cover
        // yet (see HeroMaterialMap): red marking pen, lens glass and the LED
        // strip of the crank flashlight.
        static int BuildStoryMaterials()
        {
            Shader lit = Shader.Find(UrpLitShaderName);
            if (lit == null) return 0;
            int count = 0;

            Material redInk = GetOrCreateLitMaterial("Mat_Story_RedInk", lit);
            redInk.SetColor("_BaseColor", StoryRedInkColor);
            redInk.SetFloat("_Smoothness", 0.5f);
            EditorUtility.SetDirty(redInk);
            count++;

            Material glass = GetOrCreateLitMaterial("Mat_Story_Glass", lit);
            glass.SetColor("_BaseColor", StoryGlassColor);
            glass.SetFloat("_Smoothness", 0.92f);
            glass.SetFloat("_Metallic", 0f);
            ConfigureTransparentLitMaterial(glass);
            EditorUtility.SetDirty(glass);
            count++;

            Material led = GetOrCreateLitMaterial("Mat_Story_LED", lit);
            led.SetColor("_BaseColor", StoryLedColor);
            led.SetFloat("_Smoothness", 0.6f);
            led.EnableKeyword("_EMISSION");
            led.SetColor("_EmissionColor", StoryLedEmissionColor * 2.2f);
            led.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(led);
            count++;

            return count;
        }

        // URP/Lit transparent surface setup (alpha blend, no z-write) for the
        // lens glass; the same property set the particle materials use,
        // without the particle-specific bits.
        static void ConfigureTransparentLitMaterial(Material material)
        {
            material.SetFloat("_Surface", 1f); // transparent
            material.SetFloat("_Blend", 0f);   // alpha
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        // -------------------------------------------------- special materials
        static int BuildSpecialMaterials()
        {
            int count = 0;

            // The Shadow's smoke silhouette uses the hand-written shader.
            Shader smokeShader = Shader.Find(ShadowSmokeShaderName);
            if (smokeShader == null)
            {
                // Fallback keeps the pass running (plain dark transparent Lit)
                // but logs loudly so the broken shader gets fixed first.
                Debug.LogError("[ArtImporter] Shader '" + ShadowSmokeShaderName
                    + "' not found; falling back to URP/Lit transparent. Fix the shader asset.");
                smokeShader = Shader.Find(UrpLitShaderName);
            }
            Material smoke = GetOrCreateMaterial("M_ShadowSmoke", smokeShader);
            smoke.SetColor("_BaseColor", SmokeBodyColor);
            if (smoke.HasProperty("_EdgeColor")) smoke.SetColor("_EdgeColor", SmokeEdgeColor);
            if (smoke.HasProperty("_RimPower")) smoke.SetFloat("_RimPower", 3.2f);
            if (smoke.HasProperty("_RimStrength")) smoke.SetFloat("_RimStrength", 0.9f);
            if (smoke.HasProperty("_WobbleAmplitude")) smoke.SetFloat("_WobbleAmplitude", 0.05f);
            smoke.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(smoke);
            count++;

            // One soft radial sprite shared by dust and smoke puffs.
            Texture2D dustSprite = BuildRadialSprite();

            Material dust = GetOrCreateMaterial("M_Art_DustMote", Shader.Find(UrpParticlesUnlitShaderName));
            dust.SetTexture("_BaseMap", dustSprite);
            dust.SetColor("_BaseColor", new Color(DustColor.r, DustColor.g, DustColor.b, 1f));
            ConfigureAdditiveParticleMaterial(dust);
            EditorUtility.SetDirty(dust);
            count++;

            Material puff = GetOrCreateMaterial("M_Art_SmokePuff", Shader.Find(UrpParticlesUnlitShaderName));
            puff.SetTexture("_BaseMap", dustSprite);
            puff.SetColor("_BaseColor", new Color(PuffColor.r, PuffColor.g, PuffColor.b, 1f));
            ConfigureAlphaParticleMaterial(puff);
            EditorUtility.SetDirty(puff);
            count++;

            // Darkness pass: a real flashlight cookie (hot centre, irregular
            // streaks) so the beam throws believable structure, and an additive
            // material for the volumetric beam cone.
            Texture2D cookie = BuildFlashlightCookie();
            Material beam = GetOrCreateMaterial("M_Art_Beam", Shader.Find(UrpParticlesUnlitShaderName));
            beam.SetTexture("_BaseMap", dustSprite);
            beam.SetColor("_BaseColor", new Color(1f, 0.93f, 0.78f, 1f));
            ConfigureAdditiveParticleMaterial(beam);
            beam.EnableKeyword("_VERTEX_COLOR"); // alpha comes from the cone mesh
            beam.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(beam);
            count++;

            return count;
        }

        // 256 px spotlight cookie: warm hotspot, soft round falloff, faint
        // radial streaks and per-pixel dust so walls show beam structure.
        // Imported as a Cookie (spot) asset the prefab builder assigns.
        static Texture2D BuildFlashlightCookie()
        {
            string path = ProcessedTexturesFolder + "/T_FlashlightCookie.png";
            if (File.Exists(path)) return LoadAssetGuaranteed<Texture2D>(path);

            const int size = 256;
            Texture2D cookie = new Texture2D(size, size, TextureFormat.RGBA32, false, true); // linear
            var random = new System.Random(8080);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f;
                    float dy = (y + 0.5f) / size * 2f - 1f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx);

                    float falloff = Mathf.Clamp01(1f - Mathf.Pow(distance, 1.6f));
                    float hotspot = Mathf.Clamp01(1f - distance * 2.2f) * 0.8f;
                    // A few fixed-frequency streaks + fine dust grain.
                    float streaks = 0.86f + 0.10f * Mathf.Sin(angle * 7f + 1.7f)
                                        + 0.06f * Mathf.Sin(angle * 13f);
                    float grain = 1f + ((float)random.NextDouble() * 2f - 1f) * 0.05f;

                    float value = Mathf.Clamp01((falloff * streaks + hotspot) * grain);
                    byte channel = (byte)(value * 255f);
                    cookie.SetPixel(x, y, new Color32(channel, channel, channel, 255));
                }
            }
            cookie.Apply();

            EnsureFolder(ProcessedTexturesFolder);
            File.WriteAllBytes(path, cookie.EncodeToPNG());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Cookie;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.mipmapEnabled = true;
                importer.sRGBTexture = false;
                importer.maxTextureSize = 256;
                importer.SaveAndReimport();
            }
            return LoadAssetGuaranteed<Texture2D>(path);
        }

        // Procedural 64x64 radial falloff sprite for particles (soft edges
        // without shipping a texture file).
        static Texture2D BuildRadialSprite()
        {
            string path = ProcessedTexturesFolder + "/T_ParticleSoft.png";
            if (File.Exists(path)) return LoadAssetGuaranteed<Texture2D>(path);

            const int size = 64;
            Texture2D sprite = new Texture2D(size, size, TextureFormat.RGBA32, false, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f;
                    float dy = (y + 0.5f) / size * 2f - 1f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float falloff = Mathf.Clamp01(1f - distance);
                    byte alpha = (byte)(falloff * falloff * 255f);
                    sprite.SetPixel(x, y, new Color32(255, 255, 255, alpha));
                }
            }
            sprite.Apply();

            EnsureFolder(ProcessedTexturesFolder);
            File.WriteAllBytes(path, sprite.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(sprite);

            AssetDatabase.ImportAsset(path);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.sRGBTexture = true;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = true;
                importer.maxTextureSize = 64;
                importer.SaveAndReimport();
            }
            return LoadAssetGuaranteed<Texture2D>(path);
        }

        // ----------------------------------------------------- special prefabs
        static string BuildSpecialPrefabs()
        {
            string shadow = BuildShadowPrefab();
            string hands = BuildHandsPrefab();
            string dust = BuildDustMotesPrefab();
            string restyle = RestyleExistingPrefabs();
            return shadow + "\n" + hands + "\n" + dust + "\n" + restyle;
        }

        // Shadow_Art.prefab: the Kenney blocky humanoid wrapped in the smoke
        // shader plus a slow dark particle halo. ENG can drop this under any
        // enemy visual root; TheShadow.prefab is rewired automatically below.
        static string BuildShadowPrefab()
        {
            // Rebuild (not skip) while the current prefab predates the wraith
            // mesh: the marker child names the silhouette source.
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(ShadowArtPrefabPath);
            if (existing != null && existing.transform.Find("Wraith") != null)
            {
                return "Shadow_Art.prefab: already present (wraith, idempotent skip)";
            }
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(ShadowArtPrefabPath);
            }

            Material smoke = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/M_ShadowSmoke.mat");
            Material puff = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/M_Art_SmokePuff.mat");
            if (smoke == null) return "Shadow_Art.prefab: SKIPPED - M_ShadowSmoke missing";
            if (puff == null) puff = smoke; // halo degrades to the smoke material

            Mesh wraith = BuildWraithMesh();

            var root = new GameObject("Shadow_Art");

            GameObject silhouette = new GameObject("Wraith");
            silhouette.transform.SetParent(root.transform, false);
            MeshFilter filter = silhouette.AddComponent<MeshFilter>();
            filter.sharedMesh = wraith;
            MeshRenderer renderer = silhouette.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = smoke;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            // Normalize to the enemy's collider height.
            float scale = ShadowTargetHeight / WraithHeight;
            silhouette.transform.localScale = new Vector3(scale, scale, scale);

            // Slow smoke halo: dark alpha-blended puffs in local space so they
            // drift with the enemy (no additive glow - it must stay a shadow).
            GameObject halo = new GameObject("SmokeHalo");
            halo.transform.SetParent(root.transform, false);
            ParticleSystem particles = halo.AddComponent<ParticleSystem>();
            ConfigureSmokeHalo(particles, puff);
            foreach (Renderer haloRenderer in halo.GetComponentsInChildren<Renderer>()) haloRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            PrefabUtility.SaveAsPrefabAsset(root, ShadowArtPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return "Shadow_Art.prefab: rebuilt with procedural wraith silhouette";
        }

        // The wraith silhouette: a revolved profile (hooded head, shoulder
        // swell, ragged trailing skirt) with deterministic per-vertex noise so
        // the outline is organic, not a cone. The smoke shader wobbles it
        // further at runtime. Replaces the Kenney blocky humanoid (player
        // request: "not a good 3d model").
        const float WraithHeight = 2.3f;

        static Mesh BuildWraithMesh()
        {
            const int segments = 36;
            const int rings = 14;

            // Radius profile from the ragged bottom (0) to the hood tip (1).
            float[] ringHeight = new float[rings];
            float[] ringRadius = new float[rings];
            for (int i = 0; i < rings; i++)
            {
                float t = i / (float)(rings - 1);
                ringHeight[i] = t * WraithHeight;
                // Hood (0.72..1), neck pinch (0.58..0.72), shoulder swell
                // (0.40..0.58), taper (0.14..0.40), ragged skirt (0..0.14).
                float radius;
                if (t > 0.72f) radius = Mathf.Lerp(0.34f, 0.02f, (t - 0.72f) / 0.28f);
                else if (t > 0.58f) radius = Mathf.Lerp(0.46f, 0.34f, (t - 0.58f) / 0.14f);
                else if (t > 0.40f) radius = Mathf.Lerp(0.30f, 0.46f, (t - 0.40f) / 0.18f);
                else if (t > 0.14f) radius = Mathf.Lerp(0.44f, 0.30f, (t - 0.14f) / 0.26f);
                else radius = Mathf.Lerp(0.24f, 0.44f, t / 0.14f);
                ringRadius[i] = radius;
            }

            var random = new System.Random(20626);
            var vertices = new Vector3[rings * (segments + 1)];
            var uvs = new Vector2[vertices.Length];
            int vertexIndex = 0;

            for (int ring = 0; ring < rings; ring++)
            {
                float t = ring / (float)(rings - 1);
                for (int segment = 0; segment <= segments; segment++)
                {
                    float angle = segment / (float)segments * Mathf.PI * 2f;
                    float directionX = Mathf.Cos(angle);
                    float directionZ = Mathf.Sin(angle);

                    // Deterministic raggedness: strongest at the skirt, fading
                    // toward the hood so the head reads cleanly.
                    float raggedness = Mathf.Lerp(0.5f, 0.12f, t);
                    float noise = 1f + ((float)random.NextDouble() * 2f - 1f) * raggedness;
                    // Low-frequency lumps so the raggedness stays smooth.
                    noise *= 1f + 0.18f * Mathf.Sin(angle * 3f + t * 9f);

                    float radius = ringRadius[ring] * noise;
                    // A slight forward hunch: the upper body leans toward -Z.
                    float hunch = Mathf.SmoothStep(0f, 0.22f, Mathf.Clamp01((t - 0.5f) / 0.5f));

                    vertices[vertexIndex] = new Vector3(
                        directionX * radius,
                        ringHeight[ring],
                        directionZ * radius - hunch);
                    uvs[vertexIndex] = new Vector2(segment / (float)segments, t);
                    vertexIndex++;
                }
            }

            var triangles = new int[(rings - 1) * segments * 6];
            int triangleIndex = 0;
            for (int ring = 0; ring < rings - 1; ring++)
            {
                for (int segment = 0; segment < segments; segment++)
                {
                    int current = ring * (segments + 1) + segment;
                    int next = current + segments + 1;
                    triangles[triangleIndex++] = current;
                    triangles[triangleIndex++] = next;
                    triangles[triangleIndex++] = current + 1;
                    triangles[triangleIndex++] = current + 1;
                    triangles[triangleIndex++] = next;
                    triangles[triangleIndex++] = next + 1;
                }
            }

            Mesh mesh = new Mesh
            {
                name = "SM_Wraith",
                vertices = vertices,
                triangles = triangles,
                uv = uvs,
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            AssetDatabase.CreateAsset(mesh, "Assets/_Project/Models/SM_Wraith.asset");
            return mesh;
        }

        // Hands_Art.prefab: improved procedural first-person hand (no CC0 arm
        // pack exists on the allowed direct-download sources - documented in
        // the report). Hierarchy Root > Forearm > Hand for FirstPersonHands
        // wiring; every node sits on the Player layer; no colliders.
        static string BuildHandsPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(HandsArtPrefabPath) != null)
            {
                return "Hands_Art.prefab: already present (idempotent skip)";
            }

            Material skin = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/Mat_Art_HandSkin.mat");
            Material torch = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/Mat_Art_TorchMetal.mat");
            if (skin == null || torch == null) return "Hands_Art.prefab: SKIPPED - hand materials missing";

            var root = new GameObject("Hands_Art");
            root.layer = SceneBuilder.PlayerLayer;

            // Forearm: tapered capsule pointing forward (-Z is the flashlight
            // direction inside the camera rig).
            GameObject forearm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            forearm.name = "Forearm";
            forearm.layer = SceneBuilder.PlayerLayer;
            forearm.transform.SetParent(root.transform, false);
            forearm.transform.localPosition = new Vector3(0f, -0.045f, 0.12f);
            forearm.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            forearm.transform.localScale = new Vector3(0.058f, 0.10f, 0.058f);
            forearm.GetComponent<Renderer>().sharedMaterial = skin;

            GameObject hand = new GameObject("Hand");
            hand.layer = SceneBuilder.PlayerLayer;
            hand.transform.SetParent(root.transform, false);
            hand.transform.localPosition = new Vector3(0f, 0.005f, 0.02f);

            // Palm: slightly rounded box via a squashed sphere cluster is
            // overkill at this pixel size; one box + knuckle spheres reads well.
            GameObject palm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            palm.name = "Palm";
            palm.layer = SceneBuilder.PlayerLayer;
            palm.transform.SetParent(hand.transform, false);
            palm.transform.localPosition = new Vector3(0f, 0f, -0.015f);
            palm.transform.localScale = new Vector3(0.085f, 0.045f, 0.09f);
            palm.GetComponent<Renderer>().sharedMaterial = skin;

            // Four fingers curled around the torch barrel.
            float[] fingerX = { -0.032f, -0.011f, 0.011f, 0.032f };
            for (int i = 0; i < fingerX.Length; i++)
            {
                GameObject finger = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                finger.name = "Finger" + i;
                finger.layer = SceneBuilder.PlayerLayer;
                finger.transform.SetParent(hand.transform, false);
                finger.transform.localPosition = new Vector3(fingerX[i], 0.008f, -0.068f);
                finger.transform.localRotation = Quaternion.Euler(55f, 0f, 0f);
                finger.transform.localScale = new Vector3(0.014f, 0.026f, 0.014f);
                finger.GetComponent<Renderer>().sharedMaterial = skin;
            }

            // Thumb crossing the grip from the near side.
            GameObject thumb = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            thumb.name = "Thumb";
            thumb.layer = SceneBuilder.PlayerLayer;
            thumb.transform.SetParent(hand.transform, false);
            thumb.transform.localPosition = new Vector3(-0.048f, 0.012f, -0.045f);
            thumb.transform.localRotation = Quaternion.Euler(35f, 0f, 55f);
            thumb.transform.localScale = new Vector3(0.016f, 0.03f, 0.016f);
            thumb.GetComponent<Renderer>().sharedMaterial = skin;

            // Torch body + head ring: matches the old TorchBody footprint so
            // the flashlight beam still reads from the same spot.
            GameObject torchBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            torchBody.name = "TorchBody";
            torchBody.layer = SceneBuilder.PlayerLayer;
            torchBody.transform.SetParent(hand.transform, false);
            torchBody.transform.localPosition = new Vector3(0.005f, 0.03f, -0.03f);
            torchBody.transform.localRotation = Quaternion.Euler(90f, 0f, 4f);
            torchBody.transform.localScale = new Vector3(0.05f, 0.15f, 0.05f);
            torchBody.GetComponent<Renderer>().sharedMaterial = torch;

            GameObject torchHead = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            torchHead.name = "TorchHead";
            torchHead.layer = SceneBuilder.PlayerLayer;
            torchHead.transform.SetParent(hand.transform, false);
            torchHead.transform.localPosition = new Vector3(0.008f, 0.03f, -0.108f);
            torchHead.transform.localRotation = Quaternion.Euler(90f, 0f, 4f);
            torchHead.transform.localScale = new Vector3(0.062f, 0.03f, 0.062f);
            torchHead.GetComponent<Renderer>().sharedMaterial = torch;

            foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
                UnityEngine.Object.DestroyImmediate(collider);

            PrefabUtility.SaveAsPrefabAsset(root, HandsArtPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return "Hands_Art.prefab: created (procedural hand, Root/Forearm/Hand)";
        }

        // DustMotes.prefab: 60-second looping world-space dust drifting through
        // lamp light. Place 1-3 instances per big room; they cost ~200 quads.
        static string BuildDustMotesPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(DustMotesPrefabPath) != null)
            {
                return "DustMotes.prefab: already present (idempotent skip)";
            }

            Material dust = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/M_Art_DustMote.mat");
            if (dust == null) return "DustMotes.prefab: SKIPPED - M_Art_DustMote missing";

            var root = new GameObject("DustMotes");
            ParticleSystem particles = root.AddComponent<ParticleSystem>();

            var main = particles.main;
            main.duration = 60f;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(16f, 28f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.01f, 0.05f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.012f, 0.045f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(DustColor.r, DustColor.g, DustColor.b, 0.05f),
                new Color(DustColor.r, DustColor.g, DustColor.b, 0.11f));
            main.gravityModifier = 0f;
            main.maxParticles = 220;

            var emission = particles.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(8f);

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(18f, 3f, 26f);

            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.005f, 0.015f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);

            var noise = particles.noise;
            noise.enabled = true;
            noise.strength = new ParticleSystem.MinMaxCurve(0.03f);
            noise.frequency = 0.25f;

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[]
                {
                    new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f),
                    new GradientAlphaKey(1f, 0.75f), new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            var sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.8f, 1f, 1.2f));

            ParticleSystemRenderer renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.material = dust;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;

            PrefabUtility.SaveAsPrefabAsset(root, DustMotesPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return "DustMotes.prefab: created (world space, 60 s loop, ~8/s)";
        }

        // Wires the special art into the two EXISTING prefabs without touching
        // any gameplay component: TheShadow gains the smoke silhouette under
        // Visual (primitive renderers hidden, transforms kept for ShadowMotion,
        // ember eyes kept), and the Player's camera Hand swaps its primitive
        // blocks for Hands_Art. FirstPersonHands is not modified.
        static string RestyleExistingPrefabs()
        {
            string report = "";

            if (AssetDatabase.LoadAssetAtPath<GameObject>(ShadowArtPrefabPath) != null
                && AssetDatabase.LoadAssetAtPath<GameObject>(EnemyShadowPrefabPath) != null)
            {
                GameObject contents = PrefabUtility.LoadPrefabContents(EnemyShadowPrefabPath);
                try
                {
                    Transform visual = contents.transform.Find("Visual");
                    if (visual == null)
                    {
                        report += "TheShadow.prefab: no Visual node found - skipped";
                    }
                    else
                    {
                        if (visual.Find("Shadow_Art") == null)
                        {
                            GameObject art = (GameObject)PrefabUtility.InstantiatePrefab(
                                AssetDatabase.LoadAssetAtPath<GameObject>(ShadowArtPrefabPath), visual);
                            art.name = "Shadow_Art";
                        }

                        // Hide the greybox shapes but keep every transform:
                        // ShadowMotion animates ArmL/ArmR by name.
                        foreach (string shapeName in new[] { "Body", "Head", "ArmL", "ArmR" })
                        {
                            Transform shape = visual.Find(shapeName);
                            if (shape == null) continue;
                            Renderer renderer = shape.GetComponent<Renderer>();
                            if (renderer != null) renderer.enabled = false;
                        }

                        PrefabUtility.SaveAsPrefabAsset(contents, EnemyShadowPrefabPath);
                        report += "TheShadow.prefab: smoke silhouette added, primitives hidden";
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
                report += "\n";
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(HandsArtPrefabPath) != null
                && AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath) != null)
            {
                GameObject contents = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
                try
                {
                    Transform hand = FindChildByName(contents.transform, "Hand");
                    if (hand == null)
                    {
                        report += "Player.prefab: no Hand node found - skipped";
                    }
                    else
                    {
                        if (hand.Find("Hands_Art") == null)
                        {
                            GameObject hands = (GameObject)PrefabUtility.InstantiatePrefab(
                                AssetDatabase.LoadAssetAtPath<GameObject>(HandsArtPrefabPath), hand);
                            hands.name = "Hands_Art";
                            hands.transform.localPosition = Vector3.zero;
                            hands.transform.localRotation = Quaternion.identity;
                        }

                        foreach (string blockName in new[] { "Palm", "Thumb", "Forearm", "TorchBody" })
                        {
                            Transform block = hand.Find(blockName);
                            if (block == null) continue;
                            Renderer renderer = block.GetComponent<Renderer>();
                            if (renderer != null) renderer.enabled = false;
                        }

                        PrefabUtility.SaveAsPrefabAsset(contents, PlayerPrefabPath);
                        report += "Player.prefab: procedural hand installed under camera Hand";
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }

            return report;
        }

        // ------------------------------------------------------------- post
        // Extends the existing volume profiles through the VolumeProfile API
        // (AddOverride-equivalent via Add<T>): exponential fog, tuned bloom and
        // a subtle film grain. Depth of field is ensured ABSENT in gameplay.
        static string UpdatePostVolumes()
        {
            string night = TunePostProfile(PostNightPath, NightFogColor, NightFogDensity,
                NightBloomThreshold, NightBloomIntensity, NightBloomScatter, NightFilmGrainIntensity);
            string dawn = TunePostProfile(PostDawnPath, DawnFogColor, DawnFogDensity,
                DawnBloomThreshold, DawnBloomIntensity, NightBloomScatter, 0f);
            return "Post_Night: " + night + "\nPost_Dawn: " + dawn;
        }

        static string TunePostProfile(string path, Color fogColor, float fogDensity,
            float bloomThreshold, float bloomIntensity, float bloomScatter, float grainIntensity)
        {
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                return "SKIPPED - profile missing (run LightShadow.Tools.SceneBuilder.BuildAllBatch first)";
            }

            // URP 17.4 has no Fog volume override; fog is applied per scene via
            // RenderSettings (see ApplySceneFog, called by the art pass). The
            // profile tuning therefore covers bloom/grain only.

            Bloom bloom = GetOrAdd<Bloom>(profile);
            bloom.threshold.overrideState = true;
            bloom.threshold.value = bloomThreshold;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = bloomIntensity;
            bloom.scatter.overrideState = true;
            bloom.scatter.value = bloomScatter;
            bloom.highQualityFiltering.overrideState = true;
            bloom.highQualityFiltering.value = false; // cheaper on mobile

            if (grainIntensity > 0f)
            {
                FilmGrain grain = GetOrAdd<FilmGrain>(profile);
                grain.type.overrideState = true;
                grain.type.value = FilmGrainLookup.Medium2;
                grain.intensity.overrideState = true;
                grain.intensity.value = grainIntensity;
            }

            // Depth of field stays off in gameplay: if an earlier pass added
            // it, switch the component off instead of deleting (idempotent).
            DepthOfField depth = GetComponentOrNull<DepthOfField>(profile);
            if (depth != null) depth.active = false;

            EditorUtility.SetDirty(profile);
            return "fog/bloom/grain tuned";
        }

        static T GetOrAdd<T>(VolumeProfile profile) where T : VolumeComponent, new()
        {
            T existing = GetComponentOrNull<T>(profile);
            if (existing != null) return existing;
            return profile.Add<T>(true);
        }

        /// <summary>Enables exponential scene fog on the currently open scene
        /// (URP 17.4 has no Fog volume override; RenderSettings is the control).
        /// The ending scene uses the dawn palette, everything else the night one.</summary>
        public static void ApplySceneFog(bool isDawn)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = isDawn ? DawnFogColor : NightFogColor;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = isDawn ? DawnFogDensity : NightFogDensity;
        }

        static T GetComponentOrNull<T>(VolumeProfile profile) where T : VolumeComponent
        {
            foreach (VolumeComponent component in profile.components)
            {
                if (component is T typed) return typed;
            }
            return null;
        }

        // ------------------------------------------------------------ helpers
        static void ConfigureSmokeHalo(ParticleSystem particles, Material puffMaterial)
        {
            var main = particles.main;
            main.duration = 8f;
            main.loop = true;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.5f, 4.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.8f);
            main.startColor = new ParticleSystem.MinMaxGradient(PuffColor);
            main.maxParticles = 24;
            main.gravityModifier = -0.02f; // smoke rises

            var emission = particles.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(4f);

            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.scale = new Vector3(0.6f, 1.6f, 0.6f);

            var sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 1.5f));

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[]
                {
                    new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.3f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.material = puffMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        static void ConfigureAdditiveParticleMaterial(Material material)
        {
            material.SetFloat("_Surface", 1f); // transparent
            material.SetFloat("_Blend", 1f); // additive
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.One);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        static void ConfigureAlphaParticleMaterial(Material material)
        {
            material.SetFloat("_Surface", 1f); // transparent
            material.SetFloat("_Blend", 0f); // alpha
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        // The Shadow's silhouette mesh: the tallest blocky character reads as
        // the most humanoid silhouette without any inspection tooling.
        static GameObject PickTallestCharacter()
        {
            string packPath = ThirdPartyFolder + "/KenneyBlockyCharacters";
            if (!AssetDatabase.IsValidFolder(packPath)) return null;

            GameObject best = null;
            float bestHeight = 0f;
            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { packPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) continue;
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model == null) continue;
                Bounds bounds = ComputePrefabBounds(model);
                if (bounds.size.y > bestHeight)
                {
                    bestHeight = bounds.size.y;
                    best = model;
                }
            }
            return best;
        }

        // Prefab-asset bounds in the asset's root space (no scene needed):
        // every mesh's local bounds corners go through localToWorldMatrix so
        // rotated/scaled FBX children land correctly.
        // Shared with SceneBuilderArt for the per-instance scale normalization.
        public static Bounds ComputePrefabBounds(GameObject prefabAsset)
        {
            bool hasAny = false;
            Vector3 min = Vector3.zero;
            Vector3 max = Vector3.zero;
            Matrix4x4 rootToWorld = prefabAsset.transform.worldToLocalMatrix;
            MeshFilter[] filters = prefabAsset.GetComponentsInChildren<MeshFilter>(true);
            foreach (MeshFilter filter in filters)
            {
                Mesh mesh = filter.sharedMesh;
                if (mesh == null) continue;
                Matrix4x4 meshToRoot = rootToWorld * filter.transform.localToWorldMatrix;
                Bounds meshBounds = mesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 corner = meshBounds.center
                        + Vector3.Scale(meshBounds.extents,
                            new Vector3(
                                (i & 1) == 0 ? -1f : 1f,
                                (i & 2) == 0 ? -1f : 1f,
                                (i & 4) == 0 ? -1f : 1f));
                    Vector3 inRoot = meshToRoot.MultiplyPoint3x4(corner);
                    if (!hasAny)
                    {
                        min = inRoot;
                        max = inRoot;
                        hasAny = true;
                    }
                    else
                    {
                        min = Vector3.Min(min, inRoot);
                        max = Vector3.Max(max, inRoot);
                    }
                }
            }
            return new Bounds((min + max) * 0.5f, hasAny ? max - min : Vector3.one);
        }

        static Material GetOrCreateLitMaterial(string name, Shader lit)
        {
            return GetOrCreateMaterial(name, lit);
        }

        static Material GetOrCreateMaterial(string name, Shader shader)
        {
            string path = MaterialsFolder + "/" + name + ".mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;

            Material material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        // Copies the texture wiring of one Mat_Art_* onto another (used for the
        // tinted variants: ceiling/gate/stone share maps with their source).
        static void CopyTextureSetup(string sourceName, Material target)
        {
            Material source = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/" + sourceName + ".mat");
            if (source == null) return;
            target.SetTexture("_BaseMap", source.GetTexture("_BaseMap"));
            target.SetTexture("_BumpMap", source.GetTexture("_BumpMap"));
            if (source.GetTexture("_BumpMap") != null) target.EnableKeyword("_NORMALMAP");
            target.SetTexture("_MetallicGlossMap", source.GetTexture("_MetallicGlossMap"));
            if (source.GetTexture("_MetallicGlossMap") != null) target.EnableKeyword("_METALLICSPECGLOSSMAP");
            target.SetFloat("_SmoothnessTextureChannel", source.GetFloat("_SmoothnessTextureChannel"));
        }

        static T LoadAssetGuaranteed<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                Debug.LogWarning("[ArtImporter] Expected asset missing at " + path);
            }
            return asset;
        }

        static T[] FillArray<T>(int length, T value)
        {
            T[] result = new T[Mathf.Max(1, length)];
            for (int i = 0; i < result.Length; i++) result[i] = value;
            return result;
        }

        static Transform FindChildByName(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindChildByName(parent.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;
            string parent = Path.GetDirectoryName(folderPath).Replace('\\', '/');
            string leaf = Path.GetFileName(folderPath);
            if (string.IsNullOrEmpty(parent) || !parent.StartsWith("Assets")) return;
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
