using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using LightShadow.Gameplay;

namespace LightShadow.Tools
{
    // Art-swap layer over the generated scenes (DECISIONS.md D3): the
    // SceneBuilder keeps building the validated greybox, and this partial
    // walks the built scenes afterwards and upgrades the VISUALS in place.
    //
    // Rules of the pass:
    //   - objects are never deleted or renamed, colliders never touched
    //     (the NavMesh and ValidateBatch counts stay valid);
    //   - TextMesh labels stay as they are;
    //   - a primitive prop keeps its collider but its renderer is disabled
    //     and a real mesh ("ArtModel_*") is parented in its place;
    //   - architectural boxes (walls, floors, ceilings) keep the cube mesh
    //     and receive tiled PBR materials instead (per-size variants so the
    //     texture density stays true to world scale);
    //   - everything is idempotent: materials check for Mat_Art_*, model
    //     swaps check for an existing ArtModel_* child.
    public static partial class SceneBuilder
    {
        const string ArtMaterialsFolder = "Assets/_Project/Materials/Art";
        const string ArtPrefabsFolder = "Assets/_Project/Prefabs/Art";
        const string ThirdPartyFolder = "Assets/ThirdParty";

        // ------------------------------------------------------ swap targets
        // Kenney model basenames used by the swap table (per Docs/ASSET_LICENSES.md).
        const string ModelDesk = "desk";
        const string ModelTable = "table";
        const string ModelChair = "chairDesk";
        const string ModelBookcaseOpen = "bookcaseOpen";
        const string ModelBookcaseClosed = "bookcaseClosedDoors";
        const string ModelBench = "bench";
        const string ModelRug = "rugRectangle";
        const string ModelCardboardBox = "cardboardBoxClosed";
        const string ModelPlantBush = "plant_bushSmall";
        const string ModelBush = "plant_bush";
        const string ModelTree = "tree_detailed";
        const string ModelPine = "pine";
        const string ModelLightpost = "lightpost-single";
        const string ModelFence = "iron-fence";

        // P2 hero props: Blender FBX exports under _Project/Models/Hero
        // (Art/pipeline). Real-world metres, Unity axes, bottom-centre origins.
        // Placed uniformly (never stretched) and rebound onto the canonical
        // art materials by name via ArtImporter.HeroRemapMaterial.
        const string HeroModelFolder = "Assets/_Project/Models/Hero";
        const string HeroModelMemoryBox = "SM_Story_MemoryBox";
        const string HeroModelCard = "SM_Story_Card";
        const string HeroModelBookmark = "SM_Story_Bookmark";
        const string HeroModelExamPaper = "SM_Story_ExamPaper";
        const string HeroModelPrism = "SM_Story_Prism";
        const string HeroModelPhotograph = "SM_Story_Photograph";
        const string HeroModelProgramme = "SM_Story_Programme";
        const string HeroModelMug = "SM_Story_Mug";
        const string HeroModelFlashlight = "SM_Player_Flashlight";
        const string HeroModelDoorGrille = "SM_Arch_DoorGrille";
        const string HeroModelChalkboard = "SM_Arch_Chalkboard";
        const string HeroModelGateCrank = "SM_Arch_GateCrank";
        const string HeroModelIronGate = "SM_Arch_IronGate";

        // Target real-world sizes (metres) for the swapped models. They come
        // from the greybox dimensions the SceneBuilder builds, not from taste.
        const float DeskTargetHeight = 0.78f;
        const float ChairTargetHeight = 0.85f;
        const float TableTargetHeight = 0.78f;
        const float ShelfTargetHeight = 2.4f;
        const float CabinetTargetHeight = 2.1f;
        const float BenchTargetHeight = 0.9f;
        const float BushTargetHeight = 1.1f;
        const float PlanterBushTargetHeight = 0.55f;
        const float TreeTargetHeight = 5.5f;
        const float PineTargetHeight = 6.5f;
        const float LightpostTargetHeight = 3.4f;
        const float FenceTargetHeight = 1.7f;
        const float FenceSegmentWidth = 2.4f;
        const float BoxTargetHeight = 0.32f;
        const float RugTargetHeight = 0.04f;

        // Hero swap targets. Door/chalkboard match the greybox hosts they
        // replace (both FBX models are authored at exactly these heights, so
        // the normalisation lands on scale 1); the QuizBoard prefab board is
        // 1.2 m tall like the portable chalkboard FBX.
        const float DoorTargetHeight = 2.05f;
        const float ChalkboardHostTargetHeight = 1.9f;
        const float QuizBoardChalkboardTargetHeight = 1.2f;
        const int IronGateSegmentsPerSide = 2;
        const float StoryItemAnchorY = 0.4f; // fragment "Paper" placeholder centre

        // Player-hand anchor for SM_Player_Flashlight. The FBX origin is the
        // torch tail (bottom centre), so the anchor compensates for the half
        // length the old centre-origin cylinder primitive implied, and the
        // pitch turns the model's +Y axis into the hand's forward axis with
        // the same 6-degree roll as the primitive. Public because
        // ArtImporter's prefab restyle installs the identical child.
        public static readonly Vector3 FlashlightHandAnchor = new Vector3(0.028f, 0.035f, -0.176f);
        public static readonly Quaternion FlashlightHandRotation = Quaternion.Euler(90f, 0f, 6f);

