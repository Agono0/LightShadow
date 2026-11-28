using NUnit.Framework;
using LightShadow.Core;

namespace LightShadow.Tests
{
    public class SaveLayoutVersionTests
    {
        [Test]
        public void LegacySave_withoutLayoutVersion_keepsPosition()
        {
            Assert.IsTrue(SaveSystem.HasUsablePosition(new SaveSystem.SaveData { layoutVersion = 0 }));
        }

        [Test]
        public void CurrentLayout_keepsPosition()
        {
            Assert.IsTrue(SaveSystem.HasUsablePosition(new SaveSystem.SaveData { layoutVersion = SaveSystem.LayoutVersion }));
        }

        [Test]
        public void OtherLayout_dropsPosition()
        {
            Assert.IsFalse(SaveSystem.HasUsablePosition(new SaveSystem.SaveData { layoutVersion = SaveSystem.LayoutVersion + 1 }));
        }
    }
}
