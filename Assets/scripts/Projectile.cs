﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    public static string[] names = new string[] {
        "Projectile",
        "Arrow"
    };
    public int id; // ID based on which player shot this object
    public float activeDuration = 10f; // How long before destroying object
    public float moveSpd = 6f;
    public int dmg = 1, // How much damage this can inflict
               attributeID; // Unique attribute attached to projectile
    public bool hitGround, // Did this get dull from hitting ground/wall? Can't damage enemies if so 
                isHoming = false, // Does this object lock on and chase target?
                isShot; // Did this get launched?
    public GameObject target; // Is there a target locked on to chase?
    protected bool deflected; // Did this not penetrate object? If so, bounce 
    public TrailRenderer tr; // Shows trajectory path tailing this object
    public string ownerType; // What type of object shot me?
    public int ownerID; // Who specifically shot me?
	// Use this for initialization
	protected virtual void Start () {
        hitGround = false;
        deflected = false;
        tr = transform.GetComponent<TrailRenderer>();
        if (tr)
            tr.enabled = false;

        SetAttribute(GameManager.gm.selectedAttribute);
    }

    public virtual void Shoot(GameObject t, string ownerType, int ownerID)
    {
        //print("shoot");
        isShot = true;
        this.ownerType = ownerType;
        this.ownerID = ownerID;
        id = ownerID;
        if (tr)
            tr.enabled = true;
        target = t;
    }
	
	// Update is called once per frame
	protected virtual void Update () {
        if (isShot)
        {
            activeDuration -= Time.deltaTime;
            if (target)
            {
                transform.LookAt(target.transform);
                transform.position += transform.forward * Time.deltaTime * moveSpd;
            }
        }
        if(activeDuration <= 0)
        {
            Destroy(gameObject);
            return;
        }

	}

    public void CreateExplosion()
    {
        GameObject explosion = Instantiate(GameManager.gm.attributePrefabs[attributeID]);
        explosion.transform.position = transform.position;
        explosion.transform.GetComponent<Explosion>().dmg = 5;
        explosion.transform.GetComponent<Explosion>().growthLimit = 14;
        explosion.transform.GetComponent<Explosion>().id = id;
    }


    public virtual void SetAttribute(int aID)
    {
        attributeID = aID;
    }

    protected virtual void OnTriggerEnter(Collider collision)
    {
        //print("ATTRIBUTE::>>>>>>>>>>>>>>" + attributeID);
        //print(collision.gameObject.name + " " + collision.gameObject.tag + " " + ownerType);
        //print((id != GameManager.gm.player.transform.GetComponent<PlayerController>().id));
        //print(deflected);
        //print(!isShot);
        if (deflected || !isShot)
            return;
        if(collision.tag == "Player" && ownerType == "Enemy" && collision.GetComponent<PlayerController>().IsMyPlayer())
        {
            print("SPLAT");
            GameManager.gm.Blackout();
            Destroy(gameObject);
        }
        if (ownerType != "Player")
            return;
        if (id != GameManager.gm.player.transform.GetComponent<PlayerController>().id)
            return;
        if (collision.tag == "Reward")
        {
            print("HIT REWARD");
            collision.GetComponent<Floater>().OnHit();
        }
        if (collision.transform.tag == "Enemy" && ownerType != "Enemy")
        {
            print("hit");
            GameManager.gm.OnHitEnemy();
            // If can damage enemy and this is shot by my player
            if (!hitGround && !deflected)
            {
                print("hit2");
                Transform t = collision.transform;
                Enemy e = collision.transform.parent.GetComponent<Enemy>();
                while (e == null)
                {
                    t = t.parent;
                    e = t.parent.GetComponent<Enemy>();
                }

                if (attributeID == 1)
                {
                    CreateExplosion();
                }
                e.OnHit();
                // If piercing attribute, don't stop arrow
                if (attributeID != 2)
                {
                    deflected = true;
                    transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    transform.GetComponent<Rigidbody>().useGravity = false;
                    tr.enabled = false;
                    transform.SetParent(collision.transform);
                }
                //e.transform.GetComponent<Rigidbody>().velocity = Vector3.zero; // Disable physics force applied when colliding
                // If using online feature, let Network Manager handle this
                if (NetworkManager.nm.isStarted)
                {
                    print("hit4");
                    NetworkManager.nm.NotifyObjectDamagedBy(e.gameObject, gameObject);
                    return;
                }
                // If not using online feature, inflict damage to enemy
                if (e.TakeDamage(dmg))
                {
                    // If enemy is still alive, leave arrow stuck in enemy
                    //if (e.health > 0 )
                    //{
                        
                    //}
                }
            }
        }
        else if (collision.transform.tag == "Ground" || collision.transform.tag == "Impenetrable" || collision.transform.tag == "Path")
        {
            //print(collision.transform.name);
            if (!isShot)
                return;

            if (attributeID == 1)
            {
                CreateExplosion();
            }
            hitGround = true;
            deflected = true;
            Destroy(gameObject);
        }
        else if(collision.tag == "Trap")
        {
            Debug.Log("???");
            if (NetworkManager.nm.isStarted)
            {
                NetworkManager.nm.NotifyObjectDamagedBy(collision.gameObject, gameObject);
                return;
            }
            collision.GetComponent<Trap>().TakeDamage(dmg);
        }
    }
    //unused
    protected virtual void OnCollisionEnter(Collision collision)
    {
        print(1);
        if (collision.transform.tag == "Enemy" && ownerType != "Enemy")
        {
            if (!hitGround && id == GameManager.gm.player.transform.GetComponent<PlayerController>().id)
            {
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
            }
        }
        else if (collision.transform.tag == "Ground")
        {
            print("!?");
            hitGround = true;
            Destroy(gameObject);
        }
    }
}
