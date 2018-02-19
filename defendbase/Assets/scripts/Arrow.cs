using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 
 */
public class Arrow : Projectile {
	// Use this for initialization
	protected override void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	protected virtual void FixedUpdate () {
        // Orient Arrow in the direction of its trajectory
        if (transform.GetComponent<Rigidbody>().velocity != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(transform.GetComponent<Rigidbody>().velocity);
	}
}
