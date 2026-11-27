using System.Collections.Generic;
using LightShadow.Core;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    // GAP COVERED: RunResults (Scripts/Core/RunResults.cs), the static store that
    // carries quiz answers from 02_School to the 03_Ending recap, has no tests.
    // UNVERIFIED LIMITS: records are built with a null QuestionData reference
    // (struct fields only); the recap UI that consumes the list is not covered.
    // Not run in an editor by the author; CI test-editmode is the first run.
    public class RunResultsTests
    {
        [SetUp]
        public void SetUp() { RunResults.Reset(); }

        [TearDown]
        public void TearDown() { RunResults.Reset(); }

        static GameManager.AnswerRecord Record(int chosen, bool correct)
        {
            return new GameManager.AnswerRecord { question = null, chosenIndex = chosen, correct = correct };
        }

        [Test]
        public void CopyFrom_CopiesAllRecordsInOrder()
        {
            var source = new List<GameManager.AnswerRecord> { Record(1, true), Record(-1, false), Record(3, false) };
            RunResults.CopyFrom(source);
            Assert.AreEqual(3, RunResults.Answers.Count);
            Assert.AreEqual(1, RunResults.Answers[0].chosenIndex);
            Assert.AreEqual(-1, RunResults.Answers[1].chosenIndex);
            Assert.IsFalse(RunResults.Answers[2].correct);
        }

        [Test]
        public void CopyFrom_ReplacesPreviousContent()
        {
            RunResults.CopyFrom(new List<GameManager.AnswerRecord> { Record(0, true), Record(1, true) });
            RunResults.CopyFrom(new List<GameManager.AnswerRecord> { Record(2, false) });
            Assert.AreEqual(1, RunResults.Answers.Count);
            Assert.AreEqual(2, RunResults.Answers[0].chosenIndex);
        }

        [Test]
        public void CopyFrom_IsACopy_NotAnAlias()
        {
            var source = new List<GameManager.AnswerRecord> { Record(0, true) };
            RunResults.CopyFrom(source);
            source.Add(Record(1, false));
            Assert.AreEqual(1, RunResults.Answers.Count);
        }

        [Test]
        public void CopyFrom_Null_ClearsWithoutThrowing()
        {
            RunResults.CopyFrom(new List<GameManager.AnswerRecord> { Record(0, true) });
            Assert.DoesNotThrow(() => RunResults.CopyFrom(null));
            Assert.AreEqual(0, RunResults.Answers.Count);
        }

        [Test]
        public void Reset_ClearsAnswers()
        {
            RunResults.CopyFrom(new List<GameManager.AnswerRecord> { Record(0, true) });
            RunResults.Reset();
            Assert.AreEqual(0, RunResults.Answers.Count);
        }
    }
}
