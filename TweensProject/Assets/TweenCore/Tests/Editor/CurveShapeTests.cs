using System;
using NUnit.Framework;
using UnityEngine;
using Tweening;

namespace Tweening.Tests
{
    /// <summary>
    /// Behavioural coverage of the type and ease curves, written from the claims the README makes
    /// about them rather than from the implementations :
    ///
    ///   "Type is the shape of the curve, Ease is the direction it is applied in, so Bounce + Out
    ///    and Bounce + In are mirror images of each other. Every type function is an In shape,
    ///    which Out, InOut and OutIn then mirror."
    ///
    ///   "Either half can be replaced by your own function or by an AnimationCurve."
    /// </summary>
    public class CurveShapeTests
    {
        private const float TOLERANCE = 0.0001f;

        // ----- Every type function is an In shape ----- \\

        [Test]
        public void EveryBuiltInType_IsAnInShape([ValueSource(typeof(CurveSampler), nameof(CurveSampler.BuiltInTypes))] TweenCoreType type)
        {
            // An In shape is back loaded : at the halfway point it has not yet covered half the
            // distance. Linear is the boundary case and sits exactly on the midpoint.
            float halfway = CurveSampler.Sample(type, TweenCoreEase.In, 0.5f);

            Assert.LessOrEqual(halfway, 0.5f + TOLERANCE,
                $"{type} with ease In is past the midpoint at t = 0.5, so it is not an In shape.");
        }

        [Test]
        public void EveryBuiltInTypeExceptLinear_IsStrictlyBackLoaded([ValueSource(typeof(CurveSampler), nameof(CurveSampler.BuiltInTypes))] TweenCoreType type)
        {
            if (type == TweenCoreType.Linear) Assert.Ignore("Linear is the boundary case : it sits on the midpoint by definition.");

            float halfway = CurveSampler.Sample(type, TweenCoreEase.In, 0.5f);

            Assert.Less(halfway, 0.5f, $"{type} with ease In should be strictly below the midpoint at t = 0.5.");
        }

        [Test]
        public void Linear_IsTheIdentity()
        {
            Assert.That(CurveSampler.Sample(TweenCoreType.Linear, TweenCoreEase.In, 0.25f), Is.EqualTo(0.25f).Within(TOLERANCE));
            Assert.That(CurveSampler.Sample(TweenCoreType.Linear, TweenCoreEase.In, 0.5f), Is.EqualTo(0.5f).Within(TOLERANCE));
            Assert.That(CurveSampler.Sample(TweenCoreType.Linear, TweenCoreEase.In, 0.75f), Is.EqualTo(0.75f).Within(TOLERANCE));
        }

        // ----- Out mirrors In ----- \\

        [Test]
        public void Out_IsTheMirrorOfIn([ValueSource(typeof(CurveSampler), nameof(CurveSampler.BuiltInTypes))] TweenCoreType type)
        {
            // "Bounce + Out and Bounce + In are mirror images of each other" : out(t) = 1 - in(1 - t).
            float[] samples = { 0.25f, 0.5f, 0.75f };

            foreach (float t in samples)
            {
                float outValue = CurveSampler.Sample(type, TweenCoreEase.Out, t);
                float mirrored = 1f - CurveSampler.Sample(type, TweenCoreEase.In, 1f - t);

                Assert.That(outValue, Is.EqualTo(mirrored).Within(TOLERANCE),
                    $"{type} : Out at t = {t} is not the mirror of In at t = {1f - t}.");
            }
        }

        [Test]
        public void Out_IsFrontLoaded([ValueSource(typeof(CurveSampler), nameof(CurveSampler.BuiltInTypes))] TweenCoreType type)
        {
            if (type == TweenCoreType.Linear) Assert.Ignore("Linear is symmetric : In and Out are the same curve.");

            float halfway = CurveSampler.Sample(type, TweenCoreEase.Out, 0.5f);

            Assert.Greater(halfway, 0.5f, $"{type} with ease Out should be past the midpoint at t = 0.5.");
        }

        // ----- InOut and OutIn ----- \\

        [Test]
        public void InOut_PassesThroughTheMidpoint([ValueSource(typeof(CurveSampler), nameof(CurveSampler.BuiltInTypes))] TweenCoreType type)
        {
            float halfway = CurveSampler.Sample(type, TweenCoreEase.InOut, 0.5f);

            Assert.That(halfway, Is.EqualTo(0.5f).Within(TOLERANCE),
                $"{type} with ease InOut should hand over from In to Out exactly at the midpoint.");
        }

        [Test]
        public void OutIn_PassesThroughTheMidpoint([ValueSource(typeof(CurveSampler), nameof(CurveSampler.BuiltInTypes))] TweenCoreType type)
        {
            float halfway = CurveSampler.Sample(type, TweenCoreEase.OutIn, 0.5f);

            Assert.That(halfway, Is.EqualTo(0.5f).Within(TOLERANCE),
                $"{type} with ease OutIn should hand over from Out to In exactly at the midpoint.");
        }

        [Test]
        public void InOut_IsTheInShapeCompressedIntoTheFirstHalf([ValueSource(typeof(CurveSampler), nameof(CurveSampler.BuiltInTypes))] TweenCoreType type)
        {
            // The first half of InOut is the In shape run at double speed and half height.
            float inOutQuarter = CurveSampler.Sample(type, TweenCoreEase.InOut, 0.25f);
            float expected = CurveSampler.Sample(type, TweenCoreEase.In, 0.5f) * 0.5f;

            Assert.That(inOutQuarter, Is.EqualTo(expected).Within(TOLERANCE),
                $"{type} : InOut at t = 0.25 should equal half of In at t = 0.5.");
        }

