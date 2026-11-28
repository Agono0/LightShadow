using NUnit.Framework;
using UnityEngine;
using LightShadow.Core;
using LightShadow.UI;

namespace LightShadow.Tests
{
    // S1: a board answer goes through GameManager.ApplyQuizResult, which must
    // raise QuestionAnswered once and record the answer once.
    public class QuizBoardRoutingTests
    {
        [Test]
        public void ApplyQuizResult_RaisesEventOnce_AndRecordsOnce()
        {
            GameObject host = new GameObject("QBR_Host");
            host.SetActive(false);
            QuestionData question = ScriptableObject.CreateInstance<QuestionData>();
            int raised = 0;
            bool lastCorrect = false;
            System.Action<bool> handler = c => { raised++; lastCorrect = c; };
            GameEvents.QuestionAnswered += handler;
            try
            {
                GameManager gm = host.AddComponent<GameManager>();
                int before = gm.Answers.Count;
                gm.ApplyQuizResult(question, 2, true);
                Assert.AreEqual(1, raised);
                Assert.IsTrue(lastCorrect);
                Assert.AreEqual(before + 1, gm.Answers.Count);
            }
            finally
            {
                GameEvents.QuestionAnswered -= handler;
                Object.DestroyImmediate(question);
                Object.DestroyImmediate(host);
            }
        }
    }
}
