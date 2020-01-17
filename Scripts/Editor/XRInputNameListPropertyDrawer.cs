using System;
using System.Collections.Generic;
using Fjord.Common.Data;
using Fjord.Common.Types;
using Fjord.Common.UnityEditor;
using Fjord.XRInteraction.XRInput;
using Fjord.XRInteraction.XRUser;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Fjord.Common
{
    [CustomPropertyDrawer(typeof(XRInputNameList))]
    public class XRInputNameListPropertyDrawer : PropertyDrawer
    {
        private ReorderableList _list;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InitializeList(property);

            property.serializedObject.Update();
            _list.DoList(position);
            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int arraySize = property.FindPropertyRelative("_inputNames").arraySize;
            if (arraySize == 0)
            {
                return EditorGUIUtility.singleLineHeight * 3;
            }
            else
            {
                return (arraySize * EditorGUIUtility.singleLineHeight) + EditorGUIUtility.singleLineHeight * 2;
            }
        }

        private void InitializeList(SerializedProperty property)
        {
            if (null == _list)
            {
                _list = new ReorderableList(
                    property.serializedObject,
                    property.FindPropertyRelative("_inputNames"),
                    true,
                    true,
                    true,
                    true);

                _list.drawElementCallback += DrawElementCallback;
                _list.drawHeaderCallback += DrawHeaderCallback;
                _list.onAddDropdownCallback += OnAddDropdownCallback;
            }
        }

        private void OnAddDropdownCallback(Rect buttonRect, ReorderableList list)
        {
            GenericMenu menu = new GenericMenu();

            string[] names = Enum.GetNames(typeof(XRInputName));
            XRInputName[] values = (XRInputName[])Enum.GetValues(typeof(XRInputName));
            
            int elementsAdded = 0;
            for (int i = 0; i < names.Length; ++i)
            {
                if (!ContainsEnum(values[i]))
                {
                    menu.AddItem(new GUIContent(names[i]), false, AddTag, values[i]);
                    elementsAdded++;
                }
            }

            if (elementsAdded == 0)
            {
                menu.AddDisabledItem(new GUIContent("All available enums added."));
            }
            menu.ShowAsContext();
        }

        private void AddTag(object userData)
        {
            int index = _list.serializedProperty.arraySize;
            _list.serializedProperty.arraySize++;
            _list.index = index;
            SerializedProperty element = _list.serializedProperty.GetArrayElementAtIndex(index);
            element.enumValueIndex = (int)userData;
            _list.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeaderCallback(Rect rect)
        {
            GUI.Label(rect, "XR Input Names");
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty property = _list.serializedProperty.GetArrayElementAtIndex(index);
            GUI.Label(rect, property.enumValue<XRInputName>().ToString());
        }

        private bool ContainsEnum(XRInputName inputName)
        {
            for (int i = 0; i < _list.serializedProperty.arraySize; ++i)
            {
                if (_list.serializedProperty.GetArrayElementAtIndex(i).enumValue<XRInputName>() == inputName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}