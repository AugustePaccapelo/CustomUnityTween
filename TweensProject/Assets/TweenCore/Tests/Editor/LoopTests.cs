using NUnit.Framework;
using UnityEngine;
using Tweening;

namespace Tweening.Tests
{
    /// <summary>
    /// Behavioural coverage of looping, written from the README :
    ///
    ///   "SetLoop(bool isLoop, int numIteration = -1) - negative is infinite, 0 runs nothing."
    ///   "OnLoopFinish&lt;TweenCore&gt;"
    ///
    /// TweenCoreSequencingTests already pins the two defects found in the audit : a zero iteration
    /// loop that still ran, and a finite loop that ran the wrong number of times. This file covers
    /// the surrounding documented behaviour.
    /// </summary>
    public class LoopTests
    {
        private const float TOLERANCE = 0.0001f;

        private static TweenCoreProperty<float> Property(float start, float end, float duration)
        {
            return new TweenCoreProperty<float>(start, end, duration);
        }

        private static TweenCore Looping(TweenCorePropertyBase property, int iterations)
        {
            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(property);
            tween.SetLoop(true, iterations);
            return tween;
        }

        // ----- Flags ----- \\

        [Test]
        public void ATweenDoesNotLoopByDefault()
        {
            Assert.IsFalse(TweenCore.CreateTween().IsLoop);
        }

        [Test]
        public void SetLoop_IsExposedAsIsLoopAndNumIteration()
        {
            TweenCore tween = TweenCore.CreateTween().SetLoop(true, 4);

            Assert.IsTrue(tween.IsLoop);
            Assert.AreEqual(4, tween.NumIteration);
        }

        [Test]
        public void SetLoop_DefaultsToInfinite()
        {
            // "negative for infinite" - and the default argument is -1.
            Assert.AreEqual(-1, TweenCore.CreateTween().SetLoop(true).NumIteration);
        }

        [Test]
        public void SetLoop_False_StopsTheTweenLooping()
        {
            Assert.IsFalse(TweenCore.CreateTween().SetLoop(true, 3).SetLoop(false).IsLoop);
        }

        // ----- Iteration counting ----- \\

        [Test]
        public void CurrentIteration_AdvancesOncePerCompletedCycle()
        {
            TweenCore tween = Looping(Property(0f, 10f, 1f), -1);
            tween.Play();

            tween.Update(1f);
            Assert.AreEqual(1, tween.CurrentIteration);

            tween.Update(1f);
            Assert.AreEqual(2, tween.CurrentIteration);
        }

        [Test]
        public void ANewIteration_DoesNotCompleteUntilItsPropertiesDo()
        {
            // The completion counter has to be reset when a cycle restarts. If it is not, the
            // count stays at its previous total, the "everything finished" test passes on the very
            // next frame, and the tween burns through an iteration per frame instead of per cycle.
            //
            // A fractional step is what makes this visible : stepping a single property by its
            // full duration looks identical whether or not the counter was reset.
            TweenCore tween = Looping(Property(0f, 10f, 1f), -1);
            tween.Play();

            tween.Update(1f);

            Assert.AreEqual(1, tween.CurrentIteration);

            tween.Update(0.1f);

            Assert.AreEqual(1, tween.CurrentIteration,
                "A tenth of the way into the second cycle is not a completed iteration.");
        }

        [Test]
        public void AMultiPropertyLoop_CountsOneIterationPerFullCycle()
        {
            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(Property(0f, 10f, 1f));
            tween.AddProperty(Property(0f, 10f, 1f));
            tween.SetLoop(true, -1).Play();

            tween.Update(1f);
            tween.Update(0.5f);

            Assert.AreEqual(1, tween.CurrentIteration,
                "Halfway through the second cycle is still one completed iteration.");
        }

        [Test]
        public void AnInfiniteLoop_KeepsPlayingWellPastItsDuration()
        {
            TweenCore tween = Looping(Property(0f, 10f, 1f), -1);
            tween.Play();

            for (int i = 0; i < 10; i++) tween.Update(1f);

            Assert.IsTrue(tween.IsPlaying, "A negative iteration count means never stop.");
            Assert.IsFalse(tween.IsFinished);
        }

        [Test]
        public void AFiniteLoop_StopsAfterTheRequestedIterations()
        {
            TweenCore tween = Looping(Property(0f, 10f, 1f), 3);
            tween.Play();

            tween.Update(1f);
            tween.Update(1f);

            Assert.IsTrue(tween.IsPlaying, "Two of three iterations done.");

            tween.Update(1f);

            Assert.IsFalse(tween.IsPlaying);
            Assert.IsTrue(tween.IsFinished);
        }

        // ----- Restarting the properties ----- \\

        [Test]
        public void EachIteration_RestartsThePropertyFromItsStartValue()
        {
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);
            TweenCore tween = Looping(property, -1);
            tween.Play();

            tween.Update(1f);
            tween.Update(0.1f);

            Assert.That(property.CurrentValue, Is.EqualTo(1f).Within(TOLERANCE),
                "A new iteration should replay from the start, not continue from the end.");
        }

        [Test]
        public void ALoopedChain_RestartsFromItsFirstLink()
        {
            TweenCoreProperty<float> first = Property(0f, 10f, 1f);
            TweenCoreProperty<float> second = Property(0f, 10f, 1f);

            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(first);
            tween.AddProperty(second);
            tween.Chain().SetLoop(true, -1).Play();

            tween.Update(1f);
            tween.Update(1f);
            tween.Update(0.1f);

            Assert.That(first.CurrentValue, Is.EqualTo(1f).Within(TOLERANCE),
                "The second cycle should begin at the first link again.");
        }

        [Test]
        public void ALoopingTween_KeepsItsPropertiesBetweenIterations()
        {
            TweenCore tween = Looping(Property(0f, 10f, 1f), -1);
            tween.Play();

            tween.Update(1f);

            Assert.AreEqual(1, tween.NumProperties,
                "A looping tween must not discard the property it is about to replay.");
        }

        // ----- OnLoopFinish ----- \\

        [Test]
        public void OnLoopFinish_FiresOncePerCompletedCycleOfAnInfiniteLoop()
        {
            int loops = 0;
            TweenCore tween = Looping(Property(0f, 10f, 1f), -1);
            tween.OnLoopFinish += _ => loops++;
            tween.Play();

            tween.Update(1f);
            tween.Update(1f);
            tween.Update(1f);

            Assert.AreEqual(3, loops);
        }

        [Test]
        public void OnLoopFinish_DoesNotFireForANonLoopingTween()
        {
            int loops = 0;
            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(Property(0f, 10f, 1f));
            tween.OnLoopFinish += _ => loops++;
            tween.Play();

            tween.Update(1f);

            Assert.AreEqual(0, loops);
        }

        [Test]
        public void OnFinish_FiresOnceWhenAFiniteLoopEnds()
        {
            int finishes = 0;
            TweenCore tween = Looping(Property(0f, 10f, 1f), 2);
            tween.OnFinish += _ => finishes++;
            tween.Play();

            tween.Update(1f);
            tween.Update(1f);

            Assert.AreEqual(1, finishes);
        }

        // ----- Zero iterations ----- \\

        [Test]
        public void ZeroIterations_FinishesImmediatelyWithoutPlaying()
        {
            // "0 runs nothing."
            TweenCore tween = Looping(Property(0f, 10f, 1f), 0);

            tween.Play();

            Assert.IsFalse(tween.IsPlaying);
            Assert.IsTrue(tween.IsFinished);
        }
    }
}
