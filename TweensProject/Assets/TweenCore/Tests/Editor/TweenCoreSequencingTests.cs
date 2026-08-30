using System;
using NUnit.Framework;
using UnityEngine;
using Tweening;

// Author : Auguste Paccapelo

namespace Tweening.Tests
{
    /// <summary>
    /// Drives tweens by hand, without a TweenCoreManager, so the completion state machine can be
    /// exercised frame by frame. Every test here is named after the defect it prevents.
    /// </summary>
    public class TweenCoreSequencingTests
    {
        private const float TOLERANCE = 0.0001f;

        private static TweenCoreProperty<float> Property(float start, float end, float duration)
        {
            return new TweenCoreProperty<float>(start, end, duration);
        }

        // ----- Easing ----- \\

        [Test]
        public void EveryTypeAndEase_LandsOnTheFinalValue()
        {
            TweenCoreType[] types =
            {
                TweenCoreType.Linear, TweenCoreType.Sine, TweenCoreType.Cubic, TweenCoreType.Quint,
                TweenCoreType.Circ, TweenCoreType.Elastic, TweenCoreType.Quad, TweenCoreType.Quart,
                TweenCoreType.Expo, TweenCoreType.Back, TweenCoreType.Bounce,
            };

            TweenCoreEase[] eases =
            {
                TweenCoreEase.In, TweenCoreEase.Out, TweenCoreEase.InOut, TweenCoreEase.OutIn,
            };

            foreach (TweenCoreType type in types)
            {
                foreach (TweenCoreEase ease in eases)
                {
                    TweenCore tween = TweenCore.CreateTween();
                    TweenCoreProperty<float> property = Property(0f, 10f, 1f);
                    property.SetType(type).SetEase(ease);
                    tween.AddProperty(property);

                    tween.Play();
                    tween.Update(1f);

                    Assert.That(property.CurrentValue, Is.EqualTo(10f).Within(TOLERANCE),
                        $"{type} / {ease} did not land on the final value.");
                }
            }
        }

        [Test]
        public void Bounce_IsAnInShape_LikeEveryOtherTypeFunction()
        {
            // The shipped curve used to be easeOutBounce, so SetEase(In) produced an out bounce.
            // Halfway through, an In shape is still well below the midpoint.
            TweenCore tween = TweenCore.CreateTween();
            TweenCoreProperty<float> property = Property(0f, 1f, 1f);
            property.SetType(TweenCoreType.Bounce).SetEase(TweenCoreEase.In);
            tween.AddProperty(property);

            tween.Play();
            tween.Update(0.5f);

            Assert.Less(property.CurrentValue, 0.5f, "Bounce with ease In should still be in the lower half at t = 0.5");
        }

        // ----- Zero duration properties ----- \\

        [Test]
        public void ZeroDurationProperties_InParallel_DoNotThrowAndCompleteTheTween()
        {
            // Play() used to iterate the property list while properties removed themselves from it.
            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(Property(0f, 1f, 0f));
            tween.AddProperty(Property(0f, 1f, 0f));

            Assert.DoesNotThrow(() => tween.Play());

            tween.Update(0.016f);

            Assert.IsTrue(tween.IsFinished, "A tween of instant properties must finish instead of running forever.");
        }

        [Test]
        public void ZeroDurationFirstLink_DoesNotCutTheChainShort()
        {
            // The expected count used to be taken after the first link had already removed itself,
            // which made the tween declare itself finished one property early.
            TweenCore tween = TweenCore.CreateTween();
            TweenCoreProperty<float> first = Property(0f, 1f, 0f);
            TweenCoreProperty<float> second = Property(0f, 1f, 1f);
            TweenCoreProperty<float> third = Property(0f, 1f, 1f);

            tween.AddProperty(first);
            tween.AddProperty(second);
            tween.AddProperty(third);
            tween.Chain().Play();

            tween.Update(1f);

            Assert.IsFalse(tween.IsFinished, "The tween finished while the last link had not run.");

            tween.Update(1f);

            Assert.IsTrue(tween.IsFinished);
            Assert.That(third.CurrentValue, Is.EqualTo(1f).Within(TOLERANCE));
        }

        // ----- Stop ----- \\

        [Test]
        public void Stop_StopsEveryProperty_NotOnlyTheFirst()
        {
            // In loop mode nothing is removed from the list, which is where the [0] indexer bug bit.
            TweenCore tween = TweenCore.CreateTween();
            TweenCoreProperty<float>[] properties =
            {
                Property(0f, 1f, 1f), Property(0f, 1f, 1f), Property(0f, 1f, 1f),
            };

            foreach (TweenCoreProperty<float> property in properties) tween.AddProperty(property);

            tween.SetLoop(true, -1).Play();
            tween.Update(0.1f);
            tween.Stop();

            foreach (TweenCoreProperty<float> property in properties)
            {
                Assert.IsFalse(property.IsPlaying, "A property was left running after Stop().");
                Assert.That(property.CurrentValue, Is.EqualTo(1f).Within(TOLERANCE),
                    "A property was not snapped to its final value by Stop().");
            }
        }

