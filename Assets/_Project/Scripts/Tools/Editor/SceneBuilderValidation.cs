using System;
using System.Collections.Generic;
using System.Text;
using LightShadow.Core;
using LightShadow.Enemy;
using LightShadow.Gameplay;
using LightShadow.Player;
using LightShadow.UI;
using UnityEngine.Audio;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LightShadow.Tools
{
    // Post-build smoke checks. This does not enter Play mode (batch mode has no
    // input); it verifies the generated scene graph, wiring, and NavMesh so a
    // broken build fails loudly instead of at the showcase.
    public static partial class SceneBuilder
    {
        static bool HasAncestorNamed(Transform t, string ancestorName)
        {
            for (Transform p = t.parent; p != null; p = p.parent)
            {
                if (p.name == ancestorName) return true;
            }
            return false;
        }

        static string ValidateScenes()
        {
            var report = new StringBuilder();
            var problems = new List<string>();

            if (EditorBuildSettings.scenes.Length != 4)
            {
                problems.Add("Build settings should contain exactly 4 scenes, found " + EditorBuildSettings.scenes.Length + ".");
            }

            // Light budget (audit S1): the school must stay dark. No directional
            // fill, no moon/window fills, and a capped point-light count so
            // mobile stays cheap and the torch stays the main light.
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SchoolPath) != null)
            {
                EditorSceneManager.OpenScene(SchoolPath, OpenSceneMode.Single);
                int points = 0;
                foreach (Light light in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (light.type == LightType.Directional) problems.Add("School scene has a directional light (" + light.name + "); it must stay dark.");
                    if (light.type == LightType.Point)
                    {
                        points++;
                        // Torch-first darkness (playtest 2026-11-28): no fixed lamp may
                        // out-shine the flashlight. Wing lamps and the entrance stay soft.
                        // Dawn exception: the DawnChase corridor lamps are the story's dawn.
                        // They start inactive and only come up (DawnRiser) after the gate is
                        // powered, so they are not part of the dark night the cap protects.
                        // AddLamp nests the light as DawnChase/Lamp/Light, so look at
                        // every ancestor (a direct-parent check never matched; the
                        // first editor run failed on all four dawn lamps).
                        bool dawnLamp = HasAncestorNamed(light.transform, "DawnChase");
                        if (!dawnLamp && light.intensity > 1.0f) problems.Add("School lamp too bright for a torch-first game: " + light.name + " intensity " + light.intensity.ToString("0.0") + " (max 1.0).");
                    }
                    if (light.name.Contains("Moon")) problems.Add("School scene still has a moon fill light: " + light.name);
                }
                Color ambient = RenderSettings.ambientLight;
                float ambientLuma = ambient.r * 0.2126f + ambient.g * 0.7152f + ambient.b * 0.0722f;
                if (ambientLuma > 0.03f) problems.Add("School ambient light is " + ambientLuma.ToString("0.000") + "; it must stay near black (max 0.03).");
                report.AppendLine("School point lights: " + points);
                if (points > 90) problems.Add("School scene has " + points + " point lights; budget is 90.");
            }

            string[] expectedScenes = { BootstrapPath, MainMenuPath, SchoolPath, EndingPath };
            foreach (string path in expectedScenes)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
                {
                    problems.Add("Missing scene asset: " + path);
                }
            }

            // Question bank integrity (doc 4.1 and T13): at least 60 valid
            // questions, every one draft-marked for the human fact-check and
            // carrying full Arabic fields (task P4; HasArabic + explanationAr).
            QuestionDatabase database = AssetDatabase.LoadAssetAtPath<QuestionDatabase>(DatabasePath);
            if (database == null)
            {
                problems.Add("Missing QuestionDatabase asset.");
            }
            else
            {
                int valid = 0;
                int arabic = 0;
                var categories = new HashSet<QuestionCategory>();
                foreach (QuestionData question in database.all)
                {
                    if (question == null || !question.IsValid) continue;
                    valid++;
                    categories.Add(question.category);
                    if (question.HasArabic && !string.IsNullOrWhiteSpace(question.explanationAr)) arabic++;
                    if (string.IsNullOrEmpty(question.factSource)) problems.Add("Question " + question.name + " has no fact source.");
                }

                report.AppendLine("Question bank: " + valid + " valid / " + database.all.Count + " linked");
                int pendingFactCheck = 0;
                foreach (QuestionData q in database.all)
                {
                    if (q != null && q.factSource == "NEEDS-FACT-CHECK") pendingFactCheck++;
                }
                report.AppendLine("Questions awaiting teacher fact-check: " + pendingFactCheck + "/" + database.all.Count
                    + (pendingFactCheck > 0 ? " (RELEASE GATE OPEN: do not ship as verified)" : ""));
                report.AppendLine("Question categories: " + categories.Count + "/6");
                report.AppendLine("Question Arabic fields: " + arabic + "/" + valid);
                if (valid < 60) problems.Add("Question bank has " + valid + " valid questions; the project document asks for 60+.");
                if (categories.Count < 6) problems.Add("Question bank is missing one or more categories.");
                if (arabic < valid) problems.Add("Question bank: " + (valid - arabic) + " of " + valid
                    + " valid questions lack full Arabic fields (questionAr/optionsAr/explanationAr).");
            }

            EditorSceneManager.OpenScene(SchoolPath, OpenSceneMode.Single);
            report.AppendLine("School scene root objects: " + EditorSceneManager.GetActiveScene().rootCount);

            GameManager manager = Object.FindAnyObjectByType<GameManager>();
            if (manager == null) problems.Add("School: no GameManager.");
            else
            {
                if (manager.config == null) problems.Add("School: GameManager has no GameConfig.");
                if (manager.player == null) problems.Add("School: GameManager has no player.");
                if (manager.enemy == null) problems.Add("School: GameManager has no enemy.");
                if (manager.database == null) problems.Add("School: GameManager has no QuestionDatabase.");
                if (manager.hud == null) problems.Add("School: GameManager has no HUD.");
                if (manager.quizUI == null) problems.Add("School: GameManager has no QuizUI.");
            }

            PlayerController player = Object.FindAnyObjectByType<PlayerController>();
            if (player == null) problems.Add("School: no player controller.");
            else
            {
                if (player.GetComponent<CharacterController>() == null) problems.Add("School: player has no CharacterController.");
                if (player.GetComponent<FlashlightBattery>() == null) problems.Add("School: player has no FlashlightBattery.");
                if (player.GetComponent<BrightIdeaMeter>() == null) problems.Add("School: player has no BrightIdeaMeter.");
                if (player.GetComponent<InteractionRaycaster>() == null) problems.Add("School: player has no InteractionRaycaster.");
                if (player.GetComponentInChildren<Camera>() == null) problems.Add("School: player has no camera.");
                if (player.GetComponentInChildren<Light>() == null) problems.Add("School: player has no flashlight light.");

                // P3 horror systems ride the player rig.
                if (player.GetComponent<NoiseEmitter>() == null) problems.Add("School: player has no NoiseEmitter.");
                if (player.GetComponent<FlashlightCrank>() == null) problems.Add("School: player has no FlashlightCrank.");
                if (player.GetComponent<HidingSystem>() == null) problems.Add("School: player has no HidingSystem.");
                if (player.GetComponentInChildren<CalmEffectsController>() == null)
                {
                    problems.Add("School: player camera has no CalmEffectsController.");
                }

                // Premium pass §8.3: the occlusion probe rides the camera next
                // to the AudioListener.
                if (player.GetComponentInChildren<LightShadow.Audio.AudioOcclusionProbe>() == null)
                {
                    problems.Add("School: player camera has no AudioOcclusionProbe.");
                }
            }

            EnemyBehaviour shadow = Object.FindAnyObjectByType<EnemyBehaviour>();
            if (shadow == null) problems.Add("School: no Shadow.");
            else
            {
                if (shadow.route == null || shadow.route.Count < 4) problems.Add("School: Shadow patrol route is missing or too short.");
                if (shadow.sight == null) problems.Add("School: Shadow has no EnemySight.");
            }

            HUDController hud = Object.FindAnyObjectByType<HUDController>();
            if (hud == null) problems.Add("School: no HUDController.");
            else
            {
                if (hud.meterFill == null) problems.Add("School: HUD meter fill missing.");
                if (hud.batteryFill == null) problems.Add("School: HUD battery fill missing.");
                if (hud.fadeImage == null) problems.Add("School: HUD fade image missing.");
            }

            QuizUIController quiz = Object.FindAnyObjectByType<QuizUIController>(FindObjectsInactive.Include);
            if (quiz == null) problems.Add("School: no QuizUIController.");
            else
            {
                if (quiz.panel == null) problems.Add("School: quiz panel missing.");
                if (quiz.optionButtons == null || quiz.optionButtons.Length != 4) problems.Add("School: quiz does not have four option buttons.");
            }

            int fuses = Object.FindObjectsByType<FuseBox>(FindObjectsInactive.Include).Length;
            int fragments = Object.FindObjectsByType<KnowledgeFragment>(FindObjectsInactive.Include).Length;
            int batteries = Object.FindObjectsByType<BatteryPickup>(FindObjectsInactive.Include).Length;
            int boards = Object.FindObjectsByType<QuizBoard>(FindObjectsInactive.Include).Length;
            int gates = Object.FindObjectsByType<ExitGate>(FindObjectsInactive.Include).Length;
            int zones = Object.FindObjectsByType<LitZone>(FindObjectsInactive.Include).Length;
            int hideSpots = Object.FindObjectsByType<HideSpot>(FindObjectsInactive.Include).Length;

            report.AppendLine("Fuse boxes: " + fuses + " (need 3)");
            report.AppendLine("Fragments: " + fragments + " (need at least " + GameManager.FragmentTarget + ")");
            report.AppendLine("Batteries: " + batteries + " (need at least 5)");
            report.AppendLine("Quiz boards: " + boards + " (need at least 6)");
            report.AppendLine("Lit zones: " + zones + " (need " + ExpectedLitZones + ")");
            report.AppendLine("Exit gates: " + gates + " (need 1)");
            report.AppendLine("Hide spots: " + hideSpots + " (need 6)");

            if (fuses != 3) problems.Add("School: expected 3 fuse boxes, found " + fuses + ".");
            // At least the target: the infirmary adds a seventh, spare fragment (#26).
            if (fragments < GameManager.FragmentTarget) problems.Add("School: expected at least " + GameManager.FragmentTarget + " fragments, found " + fragments + ".");
            if (batteries < 5) problems.Add("School: expected at least 5 batteries, found " + batteries + ".");
            if (boards < 6) problems.Add("School: expected at least 6 quiz boards, found " + boards + ".");

            int puzzleCount = Object.FindObjectsByType<PuzzleBase>(FindObjectsInactive.Include).Length;
            int medkitCount = Object.FindObjectsByType<MedkitPickup>(FindObjectsInactive.Include).Length;
            bool classroomSpare = false, staffSpare = false;
            foreach (ItemPickup pickup in Object.FindObjectsByType<ItemPickup>(FindObjectsInactive.Include))
            {
                if (pickup.itemId == "classroom_key") classroomSpare = true;
                if (pickup.itemId == "staff_key") staffSpare = true;
            }
            report.AppendLine("Puzzles: " + puzzleCount + " (need " + ExpectedPuzzles + "), medkits: " + medkitCount
                + ", spare keys: classroom " + classroomSpare + ", staff " + staffSpare);
            problems.AddRange(ContentPresenceProblems(zones, puzzleCount, medkitCount, classroomSpare, staffSpare));
            if (gates != 1) problems.Add("School: expected 1 exit gate, found " + gates + ".");
            if (hideSpots < 6) problems.Add("School: expected at least 6 hide spots, found " + hideSpots + ".");

            // Premium pass §8.3: every interior room gets a reverb zone with a
            // usable collider and falloff range (8 expected: Library, PhysicsLab,
            // Classroom, Garden, StaffRoom, Corridor, Entrance, MainHall).
            AudioReverbZone[] reverbZones = Object.FindObjectsByType<AudioReverbZone>(FindObjectsInactive.Include);
            report.AppendLine("Reverb zones: " + reverbZones.Length + " (need at least 6)");
            if (reverbZones.Length < 6)
            {
                problems.Add("School: expected at least 6 reverb zones, found " + reverbZones.Length + ".");
            }
            foreach (AudioReverbZone zone in reverbZones)
            {
                if (zone.GetComponent<Collider>() == null)
                {
                    problems.Add("School: reverb zone '" + zone.name + "' has no collider.");
                }
                if (zone.maxDistance <= zone.minDistance)
                {
                    problems.Add("School: reverb zone '" + zone.name + "' maxDistance must exceed minDistance.");
                }
            }

            // P3 menace director: the Shadow pacing gauge lives on the Systems.
            if (Object.FindAnyObjectByType<MenaceDirector>() == null)
            {
                problems.Add("School: Systems has no MenaceDirector.");
            }

            NavMeshSurface surface = Object.FindAnyObjectByType<NavMeshSurface>();
            if (surface == null) problems.Add("School: no NavMeshSurface.");
            else
            {
                if (surface.navMeshData == null) problems.Add("School: NavMeshSurface has no baked data (runtime fallback would run).");
                else report.AppendLine("NavMesh data: " + AssetDatabase.GetAssetPath(surface.navMeshData));
            }

            Volume volume = Object.FindAnyObjectByType<Volume>();
            if (volume == null) problems.Add("School: no global post-processing volume.");
            else if (volume.sharedProfile == null) problems.Add("School: post-processing volume has no profile.");

            DawnRiser riser = Object.FindAnyObjectByType<DawnRiser>(FindObjectsInactive.Include);
            if (riser == null) problems.Add("School: dawn chase has no DawnRiser.");

            // D15 follow-up: the snapshot driver must sit on the school Systems
            // with the mixer asset wired (direct-play support).
            LightShadow.Audio.AudioSnapshotController snapshotDriver =
                Object.FindAnyObjectByType<LightShadow.Audio.AudioSnapshotController>();
            if (snapshotDriver == null) problems.Add("School: Systems has no AudioSnapshotController.");
            else if (snapshotDriver.mixer == null) problems.Add("School: AudioSnapshotController has no mixer wired.");

            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            int vertices = triangulation.vertices != null ? triangulation.vertices.Length : 0;
            report.AppendLine("NavMesh vertices: " + vertices);
            if (vertices == 0) problems.Add("School: baked NavMesh has no vertices.");

            if (player != null && NavMesh.SamplePosition(player.transform.position, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            {
                report.AppendLine("Player spawn is on the NavMesh (" + hit.position + ").");
            }
            else
            {
                problems.Add("School: player spawn is not near the NavMesh.");
            }

            int missingScripts = 0;
            foreach (GameObject root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (Component component in root.GetComponentsInChildren<Component>(true))
                {
                    if (component == null) missingScripts++;
                }
            }

            report.AppendLine("Missing script components: " + missingScripts);
            if (missingScripts > 0) problems.Add("School: " + missingScripts + " missing script component(s).");

            // Validate the UI scene counts too.
            EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
            MainMenuController menu = Object.FindAnyObjectByType<MainMenuController>();
            if (menu == null) problems.Add("MainMenu: no MainMenuController.");
            else if (menu.playButton == null || menu.quitButton == null) problems.Add("MainMenu: buttons are not wired.");

            EditorSceneManager.OpenScene(EndingPath, OpenSceneMode.Single);
            EndingController ending = Object.FindAnyObjectByType<EndingController>();
            RecapScreen recap = Object.FindAnyObjectByType<RecapScreen>();
            if (ending == null) problems.Add("Ending: no EndingController.");
            if (recap == null) problems.Add("Ending: no RecapScreen.");
            else if (recap.recapText == null || recap.quitButton == null || recap.savePngButton == null)
            {
                problems.Add("Ending: recap is not wired (including the SAVE PNG button).");
            }
            if (ending != null)
            {
                if (ending.sun == null) problems.Add("Ending: the sun is not wired.");
                if (ending.skyRenderer == null) problems.Add("Ending: the sky is not wired.");
                if (ending.dawnLight == null) problems.Add("Ending: the dawn light is not wired.");
                if (ending.endingCamera == null) problems.Add("Ending: the ending camera is not wired.");
                if (ending.dawnGroup == null) problems.Add("Ending: the dawn group is not wired.");
            }

            if (Object.FindAnyObjectByType<Volume>() == null) problems.Add("Ending: no global post-processing volume.");

            EditorSceneManager.OpenScene(BootstrapPath, OpenSceneMode.Single);
            if (Object.FindAnyObjectByType<BootstrapLoader>() == null) problems.Add("Bootstrap: no BootstrapLoader.");

            // The Arabic translation table is a plain dictionary, so a duplicate
            // English key throws during static initialization and takes every
            // script that touches Localization down with it (shipped release bug
            // class). Detect it at build time instead.
            var duplicateKeys = new List<string>();
            string localizationSource = System.IO.File.ReadAllText(
                "Assets/_Project/Scripts/Core/Localization.cs");
            var keyMatches = System.Text.RegularExpressions.Regex.Matches(
                localizationSource, "\\{\\s*\"(?:[^\"\\\\]|\\\\.)*\",");
            var seenKeys = new HashSet<string>();
            foreach (System.Text.RegularExpressions.Match match in keyMatches)
            {
                string key = match.Value.Substring(match.Value.IndexOf('"') + 1,
                    match.Value.LastIndexOf('"') - match.Value.IndexOf('"') - 1);
                if (!seenKeys.Add(key)) duplicateKeys.Add(key);
            }

            report.AppendLine("Localization keys: " + seenKeys.Count
                + " unique / " + keyMatches.Count + " entries");
            if (duplicateKeys.Count > 0)
                problems.Add("Localization.cs has duplicate English keys: " + string.Join(", ", duplicateKeys));

            // ---- audio mixer (D15 follow-up, spec §7.1) -----------------------
            report.AppendLine();
            report.AppendLine(AudioMixerTool.ValidateMixer());
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(AudioMixerTool.MixerPath);
            if (mixer == null)
            {
                problems.Add("Missing audio mixer asset: " + AudioMixerTool.MixerPath + " (run AudioMixerTool.EnsureBatch).");
            }
            else
            {
                string[] categoryGroups = { "Master", "Music", "Ambience", "SFX", "Voice", "Shadow", "UI" };
                string[] duckGroups = { "MusicDuck", "AmbienceDuck", "SfxDuck", "VoiceDuck", "ShadowDuck", "UiDuck" };
                foreach (string name in categoryGroups)
                {
                    if (!HasGroupExact(mixer, name)) problems.Add("Mixer: missing group " + name + ".");
                }
                foreach (string name in duckGroups)
                {
                    if (!HasGroupExact(mixer, name)) problems.Add("Mixer: missing duck child " + name + ".");
                }
                string[] snapshotNames = LightShadow.Audio.AudioSnapshotController.SnapshotNames;
                foreach (string name in snapshotNames)
                {
                    if (mixer.FindSnapshot(name) == null) problems.Add("Mixer: missing snapshot " + name + ".");
                }
                AudioMixerSnapshot start = LightShadow.Tools.AudioMixerTool.ReadStartSnapshot(mixer);
                if (start == null || start.name != LightShadow.Audio.AudioSnapshotController.SnapshotExplore)
                {
                    problems.Add("Mixer: startSnapshot must be Snapshot_Explore.");
                }
                string[] exposed = AudioMixerTool.ExposedParameterNames();
                foreach (string parameter in exposed)
                {
                    if (!mixer.GetFloat(parameter, out _))
                    {
                        problems.Add("Mixer: exposed parameter missing: " + parameter + ".");
                    }
                }

            // ------------------------------------------------- P2 hero props
            // The 14 Blender FBX models must be imported, every registered
            // FBX material (the three Mat_Story_* slots included) must resolve
            // to its canonical asset, and the built scenes must carry the
            // hero swap instances. Swap counts are reported per model name.
            string[] heroNames = ArtImporter.HeroModelNames();
            int heroFbxCount = 0;
            foreach (string heroName in heroNames)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(ArtImporter.HeroModelsFolder + "/" + heroName + ".fbx") != null)
                {
                    heroFbxCount++;
                }
                else
                {
                    problems.Add("Hero FBX not imported (run ArtImporter.ImportAllBatch): " + heroName);
                }
            }
            report.AppendLine("Hero FBX assets: " + heroFbxCount + "/" + heroNames.Length);

            var heroSwapCounts = new SortedDictionary<string, int>();
            int heroNullMaterialSlots = 0;
            foreach (string scenePath in new[] { SchoolPath, EndingPath })
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null) continue;
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                foreach (GameObject rootObject in EditorSceneManager.GetActiveScene().GetRootGameObjects())
                {
                    foreach (Transform node in rootObject.GetComponentsInChildren<Transform>(true))
                    {
                        if (!node.name.StartsWith("ArtModel_", StringComparison.Ordinal)) continue;
                        foreach (Renderer renderer in node.GetComponentsInChildren<Renderer>(true))
                        {
                            foreach (Material material in renderer.sharedMaterials)
                            {
                                if (material == null) heroNullMaterialSlots++;
                            }
                        }
                        foreach (string heroName in heroNames)
                        {
                            if (!node.name.Contains(heroName)) continue;
                            heroSwapCounts.TryGetValue(heroName, out int swapCount);
                            heroSwapCounts[heroName] = swapCount + 1;
                            break;
                        }
                    }
                }
            }

            var swapSummary = new List<string>();
            foreach (KeyValuePair<string, int> pair in heroSwapCounts)
            {
                swapSummary.Add(pair.Key + "=" + pair.Value);
            }
            report.AppendLine("Hero swaps: " + (swapSummary.Count > 0 ? string.Join(", ", swapSummary) : "none"));
            if (heroNullMaterialSlots > 0)
            {
                problems.Add("Hero swap instances carry " + heroNullMaterialSlots + " null material slot(s).");
            }

            string[] heroMaterialNames = ArtImporter.HeroMaterialNames();
            var missingHeroMaterials = new List<string>();
            foreach (string materialName in heroMaterialNames)
            {
                if (AssetDatabase.LoadAssetAtPath<Material>(ArtMaterialsFolder + "/" + materialName + ".mat") == null)
                {
                    missingHeroMaterials.Add(materialName);
                }
            }
            report.AppendLine("Hero materials: " + (heroMaterialNames.Length - missingHeroMaterials.Count)
                + "/" + heroMaterialNames.Length
                + (missingHeroMaterials.Count > 0 ? " (missing: " + string.Join(", ", missingHeroMaterials) + ")" : ""));
            if (missingHeroMaterials.Count > 0)
            {
                problems.Add("Hero material assets missing, run ArtImporter first: "
                    + string.Join(", ", missingHeroMaterials));
            }

            // The three P2 story materials need their specific looks, not just
            // presence (covered by the map check above): emissive LED,
            // transparent glass.
            Material storyLed = AssetDatabase.LoadAssetAtPath<Material>(ArtMaterialsFolder + "/Mat_Story_LED.mat");
            if (storyLed != null && !storyLed.IsKeywordEnabled("_EMISSION"))
            {
                problems.Add("Mat_Story_LED has no emission enabled.");
            }
            Material storyGlass = AssetDatabase.LoadAssetAtPath<Material>(ArtMaterialsFolder + "/Mat_Story_Glass.mat");
            if (storyGlass != null && storyGlass.GetFloat("_Surface") != 1f)
            {
                problems.Add("Mat_Story_Glass is not set up as a transparent surface.");
            }

            }

            report.AppendLine();
            if (problems.Count == 0)
            {
                report.AppendLine("RESULT: PASS");
            }
            else
            {
                report.AppendLine("RESULT: FAIL");
                foreach (string problem in problems) report.AppendLine("- " + problem);
            }

            return report.ToString();
        }

        static bool HasGroupExact(AudioMixer mixer, string name)
        {
            AudioMixerGroup[] matches = mixer.FindMatchingGroups(name);
            for (int i = 0; i < matches.Length; i++)
            {
                if (matches[i] != null && matches[i].name == name) return true;
            }
            return false;
        }
    }
}
