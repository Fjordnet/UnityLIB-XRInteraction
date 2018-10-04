using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Fjord.XRInteraction.XRHaptics
{
    [CreateAssetMenu(fileName = "XRHaptictMap", menuName = "XR Haptic Map", order = 100)]
    public class XRHapticMap : ScriptableObject
    {    
        [Header("Description of each Haptic.")]
        [SerializeField]
        private List<XRHapticDescription> _descriptions = new List<XRHapticDescription>();

        public ReadOnlyCollection<XRHapticDescription> Descriptions
        {
            get { return _descriptions.AsReadOnly(); }
        }

        public XRHapticDescription GetHapticDescription(string hapticKey)
        {
            return _descriptions.Find(d => d.HapticKey == hapticKey);
        }
    }
}