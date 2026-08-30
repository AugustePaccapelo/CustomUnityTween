#if UNITY_EDITOR
// In editor the script will compile
// When building the project, this script will be ignored
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Tweening;

// Author : Auguste Paccapelo

namespace Tweening.EditorScripts
{
    [CustomPropertyDrawer(typeof(TweenCorePropertyBase), true)]
    public class TweenCorePropertyBaseEditor : PropertyDrawer
    {
        // ----- CLASS ----- \\

        /// <summary>
        /// Everything one drawn TweenProperty needs, plus the layout cursor.
        /// The same walk both measures and draws, so the two can never disagree.
        /// </summary>
        private class TweenPropertyEditorContext
        {
            // ----- VARIABLES ----- \\

            // Tween related \\

            public SerializedProperty propTweenTargetObj;
            public SerializedProperty propLastKnownTweenTargetGO;
            public SerializedProperty property;
            public SerializedProperty propCurrentObject;
            public SerializedProperty propLastKnownObject;
            public SerializedProperty propCurrentPropertyChoosedIndex;
            public SerializedProperty propPropertyChoosedName;

            public SerializedProperty propIsEmpty;
            public SerializedProperty propTweenType;
            public SerializedProperty propTweenEase;
            public SerializedProperty propDuration;
            public SerializedProperty propDelay;
            public SerializedProperty propTypeAnimCurve;
            public SerializedProperty propEaseAnimCurve;
            public SerializedProperty propFromCurrentValue;
            public SerializedProperty propIsAdd;
            public SerializedProperty propStartValue;
            public SerializedProperty propEndValue;
            public SerializedProperty propUnityEvents;

            public long referenceId;

            public GameObject TargetGameObject => propTweenTargetObj?.objectReferenceValue as GameObject;
            public UnityEngine.Object TargetObject => propCurrentObject?.objectReferenceValue;

            // Unity Editor related \\

            public bool Draw { get; private set; }

            private float _x;
            private float _width;
            private float _cursorY;
            private float _startY;

            public float TotalHeight => _cursorY - _startY;

            public TweenPropertyEditorContext(SerializedProperty property, Rect position, bool draw)
            {
                this.property = property;
                Draw = draw;

                _x = position.x;
                _width = position.width;
                _startY = position.y;
                _cursorY = position.y;

                InitSerializedProperties();

                if (property != null) referenceId = property.managedReferenceId;
            }

            public void InitSerializedProperties()
            {
                if (property == null) return;

                propTweenTargetObj = property.FindPropertyRelative("_tweenTargetObj");
                propLastKnownTweenTargetGO = property.FindPropertyRelative("_lastKnownTweenTargetGO");

                propCurrentObject = property.FindPropertyRelative("obj");
                propLastKnownObject = property.FindPropertyRelative("_lastKnownObject");
                propCurrentPropertyChoosedIndex = property.FindPropertyRelative("propertyIndex");
                propPropertyChoosedName = property.FindPropertyRelative("propertyName");

                propIsEmpty = property.FindPropertyRelative("isEmpty");
                propTweenType = property.FindPropertyRelative("type");
                propTweenEase = property.FindPropertyRelative("ease");
                propDuration = property.FindPropertyRelative("duration");
                propDelay = property.FindPropertyRelative("delay");
                propTypeAnimCurve = property.FindPropertyRelative("typeAnimationCurve");
                propEaseAnimCurve = property.FindPropertyRelative("easeAnimationCurve");
                propFromCurrentValue = property.FindPropertyRelative("fromCurrentValue");
                propIsAdd = property.FindPropertyRelative("isIncreasingValue");
                propStartValue = property.FindPropertyRelative("_startValue");
                propEndValue = property.FindPropertyRelative("_finalValue");
                propUnityEvents = property.FindPropertyRelative("_unityEvents");
            }

            /// <summary>
            /// Reserve the next row and advance the cursor, whether or not anything is drawn.
            /// </summary>
            public Rect Row(float height)
            {
                Rect rect = new Rect(_x, _cursorY, _width, height);
                _cursorY += height + EditorGUIUtility.standardVerticalSpacing;
                return rect;
            }

