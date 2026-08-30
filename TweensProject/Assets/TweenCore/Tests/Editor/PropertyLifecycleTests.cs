using System;
using NUnit.Framework;
using UnityEngine;
using Tweening;

namespace Tweening.Tests
{
    /// <summary>
    /// Behavioural coverage of a property's life : delay, the playing / paused / finished flags,
    /// elapsed time, the event callbacks, and the two ways a property can be ended.
    ///
    /// Written from the README's method list and the XML doc comments, driven by hand so no frame
    /// timing is involved. Tweens are built without a manager, exactly as the existing suite does.
    /// </summary>
    public class PropertyLifecycleTests
    {
        private const float TOLERANCE = 0.0001f;

        private static TweenCoreProperty<float> Property(float start, float end, float duration)
        {
            return new TweenCoreProperty<float>(start, end, duration);
        }

        private static TweenCore TweenWith(TweenCorePropertyBase property)
        {
            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(property);
            return tween;
        }

        // ----- Delay ----- \\

        [Test]
        public void SetDelay_IsExposedAsDelay()
        {
            TweenCoreProperty<float> property = Property(0f, 1f, 1f).SetDelay(0.5f);

            Assert.That(property.Delay, Is.EqualTo(0.5f).Within(TOLERANCE));
        }

        [Test]
        public void SetDelay_PushesCompletionOutByTheDelay()
        {
            // "Add a delay before the animation start" : a 1s property delayed by 0.5s is not done
            // at t = 1, it is done at t = 1.5.
            TweenCoreProperty<float> property = Property(0f, 10f, 1f).SetDelay(0.5f);
            TweenCore tween = TweenWith(property);

            tween.Play();
            tween.Update(1f);

            Assert.IsFalse(tween.IsFinished, "The delay should have pushed completion past t = 1.");

            tween.Update(0.5f);

            Assert.IsTrue(tween.IsFinished);
            Assert.That(property.CurrentValue, Is.EqualTo(10f).Within(TOLERANCE));
        }

        [Test]
        public void SetDelay_ShiftsTheAnimationRatherThanCompressingIt()
        {
            // A delay postpones the animation, it does not change its shape : a property delayed
            // by 1s, sampled at t = 1.5, must be exactly where an undelayed one is at t = 0.5.
            //
            // Asserting the equality rather than an inequality matters. "The delayed one is
            // behind" would also hold if the property had simply never been written, so it could
            // pass while the delay logic was broken.
            TweenCoreProperty<float> delayed = Property(0f, 10f, 1f).SetDelay(1f);
            TweenCoreProperty<float> immediate = Property(0f, 10f, 1f);

            TweenCore delayedTween = TweenWith(delayed);
            TweenCore immediateTween = TweenWith(immediate);

            delayedTween.Play();
            immediateTween.Play();
            delayedTween.Update(1.5f);
            immediateTween.Update(0.5f);

            Assert.That(delayed.CurrentValue, Is.EqualTo(immediate.CurrentValue).Within(TOLERANCE));
            Assert.That(delayed.CurrentValue, Is.EqualTo(5f).Within(TOLERANCE));
        }

        // ----- Playback flags ----- \\

        [Test]
        public void ANewProperty_IsNotPlayingUntilTheTweenPlays()
        {
            TweenCoreProperty<float> property = Property(0f, 1f, 1f);

            Assert.IsFalse(property.HasStarted);
            Assert.IsFalse(property.IsPlaying);
        }

        [Test]
        public void Play_MarksTheTweenStartedAndPlaying()
        {
            TweenCore tween = TweenWith(Property(0f, 1f, 1f));

            tween.Play();

            Assert.IsTrue(tween.HasStarted);
            Assert.IsTrue(tween.IsPlaying);
            Assert.IsFalse(tween.IsPaused);
            Assert.IsFalse(tween.IsFinished);
        }

        [Test]
        public void Play_ATweenAlreadyPlaying_DoesNotRestartIt()
        {
            // "Can't start 2 times."
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);
            TweenCore tween = TweenWith(property);

            tween.Play();
            tween.Update(0.5f);
            tween.Play();

            Assert.That(tween.ElapsedTime, Is.EqualTo(0.5f).Within(TOLERANCE),
                "A second Play() should be ignored, not rewind the tween.");
        }

