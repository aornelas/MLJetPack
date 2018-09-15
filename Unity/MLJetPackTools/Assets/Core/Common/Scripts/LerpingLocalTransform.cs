using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpingLocalTransform : MonoBehaviour {
	public Vector3 correctPlayerLocalPos;
	public Quaternion correctPlayerLocalRotation;
	public Vector3 correctPlayerLocalScale;

	public Vector3 lerpMask = new Vector3(1,0,0);

	// Use this for initialization
	void Start () {
		correctPlayerLocalPos = transform.localPosition;
		correctPlayerLocalRotation = transform.localRotation;
		correctPlayerLocalScale = transform.localScale;
	}

	public float startTime = -1;
	public float duration = 1f;

	public Transform lerpingTarget;
	// Update is called once per framef
	void Update () {
//		#if PHOTON && !IRON_MAN
//		if (GetComponentInParent<PhotonView>() != null &&
//			!GetComponentInParent<PhotonView>().isMine) {
//			// Control is someone else's
//			return;
//		}
//		#endif

		if (lerpingTarget != null) {
			if (startTime == -1 &&
				((correctPlayerLocalPos - lerpingTarget.localPosition).magnitude > 0.1f ||
					(transform.localPosition - correctPlayerLocalPos).magnitude > 0.1f)) {
				startTime = Time.time;
				correctPlayerLocalPos = lerpingTarget.localPosition;
				correctPlayerLocalRotation = lerpingTarget.localRotation;
				correctPlayerLocalScale = lerpingTarget.localScale;
				return;
			}
		}

		if (startTime != -1) {
			float timeElapsed = (Time.time - startTime);
			if (lerpMask.x != 0) {
				transform.localPosition = Vector3.Lerp (transform.localPosition, this.correctPlayerLocalPos, timeElapsed / duration);
//				Debug.Log ("Transform pos: " + transform.position.ToString("F3") + " / elapsed time " + timeElapsed);
			}

			if (lerpMask.y != 0) {
				transform.localRotation = Quaternion.Lerp (transform.localRotation, this.correctPlayerLocalRotation, timeElapsed / duration);
			}

			if (lerpMask.z != 0) {
				transform.localScale = Vector3.Lerp (transform.localScale, this.correctPlayerLocalScale, timeElapsed / duration);
			}

			if (timeElapsed > duration) {
				startTime = -1;
				if (lerpMask.x != 0) {
					transform.localPosition = this.correctPlayerLocalPos;
				}

				if (lerpMask.y != 0) {
					transform.localRotation = this.correctPlayerLocalRotation;
				}

				if (lerpMask.z != 0) {
					transform.localScale = this.correctPlayerLocalScale;
				}
			}
		}
	}

	public void SetPosition(Vector3 localPosition) {
		correctPlayerLocalPos = localPosition;
		startTime = Time.time;
	}

	public void SetRotation(Quaternion localRotation) {
		correctPlayerLocalRotation = localRotation;
		startTime = Time.time;
	}

	public void SetScale(float localScale) {
		correctPlayerLocalScale = localScale * Vector3.one;
		startTime = Time.time;
	}

	public void SetLerpingTransform(Transform target) {
		correctPlayerLocalPos = target.localPosition;
		correctPlayerLocalRotation = target.localRotation;
		correctPlayerLocalScale = target.localScale;
		startTime = Time.time;
	}
}
