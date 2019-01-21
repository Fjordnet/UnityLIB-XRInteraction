using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fjord.XRInteraction
{
    [InitializeOnLoad]
    public class InitializeXRInteractionEditor : Editor
    {
        static List<string> tagsToAdd = new List<string>
        {
            "XRLaser",
            "XRProximity",
            "XRTeleport"
        };

        static InitializeXRInteractionEditor()
        {
            UpdateTagList();
        }

        static void UpdateTagList()
        {
            SerializedObject tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")
            );
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            
            foreach (string tagText in tagsToAdd)
            {
                FindOrAddTag(tagsProp, tagText);
            }
            tagManager.ApplyModifiedProperties();
        }

        static void FindOrAddTag(SerializedProperty tagsProp, string text)
        {
            bool foundTag = false;
            for (int i=0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty existingTag = tagsProp.GetArrayElementAtIndex(i);

                // Already exists, exit
                if (existingTag.stringValue.Equals(text))
                {
                    foundTag = true;
                    break;
                }
            }

            if (!foundTag)
            {
                tagsProp.arraySize++;
                SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize-1);
                newTag.stringValue = text;
                Debug.LogFormat("XRInteraction Tags: Added tag {0} to project at index {1}.", text, tagsProp.arraySize);
            }
        }
    }
}
