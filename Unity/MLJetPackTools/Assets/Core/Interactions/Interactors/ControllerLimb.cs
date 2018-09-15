using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLJetPack.Interactions {
	public class ControllerLimb : MonoBehaviour {
		public ProximityController controller;

		void Awake() {
			if (controller == null) {
				controller = GetComponentInParent<ProximityController> ();
				controller.interactionPoint = gameObject;
			}

			if (GetComponent<Rigidbody> () == null) {
				Rigidbody rb = gameObject.AddComponent<Rigidbody> ();
				rb.isKinematic = true;
				rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			}

			if (GetComponentInChildren<Collider> () != null) {
                GetComponentInChildren<Collider> ().isTrigger = true;
			} else {
				Debug.LogError ("No collider set on limb");
			}
		}
	};
}
