using System.Collections.Generic;
using System.Reflection;
using LightShadow.Core;
using LightShadow.UI;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    // Recap stats/body builders (static, pure): fallback lines, percent
    // rounding, per-entry ordering, the out-of-time branch, and the closing
    // line. English assertions; the original language is restored afterwards.
    public class RecapScreenStatsTests
    {
        static readonly MethodInfo BuildStatsMethod = typeof(RecapScreen)
            .GetMethod("BuildStats", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo BuildBodyMethod = typeof(RecapScreen)
            .GetMethod("BuildBody", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        GameLanguage originalLanguage;

        [SetUp]
        public void SetUp()
        {
            originalLanguage = Localization.Current;
            Localization.SetLanguage(GameLanguage.English);
        }

        [TearDown]
        public void TearDown()
        {
            Localization.SetLanguage(originalLanguage);
        }

        static string BuildStats(List<GameManager.AnswerRecord> answers)
        {
            return (string)BuildStatsMethod.Invoke(null, new object[] { answers });
        }

        static string BuildBody(List<GameManager.AnswerRecord> answers)
        {
            return (string)BuildBodyMethod.Invoke(null, new object[] { answers });
        }

        static QuestionData MakeQuestion()
        {
            QuestionData question = ScriptableObject.CreateInstance<QuestionData>();
            question.question = "2 + 2 = ?";
            question.options = new[] { "3", "4", "5", "6" };
            question.correctIndex = 1;
            question.explanation = "Two and two make four.";
            return question;
        }

        [Test]
        public void BuildStats_NullOrEmpty_ShowsFallbackLine()
        {
            Assert.AreEqual("No questions were answered tonight.", BuildStats(null));
            Assert.AreEqual("No questions were answered tonight.",
                BuildStats(new List<GameManager.AnswerRecord>()));
        }

        [Test]
        public void BuildStats_RoundsPercentAndCountsCorrect()
        {
            var answers = new List<GameManager.AnswerRecord>
            {
                new GameManager.AnswerRecord { correct = true },
                new GameManager.AnswerRecord { correct = true },
                new GameManager.AnswerRecord { correct = false },
            };
            Assert.AreEqual("Answered: 3    Correct: 2 (67%)", BuildStats(answers));
        }

        [Test]
        public void BuildStats_AllWrong_ShowsZeroPercent()
        {
            var answers = new List<GameManager.AnswerRecord>
            {
                new GameManager.AnswerRecord { correct = false },
            };
            Assert.AreEqual("Answered: 1    Correct: 0 (0%)", BuildStats(answers));
        }

        [Test]
        public void BuildBody_NullOrEmpty_ShowsFallbackAndClosingLine()
        {
            string body = BuildBody(null);
            StringAssert.Contains("Nothing was asked tonight.", body);
            StringAssert.Contains("Knowledge is the only light that follows a person into the dark.", body);
        }

        [Test]
        public void BuildBody_NumbersEntriesInOrder()
        {
            QuestionData first = MakeQuestion();
            QuestionData second = MakeQuestion();
            try
            {
                second.question = "3 + 3 = ?";
                second.options = new[] { "5", "6", "7", "8" };
                second.correctIndex = 1;
                second.explanation = "Three and three make six.";
                var answers = new List<GameManager.AnswerRecord>
                {
                    new GameManager.AnswerRecord { question = first, chosenIndex = 1, correct = true },
                    new GameManager.AnswerRecord { question = second, chosenIndex = 0, correct = false },
                };
                string body = BuildBody(answers);
                StringAssert.Contains("1. 2 + 2 = ?", body);
                StringAssert.Contains("2. 3 + 3 = ?", body);
                Assert.Less(body.IndexOf("1. 2 + 2 = ?"), body.IndexOf("2. 3 + 3 = ?"));
                StringAssert.Contains("Your answer: 4", body);
                StringAssert.Contains("Correct answer: 6", body);
                StringAssert.Contains("Two and two make four.", body);
                StringAssert.Contains("Three and three make six.", body);
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void BuildBody_TimerRanOut_ShowsOutOfTimeLine()
        {
            QuestionData question = MakeQuestion();
            try
            {
                var answers = new List<GameManager.AnswerRecord>
                {
                    new GameManager.AnswerRecord { question = question, chosenIndex = -1, correct = false },
                };
                StringAssert.Contains("Your answer: none (out of time)", BuildBody(answers));
            }
            finally
            {
                Object.DestroyImmediate(question);
            }
        }

        [Test]
        public void BuildBody_ChosenIndexOutOfRange_TreatedAsOutOfTime()
        {
            QuestionData question = MakeQuestion();
            try
            {
                var answers = new List<GameManager.AnswerRecord>
                {
                    new GameManager.AnswerRecord { question = question, chosenIndex = 9, correct = false },
                };
                StringAssert.Contains("Your answer: none (out of time)", BuildBody(answers));
            }
            finally
            {
                Object.DestroyImmediate(question);
            }
        }
    }
}
