using System;
using System.Globalization;
using LightShadow.Core;
using LightShadow.Story;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // Main-menu save slot panel (brief #5, decision D12): three slots with act
    // name, fragment count and timestamp; CONTINUE (NewestSlot), per-slot LOAD /
    // NEW GAME (with overwrite confirm) / DELETE (with confirm). Selecting a
    // slot sets SaveSystem.CurrentSlot; loads run through LoadingScreen.
    //
    // Layout is built by Tools/Editor/SceneBuilderStory.cs into the main-menu
    // canvas (dark card, 64 px touch rows). The controller only fills rows and
    // routes buttons - all persistence is SaveSystem's.
    public class SaveSlotUI : MonoBehaviour
    {
        [Serializable]
        public class SlotRow
        {
            public GameObject root;
            public Text slotText;
            public Text infoText;
            public Button loadButton;
            public Button newButton;
            public Button deleteButton;
            public GameObject confirmRoot;
            public Text confirmText;
            public Button confirmYes;
            public Button confirmNo;
        }

        [Header("Wiring (built by SceneBuilderStory)")]
        public GameObject panel;
        public Text titleText;
        public SlotRow[] rows = new SlotRow[SaveSystem.SlotCount];
        public Button continueButton;
        public Button backButton;

        public bool IsOpen => panel != null && panel.activeSelf;

        // What happens after the panel picks a slot.
        enum Intent { None, Load, NewGame, Delete, Notice }
        Intent pendingIntent;
        int pendingSlot = -1; // the slot the open confirmation belongs to

        void Awake()
        {
            if (panel != null) panel.SetActive(false);

            if (continueButton != null) continueButton.onClick.AddListener(ContinueNewest);
            if (backButton != null) backButton.onClick.AddListener(Hide);

            for (int slot = 0; slot < rows.Length; slot++)
            {
                int index = slot;
                SlotRow row = rows[slot];
                if (row == null) continue;

                if (row.loadButton != null) row.loadButton.onClick.AddListener(() => LoadSlot(index));
                if (row.newButton != null) row.newButton.onClick.AddListener(() => AskNewGame(index));
                if (row.deleteButton != null) row.deleteButton.onClick.AddListener(() => AskDelete(index));
                if (row.confirmYes != null) row.confirmYes.onClick.AddListener(() => ConfirmYes(index));
                if (row.confirmNo != null) row.confirmNo.onClick.AddListener(() => CancelConfirm(index));
            }

            Localization.LanguageChanged += Refresh;
        }

        void OnDestroy()
        {
            Localization.LanguageChanged -= Refresh;
        }

        public void Show()
        {
            if (panel != null) panel.SetActive(true);
            Refresh();
            SelectFirst();
        }

        public void SelectFirst()
        {
            if (EventSystem.current != null && rows.Length > 0 && rows[0] != null
                && rows[0].loadButton != null)
            {
                EventSystem.current.SetSelectedGameObject(rows[0].loadButton.gameObject);
            }
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }

        public void Toggle()
        {
            if (IsOpen) Hide();
            else Show();
        }

        public void Refresh()
        {
            bool arabic = Localization.Current == GameLanguage.Arabic;
            Font font = Localization.CurrentFont;

            if (titleText != null)
            {
                titleText.text = StoryStrings.Tr("SAVE SLOTS");
                titleText.font = font;
            }

            int newest = SaveSystem.NewestSlot();

            for (int slot = 0; slot < rows.Length; slot++)
            {
                SlotRow row = rows[slot];
                if (row == null) continue;

                if (row.slotText != null)
                {
                    row.slotText.text = string.Format(StoryStrings.Tr("SLOT {0}"), slot + 1);
                    row.slotText.font = font;
                    row.slotText.alignment = arabic ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
                }

                bool hasSave = SaveSystem.HasSave(slot);
                SaveSystem.SaveData data = hasSave ? SaveSystem.LoadFromSlot(slot) : null;

                if (row.infoText != null)
                {
                    row.infoText.font = font;
                    row.infoText.alignment = arabic ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
                    row.infoText.text = data != null ? Describe(data) : StoryStrings.Tr("EMPTY");
                }

                SetRowButton(row.loadButton, data != null);
                SetRowButton(row.deleteButton, data != null);
                SetRowButton(row.newButton, true);

                // Continue-style affordance: highlight the newest slot's LOAD.
                if (row.loadButton != null && data != null)
                {
                    ColorBlock colors = row.loadButton.colors;
                    bool isNewest = slot == newest;
                    colors.normalColor = isNewest
                        ? new Color(0.36f, 0.25f, 0.13f, 0.98f)   // warm (ButtonWarm)
                        : new Color(0.16f, 0.15f, 0.14f, 0.95f);  // dark (ButtonDark)
                    row.loadButton.colors = colors;
                }

                if (row.confirmRoot != null) row.confirmRoot.SetActive(false);
            }
        }

        static void SetRowButton(Button button, bool enabled)
        {
            if (button == null) return;
            button.interactable = enabled;
        }

        string Describe(SaveSystem.SaveData data)
        {
            // Act from progress (approximate, documented): no fuses = Act I,
            // some = Act II, all three = Act III / dawn. Templates are
            // translated first, then formatted.
            int chapter = data.fusesRestored <= 0 ? 1 : data.fusesRestored < GameManager.FuseTarget ? 2 : 3;
            return string.Format(StoryStrings.Tr("CHAPTER {0}"), chapter) + "\n"
                + FormatFragments(data.fragmentsCollected) + "\n"
                + string.Format(StoryStrings.Tr("Saved {0}"), FormatStamp(data.savedAtUtc));
        }

        // audit v3 5.5: the fragment target lives in GameManager.FragmentTarget.
        // The translated template carries a baked "/6", so substitute the live
        // target - the UI can never desync if the target moves.
        public static string FormatFragments(int collected)
        {
            string template = StoryStrings.Tr("Fragments: {0}/6");
            const string baked = "/6";
            int slash = template.LastIndexOf(baked, StringComparison.Ordinal);
            if (slash >= 0)
            {
                template = template.Substring(0, slash)
                    + "/" + GameManager.FragmentTarget
                    + template.Substring(slash + baked.Length);
            }
            return string.Format(template, collected);
        }

        static string FormatStamp(string savedAtUtc)
        {
            if (string.IsNullOrEmpty(savedAtUtc)) return "-";
            if (DateTime.TryParse(savedAtUtc, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime stamp))
            {
                return stamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            }
            return savedAtUtc;
        }

        // ------------------------------------------------------------ actions --

        public void ContinueNewest()
        {
            int slot = SaveSystem.NewestSlot();
            if (slot < 0) return;
            SaveSystem.CurrentSlot = slot;
            StartRun(Intent.Load);
        }

        void LoadSlot(int slot)
        {
            if (!SaveSystem.HasSave(slot)) return;
            SaveSystem.CurrentSlot = slot;
            StartRun(Intent.Load);
        }

        void AskNewGame(int slot)
        {
            if (SaveSystem.HasSave(slot)) ShowConfirm(slot, StoryStrings.Tr("Overwrite this slot?"), Intent.NewGame);
            else
            {
                SaveSystem.CurrentSlot = slot;
                StartRun(Intent.NewGame);
            }
        }

        void AskDelete(int slot)
        {
            if (!SaveSystem.HasSave(slot)) return;
            ShowConfirm(slot, StoryStrings.Tr("Delete this save?"), Intent.Delete);
        }

        void CloseAllConfirms()
        {
            if (rows != null)
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    if (rows[i] != null && rows[i].confirmRoot != null) rows[i].confirmRoot.SetActive(false);
                }
            }
            pendingIntent = Intent.None;
            pendingSlot = -1;
        }

        // One confirmation at a time: opening one closes the rest, and the
        // open one remembers its slot and intent (audit: stale dialogs).
        void ShowConfirm(int slot, string message, Intent intent)
        {
            SlotRow row = rows[slot];
            if (row == null || row.confirmRoot == null) return;
            CloseAllConfirms();
            pendingIntent = intent;
            pendingSlot = slot;
            if (row.confirmText != null) row.confirmText.text = message;
            row.confirmRoot.SetActive(true);
        }

        // Close-only message on a slot row; either button just closes it.
        void ShowNotice(int slot, string message)
        {
            if (slot < 0 || rows == null || slot >= rows.Length) return;
            ShowConfirm(slot, message, Intent.Notice);
        }

        void CancelConfirm(int slot)
        {
            SlotRow row = rows[slot];
            if (slot == pendingSlot) { pendingIntent = Intent.None; pendingSlot = -1; }
            if (row != null && row.confirmRoot != null) row.confirmRoot.SetActive(false);
        }

        void ConfirmYes(int slot)
        {
            SlotRow row = rows[slot];
            if (row != null && row.confirmRoot != null) row.confirmRoot.SetActive(false);

            // Only the dialog that is actually open may act, and None never
            // means delete.
            Intent intent = slot == pendingSlot ? pendingIntent : Intent.None;
            pendingIntent = Intent.None;
            pendingSlot = -1;

            switch (intent)
            {
                case Intent.NewGame:
                    SaveSystem.DeleteSlot(slot); // "New Game (choose slot -> clears)"
                    SaveSystem.CurrentSlot = slot;
                    StartRun(Intent.NewGame);
                    break;
                case Intent.Load:
                    LoadSlot(slot);
                    break;
                case Intent.Delete:
                    SaveSystem.DeleteSlot(slot);
                    Refresh();
                    break;
                default:
                    break; // Notice or a stale dialog: nothing happens
            }
        }

        void StartRun(Intent intent)
        {
            SaveSystem.SaveData loaded = null;
            if (intent == Intent.Load)
            {
                // Read the slot BEFORE leaving the menu: a corrupt or unreadable
                // slot must not silently become a new game that inherits the
                // previous run (audit F1).
                loaded = SaveSystem.LoadFromSlot(SaveSystem.CurrentSlot);
                if (loaded == null)
                {
                    Debug.LogWarning("[LightShadow] Save slot " + SaveSystem.CurrentSlot + " could not be read; staying on the menu.");
                    Refresh();
                    ShowNotice(SaveSystem.CurrentSlot, StoryStrings.Tr("That save could not be read. It was left untouched."));
                    return;
                }
            }

            Hide();

            if (intent == Intent.NewGame)
            {
                // A deferred restore from an earlier Load attempt must not
                // survive into the fresh run (audit A3).
                SaveSystem.ClearPendingRestore();
                StoryDirector.ResetForNewRun();
                // Stale hints must not leak into the new run (overlay review).
                LightShadow.Contrib.Abdelraouf.HintLog.HintLogRecord.Clear();
                RunResults.Reset();
                Inventory.ResetForNewRun();
                LightShadow.Player.HidingSystem.ResetState();
                LightShadow.Contrib.Abdelraouf.Journal.JournalProgress.ResetForNewRun();
            }

            LoadingScreen.Transition(SceneNames.School);

            if (intent == Intent.Load)
            {
                // Restore is deferred by SaveSystem until the school scene is
                // live (it subscribes to sceneLoaded itself).
                SaveSystem.Restore(loaded);
            }
        }
    }
}
