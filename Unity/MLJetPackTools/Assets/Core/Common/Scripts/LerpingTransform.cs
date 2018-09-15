using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpingTransform : MonoBehaviour {
	public Vector3 		correctTargetPos;
	public Quaternion 	correctTargetRot;
	public Vector3 		correctTargetScale = Vector3.one;
	public Vector3 		lerpMask = Vector3.one;

	public delegate void TransformCallback(LerpingTransform lerpingTransform);

	public TransformCallback OnTransformComplete;

	public float startTime = -1;

	public float rotationDuration = 0.5f;
	public float positionDuration = 0.001f;

	public bool streaming = false;

	public Transform lerpingPositionTarget;
	public Transform lerpingTarget;
	// Update is called once per frame
	void Update () {
		bool shouldStream = streaming 
   //         && 
			//((Input.touchCount > 0 || Input.GetMouseButton(0))
			//#if UNITY_EDITOR
			//	|| true
			//#endif
				//)
            ;

		if (lerpingTarget != null && shouldStream) {
			if (lerpMask.x != 0) {
				transform.position = Vector3.Lerp (transform.position, lerpingPositionTarget.transform.position, Time.deltaTime / positionDuration);
				//				Debug.Log ("Transform pos: " + transform.position.ToString("F3") + " / elapsed time " + timeElapsed);
			}

			if (lerpMask.y != 0) {
				transform.rotation = Quaternion.Lerp (transform.rotation, lerpingTarget.transform.rotation, Time.deltaTime/rotationDuration);
			}

			if (lerpMask.z != 0) {
				transform.localScale = Vector3.Lerp (transform.localScale, lerpingTarget.transform.localScale, Time.deltaTime/rotationDuration);
			}
			startTime = -1;
			return;
		}

		if (lerpingTarget != null) {
			if (startTime == -1 &&
				((correctTargetPos - lerpingPositionTarget.position).magnitude > 0.1f ||
					(transform.position - correctTargetPos).magnitude > 0.1f)) {
				startTime = Time.time;
				if (lerpMask.x != 0) {
					correctTargetPos = lerpingTarget.position;
				}
				if (lerpMask.y != 0) {
					correctTargetRot = lerpingTarget.rotation;
				}
				if (lerpMask.z != 0) {
					correctTargetScale = lerpingTarget.localScale;
				}
				return;
			}
		}

		if (startTime != -1) {
			float timeElapsed = (Time.time - startTime);
			if (lerpMask.x != 0) {
				transform.position = Vector3.Lerp (transform.position, this.correctTargetPos, timeElapsed / positionDuration);
//				Debug.Log ("Transform pos: " + transform.position.ToString("F3") + " / elapsed time " + timeElapsed);
			}

			if (lerpMask.y != 0) {
				transform.rotation = Quaternion.Lerp (transform.rotation, this.correctTargetRot, timeElapsed/rotationDuration);
			}

			if (lerpMask.z != 0) {
				transform.localScale = Vector3.Lerp (transform.localScale, this.correctTargetScale, timeElapsed/rotationDuration);
			}

			if (timeElapsed >= rotationDuration) {
				startTime = -1;
				if (lerpMask.x != 0) {
					transform.position = this.correctTargetPos;
				}
				if (lerpMask.y != 0) {
					transform.rotation = this.correctTargetRot;
				}
				if (lerpMask.z != 0) {
					transform.localScale = this.correctTargetScale;
				}
				if (OnTransformComplete != null) {
//					Debug.Log ("TC Callback");
					this.OnTransformComplete (this);
				} else {
//					Debug.Log ("TC NO Callback");
				}
			}
		}
	}

	public void SetPosition(Vector3 position, bool immediate = true) {
		correctTargetPos = position;
		if (immediate) {
			transform.position = correctTargetPos;
		} else {
			startTime = Time.time;
		}
	}

    public void SetRotation(Quaternion rotation, bool immediate = true)
    {
        correctTargetRot = rotation;
        if (immediate)
        {
            transform.rotation = correctTargetRot;
        }
        else
        {
            startTime = Time.time;
        }
    }

	public void SetScale(float newScale) {
		correctTargetScale = new Vector3 (newScale, newScale, newScale);
		startTime = Time.time;
	}

	public void Warp() {
//		Debug.Log ("WARP");
		if (lerpingPositionTarget != null) {
			if (lerpMask.x != 0) {
				transform.position = correctTargetPos = lerpingPositionTarget.position;
			}
		}
		if (lerpingTarget != null) {
			if (lerpMask.y != 0) {
				lerpingTarget.rotation = Quaternion.LookRotation (GlobalAppMonitor.mainCamera.transform.forward, GlobalAppMonitor.mainCamera.transform.up);
				transform.rotation = correctTargetRot = lerpingTarget.rotation;
			}
			if (lerpMask.z != 0) {
				transform.localScale = correctTargetScale = lerpingTarget.localScale;
			}
		}		
	}

    private void Awake()
    {
        if (lerpingTarget != null && lerpingPositionTarget == null) {
            lerpingPositionTarget = lerpingTarget;
        }
    }
}
