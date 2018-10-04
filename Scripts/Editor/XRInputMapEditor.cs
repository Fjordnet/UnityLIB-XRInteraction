using System;
using System.Collections;
using System.Collections.Generic;
using Fjord.Common.Data;
using Fjord.Common.Types;
using Fjord.XRInteraction.XRInput;
using Fjord.XRInteraction.XRUser;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Fjord.Common
{
    [CustomEditor(typeof(XRInputMap))]
    public class XRInputMapEditor : Editor
    {
        private ReorderableList _list;

        private void OnEnable()
        {
            _list = new ReorderableList(serializedObject,
                serializedObject.FindProperty("_descriptions"),
                true, true, true, true);

            _list.drawElementCallback += DrawElementCallback;
            _list.drawHeaderCallback += DrawHeaderCallback;
            _list.elementHeightCallback += ElementHeightCallback;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty property = _list.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty inputNameProperty = property.FindPropertyRelative("_inputName");
            SerializedProperty chiralityProperty = property.FindPropertyRelative("_chirality");
            SerializedProperty inputTypeProperty = property.FindPropertyRelative("_inputType");

            Rect currentRect = rect;
            currentRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(currentRect, 
                "-- " + chiralityProperty.enumValue<Chirality>() + " " + 
                inputNameProperty.enumDisplayNames[inputNameProperty.enumValueIndex] + 
                " --");
            currentRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(currentRect, inputNameProperty);
            currentRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(currentRect, chiralityProperty);
            currentRect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(currentRect, inputTypeProperty);
            currentRect.x += 32;
            currentRect.width -= 32;

            SerializedProperty buttonKeyCodeProperty = property.FindPropertyRelative("_buttonKeycode");
            SerializedProperty axis0NameProperty = property.FindPropertyRelative("_axis0Name");
            SerializedProperty axis1NameProperty = property.FindPropertyRelative("_axis1Name");
            SerializedProperty downThresholdProperty = property.FindPropertyRelative("_downThreshold");
            SerializedProperty upThresholdProperty = property.FindPropertyRelative("_upThreshold");

            switch (inputTypeProperty.enumValue<XRInputType>())
            {
                case XRInputType.Button:
                    currentRect.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(currentRect, buttonKeyCodeProperty);
                    break;
                case XRInputType.ThresholdButton:
                    currentRect.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(currentRect, axis0NameProperty);
                    currentRect.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(currentRect, downThresholdProperty);
                    currentRect.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(currentRect, upThresholdProperty);
                    break;
                case XRInputType.Axis1D:
                    currentRect.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(currentRect, axis0NameProperty);
                    break;
                case XRInputType.Axis2D:
                    currentRect.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(currentRect, axis0NameProperty);
                    currentRect.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(currentRect, axis1NameProperty);
                    break;
            }
        }

        private float ElementHeightCallback(int index)
        {
            float height = EditorGUIUtility.singleLineHeight * 4.5f;
            SerializedProperty inputTypeProperty = _list.serializedProperty.GetArrayElementAtIndex(index)
                .FindPropertyRelative("_inputType");
            switch (inputTypeProperty.enumValue<XRInputType>())
            {
                case XRInputType.Button:
                    height += EditorGUIUtility.singleLineHeight;
                    break;
                case XRInputType.ThresholdButton:
                    height += EditorGUIUtility.singleLineHeight * 3;
                    break;
                case XRInputType.Axis1D:
                    height += EditorGUIUtility.singleLineHeight;
                    break;
                case XRInputType.Axis2D:
                    height += EditorGUIUtility.singleLineHeight * 2;
                    break;
            }
            return height;
        }

        private void DrawHeaderCallback(Rect rect)
        {
            GUI.Label(rect, "Descriptions");
        }
    }
}