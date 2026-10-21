using LightShadow.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // 01_MainMenu: title, the way into the school, a short controls card, and quit.
    // The menu is deliberately quiet - one warm light, no music - so the first
    // step into the dark school does the work.
    public class MainMenuController : MonoBehaviour
    {
        [Header("Wiring")]
        public Button playButton;
        public Button howToButton;
        public Button settingsButton;
        public Button closeHowToButton;
        public Button quitButton;
        public GameObject howToPanel;
        public SettingsPanelController settings;

        void Awake()
        {
            if (playButton != null) playButton.onClick.AddListener(Play);
            if (howToButton != null) howToButton.onClick.AddListener(ToggleHowTo);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (closeHowToButton != null) closeHowToButton.onClick.AddListener(ToggleHowTo);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
            if (howToPanel != null) howToPanel.SetActive(false);
        }

        void Start()
        {
            if (EventSystem.current != null && playButton != null)
            {
                EventSystem.current.SetSelectedGameObject(playButton.gameObject);
            }
        }

        void Update()
        {
            // Enter / Space starts the night without hunting for the mouse.
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                if (settings != null && settings.IsOpen) return;
                if (howToPanel != null && howToPanel.activeSelf) howToPanel.SetActive(false);
                else Play();
                return;
            }

            // Esc closes the settings or controls card instead of leaving the menu.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (settings != null && settings.IsOpen) settings.Hide();
                else if (howToPanel != null && howToPanel.activeSelf) howToPanel.SetActive(false);
            }
        }

        public void OpenSettings()
        {
            if (howToPanel != null) howToPanel.SetActive(false);
            if (settings != null) settings.Show();
        }

        public void Play()
        {
            Time.timeScale = 1f;
            RunResults.Reset();
            SceneManager.LoadScene(SceneNames.School);
        }

        public void ToggleHowTo()
        {
            if (howToPanel == null) return;
            howToPanel.SetActive(!howToPanel.activeSelf);
        }

        public void QuitGame()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
