using NUnit.Framework;
using UnityEngine;
using Tweening;

// Author : Auguste Paccapelo

namespace Tweening.Tests
{
    /// <summary>
    /// Covers the per type interpolation layer. The integer cases are the regression tests for
    /// the boxed-float-unboxed-as-int crash.
    /// </summary>
    public class TweenCoreOpsTests
    {
        [Test]
        public void EverySupportedType_ResolvesALerp()
        {
            Assert.IsTrue(TweenCoreOps<float>.IsSupported);
            Assert.IsTrue(TweenCoreOps<double>.IsSupported);
            Assert.IsTrue(TweenCoreOps<int>.IsSupported);
            Assert.IsTrue(TweenCoreOps<uint>.IsSupported);
            Assert.IsTrue(TweenCoreOps<long>.IsSupported);
            Assert.IsTrue(TweenCoreOps<ulong>.IsSupported);
            Assert.IsTrue(TweenCoreOps<decimal>.IsSupported);
            Assert.IsTrue(TweenCoreOps<Vector2>.IsSupported);
            Assert.IsTrue(TweenCoreOps<Vector3>.IsSupported);
            Assert.IsTrue(TweenCoreOps<Vector4>.IsSupported);
            Assert.IsTrue(TweenCoreOps<Quaternion>.IsSupported);
            Assert.IsTrue(TweenCoreOps<Color>.IsSupported);
            Assert.IsTrue(TweenCoreOps<Color32>.IsSupported);
        }

        [Test]
        public void UnsupportedType_ReportsItself()
        {
            Assert.IsFalse(TweenCoreOps<string>.IsSupported);
        }

        // ----- Integers : these used to throw InvalidCastException on the first frame ----- \\

        [Test]
        public void Int_InterpolatesAndRounds()
        {
            Assert.AreEqual(0, TweenCoreOps<int>.Lerp(0, 10, 0f));
            Assert.AreEqual(5, TweenCoreOps<int>.Lerp(0, 10, 0.5f));
            Assert.AreEqual(10, TweenCoreOps<int>.Lerp(0, 10, 1f));
        }

        [Test]
        public void Int_HandlesDescendingRange()
        {
            Assert.AreEqual(5, TweenCoreOps<int>.Lerp(10, 0, 0.5f));
            Assert.AreEqual(-10, TweenCoreOps<int>.Lerp(0, -10, 1f));
        }

        [Test]
        public void UInt_DoesNotWrapWhenDescending()
        {
            // (uint)b - (uint)a wraps to ~4 billion when b < a, so this has to be computed wider.
            Assert.AreEqual(5u, TweenCoreOps<uint>.Lerp(10u, 0u, 0.5f));
        }

        [Test]
        public void UInt_ClampsWhenTheCurveOvershootsBelowZero()
        {
            // Back and Elastic legitimately produce weights outside 0..1.
            Assert.AreEqual(0u, TweenCoreOps<uint>.Lerp(10u, 0u, 1.5f));
        }

        [Test]
        public void Long_And_ULong_Interpolate()
        {
            Assert.AreEqual(50L, TweenCoreOps<long>.Lerp(0L, 100L, 0.5f));
            Assert.AreEqual(50UL, TweenCoreOps<ulong>.Lerp(0UL, 100UL, 0.5f));
            Assert.AreEqual(0UL, TweenCoreOps<ulong>.Lerp(0UL, 100UL, -0.5f));
        }

        [Test]
        public void Decimal_Interpolates()
        {
            Assert.AreEqual(5m, TweenCoreOps<decimal>.Lerp(0m, 10m, 0.5f));
        }

        // ----- Unity types ----- \\

        [Test]
        public void Vector3_AllowsOvershoot()
        {
            // Back and Elastic depend on the value leaving the start..end range.
            Vector3 overshoot = TweenCoreOps<Vector3>.Lerp(Vector3.zero, Vector3.one, 1.5f);
            Assert.That(overshoot.x, Is.EqualTo(1.5f).Within(0.0001f));
        }

        [Test]
        public void Color32_ClampsInsteadOfOverflowing()
        {
            Color32 result = TweenCoreOps<Color32>.Lerp(new Color32(0, 0, 0, 255), new Color32(200, 0, 0, 255), 1.5f);
            Assert.AreEqual(255, result.r);
        }

        // ----- Additive ----- \\

        [Test]
        public void AdditiveIsSupported_ForEveryTypeTheReadmeClaims()
        {
            Assert.IsTrue(TweenCoreOps<float>.SupportsAdditive);
            Assert.IsTrue(TweenCoreOps<Vector3>.SupportsAdditive);
            Assert.IsTrue(TweenCoreOps<Color>.SupportsAdditive);

            // These two used to throw KeyNotFoundException instead of reporting themselves.
            Assert.IsTrue(TweenCoreOps<Quaternion>.SupportsAdditive);
            Assert.IsTrue(TweenCoreOps<Color32>.SupportsAdditive);
        }

        [Test]
        public void Color32_AddClamps()
        {
            Color32 sum = TweenCoreOps<Color32>.Add(new Color32(200, 0, 0, 255), new Color32(100, 0, 0, 255));
            Assert.AreEqual(255, sum.r);
        }
    }
}
