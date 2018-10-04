# XRInteraction-UnityLIB

## Introduction

#### Overview

The XRInteraction library provides all necessary logic, and abstractions, in order to interface and create behavior with any HMD and controller setup such as Vive, Oculus or Windows MR Headsets.

This Library provides similar abstraction to what is found in VRTK, Windows MR, or SteamVR libraries. However it has the unique quality of being built off of the XR interoperability built into Unity, so it does not carry a dependency to the Oculus, SteamVR nor Window MR SDK's. This is the only library necessary to import to interface with all devices. This library is being developed specifically for enterprise VR and realistic simulation. So there is a focus on enabling highly complex, and realistic, interactions versus more abstracted and simplified interactions found in many VR games.

#### Getting Started

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

## System Overview

This system has been designed with a focus on enabling realism in interactions. Thus it is extremely configurable to allow any type of interaction. There is very little assumption in what your interactions will be. Rather than being thought of primarily as a library of interactions, it is more accurate to think of this library as a way to route data from XR controllers out into the 3D environment. You can configure it to route the data in any manner.

One specific example of what we mean by this, in other libraries you can find specific abstractions of 'Use' and 'Grab' hardcoded into components, this library carries no such assumptions. Rather, this system broadcasts controller input into the environment to be interpreted in any manner. This carries the unique benefit that a component which allows you to drag an object around at a distance via a laser is the same component which enables you to grab an object by nearest proximity.

We have provided pre-configured components and prefabs to enable you to get started quickly, such as the 'StandardUserRoot' prefab. But it should be understood these are pre-configured component compositions. Rearrangement, and reconfiguring of these components can produce much different behavior.

Following is an overview of the base abstractions used to route XR controller data and define any type of interactions.

![](XRInteraction.png)

##### XRUserRoot Component

This is the entry point into the entire system. This is the only component you need to place in your scene in your order to load an XR user into the environment. This is the component you would access via FindObjectOfType<XRUserRoot> in order to get to the [[XRUserHead]] or the [[XRUserController]].

The [[XRUserRoot]] contains an array of [[XRUserRootConfig]] components. Each [[XRUserRootConfig]] contains a configuration for a different headset, the [[XRUserRoot]] will detect which headset is being used and then search for a corresponding [[XRUserRootConfig]] to load all the necessary components for that headset.

For further explanation refer to the [[XRUserRoot section|XRUserRoot]].

##### XRUserRootConfig Datum

Contains references to all the necessary prefabs and datums required to construct an XRUser in the game world for a particular headset and platform. You will have one XRUserRootConfig for each XR platform.

For further explanation refer to the [[XRUserRootConfig section|XRUserRootConfig]].

##### XRInputMap Datum

An InputMap can be created for each type of controller, Touch, Vive, or Windows MR. An input map defines how the physical buttons of a controller should be routed to a unified set of input abstractions (i.e. Trigger Button, Grab Button, etc.). This allows your application to become independent of the specifics of the controller hardware it is resting on.

An InputMap can be created by going to "Assets > Create > XR Input Map". A selection of preconfigured InputMaps are located in "Datum/Mappings" for the Vive, Oculus and Windows MR controllers.

For further explanation refer to the [[XRInputMap section|XRInputMap]].

##### XRUserController Component

This represents a physical controller in the world. An [[XRUserController]] must have [[XRInteractor]]'s placed on GameObjects which are children of it in order to define how the controller will interact with the world.

For further explanation refer to the [[XRUserController section|XRUserController]].

##### XRInteractor Components

Within the system are the base abstractions of an Interactor and an Interaction.  A [[XRInteractor]] broadcasts an unified set messages and data to a [[XRInteractionReceiver]], which then does what it wishes with this data to produce any desired interaction.

[[XRInteractor]] components are added as children of an XRUserController. See the prefab 'StandardUserController' for a preconfigured example.

- Default _XRInteractor_ components.
  - _XRLaserInteractor_
  - _XRProximityInteractor_
  - _XRFlexLaserInteractor_
  - _XRUnityUILaserInteractor_

For further explanation refer to the [[XRInteractor section|XRInteractor]].

##### XRUserHead

The [[XRUserRoot]] must have a [[XRUserHead]] prefab specified. The [[XRUserHead]] represents the actual head of the user, and contains the XR Unity Camera, as well as any other functionality related to the head.

For further explanation refer to the [[XRUserHead section|XRUserHead]].

##### XRInteractionEventReceiver

This receives the events broadcast by the [[XRInteractor]] components. Implement this class and override the appropriate methods, or subscribe to the appropriate events, to receive the data required to implement any interaction.

For further explanation refer to the [[XRInteractionEventReceiver section|XRInteractionEventReceiver]].

#### Attachable & AttachableHolder