            public Rect Line() => Row(EditorGUIUtility.singleLineHeight);

            public void DrawProperty(SerializedProperty property, string name = "")
            {
                if (property == null) return;

                Rect rect = Row(EditorGUI.GetPropertyHeight(property, true));

                if (!Draw) return;

                if (string.IsNullOrEmpty(name)) EditorGUI.PropertyField(rect, property, true);
                else EditorGUI.PropertyField(rect, property, new GUIContent(name), true);
            }
        }

        /// <summary>
        /// What was resolved for one drawn property, kept only as long as it stays valid.
        /// </summary>
        private class ReflectionCache
        {
            public int targetGameObjectId;
            public int targetObjectId;
            public List<Component> components = new List<Component>();
            public string[] componentNames = Array.Empty<string>();
            public string[] memberNames = Array.Empty<string>();
        }

        // ---------- VARIABLES ---------- \\

        private float Line => EditorGUIUtility.singleLineHeight;

        private readonly Dictionary<long, ReflectionCache> _caches = new Dictionary<long, ReflectionCache>();

        // Built once instead of on every repaint.
        private static readonly TweenCoreType[] _typeValues =
        {
            TweenCoreType.Linear, TweenCoreType.Back, TweenCoreType.Bounce, TweenCoreType.Circ,
            TweenCoreType.Cubic, TweenCoreType.Elastic, TweenCoreType.Expo, TweenCoreType.Quad,
            TweenCoreType.Quart, TweenCoreType.Quint, TweenCoreType.Sine, TweenCoreType.CustomCurve,
        };

        private static readonly string[] _typeLabels =
        {
            "Linear", "Back", "Bounce", "Circ", "Cubic", "Elastic",
            "Expo", "Quad", "Quart", "Quint", "Sine", "CustomCurve",
        };

        private static readonly TweenCoreEase[] _easeValues =
        {
            TweenCoreEase.In, TweenCoreEase.Out, TweenCoreEase.InOut,
            TweenCoreEase.OutIn, TweenCoreEase.CustomCurve,
        };

        private static readonly string[] _easeLabels = { "In", "Out", "InOut", "OutIn", "CustomCurve" };

        // ---------- FUNCTIONS ---------- \\

        // ----- Buil-in ----- \\

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            Layout(position, property, label, draw: true);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Rect measureRect = new Rect(0f, 0f, EditorGUIUtility.currentViewWidth, 0f);
            return Layout(measureRect, property, label, draw: false);
        }

        // ----- My functions ----- \\

        /// <summary>
        /// Single source of truth for the drawer : walked once to measure, once to draw.
        /// </summary>
        private float Layout(Rect position, SerializedProperty property, GUIContent label, bool draw)
        {
            TweenPropertyEditorContext context = new TweenPropertyEditorContext(property, position, draw);

            // Show name of TweenProperty & ability to collapse it
            Rect foldoutRect = context.Line();
            if (draw) property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            // If property is collapsed, the user can't interact with it so no need to compute
            if (!property.isExpanded) return context.TotalHeight;

            if (draw) EditorGUI.indentLevel++;

            context.DrawProperty(context.propIsEmpty);

            // If not empty, handle the object related fields
            if (context.propIsEmpty != null && !context.propIsEmpty.boolValue)
            {
                HandlePropertyIsNotEmpty(context);
            }

            DrawTypePopup(context);

            if (GetEnum(context.propTweenType, TweenCoreType.Linear) == TweenCoreType.CustomCurve)
            {
                context.DrawProperty(context.propTypeAnimCurve);
            }

            DrawEasePopup(context);

            if (GetEnum(context.propTweenEase, TweenCoreEase.In) == TweenCoreEase.CustomCurve)
            {
                context.DrawProperty(context.propEaseAnimCurve);
            }

            bool isEmpty = context.propIsEmpty != null && context.propIsEmpty.boolValue;
            bool fromCurrent = context.propFromCurrentValue != null && context.propFromCurrentValue.boolValue;

            // If isEmpty, there is no target to read a current value from
            if (!isEmpty)
            {
                context.DrawProperty(context.propFromCurrentValue);

                if (fromCurrent)
                {
                    context.DrawProperty(context.propIsAdd);
                }
            }

            if (!fromCurrent || isEmpty)
            {
                context.DrawProperty(context.propStartValue);
            }

            string endName = context.propIsAdd != null && context.propIsAdd.boolValue ? "Value to add" : "";
            context.DrawProperty(context.propEndValue, endName);

            context.DrawProperty(context.propDuration);
            context.DrawProperty(context.propDelay);
            context.DrawProperty(context.propUnityEvents);

            if (draw) EditorGUI.indentLevel--;

            return context.TotalHeight;
        }

