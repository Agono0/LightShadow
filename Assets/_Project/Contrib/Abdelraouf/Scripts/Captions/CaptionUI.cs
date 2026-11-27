using System.Collections.Generic;
using LightShadow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Contrib.Abdelraouf.Captions
{
    // Draws the caption lines near the bottom centre of the screen. Built in
    // code (no prefab, no scene YAML). It never takes input. The service
    // decides the text and alpha; this class only shows rows.
    //
    // The direction marker is plain ASCII on purpose ("<", ">", "v") so it
    // renders in any project font: "<" is to the left, ">" to the right, "v"
    // behind the player. Cues in front of the player show no marker.
    public class CaptionUI : MonoBehaviour
    {
        public struct Row
        {
            public string text;
            public char marker; // '<', '>', 'v' or ' '
            public float alpha;
        }

        const int MaxRows = 6;
        const float RowHeight = 40f;

        readonly List<Text> labels = new List<Text>();
        RectTransform root;

        void Awake()
        {
            var canvasObject = new GameObject("CaptionCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            var rootObject = new GameObject("CaptionRoot", typeof(RectTransform));
            rootObject.transform.SetParent(canvasObject.transform, false);
            root = (RectTransform)rootObject.transform;
            root.anchorMin = new Vector2(0.25f, 0f);
            root.anchorMax = new Vector2(0.75f, 0f);
            root.pivot = new Vector2(0.5f, 0f);
            root.anchoredPosition = new Vector2(0f, 220f);
            root.sizeDelta = new Vector2(0f, RowHeight * MaxRows);

            for (int i = 0; i < MaxRows; i++)
            {
                labels.Add(MakeLabel(i));
            }
        }

        Text MakeLabel(int index)
        {
            var go = new GameObject("Caption_" + index, typeof(RectTransform), typeof(Image), typeof(Text));
            go.transform.SetParent(root, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, index * RowHeight);
            rect.sizeDelta = new Vector2(0f, RowHeight - 4f);
            var image = go.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.6f);
            image.raycastTarget = false;
            var text = go.GetComponent<Text>();
            text.font = Localization.CurrentFont;
            text.fontSize = 22;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            go.SetActive(false);
            return text;
        }

        public void SetRows(IList<Row> rows)
        {
            Font font = Localization.CurrentFont;
            for (int i = 0; i < labels.Count; i++)
            {
                Text label = labels[i];
                if (i >= rows.Count)
                {
                    if (label.gameObject.activeSelf) label.gameObject.SetActive(false);
                    continue;
                }
                Row row = rows[i];
                if (!label.gameObject.activeSelf) label.gameObject.SetActive(true);
                label.font = font;
                label.text = row.marker == ' ' ? row.text : MarkerLine(row);
                Color color = label.color;
                color.a = row.alpha;
                label.color = color;
                Image background = label.GetComponent<Image>();
                Color backgroundColor = background.color;
                backgroundColor.a = 0.6f * row.alpha;
                background.color = backgroundColor;
            }
        }

        // Marker on the side the sound comes from; "v" goes in front of the text.
        static string MarkerLine(Row row)
        {
            switch (row.marker)
            {
                case '<': return "<  " + row.text;
                case '>': return row.text + "  >";
                default: return "v  " + row.text + "  v";
            }
        }
    }
}
