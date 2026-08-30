using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Tweening;

namespace Tweening.Tests
{
    /// <summary>
    /// Behavioural coverage of the reflection tweens, written from the README :
    ///
    ///   "Easiest to use, costs the most. The target property or field is resolved once when the
    ///    tween starts."
    ///   "With this overload the start value is whatever the target holds when Play() is called."
    ///   "FromCurrent() - reflection tweens only."
    ///
    /// and from the 1.1 changelog, which records that a destroyed target and an unresolvable name
    /// each used to throw once per frame.
    ///
    /// Real GameObjects are created and torn down per test. Values are still driven by hand.
    /// </summary>
    public class ReflectionBindingTests
    {
        private const float TOLERANCE = 0.0001f;

        private readonly List<GameObject> _spawned = new List<GameObject>();
        private readonly List<Object> _assets = new List<Object>();

        private GameObject NewObject()
        {
            GameObject go = new GameObject("tween target");
            _spawned.Add(go);
            return go;
        }

        private FakeTweenTarget NewTarget()
        {
            FakeTweenTarget target = ScriptableObject.CreateInstance<FakeTweenTarget>();
            _assets.Add(target);
            return target;
        }

        [TearDown]
        public void TearDown()
        {
            LogAssert.ignoreFailingMessages = false;

            for (int i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null) Object.DestroyImmediate(_spawned[i]);
            }

            for (int i = 0; i < _assets.Count; i++)
            {
                if (_assets[i] != null) Object.DestroyImmediate(_assets[i]);
            }

            _spawned.Clear();
            _assets.Clear();
        }

        // ----- Writing to a Transform ----- \\

        [Test]
        public void AReflectionTween_WritesToTheTargetsProperty()
        {
            Transform target = NewObject().transform;

            TweenCore tween = TweenCore.CreateTween();
            tween.NewProperty(target, "localPosition", Vector3.zero, new Vector3(10f, 0f, 0f), 1f);
            tween.Play();

            tween.Update(0.5f);

            Assert.That(target.localPosition.x, Is.EqualTo(5f).Within(TOLERANCE));
        }

        [Test]
        public void TheTargetConstants_ResolveTheSameMembersAsTheirStrings()
        {
            // The constants exist so callers do not have to type the string. They must agree.
            Transform byString = NewObject().transform;
            Transform byConstant = NewObject().transform;

            TweenCore a = TweenCore.CreateTween();
            a.NewProperty(byString, "localScale", Vector3.one, new Vector3(3f, 3f, 3f), 1f);
            a.Play();
            a.Update(1f);

            TweenCore b = TweenCore.CreateTween();
            b.NewProperty(byConstant, TweenCoreTarget.Transform.LOCAL_SCALE, Vector3.one, new Vector3(3f, 3f, 3f), 1f);
            b.Play();
            b.Update(1f);

            Assert.That(Vector3.Distance(byString.localScale, byConstant.localScale), Is.LessThan(TOLERANCE));
            Assert.That(byConstant.localScale.x, Is.EqualTo(3f).Within(TOLERANCE));
        }

        [Test]
        public void AReflectionTween_WritesGlobalPosition()
        {
            Transform target = NewObject().transform;

            TweenCore tween = TweenCore.CreateTween();
            tween.NewProperty(target, TweenCoreTarget.Transform.GLOBAL_POSITION,
                Vector3.zero, new Vector3(0f, 8f, 0f), 1f);
            tween.Play();

            tween.Update(1f);

            Assert.That(target.position.y, Is.EqualTo(8f).Within(TOLERANCE));
        }

        [Test]
        public void AReflectionTween_WritesEulerAngles()
        {
            Transform target = NewObject().transform;

            TweenCore tween = TweenCore.CreateTween();
            tween.NewProperty(target, TweenCoreTarget.Transform.LOCAL_ROTATION_EULER_ANGLE,
                Vector3.zero, new Vector3(0f, 90f, 0f), 1f);
            tween.Play();

            tween.Update(1f);

            Assert.That(Quaternion.Angle(target.localRotation, Quaternion.Euler(0f, 90f, 0f)), Is.LessThan(0.5f));
        }

