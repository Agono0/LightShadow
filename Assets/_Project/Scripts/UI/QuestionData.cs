using UnityEngine;

namespace LightShadow.UI
{
    public enum QuestionCategory { Science, Math, History, Geography, Language, Health }

    // Doc 4.1: questions carry a difficulty tag so the quiz ramps
    // Act I (recall) -> Act II (apply) -> Act III (mixed).
    public enum QuestionDifficulty { ActI = 1, ActII = 2, ActIII = 3 }

    // One quiz question as a ScriptableObject asset (Q_Sci_001.asset style).
    // Create through: Assets > Create > Questions > New Question.
    [CreateAssetMenu(menuName = "Questions/New Question", fileName = "Q_New_001")]
    public class QuestionData : ScriptableObject
    {
        [TextArea(2, 4)] public string question;
        public string[] options = new string[4];
        public int correctIndex;
        public QuestionCategory category = QuestionCategory.Science;
        public QuestionDifficulty difficulty = QuestionDifficulty.ActI;
        [TextArea(1, 3)] public string explanation;
        public string factSource = "NEEDS-FACT-CHECK";

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(question) &&
            options != null && options.Length == 4 &&
            correctIndex >= 0 && correctIndex < 4 &&
            !string.IsNullOrWhiteSpace(options[0]) &&
            !string.IsNullOrWhiteSpace(options[1]) &&
            !string.IsNullOrWhiteSpace(options[2]) &&
            !string.IsNullOrWhiteSpace(options[3]);

        // Keep the data safe: exactly four options, index inside range.
        void OnValidate()
        {
            if (options == null || options.Length != 4)
            {
                string[] fixedOptions = new string[4];
                if (options != null)
                {
                    for (int i = 0; i < options.Length && i < 4; i++) fixedOptions[i] = options[i];
                }
                options = fixedOptions;
            }
            correctIndex = Mathf.Clamp(correctIndex, 0, 3);
        }
    }
}
