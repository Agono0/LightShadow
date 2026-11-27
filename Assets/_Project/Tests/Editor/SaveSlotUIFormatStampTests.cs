using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using LightShadow.UI;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    // SaveSlotUI.FormatStamp: the slot row's "Saved {0}" timestamp. Empty
    // stamps dash out, unparseable stamps pass through verbatim, valid UTC
    // stamps render local "yyyy-MM-dd HH:mm".
    public class SaveSlotUIFormatStampTests
    {
        static readonly MethodInfo FormatStampMethod = typeof(SaveSlotUI)
            .GetMethod("FormatStamp", BindingFlags.NonPublic | BindingFlags.Static);

        static string FormatStamp(string savedAtUtc)
        {
            return (string)FormatStampMethod.Invoke(null, new object[] { savedAtUtc });
        }

        [Test]
        public void NullOrEmpty_RendersDash()
        {
            Assert.AreEqual("-", FormatStamp(null));
            Assert.AreEqual("-", FormatStamp(""));
        }

        [Test]
        public void Unparseable_PassesThroughVerbatim()
        {
            Assert.AreEqual("not-a-date", FormatStamp("not-a-date"));
        }

        [Test]
        public void ValidUtc_RendersLocalTimestampShape()
        {
            const string stamp = "2026-11-27T21:30:00Z";
            string rendered = FormatStamp(stamp);

            StringAssert.IsMatch(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}$", rendered);

            // Timezone-independent expectation: same documented conversion.
            DateTime parsed = DateTime.Parse(stamp, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            Assert.AreEqual(parsed.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                rendered);
        }
    }
}