        [Test]
        public void AReflectionTween_WritesAQuaternionRotation()
        {
            Transform target = NewObject().transform;

            TweenCore tween = TweenCore.CreateTween();
            tween.NewProperty(target, TweenCoreTarget.Transform.LOCAL_ROTATION_QUATERNION,
                Quaternion.identity, Quaternion.Euler(0f, 90f, 0f), 1f);
            tween.Play();

            tween.Update(1f);

            Assert.That(Quaternion.Angle(target.localRotation, Quaternion.Euler(0f, 90f, 0f)), Is.LessThan(0.5f));
        }

        // ----- Properties and fields ----- \\

        [Test]
        public void AReflectionTween_WritesAWritableProperty()
        {
            FakeTweenTarget target = NewTarget();

            TweenCore tween = TweenCore.CreateTween();
            tween.NewProperty(target, nameof(FakeTweenTarget.WritableProperty), 0f, 10f, 1f);
            tween.Play();

            tween.Update(1f);

            Assert.That(target.WritableProperty, Is.EqualTo(10f).Within(TOLERANCE));
        }

        [Test]
        public void AReflectionTween_WritesAPublicField()
        {
            // "It now lists writable properties *and* writable fields" - fields are targets too.
            FakeTweenTarget target = NewTarget();

            TweenCore tween = TweenCore.CreateTween();
            tween.NewProperty(target, nameof(FakeTweenTarget.writableField), 0f, 10f, 1f);
            tween.Play();

            tween.Update(1f);

            Assert.That(target.writableField, Is.EqualTo(10f).Within(TOLERANCE));
        }

        // ----- Where the start value comes from ----- \\

        [Test]
        public void WithoutAStartValue_TheTargetsCurrentValueIsTheStart()
        {
            // "With this overload the start value is whatever the target holds when Play() is
            // called."
            FakeTweenTarget target = NewTarget();
            target.writableField = 4f;

            TweenCore tween = TweenCore.CreateTween();
            tween.NewProperty(target, nameof(FakeTweenTarget.writableField), 10f, 1f);
            tween.Play();

            tween.Update(0.5f);

            Assert.That(target.writableField, Is.EqualTo(7f).Within(TOLERANCE),
                "Halfway from the target's own 4 to 10 is 7.");
        }

        [Test]
        public void TheStartValueIsReadAtPlay_NotAtConstruction()
        {
            FakeTweenTarget target = NewTarget();
            target.writableField = 0f;

            TweenCore tween = TweenCore.CreateTween();
            tween.NewProperty(target, nameof(FakeTweenTarget.writableField), 10f, 1f);

            // Changed after the property is built but before the tween plays.
            target.writableField = 4f;

            tween.Play();
            tween.Update(0.5f);

            Assert.That(target.writableField, Is.EqualTo(7f).Within(TOLERANCE));
        }

        [Test]
        public void WithAStartValue_TheTargetsCurrentValueIsIgnored()
        {
            FakeTweenTarget target = NewTarget();
            target.writableField = 99f;

            TweenCore tween = TweenCore.CreateTween();
            tween.NewProperty(target, nameof(FakeTweenTarget.writableField), 0f, 10f, 1f);
            tween.Play();

            tween.Update(0.5f);

            Assert.That(target.writableField, Is.EqualTo(5f).Within(TOLERANCE));
        }

        [Test]
        public void FromCurrent_ReadsTheTargetWhenTheTweenPlays()
        {
            FakeTweenTarget target = NewTarget();
            target.writableField = 4f;

            TweenCore tween = TweenCore.CreateTween();
            TweenCoreProperty<float> property =
                tween.NewProperty(target, nameof(FakeTweenTarget.writableField), 0f, 10f, 1f);
            property.FromCurrent();

            tween.Play();
            tween.Update(0.5f);

            Assert.That(target.writableField, Is.EqualTo(7f).Within(TOLERANCE),
                "FromCurrent should override the start value of 0 with the target's 4.");
        }

        [Test]
        public void FromCurrent_OnANonReflectionTween_IsIgnored()
        {
            // "FromCurrent() - reflection tweens only." It warns and carries on rather than
            // throwing or silently changing the start value.
            LogAssert.ignoreFailingMessages = true;

            TweenCoreProperty<float> property = new TweenCoreProperty<float>(2f, 10f, 1f);

            Assert.DoesNotThrow(() => property.FromCurrent());
            Assert.IsFalse(property.FromCurrentValue);
        }

