using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace MLJetPack.Interactions {
	// storage class for controller specific data
	public class UILaser : LaserBase {
		public PointerEventData pointerEvent;

		public GameObject currentlyPressedObject;
		public GameObject currentObjectBeingDragged;

		public GameObject 	clickDownObject;

		public float 		clickDownTime;

		public override Vector3 HitObjectNormal() {
			return -Vector3.forward;
		}

		public override bool IsConnectedBeam ()
		{
			return currentObjectBeingDragged != null;
		}

		void Start() {
			if (parentBeamInteractor == null) {
				parentBeamInteractor = GetComponentInParent<LaserBeamController> ();
			}

			InitializePointer ();
		}

		public LaserBeamController parentBeamInteractor;
		public float maxDistance {
			get {
				if (parentBeamInteractor != null) {
					return Mathf.Min (parentBeamInteractor.currentLaserDistance, 
						parentBeamInteractor.maxLaserDistance) + .125f;
				}
				return 10;
			}
		}

		private bool pointerInitialized = false;

		// Use this for initialization
		void InitializePointer()
		{			
			if (pointerInitialized) {
				return;
			}

			pointerEvent = new PointerEventData (UILaserInputModule.instance.GetComponent<EventSystem>());

			// register with the LaserPointerInputModule
			pointerInitialized = true;
		}

        public PointerEventData GetPointerEvent() {
            if (pointerEvent == null) {
                pointerEvent = new PointerEventData(UILaserInputModule.instance.GetComponent<EventSystem>());
            }
            return pointerEvent;
        }

		void OnDestroy()
		{
		}

		public virtual bool PerformClickOnButtonDownEvent()
		{
			return false;
		}
			
		// select a game object
		public void SelectUiObject(GameObject go)
		{
			ClearUiSelection();

			if(ExecuteEvents.GetEventHandler<ISelectHandler>(go)) {
//				Debug.Log (name + ": selecting game object: " + go.name);
				EventSystem.current.SetSelectedGameObject(go);
			}
		}

		public void ClearUiSelection() {
			if (EventSystem.current.currentSelectedGameObject != null) {
//				Debug.Log (name + ": selecting no game object, last was: " + EventSystem.current.currentSelectedGameObject.name);
				EventSystem.current.SetSelectedGameObject (null);
			} else {
//				Debug.Log (name + ": nothing to deselect");
			}
		}

		public bool debugLaser = false;

		public override void PassButtonDown(Interactable pointerOverInteractable) {
			if (debugLaser) {
				Debug.Log ("Pass Button Down");
			}

			UILaserInputModule.instance.UpdateCameraPosition (this);

			pointerEvent.pressPosition = pointerEvent.position;
			pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
			pointerEvent.pointerPress = null;

			clickDownTime = Time.time;

			if (!hitObject) {
				return;
			}

			// update current pressed if the curser is over an element
			if (pointerOverInteractable.hoverState == HoverState.Hovered) {
				currentlyPressedObject = pointerOverInteractable.gameObject;

				GameObject newlyPressedObject = ExecuteEvents.ExecuteHierarchy(currentlyPressedObject, pointerEvent, ExecuteEvents.pointerDownHandler);
				if(newlyPressedObject != null) {
					pointerEvent.pointerPress = newlyPressedObject;
					currentlyPressedObject = newlyPressedObject;

					SelectUiObject(currentlyPressedObject);
				}

				if (debugLaser) {
					Debug.Log ("Begin Drag Handle");
				}

				GameObject dragObject = ExecuteEvents.ExecuteHierarchy(currentlyPressedObject, pointerEvent, ExecuteEvents.beginDragHandler);
				if (debugLaser) {
					Debug.Log ("Drag Object: " + (dragObject != null ? dragObject.name : "null"));
				}
				pointerEvent.pointerDrag = currentlyPressedObject;
				currentObjectBeingDragged = currentlyPressedObject;
				clickDownObject = currentlyPressedObject;

				pointerOverInteractable.hoverState = HoverState.Pressed;
			}
		}

		public override void PassButtonUp(Interactable pointerOverInteractable, float clickThreshold = 10) {
			if(currentObjectBeingDragged != null) {
				if (debugLaser) {
					Debug.Log ("End Drag Handle");
				}
				ExecuteEvents.ExecuteHierarchy(currentObjectBeingDragged, pointerEvent, ExecuteEvents.endDragHandler);
				if(pointerOverInteractable != null) {
					if (debugLaser) {
						Debug.Log ("Execute Drop Handler");
					}
					ExecuteEvents.ExecuteHierarchy(pointerOverInteractable.gameObject, pointerEvent, ExecuteEvents.dropHandler);
				}
				pointerEvent.pointerDrag = null;
				currentObjectBeingDragged = null;
			}

			if (currentlyPressedObject == null && pointerOverInteractable != null) {
				currentlyPressedObject = pointerOverInteractable.gameObject;
			}

			bool clearSelection = false;
			if (currentlyPressedObject) {
				if (currentlyPressedObject == clickDownObject) {
					if (Time.time - clickDownTime < clickThreshold) {
						if (pointerOverInteractable.hoverState == HoverState.Pressed) {
							if (!PerformClickOnButtonDownEvent ()) {
								if (debugLaser) {
									Debug.Log ("Execute Pointer Up Handler");
								}
								ExecuteEvents.ExecuteHierarchy (currentlyPressedObject, pointerEvent, ExecuteEvents.pointerClickHandler);
							}
						}
					}
					ExecuteEvents.ExecuteHierarchy (currentlyPressedObject, pointerEvent, ExecuteEvents.pointerUpHandler);

					if (currentlyPressedObject.GetComponent<Button> () != null) {
						clearSelection = true;
					}
					pointerEvent.rawPointerPress = null;
					pointerEvent.pointerPress = null;
					currentlyPressedObject = null;

					UILaserInputModule.instance.HandleInputExitAndEnter (pointerEvent, null);
				}
			}
			clickDownObject = null;
			if (clearSelection) {
				ClearUiSelection ();
			}
		}

        public override bool ScanForObjects (float maxDistance)
		{
			if (UILaserInputModule.instance != null && UILaserInputModule.instance.enabled) {
                //Debug.Log("Scan for laser at max d: " + maxDistance);
                return UILaserInputModule.instance.ScanForObjects (this, maxDistance);
			}
			return false;
		}

		void Update() {
			if (currentObjectBeingDragged != null) {
				ExecuteEvents.ExecuteHierarchy (currentObjectBeingDragged, pointerEvent, ExecuteEvents.dragHandler);
			}
		}
	}
}
