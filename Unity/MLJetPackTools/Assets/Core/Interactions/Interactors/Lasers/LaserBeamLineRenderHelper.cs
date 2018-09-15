using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLJetPack.Interactions {
	[RequireComponent(typeof(LineRenderer))]
	public class LaserBeamLineRenderHelper : MonoBehaviour {
		LineRenderer lineRenderer;
		// Use this for initialization
		void Awake () {
			if (lineRenderer == null) {
				lineRenderer = GetComponent<LineRenderer> ();
			}
			LaserBeamController interactor = GetComponentInParent<LaserBeamController> ();
			if (interactor != null) {
				interactor.OnReticleSet += AdjustLineToReticle;
			}
		}
  
		// Update is called once per frame
		void AdjustLineToReticle (GameObject reticle) {
			if (!this.enabled) {
				return;
			}
			if (reticle.activeInHierarchy) {
				lineRenderer.enabled = true;
				lineRenderer.SetPosition (1, transform.InverseTransformPoint(reticle.transform.position));
			} else {
				lineRenderer.enabled = false;
			}
		}
	}
}
