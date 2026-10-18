using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // Esc key or touch button: freeze the world, free the cursor,
    // offer Resume and Quit. Quiz overlays swallow Esc so answers are not lost.
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu Instance { get; private set; }

        [Header("Wiring")]
        public GameObject panel;
        public Button resumeButton;
        public Button quitButton;

        public bool IsOpen { get; private set; }

        void Awake()
        {
            Instance = this;
            if (panel != null) panel.SetActive(false);
            if (resumeButton != null) resumeButton.onClick.AddListener(Close);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            // While a question is up, Esc does nothing - let the timer decide.
            Core.GameManager gm = Core.GameManager.Instance;
            if (!IsOpen && gm != null && gm.quizUI != null && gm.quizUI.IsOpen) return;

            Toggle();
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public void Open()
        {
            IsOpen = true;
            if (panel != null) panel.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Close()
        {
            IsOpen = false;
            if (panel != null) panel.SetActive(false);
            Time.timeScale = 1f;

            if (!Application.isMobilePlatform)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
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
