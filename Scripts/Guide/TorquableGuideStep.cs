using System.Collections;
using System.Collections.Generic;
using Fjord.Common.UnityEvents;
using Fjord.XRInteraction.XRInteractions;
using Fjord.XRInteraction.XRUser;
using UnityEngine;
using UnityEngine.Events;

namespace Fjord.XRInteraction.Attachables
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Torquable))]
    public class TorquableGuideStep : GuideStep
    {
        [SerializeField]
        private GameObject _toolGuideVisual;

        [SerializeField]
        private GameObject _torqueGuideVisual;
        
        [SerializeField]
        private Material _highlightMaterial;

        private Torquable _torquable;
        private MeshFilter[] _toolMeshFilters;
        private bool _drawToolHighlight;

        private void Awake()
        {
            enabled = false;
        }

        public override void Intialize(GuideSequence guideSequence, int index)
        {
            base.Intialize(guideSequence, index);
            
            _torquable = GetComponent<Torquable>();
            _torquable.enabled = false;
            _torquable.TorqueCompleted.AddListener(Torqued);
            if (_torquable.TorquableBy.Count > 0)
            {
                _torquable.TorquableBy[0].MovementInteraction.Events.ButtonDown.AddListener(ButtonDown);
                _torquable.TorquableBy[0].MovementInteraction.Events.ButtonUp.AddListener(ButtonUp);
                
                if (null != _highlightMaterial)
                {
                    _highlightMaterial = new Material(_highlightMaterial);
                    _toolMeshFilters = _torquable.TorquableBy[0].MovementInteraction.GetComponentsInChildren<MeshFilter>();
                }
            }

            if (null != _toolGuideVisual) _toolGuideVisual.gameObject.SetActive(false);
            if (null != _torqueGuideVisual) _torqueGuideVisual.gameObject.SetActive(false);
            
            Debug.Log(name + "   " + _torqueGuideVisual);
        }

        private void LateUpdate()
        {
            if (_drawToolHighlight && null != _highlightMaterial)
            {
                DrawPulsingHighlight(_highlightMaterial, _toolMeshFilters);
            }
        }

        private void ButtonDown(XRButtonDatum datum, XRInteractionEventReceiver receiver)
        {
            if (receiver.HeldButtons.Count == 1 && Activated)
            {
                if (null != _toolGuideVisual) _toolGuideVisual.gameObject.SetActive(false);
                if (null != _torqueGuideVisual) _torqueGuideVisual.gameObject.SetActive(true);
                _drawToolHighlight = false;
            }
        }

        private void ButtonUp(XRButtonDatum datum, XRInteractionEventReceiver receiver)
        {
            if (receiver.HeldButtons.Count == 0 && Activated)
            {
                if (null != _toolGuideVisual && !Completed) _toolGuideVisual.gameObject.SetActive(true);
                if (null != _torqueGuideVisual) _torqueGuideVisual.gameObject.SetActive(false);
                _drawToolHighlight = true;
            }
        }

        public override void OnStepActivated()
        {
            base.OnStepActivated();
            _torquable.enabled = true;
            enabled = true;
            if (_torquable.TorquableBy.Count > 0 && _torquable.TorquableBy[0].MovementInteraction.HeldButtons.Count > 0)
            {
                _drawToolHighlight = false;
                
                if (null != _toolGuideVisual)
                {
                    _toolGuideVisual.gameObject.SetActive(false);
                }

                if (null != _torqueGuideVisual)
                {
                    _torqueGuideVisual.gameObject.SetActive(true);
                }
            }
            else
            {
                if (null != _toolGuideVisual) _toolGuideVisual.gameObject.SetActive(true);
                _drawToolHighlight = true;
            }

            if (null != _activateSound)
                AudioSource.PlayClipAtPoint(
                    _activateSound,
                    _torquable.TorquableBy[0].transform.position);
        }

        private void Torqued()
        {
            if (null != _toolGuideVisual) _toolGuideVisual.gameObject.SetActive(false);
            if (null != _torqueGuideVisual) _torqueGuideVisual.gameObject.SetActive(false);
            _drawToolHighlight = false;
            enabled = false;
            OnStepCompleted();
        }

        public override void DrawDizmoFromSequence(GuideSequence sequence, int index)
        {
            const float offset = .05f;
#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.blue;
            style.fontSize = 18;
            UnityEditor.Handles.Label(transform.position + new Vector3(0, offset, 0), index.ToString(), style);
#endif
            Gizmos.color = Color.blue / 2;
            Gizmos.DrawLine(sequence.transform.position, transform.position + new Vector3(0, offset, 0));
        }
    }
}