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
            // Reselect one frame after a confirm key was consumed, so the
            // same keypress can never reach the EventSystem's Submit too.
            if (reselect && Time.frameCount > consumedFrame)
            {
                reselect = false;
                if (saveSlots != null && saveSlots.IsOpen) saveSlots.SelectFirst();
                else if (!AnyCardOpen() && EventSystem.current != null && playButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(playButton.gameObject);
                }
            }

            // Enter / Space starts the night without hunting for the mouse.
            // Update is the single owner of the confirm key (U1).
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
            {
                ConfirmAction action = DecideConfirm(
                    settings != null && settings.IsOpen,
                    saveSlots != null && saveSlots.IsOpen,
                    howToPanel != null && howToPanel.activeSelf,
                    creditsPanel != null && creditsPanel.activeSelf);
                if (action == ConfirmAction.None) return;
                ConsumeConfirm();
                if (action == ConfirmAction.CloseHowTo) howToPanel.SetActive(false);
                else if (action == ConfirmAction.CloseCredits) creditsPanel.SetActive(false);
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

        public enum ConfirmAction { None, CloseHowTo, CloseCredits, Play }

        bool reselect;
        int consumedFrame = -1;
        bool starting;

        // Pure decision for the confirm key. Settings and the slot panel own
        // their own keys; an open card closes first; otherwise PLAY.
        public static ConfirmAction DecideConfirm(bool settingsOpen, bool savesOpen, bool howToOpen, bool creditsOpen)
        {
            if (settingsOpen || savesOpen) return ConfirmAction.None;
            if (howToOpen) return ConfirmAction.CloseHowTo;
            if (creditsOpen) return ConfirmAction.CloseCredits;
            return ConfirmAction.Play;
        }

        bool AnyCardOpen()
        {
            return (howToPanel != null && howToPanel.activeSelf) || (creditsPanel != null && creditsPanel.activeSelf)
                || (settings != null && settings.IsOpen) || (saveSlots != null && saveSlots.IsOpen);
        }

        // Drop the selection for the rest of this frame (the EventSystem
        // would otherwise Submit the same keypress on the selected button) and
        // put it back next frame.
        void ConsumeConfirm()
        {
            consumedFrame = Time.frameCount;
            reselect = true;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        void ClearSelectionForCard()
        {
            reselect = true;
            consumedFrame = Time.frameCount;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        public void OpenSettings()
        {
            if (howToPanel != null) howToPanel.SetActive(false);
            if (saveSlots != null) saveSlots.Hide();
            if (settings != null) settings.Show();
            ClearSelectionForCard();
        }

        public void OpenSaves()
        {
            if (howToPanel != null) howToPanel.SetActive(false);
            if (settings != null) settings.Hide();
            if (saveSlots != null) saveSlots.Show();
            ClearSelectionForCard();
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
            if (starting) return; // one keypress or click starts the night once
            starting = true;
            Time.timeScale = 1f;
            // New game must not inherit the previous run (audit 2.3): the story
            // director survives scene loads, and the hint log and a deferred
            // restore can still be pending after quit-to-menu.
            SaveSystem.ClearPendingRestore();
            LightShadow.Story.StoryDirector.ResetForNewRun();
            LightShadow.Contrib.Abdelraouf.HintLog.HintLogRecord.Clear();
            RunResults.Reset();
            Inventory.ResetForNewRun();
            LightShadow.Player.HidingSystem.ResetState();
            LightShadow.Contrib.Abdelraouf.Journal.JournalProgress.ResetForNewRun();
            LoadingScreen.Transition(SceneNames.School);
        }

        public void ToggleHowTo()
        {
            if (howToPanel == null) return;
            // Symmetric with ToggleCredits: the two cards never stack.
            if (creditsPanel != null) creditsPanel.SetActive(false);
            howToPanel.SetActive(!howToPanel.activeSelf);
            ClearSelectionForCard();
        }

        public void ToggleCredits()
        {
            if (creditsPanel == null) return;
            if (howToPanel != null) howToPanel.SetActive(false);
            creditsPanel.SetActive(!creditsPanel.activeSelf);
            ClearSelectionForCard();
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
