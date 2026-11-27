using LightShadow.Contrib.Abdelraouf.Overlay;
using System.Collections.Generic;
using LightShadow.Core;
using LightShadow.Enemy;
using LightShadow.Player;
using LightShadow.UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace LightShadow.DebugTools
{
    // P3 dev console: backquote toggles a tiny command line (plus an on-screen
    // DEBUG button for phones). Builds ONLY when GameSettings.DebugMode is on
    // (off by default; BootstrapLoader calls EnsureCreated which no-ops it).
    //
    // Commands (lean string parsing, no reflection):
    //   setflag <name>     set a story flag (dialogue routing tests)
    //   givemeter <n>      add n to the Bright Idea Meter
    //   teleport <room>    entrance library physics classroom garden staff hall gate
    //   skipact <1-3>      restore the right fuse count / power the gate for act III
    //   toggleai           freeze/unfreeze the Shadow (behaviour + agent)
    //   timescale <x>      set Time.timeScale (0..5)
    //
    // While the console is open the player input is locked so typing WASD does
    // not move Nour; closing hands the lock back.
    public class DevConsole : MonoBehaviour
    {
        public static DevConsole Instance { get; private set; }

        const int MaxLines = 12;

        // Dev-only teleport table: fixed room anchors matching the built school
        // layout (SceneBuilderScenes). Not gameplay tuning - a debug shortcut.
        static readonly Dictionary<string, Vector3> Rooms = new Dictionary<string, Vector3>
        {
            { "entrance", new Vector3(0f, 0.05f, -29.5f) },
            { "library", new Vector3(-12f, 0.05f, -17f) },
            { "physics", new Vector3(12f, 0.05f, -15f) },
            { "classroom", new Vector3(-12f, 0.05f, 10f) },
            { "garden", new Vector3(12f, 0.05f, 10f) },
            { "staff", new Vector3(-12f, 0.05f, 23f) },
            { "hall", new Vector3(0f, 0.05f, 30f) },
            { "gate", new Vector3(0f, 0.05f, -31.5f) },
        };

        public Canvas canvas;
        public GameObject panel;
        public Text outputText;
        public InputField inputField;
        public Button debugButton;

        readonly List<string> lines = new List<string>();
        InputLockToken consoleLock;

        public static void EnsureCreated()
        {
            // Builds only when the debug flag is on (default off).
            if (!GameSettings.DebugMode) return;
            if (Instance != null) return;
            new GameObject("DevConsole").AddComponent<DevConsole>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildUi();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote)) TogglePanel();
        }

        public void TogglePanel()
        {
            bool opening = panel == null || !panel.activeSelf;
            if (panel != null) panel.SetActive(opening);

            if (opening)
            {
                if (consoleLock == null || !consoleLock.IsActive) consoleLock = OverlayStack.Acquire("DevConsole", true, false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (inputField != null)
                {
                    inputField.text = "";
                    inputField.ActivateInputField();
                }
                RenderOutput();
            }
            else
            {
                OverlayStack.Release(ref consoleLock);
                if (!Application.isMobilePlatform && !PauseMenu.IsOpen)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        // -------------------------------------------------------- commands --

        void RunCommand(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return;
            string[] parts = raw.Trim().Split(' ');
            string verb = parts[0].ToLowerInvariant();

            switch (verb)
            {
                case "setflag":
                    if (parts.Length < 2) { Log("usage: setflag <name>"); break; }
                    if (Story.StoryDirector.Instance != null)
                    {
                        Story.StoryDirector.Instance.SetFlag(parts[1]);
                        Log("flag set: " + parts[1]);
                    }
                    else Log("no StoryDirector");
                    break;

                case "givemeter":
                    if (parts.Length < 2 || !float.TryParse(parts[1], out float amount))
                    {
                        Log("usage: givemeter <n>");
                        break;
                    }
                    BrightIdeaMeter meter = GameManager.Instance != null ? GameManager.Instance.GetMeter() : null;
                    if (meter != null)
                    {
                        meter.SetMeter(meter.Meter + amount);
                        Log("meter -> " + Mathf.RoundToInt(meter.Meter));
                    }
                    else Log("no meter");
                    break;

                case "teleport":
                    if (parts.Length < 2 || !Rooms.TryGetValue(parts[1].ToLowerInvariant(), out Vector3 room))
                    {
                        Log("rooms: " + string.Join(" ", new List<string>(Rooms.Keys).ToArray()));
                        break;
                    }
                    TeleportPlayer(room, parts[1]);
                    break;

                case "skipact":
                    if (parts.Length < 2 || !int.TryParse(parts[1], out int act))
                    {
                        Log("usage: skipact <1-3>");
                        break;
                    }
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.DevSkipToAct(act);
                        Log("skipped to act " + act);
                    }
                    break;

                case "toggleai":
                    ToggleEnemyAi();
                    break;

                case "timescale":
                    if (parts.Length < 2 || !float.TryParse(parts[1], out float scale))
                    {
                        Log("usage: timescale <x>");
                        break;
                    }
                    Time.timeScale = Mathf.Clamp(scale, 0f, 5f);
                    Log("timescale -> " + Time.timeScale);
                    break;

                default:
                    Log("unknown: " + verb + "  (setflag givemeter teleport skipact toggleai timescale)");
                    break;
            }
        }

        void TeleportPlayer(Vector3 position, string roomName)
        {
            GameManager game = GameManager.Instance;
            if (game == null || game.player == null) { Log("no player"); return; }

            // Same hop as the checkpoint wake: switch the capsule off for the move.
            CharacterController controller = game.player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            game.player.SetPositionAndRotation(position, Quaternion.identity);
            if (controller != null) controller.enabled = true;
            Log("teleported: " + roomName);
        }

        void ToggleEnemyAi()
        {
            GameManager game = GameManager.Instance;
            EnemyBehaviour enemy = game != null ? game.enemy : null;
            if (enemy == null) { Log("no enemy"); return; }

            bool enable = !enemy.enabled;
            enemy.enabled = enable;
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null) agent.enabled = enable;
            Log("shadow ai " + (enable ? "ON" : "OFF"));
        }

        void Log(string line)
        {
            lines.Add(line);
            if (lines.Count > MaxLines) lines.RemoveAt(0);
            RenderOutput();
        }

        void RenderOutput()
        {
            if (outputText == null) return;
            outputText.text = "> " + string.Join("\n> ", lines.ToArray());
        }

        // ------------------------------------------------------- built UI --

        // Minimal runtime UI: DEBUG chip top-right, panel with output + input.
        void BuildUi()
        {
            GameObject host = new GameObject("DevConsoleCanvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            host.transform.SetParent(transform, false);
            canvas = host.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900;   // above everything, dev only

            CanvasScaler scaler = host.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            debugButton = MakeButton(host.transform, "DebugChip", "DEBUG",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-90f, -64f), new Vector2(90f, 64f),
                () => TogglePanel());

            panel = MakePanel(host.transform, "Panel", new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -420f), new Vector2(0f, 0f));
            panel.SetActive(false);

            outputText = MakeText(panel.transform, "Output", "", 20,
                new Color(0.85f, 0.95f, 0.85f), TextAnchor.UpperLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -16f), new Vector2(-16f, -290f));

            inputField = MakeInput(panel.transform, "Input");
            inputField.onSubmit.AddListener(command =>
            {
                RunCommand(command);
                inputField.text = "";
                inputField.ActivateInputField();
            });
        }

        GameObject MakePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            go.GetComponent<Image>().color = new Color(0f, 0.04f, 0.02f, 0.92f);
            return go;
        }

        Text MakeText(Transform parent, string name, string content, int fontSize, Color color,
            TextAnchor anchor, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            Text text = go.AddComponent<Text>();
            text.text = content;
            text.font = Localization.CurrentFont;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        Button MakeButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, System.Action onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            go.GetComponent<Image>().color = new Color(0.1f, 0.2f, 0.1f, 0.9f);

            Text text = MakeText(go.transform, "Label", label, 22,
                new Color(0.9f, 0.98f, 0.9f), TextAnchor.MiddleCenter, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);
            text.fontSize = 22;

            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(() => onClick());
            return button;
        }

        InputField MakeInput(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(16f, -356f);
            rect.offsetMax = new Vector2(-16f, -300f);
            go.GetComponent<Image>().color = new Color(0.05f, 0.1f, 0.05f, 0.95f);

            Text placeholder = MakeText(go.transform, "Placeholder", "command (setflag givemeter teleport skipact toggleai timescale)",
                20, new Color(0.5f, 0.6f, 0.5f, 0.8f), TextAnchor.MiddleLeft, Vector2.zero, Vector2.one,
                new Vector2(10f, 0f), new Vector2(-10f, 0f));
            Text editText = MakeText(go.transform, "Text", "", 20,
                new Color(0.92f, 0.98f, 0.92f), TextAnchor.MiddleLeft, Vector2.zero, Vector2.one,
                new Vector2(10f, 0f), new Vector2(-10f, 0f));

            InputField field = go.AddComponent<InputField>();
            field.targetGraphic = go.GetComponent<Image>();
            field.textComponent = editText;
            field.placeholder = placeholder;
            return field;
        }
    }
}
