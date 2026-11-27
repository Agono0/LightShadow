using LightShadow.Player;
using NUnit.Framework;

namespace LightShadow.Tests.Editor
{
    public class BreathingModelTests
    {
        static BreathingInputs Rested()
        {
            return new BreathingInputs
            {
                Stamina01 = 1f, IsSprinting = false, IsExhausted = false,
                WraithDistance = 999f, WraithAware = false, HitsRemaining = 2,
            };
        }

        [Test]
        public void WalkingIsSilent()
        {
            var model = new BreathingModel();
            for (int i = 0; i < 600; i++) model.Tick(Rested(), 0.016f);
            Assert.AreEqual(0f, model.Volume(), 0.0001f);
        }

        [Test]
        public void TwoStepsOfSprintDoNotPant()
        {
            var model = new BreathingModel();
            BreathingInputs input = Rested();
            input.IsSprinting = true;
            input.Stamina01 = 0.97f; // about 0.15 s of sprinting
            for (int i = 0; i < 10; i++) model.Tick(input, 0.016f);
            Assert.Less(model.Intensity, 0.1f);
            Assert.Less(model.Volume(), 0.07f);
        }

        [Test]
        public void LongSprintBuildsBreathOverSeconds()
        {
            var model = new BreathingModel();
            BreathingInputs input = Rested();
            input.IsSprinting = true;
            input.Stamina01 = 0.1f;
            model.Tick(input, 1f);
            float afterOne = model.Intensity;
            for (int i = 0; i < 6; i++) model.Tick(input, 1f);
            Assert.Greater(model.Intensity, afterOne);
            Assert.Greater(model.Volume(), 0.1f);
        }

        [Test]
        public void VolumeIsCapped()
        {
            var model = new BreathingModel();
            BreathingInputs input = Rested();
            input.IsExhausted = true;
            input.Stamina01 = 0f;
            for (int i = 0; i < 100; i++) model.Tick(input, 0.5f);
            Assert.LessOrEqual(model.Volume(), 0.3001f);
        }

        [Test]
        public void BreathRecoversSlowerThanItBuilds()
        {
            var model = new BreathingModel();
            BreathingInputs spent = Rested();
            spent.IsExhausted = true;
            spent.Stamina01 = 0f;
            for (int i = 0; i < 20; i++) model.Tick(spent, 0.5f);
            float peak = model.Intensity;
            model.Tick(Rested(), 2f);
            Assert.Greater(model.Intensity, peak - 0.5f, "falls by less than a rise of equal length");
            for (int i = 0; i < 40; i++) model.Tick(Rested(), 0.5f);
            Assert.AreEqual(0f, model.Intensity, 0.0001f);
        }

        [Test]
        public void UnawareWraithNeverFrightensHer()
        {
            var model = new BreathingModel();
            BreathingInputs input = Rested();
            input.WraithDistance = 2f;
            input.WraithAware = false;
            for (int i = 0; i < 100; i++) model.Tick(input, 0.1f);
            Assert.AreEqual(0f, model.Intensity, 0.0001f);
        }

        [Test]
        public void AwareWraithClosePanicsHer()
        {
            var model = new BreathingModel();
            BreathingInputs input = Rested();
            input.WraithDistance = 2f;
            input.WraithAware = true;
            for (int i = 0; i < 100; i++) model.Tick(input, 0.1f);
            Assert.Greater(model.Intensity, 0.4f);
        }

        [Test]
        public void AwareWraithOutOfRangeDoesNothing()
        {
            Assert.AreEqual(0f, BreathingModel.FearDrive(BreathingModel.FearRange + 1f, true, 2), 0.0001f);
        }

        [Test]
        public void CycleGetsFasterWithIntensity()
        {
            var calm = new BreathingModel();
            var hot = new BreathingModel();
            BreathingInputs input = Rested();
            input.IsExhausted = true;
            for (int i = 0; i < 100; i++) hot.Tick(input, 0.5f);
            Assert.Less(hot.CycleSeconds(), calm.CycleSeconds());
            Assert.Less(hot.Pitch(), 1.1f, "urgency is rate, not chipmunk pitch");
        }

        [Test]
        public void ResetSilencesImmediately()
        {
            var model = new BreathingModel();
            BreathingInputs input = Rested();
            input.IsExhausted = true;
            for (int i = 0; i < 100; i++) model.Tick(input, 0.5f);
            model.Reset();
            Assert.AreEqual(0f, model.Volume(), 0.0001f);
        }

        [Test]
        public void ZeroDeltaDoesNothing()
        {
            var model = new BreathingModel();
            BreathingInputs input = Rested();
            input.IsExhausted = true;
            model.Tick(input, 0f);
            Assert.AreEqual(0f, model.Intensity, 0.0001f);
        }
    }
}
