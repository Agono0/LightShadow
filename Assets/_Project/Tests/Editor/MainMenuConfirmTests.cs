using NUnit.Framework;
using LightShadow.UI;

namespace LightShadow.Tests
{
    // Covers the confirm-key decision only, not the EventSystem half (U1).
    public class MainMenuConfirmTests
    {
        [Test] public void Bare_menu_plays() { Assert.AreEqual(MainMenuController.ConfirmAction.Play, MainMenuController.DecideConfirm(false, false, false, false)); }
        [Test] public void Open_howto_closes_first() { Assert.AreEqual(MainMenuController.ConfirmAction.CloseHowTo, MainMenuController.DecideConfirm(false, false, true, false)); }
        [Test] public void Open_credits_closes_first() { Assert.AreEqual(MainMenuController.ConfirmAction.CloseCredits, MainMenuController.DecideConfirm(false, false, false, true)); }
        [Test] public void Settings_and_saves_own_their_keys()
        {
            Assert.AreEqual(MainMenuController.ConfirmAction.None, MainMenuController.DecideConfirm(true, false, false, false));
            Assert.AreEqual(MainMenuController.ConfirmAction.None, MainMenuController.DecideConfirm(false, true, true, false));
        }
    }
}