        private static TEnum GetEnum<TEnum>(SerializedProperty property, TEnum fallback) where TEnum : struct, Enum
        {
            // intValue is the underlying enum value. enumValueIndex is the position in the name
            // list, which only matches while the enum is declared 0..n with no explicit values.
            if (property == null) return fallback;
            return (TEnum)Enum.ToObject(typeof(TEnum), property.intValue);
        }

        private void DrawTypePopup(TweenPropertyEditorContext context)
        {
            Rect rect = context.Line();
            if (!context.Draw || context.propTweenType == null) return;

            int currentIndex = Array.IndexOf(_typeValues, GetEnum(context.propTweenType, TweenCoreType.Linear));
            if (currentIndex < 0) currentIndex = 0;

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(rect, "Type", currentIndex, _typeLabels);
            if (EditorGUI.EndChangeCheck())
            {
                context.propTweenType.intValue = (int)_typeValues[newIndex];
            }
        }

        private void DrawEasePopup(TweenPropertyEditorContext context)
        {
            Rect rect = context.Line();
            if (!context.Draw || context.propTweenEase == null) return;

            int currentIndex = Array.IndexOf(_easeValues, GetEnum(context.propTweenEase, TweenCoreEase.In));
            if (currentIndex < 0) currentIndex = 0;

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(rect, "Ease", currentIndex, _easeLabels);
            if (EditorGUI.EndChangeCheck())
            {
                context.propTweenEase.intValue = (int)_easeValues[newIndex];
            }
        }

        private void HandlePropertyIsNotEmpty(TweenPropertyEditorContext context)
        {
            context.DrawProperty(context.propTweenTargetObj);

            GameObject targetGO = context.TargetGameObject;

            if (targetGO == null)
            {
                if (context.Draw && context.propCurrentObject != null && context.propCurrentObject.objectReferenceValue != null)
                {
                    context.propCurrentObject.objectReferenceValue = null;
                    if (context.propCurrentPropertyChoosedIndex != null) context.propCurrentPropertyChoosedIndex.intValue = 0;
                    if (context.propPropertyChoosedName != null) context.propPropertyChoosedName.stringValue = string.Empty;
                }

                _caches.Remove(context.referenceId);
                return;
            }

            ReflectionCache cache = GetCache(context, targetGO);

            DrawComponentPopup(context, cache);

            if (context.TargetObject == null) return;

            DrawMemberPopup(context, cache);
        }

        /// <summary>
        /// Caches are keyed by managed reference id, but drawers are pooled and reused across
        /// objects, so the cache is only trusted while it still describes the same target.
        /// </summary>
        private ReflectionCache GetCache(TweenPropertyEditorContext context, GameObject targetGO)
        {
            int targetGOId = targetGO.GetInstanceID();
            UnityEngine.Object targetObject = context.TargetObject;
            int targetObjectId = targetObject != null ? targetObject.GetInstanceID() : 0;

            if (_caches.TryGetValue(context.referenceId, out ReflectionCache cache)
                && cache.targetGameObjectId == targetGOId
                && cache.targetObjectId == targetObjectId)
            {
                return cache;
            }

            cache = new ReflectionCache
            {
                targetGameObjectId = targetGOId,
                targetObjectId = targetObjectId,
            };

            targetGO.GetComponents(cache.components);

            cache.componentNames = new string[cache.components.Count];
            for (int i = 0; i < cache.components.Count; i++)
            {
                cache.componentNames[i] = cache.components[i] != null ? cache.components[i].GetType().Name : "<missing>";
            }

            cache.memberNames = targetObject != null
                ? GetTweenableMemberNames(targetObject, GetValueType(context.property))
                : Array.Empty<string>();

            _caches[context.referenceId] = cache;
            return cache;
        }

