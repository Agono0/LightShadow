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

            GameManager gm = GameManager.Instance;
            if (gm != null) gm.ShowMessage(fragmentText, memorySeconds);

            // The narrator reads the memory aloud when a voice is available.
            StoryTeller.Speak(Localization.Tr(fragmentText));

            yield return new WaitForSeconds(memorySeconds);

            // The themed question. Answering or not, the fragment is recovered.
            if (gm != null && gm.quizUI != null && gm.database != null)
            {
                QuestionData question = gm.database.GetNextQuestion(category);
                if (question != null)
                {
                    bool answered = false;
                    gm.quizUI.ShowQuestion(question, (correct, chosenIndex) =>
                    {
                        gm.ApplyQuizResult(question, chosenIndex, correct);
                        answered = true;
                    });

                    while (!answered) yield return null;
                }
            }

            collected = true;
            if (gm != null) gm.OnFragmentCollected();
            gameObject.SetActive(false);
        }
    }
}
