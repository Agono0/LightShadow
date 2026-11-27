using LightShadow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Contrib.Abdelraouf.Hints
{
    // Small hint banner at the bottom of the screen, built in code (no
    // prefab, no scene YAML). It never takes input and never locks the player:
    // it shows a title line and the hint text, then fades out.
    public class HintUI : MonoBehaviour
    {
        const float FadeSeconds = 0.6f;

        GameObject panel;
        CanvasGroup group;
        Text titleText;
        Text bodyText;
        float remaining;

        void Awake()
        {
            BuildUi();
            panel.SetActive(false);
        }

        public void Show(string title, string body, float seconds)
        {
            Font font = Localization.CurrentFont;
            bool arabic = Localization.Current == GameLanguage.Arabic;
            titleText.font = font;
            bodyText.font = font;
            titleText.text = title;
            bodyText.text = body;
            TextAnchor anchor = TextAnchor.MiddleCenter;
            titleText.alignment = anchor;
            bodyText.alignment = anchor;
            if (arabic) bodyText.horizontalOverflow = HorizontalWrapMode.Overflow;
            remaining = Mathf.Max(1f, seconds);
            group.alpha = 1f;
            panel.SetActive(true);
        }

        public void Hide()
        {
            remaining = 0f;
            if (panel != null) panel.SetActive(false);
        }

        void Update()
        {
            if (!panel.activeSelf) return;
            remaining -= Time.unscaledDeltaTime;
            if (remaining <= 0f)
            {
                panel.SetActive(false);
                return;
            }
            group.alpha = Mathf.Clamp01(remaining / FadeSeconds);
        }

        void BuildUi()
        {
            var canvasObject = new GameObject("HintCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 70;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            panel = new GameObject("HintPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            panel.transform.SetParent(canvasObject.transform, false);
            var rect = (RectTransform)panel.transform;
            rect.anchorMin = new Vector2(0.2f, 0f);
            rect.anchorMax = new Vector2(0.8f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 90f);
            rect.sizeDelta = new Vector2(0f, 110f);
            panel.GetComponent<Image>().color = new Color(0.04f, 0.04f, 0.06f, 0.82f);
            panel.GetComponent<Image>().raycastTarget = false;
            group = panel.GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;

            Font font = Localization.CurrentFont;
            titleText = MakeText(panel.transform, "Title", font, 20, new Vector2(0f, 0.62f), new Vector2(1f, 1f));
            titleText.color = new Color(0.95f, 0.78f, 0.4f, 1f);
            bodyText = MakeText(panel.transform, "Body", font, 24, new Vector2(0f, 0f), new Vector2(1f, 0.64f));
        }

        static Text MakeText(Transform parent, string name, Font font, int size, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = new Vector2(14f, 4f);
            rect.offsetMax = new Vector2(-14f, -4f);
            var text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }
    }
}
