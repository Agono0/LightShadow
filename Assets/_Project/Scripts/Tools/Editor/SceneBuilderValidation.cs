using System;
using System.Collections.Generic;
using System.Text;
using LightShadow.Core;
using LightShadow.Enemy;
using LightShadow.Gameplay;
using LightShadow.Player;
using LightShadow.UI;
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
        static string ValidateScenes()
        {
            var report = new StringBuilder();
            var problems = new List<string>();

            if (EditorBuildSettings.scenes.Length != 4)
            {
                problems.Add("Build settings should contain exactly 4 scenes, found " + EditorBuildSettings.scenes.Length + ".");
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
            // questions, every one draft-marked for the human fact-check.
            QuestionDatabase database = AssetDatabase.LoadAssetAtPath<QuestionDatabase>(DatabasePath);
            if (database == null)
            {
                problems.Add("Missing QuestionDatabase asset.");
            }
            else
            {
                int valid = 0;
                var categories = new HashSet<QuestionCategory>();
                foreach (QuestionData question in database.all)
                {
                    if (question == null || !question.IsValid) continue;
                    valid++;
                    categories.Add(question.category);
                    if (string.IsNullOrEmpty(question.factSource)) problems.Add("Question " + question.name + " has no fact source.");
                }

                report.AppendLine("Question bank: " + valid + " valid / " + database.all.Count + " linked");
                report.AppendLine("Question categories: " + categories.Count + "/6");
                if (valid < 60) problems.Add("Question bank has " + valid + " valid questions; the project document asks for 60+.");
                if (categories.Count < 6) problems.Add("Question bank is missing one or more categories.");
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

            report.AppendLine("Fuse boxes: " + fuses + " (need 3)");
            report.AppendLine("Fragments: " + fragments + " (need 6)");
            report.AppendLine("Batteries: " + batteries + " (need at least 5)");
            report.AppendLine("Quiz boards: " + boards + " (need at least 6)");
            report.AppendLine("Lit zones: " + zones + " (need 5)");
            report.AppendLine("Exit gates: " + gates + " (need 1)");

            if (fuses != 3) problems.Add("School: expected 3 fuse boxes, found " + fuses + ".");
            if (fragments != 6) problems.Add("School: expected 6 fragments, found " + fragments + ".");
            if (batteries < 5) problems.Add("School: expected at least 5 batteries, found " + batteries + ".");
            if (boards < 6) problems.Add("School: expected at least 6 quiz boards, found " + boards + ".");
            if (zones != 5) problems.Add("School: expected 5 lit zones, found " + zones + ".");
            if (gates != 1) problems.Add("School: expected 1 exit gate, found " + gates + ".");

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
            else if (recap.recapText == null || recap.quitButton == null) problems.Add("Ending: recap is not wired.");
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
    }
}
