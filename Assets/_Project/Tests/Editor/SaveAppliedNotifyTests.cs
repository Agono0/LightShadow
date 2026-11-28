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
            Action<SaveSystem.SaveData> a = d => calls++;
            Action<SaveSystem.SaveData> b = d => calls++;
            SaveSystem.SaveApplied += a;
            SaveSystem.SaveApplied += b;
            try
            {
                Assert.DoesNotThrow(() => SaveSystem.NotifySaveApplied(new SaveSystem.SaveData()));
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
