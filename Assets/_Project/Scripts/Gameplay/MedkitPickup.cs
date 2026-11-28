using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // Medkits are the only way to heal (user brief 2026-11-27) and stay rare:
    // mostly puzzle rewards, never lying around in bulk. One tap to collect;
    // use with M (desktop) or the MEDKIT touch button.
    //
    // Rules (audit K1): the player carries ONE kit. Each prop has its own
    // persisted identity (its object name), so a load hides only kits that
    // were actually picked. Taking a kit while carrying one is refused: the
    // prop stays, no sound.
    public class MedkitPickup : MonoBehaviour, IInteractable
    {
        public const string ItemId = "medkit";
        public const string AlreadyCarryingMessage = "Already carrying a medkit.";

        [Header("Wiring")]
        public AudioClip pickupClip;

        bool collected;

        public string Prompt => "Take the medkit";
        public bool CanInteract => true;
        public bool Collected => collected;
        public string SaveId => name;

        // Pure guard: a kit can only be packed when none is carried.
        public static bool CanPack(bool carryingKit) => !carryingKit;

        public void Interact(GameObject interactor)
        {
            GameManager gm = GameManager.Instance;
            if (!CanPack(Inventory.Has(ItemId)))
            {
                if (gm != null) gm.ShowMessage(AlreadyCarryingMessage, 3f);
                return;
            }

            Inventory.Add(ItemId);
            collected = true;

            if (pickupClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(pickupClip);
            else LightShadow.Audio.Sfx.Play("SFX_Pickup", ProceduralSfx.Pickup);

            if (gm != null) gm.ShowMessage("Medkit packed. I opens the bag, M uses it.", 4f);

            gameObject.SetActive(false);
        }

        // Called by GameManager when a save says this kit was already taken.
        public void RestoreFromSave()
        {
            collected = true;
            gameObject.SetActive(false);
        }
    }
}
