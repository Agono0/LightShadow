using LightShadow.Core;
using LightShadow.Gameplay;
using LightShadow.Player;
using UnityEngine;

namespace LightShadow.UI
{
    // Doc 4.4: wall-mounted chalkboards place quizzes in the world. They can be
    // answered as often as the player likes - knowledge never locks a door.
    // The difficulty ramps with restored fuse boxes (GameManager.CurrentDifficulty).
    public class QuizBoard : MonoBehaviour, IInteractable
    {
        [Header("Wiring")]
        public QuestionDatabase database;
        public BrightIdeaMeter meter;

        bool asking;

        public string Prompt => "Read the chalkboard question";
        public bool CanInteract => !asking;

        void Awake()
        {
            if (database == null && GameManager.Instance != null) database = GameManager.Instance.database;
        }

        public void Interact(GameObject interactor)
        {
            if (asking) return;

            GameManager gm = GameManager.Instance;
            if (gm == null || gm.quizUI == null || gm.database == null) return;
            if (gm.quizUI.IsOpen) return;

            QuestionData question = gm.database.GetNextQuestion(gm.CurrentDifficulty);
            if (question == null) return;

            asking = true;
            gm.quizUI.ShowQuestion(question, (correct, chosenIndex) =>
            {
                // Doc 4.1: the meter responds immediately; the recap remembers everything.
                BrightIdeaMeter target = meter != null ? meter : gm.GetMeter();
                if (target != null)
                {
                    if (correct) target.AddCorrect();
                    else target.AddWrong();
                }
                gm.RecordAnswer(question, chosenIndex, correct);
                asking = false;
            });
        }
    }
}
