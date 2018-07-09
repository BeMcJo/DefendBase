using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Keeps track of projectile details
public class ProjectileStats
{
    public string name,
                  description;
    //unlockCondition;
    public UnlockCondition unlockCondition;
    public int price = 0; // How much to initially buy from store
   
    public ProjectileStats(string nm, int price, UnlockCondition unlockCond, string description)
    {
        name = nm;
        this.price = price;
        unlockCondition = unlockCond;
        this.description = description;
    }
}

public class Projectile : MonoBehaviour {
    public static ProjectileStats[] projectileStats = new ProjectileStats[]
    {
        new ProjectileStats(
            "Arrow",
            0,
            UnlockCondition.Free,
            "Your standard arrow"
            ),
        new ProjectileStats(
            "Bomb Arrow",
            0,
            UnlockCondition.Purchase,
            "Explode your targets!"
            ),
        new ProjectileStats(
            "Piercing Arrow",
            0,
            UnlockCondition.Purchase,
            "Shoots through the enemies!"
            ),
        new ProjectileStats(
            "Bullet Arrow",
            0,
            UnlockCondition.Purchase,
            "Shoots straight to the target!"
            ),
        new ProjectileStats(
            "Ice Arrow",
            0,
            UnlockCondition.Purchase,
            "Creates a chilly explosion, freezing nearby enemies"
            ),

    };
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
    protected Transform attributesContainer; // Holds attribute objects
    protected bool deflected; // Did this not penetrate object? If so, bounce 
    public TrailRenderer tr; // Shows trajectory path tailing this object
    public string ownerType; // What type of object shot me?
    public int ownerID; // Who specifically shot me?
    public Material[] materials; // List of materials based on attribute
	// Use this for initialization
	protected virtual void Awake () {
        hitGround = false;
        deflected = false;
        tr = transform.GetComponent<TrailRenderer>();
        if (tr)
            tr.enabled = false;
        attributesContainer = transform.Find("Attributes");
        //SetAttribute(GameManager.gm.selectedArrowAttribute);
    }

