using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MLJetPack.Interactions {
	public class RaycastLaser : LaserBase {
		public GameObject reticle;

		const float maxDistance = 10;

		public LayerMask defaultLayerMask = -1;
		public LayerMask layerMask {
			get {
				if (GetComponentInParent<LaserBeamController> () != null) {
//					Debug.Log ("Layer mask: " + GetComponentInParent<LaserBeamController> ().layerMask.value);
					return GetComponentInParent<LaserBeamController> ().layerMask;
				}
				return defaultLayerMask;
			}
		}
			
		public bool debugLaser = false;

		void Awake() {
			reticle.layer = LaserBeamController.LAYER_IGNORERAYCAST;
		}

		public override bool ScanForObjects(float maxDistance = -1) {
			Ray ray = new Ray(transform.position, transform.forward);
            
			RaycastHit hitInfo;
			bool bHit = Physics.Raycast(ray, out hitInfo, maxDistance, layerMask);
			if (bHit) {
				hitObject = hitInfo.collider.gameObject;
				hitObjectDistance = hitInfo.distance;
				hitObjectNormal = hitInfo.normal;

			} else {
				hitObject = null;
				hitObjectDistance = float.MaxValue;
			}

			return bHit;
		}
	}
}