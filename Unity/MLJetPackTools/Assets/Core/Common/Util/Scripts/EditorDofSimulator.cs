using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLJetPack.Util {
	public class EditorDofSimulator : MonoBehaviour {
		public float moveSpeedMultiplier = 1.0f;

		[SerializeField]
		Transform dofTransform;

		[SerializeField]
		DofShiftType dofShiftType = DofShiftType.Both;

		public enum DofShiftType {
			Both,
			Left,
			Right
		}

		void Awake() {
			#if !UNITY_EDITOR
			Destroy(this);
			return;
			#endif
			if (dofTransform == null) {
				dofTransform = transform;
			}
		}

		void Update () {
			bool doTheScript = false;

			switch (dofShiftType) {
			case DofShiftType.Left:
				doTheScript = Input.GetKey (KeyCode.LeftShift);
				break;

			case DofShiftType.Right:
				doTheScript = Input.GetKey (KeyCode.RightShift);
				break;

			case DofShiftType.Both:
				doTheScript = 
					(Input.GetKey (KeyCode.LeftShift) || 
						Input.GetKey (KeyCode.RightShift));
				break;
			}
			if (!doTheScript) {
				return;
			}

			Vector3 localPosition = dofTransform.localPosition;
			Vector3 direction = Vector3.zero;
			if (Input.GetKey (KeyCode.A)) {
				direction = -dofTransform.right;
			}
			if (Input.GetKey (KeyCode.D)) {
				direction = dofTransform.right;
			}
			if (Input.GetKey (KeyCode.W)) {
				direction = dofTransform.forward;
			}
			if (Input.GetKey (KeyCode.S)) {
				direction = -dofTransform.forward;
			}
			direction.y = 0;
			if (Input.GetKey (KeyCode.Q)) {
				direction = Vector3.up;
			}
			if (Input.GetKey (KeyCode.Z)) {
				direction = -Vector3.up;
			}
			direction = direction.normalized;

			localPosition += direction * .01f * moveSpeedMultiplier;
			dofTransform.localPosition = localPosition;


			Vector3 localAngles = dofTransform.localEulerAngles;
			if (Input.GetKey(KeyCode.LeftArrow)) {
				localAngles.y -= 1;
			}
			if (Input.GetKey(KeyCode.RightArrow)) {
				localAngles.y += 1;
			}
			if (Input.GetKey(KeyCode.UpArrow)) {
				localAngles.x -= 1;
			}
			if (Input.GetKey(KeyCode.DownArrow)) {
				localAngles.x += 1;
			}

			if (localAngles.x > 90 && localAngles.x < 269) {
				localAngles.x = 90;
			} else if (localAngles.x < -90) {
				localAngles.x = -90;
			}
			dofTransform.localEulerAngles = localAngles;
		}
	}
}
