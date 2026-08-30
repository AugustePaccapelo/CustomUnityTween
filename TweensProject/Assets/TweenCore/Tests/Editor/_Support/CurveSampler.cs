using System;
using UnityEngine;
using Tweening;

namespace Tweening.Tests
{
    /// <summary>
    /// Samples a curve through the public API : a 0 -> 1 float property of duration 1, driven by
    /// hand to the requested normalised time. The tween state machine is one shot, so every sample
    /// builds a fresh tween rather than rewinding one.
    ///
    /// Sampling at t = 0 is not supported : Update() writes nothing until elapsed time passes the
    /// delay, so a zero step leaves the property untouched. Use a small positive t instead.
    /// </summary>
    public static class CurveSampler
    {
        private static float Drive(TweenCoreProperty<float> property, float t)
        {
            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(property);
            tween.Play();
            tween.Update(t);

            return property.CurrentValue;
        }

        private static TweenCoreProperty<float> Unit()
        {
            return new TweenCoreProperty<float>(0f, 1f, 1f);
        }

        /// <summary>The value of a built in type / ease pair at normalised time t.</summary>
        public static float Sample(TweenCoreType type, TweenCoreEase ease, float t)
        {
            return Drive(Unit().SetType(type).SetEase(ease), t);
        }

        /// <summary>The value of a custom type function under a built in ease.</summary>
        public static float SampleCustomType(Func<float, float> customType, TweenCoreEase ease, float t)
        {
            return Drive(Unit().SetType(customType).SetEase(ease), t);
        }

        /// <summary>The value of an AnimationCurve used as the type, under a built in ease.</summary>
        public static float SampleCurveType(AnimationCurve curve, TweenCoreEase ease, float t)
        {
            return Drive(Unit().SetType(curve).SetEase(ease), t);
        }

        /// <summary>The value of a built in type under a custom ease function.</summary>
        public static float SampleCustomEase(TweenCoreType type, Func<float, Func<float, float>, float> customEase, float t)
        {
            return Drive(Unit().SetType(type).SetEase(customEase), t);
        }

        /// <summary>The value of a built in type under an AnimationCurve used as the ease.</summary>
        public static float SampleCurveEase(TweenCoreType type, AnimationCurve curve, float t)
        {
            return Drive(Unit().SetType(type).SetEase(curve), t);
        }

        /// <summary>
        /// The eleven type functions the README documents as built in shapes. Custom and
        /// CustomCurve are excluded : they have no shape until one is supplied.
        /// </summary>
        public static readonly TweenCoreType[] BuiltInTypes =
        {
            TweenCoreType.Linear, TweenCoreType.Sine, TweenCoreType.Cubic, TweenCoreType.Quint,
            TweenCoreType.Circ, TweenCoreType.Elastic, TweenCoreType.Quad, TweenCoreType.Quart,
            TweenCoreType.Expo, TweenCoreType.Back, TweenCoreType.Bounce,
        };

        /// <summary>The four eases that are a direction rather than a supplied shape.</summary>
        public static readonly TweenCoreEase[] BuiltInEases =
        {
            TweenCoreEase.In, TweenCoreEase.Out, TweenCoreEase.InOut, TweenCoreEase.OutIn,
        };
    }
}
