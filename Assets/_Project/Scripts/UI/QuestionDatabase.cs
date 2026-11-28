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
            asked.Clear();
        }

        // Questions already drawn this run, by asset name. A save keeps them so a
        // loaded run does not repeat what the player already answered.
        readonly HashSet<string> asked = new HashSet<string>();
        public void MarkAsked(string questionName)
        {
            if (!string.IsNullOrEmpty(questionName)) asked.Add(questionName);
        }

        public QuestionData FindByName(string questionName)
        {
            if (string.IsNullOrEmpty(questionName)) return null;
            for (int i = 0; i < all.Count; i++) if (all[i] != null && all[i].name == questionName) return all[i];
            return null;
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
            asked.Add(picked.name);
            return picked;
        }

        void RefillBag(List<QuestionData> bag, Func<QuestionData, bool> filter)
        {
            bag.Clear();
            for (int pass = 0; pass < 2 && bag.Count == 0; pass++)
            {
                // Pass 0 skips what this run already asked; if that empties the pool
                // the second pass allows repeats again.
                foreach (QuestionData q in all)
                {
                    if (q == null || !q.IsValid) continue;
                    if (filter != null && !filter(q)) continue;
                    if (pass == 0 && asked.Contains(q.name)) continue;
                    bag.Add(q);
                }
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
