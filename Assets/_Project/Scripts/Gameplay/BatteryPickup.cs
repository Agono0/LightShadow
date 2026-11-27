using LightShadow.Core;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // Doc 4.2: battery pickups restore 50% charge. One tap to collect.
    public class BatteryPickup : MonoBehaviour, IInteractable
    {
        [Header("Wiring")]
        public AudioClip pickupClip;

        public string Prompt => "Take the battery";
        public bool CanInteract => true;

        public void Interact(GameObject interactor)
        {
            float fraction = 0.5f;
            GameManager gm = GameManager.Instance;
            if (gm != null && gm.config != null) fraction = gm.config.batteryPickupFraction;

            Player.FlashlightBattery flashlight = gm != null && gm.player != null
                ? gm.player.GetComponentInChildren<Player.FlashlightBattery>()
                : null;
            // Fallback: never eat the pickup without charging (user report:
            // the bar did not fill on pickup in some scenes).
            if (flashlight == null) flashlight = Object.FindAnyObjectByType<Player.FlashlightBattery>();
            if (flashlight != null) flashlight.AddBattery(fraction);

            if (pickupClip == null) pickupClip = Core.ProceduralSfx.Pickup;
            if (pickupClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(pickupClip);
            gameObject.SetActive(false);
        }
    }
}
