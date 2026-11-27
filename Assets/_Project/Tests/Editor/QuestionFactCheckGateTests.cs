using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    // Release gate (audit 7.1). Question text is AI-seeded and must be checked
    // by a teacher before the game is described as accurate. This test never
    // passes a question on someone's behalf: it only counts the ones still
    // marked NEEDS-FACT-CHECK and reports the number, marking the run
    // Inconclusive while any remain.
    public class QuestionFactCheckGateTests
    {
        const string Pending = "NEEDS-FACT-CHECK";

        [Test]
        public void EveryQuestionHasAFactSourceField_AndPendingOnesAreCounted()
        {
            string[] guids = AssetDatabase.FindAssets("t:QuestionData");
            if (guids.Length == 0)
            {
                Assert.Inconclusive("No QuestionData assets found.");
                return;
            }

            int pending = 0;
            var empty = new List<string>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var question = AssetDatabase.LoadAssetAtPath<LightShadow.UI.QuestionData>(path);
                if (question == null) continue;
                if (string.IsNullOrWhiteSpace(question.factSource)) empty.Add(path);
                else if (question.factSource == Pending) pending++;
            }

            Assert.IsEmpty(empty, "Questions with an empty factSource: " + string.Join(", ", empty));
            if (pending > 0)
            {
                Assert.Inconclusive(pending + " of " + guids.Length
                    + " questions still await a teacher fact-check. Do not describe the content as verified.");
            }
        }
    }
}
