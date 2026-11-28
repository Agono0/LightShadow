using LightShadow.Contrib.Abdelraouf.InventoryScreen;
using LightShadow.Player;
using UnityEngine;

namespace LightShadow.Core
{
    // Registers the real effects for inventory items the inventory screen can
    // use. The screen module ships no gameplay effects (contract), so this is
    // the owner's side of that contract. A handler returns true only when the
    // item was actually consumed.
    public static class InventoryUseHandlers
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            InventoryItemCatalog.Register("medkit", UseMedkit);
        }

        // TryUseMedkit already removes the item and refills health. The catalog
        // removes the id again on success; Inventory.Remove ignores a missing id.
        static bool UseMedkit()
        {
            PlayerHealth health = PlayerHealth.Instance;
            if (health == null) return false;
            bool used = health.TryUseMedkit();
            if (used && GameManager.Instance != null)
            {
                GameManager.Instance.ShowMessage("Medkit used. Back on her feet.", 3f);
            }
            return used;
        }
    }
}
