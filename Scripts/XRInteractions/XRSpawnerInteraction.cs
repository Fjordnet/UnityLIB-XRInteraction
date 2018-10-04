using Fjord.XRInteraction.XRInput;
using Fjord.XRInteraction.XRUser;
using UnityEngine;

namespace Fjord.XRInteraction.XRInteractions
{
    public class XRSpawnerInteraction : XRInteractionEventReceiver
    {
        [SerializeField]
        private GameObject _instantiationPrefab;
        
        [SerializeField]
        private Transform _spawnLocation;
        
        [SerializeField]
        private XRInputName _fireButton;

        [SerializeField]
        private XRInputName _holdingButton;

        private bool _holding;
        
        public override void OnButtonDown(XRButtonDatum proximityButtonDatum)
        {
            if (proximityButtonDatum.InputName == _holdingButton)
            {
                _holding = true;
            }
        }

        public override void OnButtonUp(XRButtonDatum proximityButtonDatum)
        {
            if (proximityButtonDatum.InputName == _holdingButton)
            {
                _holding = false;
            }

            if (_holding && proximityButtonDatum.InputName == _fireButton)
            {
                Instantiate(_instantiationPrefab, _spawnLocation.position, _spawnLocation.rotation);
            }
        }
    }
}