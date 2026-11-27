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
            SaveSystem.SaveApplied += OnSaveApplied;
            Rebuild();
        }

        void OnDisable()
        {
            Inventory.ItemAdded -= OnItemAdded;
            SaveSystem.SaveApplied -= OnSaveApplied;
        }

        void OnItemAdded(string id) => Rebuild();

        void OnSaveApplied(SaveSystem.SaveData data) => Rebuild();

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
                chip.text = icon != null ? "      " + Inventory.DisplayName(id) : Inventory.DisplayName(id);
                if (icon != null)
                {
                    var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                    iconGo.transform.SetParent(chip.transform, false);
                    RectTransform iconRect = (RectTransform)iconGo.transform;
                    iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
                    iconRect.pivot = new Vector2(0f, 0.5f);
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
