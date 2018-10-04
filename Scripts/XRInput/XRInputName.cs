using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fjord.XRInteraction.XRInput
{
    [Flags]
    public enum XRInputName
    {
        TriggerButton = 1,
        GrabButton = 2,
        Joystick = 4,
        JoystickButton = 8,
        Button0 = 16,
        Button1 = 32,
        Button2 = 64,
        Button3 = 128,
        Button4 = 256,
        Button5 = 512,
        Button6 = 1024,
        Button7 = 2048,
        JoystickUp = 4096,
        JoystickRight = 8192,
        JoystickDown = 16384,
        JoystickLeft = 32768,
    }
}