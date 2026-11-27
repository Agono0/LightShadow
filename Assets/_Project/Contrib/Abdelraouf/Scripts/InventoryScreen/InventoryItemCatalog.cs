using System;
using System.Collections.Generic;
using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.InventoryScreen
{
    // Item metadata and use handlers for the inventory screen.
    //
    // The catalog holds descriptions (EN/AR), icon keys and a use-handler
    // registry. REAL USE HANDLERS ARE REGISTERED BY THE OWNER'S SIDE AT
    // STARTUP (contract 2026-11-27): this module ships no gameplay effects.
    // Example for their bootstrap:
    //   InventoryItemCatalog.Register("medkit",
    //       () => PlayerHealth.Instance != null && PlayerHealth.Instance.TryUseMedkit());
    // A handler returns true when the item was actually consumed; only then is
    // the id removed from the inventory and ItemUsed fired.
    public static class InventoryItemCatalog
    {
        public class Entry
        {
            public string Id;
            public string DescriptionEn = "";
            public string DescriptionAr = "";
            public string IconKey = "generic";
        }

        // Icon keys are stable strings; the owner's side maps them to sprites
        // (InventoryStripUI already picks icons by the same key/medkit idea).
        public const string IconKeyGeneric = "generic";
        public const string IconKeyKey = "key";
        public const string IconKeyMedkit = "medkit";

        static readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>
        {
            // Arabic descriptions are AI drafts, not native-checked.
            { "master_key", new Entry { Id = "master_key", IconKey = IconKeyKey,
                DescriptionEn = "Opens the school's main door.",
                DescriptionAr = "يفتح الباب الرئيسي للمدرسة." } },
            { "fuse", new Entry { Id = "fuse", IconKey = IconKeyGeneric,
                DescriptionEn = "A spare fuse from the first-aid cabinet. Nothing needs it yet.",
                DescriptionAr = "مصهر احتياطي من خزانة الإسعافات. لا حاجة له الآن." } },
            { "classroom_key", new Entry { Id = "classroom_key", IconKey = IconKeyKey,
                DescriptionEn = "Unlocks a classroom door.",
                DescriptionAr = "يفتح باب أحد الفصول الدراسية." } },
            { "staff_key", new Entry { Id = "staff_key", IconKey = IconKeyKey,
                DescriptionEn = "Unlocks the staff room.",
                DescriptionAr = "يفتح غرفة المعلمين." } },
            { "medkit", new Entry { Id = "medkit", IconKey = IconKeyMedkit,
                DescriptionEn = "First-aid kit. Using it restores one hit.",
                DescriptionAr = "حقيبة إسعافات أولية تعيد إصابة واحدة عند استخدامها." } },
        };

        static readonly Dictionary<string, Func<bool>> useHandlers = new Dictionary<string, Func<bool>>();

        // Fired after an item was successfully used and removed.
        public static event Action<string> ItemUsed;

        // Registers the real use handler for an item id. Called by the owner's
        // side at startup. A null handler clears the registration.
        public static void Register(string id, Func<bool> useHandler)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (useHandler == null) useHandlers.Remove(id);
            else useHandlers[id] = useHandler;
        }

        public static bool HasUseHandler(string id)
        {
            return !string.IsNullOrEmpty(id) && useHandlers.ContainsKey(id);
        }

        // Test hook: drops every registered handler (entries stay).
        public static void ClearUseHandlers()
        {
            useHandlers.Clear();
        }

        // Adds or replaces the metadata for an item id (for ids their side
        // introduces later).
        public static void SetEntry(string id, string descriptionEn, string descriptionAr, string iconKey)
        {
            if (string.IsNullOrEmpty(id)) return;
            entries[id] = new Entry
            {
                Id = id,
                DescriptionEn = descriptionEn ?? "",
                DescriptionAr = descriptionAr ?? "",
                IconKey = string.IsNullOrEmpty(iconKey) ? IconKeyGeneric : iconKey,
            };
        }

        public static string Description(string id)
        {
            Entry entry;
            if (string.IsNullOrEmpty(id) || !entries.TryGetValue(id, out entry)) return "";
            bool arabic = Localization.Current == GameLanguage.Arabic;
            string text = arabic ? entry.DescriptionAr : entry.DescriptionEn;
            if (string.IsNullOrEmpty(text)) text = arabic ? entry.DescriptionEn : entry.DescriptionAr;
            return text ?? "";
        }

        public static string IconKey(string id)
        {
            Entry entry;
            if (!string.IsNullOrEmpty(id) && entries.TryGetValue(id, out entry)) return entry.IconKey;
            // Same heuristic the HUD strip uses for ids the catalog does not know.
            if (!string.IsNullOrEmpty(id))
            {
                if (id == "medkit") return IconKeyMedkit;
                if (id.EndsWith("key", StringComparison.Ordinal)) return IconKeyKey;
            }
            return IconKeyGeneric;
        }

        // Runs the registered handler. Only when the handler reports the item
        // was consumed is it removed from the inventory and ItemUsed fired.
        public static bool TryUse(string id)
        {
            Func<bool> handler;
            if (string.IsNullOrEmpty(id) || !useHandlers.TryGetValue(id, out handler) || handler == null)
            {
                return false;
            }

            bool consumed;
            try
            {
                consumed = handler();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[LightShadow] Inventory use handler for '" + id + "' threw: " + exception.Message);
                return false;
            }

            if (!consumed) return false;

            Inventory.Remove(id);
            Action<string> used = ItemUsed;
            if (used != null) used(id);
            return true;
        }
    }
}
