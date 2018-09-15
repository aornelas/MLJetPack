using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Base class for an interactable object. This allows the object to be used for simple targeting
// UI from any other object, covering hover, drag, etc.
//
//

namespace MLJetPack.Interactions {

	public enum HoverState
	{
		None,
		Hovered,
		Pressed,
		DraggedAway,
		Attached
	}

	public enum HoverAction
	{
		Enter,
		Stay,
		Cancel,
		Exit
	}
		
	public class InteractionEventData {
		public ProximityController controller;
		public Interactable hoveredInteractable;
		public Interactable downedInteractable;
		public int operationId;
		public Dictionary<string,object> parameters = null;
	};
		
	public class Interactable : MonoBehaviour {
		public delegate void InteractionEvent(InteractionEventData eventData);
		public delegate void HoverEvent(ProximityController controller);

		public HoverEvent OnHoverEnter = null;
		public HoverEvent OnHoverStay = null;
		public HoverEvent OnHoverPress = null;
		public HoverEvent OnHoverDragAway = null;
		public HoverEvent OnHoverExit = null;
		public HoverEvent OnHoverCancel = null;

		public InteractionEvent OnInteractionClick;
		public InteractionEvent OnInteractionDown;
		public InteractionEvent OnInteractionUp;
		public InteractionEvent OnInteractionContinue;

		bool interactionEnabled = true;

		[SerializeField]
		protected List<ProximityController> hoveredList = new List<ProximityController>();

		public List<int> selectedOperationsList = new List<int> ();
		public List<int> hoveredOperationsList = new List<int> ();

		protected virtual void Awake() {
			if (!selectedOperationsList.Contains (0)) {
				selectedOperationsList.Add (0);
			}
			if (!hoveredOperationsList.Contains (0)) {
				hoveredOperationsList.Add (0);
			}
		}

		void OnDisable() {
			if (hoveredList.Count > 0) {
				foreach (ProximityController m in hoveredList) {
					m.RemoveHoveredItem (this);
				}
				hoveredList.Clear ();
			}
		}

		void OnDestroy() {
			if (hoveredList.Count > 0) {
				foreach (ProximityController m in hoveredList) {
					m.RemoveHoveredItem (this);
				}
			}
			SetInteractionEnabled (false);
		}

		public virtual void SetPhysicsInteractive(bool shouldBeInteractive) {
			foreach (Collider collider in GetComponentsInChildren<Collider>()) {
				collider.enabled = shouldBeInteractive;
			}
		}

		public delegate bool PollCanHoverQuery (ProximityController controller);
		public PollCanHoverQuery PollCanHover;

		public virtual bool CanBeHovered(ProximityController controller) {
			if (!interactionEnabled) {
				return false;
			}

			if (!controller.CanHover (gameObject)) {
				#if UNITY_EDITOR
				if (debugHover) {
					Debug.Log (name + ": Controller Cannot hover: Cannot be hovered");
				}
				#endif
				return false;
			}

			if (!controller.DriverOperationIdle (0)) {
				#if UNITY_EDITOR
				if (debugHover) {
					Debug.Log (name + ": Driver Operation not idle: Cannot be hovered");
				}
				#endif
				return false;
			}

			if (PollCanHover != null) {
				foreach (PollCanHoverQuery canHover in PollCanHover.GetInvocationList()) {
					if (!canHover(controller)) {
						return false;
					}
				}
			}
			return true;
		}

		public virtual void SetInteractionEnabled(bool shouldBeEnabled) {
			interactionEnabled = shouldBeEnabled;
			if (!shouldBeEnabled) {
				foreach (ProximityController controller in hoveredList) {
					controller.RemoveHoveredItem (this);
				}
				List<ProximityController> clearedList = hoveredList;
				hoveredList = new List<ProximityController> ();
				CheckHoverState(clearedList);
			}
		}

		public void ClearHoveredList()
		{
			while (hoveredList.Count > 0)
			{
				hoveredList.Remove(hoveredList[0]);
			}
		}

		public bool doBoundsCheck = false;
		// Update is called once per frame
		protected virtual void Update() {
			if (doBoundsCheck) {
				BoundsCheck ();
			}
		}

		public float floorDeathValue = float.MinValue;
		protected virtual void BoundsCheck()
		{
			if (transform.position.y < floorDeathValue)
			{
				// Destroy self if out of bounds, for easy cleanup
				Destroy(gameObject);
			}
		}

		#if UNITY_EDITOR
		public bool debugHover = false;
		#endif

