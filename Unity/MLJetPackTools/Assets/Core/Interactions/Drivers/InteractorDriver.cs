using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLJetPack.Interactions {
	public class InteractorDriver : MonoBehaviour {
		public Dictionary<int, float> startTimes = new Dictionary<int, float> ();

		public float clickTolerance = 1.0f;

		public bool DriverOperationActive (int operationId) {
			return OperationIsActive (operationId);
		}

		public bool DriverOperationIdle (int operationId) {
			return OperationIsIdle (operationId) || OperationDidEnd(operationId);
		}

		public bool DriverOperationStarted (int operationId) {
			if (OperationDidStart (operationId)) {
				startTimes [operationId] = Time.time;
				return true;
			}
			return false;
		}

//		public bool DriverOperationContinues (int operationId) {
//			if (OperationIsContinuing (operationId)) {
//				return true;
//			}
//			return startTimes.ContainsKey(operationId);
//		}

		public bool DriverOperationHeld (int operationId) {
			if (OperationIsHeld (operationId)) {
				return true;
			}
			return false;
		}

		public bool DriverOperationEnded (int operationId) {
			if (OperationDidEnd (operationId)) {
				return true;
			}
			return false;
		}

		public bool DriverOperationClicked (int operationId) {
			if (OperationDidEnd (operationId)) {
				if (startTimes.ContainsKey (operationId) &&
					Time.time <= startTimes [operationId] + clickTolerance) {
					return true;
				}
			}
			return false;
		}

		protected virtual bool OperationIsActive(int operationId) {
			return false;
		}

		protected virtual bool OperationIsHeld(int operationId) {
			return false;
		}

		protected virtual bool OperationIsIdle(int operationId) {
			return false;
		}

		protected virtual bool OperationDidStart(int operationId) {
			return false;
		}

		protected virtual bool OperationDidEnd(int operationId) {
			return false;
		}
	}
}