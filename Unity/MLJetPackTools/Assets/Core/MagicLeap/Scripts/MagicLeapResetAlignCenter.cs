using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MLJetPack.AlignedXR;

public class MagicLeapResetAlignCenter : MonoBehaviour {
    public bool resetOnEnable = false;

    // Use this for initialization
    public float FORWARD_DISTANCE = 1.7f;
	void Start () {
        Vector3 forward = GlobalAppMonitor.mainCamera.transform.forward;
        forward.y = 0;
        transform.position = GlobalAppMonitor.mainCamera.transform.position + forward * FORWARD_DISTANCE;
        transform.forward = forward;
	}

    private void OnEnable()
    {
        if (resetOnEnable) {
            Vector3 forward = GlobalAppMonitor.mainCamera.transform.forward;
            forward.y = 0;
            transform.position = GlobalAppMonitor.mainCamera.transform.position + forward * FORWARD_DISTANCE;
            transform.forward = forward;
        }
    }
}
