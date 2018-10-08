using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fjord.Common.UnityEvents;
using UnityEngine;

namespace Fjord.XRInteraction.Attachables
{
    public class GuideSequence : MonoBehaviour
    {
        [Header("AttachableHolders higher in the list must be attached before later.")]
        [SerializeField]
        private List<GuideStep> _sequence = new List<GuideStep>();

        [Header("Fired when sequence progressed, passing next attachable.")]
        [SerializeField]
        private GameObjectUnityEvent _sequenceProgressed = new GameObjectUnityEvent();
        
        [Header("Fired when sequence is completed.")]
        [SerializeField]
        private GameObjectUnityEvent _sequenceCompleted = new GameObjectUnityEvent();

        public GameObjectUnityEvent SequenceProgressed
        {
            get { return _sequenceProgressed; }
        }

        public GameObjectUnityEvent SequenceCompleted
        {
            get { return _sequenceCompleted; }
        }

        public List<GuideStep> Sequence
        {
            get { return _sequence; }
        }

        private IEnumerator Start()
        {
            yield return null; //done one cycle after start to allow holders to run their start

            _sequence.RemoveAll(s => null == s);

            for (int i = 0; i < _sequence.Count; ++i)
            {
                _sequence[i].Intialize(this, i);
                if (i < _sequence.Count - 1)
                {
                    GuideStep nextStep = _sequence[i + 1];

                    _sequence[i].StepCompleted.AddListener((go) =>
                    {
                        nextStep.OnStepActivated();
                        _sequenceProgressed.Invoke(nextStep.gameObject);
                    });
                }
                else
                {
                    _sequence[i].StepCompleted.AddListener((go) =>
                    {
                        _sequenceCompleted.Invoke(go);
                    });
                }
            }

            if (_sequence.Count > 0)
            {
                _sequence[0].OnStepActivated();
                _sequenceProgressed.Invoke(_sequence[0].gameObject);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(transform.position, .1f);
        }

        private void OnDrawGizmosSelected()
        {
            for (int i = 0; i < _sequence.Count; ++i)
            {
                if (null != _sequence[i])
                {
                    _sequence[i].DrawDizmoFromSequence(this, i);
                }
            }
        }

        [ContextMenu("Gather all guide steps.")]
        private void GatherAllGuideSteps()
        {
            _sequence = FindObjectsOfType<GuideStep>().ToList();
        }
    }
}