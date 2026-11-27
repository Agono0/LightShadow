using LightShadow.Core;
using LightShadow.Gameplay;
using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.Journal
{
    // A paper note the player can aim at and pick up. The text lives in the
    // journal data file; this prop only carries the note id. Put a collider
    // on it or below it so InteractionRaycaster can hit it.
    public class JournalNote : MonoBehaviour, IInteractable
    {
        public string noteId = "";

        public string Prompt =>
            Localization.Current == GameLanguage.Arabic
                ? Localization.Present("اقرأ الورقة")
                : "Read the note";

        public bool CanInteract => !string.IsNullOrEmpty(noteId);

        void Start()
        {
            // A loaded game must not offer a note it already holds.
            if (JournalProgress.Has(noteId)) gameObject.SetActive(false);
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract) return;
            JournalProgress.Add(noteId);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowMessage(
                    Localization.Current == GameLanguage.Arabic
                        ? Localization.Present("أُضيفت ورقة إلى الدفتر")
                        : "Note added to the journal. Press J to read it.",
                    3f);
            }

            gameObject.SetActive(false);
        }
    }
}
