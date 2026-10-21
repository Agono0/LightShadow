namespace LightShadow.Core
{
    // Every scene name in one place. SceneManager.LoadScene takes strings, and a
    // typo there fails silently at runtime, so all loads go through these constants.
    public static class SceneNames
    {
        public const string Bootstrap = "00_Bootstrap";
        public const string MainMenu = "01_MainMenu";
        public const string School = "02_School";
        public const string Ending = "03_Ending";
    }
}
