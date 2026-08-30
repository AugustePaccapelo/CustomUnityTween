using NUnit.Framework;
using UnityEngine;
using Tweening;

namespace Tweening.Tests
{
    /// <summary>
    /// Behavioural coverage of how a tween sequences its properties, written from the README :
    ///
    ///   "In parallel mode, all properties start at the same time, in chain mode only one runs at
    ///    a time."
    ///   "Chain() - one property at a time instead of all at once."
    ///   "Stop(...) - cancels; chain links that never ran are left untouched."
    ///
    /// TweenCoreSequencingTests already pins the defects found in the v1.1 audit. This file covers
    /// the ordinary documented behaviour around them.
    /// </summary>
    public class ChainAndParallelTests
    {
        private const float TOLERANCE = 0.0001f;

        private static TweenCoreProperty<float> Property(float start, float end, float duration)
        {
            return new TweenCoreProperty<float>(start, end, duration);
        }

        // ----- Mode flags ----- \\

        [Test]
        public void ATweenIsParallelByDefault()
        {
            Assert.IsTrue(TweenCore.CreateTween().IsParallel);
        }

        [Test]
        public void Chain_MakesTheTweenNotParallel()
        {
            Assert.IsFalse(TweenCore.CreateTween().Chain().IsParallel);
        }

        [Test]
        public void Parallel_MakesTheTweenParallel()
        {
            Assert.IsTrue(TweenCore.CreateTween().Chain().Parallel().IsParallel);
        }

        [Test]
        public void SetParallel_AndSetChain_AreOpposites()
        {
            Assert.IsFalse(TweenCore.CreateTween().SetParallel(false).IsParallel);
            Assert.IsTrue(TweenCore.CreateTween().SetChain(false).IsParallel);
            Assert.IsFalse(TweenCore.CreateTween().SetChain(true).IsParallel);
        }

        // ----- Parallel ----- \\

        [Test]
        public void Parallel_StartsEveryPropertyAtTheSameTime()
        {
            TweenCoreProperty<float> first = Property(0f, 10f, 1f);
            TweenCoreProperty<float> second = Property(0f, 20f, 1f);

            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(first);
            tween.AddProperty(second);
            tween.Parallel().Play();

            tween.Update(0.5f);

            Assert.That(first.CurrentValue, Is.EqualTo(5f).Within(TOLERANCE));
            Assert.That(second.CurrentValue, Is.EqualTo(10f).Within(TOLERANCE));
        }

        [Test]
        public void Parallel_FinishesWhenTheLongestPropertyDoes()
        {
            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(Property(0f, 10f, 1f));
            tween.AddProperty(Property(0f, 10f, 2f));
            tween.Parallel().Play();

            tween.Update(1f);

            Assert.IsFalse(tween.IsFinished, "The two second property is still running.");

            tween.Update(1f);

            Assert.IsTrue(tween.IsFinished);
        }

        // ----- Chain ----- \\

        [Test]
        public void Chain_LeavesLaterLinksUntouchedWhileTheFirstRuns()
        {
            TweenCoreProperty<float> first = Property(0f, 10f, 1f);
            TweenCoreProperty<float> second = Property(0f, 10f, 1f);

            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(first);
            tween.AddProperty(second);
            tween.Chain().Play();

            tween.Update(0.5f);

            Assert.That(first.CurrentValue, Is.EqualTo(5f).Within(TOLERANCE));
            Assert.IsFalse(second.HasStarted, "The second link must not run until the first is done.");
        }

        [Test]
        public void Chain_StartsTheNextLinkWhenTheCurrentOneFinishes()
        {
            TweenCoreProperty<float> first = Property(0f, 10f, 1f);
            TweenCoreProperty<float> second = Property(0f, 10f, 1f);

            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(first);
            tween.AddProperty(second);
            tween.Chain().Play();

            tween.Update(1f);

            Assert.IsTrue(second.HasStarted, "Finishing the first link should start the second.");
        }

        [Test]
        public void Chain_TakesTheSumOfItsLinksToFinish()
        {
            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(Property(0f, 10f, 1f));
            tween.AddProperty(Property(0f, 10f, 1f));
            tween.Chain().Play();

            tween.Update(1f);

            Assert.IsFalse(tween.IsFinished, "Only the first of two links has run.");

            tween.Update(1f);

            Assert.IsTrue(tween.IsFinished);
        }