		public bool AddHover(ProximityController controller)
		{
			if (!this.enabled) {
				return false;
			}

			List<ProximityController> controllerList = new List<ProximityController>() {controller};

			#if UNITY_EDITOR
			if (debugHover)
			{
				if (controller.gameObject == gameObject)
				{
					Debug.LogError("Double hover on gameObject " + controller.name);
				}
			}
			#endif
//
//			if (!CanBeHovered (controller)) {
//				#if UNITY_EDITOR
//				if (debugHover)
//				{
//					Debug.Log("Can't be hovered: " + this.name);
//				}
//				#endif
//
//				return false;
//			}
//
			if (hoveredList.Contains(controller))
			{
				#if UNITY_EDITOR
				if (debugHover)
				{
					Debug.Log("Hover Stay: " + this.name);
				}
				#endif
//				CheckHoverState(controllerList); // this passes down the hover stay command
				return false;
			}

			#if UNITY_EDITOR
			if (debugHover)
			{
				Debug.Log("Adding Hovered controller: " + controller.name);
			}
			#endif
			hoveredList.Add(controller);

			Interactable other = null;
			if (controller.GetHoverCount() > 0)
			{
				// There's another hovered object, grab handle for it to exit the hover state
				other = controller.GetTopHoveredItem();
			}

			#if UNITY_EDITOR
			if (debugHover)
			{
				Debug.Log("Adding Hovered item: " + this.name);
			}
			#endif

			controller.AddHoveredItem(this);

			#if UNITY_EDITOR
			if (debugHover)
			{
				Debug.Log("Checking hover state: " + this.name);
			}
			#endif

			CheckHoverState(controllerList);

			if (other != null)
			{
				if (other.GetComponent<Interactable> () != null) {
					other.GetComponent<Interactable> ().CheckHoverState (controllerList);
				}
			}
			return true;
		}

		public bool RemovePressedState(ProximityController controller) {
			if (hoverState != HoverState.Pressed && hoverState != HoverState.DraggedAway) {
				Debug.LogError (name + ": Cannot remove Pressed state from " + hoverState);
				return false;
			}

			List<ProximityController> controllerList = new List<ProximityController>() {controller};
			HoverState newHoverState = HoverState.None;
			ApplyHoverState (newHoverState, true, controllerList);
			hoverState = newHoverState;

			controller.SurfaceTopHoveredObject ();
			return true;
		}

		public bool SetHoverState(ProximityController controller, HoverState newHoverState)
		{
			if (newHoverState == HoverState.Hovered || newHoverState == HoverState.None) {
				Debug.LogError ("Use AddHover or RemoveHover for setting hover state");
				// DSL: Might just want to re-route
				return false;
			}

			// HoverState.Pressed or HoverState.DraggedAway
			if (newHoverState == HoverState.Pressed) {
				if (hoverState != HoverState.Hovered && hoverState != HoverState.DraggedAway) {
					Debug.LogError ("Cannot move to Pressed state from " + hoverState);
					// DSL: Might just want to re-route
					return false;
				} 
				if (!controller.SortedHoveredObjectsContains(this)) {
					controller.AddHoveredItem(this);
				}
				if (!hoveredList.Contains (controller)) {
					hoveredList.Add (controller);
				}

			} else if (newHoverState == HoverState.DraggedAway) {
				if (hoverState != HoverState.Pressed) {
					Debug.LogError ("Cannot move to DraggedAway state from " + hoverState);
					// DSL: Might just want to re-route
					return false;
				}
				if (controller.SortedHoveredObjectsContains(this)) {
					controller.RemoveHoveredItem(this);
				}
				hoveredList.Remove(controller);
			}

			List<ProximityController> controllerList = new List<ProximityController>() {controller};
			ApplyHoverState (newHoverState, true, controllerList);
			hoverState = newHoverState;

			return true;
		}

		public void RemoveHover(ProximityController controller)
		{
			#if UNITY_EDITOR
			if (debugHover)
			{
				Debug.Log(name + ": Remove hover: " + controller.name);
			}
			#endif

			List<ProximityController> controllerList = new List<ProximityController>() {controller};

			// If there is a controller and it holds this object:
			int numRemaining = controller.RemoveHoveredItem(this);

			hoveredList.Remove(controller);

			CheckHoverState(controllerList);


			if (numRemaining > 0)
			{
				// turn on other hovered object
				Interactable other = controller.GetTopHoveredItem();
				if (other != null)
				{
					if (other.GetComponent<Interactable> () != null) {
						other.GetComponent<Interactable> ().CheckHoverState (controllerList);
					}
				}
			}
		}


