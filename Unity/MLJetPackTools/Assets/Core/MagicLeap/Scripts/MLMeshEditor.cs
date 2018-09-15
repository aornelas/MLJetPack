using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if !XR_NON_MAGIC_LEAP
using UnityEngine.XR.MagicLeap;
#endif

public class MLMeshEditor : MonoBehaviour {
    private void Awake()
    {
#if !UNITY_EDITOR
        Destroy(this);
#endif
    }

    // Use this for initialization
    void Start () {
        GameObject meshPrefab = GameObject.Instantiate(GetComponent<MLSpatialMapper>().meshPrefab);
        meshPrefab.transform.parent = GetComponent<MLSpatialMapper>().meshParent.transform;
        meshPrefab.transform.position = Vector3.zero;
	}

}
