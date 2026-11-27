using System.Collections.Generic;
using System.Text;
using LightShadow.Contrib.Abdelraouf.Hints;
using LightShadow.Contrib.Abdelraouf.Journal;
using LightShadow.Contrib.Abdelraouf.Overlay;
using LightShadow.Core;
using LightShadow.Player;
using LightShadow.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Contrib.Abdelraouf.HintLog
{
    // Hint Recall screen: a list of puzzles that have at least one unlocked
    // hint, and a detail pane with the unlocked levels of the selected one.
    // Built entirely in code (no prefab, no scene YAML), same style as HintUI.
    //
    // Open/close: the L key via HintLogInput (approved 2026-11-27). Esc is NOT
    // consumed here; the owner's side owns Esc close-first and modal ordering
    // and can call HintLogUI.Close() first.
    //
    // Input lock: PlayerController.ExternalInputLocked is a shared static bool
    // with no ownership. This screen refuses to open while it is already set
    // or another overlay is up, and clears it only if it set it itself. That
    // is NOT an ownership guarantee: a system that sets the lock after this
    // screen opened cannot be detected, so the owner's ordering is load-bearing.
    //
    // Data: puzzle texts come from HintCatalog.Load(HintService.Instance.
    // hintsJson.text); unlocked levels come from max(HintService.LevelFor,
    // HintLogRecord), clamped. Without a live HintService the list is empty.
    public class HintLogUI : MonoBehaviour
    {
        public static HintLogUI Instance { get; private set; }

        public static bool IsOpen => Instance != null && Instance.root != null && Instance.root.activeSelf;

        readonly HintLogModel model = new HintLogModel();
        readonly List<Image> rowBackgrounds = new List<Image>();
        readonly List<GameObject> rowObjects = new List<GameObject>();

        GameObject root;
        RectTransform listRoot;
        Text titleText;
        Text detailName;
        Text detailBody;
        Text emptyText;
        Text controlsText;
        Text closeButtonLabel;

        InputLockToken inputLock;
        bool cursorWasLocked;

        public static HintLogUI EnsureExists()
        {
            if (Instance != null) return Instance;
            var host = new GameObject("HintLog");
            return host.AddComponent<HintLogUI>();
        }

        public static void Toggle()
        {
            HintLogUI ui = EnsureExists();
            if (ui.IsOpenInstance) ui.Close();
            else ui.Open();
        }

        bool IsOpenInstance => root != null && root.activeSelf;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            BuildUi();
            root.SetActive(false);
            HintService.HintShown += OnHintShown;
            SaveSystem.SaveApplied += OnSaveApplied;
            Localization.LanguageChanged += OnLanguageChanged;
        }

        void OnDestroy()
        {
            HintService.HintShown -= OnHintShown;
            SaveSystem.SaveApplied -= OnSaveApplied;
            Localization.LanguageChanged -= OnLanguageChanged;
            OverlayStack.Release(ref inputLock);
            if (Instance == this) Instance = null;
        }

        public void Open()
        {
            if (IsOpenInstance) return;

            // Never stack on another system's lock or overlay.
            if (PlayerController.ExternalInputLocked) return;
            if (PauseMenu.IsOpen || InspectViewController.IsOpen) return;
            GameManager gm = GameManager.Instance;
            if (gm == null) return;
            if (gm.quizUI != null && gm.quizUI.IsOpen) return;

            Refresh();

            // Dismissable: Esc/Start close this screen before pause opens.
            inputLock = OverlayStack.Acquire("HintLog", true, true, Close);

            cursorWasLocked = Cursor.lockState == CursorLockMode.Locked;
            if (cursorWasLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            ApplyLanguage();
            root.SetActive(true);
        }

        public void Close()
        {
            if (!IsOpenInstance) return;
            root.SetActive(false);

            OverlayStack.Release(ref inputLock);
            if (cursorWasLocked && !Application.isMobilePlatform && !PauseMenu.IsOpen && !OverlayStack.WantsFreeCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void MoveSelection(int delta)
        {
            if (model.Move(delta))
            {
                RefreshHighlight();
                RefreshDetail();
            }
        }

        void OnHintShown(string puzzleId, int level) { if (IsOpenInstance) Refresh(); }
        void OnSaveApplied(SaveSystem.SaveData data) { if (IsOpenInstance) Refresh(); }
        void OnLanguageChanged() { if (IsOpenInstance) { ApplyLanguage(); Refresh(); } }

        // Rebuilds the model from the live catalog and unlocked levels.
        void Refresh()
        {
            HintService service = HintService.Instance;
            IReadOnlyList<HintEntry> catalog = null;
            if (service != null)
            {
                HintLogRecord.Adopt(service);
                catalog = HintCatalog.Load(service.hintsJson != null ? service.hintsJson.text : "");
            }
            model.Rebuild(catalog, UnlockedLevel);
            RebuildList();
            RefreshDetail();
        }

        static int UnlockedLevel(string puzzleId)
        {
            return HintLogModel.Clamp(System.Math.Max(HintService.LevelFor(puzzleId), HintLogRecord.Level(puzzleId)));
        }

        static string L(string english, string arabic)
        {
            return Localization.Current == GameLanguage.Arabic ? arabic : english;
        }

        // ------------------------------------------------------------ UI --

        void RebuildList()
        {
            for (int i = 0; i < rowObjects.Count; i++)
            {
                if (rowObjects[i] != null) Destroy(rowObjects[i]);
            }
            rowObjects.Clear();
            rowBackgrounds.Clear();

            Font font = Localization.CurrentFont;
            const float rowH = 48f;
            const float pad = 6f;

            for (int i = 0; i < model.Count; i++)
            {
                HintLogModel.Row row = model.Rows[i];

                var go = new GameObject("Row_" + row.PuzzleId, typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(listRoot, false);
                var rect = (RectTransform)go.transform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -i * (rowH + pad));
                rect.sizeDelta = new Vector2(0f, rowH);
                rowBackgrounds.Add(go.GetComponent<Image>());

                var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
                labelGo.transform.SetParent(go.transform, false);
                var label = labelGo.GetComponent<Text>();
                label.font = font;
                label.fontSize = 20;
                label.color = new Color(0.92f, 0.9f, 0.86f);
                label.alignment = TextAnchor.MiddleLeft;
                label.horizontalOverflow = HorizontalWrapMode.Overflow;
                label.text = row.DisplayName + "  (" + row.Unlocked + "/" + HintService.MaxLevel + ")";
                var labelRect = (RectTransform)labelGo.transform;
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(12f, 0f);
                labelRect.offsetMax = new Vector2(-8f, 0f);

                int index = i;
                go.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (model.Select(index))
                    {
                        RefreshHighlight();
                        RefreshDetail();
                    }
                });
                rowObjects.Add(go);
            }

            emptyText.gameObject.SetActive(model.Count == 0);
            emptyText.text = L(
                "No hints unlocked yet. Ask for a hint near a puzzle and it will be listed here.",
                "لم يُفتح أي تلميح بعد. اطلب تلميحًا قرب أحد الألغاز وسيظهر هنا.");
            if (Localization.Current == GameLanguage.Arabic) emptyText.text = Localization.Present(emptyText.text);
            RefreshHighlight();
        }

        void RefreshHighlight()
        {
            for (int i = 0; i < rowBackgrounds.Count; i++)
            {
                rowBackgrounds[i].color = i == model.SelectedIndex
                    ? new Color(0.55f, 0.42f, 0.18f, 0.98f)
                    : new Color(0.16f, 0.15f, 0.19f, 0.95f);
            }
        }

        void RefreshDetail()
        {
            HintLogModel.Row row = model.Selected;
            if (row == null)
            {
                detailName.text = "";
                detailBody.text = "";
                return;
            }

            bool arabic = Localization.Current == GameLanguage.Arabic;
            string[] namesEn = { "Nudge", "Clue", "Answer" };
            string[] namesAr = { "تلميح خفيف", "دليل", "الحل" };

            detailName.text = row.DisplayName;

            // Only unlocked levels are ever read from the entry (HintLogModel).
            var builder = new StringBuilder();
            List<KeyValuePair<int, string>> texts = HintLogModel.UnlockedTexts(row, arabic);
            for (int i = 0; i < texts.Count; i++)
            {
                int level = texts[i].Key;
                string body = texts[i].Value;
                if (arabic)
                {
                    builder.Append(Localization.Present(namesAr[level - 1] + " " + level + "/" + HintService.MaxLevel));
                    builder.Append('\n');
                    builder.Append(Localization.Present(JournalBook.WrapLogical(body, 40)));
                }
                else
                {
                    builder.Append("Hint ").Append(level).Append('/').Append(HintService.MaxLevel)
                        .Append(" - ").Append(namesEn[level - 1]).Append('\n').Append(body);
                }
                builder.Append("\n\n");
            }

            int locked = HintLogModel.LockedCount(row);
            if (locked > 0)
            {
                string lockedText = arabic
                    ? Localization.Present(locked + " من التلميحات لم تُفتح بعد.")
                    : locked + (locked == 1 ? " more hint is still locked." : " more hints are still locked.");
                builder.Append(lockedText);
            }
            detailBody.text = builder.ToString();
        }

        void ApplyLanguage()
        {
            Font font = Localization.CurrentFont;
            bool arabic = Localization.Current == GameLanguage.Arabic;

            titleText.text = L("Hint Recall", Localization.Present("سجل التلميحات"));
            closeButtonLabel.text = L("Close", Localization.Present("إغلاق"));
            controlsText.text = L(
                "L: close    Up / Down: select puzzle",
                "L: " + Localization.Present("إغلاق") + "    " + Localization.Present("الأعلى / الأسفل: اختيار لغز"));

            Text[] all = { titleText, detailName, detailBody, emptyText, controlsText, closeButtonLabel };
            foreach (Text text in all)
            {
                if (text == null) continue;
                text.font = font;
                if (arabic) text.horizontalOverflow = HorizontalWrapMode.Overflow;
            }
            if (arabic)
            {
                detailName.alignment = TextAnchor.MiddleRight;
                detailBody.alignment = TextAnchor.UpperRight;
            }
            else
            {
                detailName.alignment = TextAnchor.MiddleLeft;
                detailBody.alignment = TextAnchor.UpperLeft;
            }
        }

        void BuildUi()
        {
            var canvasObject = new GameObject("HintLogCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 81;   // above the hint banner (70); inventory screen uses 80
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            root = new GameObject("Root", typeof(RectTransform));
            root.transform.SetParent(canvasObject.transform, false);
            Stretch((RectTransform)root.transform);

            var backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
            backdrop.transform.SetParent(root.transform, false);
            Stretch((RectTransform)backdrop.transform);
            backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

            var window = new GameObject("Window", typeof(RectTransform), typeof(Image));
            window.transform.SetParent(root.transform, false);
            var windowRect = (RectTransform)window.transform;
            windowRect.anchorMin = windowRect.anchorMax = new Vector2(0.5f, 0.5f);
            windowRect.sizeDelta = new Vector2(920f, 560f);
            window.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.11f, 0.97f);

            titleText = MakeText(window.transform, "Title", new Vector2(0f, 248f), new Vector2(860f, 40f), 30, TextAnchor.MiddleCenter);

            listRoot = new GameObject("List", typeof(RectTransform)).GetComponent<RectTransform>();
            listRoot.SetParent(window.transform, false);
            listRoot.anchorMin = listRoot.anchorMax = new Vector2(0f, 1f);
            listRoot.pivot = new Vector2(0f, 1f);
            listRoot.anchoredPosition = new Vector2(30f, -70f);
            listRoot.sizeDelta = new Vector2(300f, 400f);

            emptyText = MakeText(window.transform, "Empty", new Vector2(0f, 20f), new Vector2(700f, 120f), 20, TextAnchor.MiddleCenter);
            emptyText.horizontalOverflow = HorizontalWrapMode.Wrap;

            detailName = MakeText(window.transform, "DetailName", new Vector2(100f, 180f), new Vector2(400f, 36f), 24, TextAnchor.MiddleLeft);
            detailBody = MakeText(window.transform, "DetailBody", new Vector2(100f, -20f), new Vector2(400f, 340f), 18, TextAnchor.UpperLeft);
            detailBody.verticalOverflow = VerticalWrapMode.Overflow;
            detailBody.horizontalOverflow = HorizontalWrapMode.Wrap;

            MakeButton(window.transform, "CloseButton", new Vector2(370f, -200f), new Vector2(130f, 44f), Close, out closeButtonLabel);
            controlsText = MakeText(window.transform, "Controls", new Vector2(-40f, -252f), new Vector2(800f, 30f), 14, TextAnchor.MiddleCenter);
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        static Text MakeText(Transform parent, string name, Vector2 position, Vector2 size, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var text = go.GetComponent<Text>();
            text.font = Localization.CurrentFont;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = new Color(0.92f, 0.9f, 0.86f);
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        static Button MakeButton(Transform parent, string name, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction onClick, out Text label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.2f, 0.19f, 0.25f, 0.98f);
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);
            label = MakeText(go.transform, "Label", Vector2.zero, size, 20, TextAnchor.MiddleCenter);
            return button;
        }
    }
}
