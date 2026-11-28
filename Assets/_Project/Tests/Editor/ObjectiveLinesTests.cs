using LightShadow.Core;
using LightShadow.UI;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class ObjectiveLinesTests
    {
        static string[] Lines(int fuses, bool key, bool wired, bool powered, int frags = 0)
        {
            return HUDController.ObjectiveLines(fuses, 3, frags, 6, key, wired, powered);
        }

        [Test]
        public void FewFuses_GateStepSaysFusesFirst()
        {
            Assert.AreEqual("Power the main gate  (three fuse boxes first)", Lines(1, false, false, false)[2]);
        }

        [Test]
        public void AllFuses_NoKey_SendsPlayerToTheKey()
        {
            Assert.AreEqual("Find the master key (staff room locker)", Lines(3, false, false, false)[2]);
        }

        [Test]
        public void AllFuses_WithKey_SaysWireTheConsole()
        {
            Assert.AreEqual("Wire the gate console", Lines(3, true, false, false)[2]);
        }

        [Test]
        public void Wired_CrankIsLive()
        {
            Assert.AreEqual("Power the main gate  (the crank is live)", Lines(3, true, true, false)[2]);
        }

        [Test]
        public void Powered_ShowsDoneAndGoNow()
        {
            string[] lines = Lines(3, true, true, true);
            Assert.AreEqual("Power the main gate  (done)", lines[2]);
            Assert.AreEqual("Escape through the main gate  (go now)", lines[3]);
        }

        [Test]
        public void FragmentsAreMarkedOptional_UntilDone()
        {
            StringAssert.Contains("optional", Lines(0, false, false, false, 2)[1]);
            StringAssert.Contains("done", Lines(0, false, false, false, 6)[1]);
        }

        [Test]
        public void EveryLine_HasAnArabicRow()
        {
            Localization.SetLanguage(GameLanguage.Arabic);
            try
            {
                foreach (bool powered in new[] { false, true })
                foreach (int fuses in new[] { 0, 3 })
                foreach (bool key in new[] { false, true })
                foreach (bool wired in new[] { false, true })
                foreach (int frags in new[] { 0, 6 })
                {
                    foreach (string line in Lines(fuses, key, wired, powered, frags))
                    {
                        Assert.AreNotEqual(line, Localization.Get(line), "no Arabic row for: " + line);
                    }
                }
            }
            finally { Localization.SetLanguage(GameLanguage.English); }
        }
    }
}
