using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightShadow.Core
{
    // 00_Bootstrap exists so every run begins from a known state, whatever scene
    // the editor was left on. It applies the project-wide runtime settings and
    // then hands control to the main menu.
    public class BootstrapLoader : MonoBehaviour
    {
        [Tooltip("Scene loaded immediately after the bootstrap frame.")]
        public string nextScene = SceneNames.MainMenu;

        void Awake()
        {
            // One consistent frame-rate target for desktop and mobile. vSync keeps
            // the desktop smooth; on phones targetFrameRate caps the battery cost.
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;
            Time.timeScale = 1f;
            RunResults.Reset();

            // Apply the saved language, quality, resolution, brightness and
            // sensitivity before the menu appears.
            GameSettings.LoadAndApply();

            // Premium pass: the quality-tier manager applies the D8 frame-rate
            // policy (Windows 120 FPS uncapped / Android display refresh) and
            // the auto-detected URP tier AFTER the settings card's fine controls,
            // so the tier provides the base and player tweaks stay on top.
            Systems.PerformanceManager.EnsureCreated();

            // P3: the dev console builds only when the LS_DebugMode flag is on
            // (default off - see GameSettings.DebugMode).
            LightShadow.DebugTools.DevConsole.EnsureCreated();
        }

        void Start()
        {
            if (string.IsNullOrEmpty(nextScene)) nextScene = SceneNames.MainMenu;
            SceneManager.LoadScene(nextScene);
        }
    }
}
