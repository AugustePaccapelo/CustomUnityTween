using NUnit.Framework;
using UnityEngine;
using Tweening;

namespace Tweening.Tests
{
    /// <summary>
    /// Behavioural coverage of the interpolation layer, written from the README's "Supported
    /// types" section rather than from the implementation :
    ///
    ///   "C# : float, double, int, uint, long, ulong, decimal.
    ///    Unity : Vector2, Vector3, Vector4, Quaternion, Color, Color32.
    ///    All of these work with SetIsAdditive. For Quaternion the offset is composed rather than
    ///    added, which is the rotation equivalent."
    ///
    /// TweenCoreOpsTests already covers the integer rounding and clamping rules and the Color32
    /// additive clamp. This file covers the interpolation of the remaining types, the endpoints,
    /// and the two Quaternion rules.
    /// </summary>
    public class TweenCoreOpsBehaviourTests
    {
        private const float TOLERANCE = 0.0001f;
        private const float DEGREES_TOLERANCE = 0.5f;

        // ----- The documented type table ----- \\

        [Test]
        public void SupportsAdditive_IsTrueForEveryDocumentedType()
        {
            // "All of these work with SetIsAdditive" - all thirteen, not just the sampled five.
            Assert.IsTrue(TweenCoreOps<float>.SupportsAdditive, "float");
            Assert.IsTrue(TweenCoreOps<double>.SupportsAdditive, "double");
            Assert.IsTrue(TweenCoreOps<int>.SupportsAdditive, "int");
            Assert.IsTrue(TweenCoreOps<uint>.SupportsAdditive, "uint");
            Assert.IsTrue(TweenCoreOps<long>.SupportsAdditive, "long");
            Assert.IsTrue(TweenCoreOps<ulong>.SupportsAdditive, "ulong");
            Assert.IsTrue(TweenCoreOps<decimal>.SupportsAdditive, "decimal");
            Assert.IsTrue(TweenCoreOps<Vector2>.SupportsAdditive, "Vector2");
            Assert.IsTrue(TweenCoreOps<Vector3>.SupportsAdditive, "Vector3");
            Assert.IsTrue(TweenCoreOps<Vector4>.SupportsAdditive, "Vector4");
            Assert.IsTrue(TweenCoreOps<Quaternion>.SupportsAdditive, "Quaternion");
            Assert.IsTrue(TweenCoreOps<Color>.SupportsAdditive, "Color");
            Assert.IsTrue(TweenCoreOps<Color32>.SupportsAdditive, "Color32");
        }

        [Test]
        public void AnUndocumentedType_SupportsNeitherLerpNorAdditive()
        {
            Assert.IsFalse(TweenCoreOps<Vector2Int>.IsSupported, "Vector2Int is not in the README's table.");
            Assert.IsFalse(TweenCoreOps<Vector2Int>.SupportsAdditive);
        }

        // ----- Interpolation ----- \\

        [Test]
        public void Float_InterpolatesLinearly()
        {
            Assert.That(TweenCoreOps<float>.Lerp(2f, 6f, 0.25f), Is.EqualTo(3f).Within(TOLERANCE));
            Assert.That(TweenCoreOps<float>.Lerp(2f, 6f, 0.5f), Is.EqualTo(4f).Within(TOLERANCE));
        }

        [Test]
        public void Double_InterpolatesLinearly()
        {
            Assert.That(TweenCoreOps<double>.Lerp(2d, 6d, 0.5f), Is.EqualTo(4d).Within(TOLERANCE));
        }

        [Test]
        public void Vector2_InterpolatesComponentwise()
        {
            Vector2 mid = TweenCoreOps<Vector2>.Lerp(new Vector2(0f, 10f), new Vector2(4f, 20f), 0.5f);

            Assert.That(mid.x, Is.EqualTo(2f).Within(TOLERANCE));
            Assert.That(mid.y, Is.EqualTo(15f).Within(TOLERANCE));
        }

        [Test]
        public void Vector4_InterpolatesComponentwise()
        {
            Vector4 mid = TweenCoreOps<Vector4>.Lerp(Vector4.zero, new Vector4(2f, 4f, 6f, 8f), 0.5f);

            Assert.That(mid.x, Is.EqualTo(1f).Within(TOLERANCE));
            Assert.That(mid.y, Is.EqualTo(2f).Within(TOLERANCE));
            Assert.That(mid.z, Is.EqualTo(3f).Within(TOLERANCE));
            Assert.That(mid.w, Is.EqualTo(4f).Within(TOLERANCE));
        }

