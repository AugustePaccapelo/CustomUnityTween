#if UNITY_EDITOR
// In editor the script will compile
// When building the project, this script will be ignored
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Tweening;

// Author : Auguste Paccapelo

namespace Tweening.EditorScripts
{
    [CustomEditor(typeof(TweenCoreComponent))]
    public class TweenCoreComponentEditor : Editor
    {
        // ---------- VARIABLES ---------- \\

        // ----- Objects ----- \\

        private SerializedProperty _name;
        private SerializedProperty _playOnStart;
        private SerializedProperty _isParallel;
        private SerializedProperty _isLoop;
        private SerializedProperty _isInfinite;
        private SerializedProperty _numIteration;
        private SerializedProperty _destroyWhenFinish;
        private SerializedProperty _surviveOnUnload;
        private SerializedProperty _useUnscaledTime;
        private SerializedProperty _unityEvents;
        private SerializedProperty _properties;

        private ReorderableList _propertiesEditorList;

        // ----- Others ----- \\

        // Every type the runtime supports that Unity can also serialize.
        // decimal is missing on purpose : Unity's serializer has no support for it,
        // so it stays available from code only.
        private static readonly (string label, Type type)[] _supportedTypes =
        {
            ("float", typeof(float)),
            ("double", typeof(double)),
            ("int", typeof(int)),
            ("uint", typeof(uint)),
            ("long", typeof(long)),
            ("ulong", typeof(ulong)),
            ("Vector2", typeof(Vector2)),
            ("Vector3", typeof(Vector3)),
            ("Vector4", typeof(Vector4)),
            ("Quaternion", typeof(Quaternion)),
            ("Color", typeof(Color)),
            ("Color32", typeof(Color32)),
        };

        // ---------- FUNCTIONS ---------- \\

        // ----- Buil-in ----- \\

        private void OnEnable()
        {
            GetProperties();
            SetPropertiesList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_name);
            EditorGUILayout.PropertyField(_playOnStart);
            EditorGUILayout.PropertyField(_isParallel);

            EditorGUILayout.PropertyField(_isLoop);

            if (_isLoop.boolValue)
            {
                EditorGUILayout.PropertyField(_isInfinite);

                if (!_isInfinite.boolValue)
                {
                    EditorGUILayout.PropertyField(_numIteration);
                }
            }

            EditorGUILayout.PropertyField(_destroyWhenFinish);
            EditorGUILayout.PropertyField(_surviveOnUnload);
            EditorGUILayout.PropertyField(_useUnscaledTime);

            _propertiesEditorList.DoLayoutList();

            EditorGUILayout.PropertyField(_unityEvents);

            serializedObject.ApplyModifiedProperties();
        }

        // ----- My Functions ----- \\

        private void GetProperties()
        {
            _name = serializedObject.FindProperty("_name");
            _playOnStart = serializedObject.FindProperty("_playOnStart");
            _isParallel = serializedObject.FindProperty("_isParallel");
            _isLoop = serializedObject.FindProperty("_isLoop");
            _isInfinite = serializedObject.FindProperty("_isInfinite");
            _numIteration = serializedObject.FindProperty("_numIteration");
            _destroyWhenFinish = serializedObject.FindProperty("_DestroyWhenFinished");
            _surviveOnUnload = serializedObject.FindProperty("_surviveOnUnload");
            _useUnscaledTime = serializedObject.FindProperty("_useUnscaledTime");
            _unityEvents = serializedObject.FindProperty("_unityEvents");
            _properties = serializedObject.FindProperty("_properties");
        }

        private void SetPropertiesList()
        {
            _propertiesEditorList = new ReorderableList(serializedObject, _properties, true, true, true, true);

            _propertiesEditorList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Tween Properties");
            };

            _propertiesEditorList.elementHeightCallback = index =>
            {
                SerializedProperty element = _properties.GetArrayElementAtIndex(index);
                float height = EditorGUI.GetPropertyHeight(element, true);

                if (element.isExpanded) height += EditorGUIUtility.singleLineHeight * 0.5f;

                return height;
            };

            _propertiesEditorList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = _properties.GetArrayElementAtIndex(index);
                string elementName = "Property " + index;
                rect.x += 10f;
                rect.width -= 20f;
                EditorGUI.PropertyField(rect, element, new GUIContent(elementName), true);
            };

            _propertiesEditorList.onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
            {
                ButtonNewPropertyPressed();
            };
        }

        private void ButtonNewPropertyPressed()
        {
            GenericMenu menu = new GenericMenu();

            foreach ((string label, Type type) supported in _supportedTypes)
            {
                Type valueType = supported.type;
                menu.AddItem(new GUIContent(supported.label), false, () => AddProperty(valueType));
            }

            menu.ShowAsContext();
        }

        /// <summary>
        /// Added through the SerializedObject so the change is undoable and recorded as a
        /// prefab override, instead of poking the target instance directly.
        /// </summary>
        private void AddProperty(Type valueType)
        {
            serializedObject.Update();

            int index = _properties.arraySize;
            _properties.InsertArrayElementAtIndex(index);

            Type genericType = typeof(TweenCoreProperty<>).MakeGenericType(valueType);
            _properties.GetArrayElementAtIndex(index).managedReferenceValue = Activator.CreateInstance(genericType);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
