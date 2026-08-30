using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Tweening;

namespace Tweening.Tests
{
    /// <summary>
    /// Behavioural coverage of TweenCoreManager, written from the README :
    ///
    ///   "Drives every tween from one Update. Creates itself on demand, survives scene loads."
    ///   "PauseAll() - pauses the manager, not the individual tweens"
    ///   "StopAll(bool setToFinalValue = true)", "AddTween", "RemoveTween", "NumTweens", "IsPlaying"
    ///
    /// The manager's own Update pump and its scene-unload hook need real frames and a real scene
    /// load, so they belong to the PlayMode suite. What is covered here is the registry it keeps
    /// and the playback flags, which are ordinary method calls.
    ///
    /// A manager is built by hand rather than through Instance : the singleton deliberately
    /// refuses to spawn outside play mode, so that edit mode tests cannot leave a stray GameObject
    /// in the open scene.
    /// </summary>
    public class ManagerTests
    {
        private const float TOLERANCE = 0.0001f;

        private readonly List<GameObject> _spawned = new List<GameObject>();

        private TweenCoreManager NewManager()
        {
            GameObject go = new GameObject(nameof(TweenCoreManager));
            _spawned.Add(go);
            return go.AddComponent<TweenCoreManager>();
        }

        private static TweenCoreProperty<float> Property(float start, float end, float duration)
        {
            return new TweenCoreProperty<float>(start, end, duration);
        }

        private static TweenCore PlayingTween(TweenCorePropertyBase property)
        {
            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(property);
            tween.Play();
            return tween;
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null) Object.DestroyImmediate(_spawned[i]);
            }

            _spawned.Clear();
        }

        // ----- The singleton ----- \\

        [Test]
        public void Instance_DoesNotSpawnAManagerOutsidePlayMode()
        {
            // Deliberate: an edit mode test or an editor script asking for Instance must not leave
            // a GameObject behind in the user's open scene. In play mode it creates itself, which
            // is what the README describes and what the PlayMode suite covers.
            Assert.IsNull(TweenCoreManager.Instance);
        }

        // ----- The registry ----- \\

        [Test]
        public void ANewManager_HasNoTweens()
        {
            Assert.AreEqual(0, NewManager().NumTweens);
        }

        [Test]
        public void AddTween_RegistersTheTween()
        {
            TweenCoreManager manager = NewManager();

            manager.AddTween(TweenCore.CreateTween());

            Assert.AreEqual(1, manager.NumTweens);
        }

        [Test]
        public void AddTween_IgnoresATweenItAlreadyHas()
        {
            TweenCoreManager manager = NewManager();
            TweenCore tween = TweenCore.CreateTween();

            manager.AddTween(tween);
            manager.AddTween(tween);

            Assert.AreEqual(1, manager.NumTweens, "The same tween must not be updated twice a frame.");
        }

        [Test]
        public void AddTween_IgnoresNull()
        {
            TweenCoreManager manager = NewManager();

            Assert.DoesNotThrow(() => manager.AddTween(null));
            Assert.AreEqual(0, manager.NumTweens);
        }

        [Test]
        public void RemoveTween_UnregistersTheTween()
        {
            TweenCoreManager manager = NewManager();
            TweenCore tween = TweenCore.CreateTween();

            manager.AddTween(tween);
            manager.RemoveTween(tween);

            Assert.AreEqual(0, manager.NumTweens);
        }

        [Test]
        public void RemoveTween_OfATweenItNeverHad_DoesNothing()
        {
            TweenCoreManager manager = NewManager();
            manager.AddTween(TweenCore.CreateTween());

            Assert.DoesNotThrow(() => manager.RemoveTween(TweenCore.CreateTween()));
            Assert.AreEqual(1, manager.NumTweens);
        }

        // ----- Playback flags ----- \\

        [Test]
        public void ANewManager_IsPlaying()
        {
            Assert.IsTrue(NewManager().IsPlaying);
        }

        [Test]
        public void PauseAll_StopsTheManager()
        {
            TweenCoreManager manager = NewManager();

            manager.PauseAll();

            Assert.IsFalse(manager.IsPlaying);
        }

        [Test]
        public void PauseAll_PausesTheManagerNotTheIndividualTweens()
        {
            // "PauseAll() - pauses the manager, not the individual tweens." The distinction
            // matters: resuming the manager must not un-pause a tween the caller paused by hand.
            TweenCoreManager manager = NewManager();
            TweenCore tween = PlayingTween(Property(0f, 10f, 1f));
            manager.AddTween(tween);

            manager.PauseAll();

            Assert.IsFalse(tween.IsPaused, "The tween itself should be untouched.");
            Assert.IsTrue(tween.IsPlaying);
        }

        [Test]
        public void ResumeAll_RestartsTheManager()
        {
            TweenCoreManager manager = NewManager();

            manager.PauseAll();
            manager.ResumeAll();

            Assert.IsTrue(manager.IsPlaying);
        }

        // ----- StopAll ----- \\

        [Test]
        public void StopAll_StopsEveryRegisteredTween()
        {
            TweenCoreManager manager = NewManager();
            TweenCore first = PlayingTween(Property(0f, 10f, 1f));
            TweenCore second = PlayingTween(Property(0f, 10f, 1f));

            manager.AddTween(first);
            manager.AddTween(second);

            manager.StopAll();

            Assert.IsFalse(first.IsPlaying);
            Assert.IsFalse(second.IsPlaying, "StopAll must reach past the first tween.");
        }

        [Test]
        public void StopAll_ByDefault_LandsTheTweensOnTheirFinalValues()
        {
            TweenCoreManager manager = NewManager();
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);
            TweenCore tween = PlayingTween(property);
            tween.Update(0.25f);

            manager.AddTween(tween);
            manager.StopAll();

            Assert.That(property.CurrentValue, Is.EqualTo(10f).Within(TOLERANCE));
        }

        [Test]
        public void StopAll_False_LeavesTheValuesWhereTheyWere()
        {
            TweenCoreManager manager = NewManager();
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);
            TweenCore tween = PlayingTween(property);
            tween.Update(0.25f);
            float atStop = property.CurrentValue;

            manager.AddTween(tween);
            manager.StopAll(false);

            Assert.That(property.CurrentValue, Is.EqualTo(atStop).Within(TOLERANCE));
        }
    }
}
