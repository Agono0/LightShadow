using LightShadow.Player;
using NUnit.Framework;
using UnityEngine;

namespace LightShadow.Tests.Editor
{
    public class TouchJoystickShapeTests
    {
        [Test]
        public void InsideDeadZone_IsZero()
        {
            Assert.AreEqual(Vector2.zero, TouchJoystick.Shape(new Vector2(8f, 0f), 100f, 0.12f));
        }

        [Test]
        public void AtTheRim_IsFullSpeed_InTheSameDirection()
        {
            Vector2 result = TouchJoystick.Shape(new Vector2(0f, 100f), 100f, 0.12f);
            Assert.AreEqual(1f, result.magnitude, 0.001f);
            Assert.AreEqual(1f, result.y, 0.001f);
            Vector2 beyond = TouchJoystick.Shape(new Vector2(0f, 400f), 100f, 0.12f);
            Assert.AreEqual(1f, beyond.magnitude, 0.001f);
        }

        [Test]
        public void Output_IsMonotonic_AndNeverAboveInput()
        {
            float previous = -1f;
            for (int i = 0; i <= 20; i++)
            {
                float m = TouchJoystick.Shape(new Vector2(i * 5f, 0f), 100f, 0.12f).magnitude;
                Assert.GreaterOrEqual(m, previous);
                Assert.LessOrEqual(m, i * 5f / 100f + 0.001f);
                previous = m;
            }
        }

        [Test]
        public void ZeroRadius_IsSafe()
        {
            Assert.AreEqual(Vector2.zero, TouchJoystick.Shape(new Vector2(5f, 5f), 0f, 0.12f));
        }
    }
}
