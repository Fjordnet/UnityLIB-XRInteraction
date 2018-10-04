using System;

namespace Fjord.XRInteraction.XRInput
{
    public enum XRInputType
    {
        Button,
        ThresholdButton,
        Axis1D,
        Axis2D,
    }

    public static class XRInputTypeExtensions
    {
        public static Type GetProcessorType(this XRInputType inputType)
        {
            switch (inputType)
            {
                case XRInputType.Button:
                    return typeof(ButtonProcessor);
                case XRInputType.ThresholdButton:
                    return typeof(ThresholdButtonProcessor);
                case XRInputType.Axis1D:;
                    return typeof(Axis1DProcessor);
                case XRInputType.Axis2D:
                    return typeof(Axis2DProcessor);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}