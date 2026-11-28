using LightShadow.Contrib.Abdelraouf.Overlay;
using LightShadow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // Esc key or touch button: freeze the world, free the cursor,
    // offer Resume, Main menu and Quit. Quiz overlays swallow Esc so answers are not lost.
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu Instance { get; private set; }

        [Header("Wiring")]
        public GameObject panel;
        public Button resumeButton;
        public Button settingsButton;
        public Button quitButton;

        // Optional. When left empty (the scene is not edited here) a copy of the
        // Quit button is built at runtime, placed under it.
        public Button menuButton;
        public SettingsPanelController settings;

        // Static: the player controller's cursor guard reads it without a
        // reference, and a single pause menu exists per scene.
        public static bool IsOpen { get; private set; }

        InputLockToken pauseLock;
        Text menuLabel;

        void Awake()
        {
            Instance = this;
            if (panel != null) panel.SetActive(false);
            if (resumeButton != null) resumeButton.onClick.AddListener(Close);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
            EnsureMenuButton();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            OverlayStack.Release(ref pauseLock);
            // Destroyed while paused (scene change to the ending): never leave
            // the game frozen with a free cursor.
            if (IsOpen) Time.timeScale = 1f;
            IsOpen = false;
        }

        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            // P3 inspect view consumes Esc first (close the item close-up).
            if (InspectViewController.IsOpen) return;

            // A dismissable overlay (puzzle, journal, inventory, hint log)
            // owns this Esc. If one already closed itself earlier this frame
            // (ClosedThisFrame) the press is spent. Otherwise close the top
            // one through OverlayStack.TryCloseTop: the screens that never
            // read Esc themselves (inventory, hint log) depend on this call,
            // and the ones that do (puzzles, journal, inspect) guard their
            // own Esc handlers on IsOpen, so their second close in the same
            // frame is a no-op. Either way pause stays shut: opening it on
            // the same press would stack it on the closing overlay.
            // Story dialogue is not dismissable and does not block pause.
            if (!IsOpen && ShouldYieldEscape(OverlayStack.AnyDismissable, OverlayStack.ClosedThisFrame))
            {
                if (!OverlayStack.ClosedThisFrame) OverlayStack.TryCloseTop();
                return;
            }

            // Settings sits above the pause card and consumes Esc first.
            if (settings != null && settings.IsOpen)
            {
                settings.Hide();
                return;
            }

            // While a question is up, Esc does nothing - let the timer decide.
            Core.GameManager gm = Core.GameManager.Instance;
            if (ShouldBlockForQuiz(IsOpen, gm != null && gm.quizUI != null && gm.quizUI.IsOpen)) return;

            Toggle();
        }

        // Pure rule, kept separate so EditMode tests can pin it.
        public static bool ShouldYieldEscape(bool anyDismissableOverlay, bool overlayClosedThisFrame)
        {
            return anyDismissableOverlay || overlayClosedThisFrame;
        }

        // U2: a live question blocks opening pause (timeScale 0 would freeze
        // its timer while the answer keys still work). Closing is never blocked.
        public static bool ShouldBlockForQuiz(bool pauseOpen, bool quizOpen)
        {
            return !pauseOpen && quizOpen;
        }

        // Pause may re-lock the cursor only when no overlay still needs it.
        public static bool ShouldRelockCursor(bool overlayWantsFreeCursor, bool mobile)
        {
            return !mobile && !overlayWantsFreeCursor;
        }

        // The ONE dispatch for the non-Esc pause inputs (gamepad Start, touch
        // pause button). The newest dismissable overlay eats the press and
        // closes first, exactly like Esc in Update; pause toggles only when
        // there was nothing to close, so it never stacks on an open overlay.
        public static void RequestToggle()
        {
            if (Instance == null) return;
            // Settings sits above the pause card: Start or the pause button
            // closes it first, like Esc does (never resume behind the card).
            if (Instance.settings != null && Instance.settings.IsOpen)
            {
                Instance.settings.Hide();
                return;
            }
            if (!IsOpen && OverlayStack.TryCloseTop()) return;
            Core.GameManager gm = Core.GameManager.Instance;
            if (ShouldBlockForQuiz(IsOpen, gm != null && gm.quizUI != null && gm.quizUI.IsOpen)) return;
            Instance.Toggle();
        }

        void OpenSettings()
        {
            if (settings != null) settings.Show();
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public void Open()
        {
            IsOpen = true;
            if (pauseLock == null || !pauseLock.IsActive) pauseLock = OverlayStack.Acquire("Pause", true, false);
            if (panel != null) panel.SetActive(true);
            RefreshMenuLabel();
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Close()
        {
            IsOpen = false;
            OverlayStack.Release(ref pauseLock);
            if (panel != null) panel.SetActive(false);
            Time.timeScale = 1f;

            if (ShouldRelockCursor(OverlayStack.WantsFreeCursor, Application.isMobilePlatform))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        // ---- back to the main menu -------------------------------------------

        // Only a slot that already holds a save is written: SaveSystem has no
        // "no slot chosen" value (CurrentSlot defaults to 0), so a run started
        // with PLAY and no slot would otherwise create slot 0 here. Checkpoint
        // autosaves already do that during play, in which case slot 0 holds a
        // save and this writes to it like any checkpoint. Autosave itself does
        // nothing outside the school scene.
        public static bool ShouldAutosaveToSlot(int slot, bool slotHasSave)
        {
            return slot >= 0 && slot < SaveSystem.SlotCount && slotHasSave;
        }

        void GoToMainMenu()
        {
            int slot = SaveSystem.CurrentSlot;
            if (ShouldAutosaveToSlot(slot, SaveSystem.HasSave(slot))) SaveSystem.Autosave();

            IsOpen = false;
            OverlayStack.Release(ref pauseLock);
            if (panel != null) panel.SetActive(false);
            Time.timeScale = 1f;
            OverlayStack.ResetAll(true);
            LoadingScreen.Transition(SceneNames.MainMenu);
        }

        void EnsureMenuButton()
        {
            if (menuButton == null && quitButton != null)
            {
                Transform parent = quitButton.transform.parent;
                Button copy = Instantiate(quitButton, parent);
                copy.name = "MainMenuButton";
                copy.onClick = new Button.ButtonClickedEvent();
                RectTransform rect = (RectTransform)copy.transform;
                // Under a layout group the copy places itself; otherwise offset it.
                if (parent == null || parent.GetComponent<LayoutGroup>() == null)
                {
                    float step = rect.rect.height + 4f;
                    rect.anchoredPosition = rect.anchoredPosition + new Vector2(0f, -step);
                }
                else copy.transform.SetSiblingIndex(quitButton.transform.GetSiblingIndex());
                menuButton = copy;
            }

            if (menuButton != null)
            {
                menuButton.onClick.RemoveListener(GoToMainMenu);
                menuButton.onClick.AddListener(GoToMainMenu);
                menuLabel = menuButton.GetComponentInChildren<Text>(true);
                RefreshMenuLabel();
            }
        }

        // The copy carries the Quit button's LocalizedText, which owns the label
        // and re-applies it on language change, so the new text goes through it.
        void RefreshMenuLabel()
        {
            if (menuLabel == null) return;
            LocalizedText localized = menuLabel.GetComponent<LocalizedText>();
            if (localized != null) localized.SetKey("MAIN MENU", "القائمة الرئيسية");
            else menuLabel.text = Localization.Current == GameLanguage.Arabic
                ? Localization.Present("القائمة الرئيسية")
                : "MAIN MENU";
        }

        void QuitGame()
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