        [Test]
        public void Color_InterpolatesComponentwise()
        {
            Color mid = TweenCoreOps<Color>.Lerp(Color.black, Color.white, 0.5f);

            Assert.That(mid.r, Is.EqualTo(0.5f).Within(TOLERANCE));
            Assert.That(mid.g, Is.EqualTo(0.5f).Within(TOLERANCE));
            Assert.That(mid.b, Is.EqualTo(0.5f).Within(TOLERANCE));
            Assert.That(mid.a, Is.EqualTo(1f).Within(TOLERANCE));
        }

        [Test]
        public void Lerp_AtTheEndpoints_ReturnsThoseValuesExactly()
        {
            Assert.That(TweenCoreOps<float>.Lerp(2f, 6f, 0f), Is.EqualTo(2f).Within(TOLERANCE));
            Assert.That(TweenCoreOps<float>.Lerp(2f, 6f, 1f), Is.EqualTo(6f).Within(TOLERANCE));

            Vector3 start = TweenCoreOps<Vector3>.Lerp(Vector3.zero, Vector3.one, 0f);
            Vector3 end = TweenCoreOps<Vector3>.Lerp(Vector3.zero, Vector3.one, 1f);

            Assert.That(Vector3.Distance(start, Vector3.zero), Is.LessThan(TOLERANCE));
            Assert.That(Vector3.Distance(end, Vector3.one), Is.LessThan(TOLERANCE));
        }

        // ----- Addition ----- \\

        [Test]
        public void Add_SumsTheNumericTypes()
        {
            Assert.That(TweenCoreOps<float>.Add(1.5f, 2.25f), Is.EqualTo(3.75f).Within(TOLERANCE));
            Assert.That(TweenCoreOps<double>.Add(1.5d, 2.25d), Is.EqualTo(3.75d).Within(TOLERANCE));
            Assert.AreEqual(7, TweenCoreOps<int>.Add(3, 4));
            Assert.AreEqual(7m, TweenCoreOps<decimal>.Add(3m, 4m));
        }

        [Test]
        public void Add_IsComponentwiseForVectors()
        {
            Vector3 sum = TweenCoreOps<Vector3>.Add(new Vector3(1f, 2f, 3f), new Vector3(10f, 20f, 30f));

            Assert.That(Vector3.Distance(sum, new Vector3(11f, 22f, 33f)), Is.LessThan(TOLERANCE));
        }

        // ----- The two Quaternion rules ----- \\

        [Test]
        public void Quaternion_AdditiveComposesTheRotationRatherThanAddingComponents()
        {
            // "For Quaternion the offset is composed rather than added, which is the rotation
            // equivalent." Composing two 90 degree yaws gives 180 degrees; adding the components
            // would not be a unit rotation at all.
            Quaternion ninety = Quaternion.Euler(0f, 90f, 0f);

            Quaternion composed = TweenCoreOps<Quaternion>.Add(ninety, ninety);

            Assert.That(Quaternion.Angle(composed, Quaternion.Euler(0f, 180f, 0f)), Is.LessThan(DEGREES_TOLERANCE));
        }

        [Test]
        public void Quaternion_InterpolationIsUnclamped_SoOvershootingCurvesWork()
        {
            // "Quaternion interpolation is unclamped so Back and Elastic overshoot on rotations
            // like they do everywhere else." The claim is that a weight beyond 1 travels past the
            // end rotation - not that it lands on any particular angle. The implementation is a
            // component wise LerpUnclamped, so a weight of 2 from identity to 90 degrees reaches
            // about 147 degrees rather than the 180 a spherical interpolation would give. This
            // asserts the documented property only, so it holds under either.
            Quaternion end = Quaternion.Euler(0f, 90f, 0f);
            Quaternion overshot = TweenCoreOps<Quaternion>.Lerp(Quaternion.identity, end, 2f);

            float endAngle = Quaternion.Angle(Quaternion.identity, end);
            float overshotAngle = Quaternion.Angle(Quaternion.identity, overshot);

            Assert.Greater(overshotAngle, endAngle + DEGREES_TOLERANCE,
                "A weight beyond 1 should travel past the end rotation, not clamp to it.");
        }
    }
}
