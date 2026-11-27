using System.Collections.Generic;
using System.Text;
using LightShadow.Contrib.Abdelraouf.Overlay;
using LightShadow.Core;
using LightShadow.Player;
using LightShadow.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Contrib.Abdelraouf.Journal
{
    // The journal screen. Built entirely in code with uGUI (no prefab, no
    // scene YAML): a dark panel, the found notes as a list on one side and
    // the selected note on the other, in English or Arabic.
    //
    // Keys: J opens and closes it. Esc is left to PauseMenu, which this
    // component does not touch. Mobile builds get a small Journal button.
    // While open it holds PlayerController.ExternalInputLocked and frees the
    // cursor, the same way the puzzle overlays do.
    public class JournalBook : MonoBehaviour
    {
        public static JournalBook Instance { get; private set; }

        [Tooltip("Data/Journal/journal_notes.json, assigned by the level builder.")]
        public TextAsset notesJson;

        const int ArabicWrapChars = 34;

        GameObject panel;
        GameObject openButton;
        Text titleText;
        Text emptyText;
        Text detailTitle;
        Text detailBody;
        Text closeLabel;
        Text openLabel;
        RectTransform listContent;
        readonly List<Text> listLabels = new List<Text>();
        string selectedId = "";
        bool cursorWasLocked;
        InputLockToken lockToken;

        public bool IsOpen => panel != null && panel.activeSelf;

        void Awake()
        {
            Instance = this;
            BuildUi();
            panel.SetActive(false);
        }

        void OnEnable()
        {
            JournalProgress.Changed += OnProgressChanged;
            Localization.LanguageChanged += OnProgressChanged;
        }

        void OnDisable()
        {
            JournalProgress.Changed -= OnProgressChanged;
            Localization.LanguageChanged -= OnProgressChanged;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (IsOpen) ReleaseLock();
        }

        void Update()
        {
            // Esc closes the journal first; PauseMenu sees the token (or the
            // close this frame) and stays shut, in either Update order.
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
                return;
            }
            if (!Input.GetKeyDown(KeyCode.J)) return;
            Toggle();
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public void Open()
        {
            if (IsOpen) return;
            // Do not stack on the pause menu or another overlay.
            if (PauseMenu.IsOpen || PlayerController.ExternalInputLocked) return;

            lockToken = OverlayStack.Acquire("Journal", true, true, Close);
            cursorWasLocked = Cursor.lockState == CursorLockMode.Locked;
            if (cursorWasLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            panel.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            if (!IsOpen) return;
            panel.SetActive(false);
            ReleaseLock();
        }

        void ReleaseLock()
        {
            OverlayStack.Release(ref lockToken);
            if (cursorWasLocked && !Application.isMobilePlatform)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            cursorWasLocked = false;
        }

        void OnProgressChanged()
        {
            if (IsOpen) Refresh();
        }

        // ---- rendering ---------------------------------------------------

        // The journal ships its own two-language strings; the shared
        // Localization table is not edited from the contributor lane.
        static string Ui(string english, string arabic)
        {
            return Localization.Current == GameLanguage.Arabic ? Localization.Present(arabic) : english;
        }

        void Refresh()
        {
            bool arabic = Localization.Current == GameLanguage.Arabic;
            Font font = Localization.CurrentFont;

            titleText.font = font;
            titleText.text = Ui("Journal", "دفتر الملاحظات");
            closeLabel.font = font;
            closeLabel.text = Ui("Close", "إغلاق");
            if (openLabel != null)
            {
                openLabel.font = font;
                openLabel.text = Ui("Journal", "دفتر الملاحظات");
            }

            IReadOnlyList<string> found = JournalProgress.Found;
            string text = notesJson != null ? notesJson.text : "";

            // Clear the list.
            for (int i = listContent.childCount - 1; i >= 0; i--)
            {
                Destroy(listContent.GetChild(i).gameObject);
            }
            listLabels.Clear();

            bool selectedStillValid = false;
            int shown = 0;
            for (int i = 0; i < found.Count; i++)
            {
                JournalNoteEntry entry = JournalCatalog.Find(text, found[i]);
                if (entry == null) continue;
                if (entry.id == selectedId) selectedStillValid = true;
                AddListRow(entry, shown, font, arabic);
                shown++;
            }

            if (!selectedStillValid) selectedId = "";
            if (selectedId == "" && shown > 0)
            {
                for (int i = 0; i < found.Count; i++)
                {
                    if (JournalCatalog.Find(text, found[i]) != null) { selectedId = found[i]; break; }
                }
            }

            listContent.sizeDelta = new Vector2(0f, Mathf.Max(10f, shown * 52f + 10f));

            emptyText.font = font;
            emptyText.alignment = TextAnchor.MiddleCenter;
            emptyText.gameObject.SetActive(shown == 0);
            emptyText.text = Ui("No notes found yet.", "لم تجد أي ورقة بعد");

            RenderDetail(JournalCatalog.Find(text, selectedId), font, arabic);
        }

        void AddListRow(JournalNoteEntry entry, int index, Font font, bool arabic)
        {
            var row = new GameObject("Note_" + entry.id, typeof(RectTransform), typeof(Image), typeof(Button));
            row.transform.SetParent(listContent, false);
            var rect = (RectTransform)row.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -10f - index * 52f);
            rect.sizeDelta = new Vector2(-20f, 46f);

            bool selected = entry.id == selectedId;
            row.GetComponent<Image>().color = selected
                ? new Color(0.85f, 0.7f, 0.35f, 0.35f)
                : new Color(1f, 1f, 1f, 0.08f);

            string id = entry.id;
            row.GetComponent<Button>().onClick.AddListener(() =>
            {
                selectedId = id;
                Refresh();
            });

            Text label = MakeText(row.transform, "Label", font, 20, arabic ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft);
            Stretch(label.rectTransform, 10f, 10f, 2f, 2f);
            string title = arabic && !string.IsNullOrEmpty(entry.titleAr) ? entry.titleAr : entry.titleEn;
            label.text = Localization.Present(title);
            listLabels.Add(label);
        }

        void RenderDetail(JournalNoteEntry entry, Font font, bool arabic)
        {
            detailTitle.font = font;
            detailBody.font = font;
            detailTitle.alignment = arabic ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
            detailBody.alignment = arabic ? TextAnchor.UpperRight : TextAnchor.UpperLeft;

            if (entry == null)
            {
                detailTitle.text = "";
                detailBody.text = "";
                return;
            }

            if (arabic && !string.IsNullOrEmpty(entry.bodyAr))
            {
                detailTitle.text = Localization.Present(string.IsNullOrEmpty(entry.titleAr) ? entry.titleEn : entry.titleAr);
                // ArabicSupport reverses each line for display, so a long
                // paragraph cannot be left to the Text component's own wrap
                // (it would put the last line first). Wrap in logical order.
                detailBody.text = Localization.Present(WrapLogical(entry.bodyAr, ArabicWrapChars));
            }
            else
            {
                detailTitle.text = entry.titleEn;
                detailBody.text = entry.bodyEn;
            }
        }

        // Greedy word wrap on the logical string, '\n' between lines.
        public static string WrapLogical(string text, int maxChars)
        {
            if (string.IsNullOrEmpty(text) || maxChars < 8) return text;
            string[] words = text.Split(' ');
            var builder = new StringBuilder(text.Length + 8);
            int lineLength = 0;
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                if (word.Length == 0) continue;
                if (lineLength > 0 && lineLength + 1 + word.Length > maxChars)
                {
                    builder.Append('\n');
                    lineLength = 0;
                }
                else if (lineLength > 0)
                {
                    builder.Append(' ');
                    lineLength++;
                }
                builder.Append(word);
                lineLength += word.Length;
            }
            return builder.ToString();
        }

        // ---- UI construction ---------------------------------------------

        void BuildUi()
        {
            var canvasObject = new GameObject("JournalCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            Font font = Localization.CurrentFont;

            panel = new GameObject("JournalPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasObject.transform, false);
            Stretch((RectTransform)panel.transform, 0f, 0f, 0f, 0f);
            panel.GetComponent<Image>().color = new Color(0.04f, 0.04f, 0.06f, 0.94f);

            titleText = MakeText(panel.transform, "Title", font, 36, TextAnchor.UpperCenter);
            var titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -20f);
            titleRect.sizeDelta = new Vector2(0f, 56f);

            // List column (left 34%).
            var scroll = new GameObject("List", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(Mask));
            scroll.transform.SetParent(panel.transform, false);
            var scrollRect = (RectTransform)scroll.transform;
            scrollRect.anchorMin = new Vector2(0.03f, 0.14f);
            scrollRect.anchorMax = new Vector2(0.36f, 0.86f);
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            scroll.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.05f);
            scroll.GetComponent<Mask>().showMaskGraphic = true;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(scroll.transform, false);
            listContent = (RectTransform)content.transform;
            listContent.anchorMin = new Vector2(0f, 1f);
            listContent.anchorMax = new Vector2(1f, 1f);
            listContent.pivot = new Vector2(0.5f, 1f);
            listContent.anchoredPosition = Vector2.zero;
            listContent.sizeDelta = new Vector2(0f, 10f);

            var scrollView = scroll.GetComponent<ScrollRect>();
            scrollView.content = listContent;
            scrollView.horizontal = false;
            scrollView.vertical = true;
            scrollView.movementType = ScrollRect.MovementType.Clamped;
            scrollView.scrollSensitivity = 30f;

            emptyText = MakeText(panel.transform, "Empty", font, 22, TextAnchor.MiddleCenter);
            var emptyRect = emptyText.rectTransform;
            emptyRect.anchorMin = new Vector2(0.03f, 0.14f);
            emptyRect.anchorMax = new Vector2(0.36f, 0.86f);
            emptyRect.offsetMin = Vector2.zero;
            emptyRect.offsetMax = Vector2.zero;

            // Detail column (right 58%).
            detailTitle = MakeText(panel.transform, "DetailTitle", font, 28, TextAnchor.UpperLeft);
            var dtRect = detailTitle.rectTransform;
            dtRect.anchorMin = new Vector2(0.40f, 0.72f);
            dtRect.anchorMax = new Vector2(0.97f, 0.86f);
            dtRect.offsetMin = Vector2.zero;
            dtRect.offsetMax = Vector2.zero;
            detailTitle.color = new Color(0.95f, 0.8f, 0.45f, 1f);

            detailBody = MakeText(panel.transform, "DetailBody", font, 24, TextAnchor.UpperLeft);
            var dbRect = detailBody.rectTransform;
            dbRect.anchorMin = new Vector2(0.40f, 0.14f);
            dbRect.anchorMax = new Vector2(0.97f, 0.70f);
            dbRect.offsetMin = Vector2.zero;
            dbRect.offsetMax = Vector2.zero;

            // Close button.
            var closeObject = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeObject.transform.SetParent(panel.transform, false);
            var closeRect = (RectTransform)closeObject.transform;
            closeRect.anchorMin = new Vector2(0.5f, 0f);
            closeRect.anchorMax = new Vector2(0.5f, 0f);
            closeRect.pivot = new Vector2(0.5f, 0f);
            closeRect.anchoredPosition = new Vector2(0f, 24f);
            closeRect.sizeDelta = new Vector2(220f, 52f);
            closeObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);
            closeObject.GetComponent<Button>().onClick.AddListener(Close);
            closeLabel = MakeText(closeObject.transform, "Label", font, 24, TextAnchor.MiddleCenter);
            Stretch(closeLabel.rectTransform, 0f, 0f, 0f, 0f);

            if (Application.isMobilePlatform)
            {
                openButton = new GameObject("JournalOpenButton", typeof(RectTransform), typeof(Image), typeof(Button));
                openButton.transform.SetParent(canvasObject.transform, false);
                var openRect = (RectTransform)openButton.transform;
                openRect.anchorMin = new Vector2(1f, 1f);
                openRect.anchorMax = new Vector2(1f, 1f);
                openRect.pivot = new Vector2(1f, 1f);
                openRect.anchoredPosition = new Vector2(-24f, -96f);
                openRect.sizeDelta = new Vector2(150f, 48f);
                openButton.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.45f);
                openButton.GetComponent<Button>().onClick.AddListener(Toggle);
                openLabel = MakeText(openButton.transform, "Label", font, 22, TextAnchor.MiddleCenter);
                Stretch(openLabel.rectTransform, 0f, 0f, 0f, 0f);
                openLabel.text = Ui("Journal", "دفتر الملاحظات");
            }
        }

        static Text MakeText(Transform parent, string name, Font font, int size, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        static void Stretch(RectTransform rect, float left, float right, float top, float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }
    }
}
