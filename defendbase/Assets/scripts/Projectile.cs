using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour {
    public int id;
    public float activeDuration = 10f;
    public int dmg = 1;
    public bool hitGround, isShot;
    protected bool deflected;
    public TrailRenderer tr;
	// Use this for initialization
	protected virtual void Start () {
        hitGround = false;
        isShot = false;
        deflected = false;
        tr = transform.GetComponent<TrailRenderer>();
        if (tr)
            tr.enabled = false;
	}
	
	// Update is called once per frame
	protected virtual void Update () {
        if (isShot)
        {
            activeDuration -= Time.deltaTime;
            if (tr)
                tr.enabled = true;
        }
        if(activeDuration <= 0)
        {
            Destroy(gameObject);
            return;
        }

	}

    protected virtual void OnTriggerEnter(Collider collision)
    {
        //Debug.Log("Ar");
        if (collision.transform.tag == "Enemy")
        {
            if (!hitGround && !deflected && id == GameManager.gm.player.transform.GetComponent<PlayerController>().id)
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
                        transform.GetComponent<Rigidbody>().useGravity = false;
                        tr.enabled = false;
                        //this.enabled = false;
                        transform.SetParent(collision.transform);
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
