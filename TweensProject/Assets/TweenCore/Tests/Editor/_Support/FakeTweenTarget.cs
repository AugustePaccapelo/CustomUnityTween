using System;
using UnityEngine;

namespace Tweening.Tests
{
    /// <summary>
    /// A target with one of every member shape the reflection layer and the inspector's property
    /// picker have to tell apart : writable and read only properties, plain public fields, an
    /// indexer, readonly and const fields, and obsolete members that should never be offered.
    ///
    /// Kept deliberately dull. The point is the member shapes, not the behaviour.
    ///
    /// A ScriptableObject rather than a MonoBehaviour on purpose. Unity refuses to attach a
    /// behaviour that lives in an Editor assembly to a GameObject ("Can't add script behaviour ...
    /// because it is an editor script"), and this fixture has to live beside the tests that use
    /// it. What matters for the reflection layer is that the target is a UnityEngine.Object with
    /// the right member shapes, which this is.
    /// </summary>
    public class FakeTweenTarget : ScriptableObject
    {
        // ----- Should be offered ----- \\

        public float writableField;
        public Vector3 vectorField;

        public float WritableProperty { get; set; }
        public Vector3 WritableVectorProperty { get; set; }

        // ----- Should not be offered ----- \\

        /// <summary>No setter : offering this one in the inspector used to throw at runtime.</summary>
        public float ReadOnlyProperty => 42f;

        /// <summary>Cannot be assigned after construction.</summary>
        public readonly float readonlyField;

        /// <summary>A literal, not a storage location.</summary>
        public const float ConstField = 7f;

        /// <summary>Takes an index, so there is no single member to write.</summary>
        public float this[int index]
        {
            get => index;
            set { }
        }

        [Obsolete("Here so the picker has an obsolete property to skip.")]
        public float ObsoleteProperty { get; set; }

        [Obsolete("Here so the picker has an obsolete field to skip.")]
        public float obsoleteField;
    }
}
