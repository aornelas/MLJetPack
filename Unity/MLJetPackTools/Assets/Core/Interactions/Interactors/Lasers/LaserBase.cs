using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLJetPack.Interactions {
	public class LaserBase : MonoBehaviour {
		public virtual GameObject HoveredObject() {
			return null;
		}

		public virtual float ObjectDistance() {
			return float.MaxValue;
		}

		public virtual bool IsConnectedBeam() {
			return false;
		}

		[SerializeField]
		private GameObject _hitObject;
		public GameObject hitObject {
			get {
				return _hitObject;
			}
			set {
//				if (this.GetType() == GetComponent<UILaser>().GetType()) {
//					Debug.Log (Time.frameCount + ": Setting hitobject to " + (value != null ? value.name : "null"));
//				}
				_hitObject = value;
			}
		}

		public float hitObjectDistance;
		public Vector3 hitObjectNormal;

		public virtual Vector3 HitObjectNormal() {
			return hitObjectNormal;
		}

		public virtual bool ScanForObjects (float maxDistance = -1) {
			return hitObject != null;
		}

		public virtual void PassButtonDown(Interactable pointerOverInteractable) {
		}

		public virtual void PassButtonUp(Interactable pointerOverInteractable, float clickDownTime = float.MaxValue) {
		}
	}
}