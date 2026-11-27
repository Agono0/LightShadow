using System.Reflection;
using LightShadow.Core;
using LightShadow.Story;
using LightShadow.UI;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    // audit v3 5.5: the save-slot fragment line must take its target from
    // GameManager.FragmentTarget, not a literal "/6" baked beside the call,
    // so the UI cannot desync when the target moves.
    public class SaveSlotUIFragmentTargetTests
    {
        static readonly MethodInfo FormatFragmentsMethod =
            typeof(SaveSlotUI).GetMethod("FormatFragments", BindingFlags.NonPublic | BindingFlags.Static);
        static readonly MethodInfo DescribeMethod =
            typeof(SaveSlotUI).GetMethod("Describe", BindingFlags.NonPublic | BindingFlags.Instance);

        static string FormatFragments(int collected)
        {
            return (string)FormatFragmentsMethod.Invoke(null, new object[] { collected });
        }

        [Test]
        public void FormatFragments_UsesGameManagerFragmentTarget()
        {
            string line = FormatFragments(3);

            string expectedTemplate = StoryStrings.Tr("Fragments: {0}/6")
                .Replace("/6", "/" + GameManager.FragmentTarget);
            Assert.AreEqual(string.Format(expectedTemplate, 3), line);
            StringAssert.Contains("/" + GameManager.FragmentTarget, line);
        }

        [Test]
        public void Describe_UsesSharedFragmentFormatter()
        {
            var host = new GameObject("SaveSlotUITest", typeof(SaveSlotUI));
            try
            {
                var data = new SaveSystem.SaveData
                {
                    fusesRestored = 0,
                    fragmentsCollected = 4,
                    savedAtUtc = "",
                };
                string description = (string)DescribeMethod.Invoke(
                    host.GetComponent<SaveSlotUI>(), new object[] { data });
                StringAssert.Contains(FormatFragments(4), description);
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }
    }
}
