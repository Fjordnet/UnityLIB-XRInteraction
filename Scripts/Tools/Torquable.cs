using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;
using UnityEngine.Events;

namespace Fjord.XRInteraction.Attachables
{
    public class Torquable : MonoBehaviour
    {
        [Header("Tags which represent this.")]
        [SerializeField]
        private AttachableTagMask _tags;

        [Header("Can be torqued by.")]
        [SerializeField]
        private List<TorqueTool> _torquableBy;

        [Header("How much rotation must occur to complete.")]
        [SerializeField]
        private float _currentTorque = 180;

        [SerializeField]
        private UnityEvent _torqueCompleted;

        [SerializeField]
        private bool _rotateParent = true;

        public bool Torqued { get; private set; }

        /// <summary>
        /// If is a child GameObject in a more complex assembly, will be the parent
        /// transform which object should be moved through. Otherwise will be this transform.
        /// </summary>
        public Transform ParentTransform { get; private set; }

        public Rigidbody AttachedRigidbody { get; private set; }

        public UnityEvent TorqueCompleted
        {
            get { return _torqueCompleted; }
        }

        public List<TorqueTool> TorquableBy
        {
            get { return _torquableBy; }
        }

        private void Start()
        {
            AttachedRigidbody = GetComponentInParent<Rigidbody>();
            ParentTransform = null == AttachedRigidbody ? transform : AttachedRigidbody.transform;
        }

        /// <summary>
        /// Apply torque.
        /// </summary>
        /// <returns> True is torque limit reached. </returns>
        public bool ApplyTorque(float amount)
        {
            _currentTorque += amount;

            if (_currentTorque < 0 && !Torqued)
            {
                Torqued = true;
                _torqueCompleted.Invoke();
            }
            else if (!Torqued)
            {
                if (_rotateParent)
                    ParentTransform.Rotate(0, -amount, 0, Space.Self);
                else
                    transform.Rotate(0, -amount, 0, Space.Self);
            }

            if (Torqued) return true;
            return false;
        }

        public bool CanBeTorquedBy(TorqueTool torqueTool)
        {
            if (!enabled) return false;
            if (_torquableBy.Contains(torqueTool))
            {
                return true;
            }
            if (_tags.CompareTags(torqueTool.Tags))
            {
                return true;
            }
            return false;
        }
    }
}