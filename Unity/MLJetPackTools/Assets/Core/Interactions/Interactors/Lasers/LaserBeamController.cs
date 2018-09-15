using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

namespace MLJetPack.Interactions {
	public class LaserBeamController : ProximityController
	{
		public List<LaserBase> lasers = new List<LaserBase>();

		public GameObject 	reticle;
		public float 		reticleScale = 0.05f;

		public float 		maxLaserDisplayDistance = 20;
		public float 		maxLaserDistance = 20;
		public float		currentLaserDistance = float.MaxValue;

		public delegate void PreUpdateCallback();
		public PreUpdateCallback OnPreUpdate;

		public UILaser uiLaser;

		// Use this for initialization
		protected override void Start ()
		{
			base.Start ();

			foreach (LaserBase laser in GetComponentsInChildren<LaserBase>(true)) {
				if (!lasers.Contains (laser)) {
					lasers.Add (laser);
				}
				if (laser.GetComponent<UILaser>() != null) {
					uiLaser = laser.GetComponent<UILaser> ();
				}
			}
		}

		// Update is called once per frame
		protected override void Update ()
		{
			if (OnPreUpdate != null) {
				OnPreUpdate ();
			}
				
			// If there's a connected beam:
//			LaserBase connectedLaser = null;
//			foreach (LaserBase laser in lasers) {
//				// This is if an object is essentially glued to the laser
//				// Action transfers to the laser in this case if it is a nested controller.
//				//
//				if (laser.IsConnectedBeam ()) {
//					connectedLaser = laser;
//					break;
//				}
//			}

			//		Scan for UI & Physics
			ScanForObjects (maxLaserDistance);//Mathf.Min(maxLaserDistance, currentLaserDistance) + .125f);

			base.Update ();
        }
        
		public Interactable lastClosestInteractable = null;

		#if UNITY_EDITOR
		public LaserBase thisIsClosestLaser;
		public GameObject thisIsClosestLaserHitObject;
		public Selectable thisIsClosestLaserSelectable;
		#endif

		public Interactable hoveredObject;

        public delegate void LaserHoverCallback(LaserBeamController controller, Interactable lastInteractable, Interactable currentInteractable);
        public LaserHoverCallback OnLaserHoverOut;
        public LaserHoverCallback OnLaserHoverIn;

