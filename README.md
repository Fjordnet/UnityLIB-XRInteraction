# XRInteraction-UnityLIB

## Introduction

#### Overview

The XRInteraction library provides all necessary logic, and abstractions, in order to interface and create behavior with any HMD and controller setup such as Vive, Oculus or Windows MR Headsets.

This Library is built off the XR interoperability built into Unity, so it does not carry a dependency to the Oculus, SteamVR nor Window MR SDK's. Currently this is the only library necessary to import to interface with supported devices, the intention is to keep it that way moving forawrd.. This library is being developed specifically for enterprise VR and realistic simulation. So there is a focus on enabling highly complex, and realistic, interactions versus more abstracted and simplified interactions.

[Please visit the Wiki for further documentation.](https://github.com/Fjordnet/UnityLIB-XRInteraction/wiki)

**Repository is currently in pre-alpha. Still under heavy development, some components still needing to be added, others refactored. heavy changes to API still expected. First stable version should within a few months.**

#### Quick Overview

###### Installation

1. Add submodule entries for Common and XRInteraction libraries.

```
git submodule add https://github.com/Fjordnet/UnityLIB-XRInteraction.git Assets/Fjord/UnityLIB-XRInteraction
git submodule add https://github.com/Fjordnet/UnityLIB-Common.git Assets/Fjord/UnityLIB-Common
```

###### Setting up a User.

1. Drag the 'StandardUserRoot' prefab into your scene.
2. Press play. You should have a VR setup that will work on Vive, Oculus, or Windows MR headset. This setup comes pre-configured with the ability to interact via lasers, or grabbing. It also enables teleporting by pressing the joystick button.

###### Making an object interactable.

1. Click 'GameObject > 3D Object > Cube'.
2. In the Inspector click 'Add Component' and add an 'XRFreeMovementInteraction' component and also a 'Rigidbody' component.
3. Press play. You should be able to grab the cube with the lasers by holding the trigger button, and also grab by bringing hand near the cube and holding the grip button.


