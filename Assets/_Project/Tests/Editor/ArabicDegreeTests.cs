using LightShadow.Core;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class ArabicDegreeTests
    {
        [Test]
        public void DegreeSignStaysGluedToItsNumber()
        {
            // "<arabic word> 90<degree>": the number and its sign must stay one
            // left-to-right unit, in that order, after the line is reversed.
            string fixedLine = ArabicSupport.Fix("\u062F\u0648\u0631 90\u00B0");
            StringAssert.Contains("90\u00B0", fixedLine);
            StringAssert.DoesNotContain("\u00B009", fixedLine);
            StringAssert.DoesNotContain("\u00B090", fixedLine);
        }

        [Test]
        public void PlainNumbersKeepTheirDigitOrder()
        {
            StringAssert.Contains("180", ArabicSupport.Fix("\u0632\u0627\u0648\u064A\u0629 180"));
        }
    }
}
