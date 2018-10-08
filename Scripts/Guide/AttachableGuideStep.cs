using System.Collections;
using System.Collections.Generic;
using Fjord.Common.UnityEvents;
using Fjord.XRInteraction.XRInteractions;
using Fjord.XRInteraction.XRUser;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;
using Valve.VR;

namespace Fjord.XRInteraction.Attachables
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Attachable))]
    public class AttachableGuideStep : GuideStep
    {
        [SerializeField]
        private GameObject _guideVisual;

        [SerializeField]
        private Material _highlightMaterial;

        private Attachable _attachable;
        private MeshRenderer[] _renderers;
        private MeshFilter[] _meshFilters;
        private bool _drawHighlight;
        private float _enteredRotation;
        private int _grabCount;

        private void Awake()
        {
            enabled = false;
        }

        public override void Intialize(GuideSequence guideSequence, int index)
        {
            base.Intialize(guideSequence, index);

            _attachable = GetComponent<Attachable>();
            _attachable.Events.Attached.AddListener(Attached);
            _attachable.Events.Entered.AddListener(Entered);
            _attachable.MovementInteraction.Events.ButtonDown.AddListener(ButtonDown);
            _attachable.MovementInteraction.Events.ButtonUp.AddListener(ButtonUp);

            _renderers = _attachable.ParentTransform.GetComponentsInChildren<MeshRenderer>();
            if (null != _guideVisual) _guideVisual.gameObject.SetActive(false);
            foreach (AttachableHolder attachableHolder in _attachable.CanAttachTo)
            {
                attachableHolder.enabled = false;
            }
            if (null != _highlightMaterial)
            {
                _highlightMaterial = new Material(_highlightMaterial);
                _meshFilters = _attachable.MovementInteraction.GetComponentsInChildren<MeshFilter>();
            }
        }

        private void LateUpdate()
        {
            if (null != _guideVisual) BounceAboveRenderers(_renderers, _guideVisual.transform);

            if (_drawHighlight && null != _highlightMaterial)
            {
                DrawPulsingHighlight(_highlightMaterial, _meshFilters);
            }
        }

        public void DrawAttachableOverlay(Material material)
        {
            DrawOverlay(material, _meshFilters);
        }

        public void SetGeometryVisibility(bool visibility)
        {
            for (int i = 0; i < _renderers.Length; ++i)
            {
                _renderers[i].enabled = visibility;
            }
        }

        private void ButtonDown(XRButtonDatum datum, XRInteractionEventReceiver receiver)
        {
            if (receiver.HeldButtons.Count == 1)
            {
                if (null != _guideVisual) _guideVisual.gameObject.SetActive(false);
                _drawHighlight = false;
            }
        }

        private void ButtonUp(XRButtonDatum datum, XRInteractionEventReceiver receiver)
        {
            if (receiver.HeldButtons.Count == 0)
            {
                if (null != _guideVisual && !Completed) _guideVisual.gameObject.SetActive(true);
                _drawHighlight = true;
                _grabCount++;
            }
        }

        public override void OnStepActivated()
        {
            base.OnStepActivated();
            foreach (AttachableHolder attachableHolder in _attachable.CanAttachTo)
            {
                attachableHolder.enabled = true;
            }
            enabled = true;
            if (_attachable.MovementInteraction.HeldButtons.Count > 0)
            {
            }
            else
            {
                _drawHighlight = true;
                if (null != _guideVisual)
                {
                    _guideVisual.gameObject.SetActive(true);
                    BounceAboveRenderers(_renderers, _guideVisual.transform);
                }
            }
            if (null != _activateSound) AudioSource.PlayClipAtPoint(_activateSound, transform.position);
            _grabCount = 0;
        }

        private void Entered(Attachable attachable, AttachableHolder holder)
        {
            _enteredRotation = Vector3.Angle(attachable.ParentTransform.up, holder.transform.up);
        }

        private void Attached(Attachable attachable, AttachableHolder holder)
        {
            enabled = false;
            if (null != _guideVisual) _guideVisual.gameObject.SetActive(false);
            _drawHighlight = false;
            OnStepCompleted();
            Analytics.Add("attachment_accuracy", _enteredRotation / 36f);
            Debug.Log("attachment_accuracy  " + _enteredRotation / 36f);
            Analytics.Add("drops", Mathf.Clamp(_grabCount, 0, 5));
            Debug.Log("drops  " + _grabCount);
        }

        public override void DrawDizmoFromSequence(GuideSequence sequence, int index)
        {
#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.magenta;
            style.fontSize = 18;
            UnityEditor.Handles.Label(transform.position, index.ToString(), style);
#endif
            Gizmos.color = Color.magenta / 2;
            Gizmos.DrawLine(sequence.transform.position, transform.position);
        }
    }
}