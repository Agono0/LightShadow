using System.Reflection;
using LightShadow.Audio;
using NUnit.Framework;

namespace LightShadow.Contrib.Ahmed.Tests.Editor
{
    // Gain and clamp contracts for VolumeCategories: the slider-level -> dB
    // curve (LevelToDb) and the 0..1 category gains heard under Master
    // (CategoryVolume01 / MasterVolume01) on the legacy multiply path.
    //
    // Isolation: the only static state touched is the private static float[]
    // levels. SetUp copies the seven current values out; tests mutate array
    // elements in place; TearDown writes the original seven back. SetLevel,
    // Bootstrap, ScanActiveScene, Register and SetMixer are never called, so
    // PlayerPrefs is never read or written, no scene scan runs, no source is
    // touched and CategoryChanged never fires. No assertion depends on the
    // mixer static (these paths never read it), so the tests are independent
    // of mixer state either way.
    //
    // UNRUN: never compiled or run here; static expected-pass is not a pass.
    public class VolumeLevelContractTests
    {
        static readonly MethodInfo LevelToDbMethod =
            typeof(VolumeCategories).GetMethod("LevelToDb", BindingFlags.NonPublic | BindingFlags.Static);
        static readonly FieldInfo LevelsField =
            typeof(VolumeCategories).GetField("levels", BindingFlags.NonPublic | BindingFlags.Static);

        float[] levels;
        float[] snapshot;

        [SetUp]
        public void SetUp()
        {
            Assert.NotNull(LevelToDbMethod, "VolumeCategories.LevelToDb not found; it may have been renamed");
            Assert.NotNull(LevelsField, "VolumeCategories.levels not found; it may have been renamed");
            levels = (float[])LevelsField.GetValue(null);
            snapshot = (float[])levels.Clone();
        }

        [TearDown]
        public void TearDown()
        {
            // Restore the levels array directly; nothing else was mutated.
            if (levels == null || snapshot == null) return;
            for (int i = 0; i < snapshot.Length; i++)
            {
                levels[i] = snapshot[i];
            }
        }

        static float LevelToDb(float level)
        {
            return (float)LevelToDbMethod.Invoke(null, new object[] { level });
        }

        [Test]
        public void LevelToDb_ZeroIsSilent_TenIsUnity()
        {
            Assert.AreEqual(-80f, LevelToDb(0f), "slider 0 must be silent like the old 0");
            Assert.AreEqual(0f, LevelToDb(10f), 0.0001f, "slider 10 must be unity gain");
        }

        [Test]
        public void LevelToDb_HalfIsAboutMinus6Db()
        {
            Assert.AreEqual(-6.0206f, LevelToDb(5f), 0.001f, "20*log10(0.5)");
        }

        [Test]
        public void LevelToDb_MonotonicAcrossSlider()
        {
            float previous = LevelToDb(0f);
            for (int level = 1; level <= 10; level++)
            {
                float current = LevelToDb(level);
                Assert.GreaterOrEqual(current, previous, "dB must not decrease as the slider rises (level " + level + ")");
                previous = current;
            }
        }

        [Test]
        public void CategoryVolume01_ClampsZeroOne()
        {
            levels[(int)VolumeCategories.Category.Voice] = 0f;
            levels[(int)VolumeCategories.Category.Shadow] = 10f;

            Assert.AreEqual(0f, VolumeCategories.CategoryVolume01(VolumeCategories.Category.Voice));
            Assert.AreEqual(1f, VolumeCategories.CategoryVolume01(VolumeCategories.Category.Shadow));
        }

        [Test]
        public void VoiceAndShadowLevels_AreIndependent()
        {
            levels[(int)VolumeCategories.Category.Voice] = 3f;
            levels[(int)VolumeCategories.Category.Shadow] = 8f;

            Assert.AreEqual(0.3f, VolumeCategories.CategoryVolume01(VolumeCategories.Category.Voice), 0.0001f);
            Assert.AreEqual(0.8f, VolumeCategories.CategoryVolume01(VolumeCategories.Category.Shadow), 0.0001f);

            levels[(int)VolumeCategories.Category.Voice] = 9f;

            Assert.AreEqual(0.9f, VolumeCategories.CategoryVolume01(VolumeCategories.Category.Voice), 0.0001f);
            Assert.AreEqual(0.8f, VolumeCategories.CategoryVolume01(VolumeCategories.Category.Shadow), 0.0001f,
                "changing the Voice level must not move the Shadow bus");
        }

        [Test]
        public void MasterVolume01_ReflectsMasterOnly()
        {
            levels[(int)VolumeCategories.Category.Master] = 5f;
            levels[(int)VolumeCategories.Category.Music] = 0f;
            levels[(int)VolumeCategories.Category.Voice] = 10f;

            Assert.AreEqual(0.5f, VolumeCategories.MasterVolume01, 0.0001f,
                "MasterVolume01 must follow the master level alone");
        }
    }
}
