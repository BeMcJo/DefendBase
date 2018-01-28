using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : Projectile {
    protected bool deflected;
	// Use this for initialization
	void Start () {
        deflected = false;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (transform.GetComponent<Rigidbody>().velocity != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(transform.GetComponent<Rigidbody>().velocity);
	}
    /*
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Enemy")
        {
            if (!hitGround)
            {
                Enemy e = other.transform.GetComponent<Enemy>();
                e.TakeDamage(dmg);
                Debug.Log("trigger");
                //Destroy(gameObject);
            }
        }
        else if (other.transform.tag == "Ground")
        {
            hitGround = true;
            Destroy(gameObject);
        }
    }*/

    protected void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Ar");
        if (collision.transform.tag == "Enemy")
        {
            if (!hitGround && !deflected)
            {
                //Debug.Log("colide");
                Enemy e = collision.transform.GetComponent<Enemy>();
                e.TakeDamage(dmg);
                e.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
                if(e.health > 0)
                {
                    deflected = true;
                    transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
                }
                //Destroy(gameObject);
            }
        }
        else if (collision.transform.tag == "Ground")
        {
            hitGround = true;
            Destroy(gameObject);
        }
    }
}
