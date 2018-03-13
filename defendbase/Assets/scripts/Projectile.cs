using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour {
    public int id; // ID based on which player shot this object
    public float activeDuration = 10f; // How long before destroying object
    public int dmg = 1; // How much damage this can inflict
    public bool hitGround, // Did this get dull from hitting ground/wall? Can't damage enemies if so 
                isShot; // Did this get launched?
    protected bool deflected; // Did this not penetrate object? If so, bounce 
    public TrailRenderer tr; // Shows trajectory path tailing this object
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
        //print(collision.gameObject.name + " " + collision.gameObject.tag);
        if (id != GameManager.gm.player.transform.GetComponent<PlayerController>().id || deflected)
            return;
        if (collision.transform.tag == "Enemy")
        {
            GameManager.gm.OnHitEnemy();
            // If can damage enemy and this is shot by my player
            if (!hitGround && !deflected)
            {
                Transform t = collision.transform;
                Enemy e = collision.transform.parent.GetComponent<Enemy>();
                while (e == null)
                {
                    t = t.parent;
                    e = t.parent.GetComponent<Enemy>();
                }
                e.OnHit();
                //e.transform.GetComponent<Rigidbody>().velocity = Vector3.zero; // Disable physics force applied when colliding
                // If using online feature, let Network Manager handle this
                if (NetworkManager.nm.isStarted)
                {
                    NetworkManager.nm.NotifyObjectDamagedBy(e.gameObject, gameObject);
                    return;
                }
                // If not using online feature, inflict damage to enemy
                if (e.TakeDamage(dmg))
                {
                    // If enemy is still alive, leave arrow stuck in enemy
                    if (e.health > 0)
                    {
                        deflected = true;
                        transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        transform.GetComponent<Rigidbody>().useGravity = false;
                        tr.enabled = false;
                        transform.SetParent(collision.transform);
                    }
                }
            }
        }
        else if (collision.transform.tag == "Ground" || collision.transform.tag == "Impenetrable")
        {
            print(collision.transform.name);
            if (!isShot)
                return;
            hitGround = true;
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
        if (collision.transform.tag == "Enemy")
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
            hitGround = true;
            Destroy(gameObject);
        }
    }
}
