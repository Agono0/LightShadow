using LightShadow.Contrib.Abdelraouf.Hints;
using LightShadow.Core;
using LightShadow.Player;
using LightShadow.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Contrib.Abdelraouf.HintLog
{
    // Mobile RequestHint button. A self-built overlay button that calls
    // HintRequest.TryRequest() (which calls HintService.RequestHint()).
    //
    // Why not a TouchButton.Kind: TouchButton's Kind enum is a shared file with
    // no hint entry, and adding one is a shared edit outside this lane. This
    // component is the replacement; the owner's side can instead add a Kind
    // that calls HintRequest.TryRequest() and drop this one.
    //
    // Visibility: shown only on mobile platforms by default (showOnDesktop is
    // for editor checks), and hidden while the input lock, pause menu or a
    // quiz is up, while hints are switched off, or while there is no
    // HintService in the scene. Position: bottom right by default; the owner's
    // side adjusts anchoredPosition / size to fit the mobile HUD (they own
    // that layout). Needs an EventSystem in the scene, as TouchButton does.
    public class HintTouchButton : MonoBehaviour
    {
        public static HintTouchButton Instance { get; private set; }

        public bool showOnDesktop;
        public Vector2 anchoredPosition = new Vector2(-40f, 260f);
        public Vector2 size = new Vector2(110f, 110f);

        CanvasGroup group;
        Text label;

        public static HintTouchButton EnsureExists()
        {
            if (Instance != null) return Instance;
            var host = new GameObject("HintTouchButton");
            return host.AddComponent<HintTouchButton>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            BuildUi();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            bool visible = (Application.isMobilePlatform || showOnDesktop) && ShouldShow();
            group.alpha = visible ? 1f : 0f;
            group.blocksRaycasts = visible;
            group.interactable = visible;
            if (visible) label.text = Localization.Current == GameLanguage.Arabic ? Localization.Present("تلميح") : "Hint";
        }

        static bool ShouldShow()
        {
            if (HintService.Instance == null || !HintService.Enabled) return false;
            if (PlayerController.ExternalInputLocked || PauseMenu.IsOpen || HintLogUI.IsOpen) return false;
            GameManager gm = GameManager.Instance;
            if (gm == null) return false;
            if (gm.quizUI != null && gm.quizUI.IsOpen) return false;
            return true;
        }

        void OnPressed()
        {
            HintRequest.TryRequest();
        }

        void BuildUi()
        {
            var canvasObject = new GameObject("HintTouchCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 72;   // above the hint banner (70); below the inventory/log screens
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            group = canvasObject.GetComponent<CanvasGroup>();

            var go = new GameObject("HintButton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(canvasObject.transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.2f, 0.19f, 0.25f, 0.85f);
            go.GetComponent<Button>().onClick.AddListener(OnPressed);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            label = labelGo.GetComponent<Text>();
            label.font = Localization.CurrentFont;
            label.fontSize = 26;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.95f, 0.78f, 0.4f);
            label.raycastTarget = false;
            var labelRect = (RectTransform)labelGo.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;

            group.alpha = 0f;
            group.blocksRaycasts = false;
        }
    }
}
