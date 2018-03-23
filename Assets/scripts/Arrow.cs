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

    public override void SetAttribute(int aID)
    {
        base.SetAttribute(aID);
        if (attributeID == 1)
        {
            Color c = transform.Find("Body").GetComponent<Renderer>().material.color;
            c = Color.red;
            transform.Find("Body").GetComponent<Renderer>().material.color = c;
        }
        if (attributeID == 0)
        {
            Color c = transform.Find("Body").GetComponent<Renderer>().material.color;
            c = Color.white;
            transform.Find("Body").GetComponent<Renderer>().material.color = c;
        }
    }

    public override void Shoot(GameObject t, string ownerType, int ownerID)
    {
        base.Shoot(t, ownerType, ownerID);
        transform.Find("Tail").GetComponent<BoxCollider>().enabled = false;
    }

    // Update is called once per frame
    protected virtual void FixedUpdate () {
        // Orient Arrow in the direction of its trajectory
        if (transform.GetComponent<Rigidbody>().velocity != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(transform.GetComponent<Rigidbody>().velocity);
	}
}
