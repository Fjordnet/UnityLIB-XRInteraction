using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Fjord.XRInteraction.XRInput
{
    /// <summary>
    /// Contains multiple XRInputMap's to describe how all inputs an XR Controller
    /// should route through an IXRInputProcessor.
    /// </summary>
    [CreateAssetMenu(fileName = "XRInputMap", menuName = "XR Input Map", order = 100)]
    public class XRInputMap : ScriptableObject
    {
        [Header("Description of each input.")]
        [SerializeField]
        private List<XRInputDescription> _descriptions = new List<XRInputDescription>();

        public ReadOnlyCollection<XRInputDescription> Descriptions
        {
            get { return _descriptions.AsReadOnly(); }
        }
    }
}