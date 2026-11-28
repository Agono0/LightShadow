using NUnit.Framework;
using UnityEngine;
using LightShadow.Gameplay;
using LightShadow.Player;

namespace LightShadow.Tests
{
    public class CrankPropResetTests
    {
        const float DeadZone = 4f; // GameConfig.crankGestureDeadZone default

        [Test]
        public void Stick_pushed_registers_after_scaling()
        {
            Assert.IsTrue(FlashlightCrank.RegistersMotion(FlashlightCrank.StickToPixels(new Vector2(0.6f, 0.6f)), DeadZone));
            Assert.IsFalse(FlashlightCrank.RegistersMotion(new Vector2(0.6f, 0.6f), DeadZone), "unscaled stick can never pass (the bug)");
        }

        [Test]
        public void Stick_jitter_does_not_register()
        {
            Assert.IsFalse(FlashlightCrank.RegistersMotion(FlashlightCrank.StickToPixels(new Vector2(0.05f, 0.05f)), DeadZone));
        }

        [Test]
        public void ResetCarried_withNothingCarried_isSafeAndFreesF()
        {
            Assert.DoesNotThrow(() => ThrowableProp.ResetCarried());
            Assert.IsFalse(ThrowableProp.OwnsFKey);
        }
    }
}
