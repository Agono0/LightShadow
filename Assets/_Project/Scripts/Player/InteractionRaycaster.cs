using LightShadow.Core;
using LightShadow.Gameplay;
using LightShadow.UI;
using UnityEngine;

namespace LightShadow.Player
{
    // Shoots a short ray from the camera every frame to find IInteractable objects
    // (quiz boards, fuse boxes, fragments, batteries, the gate). E interacts on
    // desktop; the touch Interact button calls PressInteract().
    public class InteractionRaycaster : MonoBehaviour
    {
        [Header("Wiring")]
        public GameConfig config;
        public Camera rayCamera;
        public HUDController hud;

        [Tooltip("Layers the interaction ray may hit. The player's own body is excluded.")]
        public LayerMask interactMask = ~0;

        public static InteractionRaycaster Instance { get; private set; }

        IInteractable current;

        void Awake()
        {
            Instance = this;
            if (config == null && GameManager.Instance != null) config = GameManager.Instance.config;
            if (hud == null && GameManager.Instance != null) hud = GameManager.Instance.hud;
            if (rayCamera == null) rayCamera = GetComponentInChildren<Camera>();
        }

        void Start()
        {
            // GameManager may finish Awake after this object; resolve late.
            if (config == null && GameManager.Instance != null) config = GameManager.Instance.config;
            if (hud == null && GameManager.Instance != null) hud = GameManager.Instance.hud;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            UpdateTarget();

            if (Input.GetKeyDown(KeyCode.E)) PressInteract();

            // Show or hide the on-screen interaction hint.
            if (hud != null)
            {
                if (current != null && current.CanInteract)
                    hud.SetPrompt(Application.isMobilePlatform ? current.Prompt : "[E] " + current.Prompt);
                else
                    hud.SetPrompt(current != null ? current.Prompt : "");
            }
        }

        void UpdateTarget()
        {
            current = null;
            if (rayCamera == null) return;

            float range = config != null ? config.interactRange : 2.5f;
            Ray ray = new Ray(rayCamera.transform.position, rayCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range, interactMask, QueryTriggerInteraction.Ignore))
            {
                // Never let Nour's own capsule swallow the interaction ray.
                if (GameManager.Instance != null && GameManager.Instance.player != null
                    && hit.transform.IsChildOf(GameManager.Instance.player))
                {
                    return;
                }

                current = hit.collider.GetComponentInParent<IInteractable>();
            }
        }

        public void PressInteract()
        {
            if (current == null || !current.CanInteract) return;

            GameObject interactor = GameManager.Instance != null && GameManager.Instance.player != null
                ? GameManager.Instance.player.gameObject
                : transform.root.gameObject;
            current.Interact(interactor);
        }
    }
}