		public HoverState hoverState = HoverState.None;
		public virtual void CheckHoverState(List<ProximityController> changedControllers)
		{
			HoverState newHoverState = HoverState.None;

			if (hoveredList.Count > 0) {
				foreach (ProximityController controller in hoveredList) {
					#if UNITY_EDITOR
					if (debugHover) {
						Debug.Log (Time.frameCount + ": " + name + " controller: " + controller.name + ": controller interaction enabled: " +
						controller.InteractionEnabled () +
						" / get top hovered item: " +
						controller.GetTopHoveredItem () +
						" / Can Be Hovered: " +
						CanBeHovered (controller));
					}
					#endif

					if (controller.downedInteractable == this) {
						if (hoverState == HoverState.Hovered) {
							newHoverState = HoverState.Pressed;
						} else {
							newHoverState = hoverState;
						}
						#if UNITY_EDITOR
						if (debugHover) {
							Debug.Log (Time.frameCount + ": " + name + ": DOWNED HOVER STATE: " + newHoverState);
						}
						#endif
						break;
					}

					if (CanBeHovered (controller) && controller.GetTopHoveredItem () == this) {
						newHoverState = HoverState.Hovered;
						#if UNITY_EDITOR
						if (debugHover && newHoverState != hoverState) {
							Debug.Log (Time.frameCount + ": " + name + ": SET NEW HOVER STATE: HOVERED for " + controller.name);
						}
						#endif
					}
				}
			}

			#if UNITY_EDITOR
			if (debugHover) {
				if (hoverState != newHoverState) {
					Debug.Log (name + ": APPLY CHANGED HOVER STATE: " + newHoverState);
				}
			}
			#endif

			ApplyHoverState(newHoverState, hoverState != newHoverState, changedControllers);
			hoverState = newHoverState;
		}

		protected virtual void ApplyHoverState(HoverState newHoverState, bool isStateChange, List<ProximityController> changedControllers)
		{
			#if UNITY_EDITOR
			if (debugHover) {
				Debug.Log (name + ": New hover state: " + newHoverState + ", old is " + hoverState);
			}
			#endif

			switch (newHoverState)
			{
			case HoverState.None:
				#if UNITY_EDITOR
				if (debugHover) {
					Debug.Log (name + ": New hover state NONE: " + newHoverState + ", old is " + hoverState);
				}
				#endif
				if (isStateChange) {
					if (changedControllers.Count > 0) {
						// DSL: This is if a controller has multiple hovers and this is not the top hover
						foreach (ProximityController controller in changedControllers) {
							#if UNITY_EDITOR
							if (debugHover) {
								Debug.Log (Time.frameCount + ": " + name + ": Calling OnHoverExit for: " + controller.name);
							}
							#endif
							if (this.OnHoverExit != null) {
								this.OnHoverExit (controller);
							}
						}

					} else {
						if (this.OnHoverExit != null) {
							this.OnHoverExit (null);
						}
					}
				}
				break;

			case HoverState.Hovered:
				foreach (ProximityController controller in changedControllers) {
					if (isStateChange) {
						if (this.OnHoverEnter != null)
						{
							this.OnHoverEnter(controller);
						}

					} else {
						if (this.OnHoverStay != null) {
							this.OnHoverStay (controller);
						}
					}
				}
				break;

			case HoverState.Pressed:
				foreach (ProximityController controller in changedControllers) {
					if (isStateChange) {
						if (this.OnHoverPress != null) {
							this.OnHoverPress (controller);
						}

					} else {
						//						if (this.OnHoverStay != null) {
						//							this.OnHoverStay (controller);
						//						}
					}
				}
				break;

			case HoverState.DraggedAway:
				foreach (ProximityController controller in changedControllers) {
					if (isStateChange) {
						if (this.OnHoverDragAway != null)
						{
							this.OnHoverDragAway(controller);
						}

					} else {
						//						if (this.OnHoverStay != null) {
						//							this.OnHoverStay (controller);
						//						}
					}
				}
				break;
			}
		}

		protected virtual void OnTriggerEnter(Collider c)
		{
			#if UNITY_EDITOR
			if (debugHover) {
				Debug.Log ("TRIGGER ENTER: " + this.name + " / " + c.gameObject.name);
			}
			#endif

			var handle = c.gameObject.GetComponent<ControllerLimb> ();
			if (handle == null || handle.controller == null) {
				return;
			}

			ProximityController controller = handle.controller;
			TriggerEnter (controller);
		}

		public void TriggerEnter(ProximityController controller) {
			if (controller.downedInteractable == this) {
				if (hoverState == HoverState.DraggedAway) {
					SetHoverState (controller, HoverState.Pressed);
				}
			}

			if (!controller.SortedHoveredObjectsContains(this)) {
				AddHover (controller);
			}
		}

