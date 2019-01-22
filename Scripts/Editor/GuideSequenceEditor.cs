using System;
using System.Collections;
using System.Collections.Generic;
using Fjord.Common.Data;
using Fjord.Common.Types;
using Fjord.XRInteraction.Attachables;
using Fjord.XRInteraction.XRInput;
using Fjord.XRInteraction.XRUser;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Fjord.XRInteraction
{
    [CustomEditor(typeof(GuideSequence))]
    public class GuideSequenceEditor : Editor
    {
        private ReorderableList _list;
        private SerializedProperty _sequenceProgressedProperty;
        private SerializedProperty _sequenceCompletedProperty;
        
        private void OnEnable()
        {
            _list = new ReorderableList(serializedObject,
                serializedObject.FindProperty("_sequence"),
                true, true, true, true);

            _list.drawElementCallback += DrawElementCallback;
            _list.drawHeaderCallback += DrawHeaderCallback;
            _list.elementHeightCallback += ElementHeightCallback;

            _sequenceProgressedProperty = serializedObject.FindProperty("_sequenceProgressed");
            _sequenceCompletedProperty = serializedObject.FindProperty("_sequenceCompleted");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _list.DoLayoutList();
            EditorGUILayout.PropertyField(_sequenceProgressedProperty);
            EditorGUILayout.PropertyField(_sequenceCompletedProperty);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty property = _list.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, property);
        }

        private float ElementHeightCallback(int index)
        {
            return EditorGUIUtility.singleLineHeight * 1.2f;
        }

        private void DrawHeaderCallback(Rect rect)
        {
            GUI.Label(rect, "Guide Sequence");
        }
    }
}