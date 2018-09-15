using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveToggler : MonoBehaviour {	
	// Update is called once per frame
	public void ActionToggleActive () {
		gameObject.SetActive (!gameObject.activeSelf);
	}
}
