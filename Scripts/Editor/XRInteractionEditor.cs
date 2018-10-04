using System;
using System.Collections;
using System.Collections.Generic;
using Fjord.Common.Data;
using Fjord.Common.Types;
using Fjord.XRInteraction.XRInput;
using Fjord.XRInteraction.XRInteractions;
using Fjord.XRInteraction.XRUser;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Fjord.XRInteraction
{
    public static class XRInteractionEditor
    {
        [MenuItem("GameObject/Fjord/Child and make movable.", false,0)]
        public static void SetupInputManager(MenuCommand command)
        {
            GameObject gameObject = Selection.activeGameObject;
            if (null == gameObject)
            {
                Debug.LogWarning("No GameObject Selected.");
                return;
            }

            GameObject parentGameObject = new GameObject(gameObject.name);
            parentGameObject.transform.position = gameObject.GetComponent<MeshRenderer>().bounds.center;
            parentGameObject.transform.SetParent(gameObject.transform.parent);
            gameObject.transform.SetParent(parentGameObject.transform);

            if (gameObject.GetComponent<Collider>() == null)
            {
                MeshCollider collider = gameObject.AddComponent<MeshCollider>();
                collider.convex = true;
                collider.sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
                collider.inflateMesh = true;
                collider.skinWidth = .0001f;
            }

            parentGameObject.AddComponent<Rigidbody>();
            parentGameObject.AddComponent<XRFreeMovementInteraction>();
            parentGameObject.AddComponent<XRInteractionHighlighter>();
        }
    }
}