using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MLJetPack.Interactions {
	public class InteractableUi : Interactable {
		// Wrapper for Selectable object to respond to Interactor objects
		public Selectable uiObject;

		protected override void ApplyHoverState (HoverState newHoverState, bool isStateChange, List<ProximityController> changedControllers)
		{
			foreach (ProximityController interactor in changedControllers) {
				if (interactor == null) {
					continue;
				}

				LaserBeamController lbi = interactor.GetComponent<LaserBeamController> ();
				switch (newHoverState) {
				case HoverState.Hovered:
				case HoverState.Pressed:
					if (lbi.hoveredObject != this) {
//						Debug.Log (name + ": Adding hover state for pointer event: " + lbi.name);
						UILaserInputModule.instance.HandleInputExitAndEnter (lbi.uiLaser.pointerEvent, gameObject);
						lbi.hoveredObject = this;
					}
					break;

				case HoverState.DraggedAway:
				case HoverState.None:
					if (lbi.hoveredObject == this) {
//						Debug.Log (name + ": Removing hover state for pointer event: " + lbi.name);
						UILaserInputModule.instance.HandleInputExitAndEnter (lbi.uiLaser.pointerEvent, null);
						lbi.hoveredObject = null;
					}
					break;
				}
			}
		}

		public override float DistanceFromController(ProximityController controller) {
			return (controller.InteractionSourceTransform ().position - transform.position).magnitude;
		}
	}
}