        [Test]
        public void Additive_OnAReflectionTween_OffsetsFromTheTargetsValue()
        {
            FakeTweenTarget target = NewTarget();
            target.writableField = 5f;

            TweenCore tween = TweenCore.CreateTween();
            tween.NewProperty(target, nameof(FakeTweenTarget.writableField), 0f, 3f, 1f)
                 .SetIsAdditive(true);

            tween.Play();
            tween.Update(1f);

            Assert.That(target.writableField, Is.EqualTo(8f).Within(TOLERANCE),
                "The offset should be applied to whatever the target held at Play().");
        }

        // ----- The two guards ----- \\

        [Test]
        public void AReadOnlyProperty_MarksThePropertyBrokenInsteadOfThrowing()
        {
            // The 1.1 changelog : "The inspector offered read-only properties, which then threw at
            // runtime." A property with no setter must be refused up front.
            LogAssert.ignoreFailingMessages = true;

            FakeTweenTarget target = NewTarget();

            TweenCore tween = TweenCore.CreateTween();
            TweenCoreProperty<float> property =
                tween.NewProperty(target, nameof(FakeTweenTarget.ReadOnlyProperty), 0f, 10f, 1f);

            Assert.DoesNotThrow(() => tween.Play());
            Assert.IsTrue(property.IsBroken);
        }

        [Test]
        public void APropertyOfTheWrongType_MarksThePropertyBroken()
        {
            // The resolver type checks properties and fields on separate branches. This covers the
            // property branch; AMemberOfTheWrongType covers the field one. A single test hits only
            // one of the two guards, which is how the property branch was left unverified.
            LogAssert.ignoreFailingMessages = true;

            FakeTweenTarget target = NewTarget();

            TweenCore tween = TweenCore.CreateTween();
            TweenCoreProperty<float> property =
                tween.NewProperty(target, nameof(FakeTweenTarget.WritableVectorProperty), 0f, 10f, 1f);

            Assert.DoesNotThrow(() => tween.Play());
            Assert.IsTrue(property.IsBroken);
        }

        [Test]
        public void AMemberOfTheWrongType_MarksThePropertyBroken()
        {
            // Not stated in the README, but the resolver guards it. Pinned so the guard cannot be
            // dropped silently : tweening a Vector3 field as a float would otherwise misbehave at
            // the first write rather than being refused.
            LogAssert.ignoreFailingMessages = true;

            FakeTweenTarget target = NewTarget();

            TweenCore tween = TweenCore.CreateTween();
            TweenCoreProperty<float> property =
                tween.NewProperty(target, nameof(FakeTweenTarget.vectorField), 0f, 10f, 1f);

            Assert.DoesNotThrow(() => tween.Play());
            Assert.IsTrue(property.IsBroken);
        }

        [Test]
        public void AnUnresolvableName_MarksThePropertyBrokenInsteadOfThrowing()
        {
            LogAssert.ignoreFailingMessages = true;

            FakeTweenTarget target = NewTarget();

            TweenCore tween = TweenCore.CreateTween();
            TweenCoreProperty<float> property =
                tween.NewProperty(target, "thisMemberDoesNotExist", 0f, 10f, 1f);

            Assert.DoesNotThrow(() => tween.Play());
            Assert.IsTrue(property.IsBroken, "An unresolvable name should mark the property broken.");
        }

        [Test]
        public void AnUnresolvableName_DoesNotThrowOnEveryFrame()
        {
            LogAssert.ignoreFailingMessages = true;

            FakeTweenTarget target = NewTarget();

            TweenCore tween = TweenCore.CreateTween();
            tween.NewProperty(target, "thisMemberDoesNotExist", 0f, 10f, 1f);
            tween.Play();

            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 5; i++) tween.Update(0.1f);
            });
        }

        [Test]
        public void ADestroyedTarget_EndsTheTweenQuietlyInsteadOfThrowing()
        {
            LogAssert.ignoreFailingMessages = true;

            GameObject go = NewObject();
            Transform target = go.transform;

            TweenCore tween = TweenCore.CreateTween();
            tween.NewProperty(target, "localPosition", Vector3.zero, new Vector3(10f, 0f, 0f), 1f);
            tween.Play();
            tween.Update(0.25f);

            Object.DestroyImmediate(go);

            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 5; i++) tween.Update(0.1f);
            });

            Assert.IsTrue(tween.IsFinished, "A tween whose target is gone should end, not run forever.");
        }
    }
}