        [Test]
        public void Stop_DoesNotFastForwardChainLinksThatNeverRan()
        {
            TweenCore tween = TweenCore.CreateTween();
            TweenCoreProperty<float> first = Property(0f, 1f, 1f);
            TweenCoreProperty<float> second = Property(0f, 1f, 1f);

            tween.AddProperty(first);
            tween.AddProperty(second);
            tween.Chain().Play();

            tween.Update(0.1f);
            tween.Stop();

            Assert.IsFalse(second.HasStarted, "Cancelling a chain must not start the links that never ran.");
        }

        [Test]
        public void Complete_LandsEveryProperty_IncludingPendingChainLinks()
        {
            TweenCore tween = TweenCore.CreateTween();
            TweenCoreProperty<float> first = Property(0f, 1f, 1f);
            TweenCoreProperty<float> second = Property(0f, 2f, 1f);

            tween.AddProperty(first);
            tween.AddProperty(second);
            tween.Chain().Play();

            tween.Update(0.1f);
            tween.Complete();

            Assert.That(first.CurrentValue, Is.EqualTo(1f).Within(TOLERANCE));
            Assert.That(second.CurrentValue, Is.EqualTo(2f).Within(TOLERANCE));
        }

        // ----- Loop iterations ----- \\

        [Test]
        public void ZeroIterations_RunNothingAndWriteNothing()
        {
            TweenCore tween = TweenCore.CreateTween();
            TweenCoreProperty<float> property = Property(5f, 10f, 1f);
            tween.AddProperty(property);

            tween.SetLoop(true, 0).Play();

            Assert.IsFalse(property.HasStarted, "Zero iterations must not start any property.");
            Assert.That(property.CurrentValue, Is.EqualTo(0f).Within(TOLERANCE),
                "Zero iterations must not write a value.");
        }

        [Test]
        public void Loop_RunsTheRequestedNumberOfIterations()
        {
            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(Property(0f, 1f, 1f));

            int loops = 0;
            tween.OnLoopFinish += _ => loops++;

            tween.SetLoop(true, 3).Play();

            for (int i = 0; i < 3; i++) tween.Update(1f);

            Assert.IsTrue(tween.IsFinished);
            Assert.AreEqual(2, loops, "Three iterations means the tween restarts twice.");
        }

        // ----- Events ----- \\

        [Test]
        public void OnUpdateValue_FiresOncePerFrame()
        {
            TweenCore tween = TweenCore.CreateTween();
            TweenCoreProperty<float> property = Property(0f, 10f, 10f);
            tween.AddProperty(property);

            int calls = 0;
            property.OnUpdateValue += (_, __) => calls++;

            tween.Play();
            tween.Update(1f);

            Assert.AreEqual(1, calls, "OnUpdateValue was raised more than once for a single frame.");
        }

        // ----- Final value ----- \\

        [Test]
        public void FinalValueIsExact_EvenWhenACustomCurveDoesNotEndAtOne()
        {
            TweenCore tween = TweenCore.CreateTween();
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);
            // A curve that lands on 0.5 rather than 1.
            property.SetType(AnimationCurve.Linear(0f, 0f, 1f, 0.5f));
            tween.AddProperty(property);

            tween.Play();
            tween.Update(1f);

            Assert.That(property.CurrentValue, Is.EqualTo(10f).Within(TOLERANCE),
                "Finishing must land on the final value, not on whatever the curve evaluates to.");
        }

        // ----- Replay ----- \\

        [Test]
        public void Restart_ReplaysAChainCorrectly()
        {
            TweenCore tween = TweenCore.CreateTween();
            TweenCoreProperty<float> first = Property(0f, 1f, 1f);
            TweenCoreProperty<float> second = Property(0f, 1f, 1f);

            tween.AddProperty(first);
            tween.AddProperty(second);
            tween.DontDestroyWhenFinish().Chain().Play();

            tween.Update(1f);
            tween.Update(1f);
            Assert.IsTrue(tween.IsFinished);

            tween.Restart();

            Assert.IsTrue(first.HasStarted);
            Assert.IsFalse(second.HasStarted, "Replaying must not start the whole chain at once.");

            tween.Update(1f);
            tween.Update(1f);

            Assert.IsTrue(tween.IsFinished);
        }

        // ----- Unsupported types ----- \\

        [Test]
        public void UnsupportedValueType_FailsFastWithAClearMessage()
        {
            Assert.Throws<NotSupportedException>(() => new TweenCoreProperty<string>("a", "b", 1f));
        }
    }
}
