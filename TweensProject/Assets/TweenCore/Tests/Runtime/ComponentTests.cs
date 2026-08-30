using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Tweening;

namespace Tweening.Tests
{
    /// <summary>
    /// Behavioural coverage of TweenCoreComponent, written from the README :
    ///
    ///   "Build a tween from the inspector, no code."
    ///   "Play(), Pause(), Resume(), Restart(), Complete()",
    ///   "StopAndSetToFinalValue(), StopAndDontChangeValue()", "AddProperty(...)"
    ///   "Tween - the underlying TweenCore", "TweenName"
    ///
    /// and the 1.1 changelog line "TweenCoreComponent.OnDestroy could throw a
    /// NullReferenceException."
    ///
    /// A PlayMode suite, because the component is lifecycle driven. Awake creates the tween and
    /// Start applies the inspector settings, and neither runs in edit mode for a component without
    /// [ExecuteAlways] - an EditMode version of this file failed with a null Tween on every test.
    ///
    /// The [Test] cases below only need Awake, which AddComponent runs synchronously. The
    /// [UnityTest] cases yield a frame so that Start has run.
    /// </summary>
    public class ComponentTests
    {
        private const float TOLERANCE = 0.0001f;

        private readonly List<GameObject> _spawned = new List<GameObject>();

        private TweenCoreComponent NewComponent()
        {
            GameObject go = new GameObject("tween component");
            _spawned.Add(go);
            return go.AddComponent<TweenCoreComponent>();
        }

        private static TweenCoreProperty<float> Property(float start, float end, float duration)
        {
            return new TweenCoreProperty<float>(start, end, duration);
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null) Object.DestroyImmediate(_spawned[i]);
            }

            _spawned.Clear();

