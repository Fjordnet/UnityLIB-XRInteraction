using Fjord.XRInteraction.XRUnityEvents;
using Fjord.XRInteraction.XRUser;

namespace Fjord.XRInteraction.XRInteractions
{
	public class XRButtonInteraction : XRInteractionEventReceiver
	{
		public XRButtonDatumInteractionUnityEvent _clicked;
        
		public override void OnButtonUp(XRButtonDatum proximityButtonDatum)
		{
			base.OnButtonUp(proximityButtonDatum);
			if (proximityButtonDatum.OverPressSurface())
			{
				_clicked.Invoke(proximityButtonDatum, this);
			}
		}
	}
}