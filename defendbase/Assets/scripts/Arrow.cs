﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : Projectile {
	// Use this for initialization
	protected override void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	protected virtual void FixedUpdate () {
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

    protected override void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Ar");
        if (collision.transform.tag == "Enemy")
        {
            if (!hitGround && id == GameManager.gm.player.transform.GetComponent<PlayerController>().id)
            {
                //Debug.Log("colide");
                Enemy e = collision.transform.GetComponent<Enemy>();
                
                e.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
                if (NetworkManager.nm.isStarted)
                {
                    NetworkManager.nm.NotifyObjectDamagedBy(e.gameObject, gameObject);
                    return;
                }
                if (e.TakeDamage(dmg))
                {
                    if (e.health > 0)
                    {
                        deflected = true;
                        transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    }
                    Debug.Log(gameObject.tag);
                    
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
