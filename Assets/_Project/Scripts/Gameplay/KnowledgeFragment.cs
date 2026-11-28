using LightShadow.Core;
using LightShadow.UI;
using UnityEngine;

namespace LightShadow.Gameplay
{
    // Doc 4.4: each of the six Knowledge Fragments is an object tied to a place
    // Nour's mother loved. Collecting one plays a memory line, then asks a
    // themed question. The fragment counts even if the question is missed -
    // there is no lock-out, only the study recap remembering it.
    public class KnowledgeFragment : MonoBehaviour, IInteractable
    {
        [Header("Fragment")]
        public string fragmentName = "the pressed-flower bookmark";

        [TextArea(2, 4)]
        public string fragmentText = "Between the pages, a jasmine flower. Mother pressed it the week I started school.";

        public QuestionCategory category = QuestionCategory.Science;
        public AudioClip pickupClip;
        public float memorySeconds = 4f;

        bool collected;

        // Stable identity for saves (restore the SAME fragment, not the first N).
        public bool Collected => collected;
        public string SaveId => fragmentName;
        bool running;

        public string Prompt => Localization.Format("Collect {0}", fragmentName);
        public bool CanInteract => !collected && !running;

        public void Interact(GameObject interactor)
        {
            if (collected || running) return;
            StartCoroutine(CollectSequence());
        }

        System.Collections.IEnumerator CollectSequence()
        {
            running = true;

            Collider selfCollider = GetComponent<Collider>();
            if (selfCollider != null) selfCollider.enabled = false;

            if (pickupClip == null) pickupClip = Core.ProceduralSfx.Pickup;
            if (pickupClip != null && AudioManager.Instance != null) AudioManager.Instance.PlaySfx(pickupClip);

            // P3 haptics: a light buzz when a memory is recovered (Android only).
            LightShadow.Platform.HapticsService.Light();

            GameManager gm = GameManager.Instance;
            // The narrator reads the memory aloud when a voice is available.
            StoryTeller.Speak(Localization.Tr(fragmentText));

            // P3 inspect view: a 360 close-up of the fragment while the memory
            // plays; the quiz waits until the player closes it.
            UI.InspectViewController.ShowFrom(this);

            // The close-up already prints the memory text; the HUD line on top of it
            // doubled the text and ran under the CLOSE button. Only use the HUD line
            // when there is no close-up.
            if (gm != null && !UI.InspectViewController.IsOpen) gm.ShowMessage(fragmentText, memorySeconds);

            // With the close-up open, its own close (button, Esc or auto) ends
            // the wait and the question follows at once. Without it, the memory
            // line plays for memorySeconds first.
            if (UI.InspectViewController.IsOpen)
            {
                while (UI.InspectViewController.IsOpen) yield return null;
            }
            else
            {
                yield return new WaitForSeconds(memorySeconds);
            }

            // The themed question. Answering or not, the fragment is recovered.
            if (gm != null && gm.quizUI != null && gm.database != null)
            {
                QuestionData question = gm.database.GetNextQuestion(category);
                if (question != null)
                {
                    // The memory is over once the close-up closes: cut its narration and
                    // HUD line so they do not talk over the question.
                    StoryTeller.Stop();
                    if (gm.hud != null) gm.hud.ClearMessage();
                    // Never stack on another open question (it would displace it).
                    while (gm.quizUI.IsOpen) yield return null;
                    bool answered = false;
                    gm.quizUI.ShowQuestion(question, (correct, chosenIndex) =>
                    {
                        if (!UI.QuizUIController.IsCancel(chosenIndex)) gm.ApplyQuizResult(question, chosenIndex, correct);
                        answered = true;
                    });

                    while (!answered) yield return null;
                }
            }

            collected = true;
            if (gm != null) gm.OnFragmentCollected();
            gameObject.SetActive(false);
        }

        // Save restore (premium pass): mark this fragment as already collected
        // and hide it, WITHOUT counting it again or replaying memory/question.
        public void RestoreFromSave()
        {
            if (collected) return;
            collected = true;

            Collider selfCollider = GetComponent<Collider>();
            if (selfCollider != null) selfCollider.enabled = false;
            gameObject.SetActive(false);
        }
    }
}
