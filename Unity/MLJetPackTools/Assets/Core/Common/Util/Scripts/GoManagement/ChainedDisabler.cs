using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainedDisabler : MonoBehaviour {
	public List<GameObject> chains = new List<GameObject>();

	void OnEnable () {
		foreach (GameObject go in chains) {
			if (go == null) {
				Debug.LogError (name + " Chained enabler has object null");
			} else {
                go.SetActive (false);
			}
		}
	}
	
	void OnDisable () {
		foreach (GameObject go in chains) {
			if (go == null) {
				Debug.LogError (name + " Chained enabler has object null");
			} else {
                go.SetActive (true);
			}
		}
	}
}
