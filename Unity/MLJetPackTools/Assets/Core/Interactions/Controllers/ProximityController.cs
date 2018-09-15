using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MLJetPack.Interactions {
    public class ProximityController : ControllerDriver
    {
        public class HoverData
        {
            public float distance;
            public Interactable interactable;
        }

        public enum InteractionType
        {
            Interact,
            Grab,
            Release,
            Max
        }

        // Set this based on if the menu is visible on startup.
        // May need some other delegate method to answer to this later instead
        //
        public bool menuIsVisible = true;

        public bool interactionIsEnabled = true;

        [SerializeField]
        protected Interactable topHoveredItem = null;

        public virtual bool InteractionEnabled()
        {
            return interactionIsEnabled;
        }

        public virtual void SetInteractionEnabled(bool shouldBeEnabled)
        {
            interactionIsEnabled = shouldBeEnabled;

            SurfaceTopHoveredObject();
        }

        [SerializeField]
        public override LayerMask layerMask
        {
            get
            {
                if (controllerOverride != null && controllerOverride.enabled)
                {
                    return controllerOverride.layerMask;
                }
                return base.layerMask;
            }
        }

        void Awake()
        {
            if (mappedController == null)
            {
                mappedController = GetComponent<MappedController>();
                if (mappedController == null)
                {
                    Debug.LogError("No mapped controller.");
                }
            }
        }

        protected override void Start()
        {
            base.Start();
        }

        void OnDisable()
        {
            if (downedInteractable != null)
            {
                // See if there's anything to do
                if (downedInteractable.hoverState == HoverState.Attached)
                {
                    // Detach it
                    downedInteractable.OnDetachedController(this);
                }
                downedInteractable = null;
            }

            List<Interactable> interactables = new List<Interactable>();
            foreach (HoverData hoverData in sortedHoveredObjects)
            {
                interactables.Add(hoverData.interactable);
            }
            foreach (Interactable interactable in interactables)
            {
                interactable.RemoveHover(this);
            }
            sortedHoveredObjects.Clear();
        }

#if UNITY_EDITOR
        public bool debugInteractor = false;
#endif

        protected List<HoverData> sortedHoveredObjects = new List<HoverData>();
#if UNITY_EDITOR
        public List<Interactable> rawHoveredObjects = new List<Interactable>();
#endif

        public bool SortedHoveredObjectsContains(Interactable interactable)
        {
            foreach (HoverData hoverData in sortedHoveredObjects)
            {
                if (hoverData.interactable == interactable)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool ObjectCanBeHovered(Interactable interactable, ProximityController interactor)
        {
            if (interactable == null || !interactable.isActiveAndEnabled)
            {
                return false;
            }

            return interactable.CanBeHovered(interactor);
        }

        public virtual bool AddHoveredItem(Interactable item)
        {
            if (SortedHoveredObjectsContains(item))
            {
                return false;
            }

#if UNITY_EDITOR
            if (debugInteractor)
            {
                Debug.Log(name + ": Adding hovered item: " + item.name);
            }
#endif
            AddAndSortHoveredItem(item);
            return true;
        }

        public virtual int RemoveHoveredItem(Interactable interactable)
        {
            foreach (var hoverData in sortedHoveredObjects)
            {
                if (hoverData.interactable == interactable)
                {
                    sortedHoveredObjects.Remove(hoverData);
#if UNITY_EDITOR
                    rawHoveredObjects.Remove(interactable);
#endif
                    //					GetTopHoveredItem();
                    //		            Debug.Log("REMOVE Hovered Item: " + item.name);
                    break;
                }
            }
            return sortedHoveredObjects.Count;
        }

        public int GetHoverCount()
        {
            return sortedHoveredObjects.Count;
        }

        public Interactable GetTopHoveredItem()
        {
            if (sortedHoveredObjects.Count > 0)
            {
                foreach (HoverData hoverData in sortedHoveredObjects)
                {
                    if (ObjectCanBeHovered(hoverData.interactable, this))
                    {
                        topHoveredItem = hoverData.interactable;
#if UNITY_EDITOR
                        if (debugInteractor)
                        {
                            Debug.Log(name + ": Found top hovered item: " + topHoveredItem.name);
                        }
#endif
                        return hoverData.interactable;
                    }
                    else
                    {
#if UNITY_EDITOR
                        if (debugInteractor)
                        {
                            Debug.Log(name + ": Cannot hover over: " + hoverData.interactable.name);
                        }
#endif
                    }
                }
            }
            else
            {
                //Debug.Log("Hovered Stack: 0");
            }

            topHoveredItem = null;
#if UNITY_EDITOR
            if (debugInteractor)
            {
                Debug.Log(name + ": Found no top hovered item");
            }
#endif
            return null;
        }

        public override bool CanHover(GameObject item)
        {
            return downedInteractable == null && InteractionEnabled() &&
                (controllerOverride == null || !controllerOverride.enabled || !controllerOverride.CanHover(item));
        }

        public ControllerDriver controllerOverride = null;
        public void SetOverride(ControllerDriver overrideDriver)
        {
            if (overrideDriver != null)
            {
                overrideDriver.mappedController = mappedController;
            }
            controllerOverride = overrideDriver;
        }

        protected virtual void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyUp(KeyCode.L))
            {
                if (controllerOverride != null)
                {
                    controllerOverride.enabled = !controllerOverride.enabled;
                }
            }
#endif

            if (controllerOverride != null && controllerOverride.enabled)
            {
                controllerOverride.ProcessInteractions();

            }
            else
            {
                ProcessInteractions();
            }

            if (!InteractionEnabled())
            {
                // Do nothing
                return;
            }

            CheckHoveredObjectProximities();
        }

        public Interactable downedInteractable;
        public float downedDistance;

        public GameObject interactionPoint;
        public virtual Transform InteractionSourceTransform()
        {
            return interactionPoint != null ? interactionPoint.transform : this.transform;
        }

        protected virtual bool ProcessInteractionObject(Interactable interactable, ProcessState processState)
        {
            if (interactable != null)
            {
                // Store downed interactable here
                InteractionEventData eventData = new InteractionEventData()
                {
                    controller = this,
                    hoveredInteractable = interactable,
                    downedInteractable = downedInteractable
                };

                for (int i = 0; i < (int)InteractionType.Max; i++)
                {
#if UNITY_EDITOR
                    if (debugInteractor)
                    {
                        Debug.Log(Time.frameCount + ": " + name + ": Check operation: " + i + " for state: " + processState);
                    }
#endif

                    bool operationStateChanged = false;
                    if (DriverOperationStarted(i))
                    {
                        eventData.operationId = i;

                        downedInteractable = interactable;
                        downedDistance = (interactable.transform.position - transform.position).magnitude;

                        if (interactable.OnInteractionDown != null)
                        {
                            interactable.OnInteractionDown(eventData);
                        }

                        interactable.CheckHoverState(new List<ProximityController>() { this });

                        operationStateChanged = true;
                    }

                    if (DriverOperationEnded(i))
                    {
                        eventData.operationId = i;

                        downedInteractable = null;

                        if (DriverOperationClicked(i))
                        {
                            if (interactable.OnInteractionClick != null)
                            {
                                interactable.OnInteractionClick(eventData);
                            }
                        }

                        if (interactable.OnInteractionUp != null)
                        {
                            interactable.OnInteractionUp(eventData);
                        }

                        interactable.CheckHoverState(new List<ProximityController>() { this });

                        operationStateChanged = true;
                    }

                    if (!operationStateChanged)
                    {
                        eventData.operationId = i;

                        if (interactable.OnInteractionContinue != null)
                        {
                            interactable.OnInteractionContinue(eventData);
                        }
                    }
                }
            }
            return true;
        }

        public enum ProcessState
        {
            Hovered,
            Selected
        };

        public override void ProcessInteractions()
        {
            // If we have something selected, process it
            if (downedInteractable != null)
            {
                // Return true to consume the selected item, false to continue processing hovered item
                if (ProcessInteractionObject(downedInteractable, ProcessState.Selected))
                {
                    return;
                }
            }

            // Otherwise, process top hovered item
            if (topHoveredItem != null)
            {
                ProcessInteractionObject(topHoveredItem, ProcessState.Hovered);
            }
        }
    
		void CheckHoveredObjectProximities() {
			if (downedInteractable != null) {
				return;
			}

			if (sortedHoveredObjects.Count > 0) {
				SurfaceTopHoveredObject ();
			} else {
				topHoveredItem = null;
			}
		}

        // DSL TODO: Override this in laserbeamcontroller and remove if this is too close
		public virtual void SurfaceTopHoveredObject()
		{
			Interactable currentTop = topHoveredItem;
			#if UNITY_EDITOR
			if (debugInteractor) {
				Debug.Log(name + ": Surfacing top hovered object: " + currentTop);
			}
			#endif
			if (currentTop != null ||
				(InteractionEnabled () && sortedHoveredObjects.Count > 0)) {
				#if UNITY_EDITOR
				if (debugInteractor) {
					Debug.Log ("Surfaced Top Hovered Item START");
				}
				#endif

				List<HoverData> oldList = sortedHoveredObjects;
				sortedHoveredObjects = new List<HoverData> ();

				//				#if UNITY_EDITOR
				//				rawHoveredObjects.Clear ();
				//				#endif

				foreach (HoverData hdata in oldList) {
					AddHoveredItem (hdata.interactable);
				}

				#if UNITY_EDITOR
				if (debugInteractor) {
					Debug.Log ("Search for top hovered amongst total of " + oldList.Count);
				}
				#endif

				Interactable newTopHovered = GetTopHoveredItem ();
				if (currentTop != newTopHovered) {
					if (currentTop != null) {
						CheckObjectHoverState (currentTop);
					}
				}

				if (newTopHovered != null) {
					CheckObjectHoverState (newTopHovered);
				}

			} else if (sortedHoveredObjects.Count == 1 && topHoveredItem == null) {
				topHoveredItem = sortedHoveredObjects [0].interactable;
				CheckObjectHoverState (topHoveredItem);
			}
		}

		protected virtual void CheckObjectHoverState(Interactable interactable) {
			interactable.CheckHoverState (new List<ProximityController>() {this});
		}

		// This implementation assumes a single hover is active at a time.
		public virtual void AddAndSortHoveredItem(Interactable interactable)
		{
            //if (interactable.GetComponent<InteractableUi>() != null) {
            //    Debug.Log("Add and sort: " + interactable.name);
            //}
			#if UNITY_EDITOR
			if (debugInteractor) {
				Debug.Log("Add and sort hovered item: " + interactable.name);
			}
			#endif

			// First calculate the distance from the interaction source position
			// (such as the center of the controller tip)
			//
			float distance = interactable.DistanceFromController (this);

			// Store the object and distance into a HoverData structure
			var newHoverData = new HoverData();
			newHoverData.interactable = interactable;
			newHoverData.distance = distance;

			#if UNITY_EDITOR
			if (debugInteractor) {
				Debug.Log("Distance for " + interactable.name + " = " + distance);
			}
			#endif

			// This ensures that the closest item is at the head of the stack
			var insertionIndex = 0;
			foreach (var storedHoverData in sortedHoveredObjects) {
				if (storedHoverData.distance > newHoverData.distance)
				{
					break;
				}
				else
				{
					insertionIndex++;
				}
			}

			// If the index has exceeded the stack, add object at the end
			if (insertionIndex > sortedHoveredObjects.Count)
			{
				#if UNITY_EDITOR
				if (debugInteractor) {
					Debug.Log(name + ": Adding sorted hovered item: " + interactable.name);
				}
				#endif
				sortedHoveredObjects.Add(newHoverData);
			}
			else
			{
				#if UNITY_EDITOR
				if (debugInteractor) {
					Debug.Log(name + ": Inserting sorted hovered item: " + interactable.name);
				}
				#endif
				// Otherwise insert it at the index
				sortedHoveredObjects.Insert(insertionIndex, newHoverData);
			}

			//			GetTopHoveredItem ();

			#if UNITY_EDITOR
			// Add the interactable to the hovered list
			if (!rawHoveredObjects.Contains(interactable)) {
				rawHoveredObjects.Add(interactable);
			}
			#endif
		}
			        
    }
}