using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // A world prop that becomes a carried item on interact (finish-the-game
    // plan, phase 1: item system), starting with the staff-room master key.
    // Parent it to the prop's root with a collider below it; the
    // InteractionRaycaster resolves the nearest IInteractable up the chain.
    //
    // Persistence: a picked-up item is in the Inventory blob, and Start hides
    // the prop when the id is already carried, so a loaded game never offers
    // the same item twice.
    public class ItemPickup : MonoBehaviour, IInteractable
    {
        [Header("Item")]
        public string itemId = "";
        public string displayNameEn = "an item";
        public string displayNameAr = "";

        [Header("Wiring")]
        public AudioClip pickupClip;

        string DisplayName =>
            Localization.Current == GameLanguage.Arabic && !string.IsNullOrEmpty(displayNameAr)
                ? displayNameAr
                : displayNameEn;

        public string Prompt =>
            Localization.Current == GameLanguage.Arabic
                ? Localization.Present("خذ") + " " + DisplayName
                : "Take " + DisplayName;

        public bool CanInteract => true;

        void Start()
        {
            if (!string.IsNullOrEmpty(itemId) && Inventory.Has(itemId))
            {
                gameObject.SetActive(false);
            }
        }

        public void Interact(GameObject interactor)
        {
            if (string.IsNullOrEmpty(itemId)) return;
            Inventory.Add(itemId);

            if (pickupClip == null) pickupClip = ProceduralSfx.Pickup;
            if (pickupClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(pickupClip);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowMessage(
                    Localization.Current == GameLanguage.Arabic
                        ? Localization.Present("أخذت") + ": " + DisplayName
                        : "Taken: " + DisplayName,
                    3f);
            }

            gameObject.SetActive(false);
        }
    }
}