        [Test]
        public void OutIn_IsTheOutShapeCompressedIntoTheFirstHalf([ValueSource(typeof(CurveSampler), nameof(CurveSampler.BuiltInTypes))] TweenCoreType type)
        {
            float outInQuarter = CurveSampler.Sample(type, TweenCoreEase.OutIn, 0.25f);
            float expected = CurveSampler.Sample(type, TweenCoreEase.Out, 0.5f) * 0.5f;

            Assert.That(outInQuarter, Is.EqualTo(expected).Within(TOLERANCE),
                $"{type} : OutIn at t = 0.25 should equal half of Out at t = 0.5.");
        }

        // ----- Replacing the type half ----- \\

        [Test]
        public void CustomTypeFunction_ReplacesTheShape()
        {
            // t^3 at the halfway point is 0.125, which no built in type produces.
            float halfway = CurveSampler.SampleCustomType(t => t * t * t, TweenCoreEase.In, 0.5f);

            Assert.That(halfway, Is.EqualTo(0.125f).Within(TOLERANCE));
        }

        [Test]
        public void CustomTypeFunction_StillLandsExactlyOnTheFinalValue()
        {
            float landed = CurveSampler.SampleCustomType(t => t * t * t, TweenCoreEase.In, 1f);

            Assert.That(landed, Is.EqualTo(1f).Within(TOLERANCE));
        }

        [Test]
        public void AnimationCurveAsType_ReplacesTheShape()
        {
            // A key sitting exactly on t = 0.5 evaluates to its own value, so the expected result
            // is unambiguous regardless of tangents.
            AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 0.9f), new Keyframe(1f, 1f));

            float halfway = CurveSampler.SampleCurveType(curve, TweenCoreEase.In, 0.5f);

            Assert.That(halfway, Is.EqualTo(0.9f).Within(TOLERANCE));
        }

        [Test]
        public void SetType_WithEnumAndFunction_UsesTheFunction()
        {
            TweenCore tween = TweenCore.CreateTween();
            TweenCoreProperty<float> property = new TweenCoreProperty<float>(0f, 1f, 1f);
            property.SetType(TweenCoreType.Custom, t => t * t * t).SetEase(TweenCoreEase.In);
            tween.AddProperty(property);

            tween.Play();
            tween.Update(0.5f);

            Assert.That(property.CurrentValue, Is.EqualTo(0.125f).Within(TOLERANCE));
        }

        [Test]
        public void SetType_WithEnumAndCurve_UsesTheCurve()
        {
            AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 0.9f), new Keyframe(1f, 1f));

            TweenCore tween = TweenCore.CreateTween();
            TweenCoreProperty<float> property = new TweenCoreProperty<float>(0f, 1f, 1f);
            property.SetType(TweenCoreType.CustomCurve, curve).SetEase(TweenCoreEase.In);
            tween.AddProperty(property);

            tween.Play();
            tween.Update(0.5f);

            Assert.That(property.CurrentValue, Is.EqualTo(0.9f).Within(TOLERANCE));
        }

        // ----- Replacing the ease half ----- \\

        [Test]
        public void CustomEaseFunction_ReplacesTheDirection()
        {
            // A custom ease receives the weight and the type function, so mirroring by hand must
            // reproduce the built in Out exactly.
            float custom = CurveSampler.SampleCustomEase(TweenCoreType.Quad, (w, typeFunc) => 1f - typeFunc(1f - w), 0.5f);
            float builtInOut = CurveSampler.Sample(TweenCoreType.Quad, TweenCoreEase.Out, 0.5f);

            Assert.That(custom, Is.EqualTo(builtInOut).Within(TOLERANCE));
        }

        [Test]
        public void CustomEaseFunction_CanIgnoreTheTypeEntirely()
        {
            float custom = CurveSampler.SampleCustomEase(TweenCoreType.Bounce, (w, typeFunc) => w * 0.5f, 0.5f);

            Assert.That(custom, Is.EqualTo(0.25f).Within(TOLERANCE));
        }

        [Test]
        public void AnimationCurveAsEase_ReplacesTheDirection()
        {
            AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 0.9f), new Keyframe(1f, 1f));

            float halfway = CurveSampler.SampleCurveEase(TweenCoreType.Linear, curve, 0.5f);

            Assert.That(halfway, Is.EqualTo(0.9f).Within(TOLERANCE));
        }

        [Test]
        public void SetEase_WithEnumAndFunction_UsesTheFunction()
        {
            TweenCore tween = TweenCore.CreateTween();
            TweenCoreProperty<float> property = new TweenCoreProperty<float>(0f, 1f, 1f);
            property.SetType(TweenCoreType.Linear).SetEase(TweenCoreEase.Custom, (w, typeFunc) => w * 0.5f);
            tween.AddProperty(property);

            tween.Play();
            tween.Update(0.5f);

            Assert.That(property.CurrentValue, Is.EqualTo(0.25f).Within(TOLERANCE));
        }

        [Test]
        public void SetEase_WithEnumAndCurve_UsesTheCurve()
        {
            AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 0.9f), new Keyframe(1f, 1f));

            TweenCore tween = TweenCore.CreateTween();
            TweenCoreProperty<float> property = new TweenCoreProperty<float>(0f, 1f, 1f);
            property.SetType(TweenCoreType.Linear).SetEase(TweenCoreEase.CustomCurve, curve);
            tween.AddProperty(property);

            tween.Play();
            tween.Update(0.5f);

            Assert.That(property.CurrentValue, Is.EqualTo(0.9f).Within(TOLERANCE));
        }
    }
}
