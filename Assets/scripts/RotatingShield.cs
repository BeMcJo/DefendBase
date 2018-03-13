using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingShield : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
        transform.localEulerAngles += new Vector3(0, 1, 0);
	}
}
