using System.Collections.Generic;
using LightShadow.Contrib.Abdelraouf.Overlay;
using LightShadow.Core;
using LightShadow.Player;
using LightShadow.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.Contrib.Abdelraouf.InventoryScreen
{
    // Fullscreen inventory screen, built entirely in code (no prefab, no scene
    // YAML), in the same style as HintUI and InspectViewController: a dimmed
    // backdrop, an item grid, a detail pane and Use / Close buttons.
    //
    // Open/close: the I key and the gamepad Y button (approved 2026-11-27) via
    // InventoryScreenInput; the owner's side adds the touch HUD button, which
    // only needs to call InventoryScreenUI.Toggle(). Esc/Start close-first is
    // the owner's guard in PauseMenu (approved 2026-11-27): this class exposes
    // Close() and never consumes Escape itself.
    //
    // Input lock ownership (contract 2026-11-27): PlayerController.
    // ExternalInputLocked is a shared static bool with no ownership tracking.
    // This screen therefore (a) refuses to open while any other system holds
    // the lock or an overlay is up, and (b) clears the lock only when it set
    // it itself (an OverlayStack token). That is the strongest guarantee the bool
    // API allows; a system that sets the lock AFTER this screen opened cannot
    // be detected. Reported to the owner's side.
    //
    // Inspect: the detail pane IS the inspect view (name + description of the
    // selected item). Reusing InspectViewController was rejected on purpose:
    // its Hide() clears ExternalInputLocked unconditionally, which would drop
    // this screen's lock (contract: never clear another system's lock).
    public class InventoryScreenUI : MonoBehaviour
    {
        public static InventoryScreenUI Instance { get; private set; }

        public static bool IsOpen => Instance != null && Instance.root != null && Instance.root.activeSelf;

        [Header("Behaviour")]
        public int gridColumns = 4;
        public float removedPollSeconds = 0.25f;

        readonly InventoryScreenModel model = new InventoryScreenModel();

        GameObject root;
        Text titleText;
        Text detailName;
        Text detailDescription;
        Text feedbackText;
        Text controlsText;
        Button useButton;
        Text useButtonLabel;
        Text closeButtonLabel;
        RectTransform gridRoot;
        readonly List<Image> slotBackgrounds = new List<Image>();
        readonly List<Text> slotLabels = new List<Text>();

        InputLockToken inputLock;
        bool cursorWasLocked;
        float pollClock;
        int lastKnownCount = -1;
        int lastKnownHash;

        // Creates the screen if missing and returns it (same self-build idea
        // as InspectViewController.Show).
        public static InventoryScreenUI EnsureExists()
        {
            if (Instance != null) return Instance;
            var host = new GameObject("InventoryScreen");
            return host.AddComponent<InventoryScreenUI>();
        }

        public static void Toggle()
        {
            InventoryScreenUI ui = EnsureExists();
            if (IsOpen) ui.Close();
            else ui.Open();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            model.Columns = gridColumns;
            BuildUi();
            root.SetActive(false);
            Inventory.ItemAdded += OnItemAdded;
            InventoryItemCatalog.ItemUsed += OnItemUsed;
            SaveSystem.SaveApplied += OnSaveApplied;
            Localization.LanguageChanged += OnLanguageChanged;
        }

        void OnDestroy()
        {
            Inventory.ItemAdded -= OnItemAdded;
            InventoryItemCatalog.ItemUsed -= OnItemUsed;
            SaveSystem.SaveApplied -= OnSaveApplied;
            Localization.LanguageChanged -= OnLanguageChanged;
            // Never leave the player locked behind a destroyed screen, and
            // never clear a lock another system owns.
            OverlayStack.Release(ref inputLock);
            if (Instance == this) Instance = null;
        }

        public void Open()
        {
            if (IsOpen) return;

            // Never stack on another system's lock or overlay.
            if (PlayerController.ExternalInputLocked) return;
            if (PauseMenu.IsOpen || InspectViewController.IsOpen) return;
            GameManager gm = GameManager.Instance;
            if (gm == null) return;
            if (gm.quizUI != null && gm.quizUI.IsOpen) return;

            RefreshFromInventory();

            // Dismissable: Esc/Start close this screen before pause opens.
            inputLock = OverlayStack.Acquire("Inventory", true, true, Close);

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
            if (!IsOpen) return;
            root.SetActive(false);

            OverlayStack.Release(ref inputLock);
            if (cursorWasLocked && !Application.isMobilePlatform && !PauseMenu.IsOpen && !OverlayStack.WantsFreeCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        void Update()
        {
            if (!IsOpen) return;

            // PROVISIONAL removal watch (until the owner's ItemRemoved event
            // exists): re-read the carried set a few times per second and
            // refresh when it changed. Cheap: HashSet enumeration allocates
            // nothing; the sort only runs on an actual change.
            pollClock -= Time.unscaledDeltaTime;
            if (pollClock > 0f) return;
            pollClock = removedPollSeconds;

            int count = 0;
            int hash = 0;
            foreach (string id in Inventory.Carried)
            {
                count++;
                hash = hash * 31 + (id != null ? id.GetHashCode() : 0);
            }
            if (count != lastKnownCount || hash != lastKnownHash) RefreshFromInventory();
        }

        void OnItemAdded(string id) { if (IsOpen) RefreshFromInventory(); }
        void OnItemUsed(string id) { if (IsOpen) RefreshFromInventory(); }
        void OnSaveApplied(SaveSystem.SaveData data) { if (IsOpen) RefreshFromInventory(); }
        void OnLanguageChanged() { if (IsOpen) { ApplyLanguage(); RefreshFromInventory(); } }

        void RefreshFromInventory()
        {
            model.Refresh(Inventory.Carried);
            lastKnownCount = model.Count;
            int hash = 0;
            for (int i = 0; i < model.Count; i++) hash = hash * 31 + model.Items[i].GetHashCode();
            lastKnownHash = hash;
            RebuildGrid();
            RefreshDetail();
        }

        // ------------------------------------------------------- actions --

        public void MoveSelection(int deltaColumn, int deltaRow)
        {
            if (model.Move(deltaColumn, deltaRow))
            {
                RefreshHighlight();
                RefreshDetail();
            }
        }

        public void UseSelected()
        {
            string id = model.SelectedId;
            if (id == null) return;

            if (!InventoryItemCatalog.HasUseHandler(id))
            {
                // The owner's side registers real handlers at startup; without
                // one the item is not consumable from this screen.
                feedbackText.text = L("Nothing to do with this yet.", "لا يمكن استخدام هذا بعد.");
                return;
            }

            bool used = InventoryItemCatalog.TryUse(id);
            feedbackText.text = used ? L("Used.", "تم الاستخدام.") : L("Cannot use that now.", "لا يمكن استخدامه الآن.");
            if (used) RefreshFromInventory();
        }

        // ------------------------------------------------------------ UI --

        void RebuildGrid()
        {
            for (int i = 0; i < slotLabels.Count; i++)
            {
                if (slotLabels[i] != null) Destroy(slotLabels[i].transform.parent.gameObject);
            }
            slotBackgrounds.Clear();
            slotLabels.Clear();

            Font font = Localization.CurrentFont;
            bool arabic = Localization.Current == GameLanguage.Arabic;
            const float slotW = 128f;
            const float slotH = 64f;
            const float pad = 8f;

            for (int i = 0; i < model.Count; i++)
            {
                string id = model.Items[i];

                var slot = new GameObject("Slot_" + id, typeof(RectTransform), typeof(Image), typeof(Button));
                slot.transform.SetParent(gridRoot, false);
                RectTransform rect = (RectTransform)slot.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                int row = i / gridColumns;
                int col = i % gridColumns;
                rect.anchoredPosition = new Vector2(col * (slotW + pad), -row * (slotH + pad));
                rect.sizeDelta = new Vector2(slotW, slotH);

                Image background = slot.GetComponent<Image>();
                background.color = new Color(0.16f, 0.15f, 0.19f, 0.95f);

                var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
                labelGo.transform.SetParent(slot.transform, false);
                Text label = labelGo.GetComponent<Text>();
                label.text = Inventory.DisplayName(id);
                label.font = font;
                label.alignment = TextAnchor.MiddleCenter;
                label.color = new Color(0.92f, 0.9f, 0.86f);
                label.horizontalOverflow = HorizontalWrapMode.Overflow;
                RectTransform labelRect = (RectTransform)labelGo.transform;
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;

                int index = i;
                slot.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (model.Select(index))
                    {
                        RefreshHighlight();
                        RefreshDetail();
                    }
                });

                slotBackgrounds.Add(background);
                slotLabels.Add(label);
            }

            RefreshHighlight();
        }

        void RefreshHighlight()
        {
            for (int i = 0; i < slotBackgrounds.Count; i++)
            {
                bool selected = i == model.SelectedIndex;
                slotBackgrounds[i].color = selected
                    ? new Color(0.55f, 0.42f, 0.18f, 0.98f)   // calm amber, matches quiz timer
                    : new Color(0.16f, 0.15f, 0.19f, 0.95f);
            }
        }

        void RefreshDetail()
        {
            string id = model.SelectedId;
            if (id == null)
            {
                detailName.text = L("(empty)", "(فارغ)");
                detailDescription.text = "";
                feedbackText.text = "";
                useButton.interactable = false;
                return;
            }

            detailName.text = Inventory.DisplayName(id);
            detailDescription.text = InventoryItemCatalog.Description(id);
            useButton.interactable = InventoryItemCatalog.HasUseHandler(id);
        }

        static string L(string english, string arabic)
        {
            return Localization.Current == GameLanguage.Arabic ? arabic : english;
        }

        void ApplyLanguage()
        {
            Font font = Localization.CurrentFont;
            bool arabic = Localization.Current == GameLanguage.Arabic;

            titleText.text = L("Inventory", "المخزون");
            titleText.font = font;
            useButtonLabel.text = L("Use", "استخدام");
            useButtonLabel.font = font;
            closeButtonLabel.text = L("Close", "إغلاق");
            closeButtonLabel.font = font;
            controlsText.text = L(
                "I / Y: close    Arrows / D-pad: select    Enter / A: use",
                "I / Y: إغلاق    الأسهم / D-pad: تحديد    Enter / A: استخدام");
            controlsText.font = font;

            Text[] all = { titleText, detailName, detailDescription, feedbackText, controlsText, useButtonLabel, closeButtonLabel };
            foreach (Text text in all)
            {
                if (text == null) continue;
                text.font = font;
                if (arabic)
                {
                    text.alignment = text == controlsText ? TextAnchor.MiddleCenter : TextAnchor.MiddleRight;
                    text.horizontalOverflow = HorizontalWrapMode.Overflow;
                }
            }
        }

        void BuildUi()
        {
            var canvasObject = new GameObject("InventoryCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;   // above the HUD strip and hint banner (70)
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            root = new GameObject("Root", typeof(RectTransform));
            root.transform.SetParent(canvasObject.transform, false);
            Stretch((RectTransform)root.transform);

            // Backdrop.
            var backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
            backdrop.transform.SetParent(root.transform, false);
            Stretch((RectTransform)backdrop.transform);
            backdrop.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

            // Window.
            var window = new GameObject("Window", typeof(RectTransform), typeof(Image));
            window.transform.SetParent(root.transform, false);
            RectTransform windowRect = (RectTransform)window.transform;
            windowRect.anchorMin = windowRect.anchorMax = new Vector2(0.5f, 0.5f);
            windowRect.sizeDelta = new Vector2(920f, 560f);
            window.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.11f, 0.97f);

            titleText = MakeText(window.transform, "Title", new Vector2(0f, 248f), new Vector2(860f, 40f), 30, TextAnchor.MiddleCenter);

            gridRoot = new GameObject("Grid", typeof(RectTransform)).GetComponent<RectTransform>();
            gridRoot.SetParent(window.transform, false);
            gridRoot.anchorMin = gridRoot.anchorMax = new Vector2(0f, 1f);
            gridRoot.pivot = new Vector2(0f, 1f);
            gridRoot.anchoredPosition = new Vector2(30f, -70f);
            gridRoot.sizeDelta = new Vector2(560f, 420f);

            detailName = MakeText(window.transform, "DetailName", new Vector2(620f, 180f), new Vector2(270f, 36f), 24, TextAnchor.MiddleLeft);
            detailDescription = MakeText(window.transform, "DetailDescription", new Vector2(620f, 40f), new Vector2(270f, 220f), 18, TextAnchor.UpperLeft);
            detailDescription.verticalOverflow = VerticalWrapMode.Overflow;
            feedbackText = MakeText(window.transform, "Feedback", new Vector2(620f, -120f), new Vector2(270f, 30f), 16, TextAnchor.MiddleLeft);

            useButton = MakeButton(window.transform, "UseButton", new Vector2(620f, -180f), new Vector2(130f, 44f), UseSelected, out useButtonLabel);
            MakeButton(window.transform, "CloseButton", new Vector2(760f, -180f), new Vector2(130f, 44f), Close, out closeButtonLabel);

            controlsText = MakeText(window.transform, "Controls", new Vector2(0f, -252f), new Vector2(880f, 30f), 14, TextAnchor.MiddleCenter);
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        Text MakeText(Transform parent, string name, Vector2 position, Vector2 size, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Text text = go.GetComponent<Text>();
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = new Color(0.92f, 0.9f, 0.86f);
            Font font = Localization.CurrentFont;
            if (font != null) text.font = font;
            return text;
        }

        Button MakeButton(Transform parent, string name, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction onClick, out Text label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.2f, 0.19f, 0.24f, 0.98f);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(go.transform, false);
            label = labelGo.GetComponent<Text>();
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.92f, 0.9f, 0.86f);
            RectTransform labelRect = (RectTransform)labelGo.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;

            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);
            return button;
        }
    }
}