    public virtual void Shoot(GameObject t, string ownerType, int ownerID, int dmg)
    {
        //print("shoot");
        isShot = true;
        this.dmg = dmg;
        this.ownerType = ownerType;
        this.ownerID = ownerID;
        id = ownerID;
        if (tr)
            tr.enabled = true;
        target = t;
        if(attributeID == 3)
        {
            GetComponent<Rigidbody>().AddForce(transform.forward * 5000);
        }
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
            else
            {
                // bullet type
                if (attributeID == 3 && !deflected)
                {
                    //Debug.DrawLine(transform.position, transform.);
                    //Debug.DrawRay(transform.position, transform.forward);
                    //print("shoot forward" + Time.time);
                    //GetComponent<Rigidbody>().useGravity = false;
                    RaycastHit hit;
                    /*
                    hits = Physics.RaycastAll(transform.position, transform.forward);
                    for (int i = 0; i < hits.Length; i++)
                    {
                        print(hits[i].collider.name + " " + hits[i].collider.tag);
                    }
                    */
                    if(Physics.Raycast(transform.position, transform.forward, out hit))
                    {
                        print("HIT " + hit.distance);
                        print(hit.point.ToString());
                        print(hit.collider.gameObject.name + " " + hit.collider.tag);
                        if (hit.distance <= 30)
                        {
                            GameObject mark = Instantiate(gameObject, hit.point, Quaternion.LookRotation(hit.normal)) as GameObject;
                            //mark.transform.Rotate(Vector)
                            mark.GetComponent<Projectile>().enabled = false;
                            OnHit(hit.collider.gameObject);
                        }
                    }
                }
            }

        }
        if(activeDuration <= 0)
        {
            Destroy(gameObject);
            return;
        }

	}

    // 0 = damage explosion, 1 = freezing wave
    public void CreateExplosion(ExplosionType explosionType)
    {
        GameObject explosion = Instantiate(GameManager.gm.attributePrefabs[attributeID]);
        explosion.transform.position = transform.position;
        explosion.transform.GetComponent<Explosion>().dmg = (explosionType == ExplosionType.damage)? 1:0;
        explosion.transform.GetComponent<Explosion>().explosionType = explosionType;
        explosion.transform.GetComponent<Explosion>().growthLimit = (explosionType == ExplosionType.damage) ? 14 : 20;
        explosion.transform.GetComponent<Explosion>().ownerID = ownerID;
        explosion.transform.GetComponent<Explosion>().ownerType = ownerType;
    }


    public virtual void SetAttribute(int aID)
    {
        attributeID = aID;
    }

    public void OnHit(GameObject collision)
    {
        if (deflected || !isShot)
            return;
        //print("HIT");

        // If projectile is an enemy's hitting me as the player?
        if (collision.tag == "Player" && ownerType == "Enemy")
        {
           // print("SPLAT");
            //GameManager.gm.Blackout();&& 
            //if(collision.GetComponent<PlayerController>().IsMyPlayer())
            collision.GetComponent<PlayerController>().AddBuff(1, 1);
            if (attributeID == 3)
                Destroy(gameObject, 3f);
            else
                Destroy(gameObject);
        }
        if (ownerType != "Player")
            return;

        // If not my projectile
        if (id != GameManager.gm.player.transform.GetComponent<PlayerController>().id)
        {

            if (collision.tag == "SweetSpot")
            {
                //print("SWEET");
                //collision.GetComponent<SweetSpot>().TakeDamage(gameObject);

                if (attributeID == 1)
                {
                    CreateExplosion(ExplosionType.damage);
                }
                //e.OnHit();
                // If piercing attribute, don't stop arrow
                if (attributeID != 2)
                {
                    deflected = true;
                    transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    transform.GetComponent<Rigidbody>().useGravity = false;
                    tr.enabled = false;
                    transform.SetParent(collision.transform);
                }
            }

            // If non-enemy projectile hits enemy
            else if (collision.transform.tag == "Enemy" && ownerType != "Enemy")
            {
                //print("PLAYER OTHERhit");
                // Indicate you hit enemy (sound + visual)
                //GameManager.gm.OnHitEnemy();
                // If can damage enemy and this is shot by my player
                if (!hitGround && !deflected)
                {
                    if (attributeID == 1)
                    {
                        CreateExplosion(ExplosionType.damage);
                    }
                    // ice arrow
                    else if (attributeID == 4)
                    {
                        print(UnlockCondition.Free);
                    }
                    // If piercing attribute, don't stop arrow
                    else if (attributeID != 2)
                    {
                        deflected = true;
                        transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        transform.GetComponent<Rigidbody>().useGravity = false;
                        tr.enabled = false;
                        transform.SetParent(collision.transform);
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
                    CreateExplosion(ExplosionType.damage);
                }
                // ice arrow
                else if (attributeID == 4)
                {
                    CreateExplosion(ExplosionType.freeze);
                    //e.ApplyEffect(Effect.freeze);
                }
                hitGround = true;
                deflected = true;
                if (attributeID == 3)
                {
                    Destroy(gameObject, 3f);
                }
                else
                    Destroy(gameObject);
            }
            return;
        }

        //print("HIT SOMTRHING:" + collision.tag + " " + collision.name);

        if (collision.tag == "Reward")
        {
            print("HIT REWARD");
            collision.GetComponent<Floater>().OnHit();
        }

        else if (collision.tag == "SweetSpot")
        {
            print("SWEET");
            collision.GetComponent<SweetSpot>().TakeDamage(gameObject);

            if (attributeID == 1)
            {
                CreateExplosion(ExplosionType.damage);
            }
            //e.OnHit();
            // If piercing attribute, don't stop arrow
            if (attributeID != 2)
            {
                deflected = true;
                transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
                transform.GetComponent<Rigidbody>().useGravity = false;
                tr.enabled = false;
                transform.SetParent(collision.transform);
            }
        }

        // If non-enemy projectile hits enemy
        else if (collision.transform.tag == "Enemy" && ownerType != "Enemy")
        {
            print("hit");
            // Indicate you hit enemy (sound + visual)
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

                // If piercing attribute, don't stop arrow
                if (attributeID != 2)
                {
                    deflected = true;
                    transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    transform.GetComponent<Rigidbody>().useGravity = false;
                    tr.enabled = false;
                    transform.SetParent(collision.transform);
                }
                if (attributeID == 1)
                {
                    CreateExplosion(ExplosionType.damage);
                }
                // ice arrow
                else if (attributeID == 4)
                {
                    CreateExplosion(ExplosionType.freeze);
                    //e.ApplyEffect(Effect.freeze);
                    return;
                }

                e.OnHit();
                //e.transform.GetComponent<Rigidbody>().velocity = Vector3.zero; // Disable physics force applied when colliding
                // If using online feature, let Network Manager handle this
                if (NetworkManager.nm.isStarted)
                {
                    print("hit4");
                    NetworkManager.nm.NotifyObjectDamagedBy(e.gameObject, gameObject);
                    return;
                }
                // If not using online feature, inflict damage to enemy
                e.TakeDamageFrom(dmg, ownerType, ownerID);
                //if (e.TakeDamage(dmg))
                //{
                // If enemy is still alive, leave arrow stuck in enemy
                //if (e.health > 0 )
                //{

                //}
                //}
            }
        }
        else if (collision.transform.tag == "Ground" || collision.transform.tag == "Impenetrable" || collision.transform.tag == "Path")
        {
            //print(collision.transform.name);
            if (!isShot)
                return;

            if (attributeID == 1)
            {
                CreateExplosion(ExplosionType.damage);
            }
            // ice arrow
            else if (attributeID == 4)
            {
                CreateExplosion(ExplosionType.freeze);
                //e.ApplyEffect(Effect.freeze);
            }
            hitGround = true;
            deflected = true;
            if (attributeID == 3)
            {
                deflected = true;
                Destroy(gameObject, 3f);
            }
            else
                Destroy(gameObject);
        }
        else if (collision.tag == "Trap")
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

    protected virtual void OnTriggerEnter(Collider collision)
    {
        //print("ATTRIBUTE::>>>>>>>>>>>>>>" + attributeID);
        //print(collision.gameObject.name + " " + collision.gameObject.tag + " " + ownerType);
        //print((id != GameManager.gm.player.transform.GetComponent<PlayerController>().id));
        //print(deflected);
        //print(!isShot);
        OnHit(collision.gameObject);
    }
    //unused
    /*
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
    */
}