            // Playing a tween spawns the manager on demand. Clear it so one test cannot pump
            // another test's tweens.
            if (TweenCoreManager.Instance != null) Object.DestroyImmediate(TweenCoreManager.Instance.gameObject);
        }

        // ----- Construction ----- \\

        [Test]
        public void AComponent_HasATweenAsSoonAsItExists()
        {
            Assert.IsNotNull(NewComponent().Tween);
        }

        [Test]
        public void TweenName_RoundTrips()
        {
            TweenCoreComponent component = NewComponent();

            component.TweenName = "fade in";

            Assert.AreEqual("fade in", component.TweenName);
        }

        [Test]
        public void AddProperty_IgnoresNull()
        {
            TweenCoreComponent component = NewComponent();

            Assert.DoesNotThrow(() => component.AddProperty(null));
        }

        // ----- Forwarding to the underlying tween ----- \\

        [Test]
        public void Play_StartsTheUnderlyingTween()
        {
            TweenCoreComponent component = NewComponent();
            component.Tween.AddProperty(Property(0f, 10f, 1f));

            component.Play();

            Assert.IsTrue(component.Tween.IsPlaying);
        }

        [Test]
        public void Pause_PausesTheUnderlyingTween()
        {
            TweenCoreComponent component = NewComponent();
            component.Tween.AddProperty(Property(0f, 10f, 1f));

            component.Play();
            component.Pause();

            Assert.IsTrue(component.Tween.IsPaused);
        }

        [Test]
        public void Resume_ResumesTheUnderlyingTween()
        {
            TweenCoreComponent component = NewComponent();
            component.Tween.AddProperty(Property(0f, 10f, 1f));

            component.Play();
            component.Pause();
            component.Resume();

            Assert.IsFalse(component.Tween.IsPaused);
        }

        [Test]
        public void Complete_LandsThePropertiesOnTheirFinalValues()
        {
            TweenCoreComponent component = NewComponent();
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);
            component.Tween.AddProperty(property);

            component.Play();
            component.Tween.Update(0.25f);
            component.Complete();

            Assert.That(property.CurrentValue, Is.EqualTo(10f).Within(TOLERANCE));
        }

        [Test]
        public void Restart_ReplaysFromTheStart()
        {
            TweenCoreComponent component = NewComponent();
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);
            component.Tween.DontDestroyWhenFinish();
            component.Tween.AddProperty(property);

            component.Play();
            component.Tween.Update(0.5f);
            component.Restart();
            component.Tween.Update(0.1f);

            Assert.That(property.CurrentValue, Is.EqualTo(1f).Within(TOLERANCE),
                "After a restart the property should be a tenth of the way in, not six tenths.");
        }

        [Test]
        public void StopAndSetToFinalValue_LandsTheProperties()
        {
            TweenCoreComponent component = NewComponent();
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);
            component.Tween.AddProperty(property);

            component.Play();
            component.Tween.Update(0.25f);
            component.StopAndSetToFinalValue();

            Assert.That(property.CurrentValue, Is.EqualTo(10f).Within(TOLERANCE));
            Assert.IsFalse(component.Tween.IsPlaying);
        }

        [Test]
        public void StopAndDontChangeValue_LeavesThePropertiesWhereTheyWere()
        {
            TweenCoreComponent component = NewComponent();
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);
            component.Tween.AddProperty(property);

            component.Play();
            component.Tween.Update(0.25f);
            float atStop = property.CurrentValue;
            component.StopAndDontChangeValue();

            Assert.That(property.CurrentValue, Is.EqualTo(atStop).Within(TOLERANCE));
            Assert.IsFalse(component.Tween.IsPlaying);
        }


        // ----- What Start does (needs a frame) ----- \

        // Start() calls SetBaseValues() on every property, which resolves the reflection target.
        // These use a reflection property, which is what a component built in the inspector always
        // holds. See docs/open-items.md for why a code-created manual property does not belong
        // here yet.
        private TweenCoreProperty<Vector3> ReflectionProperty(TweenCoreComponent component)
        {
            return new TweenCoreProperty<Vector3>(
                component.transform, "localPosition", Vector3.zero, new Vector3(10f, 0f, 0f), 10f);
        }

        [UnityTest]
        public IEnumerator AddProperty_WiresThePropertyIntoTheTweenOnStart()
        {
            // "Properties are added from the + menu on the Tween Properties list" - whatever the
            // inspector collected has to reach the tween when the object comes alive.
            TweenCoreComponent component = NewComponent();
            component.AddProperty(ReflectionProperty(component));

            yield return null;

            Assert.AreEqual(1, component.Tween.NumProperties);
        }

        [UnityTest]
        public IEnumerator ByDefault_TheTweenPlaysOnStart()
        {
            TweenCoreComponent component = NewComponent();
            component.AddProperty(ReflectionProperty(component));

            yield return null;

            Assert.IsTrue(component.Tween.HasStarted, "Play on start is the default.");
        }

        [UnityTest]
        public IEnumerator TheManagerDrivesTheComponentsTweenAcrossFrames()
        {
            // The end to end path : component starts a tween, the tween registers itself, and the
            // manager's Update advances it without anyone calling Update by hand.
            TweenCoreComponent component = NewComponent();
            TweenCoreProperty<Vector3> property = ReflectionProperty(component);
            component.AddProperty(property);

            yield return null;
            yield return null;
            yield return null;

            Assert.Greater(component.transform.localPosition.x, 0f,
                "The manager should have advanced the property over real frames.");
        }

        // ----- Teardown ----- \\

        [Test]
        public void DestroyingTheComponent_DoesNotThrow()
        {
            // The 1.1 changelog : "TweenCoreComponent.OnDestroy could throw a
            // NullReferenceException."
            TweenCoreComponent component = NewComponent();
            component.Tween.AddProperty(Property(0f, 10f, 1f));
            component.Play();

            Assert.DoesNotThrow(() => Object.DestroyImmediate(component.gameObject));
        }

        [Test]
        public void DestroyingAComponentThatNeverPlayed_DoesNotThrow()
        {
            TweenCoreComponent component = NewComponent();

            Assert.DoesNotThrow(() => Object.DestroyImmediate(component.gameObject));
        }

        [Test]
        public void DestroyingTheComponent_StopsItsTweenWithoutWritingValues()
        {
            TweenCoreComponent component = NewComponent();
            TweenCoreProperty<float> property = Property(0f, 10f, 1f);
            component.Tween.AddProperty(property);
            component.Play();
            component.Tween.Update(0.25f);
            float atDestroy = property.CurrentValue;

            Object.DestroyImmediate(component.gameObject);

            Assert.That(property.CurrentValue, Is.EqualTo(atDestroy).Within(TOLERANCE),
                "Tearing a component down must not write final values onto objects that may be gone.");
        }
    }
}
