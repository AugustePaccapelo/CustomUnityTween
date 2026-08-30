using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Tweening;

namespace Tweening.Tests
{
    /// <summary>
    /// Behavioural coverage of the manager's scene unload hook, written from the README :
    ///
    ///   "Drives every tween from one Update. Creates itself on demand, survives scene loads."
    ///   "SurviveOnUnload() / KillOnUnload() / SetSurviveOnUnload(bool survive)"
    ///
    /// and from the 1.1 changelog, which records the defect this hook exists for :
    ///
    ///   "Scene unload wrote final values through reflection onto objects the scene had already
    ///    destroyed."
    ///
    /// PlayMode, because there is no way to fire sceneUnloaded outside it. A throwaway scene is
    /// created and unloaded rather than loading one from build settings, so the test needs no
    /// project configuration.
    /// </summary>
    public class SceneUnloadTests
    {
        private const float TOLERANCE = 0.0001f;

        private readonly List<GameObject> _spawned = new List<GameObject>();

        private static TweenCoreProperty<float> Property()
        {
            // Long enough that a handful of frames cannot finish it on their own, so anything that
            // stops the tween must be the unload.
            return new TweenCoreProperty<float>(0f, 10f, 60f);
        }

        private static TweenCore PlayingTween(TweenCorePropertyBase property, bool survives)
        {
            TweenCore tween = TweenCore.CreateTween();
            tween.AddProperty(property);
            tween.SetSurviveOnUnload(survives);
            tween.Play();
            return tween;
        }

        private static IEnumerator CreateThenUnloadAScene()
        {
            Scene temp = SceneManager.CreateScene("TweenCoreUnloadTestScene");

            yield return null;

            yield return SceneManager.UnloadSceneAsync(temp);

            // sceneUnloaded is raised during the unload; give the handler a frame to have run.
            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null) Object.DestroyImmediate(_spawned[i]);
            }

            _spawned.Clear();

            if (TweenCoreManager.Instance != null)
            {
                Object.DestroyImmediate(TweenCoreManager.Instance.gameObject);
            }
        }

        // ----- The hook ----- \\

        [UnityTest]
        public IEnumerator UnloadingAScene_StopsATweenThatDoesNotSurvive()
        {
            TweenCore tween = PlayingTween(Property(), survives: false);

            yield return CreateThenUnloadAScene();

            Assert.IsFalse(tween.IsPlaying, "A tween that does not survive an unload should have been stopped.");
        }

        [UnityTest]
        public IEnumerator UnloadingAScene_LeavesASurvivingTweenPlaying()
        {
            TweenCore tween = PlayingTween(Property(), survives: true);

            yield return CreateThenUnloadAScene();

            Assert.IsTrue(tween.IsPlaying, "SurviveOnUnload should keep the tween running across the unload.");
        }

        [UnityTest]
        public IEnumerator UnloadingAScene_DoesNotWriteFinalValues()
        {
            // The defect itself. Stopping on unload must not snap properties to their end value,
            // because for a reflection tween that write lands on an object the scene has already
            // destroyed.
            TweenCoreProperty<float> property = Property();
            PlayingTween(property, survives: false);

            yield return null;

            float beforeUnload = property.CurrentValue;

            yield return CreateThenUnloadAScene();

            Assert.That(property.CurrentValue, Is.EqualTo(beforeUnload).Within(TOLERANCE),
                "Unload must cancel the tween, not complete it.");
            Assert.That(property.CurrentValue, Is.Not.EqualTo(10f).Within(TOLERANCE),
                "The property must not have been snapped to its final value.");
        }

        [UnityTest]
        public IEnumerator UnloadingAScene_DoesNotWriteThroughReflectionToADestroyedTarget()
        {
            // The end to end shape of the bug : a reflection tween whose target lives in the scene
            // being unloaded. Writing a final value here would reflect onto a dead object.
            GameObject target = new GameObject("unload target");
            _spawned.Add(target);

            TweenCore tween = TweenCore.CreateTween();
            tween.NewProperty(target.transform, TweenCoreTarget.Transform.LOCAL_POSITION,
                Vector3.zero, new Vector3(100f, 0f, 0f), 60f);
            tween.SetSurviveOnUnload(false);
            tween.Play();

            yield return null;

            LogAssert.NoUnexpectedReceived();

            yield return CreateThenUnloadAScene();

            Assert.IsFalse(tween.IsPlaying);
            Assert.Less(target.transform.localPosition.x, 100f,
                "The target must not have been snapped to the end position on unload.");
        }

        [UnityTest]
        public IEnumerator UnloadingAScene_DoesNotThrow()
        {
            PlayingTween(Property(), survives: false);
            PlayingTween(Property(), survives: true);

            yield return CreateThenUnloadAScene();

            // Reaching here without an exception or an unexpected log is the assertion.
            LogAssert.NoUnexpectedReceived();
            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator AStoppedTween_IsUnregisteredFromTheManager()
        {
            PlayingTween(Property(), survives: false);

            yield return null;

            Assert.AreEqual(1, TweenCoreManager.Instance.NumTweens, "The played tween should be registered.");

            yield return CreateThenUnloadAScene();

            Assert.AreEqual(0, TweenCoreManager.Instance.NumTweens,
                "A tween stopped by the unload should no longer be driven.");
        }
    }
}
