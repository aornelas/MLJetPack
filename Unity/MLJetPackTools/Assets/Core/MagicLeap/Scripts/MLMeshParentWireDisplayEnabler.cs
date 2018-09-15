using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLMeshParentWireDisplayEnabler : MonoBehaviour {
	// Use this for initialization
	void OnEnable () {
        if (MLMeshParent.instance != null) {
            MLMeshParent.instance.SetWiresEnabled(true);
        }
	}
	
	// Update is called once per frame
	void OnDisable () {
        if (MLMeshParent.instance != null)
        {
            MLMeshParent.instance.SetWiresEnabled(false);
        }
    }
}
