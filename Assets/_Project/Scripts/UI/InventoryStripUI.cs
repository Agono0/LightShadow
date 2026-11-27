using System.Collections.Generic;
using LightShadow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace LightShadow.UI
{
    // HUD strip listing carried items (finish-the-game plan, phase 1: item
    // system). One small chip per item, top-left under the gauges, hidden
    // while the inventory is empty. Rebuilds on pickup and on save load.
    public class InventoryStripUI : MonoBehaviour
    {
        public RectTransform stripRoot;
        public Text chipTemplate;
        public Sprite keyIcon;
        public Sprite medkitIcon;
        public float chipWidth = 220f;
        public float chipHeight = 34f;

        readonly List<GameObject> chips = new List<GameObject>();

        void OnEnable()
        {
            Inventory.ItemAdded += OnItemAdded;
            Inventory.ItemRemoved += OnItemRemoved;
            Localization.LanguageChanged += OnLanguageChanged;
            SaveSystem.SaveApplied += OnSaveApplied;
            Rebuild();
        }

        void OnDisable()
        {
            Inventory.ItemAdded -= OnItemAdded;
            Inventory.ItemRemoved -= OnItemRemoved;
            Localization.LanguageChanged -= OnLanguageChanged;
            SaveSystem.SaveApplied -= OnSaveApplied;
        }

        void OnItemAdded(string id) => Rebuild();

        void OnItemRemoved(string id) => Rebuild();

        void OnLanguageChanged() => Rebuild();

        void OnSaveApplied(SaveSystem.SaveData data) => Rebuild();

        // Chip label. The name is logical text; Arabic is shaped once here (the
        // chip is a plain Text, nothing downstream shapes it). The icon indent
        // is trailing in Arabic and leading in English. Pure for EditMode tests.
        public static string ChipText(string logicalName, bool hasIcon, bool arabic, System.Func<string, string> present)
        {
            const string indent = "      ";
            if (!arabic) return hasIcon ? indent + logicalName : logicalName;
            string shaped = present != null ? present(logicalName) : logicalName;
            return hasIcon ? shaped + indent : shaped;
        }

        void Rebuild()
        {
            if (stripRoot == null || chipTemplate == null) return;

            for (int i = 0; i < chips.Count; i++)
            {
                if (chips[i] != null) Destroy(chips[i]);
            }
            chips.Clear();

            float x = 0f;
            foreach (string id in Inventory.Carried)
            {
                Text chip = Instantiate(chipTemplate, stripRoot);
                chip.gameObject.SetActive(true);
                Sprite icon = id == "medkit" ? medkitIcon : (id.EndsWith("key") ? keyIcon : null);
                bool arabic = Localization.Current == GameLanguage.Arabic;
                chip.text = ChipText(Inventory.DisplayName(id), icon != null, arabic, Localization.Present);
                if (icon != null)
                {
                    var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                    iconGo.transform.SetParent(chip.transform, false);
                    RectTransform iconRect = (RectTransform)iconGo.transform;
                    // Arabic text is drawn pre-shaped and left aligned, so the
                    // icon sits at the far end of the chip instead.
                    float edge = arabic ? 1f : 0f;
                    iconRect.anchorMin = iconRect.anchorMax = new Vector2(edge, 0.5f);
                    iconRect.pivot = new Vector2(edge, 0.5f);
                    iconRect.anchoredPosition = Vector2.zero;
                    iconRect.sizeDelta = new Vector2(30f, 30f);
                    Image iconImage = iconGo.GetComponent<Image>();
                    iconImage.sprite = icon;
                    iconImage.raycastTarget = false;
                }
                RectTransform rect = chip.rectTransform;
                rect.anchoredPosition = new Vector2(x, 0f);
                x += chipWidth + 8f;
                chips.Add(chip.gameObject);
            }

            stripRoot.gameObject.SetActive(chips.Count > 0);
        }
    }
}
