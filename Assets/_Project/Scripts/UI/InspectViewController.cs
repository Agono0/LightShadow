using LightShadow.Contrib.Abdelraouf.Overlay;
using LightShadow.Core;
using LightShadow.Gameplay;
using LightShadow.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // P3 inspect view: a fullscreen close-up for story items. When a
    // KnowledgeFragment is picked up the fragment's mesh spawns in front of
    // the camera on a dark backdrop, slowly turning; drag/swipe orbits it.
    // ESC or the close button (auto-close after a while) hands back to the
    // memory/quiz flow, movement input stays locked until then.
    //
    // The whole overlay self-builds in code (backdrop, title, body, close
    // button), so it works with zero scene wiring - KnowledgeFragment calls
    // the static ShowFrom and this creates itself if missing.
    public class InspectViewController : MonoBehaviour, IDragHandler
    {
        public static InspectViewController Instance { get; private set; }

        public static bool IsOpen => Instance != null && Instance.root != null && Instance.root.activeSelf;

        [Header("Wiring (auto-built when empty)")]
        public Canvas canvas;
        public GameObject root;
        public Text titleText;
        public Text inspectText;
        public Button closeButton;
        public Image backdrop;

        GameConfig config;
        GameObject itemInstance;
        Transform itemAnchor;
        float autoCloseClock;

        public static void ShowFrom(KnowledgeFragment fragment)
        {
            if (fragment == null) return;

            Mesh mesh = null;
            Material material = null;
            MeshFilter filter = fragment.GetComponentInChildren<MeshFilter>();
            if (filter != null) mesh = filter.sharedMesh;
            MeshRenderer renderer = fragment.GetComponentInChildren<MeshRenderer>();
            if (renderer != null) material = renderer.sharedMaterial;

            Show(fragment.fragmentName, fragment.fragmentText, mesh, material);
        }

        public static void Show(string title, string text, Mesh mesh, Material material)
        {
            if (Instance == null)
            {
                Instance = new GameObject("InspectViewController").AddComponent<InspectViewController>();
            }
            Instance.ShowInternal(title, text, mesh, material);
        }

        InputLockToken lockToken;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnDestroy()
        {
            // Never leave the player locked behind a destroyed overlay.
            OverlayStack.Release(ref lockToken);
            if (Instance == this) Instance = null;
        }

        // Config shortcuts with safe fallbacks (same pattern as EnemyBehaviour).
        GameConfig Config => config != null
            ? config
            : (config = GameManager.Instance != null ? GameManager.Instance.config : null);
        // Audit H2: the item is a child of the main camera but this canvas is
        // ScreenSpaceOverlay, so it draws over the item. The 0.94 backdrop hid
        // it almost completely. Moving to ScreenSpaceCamera was rejected at
        // source level: the plane would be depth-tested against world
        // geometry (walls near the camera could cut the UI) and the item has
        // to sit nearer than the plane to show in front of the backdrop.
        // Bounded fix: a lighter backdrop so the item reads through it.
        public const float BackdropAlpha = 0.5f;
        public const float MinVisibleItemShare = 0.4f; // 1 - alpha must stay above this

        public static float ItemVisibleShare(float backdropAlpha)
        {
            return 1f - Mathf.Clamp01(backdropAlpha);
        }

        float ItemDistance => Config != null ? Config.inspectItemDistance : 0.55f;
        float ItemScale => Config != null ? Config.inspectItemScale : 1.2f;
        float SpinSpeed => Config != null ? Config.inspectSpinSpeed : 20f;
        float DragDegreesPerPixel => Config != null ? Config.inspectDragDegreesPerPixel : 0.4f;
        float AutoSeconds => Config != null ? Config.inspectAutoSeconds : 12f;

        // ------------------------------------------------------------ show --

        void ShowInternal(string title, string text, Mesh mesh, Material material)
        {
            EnsureBuilt();

            // The item rides the camera so it stays framed while the world
            // keeps moving behind the backdrop.
            if (itemInstance != null) Destroy(itemInstance);
            if (itemAnchor != null) Destroy(itemAnchor.gameObject);
            itemInstance = null;
            itemAnchor = null;

            Camera stageCamera = Camera.main;
            if (stageCamera != null)
            {
                itemAnchor = new GameObject("InspectItemStage").transform;
                itemAnchor.SetParent(stageCamera.transform, false);
                itemAnchor.localPosition = new Vector3(0f, 0f, ItemDistance);
                itemAnchor.localRotation = Quaternion.identity;

                if (mesh != null)
                {
                    itemInstance = new GameObject("InspectItem");
                    itemInstance.transform.SetParent(itemAnchor, false);
                    itemInstance.transform.localScale = Vector3.one * ItemScale;
                    itemInstance.transform.localPosition = Vector3.zero;
                    MeshFilter meshFilter = itemInstance.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh = mesh;
                    MeshRenderer meshRenderer = itemInstance.AddComponent<MeshRenderer>();
                    if (material != null) meshRenderer.sharedMaterial = material;
                }
            }

            Font font = Localization.CurrentFont;
            if (titleText != null)
            {
                titleText.text = Localization.TrPresent(title);
                titleText.font = font;
            }
            if (inspectText != null)
            {
                inspectText.text = Localization.TrPresent(text);
                inspectText.font = font;
            }

            // I1: the CLOSE label was cached at build time; refresh per open.
            Text closeLabel = closeButton != null ? closeButton.GetComponentInChildren<Text>() : null;
            if (closeLabel != null)
            {
                closeLabel.text = Localization.TrPresent("CLOSE");
                closeLabel.font = font;
            }

            autoCloseClock = AutoSeconds;
            root.SetActive(true);

            // Movement freezes while the item fills the screen (the Shadow does
            // not - the quiz convention holds: the world does not pause).
            if (lockToken == null || !lockToken.IsActive) lockToken = OverlayStack.Acquire("InspectView", true, true, Hide);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
            if (itemInstance != null) Destroy(itemInstance);
            if (itemAnchor != null) Destroy(itemAnchor.gameObject);
            itemInstance = null;
            itemAnchor = null;

            OverlayStack.Release(ref lockToken);
            if (!Application.isMobilePlatform && !PauseMenu.IsOpen && !OverlayStack.WantsFreeCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        void Update()
        {
            if (!IsOpen) return;

            autoCloseClock -= Time.unscaledDeltaTime;
            if (autoCloseClock <= 0f || Input.GetKeyDown(KeyCode.Escape))
            {
                Hide();
                return;
            }

            // Slow idle spin; drags add on top (OnDrag).
            if (itemInstance != null)
            {
                itemInstance.transform.Rotate(Vector3.up, SpinSpeed * Time.unscaledDeltaTime, Space.World);
            }
        }

        // The backdrop drag surface orbits the item (mouse and touch alike).
        public void OnDrag(PointerEventData eventData)
        {
            if (!IsOpen || itemInstance == null) return;
            itemInstance.transform.Rotate(Vector3.up, eventData.delta.x * DragDegreesPerPixel, Space.World);
            itemInstance.transform.Rotate(Vector3.right, -eventData.delta.y * DragDegreesPerPixel, Space.World);
        }

        // ------------------------------------------------- self-built UI ----

        void EnsureBuilt()
        {
            if (root != null) return;

            GameObject host = new GameObject("InspectViewCanvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            host.transform.SetParent(transform, false);
            canvas = host.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 400;   // above the HUD/quiz canvases

            CanvasScaler scaler = host.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            root = host;

            backdrop = MakeImage(root.transform, "Backdrop", Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero, new Color(0.01f, 0.01f, 0.02f, BackdropAlpha));
            backdrop.raycastTarget = true;   // the drag surface

            // Point anchors (anchorMin == anchorMax): the MakeText/MakeButton
            // offset arguments are CORNER offsets from the anchor, not a
            // position/size pair. CenteredOffsets turns the intended
            // centre-shift and size into those corner offsets; the pure math
            // is covered by Contrib/Ziad/Tests/Editor/InspectViewLayoutTests.
            titleText = MakeText(backdrop.transform, "Title", "", 44,
                new Color(0.95f, 0.89f, 0.77f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                CenteredOffsets(new Vector2(0f, -115f), new Vector2(1400f, 90f), out Vector2 titleMax), titleMax,
                FontStyle.Bold);

            inspectText = MakeText(backdrop.transform, "Body", "", 26,
                new Color(0.90f, 0.88f, 0.80f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f),
                CenteredOffsets(Vector2.zero, new Vector2(1100f, 260f), out Vector2 bodyMax), bodyMax,
                FontStyle.Italic);

            closeButton = MakeButton(backdrop.transform, "CloseButton",
                Localization.TrPresent("CLOSE"), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                CenteredOffsets(new Vector2(0f, -320f), new Vector2(260f, 64f), out Vector2 closeMax), closeMax);
            closeButton.onClick.AddListener(Hide);
        }

        // Corner offsets that centre a rect of `size` on its (equal min/max)
        // anchor, shifted by `shift`: min = shift - size/2, max = shift + size/2.
        // Pure; used only by EnsureBuilt above.
        public static Vector2 CenteredOffsets(Vector2 shift, Vector2 size, out Vector2 offsetMax)
        {
            offsetMax = shift + size * 0.5f;
            return shift - size * 0.5f;
        }

        Image MakeImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            Image image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        Text MakeText(Transform parent, string name, string content, int fontSize, Color color,
            TextAnchor anchor, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            FontStyle style)
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
            text.fontStyle = style;
            text.color = color;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        Button MakeButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            Image background = MakeImage(parent, name, anchorMin, anchorMax, offsetMin, offsetMax,
                new Color(0.16f, 0.15f, 0.14f, 0.98f));
            Text labelText = MakeText(background.transform, "Label", label, 24,
                new Color(0.95f, 0.89f, 0.77f), TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, FontStyle.Bold);
            labelText.fontSize = 24;
            Button button = background.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.36f, 0.25f, 0.13f, 1f);
            colors.pressedColor = new Color(0.24f, 0.18f, 0.1f, 1f);
            button.colors = colors;
            return button;
        }
    }
}
