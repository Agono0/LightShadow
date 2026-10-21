using System.Collections.Generic;

namespace LightShadow.Core
{
    // The answers of the current run have to survive one scene change: 02_School
    // ends, 03_Ending opens, and the study recap still needs every question.
    // A static store is enough for a single-player, single-session game.
    public static class RunResults
    {
        public static readonly List<GameManager.AnswerRecord> Answers = new List<GameManager.AnswerRecord>();

        public static void Reset()
        {
            Answers.Clear();
        }

        public static void CopyFrom(List<GameManager.AnswerRecord> source)
        {
            Answers.Clear();
            if (source == null) return;
            for (int i = 0; i < source.Count; i++) Answers.Add(source[i]);
        }
    }
}
