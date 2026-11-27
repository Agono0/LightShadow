using LightShadow.Core;
using LightShadow.Player;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // P3 hiding v1 (plan 4.12): an interactable place to hide - lockers, under
    // teacher desks, stage curtains. The solid box collider doubles as the
    // interaction-ray target ("[E] Hide in ..."), the same pattern as every
    // other interactable; HidingSystem owns the enter/exit choreography.
    //
    // The static All list is the registry the Shadow's Search state samples
    // from (it never checks the spot Nour currently occupies on the first
    // pass - that exclusion lives in EnemyBehaviour, which knows the player's
    // position). LitZone's static-list pattern, again.
    public class HideSpot : MonoBehaviour, IInteractable
    {
        [Header("Spot")]
        public string displayName = "the locker";

        [Tooltip("Where Nour stands (and peeks from) while hidden. Place it with clear space around it - the capsule must not overlap geometry.")]
        public Transform hidePoint;

        static readonly System.Collections.Generic.List<HideSpot> all = new System.Collections.Generic.List<HideSpot>();

        // Registry of every live hide spot in the scene.
        public static System.Collections.Generic.IReadOnlyList<HideSpot> All => all;

        void OnEnable()
        {
            if (!all.Contains(this)) all.Add(this);
        }

        void OnDisable()
        {
            all.Remove(this);
        }

        public string Prompt
        {
            get
            {
                return HidingSystem.IsHidden && HidingSystem.CurrentSpot == this
                    ? Localization.Format("Leave {0}", displayName)
                    : Localization.Format("Hide in {0}", displayName);
            }
        }

        // Enter from the open; exit only from the spot Nour is inside.
        public bool CanInteract => !HidingSystem.IsHidden || HidingSystem.CurrentSpot == this;

        public void Interact(GameObject interactor)
        {
            if (HidingSystem.IsHidden)
            {
                if (HidingSystem.CurrentSpot == this) HidingSystem.ExitCurrent();
                return;
            }
            HidingSystem.Enter(this);
        }
    }
}