        void ScanForObjects(float maxDistance = -1) {
			if (RaycastShouldIgnoreLaserHit (this)) {
				SetReticleAtDistance (MaxRaycastDistance(this));
				return;
			}
				
			float maxRaycastDistance = MaxRaycastDistance (this);
			if (maxRaycastDistance > 0) {
				maxDistance = Mathf.Min (maxRaycastDistance, maxDistance);
			}

			List<LaserBase> lasersHit = new List<LaserBase> ();
			if (mappedController.InteractionFunctionsEnabled ()) {
				#if UNITY_EDITOR
				if (debugInteractor) {
					Debug.Log ("Interaction functions enabled");
				}
				#endif
				foreach (LaserBase laser in GetComponentsInChildren<LaserBase>(true)) {
					#if UNITY_EDITOR
					if (debugInteractor) {
						Debug.Log ("Laser scan: " + laser.name);
					}
					#endif
					if (laser.ScanForObjects (maxDistance)) {
                        if (laser.hitObjectDistance <= maxDistance) {
                            lasersHit.Add(laser);
                        }
						#if UNITY_EDITOR
						if (debugInteractor) {
                            Debug.Log ("Laser found " + laser);
						}
						#endif
					}
				}
			} else {
				#if UNITY_EDITOR
				if (debugInteractor) {
					Debug.Log ("Interaction functions NOT enabled");
				}
				#endif
			}

			Interactable currentHoveredInteractable = null;
			if (lasersHit.Count > 0) {
				LaserBase closestLaser = null;

				foreach (LaserBase laser in lasersHit) {
					if ((closestLaser == null) || (closestLaser.hitObjectDistance >= laser.hitObjectDistance)) {
						if (closestLaser != null && (closestLaser.hitObjectDistance == laser.hitObjectDistance)) {
							if (laser.hitObject.GetComponentInParent<Selectable> () != null) {
								closestLaser = laser;
								//Debug.Log ("== Closest laser is " + closestLaser.hitObject.name + " at " + closestLaser.hitObjectDistance);
							}
						} else {
							closestLaser = laser;
							//Debug.Log ("Closest laser is " + closestLaser.hitObject.name + " at " + closestLaser.hitObjectDistance);
						}
					}
				}
					
				#if UNITY_EDITOR
				thisIsClosestLaser = closestLaser;
				thisIsClosestLaserHitObject = closestLaser.hitObject;
				thisIsClosestLaserSelectable = closestLaser.hitObject.GetComponentInParent<Selectable> ();
				#endif

				// Add hover
				Selectable currentHoveredSelectable = closestLaser.hitObject.GetComponentInParent<Selectable> ();
				currentHoveredInteractable = UILaserInputModule.instance.GetUiSelectable (currentHoveredSelectable);
				if (currentHoveredInteractable == null) {
					currentHoveredInteractable = closestLaser.hitObject.GetComponentInParent<Interactable> ();
				}

				if ((mappedController.InteractionFunctionsEnabled() && DriverOperationIdle (0)) || 
					DriverOperationStarted(0)) {
					// If in the unpressed state, add hover if available, remove old hover if needed

					if (currentHoveredInteractable != null && lastClosestInteractable != currentHoveredInteractable) {
						#if UNITY_EDITOR
						if (debugInteractor) {
							Debug.Log (currentHoveredInteractable.name + ": trigger enter");
						}
						#endif
						currentHoveredInteractable.TriggerEnter (this);
					}

					if (lastClosestInteractable != null && 
						lastClosestInteractable.GetComponent<InteractableUi>() != null) {
						if (currentHoveredInteractable == null ||
						    currentHoveredInteractable.GetComponent<InteractableUi> () == null) {

							#if UNITY_EDITOR
							if (debugInteractor) {
								Debug.Log ("clear ui selection for " + lastClosestInteractable.name + ": not interactable ui " +
									(currentHoveredInteractable == null ? "null" : currentHoveredInteractable.name));
							}
							#endif

							uiLaser.ClearUiSelection ();
						}
					}

				} else {
					// Controller is in pressed or exiting state. Only process if pressed with downed object
					if (downedInteractable != null) {
						// If this is a non-physics ui, hit the trigger manually
						if (downedInteractable == currentHoveredInteractable) {
							if (downedInteractable.hoverState == HoverState.DraggedAway) {
								#if UNITY_EDITOR
								if (debugInteractor) {
									Debug.Log ("Previously dragged away: CALL UI TRIGGER ENTER: " + downedInteractable.name);
								}
								#endif
								downedInteractable.TriggerEnter (this);
							}
						} else {
							// Different hit object
							if (downedInteractable.hoverState == HoverState.Pressed) {
								#if UNITY_EDITOR
								if (debugInteractor) {
									Debug.Log ("Pressed and dragging back off: CALL UI TRIGGER EXIT: " + downedInteractable.name);
								}
								#endif
								downedInteractable.TriggerExit (this);
							}
						}

//						if (hoveredInteractableUi != null &&
//							(downedInteractable.gameObject == hoveredInteractableUi.gameObject)) {
//							downedInteractable.TriggerEnter (this);
////							// We are on the downed ui object. We need to check the hover state
////							if (downedInteractable.hoverState == HoverState.DraggedAway) {
////								downedInteractable.SetHoverState (this, HoverState.Pressed);
////							}
//						} else {
//							downedInteractable.TriggerExit (this);
////							if (downedInteractable.hoverState == HoverState.Pressed) {
////								downedInteractable.SetHoverState (this, HoverState.DraggedAway);
////							}
//						}
					}
				}

				if (RaycastShouldUpdateReticlePosition (this, true)) {
					float reticleDistance = closestLaser.hitObjectDistance + RaycastDistanceAdjustment (this, true);
					SetReticleAtDistance (reticleDistance, closestLaser.HitObjectNormal ());
				}

			} else {
				if (DriverOperationStarted (0)) { // If button down with no object
					if (lastClosestInteractable != null && 
						lastClosestInteractable.GetComponent<InteractableUi>() != null) {

						#if UNITY_EDITOR
						if (debugInteractor) {
							Debug.Log ("clear ui selection for " + lastClosestInteractable.name + ": nothing lasered");
						}
						#endif

						uiLaser.ClearUiSelection ();
					}

				} else if (downedInteractable != null) {
					#if UNITY_EDITOR
					if (debugInteractor) {
						Debug.Log ("trigger exit for downed interactable: " + downedInteractable.name + ": nothing lasered");
					}
					#endif
					downedInteractable.TriggerExit (this);
				}

				if (RaycastShouldUpdateReticlePosition (this, false)) {
					SetReticleAtDistance (maxRaycastDistance == -1 ? maxLaserDistance : maxRaycastDistance, 
											Vector3.forward);
				}
			}

            bool hoverFrameOut = false;
            bool hoverFrameIn = false;

            if (lastClosestInteractable != currentHoveredInteractable) {
                // Something changed
				if (lastClosestInteractable != null) {
					#if UNITY_EDITOR
					if (debugInteractor) {
						Debug.Log (Time.frameCount + ": Last Closest interactable: " + lastClosestInteractable.name +
							" is not current lasered: " + (currentHoveredInteractable == null ? "null" : currentHoveredInteractable.name));

						if (thisIsClosestLaserHitObject != null) {
							Debug.Log (" -- Closest laser hit object: " + thisIsClosestLaserHitObject.name);
						}
						if (thisIsClosestLaserSelectable != null) {
							Debug.Log (" -- Closest laser hit object: " + thisIsClosestLaserSelectable.name);
						}
					}
					#endif

					lastClosestInteractable.TriggerExit (this);
                    if (currentHoveredInteractable == null) {
                        // was not null, now is null
                        hoverFrameOut = true;
                    }
                } else { // was null, now is not
                    hoverFrameIn = true;
                }
			}
			lastClosestInteractable = currentHoveredInteractable;

            if (hoverFrameOut && OnLaserHoverOut != null) {
                OnLaserHoverOut(this, lastClosestInteractable, currentHoveredInteractable);
            } else if (hoverFrameIn && OnLaserHoverIn != null) {
                OnLaserHoverIn(this, lastClosestInteractable, currentHoveredInteractable);
            }
        }