        [Test]
        public void Stop_LeavesAChainLinkThatNeverRanUntouched()
        {
            // "Stop(...) - cancels; chain links that never ran are left untouched."
            TweenCoreProperty<float> first = Property(0f, 10f, 1f);
            TweenCoreProperty<float> second = Property(0f, 10f, 1f);

            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(first);
            tween.AddProperty(second);
            tween.Chain().Play();

            tween.Update(0.5f);
            tween.Stop();

            Assert.IsFalse(second.HasStarted);
            Assert.That(second.CurrentValue, Is.Not.EqualTo(10f).Within(TOLERANCE),
                "Cancelling must not land a link that never ran on its end value.");
        }

        // ----- Counting ----- \\

        [Test]
        public void NumPropertiesFinished_CountsCompletedProperties()
        {
            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(Property(0f, 10f, 1f));
            tween.AddProperty(Property(0f, 10f, 2f));
            tween.Parallel().Play();

            tween.Update(1f);

            Assert.AreEqual(1, tween.NumPropertiesFinished);
        }

        // ----- Building ----- \\

        [Test]
        public void AddProperty_ReturnsTheTween_SoCallsChain()
        {
            TweenCore tween = TweenCore.CreateTween();

            Assert.AreSame(tween, tween.AddProperty(Property(0f, 1f, 1f)));
        }

        [Test]
        public void NewProperty_AddsThePropertyToTheTween()
        {
            TweenCore tween = TweenCore.CreateTween();
            tween.NewProperty(0f, 10f, 1f);

            tween.Play();

            Assert.AreEqual(1, tween.NumProperties);
        }

        [Test]
        public void NewProperty_WithAFunction_WritesThroughThatFunction()
        {
            float written = 0f;

            TweenCore tween = TweenCore.CreateTween();
            tween.NewProperty(v => written = v, 0f, 10f, 1f);

            tween.Play();
            tween.Update(0.5f);

            Assert.That(written, Is.EqualTo(5f).Within(TOLERANCE));
        }

        // ----- Configuration flags ----- \\

        [Test]
        public void SetUseUnscaledTime_IsExposedAsUseUnscaledTime()
        {
            Assert.IsFalse(TweenCore.CreateTween().UseUnscaledTime, "Scaled time is the default.");
            Assert.IsTrue(TweenCore.CreateTween().SetUseUnscaledTime(true).UseUnscaledTime);
        }

        [Test]
        public void DestroyWhenFinish_IsOnByDefaultAndCanBeTurnedOff()
        {
            Assert.IsTrue(TweenCore.CreateTween().DestroyOnFinish);
            Assert.IsFalse(TweenCore.CreateTween().DontDestroyWhenFinish().DestroyOnFinish);
            Assert.IsTrue(TweenCore.CreateTween().DontDestroyWhenFinish().DestroyWhenFinish().DestroyOnFinish);
            Assert.IsFalse(TweenCore.CreateTween().SetDestroyWhenFinish(false).DestroyOnFinish);
        }

        [Test]
        public void DontDestroyWhenFinish_KeepsThePropertiesAfterTheTweenEnds()
        {
            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(Property(0f, 10f, 1f));
            tween.DontDestroyWhenFinish().Play();

            tween.Update(1f);
            tween.Play();

            Assert.AreEqual(1, tween.NumProperties, "The property should survive so the tween can play again.");
        }

        [Test]
        public void SurviveOnUnload_IsExposedAndReversible()
        {
            Assert.IsFalse(TweenCore.CreateTween().SurviveOnSceneUnload, "Tweens are killed on unload by default.");
            Assert.IsTrue(TweenCore.CreateTween().SurviveOnUnload().SurviveOnSceneUnload);
            Assert.IsFalse(TweenCore.CreateTween().SurviveOnUnload().KillOnUnload().SurviveOnSceneUnload);
            Assert.IsTrue(TweenCore.CreateTween().SetSurviveOnUnload(true).SurviveOnSceneUnload);
        }

        [Test]
        public void TheObsoleteUnloadAliases_ForwardToTheNewNames()
        {
            // "The old names still compile and forward to the new ones, with an [Obsolete] warning."
#pragma warning disable 618
            Assert.IsTrue(TweenCore.CreateTween().SurviveOnSceneLoad().SurviveOnSceneUnload);
            Assert.IsFalse(TweenCore.CreateTween().SurviveOnUnload().KillOnSceneUnLoad().SurviveOnSceneUnload);
#pragma warning restore 618
        }

        // ----- Stop guard ----- \\

        [Test]
        public void Stop_OnATweenThatNeverPlayed_DoesNothing()
        {
            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(Property(0f, 10f, 1f));

            tween.Stop();

            Assert.IsFalse(tween.IsFinished, "A tween that never started has nothing to finish.");
        }
    }
}
