using LightShadow.Audio;
using LightShadow.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // 01_MainMenu: title, the way into the school, a short controls card, and quit.
    // The menu stays quiet until the audio library ships a MUS_Menu loop: the
    // hook below plays it when the id exists and keeps the deliberate silence
    // when it does not, so the first step into the dark school still does the
    // work either way.
    //
    // Premium pass (additive): when saves exist, ENTER THE SCHOOL opens the save
    // slot panel (D12) instead of starting blind; with no saves it starts a new
    // night straight away. Scene changes run through LoadingScreen. A SAVES
    // button (wired by SceneBuilderStory, optional) opens the panel directly.
    public class MainMenuController : MonoBehaviour
    {
        [Header("Wiring")]
        public Button playButton;
        public Button howToButton;
        public Button settingsButton;
        public Button closeHowToButton;
        public Button quitButton;
        public Button creditsButton;
        public Button closeCreditsButton;
        public GameObject creditsPanel;
        public GameObject howToPanel;
        public SettingsPanelController settings;

        [Header("Story pass (built by SceneBuilderStory)")]
        public SaveSlotUI saveSlots;
        public Button savesButton;

        void Awake()
        {
            if (playButton != null) playButton.onClick.AddListener(Play);
            if (howToButton != null) howToButton.onClick.AddListener(ToggleHowTo);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (closeHowToButton != null) closeHowToButton.onClick.AddListener(ToggleHowTo);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
            if (creditsButton != null) creditsButton.onClick.AddListener(ToggleCredits);
            if (closeCreditsButton != null) closeCreditsButton.onClick.AddListener(ToggleCredits);
            if (creditsPanel != null) creditsPanel.SetActive(false);
            if (savesButton != null) savesButton.onClick.AddListener(OpenSaves);
            if (howToPanel != null) howToPanel.SetActive(false);
        }

        void Start()
        {
            // User decision (2026-11-27): no background music, not even in the
            // menu. The MUS_Menu clip stays registered in the library for a
            // possible return; nothing loops.

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
                if (saveSlots != null && saveSlots.IsOpen) return;
                if (howToPanel != null && howToPanel.activeSelf) howToPanel.SetActive(false);
                else if (creditsPanel != null && creditsPanel.activeSelf) creditsPanel.SetActive(false);
                else Play();
                return;
            }

            // Esc closes the settings or controls card instead of leaving the menu.
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (settings != null && settings.IsOpen) settings.Hide();
                else if (saveSlots != null && saveSlots.IsOpen) saveSlots.Hide();
                else if (howToPanel != null && howToPanel.activeSelf) howToPanel.SetActive(false);
                else if (creditsPanel != null && creditsPanel.activeSelf) creditsPanel.SetActive(false);
            }
        }

        public void OpenSettings()
        {
            if (howToPanel != null) howToPanel.SetActive(false);
            if (saveSlots != null) saveSlots.Hide();
            if (settings != null) settings.Show();
        }

        public void OpenSaves()
        {
            if (howToPanel != null) howToPanel.SetActive(false);
            if (settings != null) settings.Hide();
            if (saveSlots != null) saveSlots.Show();
        }

        public void Play()
        {
            // With saves on disk the player chooses a slot first (D12); with a
            // clean slate the night starts immediately.
            if (saveSlots != null && SaveSystem.NewestSlot() >= 0)
            {
                OpenSaves();
                return;
            }

            StartNewNight();
        }

        // Shared by PLAY (no saves) and the slot panel's NEW GAME / LOAD.
        public void StartNewNight()
        {
            Time.timeScale = 1f;
            // New game must not inherit the previous run (audit 2.3): the story
            // director survives scene loads, and the hint log and a deferred
            // restore can still be pending after quit-to-menu.
            SaveSystem.ClearPendingRestore();
            LightShadow.Story.StoryDirector.ResetForNewRun();
            LightShadow.Contrib.Abdelraouf.HintLog.HintLogRecord.Clear();
            RunResults.Reset();
            LoadingScreen.Transition(SceneNames.School);
        }

        public void ToggleHowTo()
        {
            if (howToPanel == null) return;
            // Symmetric with ToggleCredits: the two cards never stack.
            if (creditsPanel != null) creditsPanel.SetActive(false);
            howToPanel.SetActive(!howToPanel.activeSelf);
        }

        public void ToggleCredits()
        {
            if (creditsPanel == null) return;
            if (howToPanel != null) howToPanel.SetActive(false);
            creditsPanel.SetActive(!creditsPanel.activeSelf);
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