        // Texture world-size (metres per full UV tile) per surface material.
        const float PlasterTileMeters = 3f;
        const float WoodTileMeters = 2.5f;
        const float MetalTileMeters = 1f;
        const float GrassTileMeters = 2f;
        const float FabricTileMeters = 1.5f;
        const float StoneTileMeters = 2f;
        const float DoorWoodTileMeters = 1.5f;

        // Dust motes: three placements matching the big lit spaces.
        static readonly Vector3[] DustPositions =
        {
            new Vector3(0f, 1.4f, -29f),  // entrance checkpoint
            new Vector3(0f, 1.8f, 0f),    // central corridor
            new Vector3(0f, 2.0f, 33f)    // main hall
        };

        // Garden dressing: two trees inside the quad and two lampposts by the
        // benches, placed clear of the NavMesh-carved walk area.
        static readonly Vector3[] GardenTrees =
        {
            new Vector3(17f, 0f, 16.2f),
            new Vector3(6.5f, 0f, 16.2f)
        };
        static readonly Vector3[] GardenLampposts =
        {
            new Vector3(4.2f, 0f, 5.2f),
            new Vector3(19.5f, 0f, 5.2f)
        };

        // Forecourt: pines flanking the approach road and an iron fence line
        // with the gate gap kept open (decor only, no colliders).
        static readonly Vector3[] ForecourtPines =
        {
            new Vector3(-7.5f, 0f, -39.5f),
            new Vector3(7.5f, 0f, -39.5f)
        };
        const float ForecourtFenceZ = -36.5f;
        const int FenceSegmentsPerSide = 5;
        const float ForecourtFenceGapHalfWidth = 2.6f;

        // ---------------------------------------------------------------- run
        // Called by Unity in batch mode:
        // -executeMethod LightShadow.Tools.SceneBuilder.ApplyArtPassBatch
        public static void ApplyArtPassBatch()
        {
            try
            {
                string summary = ApplyArtPass();
                Debug.Log("[LightShadow] ART PASS\n" + summary);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError("[LightShadow] ART PASS FAILED: " + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("LightShadow/Art/2. Apply Art Pass", priority = 42)]
        public static void ApplyArtPassMenu()
        {
            string summary = ApplyArtPass();
            EditorUtility.DisplayDialog("LightShadow - Art Pass", summary, "OK");
        }

        // Swap counters for the report line (reset every run so re-running the
        // pass reports honestly).
        static readonly Dictionary<string, int> ArtSwapCounts = new Dictionary<string, int>();
        static int ArtSkippedNoModel;
        static readonly Dictionary<string, GameObject> ModelAssetCache = new Dictionary<string, GameObject>();
        static readonly Dictionary<string, Material> ArtMaterialCache = new Dictionary<string, Material>();

        public static string ApplyArtPass()
        {
            AssetDatabase.Refresh();
            ArtSwapCounts.Clear();
            ArtSkippedNoModel = 0;

            // Run the same table over the school and the ending set.
            ApplyArtToScene(SchoolPath);
            ApplyArtToScene(EndingPath);

            var summary = new System.Text.StringBuilder();
            summary.AppendLine("Art swap counts:");
            foreach (KeyValuePair<string, int> pair in ArtSwapCounts)
            {
                summary.AppendLine("- " + pair.Key + ": " + pair.Value);
            }
            summary.AppendLine("- skipped (missing model): " + ArtSkippedNoModel);
            return summary.ToString();
        }

        static void ApplyArtToScene(string scenePath)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                Debug.LogWarning("[LightShadow] Art pass: scene missing, run Build Scenes first: " + scenePath);
                return;
            }

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Scene scene = EditorSceneManager.GetActiveScene();
            string sceneName = scenePath.Substring(scenePath.LastIndexOf('/') + 1);

            // Exponential scene fog (night palette; dawn palette for the ending).
            ArtImporter.ApplySceneFog(sceneName.StartsWith("03_Ending"));

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                ProcessTransform(root.transform);
            }

            // Material dedupe sweep: every renderer still pointing at a pack
            // material moves onto its shared twin (fewer SetPass calls, one
            // pass over all renderers, idempotent by construction).
            RemapThirdPartyMaterials(scene);

            if (scenePath == SchoolPath)
            {
                AddDustMotes();
            }

            SaveScene(scene, scenePath);
            Debug.Log("[LightShadow] Art pass applied to " + sceneName);
        }