        public delegate void ReticleSetCallback(GameObject reticle);
		public ReticleSetCallback OnReticleSet;

		Vector3? previousReticlePosition;
		Vector3? currentReticlePosition;

		public Transform reticleRotationReceiver;

		void SetReticleAtDistance(float distance, Vector3? normal = null) {
			#if UNITY_EDITOR
			if (debugInteractor) {
				Debug.Log ("Setting reticle at distance: " + distance.ToString ("F3"));
			}
			#endif
			currentLaserDistance = distance;

			if (currentReticlePosition.HasValue) {
				previousReticlePosition = currentReticlePosition.Value;
			}

			currentReticlePosition = reticle.transform.position = transform.position + transform.forward * distance;

			if (!previousReticlePosition.HasValue) {
				previousReticlePosition = currentReticlePosition;
			}

			float reticleDistance = 
				(reticle.transform.position - transform.position).magnitude;
            
			if (OnReticleSet != null) {
				OnReticleSet (reticle);
			}
		}

		protected override bool ProcessInteractionObject(Interactable interactable, ProcessState processState) {
			if (interactable.GetComponent<InteractableUi>() != null && uiLaser != null) {
				#if UNITY_EDITOR
				if (debugInteractor) {
					Debug.Log (name + ": Check selected UI operation: " + 0 + " for state: " + processState);
				}
				#endif

				InteractionEventData eventData = new InteractionEventData () {
					controller = this
				};

				bool operationStateChanged = false;
				if (DriverOperationStarted (0)) {
					eventData.operationId = 0;

					downedInteractable = interactable;

					uiLaser.PassButtonDown (interactable);

					operationStateChanged = true;
				}

				if (DriverOperationEnded (0)) {
					eventData.operationId = 0;

					downedInteractable = null;

					uiLaser.PassButtonUp (interactable, clickTolerance);

					interactable.RemovePressedState(this);

					SurfaceTopHoveredObject ();

					operationStateChanged = true;
				}

				if (!operationStateChanged) {
					// Held
					eventData.operationId = 0;
				}

			} else {
				return base.ProcessInteractionObject (interactable, processState);
			}
			return true;
		}

		public delegate void LaserPositionCallback(LaserBeamController controller, Vector3 forward, Vector3 up);
		public LaserPositionCallback OnLaserPositionUpdated;

		public void SetForwardPositionFromFlatScreen(Vector3 forward, Vector3 up) {
			transform.rotation = Quaternion.LookRotation (forward, up);
			if (OnLaserPositionUpdated != null) {
				OnLaserPositionUpdated (this, forward, up);
			}
//			Debug.Log ("Laser Rotation: " + transform.rotation.eulerAngles.ToString ("F3"));
		}
	}
}