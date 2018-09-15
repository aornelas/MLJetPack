using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLMeshWireframe : MonoBehaviour {
    public MeshFilter meshContainer;

	// Use this for initialization
	void Start () {
        GetComponent<MeshFilter>().sharedMesh = meshContainer.mesh;
    }
}
