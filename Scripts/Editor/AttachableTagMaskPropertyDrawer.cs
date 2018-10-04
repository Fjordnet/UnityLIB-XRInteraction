using System;
using Fjord.Common.Data;
using Fjord.Common.Types;
using Fjord.XRInteraction.Attachables;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Fjord.Common
{
    [CustomPropertyDrawer(typeof(AttachableTagMask))]
    public class AttachableTagMaskPropertyDrawer : CustomTagMaskPropertyDrawer
    {
        protected override string MultiTagDatumPath()
        {
            string[] assetGUIDs = AssetDatabase.FindAssets("AttachableTagMaskDatum");
            if (assetGUIDs.Length > 1)
            {
                Debug.LogWarning("More than one AttachableTagMasDatum found, using first.");
            }
            if (assetGUIDs.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(assetGUIDs[0]);
            }

            return null;
        }

        protected override void DrawHeaderCallback(Rect rect)
        {
            GUI.Label(rect, "Attachable Tags");
        }
    }
}