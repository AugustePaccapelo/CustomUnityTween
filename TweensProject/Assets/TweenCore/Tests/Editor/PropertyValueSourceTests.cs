using NUnit.Framework;
using UnityEngine;
using Tweening;

namespace Tweening.Tests
{
    /// <summary>
    /// Behavioural coverage of where a property's start and end values come from, written from the
    /// README's method list :
    ///
    ///   "From(value)", "FromCurrent() - reflection tweens only",
    ///   "SetIsAdditive(bool isAdd) - treats the final value as an offset; also turns FromCurrent on"
    ///
    /// FromCurrent against a live target belongs with the reflection tests, since it needs an
    /// object to read from. What is covered here is the flag it sets and the offset arithmetic,
    /// both of which are observable without a target.
    /// </summary>
    public class PropertyValueSourceTests
    {
        private const float TOLERANCE = 0.0001f;
        private const float DEGREES_TOLERANCE = 0.5f;

        private static TweenCore Run(TweenCorePropertyBase property, float deltaTime)
        {
            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(property);
            tween.Play();
            tween.Update(deltaTime);
            return tween;
        }

        // ----- Start and end values ----- \\

        [Test]
        public void StartValueAndFinalValue_AreExposed()
        {
            TweenCoreProperty<float> property = new TweenCoreProperty<float>(2f, 8f, 1f);

            Assert.That(property.StartValue, Is.EqualTo(2f).Within(TOLERANCE));
            Assert.That(property.FinalValue, Is.EqualTo(8f).Within(TOLERANCE));
        }

        [Test]
        public void From_OverridesTheStartValue()
        {
            TweenCoreProperty<float> property = new TweenCoreProperty<float>(0f, 10f, 1f).From(4f);

            Assert.That(property.StartValue, Is.EqualTo(4f).Within(TOLERANCE));
        }

        [Test]
        public void From_ChangesWhereTheAnimationBegins()
        {
            TweenCoreProperty<float> property = new TweenCoreProperty<float>(0f, 10f, 1f).From(4f);

            Run(property, 0.5f);

            // Halfway from 4 to 10 is 7, where halfway from 0 to 10 would be 5.
            Assert.That(property.CurrentValue, Is.EqualTo(7f).Within(TOLERANCE));
        }

        [Test]
        public void From_ReturnsTheSameProperty_SoCallsChain()
        {
            TweenCoreProperty<float> property = new TweenCoreProperty<float>(0f, 10f, 1f);

            Assert.AreSame(property, property.From(4f));
        }

        // ----- SetIsAdditive ----- \\

        [Test]
        public void SetIsAdditive_IsExposedAsIsIncreasingValue()
        {
            TweenCoreProperty<float> property = new TweenCoreProperty<float>(0f, 10f, 1f).SetIsAdditive(true);

            Assert.IsTrue(property.IsIncreasingValue);
        }

        [Test]
        public void SetIsAdditive_AlsoTurnsFromCurrentOn()
        {
            // "SetIsAdditive(bool isAdd) - treats the final value as an offset; also turns
            // FromCurrent on."
            TweenCoreProperty<float> property = new TweenCoreProperty<float>(0f, 10f, 1f).SetIsAdditive(true);

            Assert.IsTrue(property.FromCurrentValue);
        }

        [Test]
        public void SetIsAdditive_TreatsTheFinalValueAsAnOffset()
        {
            // Start 5, offset 3 : the property ends on 8, not on 3.
            TweenCoreProperty<float> property = new TweenCoreProperty<float>(5f, 3f, 1f).SetIsAdditive(true);

            Run(property, 1f);

            Assert.That(property.CurrentValue, Is.EqualTo(8f).Within(TOLERANCE));
        }

        [Test]
        public void WithoutSetIsAdditive_TheFinalValueIsAbsolute()
        {
            TweenCoreProperty<float> property = new TweenCoreProperty<float>(5f, 3f, 1f);

            Run(property, 1f);

            Assert.That(property.CurrentValue, Is.EqualTo(3f).Within(TOLERANCE));
        }

        [Test]
        public void SetIsAdditive_False_RestoresAbsoluteBehaviour()
        {
            TweenCoreProperty<float> property = new TweenCoreProperty<float>(5f, 3f, 1f)
                .SetIsAdditive(true)
                .SetIsAdditive(false);

            Run(property, 1f);

            Assert.That(property.CurrentValue, Is.EqualTo(3f).Within(TOLERANCE));
        }

        [Test]
        public void From_ThenAdditive_OffsetsFromTheGivenStartValue()
        {
            TweenCoreProperty<float> property = new TweenCoreProperty<float>(0f, 3f, 1f)
                .From(2f)
                .SetIsAdditive(true);

            Run(property, 1f);

            Assert.That(property.CurrentValue, Is.EqualTo(5f).Within(TOLERANCE));
        }

        [Test]
        public void Additive_HalfwayIsHalfTheOffset()
        {
            TweenCoreProperty<float> property = new TweenCoreProperty<float>(10f, 4f, 1f).SetIsAdditive(true);

            Run(property, 0.5f);

            Assert.That(property.CurrentValue, Is.EqualTo(12f).Within(TOLERANCE));
        }

        // ----- Additive across the documented types ----- \\

        [Test]
        public void Additive_OffsetsAVector3()
        {
            TweenCoreProperty<Vector3> property =
                new TweenCoreProperty<Vector3>(new Vector3(1f, 2f, 3f), new Vector3(10f, 10f, 10f), 1f)
                    .SetIsAdditive(true);

            Run(property, 1f);

            Assert.That(Vector3.Distance(property.CurrentValue, new Vector3(11f, 12f, 13f)), Is.LessThan(TOLERANCE));
        }

        [Test]
        public void Additive_OffsetsAColor()
        {
            TweenCoreProperty<Color> property =
                new TweenCoreProperty<Color>(new Color(0.1f, 0.2f, 0.3f, 1f), new Color(0.2f, 0.2f, 0.2f, 0f), 1f)
                    .SetIsAdditive(true);

            Run(property, 1f);

            Assert.That(property.CurrentValue.r, Is.EqualTo(0.3f).Within(TOLERANCE));
            Assert.That(property.CurrentValue.g, Is.EqualTo(0.4f).Within(TOLERANCE));
            Assert.That(property.CurrentValue.b, Is.EqualTo(0.5f).Within(TOLERANCE));
        }

        [Test]
        public void Additive_ComposesAQuaternionOffset()
        {
            // "For Quaternion the offset is composed rather than added, which is the rotation
            // equivalent." Starting at 90 degrees with a 90 degree offset ends at 180.
            Quaternion ninety = Quaternion.Euler(0f, 90f, 0f);
            TweenCoreProperty<Quaternion> property =
                new TweenCoreProperty<Quaternion>(ninety, ninety, 1f).SetIsAdditive(true);

            Run(property, 1f);

            Assert.That(Quaternion.Angle(property.CurrentValue, Quaternion.Euler(0f, 180f, 0f)),
                Is.LessThan(DEGREES_TOLERANCE));
        }
    }
}
