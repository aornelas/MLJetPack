using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLMeshGenerated : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (MLMeshParent.instance != null) {
            GetComponent<MeshRenderer>().materials = 
                MLMeshParent.instance.GetMaterials(MLMeshParent.instance.wiresEnabled);
            MeshCollider mc = GetComponent<MeshCollider>();
            if (mc != null) {
                mc.enabled = MLMeshParent.instance.defaultDisplayMode == MLMeshParent.DisplayMode.Occlusion;
            }
        }
	}	
}
