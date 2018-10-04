using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;

namespace Fjord.XRInteraction
{
    public static class XRInputManagerEditor
    {
        [MenuItem("Fjord/XRInteraction/Setup Input Manager")]
        public static void SetupInputManager()
        {
            Debug.Log("Setting up input manager.");
            for (int i = 1; i < 29; ++i)
            {
                InputAxis inputAxis = new InputAxis();
                inputAxis.name = "Axis" + i;
                inputAxis.gravity = 1;
                inputAxis.dead = .001f;
                inputAxis.sensitivity = 1;
                inputAxis.type = AxisType.JoystickAxis;
                inputAxis.axis = i;
                AddAxis(inputAxis);
            }
        }

        private static SerializedObject GetAxisSerializedObject()
        {
            return new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset"
                )[0]);
        }
        
        private static void ClearAxis()
        {
            SerializedObject serializedObject = GetAxisSerializedObject();
            SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");
            axesProperty.ClearArray();
            serializedObject.ApplyModifiedProperties();
        }

        private static void AddAxis(InputAxis axis)
        {
            if (AxisDefined(axis.name))
            {
                return;
            }

            SerializedObject serializedObject = GetAxisSerializedObject();
            SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

            axesProperty.arraySize++;
            serializedObject.ApplyModifiedProperties();

            SerializedProperty axisProperty = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);

            GetChildProperty(axisProperty, "m_Name").stringValue = axis.name;
            GetChildProperty(axisProperty, "descriptiveName").stringValue = axis.descriptiveName;
            GetChildProperty(axisProperty, "descriptiveNegativeName").stringValue = axis.descriptiveNegativeName;
            GetChildProperty(axisProperty, "negativeButton").stringValue = axis.negativeButton;
            GetChildProperty(axisProperty, "positiveButton").stringValue = axis.positiveButton;
            GetChildProperty(axisProperty, "altNegativeButton").stringValue = axis.altNegativeButton;
            GetChildProperty(axisProperty, "altPositiveButton").stringValue = axis.altPositiveButton;
            GetChildProperty(axisProperty, "gravity").floatValue = axis.gravity;
            GetChildProperty(axisProperty, "dead").floatValue = axis.dead;
            GetChildProperty(axisProperty, "sensitivity").floatValue = axis.sensitivity;
            GetChildProperty(axisProperty, "snap").boolValue = axis.snap;
            GetChildProperty(axisProperty, "invert").boolValue = axis.invert;
            GetChildProperty(axisProperty, "type").intValue = (int) axis.type;
            GetChildProperty(axisProperty, "axis").intValue = axis.axis - 1;
            GetChildProperty(axisProperty, "joyNum").intValue = axis.joyNum;

            serializedObject.ApplyModifiedProperties();
        }

        private enum AxisType
        {
            KeyOrMouseButton = 0,
            MouseMovement = 1,
            JoystickAxis = 2
        };

        private struct InputAxis
        {
            public string name;
            public string descriptiveName;
            public string descriptiveNegativeName;
            public string negativeButton;
            public string positiveButton;
            public string altNegativeButton;
            public string altPositiveButton;

            public float gravity;
            public float dead;
            public float sensitivity;

            public bool snap;
            public bool invert;

            public AxisType type;

            public int axis;
            public int joyNum;
        }

        private static SerializedProperty GetChildProperty(SerializedProperty parent, string name)
        {
            SerializedProperty child = parent.Copy();
            child.Next(true);
            do
            {
                if (child.name == name) return child;
            } while (child.Next(false));

            return null;
        }

        private static bool AxisDefined(string axisName)
        {
            SerializedObject serializedObject =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

            axesProperty.Next(true);
            axesProperty.Next(true);
            while (axesProperty.Next(false))
            {
                SerializedProperty axis = axesProperty.Copy();
                axis.Next(true);
                if (axis.stringValue == axisName) return true;
            }

            return false;
        }
    }
}