        [Test]
        public void Stop_ClearsPlayingAndMarksFinished()
        {
            TweenCore tween = TweenWith(Property(0f, 10f, 1f));

            tween.Play();
            tween.Stop();

            Assert.IsFalse(tween.IsPlaying);
            Assert.IsFalse(tween.HasStarted);
            Assert.IsTrue(tween.IsFinished);
        }

        // ----- Pause and resume ----- \\

        [Test]
        public void Pause_StopsTheValueAdvancing()
        {
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);
            TweenCore tween = TweenWith(property);

            tween.Play();
            tween.Update(0.25f);
            float atPause = property.CurrentValue;

            tween.Pause();
            tween.Update(0.5f);

            Assert.That(property.CurrentValue, Is.EqualTo(atPause).Within(TOLERANCE),
                "A paused tween should not move its properties.");
        }

        [Test]
        public void Pause_IsReportedByIsPaused()
        {
            TweenCore tween = TweenWith(Property(0f, 10f, 1f));

            tween.Play();
            tween.Pause();

            Assert.IsTrue(tween.IsPaused);

            tween.Resume();

            Assert.IsFalse(tween.IsPaused);
        }

        [Test]
        public void Resume_ContinuesFromWhereItPaused()
        {
            // "Properties keep their state and resume where they stopped."
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);
            TweenCore tween = TweenWith(property);

            tween.Play();
            tween.Update(0.25f);
            tween.Pause();
            tween.Update(0.5f);
            tween.Resume();
            tween.Update(0.25f);

            Assert.That(property.CurrentValue, Is.EqualTo(5f).Within(TOLERANCE),
                "Time spent paused should not count towards the animation.");
        }

        [Test]
        public void PropertyPause_StopsThatPropertyOnly()
        {
            TweenCoreProperty<float> paused = Property(0f, 10f, 1f);
            TweenCoreProperty<float> running = Property(0f, 10f, 1f);

            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(paused);
            tween.AddProperty(running);

            tween.Play();
            paused.Pause();
            tween.Update(0.5f);

            Assert.That(running.CurrentValue, Is.EqualTo(5f).Within(TOLERANCE));
            Assert.That(paused.CurrentValue, Is.Not.EqualTo(5f).Within(TOLERANCE));
        }

        // ----- Elapsed time ----- \\

        [Test]
        public void ElapsedTime_AccumulatesTheDeltasPassedIn()
        {
            TweenCore tween = TweenWith(Property(0f, 10f, 10f));

            tween.Play();
            tween.Update(0.25f);
            tween.Update(0.25f);

            Assert.That(tween.ElapsedTime, Is.EqualTo(0.5f).Within(TOLERANCE));
        }

        [Test]
        public void ElapsedTime_ResetsWhenTheTweenStops()
        {
            TweenCore tween = TweenWith(Property(0f, 10f, 10f));

            tween.Play();
            tween.Update(0.5f);
            tween.Stop();

            Assert.That(tween.ElapsedTime, Is.EqualTo(0f).Within(TOLERANCE));
        }

        // ----- Events ----- \\

        [Test]
        public void PropertyOnStart_FiresOnce_WhenThePropertyStarts()
        {
            int starts = 0;
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);
            property.OnStart += _ => starts++;

            TweenCore tween = TweenWith(property);
            tween.Play();
            tween.Update(0.5f);

            Assert.AreEqual(1, starts);
        }

        [Test]
        public void PropertyOnFinish_FiresOnce_WhenThePropertyCompletes()
        {
            int finishes = 0;
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);
            property.OnFinish += _ => finishes++;

            TweenCore tween = TweenWith(property);
            tween.Play();
            tween.Update(0.5f);

            Assert.AreEqual(0, finishes, "Not finished halfway through.");

            tween.Update(0.5f);

            Assert.AreEqual(1, finishes);
        }

        [Test]
        public void PropertyOnUpdate_FiresOncePerFrameWhileRunning()
        {
            int updates = 0;
            TweenCoreProperty<float> property = Property(0f, 10f, 10f);
            property.OnUpdate += _ => updates++;

            TweenCore tween = TweenWith(property);
            tween.Play();
            tween.Update(1f);
            tween.Update(1f);
            tween.Update(1f);

            Assert.AreEqual(3, updates);
        }

        [Test]
        public void TweenOnStart_FiresOnPlay()
        {
            int starts = 0;
            TweenCore tween = TweenWith(Property(0f, 10f, 1f));
            tween.OnStart += _ => starts++;

            tween.Play();

            Assert.AreEqual(1, starts);
        }

        [Test]
        public void TweenOnFinish_FiresWhenTheTweenEnds()
        {
            int finishes = 0;
            TweenCore tween = TweenWith(Property(0f, 10f, 1f));
            tween.OnFinish += _ => finishes++;

            tween.Play();
            tween.Update(1f);

            Assert.AreEqual(1, finishes);
        }

        [Test]
        public void TweenOnUpdate_FiresOncePerUpdateCall()
        {
            int updates = 0;
            TweenCore tween = TweenWith(Property(0f, 10f, 10f));
            tween.OnUpdate += _ => updates++;

            tween.Play();
            tween.Update(1f);
            tween.Update(1f);

            Assert.AreEqual(2, updates);
        }

        // ----- Ending a property ----- \\

        [Test]
        public void Stop_WithSetToFinalValue_LandsOnTheFinalValue()
        {
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);
            TweenCore tween = TweenWith(property);

            tween.Play();
            tween.Update(0.25f);
            tween.Stop(true);

            Assert.That(property.CurrentValue, Is.EqualTo(10f).Within(TOLERANCE));
        }

        [Test]
        public void Stop_WithoutSetToFinalValue_LeavesTheValueWhereItWas()
        {
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);
            TweenCore tween = TweenWith(property);

            tween.Play();
            tween.Update(0.25f);
            float atStop = property.CurrentValue;

            tween.Stop(false);

            Assert.That(property.CurrentValue, Is.EqualTo(atStop).Within(TOLERANCE));
        }

        [Test]
        public void Kill_IsStopWithoutWritingAValue()
        {
            // "Kill() - same as Stop(false)."
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);
            TweenCore tween = TweenWith(property);

            tween.Play();
            tween.Update(0.25f);
            float atKill = property.CurrentValue;

            tween.Kill();

            Assert.That(property.CurrentValue, Is.EqualTo(atKill).Within(TOLERANCE));
            Assert.IsFalse(tween.IsPlaying);
        }

        [Test]
        public void SetToFinalVals_LandsOnTheEndValueWithoutRunning()
        {
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);

            property.SetToFinalVals();

            Assert.That(property.CurrentValue, Is.EqualTo(10f).Within(TOLERANCE));
        }

        [Test]
        public void AZeroDurationProperty_HoldsItsFinalValueImmediately()
        {
            TweenCoreProperty<float> property = Property(0f, 10f, 0f);
            TweenCore tween = TweenWith(property);

            tween.Play();

            Assert.That(property.CurrentValue, Is.EqualTo(10f).Within(TOLERANCE),
                "A zero duration property completes inside Start().");
        }

        // ----- Accessors ----- \\

        [Test]
        public void GetCurrentValue_MatchesTheCurrentValueProperty()
        {
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);
            TweenCore tween = TweenWith(property);

            tween.Play();
            tween.Update(0.5f);

            Assert.That(property.GetCurrentValue(), Is.EqualTo(property.CurrentValue).Within(TOLERANCE));
        }

        [Test]
        public void DurationAndTypeAndEase_AreExposed()
        {
            TweenCoreProperty<float> property = Property(0f, 10f, 2.5f)
                .SetType(TweenCoreType.Quad)
                .SetEase(TweenCoreEase.OutIn);

            Assert.That(property.Duration, Is.EqualTo(2.5f).Within(TOLERANCE));
            Assert.AreEqual(TweenCoreType.Quad, property.Type);
            Assert.AreEqual(TweenCoreEase.OutIn, property.Ease);
        }

        [Test]
        public void NumProperties_CountsWhatWillRun()
        {
            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(Property(0f, 1f, 1f));
            tween.AddProperty(Property(0f, 1f, 1f));

            tween.Play();

            Assert.AreEqual(2, tween.NumProperties);
        }
    }
}