        // Material dedupe sweep (see ArtImporter.DedupeThirdPartyMaterials):
        // every renderer whose sharedMaterials point into Assets/ThirdParty is
        // re-pointed at the shared twin. Runs after the swap table so it also
        // covers the ArtModel_ children the table just created.
        static void RemapThirdPartyMaterials(Scene scene)
        {
            int remapped = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    Material[] materials = renderer.sharedMaterials;
                    bool changed = false;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        Material replacement = ArtImporter.SharedReplacementFor(materials[i]);
                        if (replacement == null) continue;
                        materials[i] = replacement;
                        changed = true;
                    }
                    if (!changed) continue;
                    renderer.sharedMaterials = materials;
                    EditorUtility.SetDirty(renderer);
                    remapped++;
                }
            }
            Debug.Log("[LightShadow] Material dedupe: remapped " + remapped + " renderers in " + scene.path);
        }

        // -------------------------------------------------------- the table
        // Name-pattern dispatch. Order matters: exact family names before
        // prefixes they share ("BenchSeat" before "Bench_").
        static void ProcessTransform(Transform transform)
        {
            GameObject go = transform.gameObject;

            // Prefab instances own their own look: interactables keep their
            // hand-tuned palette, and Player/TheShadow are restyled at the
            // prefab level by ArtImporter. The ExitGate gains its crank and
            // KnowledgeFragment instances gain their story-item meshes here.
            if (PrefabUtility.IsPartOfPrefabInstance(go))
            {
                if (go.GetComponent<ExitGate>() != null) StyleExitGate(go);
                else if (go.GetComponent<KnowledgeFragment>() != null) StyleFragment(go);
                return; // never descend into prefab instances
            }

            // TextMesh labels (MeshRenderer + TextMesh) stay untouched.
            if (go.GetComponent<TextMesh>() != null) return;

            string name = go.name;

            // ---- architecture: material-only swaps -------------------------
            if (ApplyArchitectureMaterial(transform, name)) return;

            // ---- gate leaf bars (ending scene forecourt gate) --------------
            if (name.StartsWith("Bar_") || name == "RailTop" || name == "RailBottom")
            {
                SwapMaterialTiled(go, "Mat_Art_GateMetal", MetalTileMeters);
                return;
            }

            // ---- furniture groups ------------------------------------------
            if (name == "BenchSeat" || name == "BenchBack" || name == "BenchLegL" || name == "BenchLegR")
            {
                // The garden bench is built as four separate boxes; one real
                // bench model replaces all of them (the seat box hosts it).
                SwapGardenBench(transform);
                return;
            }
            if (name.StartsWith("BenchBack_"))
            {
                // Main-hall row backs belong to the bench model on the seat.
                HideRenderer(go);
                return;
            }
            if (name.StartsWith("Bench_"))
            {
                if (transform.parent != null && transform.parent.name == "PhysicsLab")
                {
                    // Lab benches stay boxes but get the brushed-metal look.
                    SwapMaterialTiled(go, "Mat_Art_MetalLocker", MetalTileMeters);
                }
                else
                {
                    SwapBoxWithModel(go, ModelBench, BenchTargetHeight);
                }
                return;
            }
            if (name.StartsWith("Desk_") || name == "TeacherDesk")
            {
                SwapFurnitureGroup(transform, ModelDesk, DeskTargetHeight);
                return;
            }
            if (name == "StaffTable" || name == "ReceptionDesk" || name == "ReadingTable" || name == "MotherDesk")
            {
                SwapFurnitureGroup(transform, ModelTable, TableTargetHeight);
                return;
            }
            if (name.StartsWith("Chair_"))
            {
                SwapFurnitureGroup(transform, ModelChair, ChairTargetHeight);
                return;
            }

            // ---- single-box props ------------------------------------------
            if (name.StartsWith("Shelf_"))
            {
                SwapBoxWithModel(go, ModelBookcaseOpen, ShelfTargetHeight);
                return;
            }
            if (name == "Cabinet")
            {
                SwapBoxWithModel(go, ModelBookcaseClosed, CabinetTargetHeight);
                return;
            }
            if (name == "CabinetTop" || name == "ReceptionInset")
            {
                HideRenderer(go);
                return;
            }
            if (name == "Rug")
            {
                SwapBoxWithModel(go, ModelRug, RugTargetHeight);
                return;
            }
            if (name == "MemoryBox")
            {
                SwapMemoryBox(go);
                return;
            }
            if (name.StartsWith("Door_"))
            {
                // Working doors (DoorController: hinge, collider, key lock)
                // keep their swinging wooden panel. The grille swap was written
                // for the old collider-free decor boxes; on a real door it parked
                // a static, non-swinging model on the root. Only decor doors swap.
                if (transform.GetComponent<DoorController>() == null) SwapDoorWithHeroModel(transform);
                return;
            }
            if (name.StartsWith("Planter_"))
            {
                SwapMaterialTiled(go, "Mat_Art_Stone", StoneTileMeters);
                if (!HasArtModel(transform))
                {
                    AddModelChild(go, ModelPlantBush, PlanterBushTargetHeight, 0f, 0.3f, Vector3.zero);
                }
                return;
            }
            if (name == "Bush")
            {
                SwapBushWithModel(go);
                return;
            }
            if (name.StartsWith("Lockers_"))
            {
                SwapMaterialTiled(go, "Mat_Art_MetalLocker", MetalTileMeters);
                return;
            }
            if (name.StartsWith("LampPost_"))
            {
                SwapBoxWithModel(go, ModelLightpost, LightpostTargetHeight);
                return;
            }

            // Children are processed unless this object was swapped wholesale.
            for (int i = 0; i < transform.childCount; i++)
            {
                ProcessTransform(transform.GetChild(i));
            }
        }

        // Material-only families: walls, floors, ceilings and stage surfaces.
        static bool ApplyArchitectureMaterial(Transform transform, string name)
        {
            switch (name)
            {
                case "Wall":
                case "EntranceLintel":
                case "MainHallLintel":
                    SwapMaterialTiled(transform.gameObject, "Mat_Art_PlasterWall", PlasterTileMeters);
                    return true;
                case "Ceiling":
                    SwapMaterialTiled(transform.gameObject, "Mat_Art_Ceiling", PlasterTileMeters);
                    return true;
                case "Floor":
                    SwapMaterialTiled(transform.gameObject, "Mat_Art_WoodFloor", WoodTileMeters);
                    return true;
                case "GardenGrass":
                    SwapMaterialTiled(transform.gameObject, "Mat_Art_Grass", GrassTileMeters);
                    return true;
                case "Stage":
                    SwapMaterialTiled(transform.gameObject, "Mat_Art_WoodFloor", WoodTileMeters);
                    return true;
                case "StageBackdrop":
                    SwapMaterialTiled(transform.gameObject, "Mat_Art_Fabric", FabricTileMeters);
                    return true;
                case "Chalkboard":
                    SwapChalkboardHost(transform);
                    return true;
                case "ChalkFrameTop":
                case "ChalkFrameBottom":
                {
                    // The hero chalkboard FBX carries its own frame, so the
                    // greybox frame strips go dark only when the model landed.
                    Transform board = transform.parent != null
                        ? transform.parent.Find("Chalkboard")
                        : null;
                    if (board != null && HasArtModel(board))
                    {
                        HideRenderer(transform.gameObject);
                    }
                    else
                    {
                        SwapMaterialTiled(transform.gameObject, "Mat_Art_DoorWood", DoorWoodTileMeters);
                    }
                    return true;
                }
                case "PillarL":
                case "PillarR":
                    SwapMaterialTiled(transform.gameObject, "Mat_Art_Stone", StoneTileMeters);
                    return true;
            }
            return false;
        }

        // The exit gate prefab instance: metal texture on the structural parts,
        // indicators and labels untouched. The moving leaf keeps its transform;
        // only the material changes so nothing can break the open animation.
        static void StyleExitGate(GameObject gate)
        {
            MeshRenderer[] renderers = gate.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer renderer in renderers)
            {
                string rendererName = renderer.gameObject.name;
                if (rendererName == "Screen" || rendererName == "ConsoleGlow") continue;
                if (rendererName == "Label") continue;
                SwapMaterialTiled(renderer.gameObject, "Mat_Art_GateMetal", MetalTileMeters);
            }
            Count("exit gate");
            SwapGateCrank(gate);
        }

        // -------------------------------------------------------- hero swaps
        // Each one is optional: when the FBX has not shipped the greybox
        // primitive (or its Kenney stand-in) stays, so the pass degrades,
        // never breaks.

        // The crank the story keeps mentioning, mounted on the console's
        // player-facing face. Native size (the FBX is authored at crank
        // scale); yaw 90 turns the axle towards the player, and the anchor
        // puts the wheel hub on the console collider's centre height (2.75,
        // 1.2) - the interact point - with the wall plate flush against the
        // console front. Fallback: nothing (the console stays bare).
        static void SwapGateCrank(GameObject gate)
        {
            if (gate.transform.Find("ArtModel_" + HeroModelGateCrank) != null) return;
            if (AddHeroModelChild(gate, HeroModelGateCrank, 0f,
                    Quaternion.Euler(0f, 90f, 0f), new Vector3(2.75f, 1.02f, 0.405f)) == null) return;
            Count("hero gate crank");
        }

        // The mother's memory box: the hero keepsake box replaces the Kenney
        // cardboard stand-in at its authored real-world size, with the story
        // card resting on the lid (the P3 inspect view reuses both hosts).
        // Fallback: the cardboard box swap (existing behaviour).
        static void SwapMemoryBox(GameObject box)
        {
            if (HasArtModel(box.transform)) return;
            float bottom = -box.transform.lossyScale.y * 0.5f;
            if (AddHeroModelChild(box, HeroModelMemoryBox, 0f,
                    Quaternion.Euler(0f, 12f, 0f), new Vector3(0f, bottom, 0f)) == null)
            {
                SwapBoxWithModel(box, ModelCardboardBox, BoxTargetHeight);
                return;
            }
            HideRenderer(box);
            Count("hero memory box");

            GameObject boxAsset = LoadModelAsset(HeroModelMemoryBox);
            float boxHeight = boxAsset != null ? ArtImporter.ComputePrefabBounds(boxAsset).size.y : 0.1f;
            if (AddHeroModelChild(box, HeroModelCard, 0f,
                    Quaternion.Euler(0f, 15f, 0f),
                    new Vector3(0.02f, bottom + boxHeight + 0.006f, 0.02f),
                    "ArtModel_" + HeroModelCard) != null)
            {
                Count("hero story card");
            }
        }

        // Locked wing doors: the hero grille door fills its doorway box
        // (decorative, no collider - the walk gap and NavMesh stay as built).
        // Fallback: door-wood material on the box (existing pattern).
        static void SwapDoorWithHeroModel(Transform door)
        {
            if (HasArtModel(door)) return;
            // Yaw 90 puts the model's width along the wall run; the origin is
            // the door's bottom edge, hence the half-height drop.
            if (AddHeroModelChild(door.gameObject, HeroModelDoorGrille, DoorTargetHeight,
                    Quaternion.Euler(0f, 90f, 0f),
                    new Vector3(0f, -door.transform.lossyScale.y * 0.5f, 0f)) == null)
            {
                SwapMaterialTiled(door.gameObject, "Mat_Art_DoorWood", DoorWoodTileMeters);
                return;
            }
            HideRenderer(door.gameObject);
            Count("hero door grille");
        }

        // Classroom chalkboard: the hero board (frame, tray, chalk sticks and
        // eraser included) replaces the greybox slab. Fallback: the plain
        // chalk material (existing behaviour).
        static void SwapChalkboardHost(Transform host)
        {
            if (HasArtModel(host)) return;
            // Yaw 180: the FBX chalk face points +Z and this wall faces -Z
            // into the room; the origin is the board's bottom edge.
            if (AddHeroModelChild(host.gameObject, HeroModelChalkboard, ChalkboardHostTargetHeight,
                    Quaternion.Euler(0f, 180f, 0f),
                    new Vector3(0f, -host.transform.lossyScale.y * 0.5f, 0f)) == null)
            {
                SwapMaterialPlain(host.gameObject, "Mat_Art_Chalkboard");
                return;
            }
            HideRenderer(host.gameObject);
            Count("hero chalkboard");
        }

        // Story-item hosts: each fragment instance gains its real hero mesh
        // centred on the paper placeholder's spot; the P3 inspect view will
        // reuse the same hosts. Fallback: the paper primitive stays.
        static readonly Dictionary<string, string> FragmentHeroModels = new Dictionary<string, string>
        {
            { "Fragment_Bookmark", HeroModelBookmark },
            { "Fragment_ExamPaper", HeroModelExamPaper },
            { "Fragment_Optics", HeroModelPrism },
            { "Fragment_Jasmine", HeroModelPhotograph },
            { "Fragment_Mug", HeroModelMug },
            { "Fragment_Programme", HeroModelProgramme },
        };

        static void StyleFragment(GameObject fragment)
        {
            if (!FragmentHeroModels.TryGetValue(fragment.name, out string modelName)) return;
            Transform visual = fragment.transform.Find("Visual");
            if (visual == null || visual.Find("ArtModel_" + modelName) != null) return;

            GameObject modelAsset = LoadModelAsset(modelName);
            if (modelAsset == null) return; // placeholder stays; LoadModelAsset already warned

            Vector3 rawSize = ArtImporter.ComputePrefabBounds(modelAsset).size;
            if (AddHeroModelChild(visual.gameObject, modelName, 0f, Quaternion.identity,
                    new Vector3(0f, StoryItemAnchorY - rawSize.y * 0.5f, 0f)) == null) return;

            Transform paper = visual.Find("Paper");
            if (paper != null) HideRenderer(paper.gameObject);
            Count("hero story item");
        }

        // ------------------------------------------------------- swap helpers
        // Furniture built as a group (Table/Chair factories): hide the visual
        // children, keep the collider on the seat/top, add one model child.
        static void SwapFurnitureGroup(Transform group, string modelName, float targetHeight)
        {
            if (HasArtModel(group)) return;

            Vector3 footprint = Vector3.zero;
            Transform seat = group.Find("Top");
            if (seat == null) seat = group.Find("Seat");
            if (seat != null)
            {
                footprint = new Vector3(seat.lossyScale.x, targetHeight, seat.lossyScale.z);
            }

            // Hide every direct child renderer (they are the greybox shapes).
            foreach (Transform child in group)
            {
                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer != null) renderer.enabled = false;
            }

            if (AddModelChild(group.gameObject, modelName, targetHeight, 0f, 0f, footprint))
            {
                Count(modelName);
            }
        }

        // Single greybox box: hide its renderer (any BoxCollider stays for
        // physics/NavMesh) and parent a real mesh at the same spot.
        static void SwapBoxWithModel(GameObject box, string modelName, float targetHeight)
        {
            if (HasArtModel(box.transform)) return;
            HideRenderer(box);
            Vector3 scale = box.transform.lossyScale;
            var footprint = new Vector3(scale.x, targetHeight, scale.z);
            if (AddModelChild(box, modelName, targetHeight, 0f, 0f, footprint))
            {
                Count(modelName);
            }
        }

        // Bush spheres become real foliage meshes sized to the sphere.
        static void SwapBushWithModel(GameObject bush)
        {
            if (HasArtModel(bush.transform)) return;
            HideRenderer(bush);
            float diameter = bush.transform.lossyScale.x;
            var footprint = new Vector3(diameter, BushTargetHeight, diameter);
            if (AddModelChild(bush, ModelBush, BushTargetHeight, 0f, -diameter * 0.35f, footprint))
            {
                Count("bush");
            }
        }

        // The four-part garden bench: the seat box hosts the real bench, the
        // other three parts only lose their renderers.
        static void SwapGardenBench(Transform part)
        {
            if (part.name != "BenchSeat")
            {
                HideRenderer(part.gameObject);
                return;
            }
            if (HasArtModel(part)) return;

            Vector3 position = part.position;
            HideRenderer(part.gameObject);
            GameObject model = AddRawModelChild(part.gameObject, ModelBench, BenchTargetHeight);
            if (model == null) return;

            // The seat box floats at bench height; the model's own origin sits
            // at ground level, so plant it on the floor below the seat and turn
            // it to face the path (the greybox seat ran along X).
            model.transform.position = new Vector3(position.x, 0f, position.z);
            model.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            Count("garden bench");
        }

        // ----------------------------------------------------- model plumbing
        static bool HasArtModel(Transform host)
        {
            for (int i = 0; i < host.childCount; i++)
            {
                if (host.GetChild(i).name.StartsWith("ArtModel_")) return true;
            }
            return false;
        }

        static void HideRenderer(GameObject go)
        {
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }

        // Instantiates the model under host with a uniform height scale and a
        // horizontally clamped footprint (furniture must never look melted).
        // Returns true when applied.
        static bool AddModelChild(GameObject host, string modelName, float targetHeight,
            float yawDegrees, float yOffset, Vector3 footprint)
        {
            GameObject model = AddRawModelChild(host, modelName, targetHeight);
            if (model == null) return false;

            if (yawDegrees != 0f)
            {
                model.transform.localRotation = Quaternion.Euler(0f, yawDegrees, 0f);
            }
            model.transform.localPosition = new Vector3(0f, yOffset, 0f);

            Vector2 rawFootprint = RawFootprint(modelName);
            float uniform = model.transform.localScale.y;
            if (footprint.x > 0f && footprint.z > 0f && uniform > 0f
                && rawFootprint.x > 0f && rawFootprint.y > 0f)
            {
                // World width = rawWidth * uniform * stretch; keep stretch in
                // [0.45, 1.55] around the uniform height scale.
                float wantX = Mathf.Clamp(footprint.x / (rawFootprint.x * uniform), 0.45f, 1.55f);
                float wantZ = Mathf.Clamp(footprint.z / (rawFootprint.y * uniform), 0.45f, 1.55f);
                model.transform.localScale = new Vector3(uniform * wantX, uniform, uniform * wantZ);
            }
            return true;
        }

        // Adds the model as a child scaled uniformly so its height matches
        // targetHeight. Returns the instance (null when the model is missing).
        static GameObject AddRawModelChild(GameObject host, string modelName, float targetHeight)
        {
            GameObject modelAsset = LoadModelAsset(modelName);
            if (modelAsset == null)
            {
                ArtSkippedNoModel++;
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset, host.transform);
            instance.name = "ArtModel_" + modelName;

            // Imported models carry no colliders, but destroy defensively so
            // the art pass can never alter physics or the NavMesh.
            foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
            {
                UnityEngine.Object.DestroyImmediate(collider, true);
            }

            // Keep every node on the host's layer so camera culling and the
            // static-flag pass see one consistent set.
            foreach (Transform node in instance.GetComponentsInChildren<Transform>(true))
            {
                node.gameObject.layer = host.layer;
            }

            Vector3 rawSize = ArtImporter.ComputePrefabBounds(modelAsset).size;
            float uniform = targetHeight / Mathf.Max(0.05f, rawSize.y);
            instance.transform.localScale = new Vector3(uniform, uniform, uniform);
            return instance;
        }

        // Footprint (x/z) of the source prefab in metres from its raw bounds.
        static Vector2 RawFootprint(string modelName)
        {
            GameObject modelAsset = LoadModelAsset(modelName);
            if (modelAsset == null) return new Vector2(1f, 1f);
            Vector3 size = ArtImporter.ComputePrefabBounds(modelAsset).size;
            return new Vector2(Mathf.Max(0.05f, size.x), Mathf.Max(0.05f, size.z));
        }

        static GameObject LoadModelAsset(string modelName)
        {
            if (ModelAssetCache.TryGetValue(modelName, out GameObject cached))
            {
                if (cached != null) return cached;
            }

            // Hero FBX models (_Project/Models/Hero, Art/pipeline exports)
            // resolve alongside the Kenney packs; the exact-filename check
            // keeps the two namespaces apart.
            string[] guids = AssetDatabase.FindAssets(modelName, new[] { HeroModelFolder, ThirdPartyFolder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = path.Substring(path.LastIndexOf('/') + 1);
                if (!fileName.Equals(modelName + ".fbx", StringComparison.OrdinalIgnoreCase)
                    && !fileName.Equals(modelName + ".obj", StringComparison.OrdinalIgnoreCase)) continue;
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset != null)
                {
                    ModelAssetCache[modelName] = asset;
                    return asset;
                }
            }
            Debug.LogWarning("[LightShadow] Art pass: model not found: " + modelName);
            ModelAssetCache[modelName] = null;
            return null;
        }

        // -------------------------------------------------------- hero plumbing
        // Every hero swap is optional: when the FBX has not shipped the
        // greybox primitive (or its Kenney stand-in) stays, so the pass
        // degrades gracefully, never breaks.

        // Instantiates a hero FBX under host, uniformly scaled in WORLD space
        // so its height matches targetHeight (0 = native authored size), with
        // the given yaw and a metre-space local position. The model rides on
        // an unscaled carrier because the greybox hosts are scaled cubes: a
        // direct child would inherit their per-axis scale, and a yaw would
        // then swap the compensation axes. Hosts must therefore be
        // axis-aligned with identity rotation (all swap callers are). Returns
        // the FBX instance, or null when the model is missing.
        static GameObject AddHeroModelChild(GameObject host, string modelName, float targetHeight,
            Quaternion localRotation, Vector3 localPositionMeters, string carrierName = null)
        {
            GameObject modelAsset = LoadModelAsset(modelName);
            if (modelAsset == null)
            {
                ArtSkippedNoModel++;
                return null;
            }

            var carrier = new GameObject(carrierName ?? ("ArtModel_" + modelName));
            carrier.transform.SetParent(host.transform, false);
            Vector3 lossy = host.transform.lossyScale;
            Vector3 inverse = new Vector3(
                Mathf.Abs(lossy.x) > 0.0001f ? 1f / lossy.x : 1f,
                Mathf.Abs(lossy.y) > 0.0001f ? 1f / lossy.y : 1f,
                Mathf.Abs(lossy.z) > 0.0001f ? 1f / lossy.z : 1f);
            carrier.transform.localScale = inverse;
            carrier.transform.localPosition = Vector3.Scale(localPositionMeters, inverse);
            carrier.layer = host.layer;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset, carrier.transform);
            instance.name = modelName;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = localRotation;

            // Imported models carry no colliders, but destroy defensively so
            // the art pass can never alter physics or the NavMesh.
            foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
            {
                UnityEngine.Object.DestroyImmediate(collider, true);
            }

            // Keep every node on the host's layer so camera culling and the
            // static-flag pass see one consistent set.
            foreach (Transform node in instance.GetComponentsInChildren<Transform>(true))
            {
                node.gameObject.layer = host.layer;
            }

            RebindHeroMaterials(instance);

            Vector3 rawSize = ArtImporter.ComputePrefabBounds(modelAsset).size;
            float uniform = targetHeight > 0f ? targetHeight / Mathf.Max(0.05f, rawSize.y) : 1f;
            instance.transform.localScale = new Vector3(uniform, uniform, uniform);
            return instance;
        }

        // FBX material rebind: the importer writes one external .mat per FBX
        // material next to the model; those generated twins must not ship.
        // Every renderer under the instance moves onto the canonical art
        // material with the same name (ArtImporter.HeroRemapMaterial); names
        // that are not registered keep their twin, which the importer already
        // converted to URP/Lit. Public because ArtImporter's prefab restyle
        // installs hero meshes outside the scene swap table.
        public static void RebindHeroMaterials(GameObject instance)
        {
            foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < materials.Length; i++)
                {
                    Material canonical = ArtImporter.HeroRemapMaterial(
                        materials[i] != null ? materials[i].name : null);
                    if (canonical == null) continue;
                    materials[i] = canonical;
                    changed = true;
                }
                if (!changed) continue;
                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
            }
        }

        // -------------------------------------------------- material plumbing
        // Architectural boxes: swap to a per-size tiled variant of the surface
        // material so texture density stays constant across wall lengths.
        static void SwapMaterialTiled(GameObject go, string baseMaterialName, float metersPerTile)
        {
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer == null) return;
            if (renderer.sharedMaterial != null && renderer.sharedMaterial.name.StartsWith("Mat_Art")) return;

            Vector3 scale = go.transform.lossyScale;
            float[] axes = { Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z) };
            Array.Sort(axes);
            Array.Reverse(axes);
            float u = Mathf.Max(0.5f, Mathf.Round(axes[0] / metersPerTile * 2f) * 0.5f);
            float v = Mathf.Max(0.5f, Mathf.Round(axes[1] / metersPerTile * 2f) * 0.5f);

            // Dots are illegal-ish in asset names (they read as extensions), so
            // the half-metre grid is encoded as whole numbers.
            string variantName = baseMaterialName + "_T" + (int)(u * 2f) + "x" + (int)(v * 2f);
            Material material = GetTiledVariant(baseMaterialName, variantName, u, v);
            if (material == null) return;

            renderer.sharedMaterial = material;
            Count(baseMaterialName);
        }

        static void SwapMaterialPlain(GameObject go, string materialName)
        {
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer == null) return;
            if (renderer.sharedMaterial != null && renderer.sharedMaterial.name.StartsWith("Mat_Art")) return;
            Material material = GetArtMaterial(materialName);
            if (material == null) return;
            renderer.sharedMaterial = material;
            Count(materialName);
        }

        static Material GetArtMaterial(string materialName)
        {
            if (ArtMaterialCache.TryGetValue(materialName, out Material cached) && cached != null)
            {
                return cached;
            }
            Material material = AssetDatabase.LoadAssetAtPath<Material>(ArtMaterialsFolder + "/" + materialName + ".mat");
            if (material == null)
            {
                Debug.LogWarning("[LightShadow] Art pass: material missing, run ArtImporter first: " + materialName);
                return null;
            }
            ArtMaterialCache[materialName] = material;
            return material;
        }

        static Material GetTiledVariant(string baseMaterialName, string variantName, float u, float v)
        {
            string path = ArtMaterialsFolder + "/Tiled/" + variantName + ".mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;

            Material baseMaterial = GetArtMaterial(baseMaterialName);
            if (baseMaterial == null) return null;

            EnsureArtFolder(ArtMaterialsFolder + "/Tiled");
            Material variant = new Material(baseMaterial)
            {
                name = variantName
            };
            Vector2 tiling = new Vector2(u, v);
            variant.SetTextureScale("_BaseMap", tiling);
            variant.SetTextureScale("_BumpMap", tiling);
            variant.SetTextureScale("_MetallicGlossMap", tiling);
            AssetDatabase.CreateAsset(variant, path);
            return variant;
        }

        // ---------------------------------------------------------- dressing
        static void AddDustMotes()
        {
            GameObject dustPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArtPrefabsFolder + "/DustMotes.prefab");
            if (dustPrefab == null)
            {
                Debug.LogWarning("[LightShadow] Art pass: DustMotes.prefab missing, run ArtImporter first.");
                return;
            }

            GameObject environment = GameObject.Find("Level/Environment");
            if (environment == null) environment = GameObject.Find("Level");
            if (environment == null) return;

            for (int i = 0; i < DustPositions.Length; i++)
            {
                string dustName = "DustMotes_" + i;
                if (environment.transform.Find(dustName) != null) continue;
                GameObject dust = (GameObject)PrefabUtility.InstantiatePrefab(dustPrefab, environment.transform);
                dust.name = dustName;
                dust.transform.position = DustPositions[i];
                Count("dust motes");
            }

            AddGardenDressing(environment.transform);
        }

        // Decor only: trees and lampposts in the garden quad, pines and an
        // iron fence line on the forecourt. No colliders, nothing the
        // validator counts, placed clear of the walkable NavMesh paths.
        static void AddGardenDressing(Transform environment)
        {
            Transform garden = environment.Find("Garden");
            GameObject gardenHost = garden != null ? garden.gameObject : environment.gameObject;

            foreach (Vector3 position in GardenTrees)
            {
                AddDecor(gardenHost, "ArtTree", ModelTree, TreeTargetHeight, position);
            }
            foreach (Vector3 position in GardenLampposts)
            {
                AddDecor(gardenHost, "ArtLampPost", ModelLightpost, LightpostTargetHeight, position);
            }

            foreach (Vector3 position in ForecourtPines)
            {
                AddDecor(environment.gameObject, "ArtPine", ModelPine, PineTargetHeight, position);
            }

            AddForecourtFence(environment.gameObject);
        }

        static void AddDecor(GameObject parent, string prefix, string modelName, float targetHeight, Vector3 position)
        {
            if (parent.transform.Find(prefix) != null) return;
            GameObject model = AddRawModelChild(parent, modelName, targetHeight);
            if (model == null) return;
            model.name = prefix;
            model.transform.position = position;
            Count(prefix);
        }

        static void AddForecourtFence(GameObject environment)
        {
            if (environment.transform.Find("ArtFenceLine") != null) return;
            var line = new GameObject("ArtFenceLine");
            line.transform.SetParent(environment.transform, false);
            line.layer = environment.layer;

            for (int side = 0; side < 2; side++)
            {
                float direction = side == 0 ? -1f : 1f;
                for (int i = 0; i < FenceSegmentsPerSide; i++)
                {
                    float x = direction * (ForecourtFenceGapHalfWidth + FenceSegmentWidth * (i + 0.5f));
                    GameObject model = AddRawModelChild(line, ModelFence, FenceTargetHeight);
                    if (model == null) return;
                    model.name = "ArtFence_" + side + "_" + i;
                    model.transform.position = new Vector3(x, 0f, ForecourtFenceZ);
                }
            }
            Count("forecourt fence");

            // Hero iron gate sections extend the Kenney fence line into the
            // gate gap, inward from each fence end, leaving the road open
            // (decor only, no colliders, same FenceTargetHeight normalization
            // as the Kenney fence so the rails line up).
            GameObject gateAsset = LoadModelAsset(HeroModelIronGate);
            if (gateAsset == null) return;
            Vector3 gateSize = ArtImporter.ComputePrefabBounds(gateAsset).size;
            float gateWidth = gateSize.x * (FenceTargetHeight / Mathf.Max(0.05f, gateSize.y));
            for (int side = 0; side < 2; side++)
            {
                float direction = side == 0 ? -1f : 1f;
                for (int i = 0; i < IronGateSegmentsPerSide; i++)
                {
                    float x = direction * (ForecourtFenceGapHalfWidth - gateWidth * (i + 0.5f));
                    if (AddHeroModelChild(line, HeroModelIronGate, FenceTargetHeight,
                            Quaternion.identity, new Vector3(x, 0f, ForecourtFenceZ),
                            "ArtModel_" + HeroModelIronGate + "_" + side + "_" + i) == null) return;
                }
            }
            Count("forecourt iron gate");
        }

        // ------------------------------------------------------------ misc
        static void Count(string family)
        {
            ArtSwapCounts.TryGetValue(family, out int current);
            ArtSwapCounts[family] = current + 1;
        }

        static void EnsureArtFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;
            string parentPath = folderPath.Substring(0, folderPath.LastIndexOf('/'));
            string leaf = folderPath.Substring(folderPath.LastIndexOf('/') + 1);
            EnsureArtFolder(parentPath);
            AssetDatabase.CreateFolder(parentPath, leaf);
        }
    }
}
