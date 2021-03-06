﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Keeps track of projectile details
public class ProjectileStats
{
    public string name,
                  description;
    //unlockCondition;
    public UnlockCondition unlockCondition;
    public int price = 0, // How much to initially buy from store
               purchaseQty; // How many you get when purchasing
    public ProjectileStats(string nm, int price, int purchaseQty, UnlockCondition unlockCond, string description)
    {
        name = nm;
        this.price = price;
        unlockCondition = unlockCond;
        this.description = description;
        this.purchaseQty = purchaseQty;

    }
}

public class Projectile : MonoBehaviour {
    public static ProjectileStats[] projectileStats = new ProjectileStats[]
    {
        new ProjectileStats(
            "Arrow",
            0,
            0,
            UnlockCondition.Free,
            "Your standard arrow"
            ),
        new ProjectileStats(
            "Bomb Arrow",
            50,
            15,
            UnlockCondition.QuestThenPurchase,
            "Explode the enemies!"
            ),
        new ProjectileStats(
            "Piercing Arrow",
            0,
            0,
            UnlockCondition.Purchase,
            "Shoots through the enemies!"
            ),
        new ProjectileStats(
            "Bullet Arrow",
            50,
            15,
            UnlockCondition.QuestThenPurchase,
            "Shoots straight to the target!"
            ),
        new ProjectileStats(
            "Ice Arrow",
            50,
            15,
            UnlockCondition.QuestThenPurchase,
            "Freeze the enemies!"
            ),
        new ProjectileStats(
            "Fire Arrow",
            50,
            7,
            UnlockCondition.QuestThenPurchase,
            "Burn the enemies!"
            ),

    };
    public static string[] names = new string[] {
        "Projectile",
        "Arrow"
    };
    public static bool[] enableArrows = { true, true, false, false, true, true, true };
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
            if (hitGround || deflected)
            {
                return;
            }
            //print("VALOCITY>>:" + GetComponent<Rigidbody>().velocity + "............" + GetComponent<Rigidbody>().velocity.magnitude);
            activeDuration -= Time.deltaTime;
            if (target)
            {
                transform.LookAt(target.transform);
                transform.position += transform.forward * Time.deltaTime * moveSpd * ((target.tag == "Objective")? 2:1);
            }
            else
            {
                // bullet type (currently just testing to allow raycast at all times)
                if (true || transform.GetComponent<Rigidbody>().velocity.magnitude >= 55 || (attributeID == 3 && !deflected))
                {
                    //print("raycasting");
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
                        //print("HIT " + hit.distance);
                        //print(hit.point.ToString());
                        //print(hit.collider.gameObject.name + " " + hit.collider.tag);
                        if (hit.distance <= transform.GetComponent<Rigidbody>().velocity.magnitude)//30)
                        {
                            //print("CLOSE");
                            //GameObject mark = Instantiate(gameObject, hit.point, Quaternion.LookRotation(hit.normal)) as GameObject;
                            //mark.transform.Rotate(Vector)
                            //mark.GetComponent<Projectile>().enabled = false;
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

    // 0 = fire aoe
    public void CreateAreaEffect(int effectID)
    {
        GameObject areaEffect = Instantiate(GameManager.gm.areaEffectPrefabs[effectID]);
        areaEffect.transform.position = transform.position;
        AreaEffect ae = areaEffect.GetComponent<AreaEffect>();
        ae.ownerID = ownerID;
        ae.ownerType = ownerType;

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

    public void HandleOtherPlayerProjectiles(GameObject collision)
    {

        if (collision.tag == "SweetSpot")
        {
            //print("SWEET");
            //collision.GetComponent<SweetSpot>().TakeDamage(gameObject);
            // bomb arrow
            if (attributeID == 1)
            {
                CreateExplosion(ExplosionType.damage);
            }
            // ice arrow
            else if (attributeID == 4)
            {
                CreateExplosion(ExplosionType.freeze);
                //e.ApplyEffect(Effect.freeze);
                //return;
            }
            // fire arrow
            else if (attributeID == 5)
            {
                CreateAreaEffect(0);
            }
            //e.OnHit();
            // If piercing attribute, don't stop arrow
            if (attributeID != 2)
            {
                deflected = true;
                Destroy(gameObject);
                /*
                transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
                transform.GetComponent<Rigidbody>().useGravity = false;
                tr.enabled = false;
                transform.SetParent(collision.transform);
                */
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
                // bomb arrow
                if (attributeID == 1)
                {
                    CreateExplosion(ExplosionType.damage);
                }
                // ice arrow
                else if (attributeID == 4)
                {
                    CreateExplosion(ExplosionType.freeze);
                    //e.ApplyEffect(Effect.freeze);
                    //return;
                }
                // fire arrow
                else if (attributeID == 5)
                {
                    CreateAreaEffect(0);
                }
                // If piercing attribute, don't stop arrow
                else if (attributeID != 2)
                {
                    /*
                    transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    transform.GetComponent<Rigidbody>().useGravity = false;
                    tr.enabled = false;
                    transform.SetParent(collision.transform);
                    */
                }
                deflected = true;

                Destroy(gameObject);

            }
        }
        else if (collision.transform.tag == "Ground" || collision.transform.tag == "Impenetrable" || collision.transform.tag == "Path" || collision.transform.tag == "Dummy")
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
            // fire arrow
            else if (attributeID == 5)
            {
                CreateAreaEffect(0);
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
    }

    public void OnHit(GameObject collision)
    {
        if (deflected || !isShot)
            return;
        //print("HIT");

        // If projectile is an enemy's hitting me as the player?
        if (ownerType == "Enemy")
        {
            if (collision.tag == "Player")
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
            else if(collision.tag == "Objective")
            {
                // If multiplayer, synchronize the attack to prevent duplicate attacks
                if (NetworkManager.nm.isStarted)
                {
                    NetworkManager.nm.QueueAttackOnObject(GameManager.gm.enemies[ownerID].gameObject, collision.gameObject);
                }
                // Not online, just attack target
                else
                {
                    Objective o = target.transform.GetComponent<Objective>();
                    o.TakeDamage(dmg);
                }
                Destroy(gameObject);
            }
        }
        if (ownerType != "Player")
            return;

        // If not my projectile
        if (id != GameManager.gm.player.transform.GetComponent<PlayerController>().id)
        {
            HandleOtherPlayerProjectiles(collision);
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
            SweetSpot ss = collision.GetComponent<SweetSpot>();//.TakeDamage(gameObject);
            ss.TakeDamage(gameObject);
            // bomb arrow
            if (attributeID == 1)
            {
                CreateExplosion(ExplosionType.damage);
            }
            // ice arrow
            else if (attributeID == 4)
            {
                CreateExplosion(ExplosionType.freeze);
                //e.ApplyEffect(Effect.freeze);
                //return;
            }
            // fire arrow
            else if (attributeID == 5)
            {
                CreateAreaEffect(0);
            }
            //e.OnHit();
            // If piercing attribute, don't stop arrow
            if (attributeID != 2)
            {
                deflected = true;

                if (attributeID == 3)
                {
                    deflected = true;
                    Destroy(gameObject, 1f);
                }
                else
                    Destroy(gameObject);
                /*
                transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
                transform.GetComponent<Rigidbody>().useGravity = false;
                tr.enabled = false;
                transform.SetParent(collision.transform);
                */
            }
            if (ss.owner.tag == "Dummy")
                return;
            Transform t = collision.transform;
            Enemy e = collision.transform.parent.GetComponent<Enemy>();
            while (e == null)
            {
                t = t.parent;
                e = t.parent.GetComponent<Enemy>();
            }
            // if enemy dies, update record statistics
            if (e.health <= 0)
            {
                // increment kill count for attribute
                GameManager.gm.data.killsByArrowAttribute[attributeID]++;
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
                    if (attributeID == 3)
                    {
                        deflected = true;
                        Destroy(gameObject, 1f);
                    }
                    else
                        Destroy(gameObject);
                    /*
                    transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    transform.GetComponent<Rigidbody>().useGravity = false;
                    tr.enabled = false;
                    transform.SetParent(collision.transform);
                    */
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
                    //return;
                }
                // fire arrow
                else if (attributeID == 5)
                {
                    CreateAreaEffect(0);
                }

                // if projectile hits enemy, icrement counter
                GameManager.gm.player.GetComponent<PlayerController>().shotsHit++;
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
                // if enemy dies, update record statistics
                if (e.health <= 0)
                {
                    // increment kill count for attribute
                    GameManager.gm.data.killsByArrowAttribute[attributeID]++;
                }
                //if (e.TakeDamage(dmg))
                //{
                // If enemy is still alive, leave arrow stuck in enemy
                //if (e.health > 0 )
                //{

                //}
                //}
            }
        }
        else if (collision.tag == "Dummy")
        {
            print("BAKA");
            GameManager.gm.OnHitEnemy();

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
            // fire arrow
            else if (attributeID == 5)
            {
                CreateAreaEffect(0);
            }
            hitGround = true;
            deflected = true;
            if (attributeID == 3)
            {
                deflected = true;
                Destroy(gameObject, 1f);
            }
            else
                Destroy(gameObject);
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
            // fire arrow
            else if (attributeID == 5)
            {
                CreateAreaEffect(0);
            }
            hitGround = true;
            deflected = true;
            if (attributeID == 3)
            {
                deflected = true;
                Destroy(gameObject, 1f);
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
