using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightShadow.UI
{
    // Holds every question asset the game may ask and hands them out
    // one at a time without repeating until the pool is empty (a "no-repeat bag").
    [CreateAssetMenu(menuName = "Questions/New Database", fileName = "QuestionDatabase")]
    public class QuestionDatabase : ScriptableObject
    {
        public List<QuestionData> all = new List<QuestionData>();

        readonly List<QuestionData> actBag = new List<QuestionData>();
        readonly List<QuestionData> categoryBag = new List<QuestionData>();

        // Called by GameManager when a run starts so every session gets a fresh order.
        public void ResetBag()
        {
            actBag.Clear();
            categoryBag.Clear();
        }

        // Used by quiz boards. difficulty cap ramps with restored fuse boxes.
        public QuestionData GetNextQuestion(QuestionDifficulty maxDifficulty)
        {
            return Draw(actBag, q => q.difficulty <= maxDifficulty);
        }

        // Used by knowledge fragments: a question themed to the fragment.
        public QuestionData GetNextQuestion(QuestionCategory category)
        {
            return Draw(categoryBag, q => q.category == category);
        }

        QuestionData Draw(List<QuestionData> bag, Func<QuestionData, bool> filter)
        {
            if (bag.Count == 0) RefillBag(bag, filter);
            if (bag.Count == 0) RefillBag(bag, null); // pool too small for the filter
            if (bag.Count == 0) return null;

            int index = bag.Count - 1;
            QuestionData picked = bag[index];
            bag.RemoveAt(index);
            return picked;
        }

        void RefillBag(List<QuestionData> bag, Func<QuestionData, bool> filter)
        {
            bag.Clear();
            foreach (QuestionData q in all)
            {
                if (q == null || !q.IsValid) continue;
                if (filter != null && !filter(q)) continue;
                bag.Add(q);
            }
            Shuffle(bag);
        }

        static void Shuffle(List<QuestionData> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = UnityEngine.Random.Range(i, list.Count);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