		#if false
		int lastEvalFrame = -1;
		protected virtual void OnTriggerStay(Collider c)
		{
			if (lastEvalFrame == Time.frameCount) {
				return;
			}
			lastEvalFrame = Time.frameCount;
			//		Debug.Log (Time.frameCount + ": On trigger stay: " + name + " / " + c.gameObject.name);

			var handle = c.gameObject.GetComponent<ControllerLimb>();
			if (handle != null && handle.controller != null)
			{
				if (CanBeHovered(handle.controller))
				{
					if (!handle.controller.SortedHoveredInteractablesContains (this)) {
						AddHover (handle.controller);

					} else {
						CheckHoverState ();
					}

					//                LukUtils.getInstance().UniversalHoverAction(this, handle.controller, HoverAction.Stay);
				}
				else
				{
					if (handle.controller.SortedHoveredInteractablesContains(this))
					{
						//Debug.Log("Remove hover");
						RemoveHover(handle.controller);
					} else
					{
						//Debug.Log("Hovered stack doesn't have this");
					}
					// Remove Hover
				}
			}
		}
		#endif

		void OnTriggerExit(Collider c) {
			#if UNITY_EDITOR
			if (debugHover)
			{
				Debug.Log("TRIGGER EXIT: " + name + ": on trigger exit: " + c.gameObject.name + " -- handle: " + c.gameObject.GetComponent<ControllerLimb>());
			}
			#endif
			var handle = c.gameObject.GetComponent<ControllerLimb>();
			if (handle == null || handle.controller == null) {
				return;
			}

			ProximityController controller = handle.controller;

			#if UNITY_EDITOR
			if (debugHover)
			{
				Debug.Log("Call trigger exit");
			}
			#endif
			TriggerExit(controller);
		}

//
//		if (driver.DriverOperationIdle (0) || driver.DriverOperationStarted(0)) {
//			// If in the unpressed state, add hover if available, remove old hover if needed
//			if (hoveredInteractableUi != null) {
//				if (hoveredInteractableUi.hoverState == HoverState.None) {
//					hoveredInteractableUi.AddHover (this);
//				}
//			} else if (driver.DriverOperationStarted(0)) {
//				// No hover, and no selected object, but button tapped
//				uiLaser.ClearUiSelection();
//			}
//
//			if (lastClosestHitObject != hoveredSelectable) {
//				if (previousClosestInteractableUi != null) {
//					previousClosestInteractableUi.RemoveHover (this);
//				}
//			}
//
//		} else {
//			// Controller is in pressed or exiting state. Only process if pressed with downed object
//			if (downedInteractable != null) {
//				if (downedInteractable == hoveredInteractableUi) {
//					// We are on the downed ui object. We need to check the hover state
//					if (downedInteractable.hoverState == HoverState.DraggedAway) {
//						downedInteractable.SetHoverState (this, HoverState.Pressed);
//					}
//				} else {
//					if (downedInteractable.hoverState == HoverState.Pressed) {
//						downedInteractable.SetHoverState (this, HoverState.DraggedAway);
//					}
//				}
//			}
//		}




		public void TriggerExit(ProximityController controller)
		{
			if (controller.downedInteractable == this) {
				if (hoverState == HoverState.Pressed) {
					SetHoverState (controller, HoverState.DraggedAway);
				}
			}

			#if UNITY_EDITOR
			if (debugHover)
			{
				Debug.Log(name + ": trigger exit: " + controller.name + " -- " + controller.SortedHoveredObjectsContains(this));
			}
			#endif
			if (controller.SortedHoveredObjectsContains(this))
			{
				RemoveHover(controller);
			}
		}

		public Vector3 GetClosestCollisionPoint(Transform attachment)
		{
			// Find location on object that works.
			Vector3 closestPoint = Vector3.zero;
			float shortestDistance = float.MaxValue;
			foreach (Collider c in GetComponentsInChildren<Collider>())
			{				
				Vector3 closest = c.bounds.ClosestPoint(attachment.transform.position);
				float distance = Vector3.Distance(attachment.transform.position, closest);
				if (distance < shortestDistance)
				{
					shortestDistance = distance;
					closestPoint = closest;
				}
			}
			return closestPoint;
		}


		protected void SetCollidersEnabled(bool enabled)
		{
			foreach(Collider c in GetComponentsInChildren<Collider>())
			{
				c.enabled = enabled;
			}
		}

		public virtual float DistanceFromController(ProximityController controller) {
			Vector3 position = controller.InteractionSourceTransform ().position;
			float shortestDistance = float.MaxValue;
			foreach (Collider collider in GetComponentsInChildren<Collider>()) {
				Vector3 closest = collider.ClosestPointOnBounds (position);
				float distance = (closest - position).magnitude;// Vector3.Distance (AttachPosition (), closest);

				if (distance < shortestDistance) {
					shortestDistance = distance;
				}
			}
			return shortestDistance;
		}

		public virtual void OnDetachedController(ProximityController controller) {
		}
	}
}