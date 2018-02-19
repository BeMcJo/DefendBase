using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformPath : MonoBehaviour {
    public static int PlatformCount = 0;
    public int id;
    public List<GameObject> srcTargets = new List<GameObject>(),
                            destTargets = new List<GameObject>();
	// Use this for initialization
	void Start () {
        id = PlatformCount;
        PlatformCount++;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
