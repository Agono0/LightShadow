using LightShadow.UI;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    // Validation rules of QuestionData (IsValid / HasArabic / OnValidate
    // normalisation) on in-memory instances, plus a sweep over the shipped
    // question assets.
    public class QuestionDataTests
    {
        QuestionData question;

        [SetUp]
        public void SetUp()
        {
            question = ScriptableObject.CreateInstance<QuestionData>();
            question.question = "What is 2 + 2?";
            question.options = new[] { "3", "4", "5", "6" };
            question.correctIndex = 1;
            question.questionAr = "ما ناتج 2 + 2؟";
            question.optionsAr = new[] { "٣", "٤", "٥", "٦" };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(question);
        }

        [Test]
        public void IsValid_TrueForCompleteQuestion()
        {
            Assert.IsTrue(question.IsValid);
            Assert.IsTrue(question.HasArabic);
        }

        [Test]
        public void IsValid_FalseForBlankQuestionText()
        {
            question.question = "  ";
            Assert.IsFalse(question.IsValid);
        }

        [Test]
        public void IsValid_FalseForWrongOptionCount()
        {
            question.options = new[] { "a", "b", "c" };
            Assert.IsFalse(question.IsValid);
        }

        [Test]
        public void IsValid_FalseForBlankOption()
        {
            question.options = new[] { "a", "", "c", "d" };
            Assert.IsFalse(question.IsValid);
        }

        [Test]
        public void IsValid_FalseForCorrectIndexOutOfRange()
        {
            question.correctIndex = 4;
            Assert.IsFalse(question.IsValid);
            question.correctIndex = -1;
            Assert.IsFalse(question.IsValid);
        }

        [Test]
        public void HasArabic_FalseWhenAnyArabicFieldMissing()
        {
            question.questionAr = "";
            Assert.IsFalse(question.HasArabic);

            question.questionAr = "ما ناتج 2 + 2؟";
            question.optionsAr = new[] { "٣", "٤", "٥", null };
            Assert.IsFalse(question.HasArabic);

            question.optionsAr = new[] { "٣", "٤" };
            Assert.IsFalse(question.HasArabic);
        }

        [Test]
        public void ShippedQuestionAssets_AreValid_TwoLanguage_AndDraftMarked()
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:QuestionData", new[] { "Assets/_Project/Data/Questions" });
            Assert.Greater(guids.Length, 0, "no question assets found");

            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                QuestionData asset = UnityEditor.AssetDatabase.LoadAssetAtPath<QuestionData>(path);
                Assert.NotNull(asset, path);
                Assert.IsTrue(asset.IsValid, path + " is not valid");
                Assert.IsTrue(asset.HasArabic, path + " lacks Arabic fields");
                Assert.IsFalse(string.IsNullOrWhiteSpace(asset.explanation), path + " lacks explanation");
                Assert.IsFalse(string.IsNullOrWhiteSpace(asset.explanationAr), path + " lacks explanationAr");
            }
        }

        [Test]
        public void ShippedQuestionAssets_HaveDistinctOptionsWithinEachQuestion()
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:QuestionData", new[] { "Assets/_Project/Data/Questions" });
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                QuestionData asset = UnityEditor.AssetDatabase.LoadAssetAtPath<QuestionData>(path);
                var seen = new System.Collections.Generic.HashSet<string>();
                foreach (string option in asset.options)
                {
                    Assert.IsTrue(seen.Add(option.Trim()), path + " repeats option '" + option + "'");
                }
            }
        }
    }
}
