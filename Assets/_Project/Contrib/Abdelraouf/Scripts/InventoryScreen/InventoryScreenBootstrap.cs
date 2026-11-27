using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.InventoryScreen
{
    // Creates the inventory screen host after each scene load so the I key and
    // gamepad Y work without any scene wiring. The screen refuses to open
    // outside gameplay (no GameManager), so menu scenes only carry one idle
    // component. The owner's side may instead place the host through the H2
    // level contribution (InventoryLevelContribution); the Instance singleton
    // in InventoryScreenUI keeps that from ever duplicating.
    public static class InventoryScreenBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Ensure()
        {
            InventoryScreenUI ui = InventoryScreenUI.EnsureExists();
            if (ui.GetComponent<InventoryScreenInput>() == null)
            {
                ui.gameObject.AddComponent<InventoryScreenInput>();
            }
        }
    }
}