        private void DrawComponentPopup(TweenPropertyEditorContext context, ReflectionCache cache)
        {
            Rect rect = context.Line();

            if (!context.Draw || cache.components.Count == 0 || context.propCurrentObject == null) return;

            int currentIndex = cache.components.IndexOf(context.TargetObject as Component);

            if (currentIndex < 0)
            {
                currentIndex = 0;
                context.propCurrentObject.objectReferenceValue = cache.components[0];
            }

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(rect, "Component", currentIndex, cache.componentNames);
            if (EditorGUI.EndChangeCheck())
            {
                context.propCurrentObject.objectReferenceValue = cache.components[newIndex];

                // The member list belongs to the previous component.
                _caches.Remove(context.referenceId);

                if (context.propPropertyChoosedName != null) context.propPropertyChoosedName.stringValue = string.Empty;
                if (context.propCurrentPropertyChoosedIndex != null) context.propCurrentPropertyChoosedIndex.intValue = 0;
            }
        }

        private void DrawMemberPopup(TweenPropertyEditorContext context, ReflectionCache cache)
        {
            Rect rect = context.Line();

            if (!context.Draw || context.propPropertyChoosedName == null) return;

            if (cache.memberNames.Length == 0)
            {
                EditorGUI.LabelField(rect, " ", $"No writable {GetValueType(context.property)?.Name} member on this component");
                return;
            }

            // Resolved from the stored name, not the stored index : the list can change order
            // when the component gains or loses members.
            int currentIndex = Array.IndexOf(cache.memberNames, context.propPropertyChoosedName.stringValue);

            if (currentIndex < 0)
            {
                currentIndex = 0;
                context.propPropertyChoosedName.stringValue = cache.memberNames[0];
                if (context.propCurrentPropertyChoosedIndex != null) context.propCurrentPropertyChoosedIndex.intValue = 0;
            }

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(rect, "Property", currentIndex, cache.memberNames);
            if (EditorGUI.EndChangeCheck())
            {
                context.propPropertyChoosedName.stringValue = cache.memberNames[newIndex];
                if (context.propCurrentPropertyChoosedIndex != null) context.propCurrentPropertyChoosedIndex.intValue = newIndex;
            }
        }

        private static Type GetValueType(SerializedProperty property)
        {
            object value = property?.managedReferenceValue;
            if (value == null) return null;

            Type type = value.GetType();
            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(TweenCoreProperty<>))
                {
                    return type.GetGenericArguments()[0];
                }
                type = type.BaseType;
            }

            return null;
        }

        /// <summary>
        /// Only members that can actually be written : a read only property compiles fine here
        /// and then throws "Property set method not found" at runtime.
        /// </summary>
        private static string[] GetTweenableMemberNames(UnityEngine.Object target, Type valueType)
        {
            if (target == null || valueType == null) return Array.Empty<string>();

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            List<string> names = new List<string>();
            Type targetType = target.GetType();

            foreach (PropertyInfo property in targetType.GetProperties(flags))
            {
                if (property.PropertyType != valueType) continue;
                if (!property.CanWrite || property.GetSetMethod() == null) continue;
                if (property.GetIndexParameters().Length > 0) continue;
                if (property.IsDefined(typeof(ObsoleteAttribute), true)) continue;

                names.Add(property.Name);
            }

            foreach (FieldInfo field in targetType.GetFields(flags))
            {
                if (field.FieldType != valueType) continue;
                // const fields are implicitly static, so BindingFlags.Instance above never
                // returns one : only readonly has to be filtered here.
                if (field.IsInitOnly) continue;
                if (field.IsDefined(typeof(ObsoleteAttribute), true)) continue;

                names.Add(field.Name);
            }

            names.Sort(StringComparer.Ordinal);
            return names.ToArray();
        }
    }
}
#endif
