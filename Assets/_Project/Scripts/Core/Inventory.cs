using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightShadow.Core
{
    // The player's carried items (finish-the-game plan, phase 1: item system).
    // A static, scene-independent set of item ids ("master_key", ...) with a
    // save blob riding SaveSystem's capture/apply events, the same pattern the
    // story (storyJson) and puzzle (puzzleJson) blobs use. Old saves without
    // the field deserialize to an empty inventory (additive string, per the
    // repo's save-data rule).
    public static class Inventory
    {
        static readonly HashSet<string> items = new HashSet<string>();

        [Serializable]
        class InventorySave
        {
            public List<string> itemIds = new List<string>();
        }

        // Fired after an item lands in the inventory; future HUD badges listen.
        public static event Action<string> ItemAdded;

        // Fired after an item leaves the inventory (consumed medkit, used
        // key) so HUD chips can drop it. Not fired when the id was not held.
        public static event Action<string> ItemRemoved;

        // Display names for the HUD strip, keyed by item id. Scene props
        // (ItemPickup) can be inactive until revealed, so the names live here
        // instead of on the prop.
        static readonly Dictionary<string, string> nameEn = new Dictionary<string, string>
        {
            { "master_key", "Master key" },
            { "fuse", "Fuse" },
            { "classroom_key", "Classroom key" },
            { "staff_key", "Staff room key" },
            { "medkit", "Medkit" },
        };
        static readonly Dictionary<string, string> nameAr = new Dictionary<string, string>
        {
            { "master_key", "المفتاح الرئيسي" },
            { "fuse", "مصهر" },
            { "classroom_key", "مفتاح الفصل" },
            { "staff_key", "مفتاح غرفة المعلمين" },
            { "medkit", "حقيبة إسعاف" },
        };

        public static string DisplayName(string id)
        {
            Dictionary<string, string> table = Localization.Current == GameLanguage.Arabic ? nameAr : nameEn;
            string name;
            return table.TryGetValue(id, out name) ? name : id;
        }

        public static IEnumerable<string> Carried => items;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Hook()
        {
            items.Clear();
            SaveSystem.SaveCaptured -= OnSaveCaptured;
            SaveSystem.SaveCaptured += OnSaveCaptured;
            SaveSystem.SaveApplied -= OnSaveApplied;
            SaveSystem.SaveApplied += OnSaveApplied;
        }

        public static bool Has(string id)
        {
            return !string.IsNullOrEmpty(id) && items.Contains(id);
        }

        public static void Add(string id)
        {
            if (string.IsNullOrEmpty(id) || !items.Add(id)) return;
            Action<string> added = ItemAdded;
            if (added != null) added(id);
        }

        // New run: nothing carried over from the previous run (audit F2).
        public static void ResetForNewRun()
        {
            // Every new-run path comes through here: an owed puzzle medkit must not leak.
            LightShadow.Gameplay.PuzzleKeyRewards.ResetPending();
            if (items.Count == 0) return;
            var gone = new List<string>(items);
            items.Clear();
            Action<string> removed = ItemRemoved;
            if (removed == null) return;
            for (int i = 0; i < gone.Count; i++) removed(gone[i]);
        }

        public static void Remove(string id)
        {
            if (string.IsNullOrEmpty(id) || !items.Remove(id)) return;
            Action<string> removed = ItemRemoved;
            if (removed != null) removed(id);
        }

        static void OnSaveCaptured(SaveSystem.SaveData data)
        {
            if (data == null) return;
            InventorySave save = new InventorySave();
            save.itemIds.AddRange(items);
            data.inventoryJson = JsonUtility.ToJson(save);
        }

        static void OnSaveApplied(SaveSystem.SaveData data)
        {
            items.Clear();
            if (data == null || string.IsNullOrEmpty(data.inventoryJson)) return;
            try
            {
                InventorySave save = JsonUtility.FromJson<InventorySave>(data.inventoryJson);
                if (save != null && save.itemIds != null)
                {
                    for (int i = 0; i < save.itemIds.Count; i++)
                    {
                        // A null or empty id in a hand-edited save is not an item.
                        if (string.IsNullOrEmpty(save.itemIds[i])) continue;
                        items.Add(save.itemIds[i]);
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Inventory save blob unreadable, starting inventory fresh: " + exception.Message);
            }
        }
    }
}
