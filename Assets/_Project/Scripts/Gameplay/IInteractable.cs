using UnityEngine;

namespace LightShadow.Gameplay
{
    // Anything the player can aim at and press E on: quiz boards, fuse boxes,
    // knowledge fragments, battery pickups and the exit gate.
    public interface IInteractable
    {
        string Prompt { get; }
        bool CanInteract { get; }
        void Interact(GameObject interactor);
    }
}
