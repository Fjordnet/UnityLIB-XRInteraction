using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Fjord.XRInteraction.XRInput;
using Fjord.XRInteraction.XRInteractors;
using UnityEngine;

namespace Fjord.XRInteraction.XRUser
{
	/// <summary>
	/// Stores data unique to each button press that runs through an Interactor.
	/// </summary>
	[Serializable]
	public class XRButtonDatum
	{
		public readonly XRUserController ParentUserController;
		public readonly XRPhysicsInteractor ParentInteractor;
		public readonly XRInputName InputName;
		private Ray _pressHitRay;
		private Transform _pressLocalHitTransform;
		private Transform _pressLocalTransform;
		
		/// <summary>
		/// Collider which button was pressed on.
		/// </summary>
		public Collider PressCollider { get; private set; }
		
		/// <summary>
		/// The position of the PressGameObject on ButtonDown, translated as if it were a child of the controller.
		/// Setting the PressGameObject equal to this position will make it move with the interactor as if it were
		/// a child.
		/// </summary>
		public Vector3 PressColliderPositionChildedToController
		{
			get { return _pressLocalTransform.position; }
		}

		private Vector3 PriorPressColliderPositionChildedToController { get; set; }
		public Vector3 PressColliderPositionChildedToControllerDelta { get; private set; }
        
		/// <summary>
		/// The rotation of the PressGameObject on ButtonDown, translated as if it were a child of the controller.
		/// Setting the PressGameObject equal to this rotation will make it move with the interactor as if
		/// it were a child.
		/// </summary>
		public Quaternion PressColliderRotationChildedToController
		{
			get { return _pressLocalTransform.rotation; }
		}
		
		/// <summary>
		/// The ray where the PressGameObject was hit, transformed as if it were a child of PressGameObject.
		/// This ray is always on the surface of the PressGameObject.
		/// </summary>
		public Ray RayHitChildedToPressGameObject
		{
			get
			{
				Vector3 point = PressCollider.transform.TransformPoint(_pressHitRay.origin);
				Vector3 direction = PressCollider.transform.TransformDirection(_pressHitRay.direction);
				return new Ray(point, direction);
			}
			private set
			{
				Vector3 point = PressCollider.transform.InverseTransformPoint(value.origin);
				Vector3 direction = PressCollider.transform.InverseTransformDirection(value.direction);
				_pressHitRay = new Ray(point, direction);
			}
		}

		/// <summary>
		/// The ray where the PressGameObject was hit, transformed as if it were a child of the controller,
		/// </summary>
		public Ray RayHitChildedToController
		{
			get
			{
				return new Ray(_pressLocalHitTransform.position, _pressLocalHitTransform.forward);
			}
		}

		public XRButtonDatum(XRUserController parentUserController, XRPhysicsInteractor parentInteractor, XRInputName inputName)
		{
			ParentUserController = parentUserController;
			ParentInteractor = parentInteractor;
			InputName = inputName;
			
			_pressLocalTransform = new GameObject("PressLocalTransform").transform;
			_pressLocalTransform.gameObject.hideFlags = HideFlags.HideAndDontSave;
			_pressLocalTransform.SetParent(parentInteractor.transform);
			
			_pressLocalHitTransform = new GameObject("PressLocalHitTransform").transform;
			_pressLocalHitTransform.gameObject.hideFlags = HideFlags.HideAndDontSave;
			_pressLocalHitTransform.SetParent(parentInteractor.transform);
		}

		/// <summary>
		/// Is Interactor over same surface that the press occured on?
		/// </summary>
		/// <returns></returns>
		public bool OverPressSurface()
		{
			return ParentInteractor.CurrentCollider == PressCollider;
		}
		
		internal virtual void ButtonDown(Collider pressCollider, Ray pressHitRay)
		{
			PressCollider = pressCollider;
			_pressLocalTransform.position = PressCollider.transform.position;
			_pressLocalTransform.rotation = PressCollider.transform.rotation;
			RayHitChildedToPressGameObject = pressHitRay;
			_pressLocalHitTransform.position = pressHitRay.origin;
			_pressLocalHitTransform.forward = pressHitRay.direction;

			PriorPressColliderPositionChildedToController = PressColliderPositionChildedToController;
			PressColliderPositionChildedToControllerDelta = Vector3.zero;
		}
		
		internal virtual void ButtonHold()
		{
			PressColliderPositionChildedToControllerDelta = PressColliderPositionChildedToController - PriorPressColliderPositionChildedToController;
			PriorPressColliderPositionChildedToController = PressColliderPositionChildedToController;
		}
		
		internal virtual void ButtonUp()
		{
			PressCollider = null;
			_pressHitRay = new Ray(ParentInteractor.transform.position, Vector3.zero);
			_pressLocalHitTransform.position = ParentInteractor.transform.position;
			_pressLocalHitTransform.rotation = ParentInteractor.transform.rotation;
		}
	}
}