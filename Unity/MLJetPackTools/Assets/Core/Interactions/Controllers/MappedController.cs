using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLJetPack.Interactions {
	public enum TouchPadPhase {
		Idle,
		Active,

		TouchBegan,
		TouchHeld,
		TouchEnded,

		ClickDown,
		ClickUp,
		ClickHeld
	};

	public interface ControllerImplementation {
		bool ActionButtonActive (int button);
		bool ActionButtonIdle (int button);
		bool ActionButtonUp (int button);
		bool ActionButtonDown (int button);
		bool ActionButtonHeld (int button);

		void PulseHaptic (float vibeStrength);

		bool InteractionFunctionsAreEnabled();
	};

	public class MappedController : MonoBehaviour {
		public Component controllerImplementation;

        [SerializeField]
        private ControllerImplementation _impl;
        public ControllerImplementation impl {
            get {
                if (_impl == null && controllerImplementation != null) {
                    _impl = (ControllerImplementation)controllerImplementation;
                }
                return _impl;
            }
        }

		public Dictionary<int, List<int>> operationIdToButton = new Dictionary<int, List<int>> ();

		public bool InteractionFunctionsEnabled() {
			if (impl != null) {
				return impl.InteractionFunctionsAreEnabled();
            } else {
                Debug.Log(name + ": no implementation");
            }
			return false;
		}

		protected virtual void Awake() {
			if (impl == null) {
				Debug.LogError ("We need a controller implementation!");
			}

			operationIdToButton [0] = new List<int> () { 0 };
		}

		public bool OperationIsActive(int operationId) {
			if (operationIdToButton.ContainsKey(operationId)) {
				foreach (int button in operationIdToButton[operationId]) {
					if (impl.ActionButtonActive (button)) {
						return true;
					}
				}
			}
			return false;
		}

		public bool OperationIsHeld(int operationId) {
			if (operationIdToButton.ContainsKey(operationId)) {
				foreach (int button in operationIdToButton[operationId]) {
					if (impl.ActionButtonHeld (button)) {
						return true;
					}
				}
			}
			return false;
		}

		public bool OperationIsIdle(int operationId) {
			if (operationIdToButton.ContainsKey(operationId)) {
				foreach (int button in operationIdToButton[operationId]) {
					if (impl.ActionButtonIdle (button)) {
						return true;
					}
				}
			}
			return false;
		}

		public bool OperationDidStart(int operationId) {
			if (operationIdToButton.ContainsKey(operationId)) {
                //Debug.Log(operationId + ": Has ids");
				foreach (int button in operationIdToButton[operationId]) {
                    //Debug.Log(operationId + ": Check button " + button);
					if (impl.ActionButtonDown (button)) {
                        //Debug.Log("Operation started: " + operationId);
						return true;
					}
				}
			}
			return false;
		}
			
		public bool OperationDidEnd(int operationId) {
			if (operationIdToButton.ContainsKey (operationId)) {
				foreach (int button in operationIdToButton[operationId]) {
                    if (impl.ActionButtonUp (button)) {
                        //Debug.Log("Operation ended: " + operationId);
						return true;
					}
				}
			}
			return false;
		}

		protected void AddOperationIdButton(int operationId, int buttonId, bool clearOthers = false) {
			if (!operationIdToButton.ContainsKey (operationId)) {
				operationIdToButton [operationId] = new List<int> ();

			} else if (clearOthers) {
				// This is for straight remapping instead of appending
				operationIdToButton [operationId].Clear ();
			}

			if (!operationIdToButton [operationId].Contains (buttonId)) {
				operationIdToButton [operationId].Add (buttonId);
			}
		}
	}
}
