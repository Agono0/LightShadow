using System.Collections.Generic;
using LightShadow.Core;
using LightShadow.Gameplay;
using UnityEngine;

namespace LightShadow.Contrib.Abdelraouf.Journal
{
    // A paper note the player can aim at and pick up. The text lives in the
    // journal data file; this prop only carries the note id. Put a collider
    // on it or below it so InteractionRaycaster can hit it.
    //
    // Strings handed to the HUD are LOGICAL (unshaped) text: the HUD runs
    // Localization.TrPresent on whatever it gets, so shaping here as well
    // double-shaped the Arabic (audit v3 3.1). Present happens once, in the HUD.
    public class JournalNote : MonoBehaviour, IInteractable
    {
        public string noteId = "";

        // Notes that were active when their scene loaded. A save restore that
        // lands after Start (SaveSystem.Restore applies at once when the scene
        // is already live) re-checks these, because an inactive note cannot
        // listen for anything itself (audit v3 5.7).
        static readonly List<JournalNote> live = new List<JournalNote>();
        static bool subscribed;

        public const string EnglishToast = "Note added to the journal. Press J to read it.";
        public const string ArabicToast = "أُضيفت ورقة إلى الدفتر. اضغط J للقراءة.";
        public const string EnglishPrompt = "Read the note";
        public const string ArabicPrompt = "اقرأ الورقة";

        public string Prompt =>
            Localization.Current == GameLanguage.Arabic ? ArabicPrompt : EnglishPrompt;

        public bool CanInteract => !string.IsNullOrEmpty(noteId);

        // Pure rule, kept separate so EditMode tests can pin it.
        public static bool ShouldBeActive(bool collected)
        {
            return !collected;
        }

        void Awake()
        {
            live.Add(this);
            if (!subscribed)
            {
                subscribed = true;
                JournalProgress.Changed += OnProgressChanged;
            }
        }

        void OnDestroy()
        {
            live.Remove(this);
        }

        void Start()
        {
            // A loaded game must not offer a note it already holds.
            ApplyProgress();
        }

        void ApplyProgress()
        {
            bool active = ShouldBeActive(JournalProgress.Has(noteId));
            if (gameObject.activeSelf != active) gameObject.SetActive(active);
        }

        static void OnProgressChanged()
        {
            for (int i = live.Count - 1; i >= 0; i--)
            {
                JournalNote note = live[i];
                if (note == null) { live.RemoveAt(i); continue; }
                note.ApplyProgress();
            }
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract) return;
            JournalProgress.Add(noteId);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowMessage(
                    Localization.Current == GameLanguage.Arabic ? ArabicToast : EnglishToast,
                    3f);
            }

            gameObject.SetActive(false);
        }
    }
}
