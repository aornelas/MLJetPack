using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace MLJetPack.Interactions {
	public class UILaserInputModule : UnityEngine.EventSystems.BaseInputModule
	{
		private static UILaserInputModule _instance = null;
		public static UILaserInputModule instance { 
			get {
				if (_instance == null) {
					EventSystem eventSystem = GameObject.FindObjectOfType<EventSystem>();
					if (eventSystem == null)
					{
						GameObject es = new GameObject("UILaserModule");
						eventSystem = es.AddComponent<EventSystem>();
					}

#if XR_HOLOLENS
					if (EventSystem.current.GetComponent<StandaloneInputModule>() != null)
					{
					EventSystem.current.GetComponent<StandaloneInputModule>().enabled = false;
					}
					if (EventSystem.current.GetComponent<BaseInput>() != null)
					{
					EventSystem.current.GetComponent<BaseInput>().enabled = false;
					}
#endif

                    _instance = eventSystem.GetComponent<UILaserInputModule>();
                    if (_instance == null) {
                        _instance = eventSystem.gameObject.AddComponent<UILaserInputModule>();
                    }
				}
				return _instance;
			}
		}

		public Dictionary<Selectable, InteractableUi> uiLookup
		= new Dictionary<Selectable, InteractableUi>();

		public InteractableUi GetUiSelectable(Selectable selectable) {
			if (selectable == null) {
				return null;
			}

			if (!uiLookup.ContainsKey (selectable)) {
				uiLookup [selectable] = selectable.gameObject.AddComponent<InteractableUi>();
				uiLookup [selectable].uiObject = selectable;
			}
			return uiLookup [selectable];
		}


        [SerializeField]
		private Camera UICamera;

		public bool debugLaserTracking = false;

		protected override void Awake()
		{
			base.Awake();
			//Debug.Log("Instancing as child of: " + transform.parent + " as part of go " + gameObject.name);
			if(_instance != null) {
				Debug.LogWarning("Trying to instantiate multiple UILaserInputModule.");
				DestroyImmediate(this.gameObject);
			}

			_instance = this;

			StandaloneInputModule sim = GetComponent<StandaloneInputModule> ();
			if (sim != null) {
				Destroy (sim);
			}
		}

		void DisableThis() {
			this.enabled = false;
		}

		protected override void OnDestroy()
		{
			if (_instance == this) {
				_instance = null;
			}
		}

        protected override void Start()
        {
            base.Start();
            UpdateCamera();
        }

        void UpdateCamera() {
            if (GetComponentInChildren<Camera>(true) != null)
            {
                UICamera = GetComponentInChildren<Camera>(true);

            } else {
                // Create a new camera that will be used for raycasts
                UICamera = new GameObject("UI Camera").AddComponent<Camera>();
                UICamera.clearFlags = CameraClearFlags.Nothing;
                UICamera.cullingMask = 0;
                UICamera.fieldOfView = 5;
                UICamera.nearClipPlane = 0.01f;
                UICamera.transform.SetParent(this.transform, false);
            }

            RefreshCameras ();
		}

		void Update() {
            if (UICamera == null)
            {
                UpdateCamera();
            }

            #if UNITY_EDITOR
            //			if (Input.GetKeyUp(KeyCode.C)) {
            //				ClearSelection();
            //			}

            #endif
            //			foreach (var pair in trackedControllerData) {
            //				LaserPointerEventData lpEventData = pair.Value;
            //				// drag handling
            //				if (lpEventData.currentObjectBeingDragged != null) {
            //					ExecuteEvents.ExecuteHierarchy (lpEventData.currentObjectBeingDragged, lpEventData.pointerEvent, ExecuteEvents.dragHandler);
            //				}
            //			}
            //			// update selected element for keyboard focus
            //			if (EventSystem.current.currentSelectedGameObject != null) {
            //				ExecuteEvents.ExecuteHierarchy (EventSystem.current.currentSelectedGameObject, UILaserInputModule.instance.BaseEventData (), ExecuteEvents.updateSelectedHandler);
            //			}
            // update selected element for keyboard focus
            if (EventSystem.current.currentSelectedGameObject != null) {
//				Debug.Log ("Updating selected element for keyboard focus: " + EventSystem.current.currentSelectedGameObject.name);
				ExecuteEvents.ExecuteHierarchy (EventSystem.current.currentSelectedGameObject, UILaserInputModule.instance.BaseEventData (), ExecuteEvents.updateSelectedHandler);
			}
		}

		public void RegisterCanvas(Canvas c) {
			if (c.renderMode == RenderMode.WorldSpace) {
				c.worldCamera = UICamera;
			}
		}

		public void RefreshCameras() {
			// Find canvases in the scene and assign our custom
			// UICamera to them
			Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas> ();            
			foreach (Canvas canvas in canvases) {
				if (canvas.renderMode == RenderMode.WorldSpace)
					canvas.worldCamera = UICamera;
			}
		}

		public void UpdateCameraPosition(UILaser uiLaser)
		{
			if (uiLaser.isActiveAndEnabled) {
				UICamera.enabled = true;
				UICamera.transform.position = uiLaser.transform.position;
				UICamera.transform.rotation = uiLaser.transform.rotation;

                uiLaser.GetPointerEvent().position = 
					new Vector2 (UICamera.pixelWidth * 0.5f, UICamera.pixelHeight * 0.5f);

			} else if (UICamera != null) {
				UICamera.enabled = false;
			}
		}

		// clear the current selection
		public void ClearSelection()
		{
			if(base.eventSystem.currentSelectedGameObject) {
				#if UNITY_EDITOR
				if (debugLaserTracking) {
					Debug.Log ("Clearing selection");
				}
				#endif
				base.eventSystem.SetSelectedGameObject(null);
			}
		}

		// select a game object
		public void SelectGameObject(GameObject go)
		{
			ClearSelection();

			GameObject selectable = go != null ? 
				(go.GetComponentInParent<Selectable> () != null ? go.GetComponentInParent<Selectable> ().gameObject : go) : 
				null;

			#if UNITY_EDITOR
			if (debugLaserTracking) {
				Debug.Log ("Selecting: " + (selectable != null ? selectable.name : "null"));
			}
			#endif
//			if(ExecuteEvents.GetEventHandler<ISelectHandler>(selectable)) {
				base.eventSystem.SetSelectedGameObject(selectable);
//			}
		}

		bool IsSelectableObject (GameObject gameObject) {
			return gameObject.GetComponentInParent<UnityEngine.UI.Selectable> () != null;
		}

		public override void Process()
		{
			// Do nothing here

//			OldProcessScript ();
		}

//		public void OldProcessScript()
//		{
//			if (!enabled) {
//				return;
//			}
//			#if UNITY_EDITOR
//			if (debugLaserTracking)
//			{
//				Debug.Log(name + ": Module Process Start");
//			}
//			#endif
//
//			foreach (var pair in trackedControllerData) {
//				LaserBeamInteractor interactor = pair.Key;
//				interactor.uiLaser.hitObject = null;
//
//				#if UNITY_EDITOR
//				if (debugLaserTracking)
//				{
////					Debug.Log(name + ": Process " + Time.frameCount + " -> Controller: " + uiLaser.name);
//				}
//				#endif
//
//				LaserPointerEventData data = pair.Value;

        //
////				if (uiLaser.IsConnectedBeam())
////				{
////					#if UNITY_EDITOR
////					if (debugLaserTracking)
////					{
////						Debug.Log("Is Connected Beam. Ignore");
////					}
////					#endif
//////					controller.ClearButtonFlags ();
////					continue;
////				}
//
//				// Test if UICamera is looking at a GUI element
//				UpdateCameraPosition(interactor.uiLaser);
//
//				if(data.pointerEvent == null)
//					data.pointerEvent = new PointerEventData(eventSystem);
//				else
//					data.pointerEvent.Reset();
//
//				data.pointerEvent.delta = Vector2.zero;
//
//				if (UICamera.enabled) {
//					#if UNITY_EDITOR
//					if (debugLaserTracking)
//					{
////						Debug.Log("UI Camera is enabled");
//					}
//					#endif
//					data.pointerEvent.position = new Vector2 (UICamera.pixelWidth * 0.5f, UICamera.pixelHeight * 0.5f);
//				}
//				else {
//					#if UNITY_EDITOR
//					if (debugLaserTracking)
//					{
//						Debug.Log("No UI Camera is enabled");
//					}
//					#endif
//					ClearSelection ();
//					data.pointerEvent.position = new Vector2 (-1,-1);
//				}
//
//				// trigger a raycast
//				eventSystem.RaycastAll(data.pointerEvent, m_RaycastResultCache);
//
//				#if UNITY_EDITOR
//				if (debugLaserTracking) {
////					Debug.Log ("Raycast size: " + m_RaycastResultCache.Count);
//				}
//				#endif
////				Debug.Log (Time.frameCount + ": " + name + ": Selected Object = " + 
////					(EventSystem.current.currentSelectedGameObject != null ?
////						EventSystem.current.currentSelectedGameObject.name : "null"));
//
//				if (m_RaycastResultCache.Count > 0) {
//					RaycastResult tempRaycast = m_RaycastResultCache [0];
//					for (int i = 1; i < m_RaycastResultCache.Count; i++) {
//						if (!IsSelectableObject (tempRaycast.gameObject) ||
//							(m_RaycastResultCache [i].distance < tempRaycast.distance)) {
//							if (IsSelectableObject (m_RaycastResultCache [i].gameObject)) {
//								tempRaycast = m_RaycastResultCache [i];
//								#if UNITY_EDITOR
////								if (debugLaserTracking) {
////									Debug.Log ("Raycast: " + m_RaycastResultCache [i].gameObject.transform.parent.name);
////								}
//								#endif
//							}
//						}
//					}
////					#if UNITY_EDITOR
////					if (debugLaserTracking) {
////						if (IsSelectableObject (tempRaycast.gameObject)) {
////							Debug.Log ("Captured Selectable Raycast: " + tempRaycast.gameObject.transform.parent.name);
////
////						} else {
////							Debug.Log ("Captured Unselectable Raycast: " + tempRaycast.gameObject.transform.parent.name);
////						}
////					}
////					#endif
//					data.pointerEvent.pointerCurrentRaycast = tempRaycast;
//
//				} else {
//					data.pointerEvent.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
//				}
//
//				//                data.pointerEvent.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
//				m_RaycastResultCache.Clear();
//
//				// make sure our controller knows about the raycast result
//				// we add 0.01 because that is the near plane distance of our camera and we want the correct distance
//				bool isValidHit = data.pointerEvent.pointerCurrentRaycast.distance > 0.0f;
//				if (isValidHit) {
//					isValidHit = 
//						(interactor.uiLaser.hitObject == data.pointerEvent.pointerCurrentRaycast.gameObject) ||
//						(data.pointerEvent.pointerCurrentRaycast.distance <= interactor.uiLaser.maxDistance);
//				}
//
//				if (isValidHit) {
//					if (data.pointerEvent.pointerCurrentRaycast.gameObject != null) {
//						if (debugLaserTracking) {
//							Debug.Log ("Inspecting hit object: " + data.pointerEvent.pointerCurrentRaycast.gameObject.name);
//						}
//
//						if (data.pointerEvent.pointerCurrentRaycast.gameObject.GetComponent<UnityEngine.UI.Selectable> () != null ||
//						    data.pointerEvent.pointerCurrentRaycast.gameObject.GetComponentInParent<UnityEngine.UI.Selectable> () != null) {
//							interactor.uiLaser.hitObject = data.pointerEvent.pointerCurrentRaycast.gameObject;
//							interactor.uiLaser.hitObjectDistance = data.pointerEvent.pointerCurrentRaycast.distance;
//
//							if (debugLaserTracking) {
//								Debug.Log ("Setting UI object distance for : " + data.pointerEvent.pointerCurrentRaycast.gameObject.name);
//							}
//
//						} else {
//							UnityEngine.UI.Graphic graphic = data.pointerEvent.pointerCurrentRaycast.gameObject.GetComponentInParent<UnityEngine.UI.Graphic> ();
//							if (graphic != null && graphic.raycastTarget) {
//								interactor.uiLaser.hitObject = data.pointerEvent.pointerCurrentRaycast.gameObject;
//								interactor.uiLaser.hitObjectDistance = data.pointerEvent.pointerCurrentRaycast.distance;
//
//								if (debugLaserTracking) {
//									Debug.Log ("Limiting distance for raycast target: " + data.pointerEvent.pointerCurrentRaycast.gameObject.name + " to " + data.pointerEvent.pointerCurrentRaycast.distance);
//								}
//							}
//						}
//					}
//				}
//
//				#if UNITY_EDITOR
//				else {
////					Debug.Log(Time.frameCount + ": Not close enough to process: " + 
////						data.pointerEvent.pointerCurrentRaycast.distance + " vs " + 
////						uiLaser.maxDistance + "; found " + 
////						(data.pointerEvent.pointerCurrentRaycast.gameObject != null ?
////							data.pointerEvent.pointerCurrentRaycast.gameObject.name : "null") +
////						", laser sees " + (uiLaser.hitObject != null ? uiLaser.hitObject.name : "null"));
//					continue;
//				}
//				#endif
//
////				 stop if no UI element was hit
//				if(data.pointerEvent.pointerCurrentRaycast.gameObject == null)
//					continue;
//				
//				// Send control enter and exit events to our controller
//				var hitControl = data.pointerEvent.pointerCurrentRaycast.gameObject;
//				if(data.currentlyHoveredObject != hitControl) {
//					if (data.currentlyHoveredObject != null) {
//						interactor.uiLaser.hitObject = null;
//						interactor.uiLaser.hitObjectDistance = float.MaxValue;
//
//						data.clickDownObject = null;
//					}
//
//					if (hitControl != null) {
//						interactor.uiLaser.hitObject = hitControl;
//						interactor.uiLaser.hitObjectDistance = data.pointerEvent.pointerCurrentRaycast.distance;
//					}
//				}
//
////				if (data.currentPoint != hitControl) {
////					Debug.Log (Time.frameCount + ": Handling Exit: " + (data.currentPoint != null ? data.currentPoint.name : "null") 
////						+ ", Enter: " + (hitControl.gameObject != null ? hitControl.gameObject.name : "null"));
////					SelectGameObject (hitControl);
////				}
//				data.currentlyHoveredObject = hitControl;
//
//				// Handle enter and exit events on the GUI controlls that are hit
////				base.HandlePointerExitAndEnter(data.pointerEvent, data.currentlyHoveredObject);
//			}
//		}

		public bool debugPointer = true;
		public void HandleInputExitAndEnter(PointerEventData currentPointerData, GameObject newEnterTarget) {
//			if (newEnterTarget != null) {
//				currentPointerData.pointerEnter = null;
//			}

			// if we have not changed hover target
			if (currentPointerData.pointerEnter == newEnterTarget)
				return;

			GameObject commonRoot = FindCommonRoot(currentPointerData.pointerEnter, newEnterTarget);

//			if (debugPointer) {
//				if (currentPointerData.pointerEnter != null) {
//					Debug.Log ("Current pointer is not null: " + currentPointerData.pointerEnter.name);
//				} else {
//					Debug.Log ("Current pointer is null");
//				}
//			}
			// and we already an entered object from last time
			if (currentPointerData.pointerEnter != null)
			{
				// send exit handler call to all elements in the chain
				// until we reach the new target, or null!
				Transform t = currentPointerData.pointerEnter.transform;
				while (t != null)
				{
					// if we reach the common root break out!
					if (commonRoot != null && commonRoot.transform == t)
						break;

					#if UNITY_EDITOR
					if (debugLaserTracking) {
						Debug.Log ("Calling exit handler for: " + t.name);
					}
					#endif

					ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerExitHandler);
					t = t.parent;
				}
			}

			// now issue the enter call up to but not including the common root
			if (newEnterTarget != null)
			{
				Transform targetT = newEnterTarget.transform;

				while (targetT != null && targetT.gameObject != commonRoot)
				{
					#if UNITY_EDITOR
					if (debugLaserTracking) {
						Debug.Log ("Calling enter handler for: " + targetT.name);
					}
					#endif

					ExecuteEvents.Execute(targetT.gameObject, currentPointerData, ExecuteEvents.pointerEnterHandler);
					targetT = targetT.parent;
				}
			}
			currentPointerData.pointerEnter = newEnterTarget;
		}

		public UnityEngine.EventSystems.BaseEventData BaseEventData() {
			return GetBaseEventData ();
		}








		public bool ScanForObjects(UILaser uiLaser, float maxDistance)
		{
			//				if (uiLaser.IsConnectedBeam())
			//				{
			//					#if UNITY_EDITOR
			//					if (debugLaserTracking)
			//					{
			//						Debug.Log("Is Connected Beam. Ignore");
			//					}
			//					#endif
			////					controller.ClearButtonFlags ();
			//					continue;
			//				}

			// Test if UICamera is looking at a GUI element
			UpdateCameraPosition(uiLaser);

			if(uiLaser.pointerEvent == null)
				uiLaser.pointerEvent = new PointerEventData(eventSystem);
			else
				uiLaser.pointerEvent.Reset();

			uiLaser.pointerEvent.delta = Vector2.zero;

			if (UICamera != null && UICamera.enabled) {
				#if UNITY_EDITOR
				if (debugLaserTracking)
				{
//					Debug.Log("UI Camera is enabled");
				}
				#endif
				uiLaser.pointerEvent.position = new Vector2 (UICamera.pixelWidth * 0.5f, UICamera.pixelHeight * 0.5f);
			}
			else {
				#if UNITY_EDITOR
				if (debugLaserTracking)
				{
//					Debug.Log("No UI Camera is enabled");
				}
				#endif
				ClearSelection ();
				uiLaser.pointerEvent.position = new Vector2 (-1,-1);
			}

			// trigger a raycast
			eventSystem.RaycastAll(uiLaser.pointerEvent, m_RaycastResultCache);

			#if UNITY_EDITOR
			if (debugLaserTracking) {
//				Debug.Log ("Raycast size: " + m_RaycastResultCache.Count);
			}
			#endif

			if (m_RaycastResultCache.Count > 0) {
				// Finds the shortest raycast if there are multiple
				//
				RaycastResult tempRaycast = m_RaycastResultCache [0];
				for (int i = 1; i < m_RaycastResultCache.Count; i++) {
					if (!IsSelectableObject (tempRaycast.gameObject) ||
						(m_RaycastResultCache [i].distance < tempRaycast.distance)) {
						if (IsSelectableObject (m_RaycastResultCache [i].gameObject)) {
                            if (m_RaycastResultCache[i].distance <= maxDistance)
                            {

                                tempRaycast = m_RaycastResultCache[i];
#if UNITY_EDITOR
                                if (debugLaserTracking)
                                {
                                    Debug.Log("Raycast: " + m_RaycastResultCache[i].gameObject.transform.parent.name);
                                }
#endif
                            }
						}
					}
				}

				uiLaser.pointerEvent.pointerCurrentRaycast = tempRaycast;

			} else {
				uiLaser.pointerEvent.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
			}

			m_RaycastResultCache.Clear();

			//				 stop if no UI element was hit
			if (uiLaser.pointerEvent.pointerCurrentRaycast.gameObject == null) {
				uiLaser.hitObject = null;
				uiLaser.hitObjectDistance = float.MaxValue;
				return false;
			}

			uiLaser.hitObject = uiLaser.pointerEvent.pointerCurrentRaycast.gameObject;
			uiLaser.hitObjectDistance = uiLaser.pointerEvent.pointerCurrentRaycast.distance;
			return true;
		}
	}
}