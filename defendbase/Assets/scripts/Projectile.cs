using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour {
    public int id;
    public float activeDuration = 10f;
    public int dmg = 1;
    public bool hitGround, isShot;
	// Use this for initialization
	protected virtual void Start () {
        hitGround = false;
        isShot = false;
	}
	
	// Update is called once per frame
	protected virtual void Update () {
        if(isShot)
            activeDuration -= Time.deltaTime;
        if(activeDuration <= 0)
        {
            Destroy(gameObject);
            return;
        }

	}

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Enemy")
        {
            if (!hitGround && id == GameManager.gm.player.transform.GetComponent<PlayerController>().id)
            {
                Debug.Log("colide");
                Enemy e = collision.transform.GetComponent<Enemy>();
                e.TakeDamage(dmg);
                e.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
                //Destroy(gameObject);
            }
        }else if(collision.transform.tag == "Ground")
        {
            hitGround = true;
            Destroy(gameObject);
        }
    }
}
