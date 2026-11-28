using LightShadow.UI;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class SettingsHapticsRowTests
    {
        [Test]
        public void HapticsRow_IsAppendedAndInRange()
        {
            Assert.AreEqual(27, SettingsPanelController.HapticsRow);
            Assert.AreEqual(SettingsPanelController.HapticsRow + 1, SettingsPanelController.TotalRows);
            Assert.GreaterOrEqual(SettingsPanelController.HapticsRow, SettingsPanelController.PageARows);
        }

        [Test]
        public void ExistingRowIndices_DidNotMove()
        {
            Assert.AreEqual(22, SettingsPanelController.ControlSizeRow);
            Assert.AreEqual(25, SettingsPanelController.VolumeVoiceRow);
            Assert.AreEqual(26, SettingsPanelController.VolumeShadowRow);
        }

        [Test]
        public void MobileOnlyRows_AreControlSizeAndHaptics()
        {
            Assert.IsTrue(SettingsPanelController.IsMobileOnlyRow(SettingsPanelController.ControlSizeRow));
            Assert.IsTrue(SettingsPanelController.IsMobileOnlyRow(SettingsPanelController.HapticsRow));
            Assert.IsFalse(SettingsPanelController.IsMobileOnlyRow(SettingsPanelController.CaptionsRow));
        }
    }
}
