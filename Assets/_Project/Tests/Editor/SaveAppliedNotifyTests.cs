using System;
using LightShadow.Core;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class SaveAppliedNotifyTests
    {
        [Test]
        public void NotifySaveApplied_RunsEveryHandler_AndSurvivesNoHud()
        {
            int calls = 0;
            Action<SaveData> a = d => calls++;
            Action<SaveData> b = d => calls++;
            SaveSystem.SaveApplied += a;
            SaveSystem.SaveApplied += b;
            try
            {
                Assert.DoesNotThrow(() => SaveSystem.NotifySaveApplied(new SaveData()));
                Assert.AreEqual(2, calls);
            }
            finally
            {
                SaveSystem.SaveApplied -= a;
                SaveSystem.SaveApplied -= b;
            }
        }
    }
}
