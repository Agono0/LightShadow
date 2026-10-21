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
        }

        void Start()
        {
            if (string.IsNullOrEmpty(nextScene)) nextScene = SceneNames.MainMenu;
            SceneManager.LoadScene(nextScene);
        }
    }
}
