using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Tweening;

namespace Tweening.Tests
{
    /// <summary>
    /// Behavioural coverage of the inspector's property picker filtering, from the 1.1 changelog :
    ///
    ///   "The inspector offered read-only properties, which then threw at runtime. It now lists
    ///    writable properties *and* writable fields, and skips obsolete members."
    ///
    /// The picker's drawing is out of scope by design - pixel assertions test the drawing, not a
    /// decision. What is covered is the rule that decides which members appear, which is a pure
    /// function of the target type and the value type.
    ///
    /// That function is private and static. It is reached by reflection rather than by widening
    /// its accessibility, because the production code is not to be changed by this work. The type
    /// is found by scanning the loaded assemblies so the test assembly needs no reference to
    /// TweenCore.Editor.
    /// </summary>
    public class EditorPickerFilteringTests
    {
        private readonly List<UnityEngine.Object> _assets = new List<UnityEngine.Object>();
        private readonly List<GameObject> _spawned = new List<GameObject>();

        private static MethodInfo _getTweenableMemberNames;

        [OneTimeSetUp]
        public void FindThePicker()
        {
            Type editorType = null;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                editorType = assembly.GetType("Tweening.EditorScripts.TweenCorePropertyBaseEditor");
                if (editorType != null) break;
            }

            Assert.IsNotNull(editorType,
                "TweenCorePropertyBaseEditor was not found in any loaded assembly. If it was renamed or moved, this test file needs updating.");

            _getTweenableMemberNames = editorType.GetMethod(
                "GetTweenableMemberNames", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(_getTweenableMemberNames,
                "GetTweenableMemberNames was not found. If its signature changed, this test file needs updating.");
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _assets.Count; i++)
            {
                if (_assets[i] != null) UnityEngine.Object.DestroyImmediate(_assets[i]);
            }

            for (int i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null) UnityEngine.Object.DestroyImmediate(_spawned[i]);
            }

            _assets.Clear();
            _spawned.Clear();
        }

        private FakeTweenTarget NewTarget()
        {
            FakeTweenTarget target = ScriptableObject.CreateInstance<FakeTweenTarget>();
            _assets.Add(target);
            return target;
        }

        private static string[] MembersOf(UnityEngine.Object target, Type valueType)
        {
            return (string[])_getTweenableMemberNames.Invoke(null, new object[] { target, valueType });
        }

        private string[] FloatMembers()
        {
            return MembersOf(NewTarget(), typeof(float));
        }

        // ----- What is offered ----- \\

        [Test]
        public void AWritableProperty_IsOffered()
        {
            Assert.Contains(nameof(FakeTweenTarget.WritableProperty), FloatMembers());
        }

        [Test]
        public void APublicField_IsOffered()
        {
            // "It now lists writable properties *and* writable fields."
            Assert.Contains(nameof(FakeTweenTarget.writableField), FloatMembers());
        }

        [Test]
        public void MembersOfTheRequestedTypeOnly_AreOffered()
        {
            string[] floats = FloatMembers();

            Assert.Contains(nameof(FakeTweenTarget.writableField), floats);
            CollectionAssert.DoesNotContain(floats, nameof(FakeTweenTarget.vectorField),
                "A Vector3 field must not be offered for a float property.");
            CollectionAssert.DoesNotContain(floats, nameof(FakeTweenTarget.WritableVectorProperty));
        }

        [Test]
        public void AskingForVector3_OffersTheVector3Members()
        {
            string[] vectors = MembersOf(NewTarget(), typeof(Vector3));

            Assert.Contains(nameof(FakeTweenTarget.vectorField), vectors);
            Assert.Contains(nameof(FakeTweenTarget.WritableVectorProperty), vectors);
            CollectionAssert.DoesNotContain(vectors, nameof(FakeTweenTarget.writableField));
        }

        [Test]
        public void ARealComponent_OffersItsWritableVectorMembers()
        {
            GameObject go = new GameObject("picker target");
            _spawned.Add(go);

            string[] vectors = MembersOf(go.transform, typeof(Vector3));

            Assert.Contains("localPosition", vectors);
            Assert.Contains("localScale", vectors);
        }

        // ----- What is filtered out ----- \\

        [Test]
        public void AReadOnlyProperty_IsNotOffered()
        {
            // The defect this filter exists for : "a read only property compiles fine here and
            // then throws \"Property set method not found\" at runtime."
            CollectionAssert.DoesNotContain(FloatMembers(), nameof(FakeTweenTarget.ReadOnlyProperty));
        }

        [Test]
        public void AnObsoleteProperty_IsNotOffered()
        {
#pragma warning disable 618
            CollectionAssert.DoesNotContain(FloatMembers(), nameof(FakeTweenTarget.ObsoleteProperty));
#pragma warning restore 618
        }

        [Test]
        public void AnObsoleteField_IsNotOffered()
        {
#pragma warning disable 618
            CollectionAssert.DoesNotContain(FloatMembers(), nameof(FakeTweenTarget.obsoleteField));
#pragma warning restore 618
        }

        [Test]
        public void AReadonlyField_IsNotOffered()
        {
            CollectionAssert.DoesNotContain(FloatMembers(), nameof(FakeTweenTarget.readonlyField));
        }

        [Test]
        public void AConstField_IsNotOffered()
        {
            CollectionAssert.DoesNotContain(FloatMembers(), nameof(FakeTweenTarget.ConstField));
        }

        [Test]
        public void AnIndexer_IsNotOffered()
        {
            // An indexer's reflected name is "Item" and it takes a parameter, so there is no single
            // member the tween could write.
            CollectionAssert.DoesNotContain(FloatMembers(), "Item");
        }

        // ----- Shape of the result ----- \\

        [Test]
        public void TheOfferedNames_AreSorted()
        {
            // Transform on purpose. The fixture's two Vector3 members happen to come back from
            // reflection already in ordinal order, so sorting them is a no-op and the test could
            // not fail. Transform returns a dozen members in declaration order, which is not
            // alphabetical.
            GameObject go = new GameObject("sort target");
            _spawned.Add(go);

            string[] names = MembersOf(go.transform, typeof(Vector3));
            string[] sorted = (string[])names.Clone();
            Array.Sort(sorted, StringComparer.Ordinal);

            Assert.Greater(names.Length, 2, "This test needs several members to be meaningful.");
            CollectionAssert.AreEqual(sorted, names, "The picker list should be in a stable, sorted order.");
        }

        [Test]
        public void ANullTarget_OffersNothing()
        {
            Assert.IsEmpty(MembersOf(null, typeof(float)));
        }

        [Test]
        public void ANullValueType_OffersNothing()
        {
            Assert.IsEmpty(MembersOf(NewTarget(), null));
        }
    }
}
