using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformPath : MonoBehaviour {
    public static int PlatformCount = 0; // Count number of platforms in game
    public int id; // Unique ID
    public List<GameObject> srcTargets = new List<GameObject>(), // List of targets that lead to this path
                            destTargets = new List<GameObject>(); // List of targets that this path leads to
	// Use this for initialization
	void Start () {
        id = PlatformCount;
        PlatformCount++;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
